using MM_API.Database.Postgres.DbSchema;
using System.Text;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SharedNetworkFramework.Authentication.Logout;
using System.Security.Claims;
using SharedNetworkFramework.Game.Character;
using SharedNetworkFramework.Game.Kingdom.Map;
using SharedNetworkFramework.Game.Kingdom;
using SharedNetworkFramework.Game.Character.Sheet;
using SharedNetworkFramework.Game.Character.State;
using SharedNetworkFramework.Game.Character.Inventory;
using MM_API.Database.Postgres;
using Npgsql;
using SharedGameFramework.Game.Kingdom.Map;
using SharedGameFramework.Game.Armoury.Equipment;
using SharedGameFramework.Game.Character;
using NpgsqlTypes;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

namespace MM_API.Services
{
    public interface ICharacterService
    {
        public Task<ICharacterLoadResponse> LoadCharacter();
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
        public async Task<ICharacterLoadResponse> LoadCharacter()
        {
            try
            {
                var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(u => u.Type == $"{ClaimTypes.NameIdentifier}").Value;
                var user = await _userManager.FindByIdAsync(userId);

                t_Character character = await _dbContext.t_character.FirstOrDefaultAsync(c => c.fk_user_id == user.CustomUserId);

                return new CharacterLoadResponse
                {
                    CharacterName = character.character_name,
                    CharacterInventory = character.character_inventory,
                    CharacterSheet = character.character_sheet,
                    CharacterState = character.character_state
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Load character failed: {ex.Message}");
            }
            return null;
        }
        public async Task<IInventoryUpdateResponse> UpdateCharacterInventory(InventoryUpdatePayload inventoryUpdatePayload)
        {
            try
            {
                var userId = _httpContextAccessor.HttpContext.User.Claims
                    .FirstOrDefault(u => u.Type == ClaimTypes.NameIdentifier)?.Value;

                var user = await _userManager.FindByIdAsync(userId);
                string[] equipmentIdNums = inventoryUpdatePayload.EquipmentLocalIdNums;
                int equipmentAddOrRemove = inventoryUpdatePayload.EquipmentAddOrRemove;

                t_Character character = await _dbContext.t_character.FirstAsync(c => c.fk_user_id == user.CustomUserId);
                t_Armoury armoury = await _dbContext.t_armoury.FirstAsync(a => a.fk_user_id == user.CustomUserId);
                
                var armoury_inventory2 = JsonConvert.DeserializeObject(armoury.armoury_inventory);

                string armoury_inventory1 = armoury.armoury_inventory.ToString();
                //using (var transaction = await _dbContext.Database.BeginTransactionAsync())
                //{
                //    var sqlResult = GenerateInventoryUpdateSQL(user.CustomUserId, equipmentAddOrRemove, equipmentIdNums);

                //    await _dbContext.Database.ExecuteSqlRawAsync(sqlResult.Item1, sqlResult.Item2);

                //    await transaction.CommitAsync();
                //}


                return new InventoryUpdateResponse
                {
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Update character inventory failed: {ex.Message}");
            }
            return null;
        }
        public async Task<ISheetUpdateResponse> UpdateCharacterSheet(SheetUpdatePayload sheetUpdatePayload)
        {
            try
            {

                return new SheetUpdateResponse
                {

                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Update character sheet failed: {ex.Message}");
            }
            return null;
        }
        public async Task<IStateUpdateResponse> UpdateCharacterState(StateUpdatePayload stateUpdatePayload)
        {
            try
            {

                return new StateUpdateResponse
                {

                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Update character state failed: {ex.Message}");
            }
            return null;
        }

        public (string, NpgsqlParameter[]) GenerateInventoryUpdateSQL(int userId, int equipmentAddOrRemove, string[] equipmentLocalIds)
        {
            var sqlBuilder = new StringBuilder();
            var parameters = new List<NpgsqlParameter>();

            int operationStage = 0;

            for (int ii = 0; ii < equipmentLocalIds.Length; ii++)
            {
                if (operationStage == 0) //stage 0 == acquire equipment
                {
                    if (equipmentAddOrRemove == 0) //remove from character, add to armoury || de-equip character
                    {
                        sqlBuilder.Append(@"UPDATE t_armoury SET armoury_inventory = jsonb_insert(armoury_inventory, @EquipmentArray, @NewEquipment::jsonb) WHERE fk_user_id = @UserId");


                        sqlBuilder.Append(@"UPDATE t_character SET character_inventory =         WHERE fk_user_id = @UserId");
                    }
                    else if (equipmentAddOrRemove == 1) //remove from armoury, add to character || equip character
                    {

                    }
                }
                else if (operationStage == 1)//stage 1 == move equipment
                {
                    if (equipmentAddOrRemove == 0) //remove from character, add to armoury || de-equip character
                    {

                    }
                    else if (equipmentAddOrRemove == 1) //remove from armoury, add to character || equip character
                    {

                    }
                }
            }

            return (sqlBuilder.ToString(), parameters.ToArray());
        }
    }
}
#endregion



//else if (operationStage == 1) //construct SQL to move from armoury to character inventories or vice versa
//{
//    for (int ii = 0; ii < equipmentLocalIds.Length; ii++)
//    {
//        if (equipmentAddOrRemove == 0) //gather equipment piece's properties from character_inventory
//        {
//            sqlBuilder.Append(@"UPDATE t_armoury SET armoury_inventory = jsonb_insert(armoury_inventory, @EquipmentArray, @NewEquipment::jsonb) WHERE fk_user_id = @UserId");



//            parameters.Add(new NpgsqlParameter("@NewEquipment", NpgsqlDbType.Jsonb) { Value = newClaimableJson });
//            parameters.Add(new NpgsqlParameter("@EquipmentArray", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = new string[] { "Equipment", "-1" } });
//            parameters.Add(new NpgsqlParameter("@UserId", NpgsqlDbType.Integer) { Value = userId });

//        }
//        else if (equipmentAddOrRemove == 1) //gather equipment piece's properties from armoury_inventory
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

//    if (equipmentAddOrRemove[i] == 1) // Move from armoury to character (Add equipment to character)
//    {
//        // sqlBuilder.Append
//    }
//    else if (equipmentAddOrRemove[i] == 0) // Move from character to armoury (Remove equipment from character)
//    {
//    }
//}
//public (string, NpgsqlParameter[]) GenerateInventoryUpdateSQL(int userId, int[] equipmentAddOrRemove, string[] equipmentLocalIds)
//{
//    var sqlBuilder = new StringBuilder();
//    var parameters = new List<NpgsqlParameter>();

//    for (int i = 0; i < equipmentLocalIds.Length; i++)
//    {
//        string localIdParam = $"@LocalId{i}";
//        string userIdParam = $"@UserId{i}";

//        parameters.Add(new NpgsqlParameter(localIdParam, NpgsqlDbType.Text) { Value = equipmentLocalIds[i] });
//        parameters.Add(new NpgsqlParameter(userIdParam, NpgsqlDbType.Integer) { Value = userId });

//        if (equipmentAddOrRemove[i] == 1) // Move from armoury to character (Add equipment to character)
//        {
//            sqlBuilder.AppendLine($@"WITH equipment AS (SELECT equipment FROM jsonb_array_elements((SELECT armoury_inventory->'Equipment' FROM t_armoury WHERE fk_user_id = {userIdParam})) AS equipment WHERE equipment->>'LocalId' = {localIdParam}) UPDATE t_character SET character_inventory = character_inventory || equipment FROM equipment WHERE fk_user_id = {userIdParam};");
//            sqlBuilder.AppendLine($@"UPDATE t_armoury SET armoury_inventory = (SELECT jsonb_agg(equipment) FROM jsonb_array_elements(armoury_inventory->'Equipment') AS equipment WHERE equipment->>'LocalId' != {localIdParam} AND fk_user_id = {userIdParam}) WHERE fk_user_id = {userIdParam};");
//        }
//        else if (equipmentAddOrRemove[i] == 0) // Move from character to armoury (Remove equipment from character)
//        {
//            sqlBuilder.AppendLine($@"WITH equipment AS (SELECT equipment FROM jsonb_array_elements((SELECT character_inventory FROM t_character WHERE fk_user_id = {userIdParam})) AS equipment WHERE equipment->>'LocalId' = {localIdParam}) UPDATE t_armoury SET armoury_inventory = armoury_inventory || equipment FROM equipment WHERE fk_user_id = {userIdParam};");
//            sqlBuilder.AppendLine($@"UPDATE t_character SET character_inventory = (SELECT jsonb_agg(equipment) FROM jsonb_array_elements(character_inventory) AS equipment WHERE equipment->>'LocalId' != {localIdParam} AND fk_user_id = {userIdParam}) WHERE fk_user_id = {userIdParam};");
//        }
//    }

//    return (sqlBuilder.ToString(), parameters.ToArray());
//}







//public (string, NpgsqlParameter[]) GenerateInventoryUpdateSQL(int userId, int[] equipmentAddOrRemove, int[] equipmentLocalIds)
//{
//    var sqlBuilder = new StringBuilder();
//    var parameters = new List<NpgsqlParameter>();

//    for (int i = 0; i < equipmentLocalIds.Length; i++)
//    {
//        string localIdParam = $"@LocalId{i}";
//        parameters.Add(new NpgsqlParameter(localIdParam, NpgsqlTypes.NpgsqlDbType.Text) { Value = equipmentLocalIds[i] });


//        if (equipmentAddOrRemove[i] == 1) // Move from armoury to character (Add equipment to character)
//        {
//            sqlBuilder.AppendLine($@"UPDATE t_character SET character_inventory = character_inventory || jsonb_build_object('LocalId', {localIdParam}) WHERE fk_user_id = {userIdParam};");

//            sqlBuilder.AppendLine($@"UPDATE t_armoury SET armoury_inventory = (SELECT jsonb_agg(equipment) FROM jsonb_array_elements(armoury_inventory) AS equipment WHERE equipment->>'LocalId' != {localIdParam}) WHERE fk_user_id = {userIdParam};");
//        }
//        else if (equipmentAddOrRemove[i] == 0) // Move from character to armoury (Remove equipment from character)
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
//public (string, NpgsqlParameter[]) GenerateInventoryUpdateSQL(string inventoryType, int[] equipmentAddOrRemove, int userId, int[] equipmentLocalIds)
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

//        if (equipmentAddOrRemove[i] == 1) // Add equipment
//        {
//            sqlBuilder.AppendLine($@"UPDATE {tableName} 
//                             SET {column} = {column} || jsonb_build_object('ServerId', {localIdParam})
//                             WHERE fk_user_id = {userIdParam};");
//        }
//        else if (equipmentAddOrRemove[i] == 0) // Remove equipment
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
//public static (string, NpgsqlParameter[]) GenerateCharacterEquipmentUpdateSQL(int equipmentAddOrRemove, int userId, int equipmentLocalId)
//{
//    string sqlQuery = string.Empty;
//    NpgsqlParameter[] parameters = new NpgsqlParameter[5];

//    if (equipmentAddOrRemove == 1) // Add equipment
//    {//jsonb_build_object('equipmentLocalId', @EquipmentLocalId, 'EquipmentServerId', @EquipmentServerId, 'EquipmentType', @EquipmentType, 'Level', @Level)
//        sqlQuery = @"UPDATE t_character SET character_inventory = character_inventory ||  WHERE fk_user_id = @UserId;";

//        parameters =
//        [
//            new NpgsqlParameter("@EquipmentPiece", equipmentPiece),
//            new NpgsqlParameter("@UserId", userId)
//        ];
//    }
//    else if (equipmentAddOrRemove == 0) // Remove equipment
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
//public static (string, NpgsqlParameter[]) GenerateEquipmentInventoryUpdateSQL(int localId, int serverId, int equipmentAddOrRemove, int equipmentType, int level, int userId)
//{
//    string sqlQuery = string.Empty;
//    NpgsqlParameter[] parameters = new NpgsqlParameter[5];

//    if (equipmentAddOrRemove == 1) // Add equipment
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
//    else if (equipmentAddOrRemove == 0) // Remove equipment
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
