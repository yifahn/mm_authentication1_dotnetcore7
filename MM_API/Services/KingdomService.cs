using MM_API.Database.Postgres.DbSchema;
using System.Text;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using SharedNetworkFramework.Game.Kingdom.Map;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace MM_API.Services
{
    public interface IKingdomService
    {
        public Task<IMapNewResponse> NewMap(MapNewPayload payload);
        public Task<IMapLoadResponse> LoadMap(MapLoadPayload payload);
        public Task<IMapUpdateResponse> UpdateMap(MapUpdatePayload payload);


    }


    /*          NewMapPayload DTO
    ________________________________|
    SignInPayload                   |
    ________________________________|
                                    |
    email	          |  string	    |
    password	      |  string	    |
    returnSecureToken |	 boolean	|
                      |             |
    ________________________________|
    SignInResponse    |             |
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
    SignInPayload                   |       
    ________________________________|       
                                    |       
    email	          |  string	    |       
    password	      |  string	    |       
    returnSecureToken |	 boolean	|       
                      |             |       
    ________________________________|       
    SignInResponse    |             |       
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

        public async Task<IMapNewResponse> NewMap(MapNewPayload mapNewPayload)
        {
            return null;
        }
        public async Task<IMapLoadResponse> LoadMap(MapLoadPayload mapLoadPayload)
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
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TestKingdomService(MM_DbContext dbContext, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _httpContextAccessor = httpContextAccessor;
        }
        //[Authorize(Policy = "NewGamePolicy")]
        //public async Task<IMapNewResponse> NewMap(MapNewPayload mapNewPayload)
        //{
        //    var user = await _userManager.FindByNameAsync("test123");
        //    await _userManager.RemoveFromRoleAsync(user, "NewGame");
        //    return null;
        //}
        [Authorize(Policy = "UserPolicy")]
        public async Task<IMapLoadResponse> LoadMap(MapLoadPayload mapLoadPayload)
        {
            var email = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            var user = await _userManager.FindByEmailAsync(email);
            var userClaims = await _userManager.GetClaimsAsync(user);
            var newGameClaim = userClaims.FirstOrDefault(c => c.Type == "NewGame" && c.Value == "true");
            if (newGameClaim != null)
            {



                await _userManager.RemoveClaimAsync(user, newGameClaim);
                var newClaim = new Claim("NewGame", "false");
                await _userManager.AddClaimAsync(user, newClaim);
            }
            return null;
        }
        public async Task<IMapUpdateResponse> UpdateMap(MapUpdatePayload mapUpdatePayload)
        {
            return null;
        }

    }
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

