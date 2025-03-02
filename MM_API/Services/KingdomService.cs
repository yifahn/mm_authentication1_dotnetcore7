using System.Security.Claims;
using System.Xml.Linq;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Numerics;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Npgsql;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

using MM_API.Database.Postgres.DbSchema;
using MM_API.Database.Postgres;

using MonoMonarchNetworkFramework;
using MonoMonarchNetworkFramework.Game.Kingdom;
using MonoMonarchNetworkFramework.Game.Kingdom.Map;

using MonoMonarchGameFramework.Game;
using MonoMonarchGameFramework.Game.Kingdom;
using MonoMonarchGameFramework.Game.Kingdom.Nodes.Grassland;
using MonoMonarchGameFramework.Game.Kingdom.Nodes.TownCentre;
using MonoMonarchGameFramework.Game.Kingdom.Nodes.House;
using MonoMonarchGameFramework.Game.Kingdom.Nodes.Library;
using MonoMonarchGameFramework.Game.Kingdom.Nodes.Factory;
using MonoMonarchGameFramework.Game.Kingdom.Nodes.Road;
using MonoMonarchGameFramework.Game.Kingdom.Nodes.Blockade;
using MonoMonarchGameFramework.Game.Kingdom.Nodes.MTower;
using MonoMonarchGameFramework.Game.Kingdom.Nodes.Wonder;

using MonoMonarchGameFramework.Game.Armoury.Equipment;

using MonoMonarchGameFramework.Game.Treasury.Currency;
using MonoMonarchGameFramework.Game.Treasury;
using MonoMonarchGameFramework.Game.Treasury.GoldBag;
using MonoMonarchGameFramework.Game.Kingdom.Nodes;



namespace MM_API.Services
{
    public interface IKingdomService
    {
        public Task<IKingdomLoadResponse> LoadKingdomAsync();
        public Task<IKingdomMapUpdateResponse> UpdateKingdomMapAsync(KingdomMapUpdatePayload payload);
    }

    #region Production


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
        public async Task<IKingdomLoadResponse> LoadKingdomAsync()
        {
            var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(u => u.Type == $"{ClaimTypes.NameIdentifier}").Value;
            var user = await _userManager.FindByIdAsync(userId);

            t_Kingdom kingdom = await _dbContext.t_kingdom.FirstOrDefaultAsync(m => m.fk_user_id == user.CustomUserId);
            return new KingdomLoadResponse()
            {
                KingdomState = kingdom.kingdom_state,
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
        public async Task<IKingdomMapUpdateResponse> UpdateKingdomMapAsync(KingdomMapUpdatePayload mapUpdatePayload)
        {
            try
            {
                if (mapUpdatePayload.NodeIndexes.Length > 1979 || mapUpdatePayload.NodeIndexes.Length < 0
                    || mapUpdatePayload.NodeIndexes.Distinct().Count() != mapUpdatePayload.NodeIndexes.Length
                    || mapUpdatePayload.NodeIndexes.Length != mapUpdatePayload.NodeTypes.Length)
                {
                    return new ErrorResponse("Malformed payload detected");
                }


                var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(u => u.Type == $"{ClaimTypes.NameIdentifier}").Value;
                var user = await _userManager.FindByIdAsync(userId);
                int[] nodeIndexes = mapUpdatePayload.NodeIndexes;
                int[] nodeTypes = mapUpdatePayload.NodeTypes;

                t_Kingdom kingdom = _dbContext.t_kingdom.FirstOrDefault(u => u.fk_user_id == user.CustomUserId);
                t_Treasury treasury = _dbContext.t_treasury.FirstOrDefault(u => u.fk_user_id == user.CustomUserId);


                TreasuryState treasuryState = null;
                KingdomState kingdomState = null;
                BaseNode[] kingdomMap = null;

                JsonSerializer serialiser = new JsonSerializer();
                serialiser.Converters.Add(new DeserialisationSupport());
                using (StringReader sr = new StringReader(kingdom.kingdom_map))
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        kingdomMap = serialiser.Deserialize<BaseNode[]>(reader);
                    }
                }
                ///
                serialiser.Converters.Clear();
                ///
                using (StringReader sr = new StringReader(treasury.treasury_state))
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        treasuryState = serialiser.Deserialize<TreasuryState>(reader);
                    }
                }
                using (StringReader sr = new StringReader(kingdom.kingdom_state))
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        kingdomState = serialiser.Deserialize<KingdomState>(reader);
                    }
                }

                var result = treasuryState.UpdateCoinOnElapsedTicks(treasury.treasury_total, treasury.treasury_updated_at_datetime);
                treasury.treasury_total = result.Item1;
                treasury.treasury_updated_at_as_gametick += result.Item2;
                treasury.treasury_updated_at_datetime = result.Item3;



                //check if building action violates game rules
                List<BaseNode> nodesToRemove = new List<BaseNode>();
                int coinRefund = 0;
                int coinCostTotal = 0;
                //int[] totalNumBuildings = kingdomState.NumNodeTypes;
                for (int i = 0; i < nodeIndexes.Length; i++)
                {
                    //game rule check
                    if (nodeTypes[i] == 6)
                        if (!KingdomState.ValidateBlockadeRoadRule(kingdomMap[nodeIndexes[i]].NodeType, nodeTypes[i]))
                            return new ErrorResponse("Malformed payload detected");

                    //if (kingdomMap[nodeIndexes[i]].NodeType != (int)NodeTypeEnum.Road && nodeTypes[i] == (int)NodeTypeEnum.Blockade)
                    //    return new ErrorResponse("Malformed payload detected");

                    //remove enums... such shit code................. - unused props on nodes for cost

                    //get all nodes to remove
                    nodesToRemove.Add(kingdomMap[nodeIndexes[i]]);
                    //get refund amount from selling buildings
                    NodeTypeEnum nodeType1 = (NodeTypeEnum)kingdomMap[nodeIndexes[i]].NodeType;
                    NodeCostEnum nodeCost1 = KingdomState.GetNodeCost(nodeType1);
                    coinRefund += (int)nodeCost1 / 2;
                    //get total cost of new buildings
                    NodeTypeEnum nodeType2 = (NodeTypeEnum)nodeTypes[i];
                    NodeCostEnum nodeCost2 = KingdomState.GetNodeCost(nodeType2);
                    coinCostTotal += (int)nodeCost2;

                    //update total number of each nodetype
                    kingdomState.NumNodeTypes[(int)nodeType1]--;
                    kingdomState.NumNodeTypes[(int)nodeType2]++;

                    //if (map.NodeArray[nodeIndexes[i]].NodeType != (int)NodeTypeEnum.Road && nodeTypes[i] == (int)NodeTypeEnum.Blockade) return new KingdomMapUpdateResponse() { Success = false, ErrorMessage = "violates rule: cannot build build blockade on any nodetype except road" }; //violates rule: cannot build build blockade on any nodetype except road
                    kingdomMap[nodeIndexes[i]] = nodeTypes[i] switch
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
                //update treasury state
                treasuryState.UpdateCoinGainRate(kingdomState.NumNodeTypes);
                treasuryState.UpdateCoinMultiplier(kingdomState.NumNodeTypes);

                //expand on the below line of code's error message
                if (!KingdomState.ValidateBuildActionByNumOfBuildings(kingdomState.NumNodeTypes))
                    return new ErrorResponse("Malformed payload detected");


                ///


                ///coin
                ////potential bug: refund > cost == .Subtract() not handling inverse
                //long operationRemainder = treasuryState.SubtractCoin(coinCostTotal - coinRefund);
                BigInteger remainder = 0;
                if (coinRefund > coinCostTotal)
                    remainder = treasuryState.AddCoin(coinRefund - coinCostTotal);
                else if ((coinRefund < coinCostTotal))
                    remainder = treasuryState.SubtractCoin(coinCostTotal - coinRefund);
                if (remainder > 0)
                    return new ErrorResponse("Insufficient coin");


                using (var transaction = await _dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        string serialisedTreasuryState, serialisedKingdomState;
                        string serialisedKingdomMap;

                        using (StringWriter sw = new StringWriter())
                        {
                            using (JsonWriter writer = new JsonTextWriter(sw))
                            {
                                serialiser.Serialize(writer, treasuryState);
                                serialisedTreasuryState = sw.ToString();
                                sw.GetStringBuilder().Clear();
                                serialiser.Serialize(writer, kingdomState);
                                serialisedKingdomState = sw.ToString();
                                sw.GetStringBuilder().Clear();

                                serialiser.Serialize(writer, kingdomMap);
                                serialisedKingdomMap = sw.ToString();
                                sw.GetStringBuilder().Clear();
                            }
                        }
                        treasury.treasury_state = serialisedTreasuryState;

                        kingdom.kingdom_map = serialisedKingdomMap;
                        kingdom.kingdom_state = serialisedKingdomState;//kingdomState.NumNodeTypes = totalNumBuildings;
                        _dbContext.SaveChanges();

                        transaction.Commit();
                    }

                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        return new ErrorResponse("Transaction failed, rolling back");
                    }
                }


                return new KingdomMapUpdateResponse()
                {
                    CoinBagArray = treasuryState.CoinBagArray,

                    CoinUpdateDateTime = treasury.treasury_updated_at_datetime,
                    CoinUpdateOnTick = treasury.treasury_updated_at_as_gametick,

                };
            }
            catch (Exception ex)
            {
                return new ErrorResponse("Update map failed");
            }
        }
    }
}


#endregion

//public static (string, NpgsqlParameter[]) GenerateMapUpdateSQL(int nodeIndex, int nodeType, int userId)
//{
//    int[] nodeTypeArray = { nodeType };
//    var nodeData = KingdomService.ResolveNodeDataByType(nodeTypeArray);
//    string sqlQuery = @"UPDATE t_kingdom SET kingdom_map = jsonb_set(kingdom_map, ARRAY['Nodes', @NodeIndex::text], jsonb_build_object('NodeCost', @NodeCost,'NodeType', @NodeType,'NodeIndex', @NodeIndex,'NodeLevel', @NodeLevel)::jsonb) WHERE fk_user_id = @UserId;";
//    var parameters = new[]
//    {
//        new NpgsqlParameter("@NodeIndex", nodeIndex),
//        new NpgsqlParameter("@NodeCost", nodeData.Item1[0]),
//        new NpgsqlParameter("@NodeType", nodeType),
//        new NpgsqlParameter("@NodeLevel", nodeData.Item2[0]),
//        new NpgsqlParameter("@UserId", userId)
//    };
//    return (sqlQuery, parameters);
//}
//public static (string[], NpgsqlParameter[][]) GenerateMapUpdateSQL(int[] nodeIndexArray, int[] nodeTypeArray, int userId)
//{
//    var nodeData = KingdomService.ResolveNodeDataByType(nodeTypeArray);
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
//public class KingdomService : IKingdomService
//{
//    private readonly MM_DbContext _dbContext;
//    private readonly UserManager<ApplicationUser> _userManager;
//    private readonly IHttpContextAccessor _httpContextAccessor;
//    public KingdomService(MM_DbContext dbContext, UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
//    {
//        _dbContext = dbContext;
//        _userManager = userManager;
//        _httpContextAccessor = httpContextAccessor;
//    }
//    public async Task<IKingdomLoadResponse> LoadKingdomAsync()
//    {
//        var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(u => u.Type == $"{ClaimTypes.NameIdentifier}").Value;
//        var user = await _userManager.FindByIdAsync(userId);

//        t_Kingdom kingdom = await _dbContext.t_kingdom.FirstOrDefaultAsync(m => m.fk_user_id == user.CustomUserId);
//        var loadKingdomResponse = new KingdomLoadResponse()
//        {
//            KingdomName = kingdom.kingdom_name,
//        };
//        return loadKingdomResponse;
//    }

//    public async Task<IKingdomMapUpdateResponse> UpdateKingdomMapAsync(KingdomMapUpdatePayload mapUpdatePayload)
//    {
//        try
//        {
//            var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(u => u.Type == $"{ClaimTypes.NameIdentifier}").Value;
//            var user = await _userManager.FindByIdAsync(userId);
//            int[] nodeIndexes = mapUpdatePayload.NodeIndexes;
//            int[] nodeTypes = mapUpdatePayload.NodeTypes;
//            if (nodeIndexes.Length == 1)
//            {
//                var sqlGenerationResult = GenerateMapUpdateSQL(nodeIndexes[0], nodeTypes[0], user.CustomUserId);
//                await _dbContext.Database.ExecuteSqlRawAsync(sqlGenerationResult.Item1, sqlGenerationResult.Item2);
//            }
//            else if (nodeIndexes.Length > 1)
//            {
//                var sqlGenerationResult = GenerateMapUpdateSQL(nodeIndexes, nodeTypes, user.CustomUserId);
//                for (int i = 0; i < sqlGenerationResult.Item1.Length; i++)
//                {
//                    await _dbContext.Database.ExecuteSqlRawAsync(sqlGenerationResult.Item1[i], sqlGenerationResult.Item2[i]);
//                }
//            }
//            return new KingdomMapUpdateResponse()
//            {
//            };
//        }
//        catch (Exception ex)
//        {
//            System.Diagnostics.Debug.WriteLine($"Registration failed: {ex.Message}");
//        }
//        return new KingdomMapUpdateResponse()
//        {
//        };
//    }
//    public static (string, NpgsqlParameter[]) GenerateMapUpdateSQL(int nodeIndex, int nodeType, int userId)
//    {
//        int[] nodeTypeArray = { nodeType };
//        var nodeData = KingdomService.ResolveNodeDataByType(nodeTypeArray);
//        string sqlQuery = @"UPDATE t_kingdom SET kingdom_map = jsonb_set(kingdom_map, ARRAY['Nodes', @NodeIndex::text], jsonb_build_object('NodeCost', @NodeCost,'NodeType', @NodeType,'NodeIndex', @NodeIndex,'NodeLevel', @NodeLevel)::jsonb) WHERE fk_user_id = @UserId;";
//        var parameters = new[]
//        {
//        new NpgsqlParameter("@NodeIndex", nodeIndex),
//        new NpgsqlParameter("@NodeCost", nodeData.Item1[0]),
//        new NpgsqlParameter("@NodeType", nodeType),
//        new NpgsqlParameter("@NodeLevel", nodeData.Item2[0]),
//        new NpgsqlParameter("@UserId", userId)
//    };
//        return (sqlQuery, parameters);
//    }
//    public static (string[], NpgsqlParameter[][]) GenerateMapUpdateSQL(int[] nodeIndexArray, int[] nodeTypeArray, int userId)
//    {
//        var nodeData = KingdomService.ResolveNodeDataByType(nodeTypeArray);
//        string[] sqlArray = new string[nodeTypeArray.Length];
//        NpgsqlParameter[][] sqlParameterArray = new NpgsqlParameter[nodeTypeArray.Length][];
//        for (int i = 0; i < nodeTypeArray.Length; i++)
//        {
//            sqlArray[i] = @"UPDATE t_kingdom SET kingdom_map = jsonb_set(kingdom_map, ARRAY['Nodes', @NodeIndex::text], jsonb_build_object('NodeCost', @NodeCost,'NodeType', @NodeType,'NodeIndex', @NodeIndex,'NodeLevel', @NodeLevel)::jsonb) WHERE fk_user_id = @UserId;";
//            sqlParameterArray[i] =
//            [
//                new("@NodeIndex", nodeIndexArray[i]),
//            new("@NodeCost", nodeData.Item1[i]),
//            new("@NodeType", nodeTypeArray[i]),
//            new("@NodeLevel", nodeData.Item2[i]),
//            new("@UserId", userId)
//            ];
//        }
//        return (sqlArray, sqlParameterArray);
//    }


//}

