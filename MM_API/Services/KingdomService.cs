using System.Security.Claims;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

using MM_API.Database.Postgres.DbSchema;
using MM_API.Database.Postgres;

using SharedNetworkFramework.Game.Kingdom.Map;

using SharedGameFramework.Game.Kingdom.Map;
using SharedGameFramework.Game.Kingdom.Map.Node;
using SharedGameFramework.Game.Kingdom.Map.Node.Grassland;
using SharedGameFramework.Game.Kingdom.Map.Node.TownCentre;
using SharedGameFramework.Game.Kingdom.Map.Node.House;
using SharedGameFramework.Game.Kingdom.Map.Node.Library;
using SharedGameFramework.Game.Kingdom.Map.Node.Factory;
using SharedGameFramework.Game.Kingdom.Map.Node.Road;
using SharedGameFramework.Game.Kingdom.Map.Node.Blockade;
using SharedGameFramework.Game.Kingdom.Map.Node.MTower;
using SharedGameFramework.Game.Kingdom.Map.Node.Wonder;

using Npgsql;

using System.Reflection.Metadata.Ecma335;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using SharedNetworkFramework.Game.Kingdom;

namespace MM_API.Services
{
    public interface IKingdomService
    {
        public Task<IKingdomLoadResponse> LoadKingdom();
      /*  public Task<IMapLoadResponse> LoadMap();*///deprecate this for loadkingdom - on load, load all components - update individually
        public Task<IMapUpdateResponse> UpdateMap(MapUpdatePayload payload);


    }

    #region Production
    public class KingdomService : IKingdomService
    {
        private readonly MM_DbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public KingdomService(MM_DbContext dbContext, UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<IKingdomLoadResponse> LoadKingdom()
        {
            var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(u => u.Type == $"{ClaimTypes.NameIdentifier}").Value;
            var user = await _userManager.FindByIdAsync(userId);

            t_Kingdom kingdom = await _dbContext.t_kingdom.FirstOrDefaultAsync(m => m.fk_user_id == user.CustomUserId);
            var loadKingdomResponse = new KingdomLoadResponse()
            {
                KingdomName = kingdom.kingdom_name,
                KingdomMap = kingdom.kingdom_map
            };
            return loadKingdomResponse;
        }
        //public async Task<IMapLoadResponse> LoadMap() 
        //{
        //    var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(u => u.Type == $"{ClaimTypes.NameIdentifier}").Value;
        //    var user = await _userManager.FindByIdAsync(userId);

        //    t_Kingdom map = await _dbContext.t_kingdom.FirstOrDefaultAsync(m => m.fk_user_id == user.CustomUserId);
        //    var loadMapResponse = new MapLoadResponse()
        //    {
        //        KingdomMap = map.kingdom_map
        //    };
        //    return loadMapResponse;
        //}
        public async Task<IMapUpdateResponse> UpdateMap(MapUpdatePayload mapUpdatePayload)
        {
            try
            {
                var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(u => u.Type == $"{ClaimTypes.NameIdentifier}").Value;
                var user = await _userManager.FindByIdAsync(userId);
                int[] nodeIndexes = mapUpdatePayload.NodeIndexes;
                int[] nodeTypes = mapUpdatePayload.NodeTypes;
                if (nodeIndexes.Length == 1)
                {
                    var sqlGenerationResult = GenerateMapUpdateSQL(nodeIndexes[0], nodeTypes[0], user.CustomUserId);
                    await _dbContext.Database.ExecuteSqlRawAsync(sqlGenerationResult.Item1, sqlGenerationResult.Item2);
                }
                else if (nodeIndexes.Length > 1)
                {
                    var sqlGenerationResult = GenerateMapUpdateSQL(nodeIndexes, nodeTypes, user.CustomUserId);
                    for (int i = 0; i < sqlGenerationResult.Item1.Length; i++)
                    {
                        await _dbContext.Database.ExecuteSqlRawAsync(sqlGenerationResult.Item1[i], sqlGenerationResult.Item2[i]);
                    }
                }
                return new MapUpdateResponse()
                {
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Registration failed: {ex.Message}");
            }
            return new MapUpdateResponse()
            {
            };
        }
        public static (string, NpgsqlParameter[]) GenerateMapUpdateSQL(int nodeIndex, int nodeType, int userId)
        {
            int[] nodeTypeArray = { nodeType };
            var nodeData = MapService.ResolveNodeDataByType(nodeTypeArray);
            string sqlQuery = @"UPDATE t_kingdom SET kingdom_map = jsonb_set(kingdom_map, ARRAY['Nodes', @NodeIndex::text], jsonb_build_object('NodeCost', @NodeCost,'NodeType', @NodeType,'NodeIndex', @NodeIndex,'NodeLevel', @NodeLevel)::jsonb) WHERE fk_user_id = @UserId;";
            var parameters = new[]
            {
                new NpgsqlParameter("@NodeIndex", nodeIndex),
                new NpgsqlParameter("@NodeCost", nodeData.Item1[0]),
                new NpgsqlParameter("@NodeType", nodeType),
                new NpgsqlParameter("@NodeLevel", nodeData.Item2[0]),
                new NpgsqlParameter("@UserId", userId)
            };
            return (sqlQuery, parameters);
        }
        public static (string[], NpgsqlParameter[][]) GenerateMapUpdateSQL(int[] nodeIndexArray, int[] nodeTypeArray, int userId)
        {
            var nodeData = MapService.ResolveNodeDataByType(nodeTypeArray);
            string[] sqlArray = new string[nodeTypeArray.Length];
            NpgsqlParameter[][] sqlParameterArray = new NpgsqlParameter[nodeTypeArray.Length][];
            for (int i = 0; i < nodeTypeArray.Length; i++)
            {
                sqlArray[i] = @"UPDATE t_kingdom SET kingdom_map = jsonb_set(kingdom_map, ARRAY['Nodes', @NodeIndex::text], jsonb_build_object('NodeCost', @NodeCost,'NodeType', @NodeType,'NodeIndex', @NodeIndex,'NodeLevel', @NodeLevel)::jsonb) WHERE fk_user_id = @UserId;";
                sqlParameterArray[i] =
                [
                    new("@NodeIndex", nodeIndexArray[i]),
                    new("@NodeCost", nodeData.Item1[i]),
                    new("@NodeType", nodeTypeArray[i]),
                    new("@NodeLevel", nodeData.Item2[i]),
                    new("@UserId", userId)
                ];
            }
            return (sqlArray, sqlParameterArray);
        }


    }


    #endregion
    #region Development
    public class TestKingdomService : IKingdomService
    {
        private readonly MM_DbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public TestKingdomService(MM_DbContext dbContext, UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<IKingdomLoadResponse> LoadKingdom()
        {
            var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(u => u.Type == $"{ClaimTypes.NameIdentifier}").Value;
            var user = await _userManager.FindByIdAsync(userId);

            t_Kingdom kingdom = await _dbContext.t_kingdom.FirstOrDefaultAsync(m => m.fk_user_id == user.CustomUserId);
            return new KingdomLoadResponse()
            {
                KingdomName = kingdom.kingdom_name,
                KingdomMap = kingdom.kingdom_map
            };
        }
        //    public async Task<IMapLoadResponse> LoadMap() //deprecate this for loadkingdom - on load, load all components - update individually
        //{
        //    var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(u => u.Type == $"{ClaimTypes.NameIdentifier}").Value;
        //    var user = await _userManager.FindByIdAsync(userId);

        //    t_Kingdom map = await _dbContext.t_kingdom.FirstOrDefaultAsync(m => m.fk_user_id == user.CustomUserId);
        //    var loadMapResponse = new MapLoadResponse()
        //    {
        //        KingdomMap = map.kingdom_map
        //    };
        //    return loadMapResponse;
        //}

        /*
 * "
 * {""Nodes"": [
 * {""NodeCost"": 0, ""NodeType"": 0, ""NodeIndex"": 0, ""NodeLevel"": 0},
 * {""NodeCost"": 0, ""NodeType"": 0, ""NodeIndex"": 1, ""NodeLevel"": 0},
 * {""NodeCost"": 0, ""NodeType"": 0, ""NodeIndex"": 2, ""NodeLevel"": 0},
 * {""NodeCost"": 0, ""NodeType"": 0, ""NodeIndex"": 3, ""NodeLevel"": 0}, 
 * ...
 */
        //NodeType int representations GL==0, TC==1, H==2, L==3, F==4, R==5, B==6, MT==7, W==8
        public async Task<IMapUpdateResponse> UpdateMap(MapUpdatePayload mapUpdatePayload)
        {
            try
            {
                var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(u => u.Type == $"{ClaimTypes.NameIdentifier}").Value;
                var user = await _userManager.FindByIdAsync(userId);
                int[] nodeIndexes = mapUpdatePayload.NodeIndexes;
                int[] nodeTypes = mapUpdatePayload.NodeTypes;
                if (nodeIndexes.Length == 1)
                {
                    var sqlGenerationResult = GenerateMapUpdateSQL(nodeIndexes[0], nodeTypes[0], user.CustomUserId);
                    await _dbContext.Database.ExecuteSqlRawAsync(sqlGenerationResult.Item1, sqlGenerationResult.Item2);
                }
                else if (nodeIndexes.Length > 1)
                {
                    var sqlGenerationResult = GenerateMapUpdateSQL(nodeIndexes, nodeTypes, user.CustomUserId);
                    for (int i = 0; i < sqlGenerationResult.Item1.Length; i++)
                    {
                        await _dbContext.Database.ExecuteSqlRawAsync(sqlGenerationResult.Item1[i], sqlGenerationResult.Item2[i]);
                    }
                }
                return new MapUpdateResponse()
                {
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Registration failed: {ex.Message}");
            }
            return new MapUpdateResponse()
            {
            };
        }
        public static (string, NpgsqlParameter[]) GenerateMapUpdateSQL(int nodeIndex, int nodeType, int userId)
        {
            int[] nodeTypeArray = { nodeType };
            var nodeData = MapService.ResolveNodeDataByType(nodeTypeArray);
            string sqlQuery = @"UPDATE t_kingdom SET kingdom_map = jsonb_set(kingdom_map, ARRAY['Nodes', @NodeIndex::text], jsonb_build_object('NodeCost', @NodeCost,'NodeType', @NodeType,'NodeIndex', @NodeIndex,'NodeLevel', @NodeLevel)::jsonb) WHERE fk_user_id = @UserId;";
            var parameters = new[]
            {
                new NpgsqlParameter("@NodeIndex", nodeIndex),
                new NpgsqlParameter("@NodeCost", nodeData.Item1[0]),
                new NpgsqlParameter("@NodeType", nodeType),
                new NpgsqlParameter("@NodeLevel", nodeData.Item2[0]),
                new NpgsqlParameter("@UserId", userId)
            };
            return (sqlQuery, parameters);
        }
        public static (string[], NpgsqlParameter[][]) GenerateMapUpdateSQL(int[] nodeIndexArray, int[] nodeTypeArray, int userId)
        {
            var nodeData = MapService.ResolveNodeDataByType(nodeTypeArray);
            string[] sqlArray = new string[nodeTypeArray.Length];
            NpgsqlParameter[][] sqlParameterArray = new NpgsqlParameter[nodeTypeArray.Length][];
            for (int i = 0; i < nodeTypeArray.Length; i++)
            {
           
                sqlArray[i] = @"UPDATE t_kingdom SET kingdom_map = jsonb_set(kingdom_map, ARRAY['Nodes', @NodeIndex::text], jsonb_build_object('NodeCost', @NodeCost,'NodeType', @NodeType,'NodeIndex', @NodeIndex,'NodeLevel', @NodeLevel)::jsonb) WHERE fk_user_id = @UserId;";
                sqlParameterArray[i] =
                [
                    new("@NodeIndex", nodeIndexArray[i]),
                    new("@NodeCost", nodeData.Item1[i]),
                    new("@NodeType", nodeTypeArray[i]),
                    new("@NodeLevel", nodeData.Item2[i]),
                    new("@UserId", userId)
                ];
            }
            return (sqlArray, sqlParameterArray);
        }
        //chatgpt puke
        //public class NodeConverter : JsonConverter
        //{
        //    public override bool CanConvert(Type objectType)
        //    {
        //        return typeof(Node).IsAssignableFrom(objectType);
        //    }
        //public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        //{
        //    JObject jsonObject = JObject.Load(reader);
        //    int nodeType = jsonObject["NodeType"].Value<int>();

        //    Node node = nodeType switch
        //    {
        //        0 => new Grassland(),   // GL
        //        1 => new TownCentre(),  // TC
        //        2 => new House(),       // H
        //        3 => new Library(),     // L
        //        4 => new Factory(),     // F
        //        5 => new Road(),        // R
        //        6 => new Blockade(),    // B
        //        7 => new MTower(),      // MT
        //        8 => new Wonder(),      // W

        //    };
        //    serializer.Populate(jsonObject.CreateReader(), node);

        //    return node;
        //}

        //public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        //{
        //    throw new NotImplementedException("Serialization not required in this example.");
        //}
    }
}

#endregion



