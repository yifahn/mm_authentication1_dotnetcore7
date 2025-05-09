﻿using System.Security.Claims;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

using Newtonsoft.Json;

using Npgsql;

using NpgsqlTypes;

using MM_API.Database.Postgres;
using MM_API.Database.Postgres.DbSchema;

using MonoMonarchGameFramework.Game;
using MonoMonarchGameFramework.Game.Character;
using MonoMonarchGameFramework.Game.Character.Attribute;
using MonoMonarchGameFramework.Game.Character.Attribute.CharacterLevel;
using MonoMonarchGameFramework.Game.Character.Attribute.Constitution;
using MonoMonarchGameFramework.Game.Character.Attribute.Defence;
using MonoMonarchGameFramework.Game.Character.Attribute.Luck;
using MonoMonarchGameFramework.Game.Character.Attribute.Stamina;
using MonoMonarchGameFramework.Game.Character.Attribute.Strength;
using MonoMonarchGameFramework.Game.Armoury;
using MonoMonarchGameFramework.Game.Armoury.Equipment;
using MonoMonarchGameFramework.Game.Armoury.Equipment.Armour;
using MonoMonarchGameFramework.Game.Armoury.Equipment.Armour.Arms;
using MonoMonarchGameFramework.Game.Armoury.Equipment.Armour.Hands;
using MonoMonarchGameFramework.Game.Armoury.Equipment.Armour.Head;
using MonoMonarchGameFramework.Game.Armoury.Equipment.Armour.Legs;
using MonoMonarchGameFramework.Game.Armoury.Equipment.Armour.Feet;
using MonoMonarchGameFramework.Game.Armoury.Equipment.Armour.Torso;
using MonoMonarchGameFramework.Game.Armoury.Equipment.Weapon;
using MonoMonarchGameFramework.Game.Armoury.Equipment.Weapon.Axe;
using MonoMonarchGameFramework.Game.Armoury.Equipment.Weapon.Spear;
using MonoMonarchGameFramework.Game.Armoury.Equipment.Weapon.Staff;
using MonoMonarchGameFramework.Game.Armoury.Equipment.Weapon.Sword;
using MonoMonarchGameFramework.Game.Armoury.Equipment.Jewellery;
using MonoMonarchGameFramework.Game.Armoury.Equipment.Jewellery.Amulet;
using MonoMonarchGameFramework.Game.Armoury.Equipment.Jewellery.Ring;
using MonoMonarchGameFramework.Game.Soupkitchen;
using MonoMonarchGameFramework.Game.Kingdom;
using MonoMonarchGameFramework.Game.Kingdom.Nodes;

using MonoMonarchNetworkFramework;
using MonoMonarchNetworkFramework.Game.Character.Inventory;
using MonoMonarchNetworkFramework.Game.Soupkitchen.Claim;
using MonoMonarchNetworkFramework.Game.Soupkitchen;
using MonoMonarchNetworkFramework.Game.Kingdom;

namespace MM_API.Services
{
    public interface ISoupkitchenService
    {
        public Task<IClaimResponse> ClaimSoup();
        public Task<ISoupkitchenLoadResponse> LoadSoupkitchenAsync();
    }
    #region Production
    public class SoupkitchenService : ISoupkitchenService
    {
        private readonly MM_DbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public SoupkitchenService(MM_DbContext dbContext, UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<IClaimResponse> ClaimSoup()
        {
            try
            {

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Load character failed: {ex.Message}");
            }
            return null;
        }
        public async Task<ISoupkitchenLoadResponse> LoadSoupkitchenAsync()
        {
            try
            {

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Load character failed: {ex.Message}");
            }
            return null;
        }
    }
    #endregion
    #region Development
    public class TestSoupkitchenService : ISoupkitchenService
    {
        private readonly MM_DbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public TestSoupkitchenService(MM_DbContext dbContext, UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<ISoupkitchenLoadResponse> LoadSoupkitchenAsync()
        {
            var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(u => u.Type == $"{ClaimTypes.NameIdentifier}").Value;
            var user = await _userManager.FindByIdAsync(userId);

            t_Soupkitchen soupkitchen = await _dbContext.t_soupkitchen.FirstOrDefaultAsync(m => m.fk_user_id == user.CustomUserId);
            return new SoupkitchenLoadResponse()
            {
                SoupkitchenState = soupkitchen.soupkitchen_state,
            };
        }
        public async Task<IClaimResponse> ClaimSoup()
        {
            try
            {
                var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(u => u.Type == $"{ClaimTypes.NameIdentifier}").Value;
                var user = await _userManager.FindByIdAsync(userId);

                //attribute
                //item
                //int randomClaimable = new Random().Next(0, 2);
                int randomClaimable = 1; //hardcoded for testing

                //level
                //constitution
                //defence
                //luck
                //stamina
                //strength
                int randomAttribute = new Random().Next(0, 6);

                //armour
                //weapon
                //jewellery
                //int randomEquipment = new Random().Next(0, 3);
                int randomEquipment = 0;
                //arms
                //feet
                //hands
                //head
                //legs
                //torso
                int randomArmour = new Random().Next(0, 6);

                //axe
                //spear
                //staff
                //sword
                int randomWeapon = new Random().Next(0, 4);

                //amulet
                //ring
                int randomJewellery = new Random().Next(0, 2);

                //double rewardMultiplier = (new Random().NextDouble() + 1);
                int randomProperty1 = new Random().Next(1, 101);
                int randomProperty2 = new Random().Next(1, 101);


                IClaimable result = null;


                switch (randomClaimable)
                {
                    case 0:
                        result = randomAttribute switch
                        {
                            0 => new CharacterLevel() { Level = 1 },
                            1 => new Constitution() { Level = 1 },
                            2 => new Defence() { Level = 1 },
                            3 => new Luck() { Level = 1 },
                            4 => new Stamina() { Level = 1 },
                            5 => new Strength() { Level = 1 }
                        };
                        break;
                    case 1:
                        
                        switch (randomEquipment)
                        {
                            case 0: //armour
                                result = randomArmour switch
                                {
                                    0 => new Arms()
                                    {
                                        ArmourTier = randomProperty1 > 69 ? randomProperty1 > 90 ? 3 : 2 : 1,
                                        Name = randomProperty1 > 69 ? randomProperty1 > 90 ? "Steel Pauldrons" : "Bronze Armguards" : "Leather Bracers",
                                        DefenceRating = ((randomProperty1 * randomProperty2) / 10) / 20
                                    },
                                    1 => new Feet()
                                    {
                                        ArmourTier = randomProperty1 > 69 ? randomProperty1 > 90 ? 3 : 2 : 1,
                                        Name = randomProperty1 > 69 ? randomProperty1 > 90 ? "Steel Greaves" : "Bronze Boots" : "Leather Sandals",
                                        DefenceRating = ((randomProperty1 * randomProperty2) / 10) / 40
                                    },
                                    2 => new Hands()
                                    {
                                        ArmourTier = randomProperty1 > 69 ? randomProperty1 > 90 ? 3 : 2 : 1,
                                        Name = randomProperty1 > 69 ? randomProperty1 > 90 ? "Steel Gauntlets" : "Bronze Gloves" : "Leather Cuffs",
                                        DefenceRating = ((randomProperty1 * randomProperty2) / 10) / 40
                                    },
                                    3 => new Head()
                                    {
                                        ArmourTier = randomProperty1 > 69 ? randomProperty1 > 90 ? 3 : 2 : 1,
                                        Name = randomProperty1 > 69 ? randomProperty1 > 90 ? "Steel Helm" : "Bronze Cap" : "Leather Hood",
                                        DefenceRating = ((randomProperty1 * randomProperty2) / 10) / 20
                                    },
                                    4 => new Legs()
                                    {
                                        ArmourTier = randomProperty1 > 69 ? randomProperty1 > 90 ? 3 : 2 : 1,
                                        Name = randomProperty1 > 69 ? randomProperty1 > 90 ? "Steel Leggings" : "Bronze Trousers" : "Leather Pants",
                                        DefenceRating = (randomProperty1 * randomProperty2) / 10
                                    },
                                    5 => new Torso()
                                    {
                                        ArmourTier = randomProperty1 > 69 ? randomProperty1 > 90 ? 3 : 2 : 1,
                                        Name = randomProperty1 > 69 ? randomProperty1 > 90 ? "Steel Chestplate" : "Bronze Vest" : "Leather Tunic",
                                        DefenceRating = (randomProperty1 * randomProperty2) / 10
                                    }
                                };
                                break;
                            case 1: //weapon
                                result = randomWeapon switch
                                {
                                    0 => new Axe()
                                    {
                                        Name = randomProperty1 > 91 ? "Gore Cleaver" : "Axe",
                                        Unique = randomProperty1 > 91 ? true : false,
                                        DamageRating = (randomProperty1 * randomProperty2) / 8,
                                    },
                                    1 => new Spear()
                                    {
                                        Name = randomProperty1 > 91 ? "Heart Poker" : "Spear",
                                        Unique = randomProperty1 > 91 ? true : false,
                                        DamageRating = (randomProperty1 * randomProperty2) / 8,
                                    },
                                    2 => new Staff()
                                    {
                                        Name = randomProperty1 > 91 ? "Bear" : "Staff",
                                        Unique = randomProperty1 > 91 ? true : false,
                                        DamageRating = (randomProperty1 * randomProperty2) / 8,
                                    },
                                    3 => new Sword()
                                    {
                                        Name = randomProperty1 > 91 ? "Oath Keeper" : "Sword",
                                        Unique = randomProperty1 > 91 ? true : false,
                                        DamageRating = (randomProperty1 * randomProperty2) / 8,
                                    },//DIVIDE ARMOUR REDUCTION BY 6 due to 6 PIECES OF ARMOUR and 1 WEAPON
                                };
                                break;
                            case 2: //jewellery
                                result = randomJewellery switch
                                {
                                    0 => new Amulet()
                                    {
                                        JewelleryTier = randomProperty1 > 69 ? randomProperty1 > 90 ? 3 : 2 : 1,
                                        Name = randomProperty1 > 69 ? randomProperty1 > 90 ? "Dragonstone Necklass" : "Gold Necklass" : "Bone Necklass",
                                        ConstitutionBoon = randomProperty1 > 69 ? randomProperty1 > 90 ? (int)(randomProperty2 * 0.5) : (int)(randomProperty2 * 0.3) : (int)(randomProperty2 * 0.1),
                                        DefenceBoon = randomProperty1 > 69 ? randomProperty1 > 90 ? (int)(randomProperty2 * 0.5) : (int)(randomProperty2 * 0.3) : (int)(randomProperty2 * 0.1),
                                        StaminaBoon = randomProperty1 > 69 ? randomProperty1 > 90 ? (int)(randomProperty2 * 0.5) : (int)(randomProperty2 * 0.3) : (int)(randomProperty2 * 0.1),
                                        LuckBoon = randomProperty1 > 69 ? randomProperty1 > 90 ? (int)(randomProperty2 * 0.5) : (int)(randomProperty2 * 0.3) : (int)(randomProperty2 * 0.1),
                                        StrengthBoon = randomProperty1 > 69 ? randomProperty1 > 90 ? (int)(randomProperty2 * 0.5) : (int)(randomProperty2 * 0.3) : (int)(randomProperty2 * 0.1)

                                    },
                                    1 => new Ring()
                                    {
                                        JewelleryTier = randomProperty1 > 69 ? randomProperty1 > 90 ? 3 : 2 : 1,
                                        Name = randomProperty1 > 69 ? randomProperty1 > 90 ? "Dragonstone Ring" : "Gold Ring" : "Bone Ring",
                                        ConstitutionBoon = randomProperty1 > 69 ? randomProperty1 > 90 ? (int)(randomProperty2 * 0.5) : (int)(randomProperty2 * 0.3) : (int)(randomProperty2 * 0.1),
                                        DefenceBoon = randomProperty1 > 69 ? randomProperty1 > 90 ? (int)(randomProperty2 * 0.5) : (int)(randomProperty2 * 0.3) : (int)(randomProperty2 * 0.1),
                                        StaminaBoon = randomProperty1 > 69 ? randomProperty1 > 90 ? (int)(randomProperty2 * 0.5) : (int)(randomProperty2 * 0.3) : (int)(randomProperty2 * 0.1),
                                        LuckBoon = randomProperty1 > 69 ? randomProperty1 > 90 ? (int)(randomProperty2 * 0.5) : (int)(randomProperty2 * 0.3) : (int)(randomProperty2 * 0.1),
                                        StrengthBoon = randomProperty1 > 69 ? randomProperty1 > 90 ? (int)(randomProperty2 * 0.5) : (int)(randomProperty2 * 0.3) : (int)(randomProperty2 * 0.1)
                                    }
                                };
                                break;
                        }
                        break;
                }
                t_Soupkitchen soupkitchen = null;
                t_Character character = null;
                t_Armoury armoury = null;

                SoupkitchenState soupKitchenState = null;
                EquipmentInventory armouryInventory = null;
                CharacterSheet characterSheet = null;

                JsonSerializer serialiser = new JsonSerializer();

                soupkitchen = await _dbContext.t_soupkitchen.FirstAsync(u => u.fk_user_id == user.CustomUserId);
                using (StringReader sr = new StringReader(soupkitchen.soupkitchen_state))
                {
                    using (JsonReader reader = new JsonTextReader(sr)) soupKitchenState = serialiser.Deserialize<SoupkitchenState>(reader); 
                }

                serialiser.Converters.Add(new DeserialisationSupport());
                if (result is IEquipable)
                {
                    armoury = await _dbContext.t_armoury.FirstAsync(u => u.fk_user_id == user.CustomUserId);
                    armouryInventory = new EquipmentInventory();
                    if (result is BaseWeapon)
                    {
                        using (StringReader sr = new StringReader(armoury.armoury_weapons))
                        {
                            using (JsonReader reader = new JsonTextReader(sr)) armouryInventory.WeaponList = serialiser.Deserialize<BaseWeapon[]>(reader).ToList(); armouryInventory.WeaponList.Add((BaseWeapon)result);
                        }
                    }
                    else if (result is BaseArmour)
                    {
                        using (StringReader sr = new StringReader(armoury.armoury_armour))
                        {
                            using (JsonReader reader = new JsonTextReader(sr)) armouryInventory.ArmourList = serialiser.Deserialize<BaseArmour[]>(reader).ToList(); armouryInventory.ArmourList.Add((BaseArmour)result);
                        }
                    }
                    else if (result is BaseJewellery)
                    {
                        using (StringReader sr = new StringReader(armoury.armoury_jewellery))
                        {
                            using (JsonReader reader = new JsonTextReader(sr)) armouryInventory.JewelleryList = serialiser.Deserialize<BaseJewellery[]>(reader).ToList(); armouryInventory.JewelleryList.Add((BaseJewellery)result);
                        }
                    }
                }
                else if (result is BaseAttribute)
                {
                    character = await _dbContext.t_character.FirstAsync(u => u.fk_user_id == user.CustomUserId);
                    characterSheet = new CharacterSheet();

                    using (StringReader sr = new StringReader(character.character_attributes))
                    {
                        using (JsonReader reader = new JsonTextReader(sr)) characterSheet.AttributeList = serialiser.Deserialize<BaseAttribute[]>(reader).ToList(); characterSheet.AttributeList.Find(e => e.GetType() == result.GetType()).Level += (result as BaseAttribute).Level;
                    }
                }
                serialiser.Converters.Clear();

                DateTimeOffset currentTickAsDateTime = GameUtilities.GetCurrentTickAsDateTime();
                var updateStateResult = soupKitchenState.UpdateStateOnClaim(currentTickAsDateTime, soupkitchen.soupkitchen_updated_at_datetime.AddSeconds((5 * soupKitchenState.TotalNumCooldowns) + 5));

                using (var transaction = await _dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        string serialisedResult = string.Empty;
                        if (result is IEquipable)
                        {
                            if (result is BaseWeapon)
                            {
                                using (StringWriter sw = new StringWriter())
                                {
                                    using (JsonWriter writer = new JsonTextWriter(sw))
                                    {
                                        serialiser.Serialize(writer, armouryInventory.WeaponList);
                                        armoury.armoury_weapons = sw.ToString();
                                        sw.GetStringBuilder().Clear();
                                    }
                                }
                            }
                            else if (result is BaseArmour)
                            {
                                using (StringWriter sw = new StringWriter())
                                {
                                    using (JsonWriter writer = new JsonTextWriter(sw))
                                    {
                                        serialiser.Serialize(writer, armouryInventory.ArmourList);
                                        armoury.armoury_armour = sw.ToString();
                                        sw.GetStringBuilder().Clear();
                                    }
                                }
                            }
                            else if (result is BaseJewellery)
                            {
                                using (StringWriter sw = new StringWriter())
                                {
                                    using (JsonWriter writer = new JsonTextWriter(sw))
                                    {
                                        serialiser.Serialize(writer, armouryInventory.JewelleryList);
                                        armoury.armoury_jewellery = sw.ToString();
                                        sw.GetStringBuilder().Clear();
                                    }
                                }
                            }
                        }
                        else if (result is BaseAttribute)
                        {
                            using (StringWriter sw = new StringWriter())
                            {
                                using (JsonWriter writer = new JsonTextWriter(sw))
                                {
                                    serialiser.Serialize(writer, characterSheet.AttributeList);
                                    character.character_attributes = sw.ToString();
                                    sw.GetStringBuilder().Clear();
                                }
                            }
                        }


                        if (updateStateResult.Item2 == -1)
                            return new ErrorResponse("ClaimsCache < 0");

                        using (StringWriter sw = new StringWriter())
                        {
                            using (JsonWriter writer = new JsonTextWriter(sw))
                            {
                                serialiser.Serialize(writer, soupKitchenState);
                                soupkitchen.soupkitchen_state = sw.ToString();
                                sw.GetStringBuilder().Clear();
                            }
                        }
                        soupkitchen.soupkitchen_updated_at_as_gametick += (int)(currentTickAsDateTime - soupkitchen.soupkitchen_updated_at_datetime).TotalSeconds / 5;
                        soupkitchen.soupkitchen_updated_at_datetime = currentTickAsDateTime;
                        await _dbContext.SaveChangesAsync();
                        transaction.Commit();

                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        return new ErrorResponse("Transaction failed, rolling back");
                    }

                    return new ClaimResponse()
                    {
                        ClaimedItem = JsonConvert.SerializeObject(result),
                        ClaimCooldownExpiryDateTime = updateStateResult.Item1,
                        ClaimCooldownExpiryAsTick = updateStateResult.Item2,
                        

                    };
                }
            }
            catch (Exception ex)
            {
                return new ErrorResponse("Claim soup failed");
            }
        }
    }
}
#endregion

//string sqlQuery = string.Empty;
//NpgsqlParameter[] parameters;

//if (result is SharedGameFramework.Game.Character.Attribute.BaseAttribute resultAttribute)
//{
//    string attributeType = resultAttribute.AttributeType;
//    int level = resultAttribute.Level;

//    sqlQuery =  @"UPDATE t_character SET character_attributes = jsonb_set(character_attributes::jsonb,'{AttributeArray}',(SELECT jsonb_agg(CASE WHEN attr->> 'AttributeType' = @AttributeType THEN jsonb_set(attr, '{Level}', ((attr->> 'Level')::int + @Increment)::text::jsonb) ELSE attr END) FROM jsonb_array_elements(character_attributes->'AttributeArray') AS attr)) WHERE fk_user_id = @UserId;";

//    parameters =
//    [
//        new NpgsqlParameter("@AttributeType", NpgsqlDbType.Text) { Value = attributeType },
//        new NpgsqlParameter("@Increment", NpgsqlDbType.Integer ) { Value = level },
//        new NpgsqlParameter("@UserId", NpgsqlDbType.Integer ) { Value = user.CustomUserId }
//    ];
//}
//else
//{
//    sqlQuery = @"UPDATE t_armoury SET armoury_inventory = jsonb_insert(armoury_inventory, @EquipmentArray, @NewEquipment::jsonb) WHERE fk_user_id = @UserId";

//    parameters =
//    [
//        new NpgsqlParameter("@NewEquipment", NpgsqlDbType.Jsonb) { Value = newClaimableJson },
//        new NpgsqlParameter("@EquipmentArray", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = new string[] { "Equipment", "-1" } },
//        new NpgsqlParameter("@UserId", NpgsqlDbType.Integer) { Value = user.CustomUserId }
//    ];
//}

//using (var connection = new NpgsqlConnection(_dbContext.Database.GetConnectionString()))
//{
//    await connection.OpenAsync();
//    using (var command = new NpgsqlCommand(sqlQuery, connection))
//    {
//        command.Parameters.AddRange(parameters);
//        await command.ExecuteNonQueryAsync();
//    }
//}


//ommitting transaction as only one command is executed, not a series of commands. also this method of transaction is for efcore, not direct npgsql commands
//
//using (var transaction = await _dbContext.Database.BeginTransactionAsync()) 
//{
//using (var connection = new NpgsqlConnection(_dbContext.Database.GetConnectionString()))
//{
//    await connection.OpenAsync();
//    using (var command = new NpgsqlCommand(sqlQuery, connection))
//    {
//        command.Parameters.AddRange(parameters);
//        await command.ExecuteNonQueryAsync();
//    }
//}
//    await transaction.CommitAsync();
//}




// LEARNING IS A HEADACHE...
//    string sqlQuery = string.Empty;
//    NpgsqlParameter[] parameters;
//                if (result is SharedGameFramework.Game.Character.BaseAttribute.BaseAttribute resultAttribute)
//                {
//                    string attributeType = resultAttribute.AttributeType;
//    int level = resultAttribute.Level;

//    sqlQuery = $@"UPDATE t_character SET character_attributes = jsonb_set(character_attributes::jsonb,'{{AttributeArray}}',(SELECT jsonb_agg(CASE WHEN attr->>'AttributeType' = @AttributeType THEN jsonb_set(attr, '{{Level}}', (attr->>'Level')::int + @Increment || '::jsonb') ELSE attr END) FROM jsonb_array_elements(character_attributes->'AttributeArray') AS attr), true ) WHERE character_attributes @> jsonb_build_object('AttributeArray', jsonb_build_array(jsonb_build_object('AttributeType', @AttributeType)));";
//                    parameters =
//                    [
//                        new NpgsqlParameter("@AttributeType", attributeType),
//                        new NpgsqlParameter("@Increment", level),
//                        new NpgsqlParameter("@UserId", user.CustomUserId)
//                    ];
//                }
//                else
//                {
//                    var newClaimableJson = JsonConvert.SerializeObject(result); //(resultEquipment);
//                                                                                //sqlQuery = "UPDATE t_armoury SET armoury_inventory = jsonb_insert(armoury_inventory,'{Equipment, -1}', @NewEquipment::jsonb) WHERE fk_user_id = @UserId;";

//// sqlQuery = @"UPDATE t_armoury SET armoury_inventory = jsonb_insert(armoury_inventory, @EquipmentArray,@NewEquipment) WHERE fk_user_id = @UserId;";

//// sqlQuery = "UPDATE t_armoury SET armoury_inventory = jsonb_set(armoury_inventory,@EquipmentArray,armoury_inventory->'Equipment', '[]'::jsonb || @NewEquipment::jsonb)WHERE fk_user_id = @UserId;";
//// sqlQuery = "UPDATE t_armoury SET armoury_inventory = jsonb_set(armoury_inventory, @EquipmentArray, '[]'::jsonb || @NewEquipment) WHERE fk_user_id = @UserId;";

//sqlQuery = "UPDATE t_armoury SET armoury_inventory = jsonb_insert(armoury_inventory, @EquipmentArray, @NewEquipment) WHERE fk_user_id = @UserId;";

//                    //sqlQuery = @"UPDATE t_armoury SET armoury_inventory = jsonb_set(armoury_inventory, @EquipmentArray, (SELECT jsonb_agg(elem) FROM (SELECT elem FROM jsonb_array_elements(armoury_inventory->'Equipment') AS elem UNION ALL SELECT @NewEquipment::jsonb) AS combined), true) WHERE fk_user_id = @UserId;";


//                    // test equipment piece == {""ServerId"":""Test\"",\""Name\"":""Test"",""Unique"":false,""WeaponType"":""Sword"",""DamageRating"":10,""EquipmentType"":""BaseWeapon""}
//                    // it seem cannot run sql query with " { " or " } " characters, unable to escape with double {{, }} , issue remains, it seems sqlparameters work however

//                    //sqlQuery = $@"UPDATE t_armoury SET armoury_inventory = '{{}}'::jsonb WHERE fk_user_id = 16;";
//                    //sqlQuery = $@"UPDATE t_armoury SET armoury_inventory::jsonb = 'Equipment' WHERE fk_user_id = 16;";
//                    //sqlQuery = "SELECT * FROM t_armoury WHERE fk_user_id = 16;";


//                    parameters =
//                    [
//                        //new NpgsqlParameter("@NewEquipment", newClaimableJson),
//                        //new NpgsqlParameter("@EquipmentArray", NpgsqlDbType.Jsonb) {Value = "'{Equipment, [-1]}'" },
//                        //new NpgsqlParameter("@UserId", user.CustomUserId)

////new NpgsqlParameter("@NewEquipment", NpgsqlDbType.Jsonb) { Value = newClaimableJson }, // Ensure it's valid JSON
////new NpgsqlParameter("@EquipmentArray", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = new string[] { "'{Equipment}'", "-1" } }, // As a text array
////new NpgsqlParameter("@UserId", NpgsqlDbType.Integer) { Value = user.CustomUserId }

////new NpgsqlParameter("@NewEquipment", NpgsqlDbType.Jsonb) { Value = newClaimableJson },
////new NpgsqlParameter("@EquipmentArray", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = new string[] { "Equipment" } },
////new NpgsqlParameter("@UserId", NpgsqlDbType.Integer) { Value = user.CustomUserId }
////new NpgsqlParameter("@NewEquipment", NpgsqlDbType.Jsonb) { Value = newClaimableJson },
////new NpgsqlParameter("@EquipmentArray", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = new string[] { "{Equipment}" } },  // Assuming '0' indicates insertion after the last item in the array
////new NpgsqlParameter("@UserId", NpgsqlDbType.Integer) { Value = user.CustomUserId }

////new NpgsqlParameter("@NewEquipment", NpgsqlDbType.Jsonb) { Value = newClaimableJson },
////new NpgsqlParameter("@EquipmentArray", NpgsqlDbType.Text) { Value = "{Equipment}" },
////new NpgsqlParameter("@UserId", NpgsqlDbType.Integer) { Value = user.CustomUserId }

//                         new NpgsqlParameter("@NewEquipment", NpgsqlDbType.Jsonb) { Value = newClaimableJson }, // Ensure this is valid JSONB
//                         new NpgsqlParameter("@EquipmentArray", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = new string[] { "Equipment", "0" } }, // Path as a text array
//                         new NpgsqlParameter("@UserId", NpgsqlDbType.Integer) { Value = user.CustomUserId } // User ID


//                         ];
//                    var resultSQL = await _dbContext.Database.ExecuteSqlRawAsync(sqlQuery, parameters);//, parameters



//ClaimResponse response = new ClaimResponse()
//{
//    ClaimedItem = JsonConvert.SerializeObject(result)
//};
//                    return response;
//                }




//LEARNING IS A HEADACHE ... NOTEPAD STUFF
//(character_attributes, '{AttributeArray}', (SELECT jsonb_agg(CASE WHEN attr->> 'AttributeType' = @AttributeType THEN jsonb_set(attr, '{Level}', (attr->> 'Level')::int + @Increment || '::jsonb') ELSE attr END) FROM jsonb_array_elements(character_attributes->'AttributeArray') AS attr)

//"{""Equipment"": [{""ServerId"": ""b41c554b-a65a-4327-8f48-f5e5304a8bc3"", ""Name"": ""Iron Sword"", ""Unique"": false, ""WeaponType"": ""Sword"", ""DamageRating"": 10, ""EquipmentType"": ""BaseWeapon""}]}"
//"{""Equipment"": [{""ServerId"": ""b41c554b-a65a-4327-8f48-f5e5304a8bc3"", ""Name"": ""Iron Sword"", ""Unique"": false, ""WeaponType"": ""Sword"", ""DamageRating"": 10, ""EquipmentType"": ""BaseWeapon""}]}"
//"{""Equipment"": [{""ServerId"": ""b41c554b-a65a-4327-8f48-f5e5304a8bc3"", ""Name"": ""Iron Sword"", ""Unique"": false, ""WeaponType"": ""Sword"", ""DamageRating"": 10, ""EquipmentType"": ""BaseWeapon""}]}"
//"{""Equipment"": [{""ServerId"": ""866e2bec-818c-426f-b3db-f6d5d6d9e935"", ""Name"": ""Iron Sword"", ""Unique"": false, ""WeaponType"": ""Sword"", ""DamageRating"": 10, ""EquipmentType"": ""BaseWeapon""}]}"

//{"WeaponType":"Spear","DamageRating":1,"EquipmentType":"BaseWeapon","Unique":false,"Name":"Spear","ServerId":"7f4728fa-29b9-466e-8483-272c95252e61"}
//UPDATE t_armoury SET armoury_inventory = jsonb_set(armoury_inventory::jsonb,'{Equipment}', (armoury_inventory->'Equipment')::jsonb || @NewEquipment::jsonb) WHERE fk_user_id = @UserId;



//UPDATE t_armoury SET armoury_inventory = jsonb_set(armoury_inventory::jsonb,'Equipment', (armoury_inventory->'Equipment')::jsonb || @NewEquipment::jsonb) WHERE fk_user_id = @UserId;
//UPDATE t_character SET character_attributes = jsonb_set(character_attributes,'AttributeArray',(SELECT jsonb_agg(CASE WHEN attr->> 'AttributeType' = @AttributeType THEN jsonb_set(attr, 'Level', (attr->> 'Level')::int + @Increment || '::jsonb') ELSE attr END) FROM jsonb_array_elements(character_attributes->'AttributeArray') AS attr), true ) WHERE character_attributes @> jsonb_build_object('AttributeArray', jsonb_build_array(jsonb_build_object('AttributeType', @AttributeType)));

//"{""Equipment"": [{""ServerId"": ""866e2bec-818c-426f-b3db-f6d5d6d9e935"", ""Name"": ""Iron Sword"", ""Unique"": false, ""WeaponType"": ""Sword"", ""DamageRating"": 10, ""EquipmentType"": ""BaseWeapon""}]}"

//UPDATE t_armoury SET armoury_inventory = jsonb_set(armoury_inventory::jsonb, '{\"Equipment\"}', (armoury_inventory->'Equipment')::jsonb || @NewEquipment::jsonb) WHERE fk_user_id = @UserId;
//UPDATE t_armoury SET armoury_inventory = jsonb_set(armoury_inventory::jsonb, '{Equipment}', (armoury_inventory->'Equipment')::jsonb || @NewEquipment::jsonb) WHERE fk_user_id = @UserId;


//UPDATE t_armoury SET armoury_inventory = jsonb_set(armoury_inventory, '{Equipment}', armoury_inventory->'Equipment' || @NewEquipment) WHERE fk_user_id = @UserId;


//"{""Equipment"": [{""ServerId"": ""Test"", ""Name"": ""Test"", ""Unique"": false, ""WeaponType"": ""Sword"", ""DamageRating"": 10, ""EquipmentType"": ""BaseWeapon""}, {""ServerId"": ""dd41d963-bacc-421e-b14d-82dfb15f767d"", ""Name"": ""Iron Sword"", ""Unique"": false, ""WeaponType"": ""Sword"", ""DamageRating"": 10, ""EquipmentType"": ""BaseWeapon""}]}"

//UPDATE t_armoury SET armoury_inventory = jsonb_insert(armoury_inventory,'{Equipment, -1}', @NewEquipment::jsonb) WHERE fk_user_id = @UserId;

//UPDATE t_armoury SET armoury_inventory = '{}' WHERE fk_user_id = 15;
//UPDATE t_armoury SET armoury_inventory = \'{}\' WHERE fk_user_id = 15;


//UPDATE t_armoury SET armoury_inventory = jsonb_insert(armoury_inventory,'{Equipment, -1}', @NewEquipment::jsonb) WHERE fk_user_id = @UserId;

//"UPDATE t_armoury SET armoury_inventory = jsonb_insert(armoury_inventory,'{Equipment, -1}', @NewEquipment::jsonb) WHERE fk_user_id = @UserId;";


//UPDATE t_armoury SET armoury_inventory = jsonb_insert(armoury_inventory,{Equipment, -1},{\"ServerId\":\"Test\",\"Name\":\"Test\",\"Unique\":false,\"WeaponType\":\"Sword\",\"DamageRating\":10,\"EquipmentType\":\"BaseWeapon\"}}::jsonb) WHERE fk_user_id = 16;

//UPDATE t_armoury SET armoury_inventory = jsonb_insert(armoury_inventory, @EquipmentArray,{ ""ServerId"":""Test\"",\""Name\"":""Test"",""Unique"":false,""WeaponType"":""Sword"",""DamageRating"":10,""EquipmentType"":""BaseWeapon""}
//}::jsonb) WHERE fk_user_id = @UserId; ";

//"{""Equipment"": [{""ServerId"": ""866e2bec-818c-426f-b3db-f6d5d6d9e936"", ""Name"": ""Iron Sword"", ""Unique"": false, ""WeaponType"": ""Sword"", ""DamageRating"": 10, ""EquipmentType"": ""BaseWeapon""}, {""ServerId"": ""866e2bec-818c-426f-b3db-f6d5d6d9e935"", ""Name"": ""Iron Sword"", ""Unique"": false, ""WeaponType"": ""Sword"", ""DamageRating"": 10, ""EquipmentType"": ""BaseWeapon""}, {""ServerId"": ""8f488c1d-7802-4d1f-ba3f-2da8600881f7"", ""Name"": ""Staff"", ""Unique"": false, ""WeaponType"": ""Staff"", ""DamageRating"": 7, ""EquipmentType"": ""BaseWeapon""}]}"

//"{""Equipment"": [{""ServerId"": ""866e2bec-818c-426f-b3db-f6d5d6d9e936"", ""Name"": ""Iron Sword"", ""Unique"": false, ""WeaponType"": ""Sword"", ""DamageRating"": 10, ""EquipmentType"": ""BaseWeapon""}, {""ServerId"": ""866e2bec-818c-426f-b3db-f6d5d6d9e935"", ""Name"": ""Iron Sword"", ""Unique"": false, ""WeaponType"": ""Sword"", ""DamageRating"": 10, ""EquipmentType"": ""BaseWeapon""}, {""ServerId"": ""8f488c1d-7802-4d1f-ba3f-2da8600881f7"", ""Name"": ""Staff"", ""Unique"": false, ""WeaponType"": ""Staff"", ""DamageRating"": 7, ""EquipmentType"": ""BaseWeapon""}]}"

//"{""Equipment"": [{""ServerId"": ""866e2bec-818c-426f-b3db-f6d5d6d9e936"", ""Name"": ""Iron Sword"", ""Unique"": false, ""WeaponType"": ""Sword"", ""DamageRating"": 10, ""EquipmentType"": ""BaseWeapon""}, {""ServerId"": ""866e2bec-818c-426f-b3db-f6d5d6d9e935"", ""Name"": ""Iron Sword"", ""Unique"": false, ""WeaponType"": ""Sword"", ""DamageRating"": 10, ""EquipmentType"": ""BaseWeapon""}, {""ServerId"": ""8f488c1d-7802-4d1f-ba3f-2da8600881f7"", ""Name"": ""Staff"", ""Unique"": false, ""WeaponType"": ""Staff"", ""DamageRating"": 7, ""EquipmentType"": ""BaseWeapon""}]}"

//"{""Equipment"": [{""ServerId"": ""Test"", ""Name"": ""Test"", ""Unique"": false, ""WeaponType"": ""Sword"", ""DamageRating"": 10, ""EquipmentType"": ""BaseWeapon""}, {""ServerId"": ""Test"", ""Name"": ""Test"", ""Unique"": false, ""WeaponType"": ""Sword"", ""DamageRating"": 10, ""EquipmentType"": ""BaseWeapon""}, {""ServerId"": ""5e957481-09ff-4a80-b45a-980790536cb2"", ""Name"": ""Iron Sword"", ""Unique"": false, ""WeaponType"": ""Sword"", ""DamageRating"": 10, ""EquipmentType"": ""BaseWeapon""}]}"

