using System.Linq;
using System.Security.Claims;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Npgsql.Internal;

using MM_API.Database.Postgres;
using MM_API.Database.Postgres.DbSchema;

using MonoMonarchNetworkFramework.Game.Character;
using MonoMonarchNetworkFramework.Game.Kingdom.Map;
using MonoMonarchNetworkFramework.Game.Kingdom;
using MonoMonarchNetworkFramework.Game.Character.Sheet;
using MonoMonarchNetworkFramework.Game.Character.State;
using MonoMonarchNetworkFramework.Game.Character.Inventory;

using MonoMonarchGameFramework.Game;
using MonoMonarchGameFramework.Game.Kingdom.Map;
using MonoMonarchGameFramework.Game.Armoury.Equipment;
using MonoMonarchGameFramework.Game.Character;

using MonoMonarchGameFramework.Game.Armoury.Equipment.Weapon.Sword;
using MonoMonarchGameFramework.Game.Armoury;

using MonoMonarchGameFramework.Game.Soupkitchen;
using MonoMonarchGameFramework.Game.Armoury.Equipment.Jewellery.Ring;
using MonoMonarchGameFramework.Game.Armoury.Equipment.Armour.Hands;

using MonoMonarchGameFramework.Game.Armoury.Equipment.Armour.Arms;
using MonoMonarchGameFramework.Game.Armoury.Equipment.Armour.Feet;
using MonoMonarchGameFramework.Game.Armoury.Equipment.Armour.Head;
using MonoMonarchGameFramework.Game.Armoury.Equipment.Armour.Legs;
using MonoMonarchGameFramework.Game.Armoury.Equipment.Armour.Torso;
using MonoMonarchNetworkFramework;

namespace MM_API.Services
{
    public interface ICharacterService
    {
        public Task<ICharacterLoadResponse> LoadCharacterAsync();
        public Task<ISheetUpdateResponse> UpdateCharacterSheet(SheetUpdatePayload sheetUpdatePayload);
        public Task<IStateUpdateResponse> UpdateCharacterState(StateUpdatePayload stateUploadPayload);
        public Task<IInventoryUpdateResponse> UpdateCharacterInventory(InventoryUpdatePayload inventoryUpdatePayload);
    }

    #region Production
    public class CharacterService //: ICharacterService
    {
        private readonly MM_DbContext _dbContext;

        public CharacterService(MM_DbContext dbContext)
        {
            _dbContext = dbContext;
        }
    }
    #endregion
    #region Development
    public class TestCharacterService : ICharacterService
    {
        private readonly MM_DbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public TestCharacterService(MM_DbContext dbContext, UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<ICharacterLoadResponse> LoadCharacterAsync()
        {
            try
            {
                var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(u => u.Type == $"{ClaimTypes.NameIdentifier}").Value;
                var user = await _userManager.FindByIdAsync(userId);

                t_Character character = await _dbContext.t_character.FirstOrDefaultAsync(c => c.fk_user_id == user.CustomUserId);

                return new CharacterLoadResponse
                {

                    CharacterName = character.character_name,

                    CharacterWeapons = character.character_weapons,
                    CharacterArmour = character.character_armour,
                    CharacterJewellery = character.character_jewellery,

                    CharacterSheet = character.character_attributes,
                    CharacterState = character.character_state
                };
            }
            catch (Exception)
            {
                return new ErrorResponse("Load character failed");
            }
        }
        public async Task<IInventoryUpdateResponse> UpdateCharacterInventory(InventoryUpdatePayload inventoryUpdatePayload)
        {
            try
            {
                ///get method contexts
                ///
                var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.NameIdentifier)?.Value;
                var user = await _userManager.FindByIdAsync(userId);
                ///complete
                ///

                ///initial payload validity check
                ///
                //payload should never exceed the number of total character inventory slots:
                //one weapon
                //one of each BaseArmour type (6 types in total)
                //two rings (left and right hand)
                //one amulet
                if (inventoryUpdatePayload.EquipmentLocalIdNums.Count() > 10)
                    return new ErrorResponse("Malformed payload detected");
                ///complete
                ///

                ///initialise method tools
                ///
                //initialise inventory's
                EquipmentInventory armouryInventory = null;
                EquipmentInventory characterInventory = null;

                //initialise inventory property dictionaries where IEquipable LocalId is each item's index
                Dictionary<string, BaseWeapon> armouryWeaponDict = null;
                Dictionary<string, BaseArmour> armouryArmourDict = null;
                Dictionary<string, BaseJewellery> armouryJewelleryDict = null;

                Dictionary<string, BaseWeapon> characterWeaponDict = null;
                Dictionary<string, BaseArmour> characterArmourDict = null;
                Dictionary<string, BaseJewellery> characterJewelleryDict = null;

                //initialise user db objects
                t_Character character = null; //await _dbContext.t_character.FirstAsync(u => u.fk_user_id == user.CustomUserId);
                t_Armoury armoury = null; //await _dbContext.t_armoury.FirstAsync(u => u.fk_user_id == user.CustomUserId);
                ///complete
                ///

                ///determine equip or de-equip (add or remove)
                ///
                //get alive character
                character = await _dbContext.t_character
                    .Where(c => c.fk_user_id == user.CustomUserId && c.character_isalive == true)
                    .FirstOrDefaultAsync();

                //assign character inventory
                characterInventory = new EquipmentInventory();

                //setup conditional deserialisation
                Dictionary<string, bool> deserialisationCheckDict = new Dictionary<string, bool>()
                {
                    { "characterWeaponList",false }, { "characterArmourList",false }, { "characterJewelleryList",false },
                    { "armouryWeaponList",false }, { "armouryArmourList",false }, { "armouryJewelleryList",false }
                };
                JsonSerializer serialiser = new JsonSerializer();
                serialiser.Converters.Add(new DeserialisationSupport());

                //setup dictionary to track number of equipment types from payload
                Dictionary<string, int> payloadEquipmentTypes = new Dictionary<string, int>()
                    {
                        { "Weapon", 0 },
                        { "Arms", 0 }, { "Feet", 0 }, { "Hands", 0 }, { "Head", 0 }, { "Legs", 0 }, { "Torso", 0 },
                        { "Amulet", 0 }, { "Ring", 0 }
                    };

                Dictionary<string, IEquipable> swapEquipmentTypes = new Dictionary<string, IEquipable>()
                    {
                        { "Weapon", null },
                        { "Arms", null }, { "Feet", null }, { "Hands", null }, { "Head", null }, { "Legs", null }, { "Torso", null },
                        { "Amulet", null }, { "RingLeft", null }, { "RingRight", null }
                    };

                //search for payload items in character inventory
                string equipmentBaseType = string.Empty;
                Dictionary<string, bool> characterSearchResult = new Dictionary<string, bool>();
                foreach (string localId in inventoryUpdatePayload.EquipmentLocalIdNums)
                {
                    equipmentBaseType = localId.Substring(0, localId.IndexOf("_"));
                    characterSearchResult.Add(localId, false);

                    if (equipmentBaseType.Equals("weapon"))
                    {
                        {
                            using (StringReader sr = new StringReader(character.character_weapons))
                            {
                                using (JsonReader reader = new JsonTextReader(sr))
                                {
                                    characterInventory.WeaponList = serialiser.Deserialize<BaseWeapon[]>(reader).ToList();
                                }
                            }
                            deserialisationCheckDict["characterWeaponList"] = true;

                            characterWeaponDict = characterInventory.WeaponList.ToDictionary(item => item.LocalId);
                        }

                        foreach (BaseWeapon weapon in characterInventory.WeaponList)
                        {
                            if (weapon.LocalId == localId) //if weapon is to be removed/de-equipped
                            {
                                characterSearchResult[localId] = true;

                                payloadEquipmentTypes["Weapon"]++;
                                if (payloadEquipmentTypes["Weapon"] > 1)
                                    return new ErrorResponse("Malformed payload detected");
                                break;
                            }
                        }
                        if (characterSearchResult[localId] && inventoryUpdatePayload.EquipmentLocalIdNums[inventoryUpdatePayload.EquipmentLocalIdNums.Count() - 1] != localId) continue;
                        else if (characterSearchResult[localId] && inventoryUpdatePayload.EquipmentLocalIdNums[inventoryUpdatePayload.EquipmentLocalIdNums.Count() - 1] == localId) break;
                    }

                    else if (equipmentBaseType.Equals("armour"))
                    {
                        if (!deserialisationCheckDict["characterArmourList"])
                        {
                            using (StringReader sr = new StringReader(character.character_armour))
                            {
                                using (JsonReader reader = new JsonTextReader(sr))
                                {
                                    characterInventory.ArmourList = serialiser.Deserialize<BaseArmour[]>(reader).ToList();
                                }
                            }
                            deserialisationCheckDict["characterArmourList"] = true;

                            characterArmourDict = characterInventory.ArmourList.ToDictionary(item => item.LocalId);
                        }
                        foreach (BaseArmour armour in characterInventory.ArmourList)
                        {
                            if (armour.LocalId == localId)
                            {
                                characterSearchResult[localId] = true;

                                payloadEquipmentTypes[$"{armour.ArmourType}"]++;
                                if (payloadEquipmentTypes[$"{armour.ArmourType}"] > 1) 
                                    return new ErrorResponse("Malformed payload detected");
                                
                                break;
                            }
                        }
                        if (characterSearchResult[localId] && inventoryUpdatePayload.EquipmentLocalIdNums[inventoryUpdatePayload.EquipmentLocalIdNums.Count() - 1] != localId) continue;
                        else if (characterSearchResult[localId] && inventoryUpdatePayload.EquipmentLocalIdNums[inventoryUpdatePayload.EquipmentLocalIdNums.Count() - 1] == localId) break;
                    }

                    else if (equipmentBaseType.Equals("jewellery")) //add to this to support left-right hand de-equip, and check RingHand payload
                    {
                        if (!deserialisationCheckDict["characterJewelleryList"])
                        {
                            using (StringReader sr = new StringReader(character.character_jewellery))
                            {
                                using (JsonReader reader = new JsonTextReader(sr))
                                {
                                    characterInventory.JewelleryList = serialiser.Deserialize<BaseJewellery[]>(reader).ToList();
                                }
                            }
                            deserialisationCheckDict["characterJewelleryList"] = true;

                            characterJewelleryDict = characterInventory.JewelleryList.ToDictionary(item => item.LocalId);
                        }
                        foreach (BaseJewellery jewellery in characterInventory.JewelleryList)
                        {
                            if (jewellery.LocalId == localId)
                            {
                                characterSearchResult[localId] = true;

                                payloadEquipmentTypes[$"{jewellery.JewelleryType}"]++;
                                if (payloadEquipmentTypes["Ring"] > 0 && inventoryUpdatePayload.RingHand == null) 
                                    return new ErrorResponse("Malformed payload detected");
                                if (payloadEquipmentTypes["Amulet"] > 1 || payloadEquipmentTypes["Ring"] > 2) 
                                    return new ErrorResponse("Malformed payload detected");
                                break;
                            }
                        }
                    }

                    else
                    {
                        return new ErrorResponse("Malformed payload detected");
                    }
                }
                //check if search is consistant
                bool badPayloadDetected = characterSearchResult[inventoryUpdatePayload.EquipmentLocalIdNums[0]];
                foreach (bool value in characterSearchResult.Values)
                {
                    if (value != badPayloadDetected)
                        return new ErrorResponse("Malformed payload detected");
                }
                ///complete
                ///

                armoury = await _dbContext.t_armoury
                    .Where(c => c.fk_user_id == user.CustomUserId)
                    .FirstOrDefaultAsync();

                //assign armoury inventory
                armouryInventory = new EquipmentInventory();

                if (characterSearchResult[inventoryUpdatePayload.EquipmentLocalIdNums[0]])
                {//de-equip
                    ///action de-equip phase
                    ///
                    foreach (string localId in characterSearchResult.Keys)
                    {
                        equipmentBaseType = localId.Substring(0, localId.IndexOf("_"));
                        switch (equipmentBaseType)
                        {
                            case "weapon":
                                if (!deserialisationCheckDict["armouryWeaponList"])
                                {
                                    using (StringReader sr = new StringReader(armoury.armoury_weapons))
                                    {
                                        using (JsonReader reader = new JsonTextReader(sr))
                                        {
                                            armouryInventory.WeaponList = serialiser.Deserialize<BaseWeapon[]>(reader).ToList();
                                        }
                                    }
                                    deserialisationCheckDict["armouryWeaponList"] = true;

                                    armouryWeaponDict = armouryInventory.WeaponList.ToDictionary(item => item.LocalId);
                                }
                                BaseWeapon weapon = characterWeaponDict[localId];
                                armouryWeaponDict.Add(weapon.LocalId, weapon);
                                characterWeaponDict.Remove(weapon.LocalId);
                                break;
                            case "armour":
                                if (!deserialisationCheckDict["armouryArmourList"])
                                {
                                    using (StringReader sr = new StringReader(armoury.armoury_armour))
                                    {
                                        using (JsonReader reader = new JsonTextReader(sr))
                                        {
                                            armouryInventory.ArmourList = serialiser.Deserialize<BaseArmour[]>(reader).ToList();
                                        }
                                    }
                                    deserialisationCheckDict["armouryArmourList"] = true;

                                    armouryArmourDict = armouryInventory.ArmourList.ToDictionary(item => item.LocalId);
                                }
                                BaseArmour armour = characterArmourDict[localId];
                                armouryArmourDict.Add(armour.LocalId, armour);
                                characterArmourDict.Remove(armour.LocalId);
                                break;
                            case "jewellery":
                                if (!deserialisationCheckDict["armouryJewelleryList"])
                                {
                                    using (StringReader sr = new StringReader(armoury.armoury_jewellery))
                                    {
                                        using (JsonReader reader = new JsonTextReader(sr))
                                        {
                                            armouryInventory.JewelleryList = serialiser.Deserialize<BaseJewellery[]>(reader).ToList();
                                        }
                                    }
                                    deserialisationCheckDict["armouryJewelleryList"] = true;

                                    armouryJewelleryDict = armouryInventory.JewelleryList.ToDictionary(item => item.LocalId);
                                }
                                BaseJewellery jewellery = characterJewelleryDict[localId];
                                armouryJewelleryDict.Add(jewellery.LocalId, jewellery);
                                characterJewelleryDict.Remove(jewellery.LocalId);
                                break;
                        }
                    }
                    ///complete
                    ///

                }
                else
                {//equip
                    ///payload validity check against armouryInventory assets, and locate items that must be swapped if equipping an item of the same type that would exceed game rules
                    ///
                    //search for payload items in armoury inventory as part of payload validity check
                    bool isFound = false;
                    Dictionary<string, IEquipable> armourySearchResult = new Dictionary<string, IEquipable>();
                    foreach (string localId in inventoryUpdatePayload.EquipmentLocalIdNums)
                    {
                        isFound = false;

                        equipmentBaseType = localId.Substring(0, localId.IndexOf("_"));

                        if (equipmentBaseType.Equals("weapon"))
                        {
                            if (!deserialisationCheckDict["armouryWeaponList"])
                            {
                                using (StringReader sr = new StringReader(armoury.armoury_weapons))
                                {
                                    using (JsonReader reader = new JsonTextReader(sr))
                                    {
                                        armouryInventory.WeaponList = serialiser.Deserialize<BaseWeapon[]>(reader).ToList();
                                    }
                                }
                                deserialisationCheckDict["armouryWeaponList"] = true;

                                armouryWeaponDict = armouryInventory.WeaponList.ToDictionary(item => item.LocalId);
                            }
                            foreach (BaseWeapon weapon in armouryInventory.WeaponList)
                            {

                                if (weapon.LocalId == localId)
                                {
                                    //located weapon to equip
                                    armourySearchResult.Add(localId, weapon);
                                    isFound = true;

                                    payloadEquipmentTypes["Weapon"]++;
                                    if (payloadEquipmentTypes["Weapon"] > 1)
                                            return new ErrorResponse("Malformed payload detected");
                                    //locate weapon to swap if character has weapon equipped
                                    if (characterInventory.WeaponList.Count == 1) swapEquipmentTypes["Weapon"] = characterWeaponDict.ElementAt(0).Value;//characterWeaponDict != null ||characterWeaponDict.ElementAt(0).Value != null
                                    break;
                                }
                            }
                            if (isFound && inventoryUpdatePayload.EquipmentLocalIdNums[inventoryUpdatePayload.EquipmentLocalIdNums.Count() - 1] != localId) continue;
                            else if (isFound && inventoryUpdatePayload.EquipmentLocalIdNums[inventoryUpdatePayload.EquipmentLocalIdNums.Count() - 1] == localId) break;
                        }
                        else if (equipmentBaseType.Equals("armour"))
                        {
                            if (!deserialisationCheckDict["armouryArmourList"])
                            {
                                using (StringReader sr = new StringReader(armoury.armoury_armour))
                                {
                                    using (JsonReader reader = new JsonTextReader(sr))
                                    {
                                        armouryInventory.ArmourList = serialiser.Deserialize<BaseArmour[]>(reader).ToList();
                                    }
                                }
                                deserialisationCheckDict["armouryArmourList"] = true;

                                armouryArmourDict = armouryInventory.ArmourList.ToDictionary(item => item.LocalId);
                            }
                            foreach (BaseArmour armour in armouryInventory.ArmourList)
                            {
                                if (armour.LocalId == localId)
                                {
                                    armourySearchResult.Add(localId, armour);
                                    isFound = true;

                                    payloadEquipmentTypes[$"{armour.ArmourType}"]++;
                                    if (payloadEquipmentTypes[$"{armour.ArmourType}"] > 1)
                                        return new ErrorResponse("Malformed payload detected");

                                    if ((swapEquipmentTypes[$"{armour.ArmourType}"] = characterArmourDict.Values.FirstOrDefault(a => a.ArmourType == armour.ArmourType)) != null) { }
                                    break;
                                }
                            }
                            if (isFound && inventoryUpdatePayload.EquipmentLocalIdNums[inventoryUpdatePayload.EquipmentLocalIdNums.Count() - 1] != localId) continue;
                            else if (isFound && inventoryUpdatePayload.EquipmentLocalIdNums[inventoryUpdatePayload.EquipmentLocalIdNums.Count() - 1] == localId) break;
                        }
                        else if (equipmentBaseType.Equals("jewellery"))
                        {
                            if (!deserialisationCheckDict["armouryJewelleryList"])
                            {
                                using (StringReader sr = new StringReader(armoury.armoury_jewellery))
                                {
                                    using (JsonReader reader = new JsonTextReader(sr))
                                    {
                                        armouryInventory.JewelleryList = serialiser.Deserialize<BaseJewellery[]>(reader).ToList();
                                    }
                                }
                                deserialisationCheckDict["armouryJewelleryList"] = true;

                                armouryJewelleryDict = armouryInventory.JewelleryList.ToDictionary(item => item.LocalId);
                            }
                            foreach (BaseJewellery jewellery in armouryInventory.JewelleryList)
                            {
                                if (jewellery.LocalId == localId)
                                {


                                    payloadEquipmentTypes[$"{jewellery.JewelleryType}"]++;
                                    if (payloadEquipmentTypes["Ring"] > 0 && inventoryUpdatePayload.RingHand == null)
                                        return new ErrorResponse("Malformed payload detected");
                                    if (payloadEquipmentTypes["Amulet"] > 1 || payloadEquipmentTypes["Ring"] > 2)
                                        return new ErrorResponse("Malformed payload detected");

                                    //payload validation
                                    if (jewellery.JewelleryType == "Ring")
                                    {
                                        // Case when one ring is found
                                        if (payloadEquipmentTypes["Ring"] == 1)
                                        {
                                            if (inventoryUpdatePayload.RingHand.Count < 1 ||
                                                !inventoryUpdatePayload.RingHand.ContainsKey(localId))
                                                return new ErrorResponse("Malformed payload detected");

                                            //if (inventoryUpdatePayload.RingHand.Count < 1 || 
                                            //    !inventoryUpdatePayload.RingHand.TryGetValue(jewellery.LocalId, out bool handSide) ||
                                            //    (handSide != true && handSide != false))
                                            //    return new InventoryUpdateResponse { Success = false, ErrorMessage = "Ring hand information is missing or invalid." };

                                            //need to test if a exception or something is thrown/detected if a boolean neither true nor false (essentially null) is given as payload, with a valid localId for the key. if this isn't possible then the code i have is fine
                                        }
                                        else if (payloadEquipmentTypes["Ring"] == 2)
                                        {
                                            // Check if the LocalId exists in the RingHand map
                                            if (!inventoryUpdatePayload.RingHand.ContainsKey(jewellery.LocalId) ||
                                                inventoryUpdatePayload.RingHand.Count != 2 ||
                                                inventoryUpdatePayload.RingHand.Values.Distinct().Count() != 2)
                                            {
                                                return new ErrorResponse("Malformed payload detected");
                                            }
                                        }
                                        if (inventoryUpdatePayload.RingHand[jewellery.LocalId] == false)
                                        {//if payload specifies equip ring to left hand THEN check if characterInventory.JewelleryList contains a ring with RightHand == false property

                                            var matchingRing = characterJewelleryDict.Values
                                                .FirstOrDefault(a => a.JewelleryType == "Ring" && ((Ring)a).RightHand == false);
                                            if ((swapEquipmentTypes[$"RingLeft"] = matchingRing) != null) { }
                                            if (jewellery is Ring ring)
                                            {
                                                ring.RightHand = false;
                                            }
                                        }
                                        else
                                        {//if payload specifies equip ring to right hand 

                                            var matchingRing = characterJewelleryDict.Values
                                                .FirstOrDefault(a => a.JewelleryType == "Ring" && ((Ring)a).RightHand == true);
                                            if ((swapEquipmentTypes[$"RingRight"] = matchingRing) != null) { }
                                            if (jewellery is Ring ring)
                                            {
                                                ring.RightHand = true;
                                            }
                                        }

                                        armourySearchResult.Add(localId, jewellery);
                                        isFound = true;
                                    }
                                    else if (jewellery.JewelleryType == "Amulet")
                                    {
                                        // For Amulet or other jewellery types, just match by JewelleryType
                                        if ((swapEquipmentTypes[$"{jewellery.JewelleryType}"] = characterJewelleryDict.Values.FirstOrDefault(a => a.JewelleryType == jewellery.JewelleryType)) != null) { }
                                    }
                                    break;
                                }
                            }
                        }
                        else
                        {
                            return new ErrorResponse("Malformed payload detected");
                        }
                        if (!isFound)
                            return new ErrorResponse("Malformed payload detected");//required here as each payload item must exist in armouryInventory at this point
                    }
                    ///action equip phase
                    ///
                    //action swapping for items within swap dictionary (if any exist)
                    foreach (IEquipable item in swapEquipmentTypes.Values)               //for (int i = 0; i < swapEquipmentTypes.Count(); i++)
                    {
                        if (item != null)
                        {
                            switch (item)
                            {
                                case BaseWeapon weapon:
                                    armouryWeaponDict.Add(weapon.LocalId, weapon);
                                    characterWeaponDict.Remove(weapon.LocalId);
                                    break;
                                case BaseArmour armour:
                                    armouryArmourDict.Add(armour.LocalId, armour);
                                    characterArmourDict.Remove(armour.LocalId);
                                    break;
                                case BaseJewellery jewellery:
                                    armouryJewelleryDict.Add(jewellery.LocalId, jewellery);
                                    characterJewelleryDict.Remove(jewellery.LocalId);
                                    break;
                            }
                        }
                    }
                    //action equip
                    foreach (IEquipable item in armourySearchResult.Values)                // for (int i = 0; i < armourySearchResult.Count(); i++)
                    {
                        switch (item)
                        {
                            case BaseWeapon weapon:
                                characterWeaponDict.Add(weapon.LocalId, weapon);
                                armouryWeaponDict.Remove(weapon.LocalId);
                                break;
                            case BaseArmour armour:
                                characterArmourDict.Add(armour.LocalId, armour);
                                armouryArmourDict.Remove(armour.LocalId);
                                break;
                            case BaseJewellery jewellery:
                                characterJewelleryDict.Add(jewellery.LocalId, jewellery);
                                armouryJewelleryDict.Remove(jewellery.LocalId);
                                break;
                        }
                    }
                    ///complete
                }
                ///complete
                ///
                serialiser.Converters.Clear();

                //apply changes to armoury and character inventory's
                if (deserialisationCheckDict["characterWeaponList"]) characterInventory.WeaponList = characterWeaponDict.Values.ToList();
                if (deserialisationCheckDict["characterArmourList"]) characterInventory.ArmourList = characterArmourDict.Values.ToList();
                if (deserialisationCheckDict["characterJewelleryList"]) characterInventory.JewelleryList = characterJewelleryDict.Values.ToList();

                if (deserialisationCheckDict["armouryWeaponList"]) armouryInventory.WeaponList = armouryWeaponDict.Values.ToList();
                if (deserialisationCheckDict["armouryArmourList"]) armouryInventory.ArmourList = armouryArmourDict.Values.ToList();
                if (deserialisationCheckDict["armouryJewelleryList"]) armouryInventory.JewelleryList = armouryJewelleryDict.Values.ToList();

                // After the move operations, re-serialize the updated inventories back into strings
                string updatedCharacterWeapons = string.Empty;
                string updatedCharacterArmour = string.Empty;
                string updatedCharacterJewellery = string.Empty;

                string updatedArmouryWeapons = string.Empty;
                string updatedArmouryArmour = string.Empty;
                string updatedArmouryJewellery = string.Empty;
                using (StringWriter sw = new StringWriter())
                {
                    using (JsonWriter writer = new JsonTextWriter(sw))
                    {
                        if (deserialisationCheckDict["characterWeaponList"])
                        {
                            serialiser.Serialize(writer, characterInventory.WeaponList);
                            updatedCharacterWeapons = sw.ToString();
                            sw.GetStringBuilder().Clear();
                        }
                        if (deserialisationCheckDict["characterArmourList"])
                        {
                            serialiser.Serialize(writer, characterInventory.ArmourList);
                            updatedCharacterArmour = sw.ToString();
                            sw.GetStringBuilder().Clear();
                        }
                        if (deserialisationCheckDict["characterJewelleryList"])
                        {
                            serialiser.Serialize(writer, characterInventory.JewelleryList);
                            updatedCharacterJewellery = sw.ToString();
                            sw.GetStringBuilder().Clear();
                        }

                        if (deserialisationCheckDict["armouryWeaponList"])
                        {
                            serialiser.Serialize(writer, armouryInventory.WeaponList);
                            updatedArmouryWeapons = sw.ToString();

                            sw.GetStringBuilder().Clear();
                        }
                        if (deserialisationCheckDict["armouryArmourList"])
                        {
                            serialiser.Serialize(writer, armouryInventory.ArmourList);
                            updatedArmouryArmour = sw.ToString();
                            sw.GetStringBuilder().Clear();
                        }
                        if (deserialisationCheckDict["armouryJewelleryList"])
                        {
                            serialiser.Serialize(writer, armouryInventory.JewelleryList);
                            updatedArmouryJewellery = sw.ToString();
                            sw.GetStringBuilder().Clear();
                        }


                    }
                }

                using (var transaction = await _dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // Store the updated inventories back to the database
                        if (deserialisationCheckDict["characterWeaponList"]) character.character_weapons = updatedCharacterWeapons;
                        if (deserialisationCheckDict["characterArmourList"]) character.character_armour = updatedCharacterArmour;
                        if (deserialisationCheckDict["characterJewelleryList"]) character.character_jewellery = updatedCharacterJewellery;

                        if (deserialisationCheckDict["armouryWeaponList"]) armoury.armoury_weapons = updatedArmouryWeapons;
                        if (deserialisationCheckDict["armouryArmourList"]) armoury.armoury_armour = updatedArmouryArmour;
                        if (deserialisationCheckDict["armouryJewelleryList"]) armoury.armoury_jewellery = updatedArmouryJewellery;

                        await _dbContext.SaveChangesAsync();
                        transaction.Commit();

                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        return new ErrorResponse("Transaction failed, rolling back");
                    }

                    return new InventoryUpdateResponse { };
                }
            }

            catch (Exception)
            {
                return new ErrorResponse("Update character inventory failed");
            }
        }
        public async Task<ISheetUpdateResponse> UpdateCharacterSheet(SheetUpdatePayload sheetUpdatePayload)
        {
            try
            {

                return new SheetUpdateResponse
                {

                };
            }
            catch (Exception)
            {
                return new ErrorResponse("Update character sheet failed");
            }
        }
        public async Task<IStateUpdateResponse> UpdateCharacterState(StateUpdatePayload stateUpdatePayload)
        {
            try
            {

                return new StateUpdateResponse
                {

                };
            }
            catch (Exception)
            {
                return new ErrorResponse("Update character state failed");
            }
        }
    }
}
#endregion




#region Legacy


//public async Task<IInventoryUpdateResponse> UpdateCharacterInventory(InventoryUpdatePayload inventoryUpdatePayload)
//        {
//            try
//            {
//                var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.NameIdentifier)?.Value;
//                var user = await _userManager.FindByIdAsync(userId);

//                string[] equipmentIdNums = inventoryUpdatePayload.EquipmentLocalIdNums;

//                ///check payload validity
//                ///
//                //payload should never exceed the number of total character inventory slots:
//                //one weapon
//                //one of each BaseArmour type (6 types in total)
//                //two rings (left and right hand)
//                //one amulet
//                if (inventoryUpdatePayload.EquipmentLocalIdNums.Count() > 10)
//                    return new InventoryUpdateResponse { Success = false, ErrorMessage = $"I see you." };




//                ///complete
//                ///


//                ///determine equip or de-equip (add or remove)
//                ///

//                ///complete
//                ///


//                ///if equip - compile list of items required to be swapped
//                ///

//                ///complete
//                ///


//                ///


//                List<IDeserialisable> equipmentPiece = new List<IDeserialisable>();

//                EquipmentInventory characterInventory = new EquipmentInventory();
//                EquipmentInventory armouryInventory = new EquipmentInventory();

//                Dictionary<string, BaseWeapon> armouryWeaponDict = new Dictionary<string, BaseWeapon>();
//                Dictionary<string, BaseArmour> armouryArmourDict = new Dictionary<string, BaseArmour>();
//                Dictionary<string, BaseJewellery> armouryJewelleryDict = new Dictionary<string, BaseJewellery>();

//                Dictionary<string, BaseWeapon> characterWeaponDict = new Dictionary<string, BaseWeapon>();
//                Dictionary<string, BaseArmour> characterArmourDict = new Dictionary<string, BaseArmour>();
//                Dictionary<string, BaseJewellery> characterJewelleryDict = new Dictionary<string, BaseJewellery>();

//                t_Character character = await _dbContext.t_character.FirstAsync(u => u.fk_user_id == user.CustomUserId);
//                t_Armoury armoury = await _dbContext.t_armoury.FirstAsync(u => u.fk_user_id == user.CustomUserId);

//                //deserialise user inventories
//                JsonSerializer serialiser = new JsonSerializer();
//                serialiser.Converters.Add(new DeserialisationSupport());
//                ///character inventory
//                using (StringReader sr = new StringReader(character.character_weapons))
//                {
//                    using (JsonReader reader = new JsonTextReader(sr))
//                    {
//                        characterInventory.WeaponList = serialiser.Deserialize<BaseWeapon[]>(reader).ToList();
//                    }
//                }
//                using (StringReader sr = new StringReader(character.character_armour))
//                {
//                    using (JsonReader reader = new JsonTextReader(sr))
//                    {
//                        characterInventory.ArmourList = serialiser.Deserialize<BaseArmour[]>(reader).ToList();
//                    }
//                }
//                using (StringReader sr = new StringReader(character.character_jewellery))
//                {
//                    using (JsonReader reader = new JsonTextReader(sr))
//                    {
//                        characterInventory.JewelleryList = serialiser.Deserialize<BaseJewellery[]>(reader).ToList();
//                    }
//                }
//                ///armoury inventory
//                using (StringReader sr = new StringReader(armoury.armoury_weapons))
//                {
//                    using (JsonReader reader = new JsonTextReader(sr))
//                    {
//                        armouryInventory.WeaponList = serialiser.Deserialize<BaseWeapon[]>(reader).ToList();
//                    }
//                }
//                using (StringReader sr = new StringReader(armoury.armoury_armour))
//                {
//                    using (JsonReader reader = new JsonTextReader(sr))
//                    {
//                        armouryInventory.ArmourList = serialiser.Deserialize<BaseArmour[]>(reader).ToList();
//                    }
//                }
//                using (StringReader sr = new StringReader(armoury.armoury_jewellery))
//                {
//                    using (JsonReader reader = new JsonTextReader(sr))
//                    {
//                        armouryInventory.JewelleryList = serialiser.Deserialize<BaseJewellery[]>(reader).ToList();
//                    }
//                }

//                armouryWeaponDict = armouryInventory.WeaponList.ToDictionary(item => item.LocalId);
//                armouryArmourDict = armouryInventory.ArmourList.ToDictionary(item => item.LocalId);
//                armouryJewelleryDict = armouryInventory.JewelleryList.ToDictionary(item => item.LocalId);

//                characterWeaponDict = characterInventory.WeaponList.ToDictionary(item => item.LocalId);
//                characterArmourDict = characterInventory.ArmourList.ToDictionary(item => item.LocalId);
//                characterJewelleryDict = characterInventory.JewelleryList.ToDictionary(item => item.LocalId);


//                ///check if adding or removing character inventory
//                ///IF checkIfExistsInCharacter == true THEN move item to armoury 
//                ///ELSE move item to character 
//                bool checkIfExistsInCharacter = false;
//                string equipmentType1 = equipmentIdNums[0].Substring(0, equipmentIdNums[0].IndexOf("_"));
//                switch (equipmentType1)
//                {
//                    case "weapon":
//                        if (characterWeaponDict.ContainsKey(equipmentIdNums[0])) checkIfExistsInCharacter = true;
//                        break;
//                    case "armour":
//                        if (characterArmourDict.ContainsKey(equipmentIdNums[0])) checkIfExistsInCharacter = true;
//                        break;
//                    case "jewellery":
//                        if (characterJewelleryDict.ContainsKey(equipmentIdNums[0])) checkIfExistsInCharacter = true;
//                        break;
//                }

//                ///check for payload validity
//                string[] restrictedEquipables = { "Weapon", "Amulet", "Ring", "Arms", "Feet", "Hands", "Head", "Legs", "Torso" };
//                var requestedEquipables = new Dictionary<string, int>();

//                foreach (var type in restrictedEquipables) requestedEquipables.Add(type, 0);
//                for (int i = 0; i < equipmentIdNums.Length; i++)
//                {
//                    string equipmentType2 = equipmentIdNums[i].Substring(0, equipmentIdNums[0].IndexOf("_"));
//                    var selectedArmourDict = checkIfExistsInCharacter ? characterArmourDict : armouryArmourDict;
//                    switch (equipmentType2)
//                    {
//                        case "weapon":
//                            requestedEquipables["Weapon"]++;
//                            if (requestedEquipables["Weapon"] > 1) return new InventoryUpdateResponse { Success = false, ErrorMessage = $"I see you." };
//                            break;
//                        case "armour":
//                            switch (selectedArmourDict[equipmentIdNums[i]].ArmourType)
//                            {
//                                case "Arms":
//                                    requestedEquipables["Arms"]++;
//                                    if (requestedEquipables["Arms"] > 1) return new InventoryUpdateResponse { Success = false, ErrorMessage = $"I see you." };
//                                    break;
//                                case "Feet":
//                                    requestedEquipables["Feet"]++;
//                                    if (requestedEquipables["Arms"] > 1) return new InventoryUpdateResponse { Success = false, ErrorMessage = $"I see you." };
//                                    break;
//                                case "Hands":
//                                    requestedEquipables["Hands"]++;
//                                    if (requestedEquipables["Hands"] > 1) return new InventoryUpdateResponse { Success = false, ErrorMessage = $"I see you." };
//                                    break;
//                                case "Head":
//                                    requestedEquipables["Head"]++;
//                                    if (requestedEquipables["Head"] > 1) return new InventoryUpdateResponse { Success = false, ErrorMessage = $"I see you." };
//                                    break;
//                                case "Legs":
//                                    requestedEquipables["Legs"]++;
//                                    if (requestedEquipables["Legs"] > 1) return new InventoryUpdateResponse { Success = false, ErrorMessage = $"I see you." };
//                                    break;
//                                case "Torso":
//                                    requestedEquipables["Torso"]++;
//                                    if (requestedEquipables["Torso"] > 1) return new InventoryUpdateResponse { Success = false, ErrorMessage = $"I see you." };
//                                    break;
//                            }
//                            break;
//                        case "jewellery":
//                            var selectedJewelleryDict = checkIfExistsInCharacter ? characterJewelleryDict : armouryJewelleryDict;
//                            switch (selectedJewelleryDict[equipmentIdNums[i]].JewelleryType)
//                            {
//                                case "Amulet":
//                                    requestedEquipables["Amulet"]++;
//                                    if (requestedEquipables["Amulet"] > 1) return new InventoryUpdateResponse { Success = false, ErrorMessage = $"I see you." };
//                                    break;
//                                case "Ring":
//                                    requestedEquipables["Ring"]++;
//                                    if (requestedEquipables["Ring"] > 2) return new InventoryUpdateResponse { Success = false, ErrorMessage = $"I see you." };
//                                    break;
//                            }
//                            break;
//                    }
//                }
//                int total = 0;
//                foreach (int itemCount in requestedEquipables.Values) total += itemCount;
//                if (equipmentIdNums.Length != total) return new InventoryUpdateResponse { Success = false, ErrorMessage = $"I see you." };
//                ///item id's provided exist in user's character or armoury inventories

//                if (inventoryUpdatePayload.RingHand != null && (inventoryUpdatePayload.RingHand.Count > 2 ||
//                    (inventoryUpdatePayload.RingHand.Count == 1 && !inventoryUpdatePayload.RingHand.ContainsValue(true) && !inventoryUpdatePayload.RingHand.ContainsValue(false)) ||
//                    (inventoryUpdatePayload.RingHand.Count == 2 && (!inventoryUpdatePayload.RingHand.ContainsValue(true) ||
//                    !inventoryUpdatePayload.RingHand.ContainsValue(false)))))
//                {
//                    return new InventoryUpdateResponse { Success = false, ErrorMessage = $"I see you." };
//                }
//                ///payload specifies correct ringhand details


//                ///check if swap is required for equip
//                IClaimable weaponToSwap = null;
//                List<IClaimable> equipmentSwapList = new List<IClaimable>(); //compile list of items to move to armoury when equipping requested items

//                if (!checkIfExistsInCharacter) //equip to character
//                {
//                    ///compile number of equipped items by type
//                    var equippedEquipables = new Dictionary<string, int>();
//                    foreach (var type in restrictedEquipables) equippedEquipables.Add(type, 0);

//                    //weapon
//                    if (characterInventory.WeaponList.Count == 1) weaponToSwap = characterInventory.WeaponList[0];

//                    //armour
//                    for (int i = 0; i < characterInventory.ArmourList.Count; i++)
//                    {
//                        equippedEquipables[characterInventory.ArmourList[i].ArmourType]++;
//                        if (equippedEquipables[characterInventory.ArmourList[i].ArmourType] > 0) equipmentSwapList.Add(characterInventory.ArmourList[i]);

//                    }//jewellery
//                    //ring/s
//                    if (inventoryUpdatePayload.RingHand != null) //if request specifies at minimum one ring to equip
//                    {
//                        foreach (var key in inventoryUpdatePayload.RingHand.Keys) //iterate though one or both of the requested ring LocalIds
//                            if (armouryJewelleryDict.ContainsKey(key)) //if ring exists in armoury list
//                            {
//                                if (inventoryUpdatePayload.RingHand[key] == false) //equip ring to left hand
//                                {
//                                    foreach (Ring ring in characterJewelleryDict.Values)
//                                        if (ring.RightHand == false)
//                                            equipmentSwapList.Add(ring); //found ring equipped to left hand, swap with this ring
//                                }
//                                else //equip ring to right hand
//                                {
//                                    foreach (Ring ring in characterJewelleryDict.Values)
//                                        if (ring.RightHand == true)
//                                            equipmentSwapList.Add(ring);//found ring equipped to right hand, swap with this ring
//                                }
//                            }
//                    }

//                    //else return new InventoryUpdateResponse { Success = false, ErrorMessage = $"I see you." };
//                    //i dont know why i put this here originally^
//                    //amulet
//                    for (int i = 0; i < characterInventory.JewelleryList.Count; i++)
//                    {
//                        equippedEquipables[characterInventory.JewelleryList[i].JewelleryType]++;
//                        if (characterInventory.JewelleryList[i].JewelleryType == "Amulet" && equippedEquipables[characterInventory.JewelleryList[i].JewelleryType] > 0) equipmentSwapList.Add(characterInventory.JewelleryList[i]);
//                    }
//                }

//                ///action swap from character inventory to armoury inventory
//                if (equipmentSwapList != null)
//                {
//                    foreach (var item in equipmentSwapList)
//                    {

//                        switch (item)
//                        {
//                            case BaseWeapon weapon:
//                                characterInventory.WeaponList.Remove(weapon);
//                                armouryInventory.WeaponList.Add(weapon);
//                                break;
//                            case BaseArmour armour:
//                                characterInventory.ArmourList.Remove(armour);
//                                armouryInventory.ArmourList.Add(armour);
//                                break;
//                            case BaseJewellery jewellery:
//                                characterInventory.JewelleryList.Remove(jewellery);
//                                armouryInventory.JewelleryList.Add(jewellery);
//                                break;
//                        }

//                    }
//                }


//                ///action add or remove
//                for (int i = 0; i < equipmentIdNums.Length; i++)
//                {
//                    string equipmentType2 = equipmentIdNums[i].Substring(0, equipmentIdNums[0].IndexOf("_"));
//                    string equipmentId = equipmentIdNums[i];
//                    if (checkIfExistsInCharacter)
//                    { //add to armoury

//                        switch (equipmentType2)
//                        {
//                            case "weapon":
//                                armouryInventory.WeaponList.Add(characterWeaponDict[equipmentId]);
//                                characterInventory.WeaponList.Remove(characterWeaponDict[equipmentId]);
//                                break;
//                            case "armour":
//                                armouryInventory.ArmourList.Add(characterArmourDict[equipmentId]);
//                                characterInventory.ArmourList.Remove(characterArmourDict[equipmentId]);
//                                break;
//                            case "jewellery":
//                                armouryInventory.JewelleryList.Add(characterJewelleryDict[equipmentId]);
//                                characterInventory.JewelleryList.Remove(characterJewelleryDict[equipmentId]);
//                                break;
//                        }
//                    }
//                    else
//                    { //add to character
//                        switch (equipmentType2)
//                        {
//                            case "weapon":
//                                armouryInventory.WeaponList.Remove(armouryWeaponDict[equipmentId]);
//                                characterInventory.WeaponList.Add(armouryWeaponDict[equipmentId]);
//                                break;
//                            case "armour":
//                                armouryInventory.ArmourList.Remove(armouryArmourDict[equipmentId]);
//                                characterInventory.ArmourList.Add(armouryArmourDict[equipmentId]);
//                                break;
//                            case "jewellery":
//                                armouryInventory.JewelleryList.Remove(armouryJewelleryDict[equipmentId]);
//                                characterInventory.JewelleryList.Add(armouryJewelleryDict[equipmentId]);
//                                break;
//                        }
//                    }
//                }


//                // After the move operations, re-serialize the updated inventories back into strings
//                string updatedCharacterWeapons = string.Empty;
//                string updatedCharacterArmour = string.Empty;
//                string updatedCharacterJewellery = string.Empty;

//                string updatedArmouryWeapons = string.Empty;
//                string updatedArmouryArmour = string.Empty;
//                string updatedArmouryJewellery = string.Empty;
//                using (StringWriter sw = new StringWriter())
//                {
//                    using (JsonWriter writer = new JsonTextWriter(sw))
//                    {
//                        serialiser.Serialize(writer, characterInventory.WeaponList);
//                        updatedCharacterWeapons = sw.ToString();
//                        sw.GetStringBuilder().Clear();
//                        serialiser.Serialize(writer, characterInventory.ArmourList);
//                        updatedCharacterArmour = sw.ToString();
//                        sw.GetStringBuilder().Clear();
//                        serialiser.Serialize(writer, characterInventory.JewelleryList);
//                        updatedCharacterJewellery = sw.ToString();
//                        sw.GetStringBuilder().Clear();

//                        serialiser.Serialize(writer, armouryInventory.WeaponList);
//                        updatedArmouryWeapons = sw.ToString();
//                        sw.GetStringBuilder().Clear();
//                        serialiser.Serialize(writer, armouryInventory.ArmourList);
//                        updatedArmouryArmour = sw.ToString();
//                        sw.GetStringBuilder().Clear();
//                        serialiser.Serialize(writer, armouryInventory.JewelleryList);
//                        updatedArmouryJewellery = sw.ToString();
//                        sw.GetStringBuilder().Clear();


//                    }
//                }

//                using (var transaction = await _dbContext.Database.BeginTransactionAsync())
//                {
//                    try
//                    {
//                        // Store the updated inventories back to the database
//                        character.character_weapons = updatedCharacterWeapons;
//                        character.character_armour = updatedCharacterArmour;
//                        character.character_jewellery = updatedCharacterJewellery;

//                        armoury.armoury_weapons = updatedArmouryWeapons;
//                        armoury.armoury_armour = updatedArmouryArmour;
//                        armoury.armoury_jewellery = updatedArmouryJewellery;

//                        await _dbContext.SaveChangesAsync();
//                        transaction.Commit();

//                    }
//                    catch (Exception ex)
//                    {
//                        System.Diagnostics.Debug.WriteLine($"Transaction failed, rolling back: {ex.Message}");  //add to dev log with timestamp and relevent game state details
//                        await transaction.RollbackAsync();
//                        return new InventoryUpdateResponse() { Success = false, ErrorMessage = $"Transaction failed, rolling back. Contact dev support for more information." };
//                    }

//                    return new InventoryUpdateResponse
//                    {
//                        Success = true
//                    };
//                }
//            }
//            catch (Exception ex)
//            {
//                System.Diagnostics.Debug.WriteLine($"Update character inventory failed: {ex.Message}"); //add to dev log
//                return new InventoryUpdateResponse() { Success = false, ErrorMessage = $"Update character inventory failed: Contact dev support for more information." };
//            }
//        }



//public async Task<IInventoryUpdateResponse> UpdateCharacterInventory(InventoryUpdatePayload inventoryUpdatePayload)
//{
//    try
//    {
//        ///get method contexts
//        ///
//        var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.NameIdentifier)?.Value;
//        var user = await _userManager.FindByIdAsync(userId);
//        ///complete
//        ///

//        ///initial payload validity check
//        ///
//        //payload should never exceed the number of total character inventory slots:
//        //one weapon
//        //one of each BaseArmour type (6 types in total)
//        //two rings (left and right hand)
//        //one amulet
//        if (inventoryUpdatePayload.EquipmentLocalIdNums.Count() > 10)
//            return new InventoryUpdateResponse { Success = false, ErrorMessage = $"I see you." };
//        ///complete
//        ///

//        ///initialise method tools
//        ///
//        //initialise inventory's
//        EquipmentInventory armouryInventory = null;
//        EquipmentInventory characterInventory = null;

//        //initialise inventory property dictionaries where IEquipable LocalId is each item's index
//        Dictionary<string, BaseWeapon> armouryWeaponDict = null;
//        Dictionary<string, BaseArmour> armouryArmourDict = null;
//        Dictionary<string, BaseJewellery> armouryJewelleryDict = null;

//        Dictionary<string, BaseWeapon> characterWeaponDict = null;
//        Dictionary<string, BaseArmour> characterArmourDict = null;
//        Dictionary<string, BaseJewellery> characterJewelleryDict = null;

//        //initialise user db objects
//        t_Character character = null; //await _dbContext.t_character.FirstAsync(u => u.fk_user_id == user.CustomUserId);
//        t_Armoury armoury = null; //await _dbContext.t_armoury.FirstAsync(u => u.fk_user_id == user.CustomUserId);
//        ///complete
//        ///

//        ///determine equip or de-equip (add or remove)
//        ///
//        //get alive character
//        character = await _dbContext.t_character
//            .Where(c => c.fk_user_id == user.CustomUserId && c.character_isalive == true)
//            .FirstOrDefaultAsync();

//        //assign character inventory
//        characterInventory = new EquipmentInventory();

//        //setup conditional deserialisation
//        Dictionary<string, bool> deserialisationCheckDict = new Dictionary<string, bool>()
//        {
//            { "characterWeaponList",false }, { "characterArmourList",false }, { "characterJewelleryList",false },
//            { "armouryWeaponList",false }, { "armouryArmourList",false }, { "armouryJewelleryList",false }
//        };
//        JsonSerializer serialiser = new JsonSerializer();
//        serialiser.Converters.Add(new DeserialisationSupport());

//        //setup dictionary to track number of equipment types from payload
//        Dictionary<string,int> payloadEquipmentTypes = new Dictionary<string, int>()
//            {
//                { "Weapon", 0 },
//                { "Arms", 0 }, { "Feet", 0 }, { "Hands", 0 }, { "Head", 0 }, { "Legs", 0 }, { "Torso", 0 },
//                { "Amulet", 0 }, { "Ring", 0 }
//            };

//        //search for payload items in character inventory
//        string equipmentBaseType = string.Empty;
//        Dictionary<string, bool> characterSearchResult = new Dictionary<string, bool>();
//        foreach (string localId in inventoryUpdatePayload.EquipmentLocalIdNums)
//        {
//            equipmentBaseType = localId.Substring(0, localId.IndexOf("_"));
//            characterSearchResult.Add(localId, false);

//            if (equipmentBaseType.Equals("weapon"))
//            {
//                if (!deserialisationCheckDict["characterWeaponList"])
//                {
//                    using (StringReader sr = new StringReader(character.character_weapons))
//                    {
//                        using (JsonReader reader = new JsonTextReader(sr))
//                        {
//                            characterInventory.WeaponList = serialiser.Deserialize<BaseWeapon[]>(reader).ToList();
//                        }
//                    }
//                    deserialisationCheckDict["characterWeaponList"] = true;

//                    characterWeaponDict = characterInventory.WeaponList.ToDictionary(item => item.LocalId);
//                }

//                foreach (BaseWeapon weapon in characterInventory.WeaponList)
//                {
//                    if (weapon.LocalId == localId)
//                    {
//                        characterSearchResult[localId] = true;

//                        payloadEquipmentTypes["Weapon"]++;
//                        if (payloadEquipmentTypes["Weapon"] > 1) return new InventoryUpdateResponse { Success = false, ErrorMessage = $"I see you." };
//                        break;
//                    }
//                }
//                if (characterSearchResult[localId] && inventoryUpdatePayload.EquipmentLocalIdNums[inventoryUpdatePayload.EquipmentLocalIdNums.Count() - 1] != localId) continue;
//                else if (characterSearchResult[localId] && inventoryUpdatePayload.EquipmentLocalIdNums[inventoryUpdatePayload.EquipmentLocalIdNums.Count() - 1] == localId) break;
//            }

//            else if (equipmentBaseType.Equals("armour"))
//            {
//                if (!deserialisationCheckDict["characterArmourList"])
//                {
//                    using (StringReader sr = new StringReader(character.character_armour))
//                    {
//                        using (JsonReader reader = new JsonTextReader(sr))
//                        {
//                            characterInventory.ArmourList = serialiser.Deserialize<BaseArmour[]>(reader).ToList();
//                        }
//                    }
//                    deserialisationCheckDict["characterArmourList"] = true;

//                    characterArmourDict = characterInventory.ArmourList.ToDictionary(item => item.LocalId);
//                }
//                foreach (BaseArmour armour in characterInventory.ArmourList)
//                {
//                    if (armour.LocalId == localId)
//                    {
//                        characterSearchResult[localId] = true;

//                        payloadEquipmentTypes[$"{armour.ArmourType}"]++;
//                        if (payloadEquipmentTypes[$"{armour.ArmourType}"] > 1) return new InventoryUpdateResponse { Success = false, ErrorMessage = $"I see you." };
//                        break;
//                    }
//                }
//                if (characterSearchResult[localId] && inventoryUpdatePayload.EquipmentLocalIdNums[inventoryUpdatePayload.EquipmentLocalIdNums.Count() - 1] != localId) continue;
//                else if (characterSearchResult[localId] && inventoryUpdatePayload.EquipmentLocalIdNums[inventoryUpdatePayload.EquipmentLocalIdNums.Count() - 1] == localId) break;
//            }

//            else if (equipmentBaseType.Equals("jewellery"))
//            {
//                if (!deserialisationCheckDict["characterJewelleryList"])
//                {
//                    using (StringReader sr = new StringReader(character.character_jewellery))
//                    {
//                        using (JsonReader reader = new JsonTextReader(sr))
//                        {
//                            characterInventory.JewelleryList = serialiser.Deserialize<BaseJewellery[]>(reader).ToList();
//                        }
//                    }
//                    deserialisationCheckDict["characterJewelleryList"] = true;

//                    characterJewelleryDict = characterInventory.JewelleryList.ToDictionary(item => item.LocalId);
//                }
//                foreach (BaseJewellery jewellery in characterInventory.JewelleryList)
//                {
//                    if (jewellery.LocalId == localId)
//                    {
//                        characterSearchResult[localId] = true;

//                        payloadEquipmentTypes[$"{jewellery.JewelleryType}"]++;
//                        if (payloadEquipmentTypes["Amulet"] > 1 || payloadEquipmentTypes["Ring"] > 2) return new InventoryUpdateResponse { Success = false, ErrorMessage = $"I see you." };
//                        break;
//                    }
//                }
//            }

//            else
//            {
//                return new InventoryUpdateResponse { Success = false, ErrorMessage = $"I see you." }; //a valid payload at this point mustn't fail this check
//            }
//        }
//        //check if search is consistant
//        bool badPayloadDetected = characterSearchResult[inventoryUpdatePayload.EquipmentLocalIdNums[0]];
//        foreach (bool value in characterSearchResult.Values) if (value != badPayloadDetected) return new InventoryUpdateResponse { Success = false, ErrorMessage = $"I see you." };
//        ///complete
//        ///

//        //assign armoury inventory
//        armouryInventory = new EquipmentInventory();

//        if (characterSearchResult[inventoryUpdatePayload.EquipmentLocalIdNums[0]])
//        {///items are to be moved to armoury

//            //this needs work...

//            //foreach item 
//            //if jewellery
//            //if ring
//            //ringCount++
//            //if ringCount == 2
//            //de-equip both rings
//            //if ringCount == 1
//            //if RingHand == left hand
//            //de-equip left handed ring
//            //else de-equip right handed ring
//        }
//        else
//        {///items are to be moved to character (after a further check for payload validity using a search against armouryInventory), and may require swapping
//            //search for payload items in armoury inventory as part of payload validity check
//            bool isFound = false;
//            Dictionary<string, IEquipable> armourySearchResult = new Dictionary<string, IEquipable>();
//            foreach (string localId in inventoryUpdatePayload.EquipmentLocalIdNums)
//            {
//                isFound = false;

//                equipmentBaseType = localId.Substring(0, localId.IndexOf("_"));

//                if (equipmentBaseType.Equals("weapon"))
//                {
//                    if (!deserialisationCheckDict["armouryWeaponList"])
//                    {
//                        using (StringReader sr = new StringReader(character.character_weapons))
//                        {
//                            using (JsonReader reader = new JsonTextReader(sr))
//                            {
//                                armouryInventory.WeaponList = serialiser.Deserialize<BaseWeapon[]>(reader).ToList();
//                            }
//                        }
//                        deserialisationCheckDict["armouryWeaponList"] = true;

//                        armouryWeaponDict = armouryInventory.WeaponList.ToDictionary(item => item.LocalId);
//                    }
//                    foreach (BaseWeapon weapon in armouryInventory.WeaponList)
//                    {

//                        if (weapon.LocalId == localId)
//                        {
//                            armourySearchResult.Add(localId, weapon);
//                            isFound = true;

//                            payloadEquipmentTypes["Weapon"]++;
//                            if (payloadEquipmentTypes["Weapon"] > 1) return new InventoryUpdateResponse { Success = false, ErrorMessage = $"I see you." };
//                            break;
//                        }
//                    }
//                    if (isFound && inventoryUpdatePayload.EquipmentLocalIdNums[inventoryUpdatePayload.EquipmentLocalIdNums.Count() - 1] != localId) continue;
//                    else if (isFound && inventoryUpdatePayload.EquipmentLocalIdNums[inventoryUpdatePayload.EquipmentLocalIdNums.Count() - 1] == localId) break;
//                }
//                else if (equipmentBaseType.Equals("armour"))
//                {
//                    if (!deserialisationCheckDict["armouryArmourList"])
//                    {
//                        using (StringReader sr = new StringReader(character.character_armour))
//                        {
//                            using (JsonReader reader = new JsonTextReader(sr))
//                            {
//                                armouryInventory.ArmourList = serialiser.Deserialize<BaseArmour[]>(reader).ToList();
//                            }
//                        }
//                        deserialisationCheckDict["armouryArmourList"] = true;

//                        armouryArmourDict = armouryInventory.ArmourList.ToDictionary(item => item.LocalId);
//                    }
//                    foreach (BaseArmour armour in armouryInventory.ArmourList)
//                    {
//                        if (armour.LocalId == localId)
//                        {
//                            armourySearchResult.Add(localId, armour);
//                            isFound = true;

//                            payloadEquipmentTypes[$"{armour.ArmourType}"]++;
//                            if (payloadEquipmentTypes[$"{armour.ArmourType}"] > 1) return new InventoryUpdateResponse { Success = false, ErrorMessage = $"I see you." };
//                            break;
//                        }
//                    }
//                    if (isFound && inventoryUpdatePayload.EquipmentLocalIdNums[inventoryUpdatePayload.EquipmentLocalIdNums.Count() - 1] != localId) continue;
//                    else if (isFound && inventoryUpdatePayload.EquipmentLocalIdNums[inventoryUpdatePayload.EquipmentLocalIdNums.Count() - 1] == localId) break;
//                }
//                else if (equipmentBaseType.Equals("jewellery"))
//                {
//                    if (!deserialisationCheckDict["armouryJewelleryList"])
//                    {
//                        using (StringReader sr = new StringReader(character.character_jewellery))
//                        {
//                            using (JsonReader reader = new JsonTextReader(sr))
//                            {
//                                armouryInventory.JewelleryList = serialiser.Deserialize<BaseJewellery[]>(reader).ToList();
//                            }
//                        }
//                        deserialisationCheckDict["armouryJewelleryList"] = true;

//                        armouryJewelleryDict = armouryInventory.JewelleryList.ToDictionary(item => item.LocalId);
//                    }
//                    foreach (BaseJewellery jewellery in armouryInventory.JewelleryList)
//                    {
//                        if (jewellery.LocalId == localId)
//                        {
//                            armourySearchResult.Add(localId, jewellery);
//                            isFound = true;

//                            payloadEquipmentTypes[$"{jewellery.JewelleryType}"]++;
//                            if (payloadEquipmentTypes["Amulet"] > 1 || payloadEquipmentTypes["Ring"] > 2) return new InventoryUpdateResponse { Success = false, ErrorMessage = $"I see you." };
//                            break;
//                        }
//                    }
//                }
//                else
//                {
//                    return new InventoryUpdateResponse { Success = false, ErrorMessage = $"I see you." };
//                }
//                if (!isFound) return new InventoryUpdateResponse { Success = false, ErrorMessage = $"I see you." }; //required here as each payload item must exist in armouryInventory at this point
//            }









/////item id's provided exist in user's character or armoury inventories

//if (inventoryUpdatePayload.RingHand != null && (inventoryUpdatePayload.RingHand.Count > 2 ||
//    (inventoryUpdatePayload.RingHand.Count == 1 && !inventoryUpdatePayload.RingHand.ContainsValue(true) && !inventoryUpdatePayload.RingHand.ContainsValue(false)) ||
//    (inventoryUpdatePayload.RingHand.Count == 2 && (!inventoryUpdatePayload.RingHand.ContainsValue(true) ||
//    !inventoryUpdatePayload.RingHand.ContainsValue(false)))))
//{
//    return new InventoryUpdateResponse { Success = false, ErrorMessage = $"I see you." };
//}
/////payload specifies correct ringhand details

//List<IDeserialisable> equipmentPiece = new List<IDeserialisable>();

//EquipmentInventory characterInventory = new EquipmentInventory();
//EquipmentInventory armouryInventory = new EquipmentInventory();

//Dictionary<string, BaseWeapon> armouryWeaponDict = new Dictionary<string, BaseWeapon>();
//Dictionary<string, BaseArmour> armouryArmourDict = new Dictionary<string, BaseArmour>();
//Dictionary<string, BaseJewellery> armouryJewelleryDict = new Dictionary<string, BaseJewellery>();

//Dictionary<string, BaseWeapon> characterWeaponDict = new Dictionary<string, BaseWeapon>();
//Dictionary<string, BaseArmour> characterArmourDict = new Dictionary<string, BaseArmour>();
//Dictionary<string, BaseJewellery> characterJewelleryDict = new Dictionary<string, BaseJewellery>();

//t_Character character = await _dbContext.t_character.FirstAsync(u => u.fk_user_id == user.CustomUserId);
//t_Armoury armoury = await _dbContext.t_armoury.FirstAsync(u => u.fk_user_id == user.CustomUserId);

////deserialise user inventories
//JsonSerializer serialiser = new JsonSerializer();
//serialiser.Converters.Add(new DeserialisationSupport());
/////character inventory
//using (StringReader sr = new StringReader(character.character_weapons))
//{
//    using (JsonReader reader = new JsonTextReader(sr))
//    {
//        characterInventory.WeaponList = serialiser.Deserialize<BaseWeapon[]>(reader).ToList();
//    }
//}
//using (StringReader sr = new StringReader(character.character_armour))
//{
//    using (JsonReader reader = new JsonTextReader(sr))
//    {
//        characterInventory.ArmourList = serialiser.Deserialize<BaseArmour[]>(reader).ToList();
//    }
//}
//using (StringReader sr = new StringReader(character.character_jewellery))
//{
//    using (JsonReader reader = new JsonTextReader(sr))
//    {
//        characterInventory.JewelleryList = serialiser.Deserialize<BaseJewellery[]>(reader).ToList();
//    }
//}
/////armoury inventory
//using (StringReader sr = new StringReader(armoury.armoury_weapons))
//{
//    using (JsonReader reader = new JsonTextReader(sr))
//    {
//        armouryInventory.WeaponList = serialiser.Deserialize<BaseWeapon[]>(reader).ToList();
//    }
//}
//using (StringReader sr = new StringReader(armoury.armoury_armour))
//{
//    using (JsonReader reader = new JsonTextReader(sr))
//    {
//        armouryInventory.ArmourList = serialiser.Deserialize<BaseArmour[]>(reader).ToList();
//    }
//}
//using (StringReader sr = new StringReader(armoury.armoury_jewellery))
//{
//    using (JsonReader reader = new JsonTextReader(sr))
//    {
//        armouryInventory.JewelleryList = serialiser.Deserialize<BaseJewellery[]>(reader).ToList();
//    }
//}

//armouryWeaponDict = armouryInventory.WeaponList.ToDictionary(item => item.LocalId);
//armouryArmourDict = armouryInventory.ArmourList.ToDictionary(item => item.LocalId);
//armouryJewelleryDict = armouryInventory.JewelleryList.ToDictionary(item => item.LocalId);

//characterWeaponDict = characterInventory.WeaponList.ToDictionary(item => item.LocalId);
//characterArmourDict = characterInventory.ArmourList.ToDictionary(item => item.LocalId);
//characterJewelleryDict = characterInventory.JewelleryList.ToDictionary(item => item.LocalId);


/////check if adding or removing character inventory
/////IF checkIfExistsInCharacter == true THEN move item to armoury 
/////ELSE move item to character 
//bool checkIfExistsInCharacter = false;
//string equipmentType1 = equipmentIdNums[0].Substring(0, equipmentIdNums[0].IndexOf("_"));
//switch (equipmentType1)
//{
//    case "weapon":
//        if (characterWeaponDict.ContainsKey(equipmentIdNums[0])) checkIfExistsInCharacter = true;
//        break;
//    case "armour":
//        if (characterArmourDict.ContainsKey(equipmentIdNums[0])) checkIfExistsInCharacter = true;
//        break;
//    case "jewellery":
//        if (characterJewelleryDict.ContainsKey(equipmentIdNums[0])) checkIfExistsInCharacter = true;
//        break;
//}

/////check for payload validity
//string[] restrictedEquipables = { "Weapon", "Amulet", "Ring", "Arms", "Feet", "Hands", "Head", "Legs", "Torso" };
//var payloadEquipmentTypes = new Dictionary<string, int>();

//foreach (var type in restrictedEquipables) payloadEquipmentTypes.Add(type, 0);
//for (int i = 0; i < equipmentIdNums.Length; i++)
//{
//    string equipmentType2 = equipmentIdNums[i].Substring(0, equipmentIdNums[0].IndexOf("_"));
//    var selectedArmourDict = checkIfExistsInCharacter ? characterArmourDict : armouryArmourDict;
//    switch (equipmentType2)
//    {
//        case "weapon":
//            payloadEquipmentTypes["Weapon"]++;
//            if (payloadEquipmentTypes["Weapon"] > 1) return new InventoryUpdateResponse { Success = false, ErrorMessage = $"I see you." };
//            break;
//        case "armour":
//            switch (selectedArmourDict[equipmentIdNums[i]].ArmourType)
//            {
//                case "Arms":
//                    payloadEquipmentTypes["Arms"]++;
//                    if (payloadEquipmentTypes["Arms"] > 1) return new InventoryUpdateResponse { Success = false, ErrorMessage = $"I see you." };
//                    break;
//                case "Feet":
//                    payloadEquipmentTypes["Feet"]++;
//                    if (payloadEquipmentTypes["Arms"] > 1) return new InventoryUpdateResponse { Success = false, ErrorMessage = $"I see you." };
//                    break;
//                case "Hands":
//                    payloadEquipmentTypes["Hands"]++;
//                    if (payloadEquipmentTypes["Hands"] > 1) return new InventoryUpdateResponse { Success = false, ErrorMessage = $"I see you." };
//                    break;
//                case "Head":
//                    payloadEquipmentTypes["Head"]++;
//                    if (payloadEquipmentTypes["Head"] > 1) return new InventoryUpdateResponse { Success = false, ErrorMessage = $"I see you." };
//                    break;
//                case "Legs":
//                    payloadEquipmentTypes["Legs"]++;
//                    if (payloadEquipmentTypes["Legs"] > 1) return new InventoryUpdateResponse { Success = false, ErrorMessage = $"I see you." };
//                    break;
//                case "Torso":
//                    payloadEquipmentTypes["Torso"]++;
//                    if (payloadEquipmentTypes["Torso"] > 1) return new InventoryUpdateResponse { Success = false, ErrorMessage = $"I see you." };
//                    break;
//            }
//            break;
//        case "jewellery":
//            var selectedJewelleryDict = checkIfExistsInCharacter ? characterJewelleryDict : armouryJewelleryDict;
//            switch (selectedJewelleryDict[equipmentIdNums[i]].JewelleryType)
//            {
//                case "Amulet":
//                    payloadEquipmentTypes["Amulet"]++;
//                    if (payloadEquipmentTypes["Amulet"] > 1) return new InventoryUpdateResponse { Success = false, ErrorMessage = $"I see you." };
//                    break;
//                case "Ring":
//                    payloadEquipmentTypes["Ring"]++;
//                    if (payloadEquipmentTypes["Ring"] > 2) return new InventoryUpdateResponse { Success = false, ErrorMessage = $"I see you." };
//                    break;
//            }
//            break;
//    }
//}
//int total = 0;
//foreach (int itemCount in payloadEquipmentTypes.Values) total += itemCount;
//if (equipmentIdNums.Length != total) return new InventoryUpdateResponse { Success = false, ErrorMessage = $"I see you." };
/////item id's provided exist in user's character or armoury inventories

//if (inventoryUpdatePayload.RingHand != null && (inventoryUpdatePayload.RingHand.Count > 2 ||
//    (inventoryUpdatePayload.RingHand.Count == 1 && !inventoryUpdatePayload.RingHand.ContainsValue(true) && !inventoryUpdatePayload.RingHand.ContainsValue(false)) ||
//    (inventoryUpdatePayload.RingHand.Count == 2 && (!inventoryUpdatePayload.RingHand.ContainsValue(true) ||
//    !inventoryUpdatePayload.RingHand.ContainsValue(false)))))
//{
//    return new InventoryUpdateResponse { Success = false, ErrorMessage = $"I see you." };
//}
/////payload specifies correct ringhand details


/////check if swap is required for equip
//IClaimable weaponToSwap = null;
//List<IClaimable> equipmentSwapList = new List<IClaimable>(); //compile list of items to move to armoury when equipping requested items

//if (!checkIfExistsInCharacter) //equip to character
//{
//    ///compile number of equipped items by type
//    var equippedEquipables = new Dictionary<string, int>();
//    foreach (var type in restrictedEquipables) equippedEquipables.Add(type, 0);

//    //weapon
//    if (characterInventory.WeaponList.Count == 1) weaponToSwap = characterInventory.WeaponList[0];

//    //armour
//    for (int i = 0; i < characterInventory.ArmourList.Count; i++)
//    {
//        equippedEquipables[characterInventory.ArmourList[i].ArmourType]++;
//        if (equippedEquipables[characterInventory.ArmourList[i].ArmourType] > 0) equipmentSwapList.Add(characterInventory.ArmourList[i]);

//    }//jewellery
//    //ring/s
//    if (inventoryUpdatePayload.RingHand != null) //if request specifies at minimum one ring to equip
//    {
//        foreach (var key in inventoryUpdatePayload.RingHand.Keys) //iterate though one or both of the requested ring LocalIds
//            if (armouryJewelleryDict.ContainsKey(key)) //if ring exists in armoury list
//            {
//                if (inventoryUpdatePayload.RingHand[key] == false) //equip ring to left hand
//                {
//                    foreach (Ring ring in characterJewelleryDict.Values)
//                        if (ring.RightHand == false)
//                            equipmentSwapList.Add(ring); //found ring equipped to left hand, swap with this ring
//                }
//                else //equip ring to right hand
//                {
//                    foreach (Ring ring in characterJewelleryDict.Values)
//                        if (ring.RightHand == true)
//                            equipmentSwapList.Add(ring);//found ring equipped to right hand, swap with this ring
//                }
//            }
//    }

//    //else return new InventoryUpdateResponse { Success = false, ErrorMessage = $"I see you." };
//    //i dont know why i put this here originally^
//    //amulet
//    for (int i = 0; i < characterInventory.JewelleryList.Count; i++)
//    {
//        equippedEquipables[characterInventory.JewelleryList[i].JewelleryType]++;
//        if (characterInventory.JewelleryList[i].JewelleryType == "Amulet" && equippedEquipables[characterInventory.JewelleryList[i].JewelleryType] > 0) equipmentSwapList.Add(characterInventory.JewelleryList[i]);
//    }
//}

/////action swap from character inventory to armoury inventory
//if (equipmentSwapList != null)
//{
//    foreach (var item in equipmentSwapList)
//    {

//        switch (item)
//        {
//            case BaseWeapon weapon:
//                characterInventory.WeaponList.Remove(weapon);
//                armouryInventory.WeaponList.Add(weapon);
//                break;
//            case BaseArmour armour:
//                characterInventory.ArmourList.Remove(armour);
//                armouryInventory.ArmourList.Add(armour);
//                break;
//            case BaseJewellery jewellery:
//                characterInventory.JewelleryList.Remove(jewellery);
//                armouryInventory.JewelleryList.Add(jewellery);
//                break;
//        }

//    }
//}


/////action add or remove
//for (int i = 0; i < equipmentIdNums.Length; i++)
//{
//    string equipmentType2 = equipmentIdNums[i].Substring(0, equipmentIdNums[0].IndexOf("_"));
//    string equipmentId = equipmentIdNums[i];
//    if (checkIfExistsInCharacter)
//    { //add to armoury

//        switch (equipmentType2)
//        {
//            case "weapon":
//                armouryInventory.WeaponList.Add(characterWeaponDict[equipmentId]);
//                characterInventory.WeaponList.Remove(characterWeaponDict[equipmentId]);
//                break;
//            case "armour":
//                armouryInventory.ArmourList.Add(characterArmourDict[equipmentId]);
//                characterInventory.ArmourList.Remove(characterArmourDict[equipmentId]);
//                break;
//            case "jewellery":
//                armouryInventory.JewelleryList.Add(characterJewelleryDict[equipmentId]);
//                characterInventory.JewelleryList.Remove(characterJewelleryDict[equipmentId]);
//                break;
//        }
//    }
//    else
//    { //add to character
//        switch (equipmentType2)
//        {
//            case "weapon":
//                armouryInventory.WeaponList.Remove(armouryWeaponDict[equipmentId]);
//                characterInventory.WeaponList.Add(armouryWeaponDict[equipmentId]);
//                break;
//            case "armour":
//                armouryInventory.ArmourList.Remove(armouryArmourDict[equipmentId]);
//                characterInventory.ArmourList.Add(armouryArmourDict[equipmentId]);
//                break;
//            case "jewellery":
//                armouryInventory.JewelleryList.Remove(armouryJewelleryDict[equipmentId]);
//                characterInventory.JewelleryList.Add(armouryJewelleryDict[equipmentId]);
//                break;
//        }
//    }
//}





//for (int i = 0; i < equipmentIdNums.Length; i++)
//{
//    string equipmentId = equipmentIdNums[i];
//    string equipmentType = equipmentTypes[i];

//    if (equipmentEquipOrRemove == 1) // move to armoury
//    {
//        switch (equipmentType)
//        {
//            case "Weapon":
//                var weaponToMove = characterInventory.WeaponList
//                    .FirstOrDefault(w => w.LocalId == equipmentId);
//                if (weaponToMove != null)
//                {
//                    // Remove from character's inventory
//                    characterInventory.WeaponList.Remove(weaponToMove);
//                    // Add to armoury inventory
//                    armouryInventory.WeaponList.Add(weaponToMove);
//                }
//                break;

//            case "Armour":
//                var armourToMove = characterInventory.ArmourList
//                    .FirstOrDefault(a => a.LocalId == equipmentId);
//                if (armourToMove != null)
//                {
//                    // Remove from character's inventory
//                    characterInventory.ArmourList.Remove(armourToMove);
//                    // Add to armoury inventory
//                    armouryInventory.ArmourList.Add(armourToMove);
//                }
//                break;

//            case "Jewellery":
//                var jewelleryToMove = characterInventory.JewelleryList
//                    .FirstOrDefault(j => j.LocalId == equipmentId);
//                if (jewelleryToMove != null)
//                {
//                    // Remove from character's inventory
//                    characterInventory.JewelleryList.Remove(jewelleryToMove);
//                    // Add to armoury inventory
//                    armouryInventory.JewelleryList.Add(jewelleryToMove);
//                }
//                break;
//        }
//    }
//}
//else if (equipmentEquipOrRemove == 0) // move to character
//{
//    switch (equipmentType)
//    {
//        case "Weapon":
//            var weaponToMove = armouryInventory.WeaponList
//                .FirstOrDefault(w => w.LocalId == equipmentId);
//            if (weaponToMove != null)
//            {
//                // Remove from armoury inventory
//                armouryInventory.WeaponList.Remove(weaponToMove);
//                // Add to character's inventory
//                characterInventory.WeaponList.Add(weaponToMove);
//            }
//            break;

//        case "Armour":
//            var armourToMove = armouryInventory.ArmourList
//                .FirstOrDefault(a => a.LocalId == equipmentId);
//            if (armourToMove != null)
//            {
//                // Remove from armoury inventory
//                armouryInventory.ArmourList.Remove(armourToMove);
//                // Add to character's inventory
//                characterInventory.ArmourList.Add(armourToMove);
//            }
//            break;

//        case "Jewellery":
//            var jewelleryToMove = armouryInventory.JewelleryList
//                .FirstOrDefault(j => j.LocalId == equipmentId);
//            if (jewelleryToMove != null)
//            {
//                // Remove from armoury inventory
//                armouryInventory.JewelleryList.Remove(jewelleryToMove);
//                // Add to character's inventory
//                characterInventory.JewelleryList.Add(jewelleryToMove);
//            }
//            break;
//    }
//}

//        switch ()
//{
//    case "Weapon":

//        break;
//    case "Armour":

//        break;
//    case "Jewellery":

//        break;
//}
//string characterWeaponsSerialised, characterArmourSerialised, characterJewellerySerialised;
//string armouryWeaponsSerialised, armouryArmourSerialised, armouryJewellerySerialised;
//for (int i = 0; i < equipmentIdNums.Length; i++)
//{
//}
//if (equipmentEquipOrRemove == 0)//move to armoury
//{
//}
//else if (equipmentEquipOrRemove == 1)//move to character
//{

//}
//switch (equipmentTypes[i])
//{
//    case "Weapon":
//        serialiser.Deserialize(writer, characterWeapons);
//        characterWeaponsSerialised = sw.ToString();
//        sw.GetStringBuilder().Clear();
//        break;
//    case "Armour":

//        break;
//    case "Jewellery":

//        break;
//}

//        serialiser.Serialize(writer, characterWeapons);
//        characterWeaponsSerialised = sw.ToString();
//        sw.GetStringBuilder().Clear();
//        serialiser.Serialize(writer, characterArmour);
//        characterArmourSerialised = sw.ToString();
//        sw.GetStringBuilder().Clear();
//        serialiser.Serialize(writer, characterJewellery);
//        characterJewellerySerialised = sw.ToString();
//        sw.GetStringBuilder().Clear();


//        serialiser.Serialize(writer, armouryWeapons);
//        armouryWeaponsSerialised = sw.ToString();
//        sw.GetStringBuilder().Clear();
//        serialiser.Serialize(writer, armouryArmour);
//        armouryArmourSerialised = sw.ToString();
//        sw.GetStringBuilder().Clear();
//        serialiser.Serialize(writer, armouryJewellery);
//        armouryJewellerySerialised = sw.ToString();
//        sw.GetStringBuilder().Clear();
//    }
//}
//serialiser = null;


//var result = JsonConvert.DeserializeObject<IDeserialisable>(character.character_weapons,new DeserialisationSupport());

//for (int i = 0; i < equipmentIdNums.Length; i++)
//{
//    switch (equipmentTypes[i])
//    {
//        case "Weapon":
//            equipmentPiece.Add(character.character_weapons
//                .First(e => e.LocalId == equipmentIdNums[i]));
//            break;
//        case "Armour":
//            equipmentPiece.Add(character.character_armour
//                .First(e => e.LocalId == equipmentIdNums[i]));
//            break;
//        case "Jewellery":
//            equipmentPiece.Add(character.character_jewellery
//                 .First(e => e.LocalId == equipmentIdNums[i]));
//            break;
//    }
//}
//for (int i = 0; i < equipmentIdNums.Length; i++)
//{
//    switch (equipmentTypes[i])
//    {
//        case "Weapon":
//            armoury.armoury_weapons .<BaseWeapon>equipmentPiece(i);
//            break;
//        case "Armour":

//            break;
//        case "Jewellery":

//            break;
//    }

//}
//var deserialisableList = JsonConvert.DeserializeObject<IDeserialisable>(equipmentArray.ToString(), new DeserialisationSupport()) as List<IDeserialisable>;



//foreach (JToken item in equipmentArray)
//{
//    IDeserialisable equipmentItem = JsonConvert.DeserializeObject<IDeserialisable>(item.ToString(), new DeserialisationSupport());
//    deserialisableList.Add(equipmentItem);
//}

//JsonNode jsonNode = JsonNode.Parse(armoury.armoury_inventory);
//JsonArray equipmentArray = jsonNode["Equipment"].AsArray();
//int counter =  equipmentArray.Count;

//var inventory = JsonDocument.Parse(armoury.armoury_inventory);
//int equipmentCount = inventory.RootElement.GetProperty("Equipment").GetArrayLength();

//WHEN I CONTINUE, REMEMBER: REMOVE Equipment and AttributeArray array, just have array elements themselves in db, this count method shoudl work


// EquipmentInventory characterInventory = new EquipmentInventory();
//EquipmentInventory armouryInventory = JsonConvert.DeserializeObject<List<IDeserialisable>>(armoury.armoury_inventory, new DeserialisationSupport());



//using (var transaction = await _dbContext.Database.BeginTransactionAsync())
//{
//    var sqlResult = GenerateInventoryUpdateSQL(user.CustomUserId, equipmentEquipOrRemove, equipmentIdNums);

//    await _dbContext.Database.ExecuteSqlRawAsync(sqlResult.Item1, sqlResult.Item2);

//    await transaction.CommitAsync();
//}
//public (string, NpgsqlParameter[]) GenerateInventoryUpdateSQL(int userId, int equipmentEquipOrRemove, string[] equipmentLocalIds)
//{
//    var sqlBuilder = new StringBuilder();
//    var parameters = new List<NpgsqlParameter>();

//    int operationStage = 0;

//    for (int ii = 0; ii < equipmentLocalIds.Length; ii++)
//    {
//        if (operationStage == 0) //stage 0 == acquire equipment
//        {
//            if (equipmentEquipOrRemove == 0) //remove from character, add to armoury || de-equip character
//            {
//                sqlBuilder.Append(@"UPDATE t_armoury SET armoury_inventory = jsonb_insert(armoury_inventory, @EquipmentArray, @NewEquipment::jsonb) WHERE fk_user_id = @UserId");


//                sqlBuilder.Append(@"UPDATE t_character SET character_inventory =         WHERE fk_user_id = @UserId");
//            }
//            else if (equipmentEquipOrRemove == 1) //remove from armoury, add to character || equip character
//            {

//            }
//        }
//        else if (operationStage == 1)//stage 1 == move equipment
//        {
//            if (equipmentEquipOrRemove == 0) //remove from character, add to armoury || de-equip character
//            {

//            }
//            else if (equipmentEquipOrRemove == 1) //remove from armoury, add to character || equip character
//            {

//            }
//        }
//    }

//    return (sqlBuilder.ToString(), parameters.ToArray());
//}

//else if (operationStage == 1) //construct SQL to move from armoury to character inventories or vice versa
//{
//    for (int ii = 0; ii < equipmentLocalIds.Length; ii++)
//    {
//        if (equipmentEquipOrRemove == 0) //gather equipment piece's properties from character_inventory
//        {
//            sqlBuilder.Append(@"UPDATE t_armoury SET armoury_inventory = jsonb_insert(armoury_inventory, @EquipmentArray, @NewEquipment::jsonb) WHERE fk_user_id = @UserId");



//            parameters.Add(new NpgsqlParameter("@NewEquipment", NpgsqlDbType.Jsonb) { Value = newClaimableJson });
//            parameters.Add(new NpgsqlParameter("@EquipmentArray", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = new string[] { "Equipment", "-1" } });
//            parameters.Add(new NpgsqlParameter("@UserId", NpgsqlDbType.Integer) { Value = userId });

//        }
//        else if (equipmentEquipOrRemove == 1) //gather equipment piece's properties from armoury_inventory
//        {
//            sqlBuilder.Append(@"UPDATE t_armoury SET armoury_inventory = jsonb_insert(armoury_inventory, @EquipmentArray, @NewEquipment::jsonb) WHERE fk_user_id = @UserId");

//            parameters =
//            [
//                new NpgsqlParameter("@NewEquipment", NpgsqlDbType.Jsonb) { Value = newClaimableJson },
//        new NpgsqlParameter("@EquipmentArray", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = new string[] { "Equipment", "-1" } },
//        new NpgsqlParameter("@UserId", NpgsqlDbType.Integer) { Value = user.CustomUserId }
//            ];
//        }
//    }
//}


//for (int i = 0; i < equipmentLocalIds.Length; i++)
//{
//    string localIdParam = $"@LocalId{i}";
//    string userIdParam = $"@UserId{i}";

//    parameters.Add(new NpgsqlParameter(localIdParam, NpgsqlDbType.Text) { Value = equipmentLocalIds[i] });
//    parameters.Add(new NpgsqlParameter(userIdParam, NpgsqlDbType.Integer) { Value = userId });

//    if (equipmentEquipOrRemove[i] == 1) // Move from armoury to character (Add equipment to character)
//    {
//        // sqlBuilder.Append
//    }
//    else if (equipmentEquipOrRemove[i] == 0) // Move from character to armoury (Remove equipment from character)
//    {
//    }
//}
//public (string, NpgsqlParameter[]) GenerateInventoryUpdateSQL(int userId, int[] equipmentEquipOrRemove, string[] equipmentLocalIds)
//{
//    var sqlBuilder = new StringBuilder();
//    var parameters = new List<NpgsqlParameter>();

//    for (int i = 0; i < equipmentLocalIds.Length; i++)
//    {
//        string localIdParam = $"@LocalId{i}";
//        string userIdParam = $"@UserId{i}";

//        parameters.Add(new NpgsqlParameter(localIdParam, NpgsqlDbType.Text) { Value = equipmentLocalIds[i] });
//        parameters.Add(new NpgsqlParameter(userIdParam, NpgsqlDbType.Integer) { Value = userId });

//        if (equipmentEquipOrRemove[i] == 1) // Move from armoury to character (Add equipment to character)
//        {
//            sqlBuilder.AppendLine($@"WITH equipment AS (SELECT equipment FROM jsonb_array_elements((SELECT armoury_inventory->'Equipment' FROM t_armoury WHERE fk_user_id = {userIdParam})) AS equipment WHERE equipment->>'LocalId' = {localIdParam}) UPDATE t_character SET character_inventory = character_inventory || equipment FROM equipment WHERE fk_user_id = {userIdParam};");
//            sqlBuilder.AppendLine($@"UPDATE t_armoury SET armoury_inventory = (SELECT jsonb_agg(equipment) FROM jsonb_array_elements(armoury_inventory->'Equipment') AS equipment WHERE equipment->>'LocalId' != {localIdParam} AND fk_user_id = {userIdParam}) WHERE fk_user_id = {userIdParam};");
//        }
//        else if (equipmentEquipOrRemove[i] == 0) // Move from character to armoury (Remove equipment from character)
//        {
//            sqlBuilder.AppendLine($@"WITH equipment AS (SELECT equipment FROM jsonb_array_elements((SELECT character_inventory FROM t_character WHERE fk_user_id = {userIdParam})) AS equipment WHERE equipment->>'LocalId' = {localIdParam}) UPDATE t_armoury SET armoury_inventory = armoury_inventory || equipment FROM equipment WHERE fk_user_id = {userIdParam};");
//            sqlBuilder.AppendLine($@"UPDATE t_character SET character_inventory = (SELECT jsonb_agg(equipment) FROM jsonb_array_elements(character_inventory) AS equipment WHERE equipment->>'LocalId' != {localIdParam} AND fk_user_id = {userIdParam}) WHERE fk_user_id = {userIdParam};");
//        }
//    }

//    return (sqlBuilder.ToString(), parameters.ToArray());
//}







//public (string, NpgsqlParameter[]) GenerateInventoryUpdateSQL(int userId, int[] equipmentEquipOrRemove, int[] equipmentLocalIds)
//{
//    var sqlBuilder = new StringBuilder();
//    var parameters = new List<NpgsqlParameter>();

//    for (int i = 0; i < equipmentLocalIds.Length; i++)
//    {
//        string localIdParam = $"@LocalId{i}";
//        parameters.Add(new NpgsqlParameter(localIdParam, NpgsqlTypes.NpgsqlDbType.Text) { Value = equipmentLocalIds[i] });


//        if (equipmentEquipOrRemove[i] == 1) // Move from armoury to character (Add equipment to character)
//        {
//            sqlBuilder.AppendLine($@"UPDATE t_character SET character_inventory = character_inventory || jsonb_build_object('LocalId', {localIdParam}) WHERE fk_user_id = {userIdParam};");

//            sqlBuilder.AppendLine($@"UPDATE t_armoury SET armoury_inventory = (SELECT jsonb_agg(equipment) FROM jsonb_array_elements(armoury_inventory) AS equipment WHERE equipment->>'LocalId' != {localIdParam}) WHERE fk_user_id = {userIdParam};");
//        }
//        else if (equipmentEquipOrRemove[i] == 0) // Move from character to armoury (Remove equipment from character)
//        {
//            sqlBuilder.AppendLine($@"UPDATE t_character SET character_inventory = (SELECT jsonb_agg(equipment) FROM jsonb_array_elements(character_inventory) AS equipment WHERE equipment->>'LocalId' != {localIdParam}) WHERE fk_user_id = {userIdParam};");

//            sqlBuilder.AppendLine($@"UPDATE t_armoury SET armoury_inventory = armoury_inventory || jsonb_build_object('LocalId', {localIdParam}) WHERE fk_user_id = {userIdParam};");
//        }
//    }
//    return (sqlBuilder.ToString(), parameters.ToArray());
//}

/// <summary>
/// Generates SQL query for updating character or armoury inventory with NpgsqlParameters
/// </summary>
/// <param name="inventoryType">"character" or "armoury"</param>
/// <param name="equipmentAddOrRemove">An array of Add(1) or Remove(0) flags for the equipment</param>
/// <param name="userId">User's custom ID</param>
/// <param name="equipmentLocalIds">Array of local equipment IDs to add/remove</param>
/// <returns>Tuple containing SQL query and associated NpgsqlParameter[]</returns>
//public (string, NpgsqlParameter[]) GenerateInventoryUpdateSQL(string inventoryType, int[] equipmentEquipOrRemove, int userId, int[] equipmentLocalIds)
//{
//    string tableName = inventoryType == "character" ? "t_character" : "t_armoury";
//    string column = inventoryType == "character" ? "character_inventory" : "armoury_inventory";

//    var sqlBuilder = new StringBuilder();
//    var parameters = new List<NpgsqlParameter>();

//    for (int i = 0; i < equipmentLocalIds.Length; i++)
//    {
//        string localIdParam = $"@ServerId{i}";
//        string userIdParam = $"@UserId{i}";

//        parameters.Add(new NpgsqlParameter(localIdParam, equipmentLocalIds[i]));
//        parameters.Add(new NpgsqlParameter(userIdParam, userId));

//        if (equipmentEquipOrRemove[i] == 1) // Add equipment
//        {
//            sqlBuilder.AppendLine($@"UPDATE {tableName} 
//                             SET {column} = {column} || jsonb_build_object('ServerId', {localIdParam})
//                             WHERE fk_user_id = {userIdParam};");
//        }
//        else if (equipmentEquipOrRemove[i] == 0) // Remove equipment
//        {
//            sqlBuilder.AppendLine($@"UPDATE {tableName} 
//                             SET {column} = (SELECT jsonb_agg(equipment) 
//                                             FROM jsonb_array_elements({column}) AS equipment 
//                                             WHERE equipment->>'ServerId' != {localIdParam}) 
//                             WHERE fk_user_id = {userIdParam};");
//        }
//    }

//    return (sqlBuilder.ToString(), parameters.ToArray());
//}
//public async Task<IMapUpdateResponse> UpdateMap(MapUpdatePayload mapUpdatePayload)
//{
//    try
//    {
//        var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(u => u.Type == $"{ClaimTypes.NameIdentifier}").Value;
//        var user = await _userManager.FindByIdAsync(userId);
//        int[] nodeIndexes = mapUpdatePayload.NodeIndexes;
//        int[] nodeTypes = mapUpdatePayload.NodeTypes;
//        if (nodeIndexes.Length == 1)
//        {
//            var sqlGenerationResult = GenerateMapUpdateSQL(nodeIndexes[0], nodeTypes[0], user.CustomUserId);
//            await _dbContext.Database.ExecuteSqlRawAsync(sqlGenerationResult.Item1, sqlGenerationResult.Item2);
//        }
//        else if (nodeIndexes.Length > 1)
//        {
//            var sqlGenerationResult = GenerateMapUpdateSQL(nodeIndexes, nodeTypes, user.CustomUserId);
//            for (int i = 0; i < sqlGenerationResult.Item1.Length; i++)
//            {
//                await _dbContext.Database.ExecuteSqlRawAsync(sqlGenerationResult.Item1[i], sqlGenerationResult.Item2[i]);
//            }
//        }
//        return new MapUpdateResponse()
//        {
//        };
//    }
//    catch (Exception ex)
//    {
//        System.Diagnostics.Debug.WriteLine($"Registration failed: {ex.Message}");
//    }
//    return new MapUpdateResponse()
//    {
//    };
//}

//public static string ResolveEquipmentSQLParameters()
//{

//}

/// 0==authentication, 1==armoury, 2==battleboard, 3==character, 4==kingdom, 5==soupkitchen, 6==treasury
//public static (string, NpgsqlParameter[]) GenerateCharacterEquipmentUpdateSQL(int equipmentEquipOrRemove, int userId, int equipmentLocalId)
//{
//    string sqlQuery = string.Empty;
//    NpgsqlParameter[] parameters = new NpgsqlParameter[5];

//    if (equipmentEquipOrRemove == 1) // Add equipment
//    {//jsonb_build_object('equipmentLocalId', @EquipmentLocalId, 'EquipmentServerId', @EquipmentServerId, 'EquipmentType', @EquipmentType, 'Level', @Level)
//        sqlQuery = @"UPDATE t_character SET character_inventory = character_inventory ||  WHERE fk_user_id = @UserId;";

//        parameters =
//        [
//            new NpgsqlParameter("@EquipmentPiece", equipmentPiece),
//            new NpgsqlParameter("@UserId", userId)
//        ];
//    }
//    else if (equipmentEquipOrRemove == 0) // Remove equipment
//    {
//        sqlQuery = @"UPDATE t_character SET character_inventory = (SELECT jsonb_agg (equipment) FROM jsonb_array_elements(character_inventory) AS equipment WHERE equipment->>'ServerId' != @ServerId::text) WHERE fk_user_id = @UserId;";

//        parameters =
//        [
//            new NpgsqlParameter("@ServerId", localId),
//            new NpgsqlParameter("@UserId", userId)
//        ];
//    }

//    return (sqlQuery, parameters);
//}
//public static (string, NpgsqlParameter[]) GenerateEquipmentInventoryUpdateSQL(int localId, int serverId, int equipmentEquipOrRemove, int equipmentType, int level, int userId)
//{
//    string sqlQuery = string.Empty;
//    NpgsqlParameter[] parameters = new NpgsqlParameter[5];

//    if (equipmentEquipOrRemove == 1) // Add equipment
//    {
//        sqlQuery = @"UPDATE t_character SET character_inventory = character_inventory || jsonb_build_object('ServerId', @ServerId, 'ServerId', @ServerId, 'EquipmentType', @EquipmentType, 'Level', @Level) WHERE fk_user_id = @UserId;";

//        parameters =
//        [
//            new NpgsqlParameter("@ServerId", localId),
//            new NpgsqlParameter("@ServerId", serverId),
//            new NpgsqlParameter("@EquipmentType", equipmentType),
//            new NpgsqlParameter("@Level", level),
//            new NpgsqlParameter("@UserId", userId)
//        ];
//    }
//    else if (equipmentEquipOrRemove == 0) // Remove equipment
//    {
//        sqlQuery = @"UPDATE t_character SET character_inventory = (SELECT jsonb_agg(equipment)FROM jsonb_array_elements(character_inventory) AS equipmentWHERE equipment->>'ServerId' != @ServerId::text) WHERE fk_user_id = @UserId;";

//        parameters =
//        [
//            new NpgsqlParameter("@ServerId", localId),
//            new NpgsqlParameter("@UserId", userId)
//        ];
//    }

//    return (sqlQuery, parameters);
//}
//public static (string[], NpgsqlParameter[][]) GenerateEquipmentInventoryUpdateSQL(int[] equipmentIdNumArray, int[] equipmentAddOrRemoveArray, int userId)
//{
//    var nodeData = MapService.ResolveNodeDataByType(nodeTypeArray);
//    string[] sqlArray = new string[nodeTypeArray.Length];
//    NpgsqlParameter[][] sqlParameterArray = new NpgsqlParameter[nodeTypeArray.Length][];
//    for (int i = 0; i < nodeTypeArray.Length; i++)
//    {
//        sqlArray[i] = @"UPDATE t_kingdom SET kingdom_map = jsonb_set(kingdom_map, ARRAY['Nodes', @NodeIndex::text], jsonb_build_object('NodeCost', @NodeCost,'NodeType', @NodeType,'NodeIndex', @NodeIndex,'NodeLevel', @NodeLevel)::jsonb) WHERE fk_user_id = @UserId;";
//        sqlParameterArray[i] =
//        [
//            new("@NodeIndex", nodeIndexArray[i]),
//            new("@NodeCost", nodeData.Item1[i]),
//            new("@NodeType", nodeTypeArray[i]),
//            new("@NodeLevel", nodeData.Item2[i]),
//            new("@UserId", userId)
//        ];
//    }
//    return (sqlArray, sqlParameterArray);
//}
#endregion