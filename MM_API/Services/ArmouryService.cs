using MM_API.Database.Postgres.DbSchema;
using System.Text;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MonoMonarchNetworkFramework.Game.Kingdom.Map;
using MonoMonarchNetworkFramework.Game.Kingdom;
using MonoMonarchNetworkFramework.Game.Armoury;
using MM_API.Database.Postgres;
using System.Security.Claims;

namespace MM_API.Services
{
    public interface IArmouryService
    {
        public Task<IArmouryLoadResponse> LoadArmouryAsync();
    }
    #region Production
    public class ArmouryService : IArmouryService
    {
        private readonly MM_DbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ArmouryService(MM_DbContext dbContext, UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<IArmouryLoadResponse> LoadArmouryAsync()
        {
            var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(u => u.Type == $"{ClaimTypes.NameIdentifier}").Value;
            var user = await _userManager.FindByIdAsync(userId);

            t_Armoury armoury = await _dbContext.t_armoury.FirstOrDefaultAsync(m => m.fk_user_id == user.CustomUserId);
            return new ArmouryLoadResponse()
            {
                ArmouryWeapons = armoury.armoury_weapons,
                ArmouryArmour = armoury.armoury_armour,
                ArmouryJewellery = armoury.armoury_jewellery,
                
            };
        }
    }
    #endregion
    #region Development
    public class TestArmouryService : IArmouryService
    {
        private readonly MM_DbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public TestArmouryService(MM_DbContext dbContext, UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<IArmouryLoadResponse> LoadArmouryAsync()
        {
            var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(u => u.Type == $"{ClaimTypes.NameIdentifier}").Value;
            var user = await _userManager.FindByIdAsync(userId);

            t_Armoury armoury = await _dbContext.t_armoury.FirstOrDefaultAsync(m => m.fk_user_id == user.CustomUserId);
            return new ArmouryLoadResponse()
            {
                ArmouryWeapons = armoury.armoury_weapons,
                ArmouryArmour = armoury.armoury_armour,
                ArmouryJewellery = armoury.armoury_jewellery,
            };
        }



    }
}
#endregion