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
using SharedGameFramework.Game.Kingdom.Map.BaseNode;
using SharedGameFramework.Game.Kingdom.Map.BaseNode.Grassland;
using SharedGameFramework.Game.Kingdom.Map.BaseNode.TownCentre;
using SharedGameFramework.Game.Kingdom.Map.BaseNode.House;
using SharedGameFramework.Game.Kingdom.Map.BaseNode.Library;
using SharedGameFramework.Game.Kingdom.Map.BaseNode.Factory;
using SharedGameFramework.Game.Kingdom.Map.BaseNode.Road;
using SharedGameFramework.Game.Kingdom.Map.BaseNode.Blockade;
using SharedGameFramework.Game.Kingdom.Map.BaseNode.MTower;
using SharedGameFramework.Game.Kingdom.Map.BaseNode.Wonder;

using Npgsql;

using System.Reflection.Metadata.Ecma335;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using SharedNetworkFramework.Game.Kingdom;
using SharedGameFramework.Game.Armoury.Equipment;
using SharedGameFramework.Game;
using System.Xml.Linq;
using System.Diagnostics;

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
                // KingdomMap = kingdom.kingdom_map
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
                KingdomMap = kingdom.kingdom_map,

                
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

                t_Kingdom kingdom = _dbContext.t_kingdom.FirstOrDefault(u => u.fk_user_id == user.CustomUserId);
                t_Treasury treasury = _dbContext.t_treasury.FirstOrDefault(u => u.fk_user_id == user.CustomUserId);

                Map map = new Map();

                JsonSerializer serialiser = new JsonSerializer();
                serialiser.Converters.Add(new DeserialisationSupport());
                using (StringReader sr = new StringReader(kingdom.kingdom_map))
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        map.NodeArray = serialiser.Deserialize<BaseNode[]>(reader);
                    }
                }
                serialiser.Converters.Clear();
                //serialiser = null;

                //check if building action violates game rules
                List<BaseNode> nodesToRemove = new List<BaseNode>();
                int coinRefund = 0;
                int coinCostTotal = 0;
                int[] totalNumBuildings = kingdom.kingdom_num_node_types;
                for (int i = 0; i < nodeIndexes.Length; i++)
                {
                    //get refund amount from selling buildings
                    nodesToRemove.Add(map.NodeArray[nodeIndexes[i]]);

                    NodeTypeEnum nodeType1 = (NodeTypeEnum)map.NodeArray[nodeIndexes[i]].NodeType;
                    NodeCostEnum nodeCost1 = GetNodeCost(nodeType1);
                    coinRefund += (int)nodeCost1 / 2;

                    //get total cost of new buildings
                    NodeTypeEnum nodeType2 = (NodeTypeEnum)nodeTypes[i];
                    NodeCostEnum nodeCost2 = GetNodeCost(nodeType2);
                    coinCostTotal += (int)nodeCost2;

                    //get total number of each nodetype after map update
                    totalNumBuildings[(int)nodeType1]--;
                    totalNumBuildings[(int)nodeType2]++;

                    if (map.NodeArray[nodeIndexes[i]].NodeType != (int)NodeTypeEnum.Road && nodeTypes[i] == (int)NodeTypeEnum.Blockade) return new MapUpdateResponse() { Success = false, ErrorMessage = "violates rule: cannot build build blockade on any nodetype except road" }; //violates rule: cannot build build blockade on any nodetype except road
                }

                //expand on the below line of code's error message
                if (!MapService.ValidateBuildActionByNumOfBuildings(totalNumBuildings)) return new MapUpdateResponse() { Success = false, ErrorMessage = "violates a building restriction rule by nodetype totals" }; //violates building restrictions by nodetype totals
                if (treasury.treasury_coin + coinRefund < coinCostTotal) return new MapUpdateResponse() { Success = false, ErrorMessage = "insufficient funds" }; //insufficient funds


                for (int i = 0; i < nodeIndexes.Length; i++)
                {
                    map.NodeArray[nodeIndexes[i]] = nodeTypes[i] switch
                    {
                        0 => new Grassland
                        {
                            NodeIndex = nodeIndexes[i],
                            NodeType = nodeTypes[i],
                            NodeCost = (int)NodeCostEnum.Grassland,
                            NodeLevel = 0
                        },
                        1 => new TownCentre
                        {
                            NodeIndex = nodeIndexes[i],
                            NodeType = nodeTypes[i],
                            NodeCost = (int)NodeCostEnum.Grassland,
                            NodeLevel = 0
                        },
                        2 => new House
                        {
                            NodeIndex = nodeIndexes[i],
                            NodeType = nodeTypes[i],
                            NodeCost = (int)NodeCostEnum.House,
                            NodeLevel = 0
                        },
                        3 => new Library
                        {
                            NodeIndex = nodeIndexes[i],
                            NodeType = nodeTypes[i],
                            NodeCost = (int)NodeCostEnum.Library,
                            NodeLevel = 0
                        },
                        4 => new Factory
                        {
                            NodeIndex = nodeIndexes[i],
                            NodeType = nodeTypes[i],
                            NodeCost = (int)NodeCostEnum.Factory,
                            NodeLevel = 0
                        },
                        5 => new Road
                        {
                            NodeIndex = nodeIndexes[i],
                            NodeType = nodeTypes[i],
                            NodeCost = (int)NodeCostEnum.Road,
                            NodeLevel = 0
                        },
                        6 => new Blockade
                        {
                            NodeIndex = nodeIndexes[i],
                            NodeType = nodeTypes[i],
                            NodeCost = (int)NodeCostEnum.Blockade,
                            NodeLevel = 0
                        },
                        7 => new MTower
                        {
                            NodeIndex = nodeIndexes[i],
                            NodeType = nodeTypes[i],
                            NodeCost = (int)NodeCostEnum.MTower,
                            NodeLevel = 0
                        },
                        8 => new Wonder
                        {
                            NodeIndex = nodeIndexes[i],
                            NodeType = nodeTypes[i],
                            NodeCost = (int)NodeCostEnum.Wonder,
                            NodeLevel = 0
                        },

                    };
                }
               
                using (var transaction = await _dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        string serialisedKingdomMap = string.Empty;

                        using (StringWriter sw = new StringWriter())
                        {
                            using (JsonWriter writer = new JsonTextWriter(sw))
                            {
                                serialiser.Serialize(writer, map.NodeArray);
                                serialisedKingdomMap = sw.ToString();
                                sw.GetStringBuilder().Clear();
                            }
                        }

                        kingdom.kingdom_map = serialisedKingdomMap;
                        kingdom.kingdom_num_node_types = totalNumBuildings;
                        _dbContext.SaveChanges();

                        transaction.Commit();
                    }

                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Transaction failed, rolling back: {ex.Message}"); //add to dev log
                        await transaction.RollbackAsync();
                        return new MapUpdateResponse() { Success = false, ErrorMessage = $"Transaction failed, rolling back. Contact dev support for more information." };
                    }
                }


                return new MapUpdateResponse()
                {
                    Success = true
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Update map failed: {ex.Message}"); //add to dev log
                return new MapUpdateResponse() { Success = false, ErrorMessage = $"Update map failed. Contact dev support for more information." };
            }
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
        private NodeCostEnum GetNodeCost(NodeTypeEnum nodeType)
        {
            switch (nodeType)
            {
                case NodeTypeEnum.Grassland:
                    return NodeCostEnum.Grassland;
                case NodeTypeEnum.House:
                    return NodeCostEnum.House;
                case NodeTypeEnum.TownCentre:
                    return NodeCostEnum.TownCentre;
                case NodeTypeEnum.Library:
                    return NodeCostEnum.Library;
                case NodeTypeEnum.Factory:
                    return NodeCostEnum.Factory;
                case NodeTypeEnum.Road:
                    return NodeCostEnum.Road;
                case NodeTypeEnum.Blockade:
                    return NodeCostEnum.Blockade;
                case NodeTypeEnum.MTower:
                    return NodeCostEnum.MTower;
                case NodeTypeEnum.Wonder:
                    return NodeCostEnum.Wonder;
                default:
                    throw new ArgumentOutOfRangeException(nameof(nodeType), $"Unknown NodeType: {nodeType}");
            }
        }

        //if (nodeIndexes.Length == 1)
        //{




        //    //map update
        //    var sqlGenerationResult = GenerateMapUpdateSQL(nodeIndexes[0], nodeTypes[0], user.CustomUserId);
        //    await _dbContext.Database.ExecuteSqlRawAsync(sqlGenerationResult.Item1, sqlGenerationResult.Item2);
        //}
        //else if (nodeIndexes.Length > 1)
        //{




        //    //map update
        //    var sqlGenerationResult = GenerateMapUpdateSQL(nodeIndexes, nodeTypes, user.CustomUserId);
        //    for (int i = 0; i < sqlGenerationResult.Item1.Length; i++)
        //    {
        //        await _dbContext.Database.ExecuteSqlRawAsync(sqlGenerationResult.Item1[i], sqlGenerationResult.Item2[i]);
        //    }
        //}





        //chatgpt puke
        //public class NodeConverter : JsonConverter
        //{
        //    public override bool CanConvert(Type objectType)
        //    {
        //        return typeof(BaseNode).IsAssignableFrom(objectType);
        //    }
        //public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        //{
        //    JObject jsonObject = JObject.Load(reader);
        //    int nodeType = jsonObject["NodeType"].Value<int>();

        //    BaseNode node = nodeType switch
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



