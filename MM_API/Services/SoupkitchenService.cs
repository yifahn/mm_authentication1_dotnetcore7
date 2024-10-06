using MM_API.Database.Postgres.DbSchema;
using System.Text;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MM_API.Database.Postgres;
using System.Security.Claims;
using SharedNetworkFramework.Game.Soupkitchen;

using SharedGameFramework.Game.Character.Attribute;
using SharedGameFramework.Game.Character.Attribute.CharacterLevel;
using SharedGameFramework.Game.Character.Attribute.Constitution;
using SharedGameFramework.Game.Character.Attribute.Defence;
using SharedGameFramework.Game.Character.Attribute.Luck;
using SharedGameFramework.Game.Character.Attribute.Stamina;
using SharedGameFramework.Game.Character.Attribute.Strength;

using SharedGameFramework.Game.Armoury.Equipment;
using SharedGameFramework.Game.Armoury.Equipment.Armour;
using SharedGameFramework.Game.Armoury.Equipment.Armour.Arms;
using SharedGameFramework.Game.Armoury.Equipment.Armour.Hands;
using SharedGameFramework.Game.Armoury.Equipment.Armour.Head;
using SharedGameFramework.Game.Armoury.Equipment.Armour.Legs;
using SharedGameFramework.Game.Armoury.Equipment.Armour.Feet;
using SharedGameFramework.Game.Armoury.Equipment.Armour.Torso;
using SharedGameFramework.Game.Armoury.Equipment.Weapon;
using SharedGameFramework.Game.Armoury.Equipment.Weapon.Axe;
using SharedGameFramework.Game.Armoury.Equipment.Weapon.Spear;
using SharedGameFramework.Game.Armoury.Equipment.Weapon.Staff;
using SharedGameFramework.Game.Armoury.Equipment.Weapon.Sword;
using SharedGameFramework.Game.Armoury.Equipment.Jewellery;
using SharedGameFramework.Game.Armoury.Equipment.Jewellery.Amulet;
using SharedGameFramework.Game.Armoury.Equipment.Jewellery.Ring;
using Npgsql;
using SharedGameFramework.Game.Character;
using SharedGameFramework.Game.Soupkitchen;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Reflection.Emit;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using NpgsqlTypes;
using SharedGameFramework.Game.Armoury;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using static System.Net.Mime.MediaTypeNames;




namespace MM_API.Services
{
    public interface ISoupkitchenService
    {
        public Task<ISoupkitchenClaimResponse> ClaimSoup();
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
        public async Task<ISoupkitchenClaimResponse> ClaimSoup()
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
        public async Task<ISoupkitchenClaimResponse> ClaimSoup()
        {
            try
            {
                var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(u => u.Type == $"{ClaimTypes.NameIdentifier}").Value;
                var user = await _userManager.FindByIdAsync(userId);

                //attribute
                //item
                //int randomClaimable = new Random().Next(0, 2);
                int randomClaimable = 0; //hardcoded for testing

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
                int randomEquipment = new Random().Next(0, 3);

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
                                        DefenceRating = ((randomProperty1 * randomProperty2) / 100) / 20
                                    },
                                    1 => new Feet()
                                    {
                                        ArmourTier = randomProperty1 > 69 ? randomProperty1 > 90 ? 3 : 2 : 1,
                                        Name = randomProperty1 > 69 ? randomProperty1 > 90 ? "Steel Greaves" : "Bronze Boots" : "Leather Sandals",
                                        DefenceRating = ((randomProperty1 * randomProperty2) / 100) / 40
                                    },
                                    2 => new Hands()
                                    {
                                        ArmourTier = randomProperty1 > 69 ? randomProperty1 > 90 ? 3 : 2 : 1,
                                        Name = randomProperty1 > 69 ? randomProperty1 > 90 ? "Steel Gauntlets" : "Bronze Gloves" : "Leather Cuffs",
                                        DefenceRating = ((randomProperty1 * randomProperty2) / 100) / 40
                                    },
                                    3 => new Head()
                                    {
                                        ArmourTier = randomProperty1 > 69 ? randomProperty1 > 90 ? 3 : 2 : 1,
                                        Name = randomProperty1 > 69 ? randomProperty1 > 90 ? "Steel Helm" : "Bronze Cap" : "Leather Hood",
                                        DefenceRating = ((randomProperty1 * randomProperty2) / 100) / 20
                                    },
                                    4 => new Legs()
                                    {
                                        ArmourTier = randomProperty1 > 69 ? randomProperty1 > 90 ? 3 : 2 : 1,
                                        Name = randomProperty1 > 69 ? randomProperty1 > 90 ? "Steel Leggings" : "Bronze Trousers" : "Leather Pants",
                                        DefenceRating = (randomProperty1 * randomProperty2) / 100
                                    },
                                    5 => new Torso()
                                    {
                                        ArmourTier = randomProperty1 > 69 ? randomProperty1 > 90 ? 3 : 2 : 1,
                                        Name = randomProperty1 > 69 ? randomProperty1 > 90 ? "Steel Chestplate" : "Bronze Vest" : "Leather Tunic",
                                        DefenceRating = (randomProperty1 * randomProperty2) / 100
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
                                        DamageRating = (randomProperty1 * randomProperty2) / 100,
                                    },
                                    1 => new Spear()
                                    {
                                        Name = randomProperty1 > 91 ? "Heart Poker" : "Spear",
                                        Unique = randomProperty1 > 91 ? true : false,
                                        DamageRating = (randomProperty1 * randomProperty2) / 100,
                                    },
                                    2 => new Staff()
                                    {
                                        Name = randomProperty1 > 91 ? "Bear" : "Staff",
                                        Unique = randomProperty1 > 91 ? true : false,
                                        DamageRating = (randomProperty1 * randomProperty2) / 100,
                                    },
                                    3 => new Sword()
                                    {
                                        Name = randomProperty1 > 91 ? "Oath Keeper" : "Sword",
                                        Unique = randomProperty1 > 91 ? true : false,
                                        DamageRating = (randomProperty1 * randomProperty2) / 100,
                                    },
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
                var newClaimableJson = JsonConvert.SerializeObject(result); 

                string sqlQuery = string.Empty;
                NpgsqlParameter[] parameters;

                if (result is SharedGameFramework.Game.Character.Attribute.Attribute resultAttribute)
                {
                    string attributeType = resultAttribute.AttributeType;
                    int level = resultAttribute.Level;

                    sqlQuery =  @"UPDATE t_character SET character_sheet = jsonb_set(character_sheet::jsonb,'{Attributes}',(SELECT jsonb_agg(CASE WHEN attr->> 'AttributeType' = @AttributeType THEN jsonb_set(attr, '{Level}', ((attr->> 'Level')::int + @Increment)::text::jsonb) ELSE attr END) FROM jsonb_array_elements(character_sheet->'Attributes') AS attr)) WHERE fk_user_id = @UserId;";

                    parameters =
                    [
                        new NpgsqlParameter("@AttributeType", NpgsqlDbType.Text) { Value = attributeType },
                        new NpgsqlParameter("@Increment", NpgsqlDbType.Integer ) { Value = level },
                        new NpgsqlParameter("@UserId", NpgsqlDbType.Integer ) { Value = user.CustomUserId }
                    ];
                }
                else
                {
                    sqlQuery = @"UPDATE t_armoury SET armoury_inventory = jsonb_insert(armoury_inventory, @EquipmentArray, @NewEquipment::jsonb) WHERE fk_user_id = @UserId";

                    parameters =
                    [
                        new NpgsqlParameter("@NewEquipment", NpgsqlDbType.Jsonb) { Value = newClaimableJson },
                        new NpgsqlParameter("@EquipmentArray", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = new string[] { "Equipment", "-1" } },
                        new NpgsqlParameter("@UserId", NpgsqlDbType.Integer) { Value = user.CustomUserId }
                    ];
                }

                using (var connection = new NpgsqlConnection(_dbContext.Database.GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new NpgsqlCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddRange(parameters);
                        await command.ExecuteNonQueryAsync();
                    }
                }

                SoupkitchenClaimResponse response = new SoupkitchenClaimResponse()
                {
                    ClaimedItem = JsonConvert.SerializeObject(result)
                };
                return response;

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Claim soup failed: {ex.Message}");
            }
            return null;
        }
    }
}
#endregion
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
//                if (result is SharedGameFramework.Game.Character.Attribute.Attribute resultAttribute)
//                {
//                    string attributeType = resultAttribute.AttributeType;
//    int level = resultAttribute.Level;

//    sqlQuery = $@"UPDATE t_character SET character_sheet = jsonb_set(character_sheet::jsonb,'{{Attributes}}',(SELECT jsonb_agg(CASE WHEN attr->>'AttributeType' = @AttributeType THEN jsonb_set(attr, '{{Level}}', (attr->>'Level')::int + @Increment || '::jsonb') ELSE attr END) FROM jsonb_array_elements(character_sheet->'Attributes') AS attr), true ) WHERE character_sheet @> jsonb_build_object('Attributes', jsonb_build_array(jsonb_build_object('AttributeType', @AttributeType)));";
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


//                    // test equipment piece == {""Id"":""Test\"",\""Name\"":""Test"",""Unique"":false,""WeaponType"":""Sword"",""DamageRating"":10,""EquipmentType"":""Weapon""}
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



//SoupkitchenClaimResponse response = new SoupkitchenClaimResponse()
//{
//    ClaimedItem = JsonConvert.SerializeObject(result)
//};
//                    return response;
//                }




//LEARNING IS A HEADACHE ... NOTEPAD STUFF
//(character_sheet, '{Attributes}', (SELECT jsonb_agg(CASE WHEN attr->> 'AttributeType' = @AttributeType THEN jsonb_set(attr, '{Level}', (attr->> 'Level')::int + @Increment || '::jsonb') ELSE attr END) FROM jsonb_array_elements(character_sheet->'Attributes') AS attr)

//"{""Equipment"": [{""Id"": ""b41c554b-a65a-4327-8f48-f5e5304a8bc3"", ""Name"": ""Iron Sword"", ""Unique"": false, ""WeaponType"": ""Sword"", ""DamageRating"": 10, ""EquipmentType"": ""Weapon""}]}"
//"{""Equipment"": [{""Id"": ""b41c554b-a65a-4327-8f48-f5e5304a8bc3"", ""Name"": ""Iron Sword"", ""Unique"": false, ""WeaponType"": ""Sword"", ""DamageRating"": 10, ""EquipmentType"": ""Weapon""}]}"
//"{""Equipment"": [{""Id"": ""b41c554b-a65a-4327-8f48-f5e5304a8bc3"", ""Name"": ""Iron Sword"", ""Unique"": false, ""WeaponType"": ""Sword"", ""DamageRating"": 10, ""EquipmentType"": ""Weapon""}]}"
//"{""Equipment"": [{""Id"": ""866e2bec-818c-426f-b3db-f6d5d6d9e935"", ""Name"": ""Iron Sword"", ""Unique"": false, ""WeaponType"": ""Sword"", ""DamageRating"": 10, ""EquipmentType"": ""Weapon""}]}"

//{"WeaponType":"Spear","DamageRating":1,"EquipmentType":"Weapon","Unique":false,"Name":"Spear","Id":"7f4728fa-29b9-466e-8483-272c95252e61"}
//UPDATE t_armoury SET armoury_inventory = jsonb_set(armoury_inventory::jsonb,'{Equipment}', (armoury_inventory->'Equipment')::jsonb || @NewEquipment::jsonb) WHERE fk_user_id = @UserId;



//UPDATE t_armoury SET armoury_inventory = jsonb_set(armoury_inventory::jsonb,'Equipment', (armoury_inventory->'Equipment')::jsonb || @NewEquipment::jsonb) WHERE fk_user_id = @UserId;
//UPDATE t_character SET character_sheet = jsonb_set(character_sheet,'Attributes',(SELECT jsonb_agg(CASE WHEN attr->> 'AttributeType' = @AttributeType THEN jsonb_set(attr, 'Level', (attr->> 'Level')::int + @Increment || '::jsonb') ELSE attr END) FROM jsonb_array_elements(character_sheet->'Attributes') AS attr), true ) WHERE character_sheet @> jsonb_build_object('Attributes', jsonb_build_array(jsonb_build_object('AttributeType', @AttributeType)));

//"{""Equipment"": [{""Id"": ""866e2bec-818c-426f-b3db-f6d5d6d9e935"", ""Name"": ""Iron Sword"", ""Unique"": false, ""WeaponType"": ""Sword"", ""DamageRating"": 10, ""EquipmentType"": ""Weapon""}]}"

//UPDATE t_armoury SET armoury_inventory = jsonb_set(armoury_inventory::jsonb, '{\"Equipment\"}', (armoury_inventory->'Equipment')::jsonb || @NewEquipment::jsonb) WHERE fk_user_id = @UserId;
//UPDATE t_armoury SET armoury_inventory = jsonb_set(armoury_inventory::jsonb, '{Equipment}', (armoury_inventory->'Equipment')::jsonb || @NewEquipment::jsonb) WHERE fk_user_id = @UserId;


//UPDATE t_armoury SET armoury_inventory = jsonb_set(armoury_inventory, '{Equipment}', armoury_inventory->'Equipment' || @NewEquipment) WHERE fk_user_id = @UserId;


//"{""Equipment"": [{""Id"": ""Test"", ""Name"": ""Test"", ""Unique"": false, ""WeaponType"": ""Sword"", ""DamageRating"": 10, ""EquipmentType"": ""Weapon""}, {""Id"": ""dd41d963-bacc-421e-b14d-82dfb15f767d"", ""Name"": ""Iron Sword"", ""Unique"": false, ""WeaponType"": ""Sword"", ""DamageRating"": 10, ""EquipmentType"": ""Weapon""}]}"

//UPDATE t_armoury SET armoury_inventory = jsonb_insert(armoury_inventory,'{Equipment, -1}', @NewEquipment::jsonb) WHERE fk_user_id = @UserId;

//UPDATE t_armoury SET armoury_inventory = '{}' WHERE fk_user_id = 15;
//UPDATE t_armoury SET armoury_inventory = \'{}\' WHERE fk_user_id = 15;


//UPDATE t_armoury SET armoury_inventory = jsonb_insert(armoury_inventory,'{Equipment, -1}', @NewEquipment::jsonb) WHERE fk_user_id = @UserId;

//"UPDATE t_armoury SET armoury_inventory = jsonb_insert(armoury_inventory,'{Equipment, -1}', @NewEquipment::jsonb) WHERE fk_user_id = @UserId;";


//UPDATE t_armoury SET armoury_inventory = jsonb_insert(armoury_inventory,{Equipment, -1},{\"Id\":\"Test\",\"Name\":\"Test\",\"Unique\":false,\"WeaponType\":\"Sword\",\"DamageRating\":10,\"EquipmentType\":\"Weapon\"}}::jsonb) WHERE fk_user_id = 16;

//UPDATE t_armoury SET armoury_inventory = jsonb_insert(armoury_inventory, @EquipmentArray,{ ""Id"":""Test\"",\""Name\"":""Test"",""Unique"":false,""WeaponType"":""Sword"",""DamageRating"":10,""EquipmentType"":""Weapon""}
//}::jsonb) WHERE fk_user_id = @UserId; ";

//"{""Equipment"": [{""Id"": ""866e2bec-818c-426f-b3db-f6d5d6d9e936"", ""Name"": ""Iron Sword"", ""Unique"": false, ""WeaponType"": ""Sword"", ""DamageRating"": 10, ""EquipmentType"": ""Weapon""}, {""Id"": ""866e2bec-818c-426f-b3db-f6d5d6d9e935"", ""Name"": ""Iron Sword"", ""Unique"": false, ""WeaponType"": ""Sword"", ""DamageRating"": 10, ""EquipmentType"": ""Weapon""}, {""Id"": ""8f488c1d-7802-4d1f-ba3f-2da8600881f7"", ""Name"": ""Staff"", ""Unique"": false, ""WeaponType"": ""Staff"", ""DamageRating"": 7, ""EquipmentType"": ""Weapon""}]}"

//"{""Equipment"": [{""Id"": ""866e2bec-818c-426f-b3db-f6d5d6d9e936"", ""Name"": ""Iron Sword"", ""Unique"": false, ""WeaponType"": ""Sword"", ""DamageRating"": 10, ""EquipmentType"": ""Weapon""}, {""Id"": ""866e2bec-818c-426f-b3db-f6d5d6d9e935"", ""Name"": ""Iron Sword"", ""Unique"": false, ""WeaponType"": ""Sword"", ""DamageRating"": 10, ""EquipmentType"": ""Weapon""}, {""Id"": ""8f488c1d-7802-4d1f-ba3f-2da8600881f7"", ""Name"": ""Staff"", ""Unique"": false, ""WeaponType"": ""Staff"", ""DamageRating"": 7, ""EquipmentType"": ""Weapon""}]}"

//"{""Equipment"": [{""Id"": ""866e2bec-818c-426f-b3db-f6d5d6d9e936"", ""Name"": ""Iron Sword"", ""Unique"": false, ""WeaponType"": ""Sword"", ""DamageRating"": 10, ""EquipmentType"": ""Weapon""}, {""Id"": ""866e2bec-818c-426f-b3db-f6d5d6d9e935"", ""Name"": ""Iron Sword"", ""Unique"": false, ""WeaponType"": ""Sword"", ""DamageRating"": 10, ""EquipmentType"": ""Weapon""}, {""Id"": ""8f488c1d-7802-4d1f-ba3f-2da8600881f7"", ""Name"": ""Staff"", ""Unique"": false, ""WeaponType"": ""Staff"", ""DamageRating"": 7, ""EquipmentType"": ""Weapon""}]}"

//"{""Equipment"": [{""Id"": ""Test"", ""Name"": ""Test"", ""Unique"": false, ""WeaponType"": ""Sword"", ""DamageRating"": 10, ""EquipmentType"": ""Weapon""}, {""Id"": ""Test"", ""Name"": ""Test"", ""Unique"": false, ""WeaponType"": ""Sword"", ""DamageRating"": 10, ""EquipmentType"": ""Weapon""}, {""Id"": ""5e957481-09ff-4a80-b45a-980790536cb2"", ""Name"": ""Iron Sword"", ""Unique"": false, ""WeaponType"": ""Sword"", ""DamageRating"": 10, ""EquipmentType"": ""Weapon""}]}"

