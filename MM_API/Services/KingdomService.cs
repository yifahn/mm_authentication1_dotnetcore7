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

namespace MM_API.Services
{
    public interface IKingdomService
    {
        //  public Task<IMapNewResponse> NewMap(MapNewPayload payload);
        public Task<IMapLoadResponse> LoadMap();//MapLoadPayload payload
        public Task<IMapUpdateResponse> UpdateMap(MapUpdatePayload payload);


    }


    /*          NewMapPayload DTO
    ________________________________|
    LoginPayload                   |
    ________________________________|
                                    |
    email	          |  string	    |
    password	      |  string	    |
    returnSecureToken |	 boolean	|
                      |             |
    ________________________________|
    LoginResponse    |             |
    ________________________________|
                      |             |
    idToken	          |  string	    |
    email	          |  string	    |
    refreshToken	  |  string	    |
    expiresIn         |  string	    |
    localId	          |  string	    |
    registered        |  boolean	|
     */

    /*
     *          SIGNIN DTO
    ________________________________|       
    LoginPayload                   |       
    ________________________________|       
                                    |       
    email	          |  string	    |       
    password	      |  string	    |       
    returnSecureToken |	 boolean	|       
                      |             |       
    ________________________________|       
    LoginResponse    |             |       
    ________________________________|       
                      |             |       
    idToken	          |  string	    |       
    email	          |  string	    |       
    refreshToken	  |  string	    |       
    expiresIn         |  string	    |       
    localId	          |  string	    |       
    registered        |  boolean	|       
     */




    #region Production
    public class KingdomService : IKingdomService
    {
        private readonly MM_DbContext _dbContext;


        public KingdomService(MM_DbContext dbContext)
        {
            _dbContext = dbContext;

        }

        //public async Task<IMapNewResponse> NewMap(MapNewPayload mapNewPayload)
        //{
        //    return null;
        //}
        public async Task<IMapLoadResponse> LoadMap()//MapLoadPayload mapLoadPayload
        {
            return null;
        }
        public async Task<IMapUpdateResponse> UpdateMap(MapUpdatePayload mapUpdatePayload)
        {
            return null;
        }

    }
    #endregion
    #region Development
    public class TestKingdomService : IKingdomService
    {
        // private readonly SignInManager<IdentityUser> _signInManager;
        private readonly MM_DbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _env;

        public TestKingdomService(MM_DbContext dbContext, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IHttpContextAccessor httpContextAccessor, IWebHostEnvironment env)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _httpContextAccessor = httpContextAccessor;
            _env = env;
        }
        public async Task<IMapLoadResponse> LoadMap()//MapLoadPayload mapLoadPayload
        {
            var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(u => u.Type == $"{ClaimTypes.NameIdentifier}").Value;
            var user = await _userManager.FindByIdAsync(userId);

            t_Kingdom map = await _dbContext.t_kingdom.FirstOrDefaultAsync(m => m.fk_user_id == user.CustomUserId);

            //var settings = new JsonSerializerSettings
            //{
            //    Converters = new List<JsonConverter> { new NodeConverter() }
            //};

            //Map deserialisedMap = JsonConvert.DeserializeObject<Map>(map.kingdom_map, settings);

            var loadMapResponse = new MapLoadResponse()
            {
                Map = map.kingdom_map
            };

            return loadMapResponse;

            //var response = new MapLoadResponse()
            //{
            //    Map = new SharedGameFramework.Game.Kingdom.Map.Map(Node) 
            //    {
            //    }
            //};

            
        }
        public async Task<IMapUpdateResponse> UpdateMap(MapUpdatePayload mapUpdatePayload)
        {
            return null;
        }

    }
    //chatgpt puke
    public class NodeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(Node).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);
            int nodeType = jsonObject["NodeType"].Value<int>();

            Node node = nodeType switch
            {
                0 => new Grassland(),   // GL
                1 => new TownCentre(),  // TC
                2 => new House(),       // H
                3 => new Library(),     // L
                4 => new Factory(),     // F
                5 => new Road(),        // R
                6 => new Blockade(),    // B
                7 => new MTower(),      // MT
                8 => new Wonder(),      // W
                //_ => throw new ArgumentException("Unknown node type")
            };

            // Populate the properties of the node using the jsonObject
            serializer.Populate(jsonObject.CreateReader(), node);

            return node;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("Serialization not required in this example.");
        }
    }
    //var email = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
    //var user = await _userManager.FindByEmailAsync(email);
    //var userClaims = await _userManager.GetClaimsAsync(user);
    //var newGameClaim = userClaims.FirstOrDefault(c => c.Type == "NewGame" && c.Value == "true");
    //await _userManager.RemoveClaimAsync(user, newGameClaim);
    //var newClaim = new Claim("NewGame", "false");
    //await _userManager.AddClaimAsync(user, newClaim);

    #endregion
    //public async Task<Map> NewMap()
    //{

    //    try
    //    {
    //        string fb_uri = $"{FB_URL}{FB_URL_AUTH}:signUp{FB_URL_APIKEY}";
    //        string jsonPayload = JsonConvert.SerializeObject(payload);
    //        using (var client = new HttpClient())
    //        {
    //            var response = await client.PostAsync(fb_uri, new StringContent(jsonPayload, Encoding.UTF8, "application/json"));//application/json-patch+json //https://learn.microsoft.com/en-us/aspnet/core/web-api/jsonpatch?view=aspnetcore-8.0
    //            if (response.IsSuccessStatusCode)
    //            {
    //                string responseBody = await response.Content.ReadAsStringAsync();
    //                var userRecord = JsonConvert.DeserializeObject<RegistrationResponse>(responseBody);

    //                var handler = new JwtSecurityTokenHandler();
    //                var jsonToken = handler.ReadToken(userRecord.AccessToken) as JwtSecurityToken;
    //                if (jsonToken == null)
    //                {
    //                    throw new InvalidOperationException("Invalid JWT token");
    //                }
    //                t_User user = new t_User
    //                {
    //                    user_name = userRecord.Email.Substring(0, payload.Email.IndexOf('@')),
    //                    user_fb_uuid = userRecord.LocalId
    //                };
    //                t_Session session = new t_Session()
    //                {
    //                    user = user,
    //                    session_authtoken = userRecord.AccessToken,
    //                    session_refreshtoken = userRecord.RefreshToken,
    //                    session_loggedin = DateTimeOffset.UtcNow.UtcDateTime,
    //                    session_sessiontoken = "0"

    //                };
    //                await _dbContext.AddAsync(user);
    //                await _dbContext.SaveChangesAsync();
    //                await _dbContext.AddAsync(session);
    //                await _dbContext.SaveChangesAsync();
    //                return userRecord;
    //            }
    //            else
    //            {
    //                string errorResponse = await response.Content.ReadAsStringAsync();
    //                switch (errorResponse)
    //                {
    //                    case string s when s.Contains("EMAIL_EXISTS"):
    //                        throw new InvalidOperationException($"Email address is already in use: {errorResponse}");
    //                    default:
    //                        throw new Exception($"Registration failed: {errorResponse}");
    //                }
    //            }
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        System.Diagnostics.Debug.WriteLine($"Registration failed: {ex.Message}");
    //        throw;
    //    }
    //}
}

