using MM_API.Database.Postgres.DbSchema;
using System.Text;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MM_API.Database.Postgres;
using MonoMonarchNetworkFramework.Game.Kingdom;
using System.Security.Claims;
using MonoMonarchNetworkFramework.Game.Treasury;
using MonoMonarchNetworkFramework.Game.Soupkitchen;

namespace MM_API.Services
{
    public interface ITreasuryService
    {
        public Task<ITreasuryLoadResponse> LoadTreasuryAsync();
    }
    #region Production
    public class TreasuryService : ITreasuryService
    {
        private readonly MM_DbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public TreasuryService(MM_DbContext dbContext, UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<ITreasuryLoadResponse> LoadTreasuryAsync()
        {
            var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(u => u.Type == $"{ClaimTypes.NameIdentifier}").Value;
            var user = await _userManager.FindByIdAsync(userId);

            t_Treasury treasury = await _dbContext.t_treasury.FirstOrDefaultAsync(m => m.fk_user_id == user.CustomUserId);
            return new TreasuryLoadResponse()
            {
                TreasuryState = treasury.treasury_state,
                //TotalCoin = treasury.treasury_total,
            };
        }
    }
    #endregion
    #region Development
    public class TestTreasuryService : ITreasuryService
    {
        private readonly MM_DbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public TestTreasuryService(MM_DbContext dbContext, UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<ITreasuryLoadResponse> LoadTreasuryAsync()
        {
            var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(u => u.Type == $"{ClaimTypes.NameIdentifier}").Value;
            var user = await _userManager.FindByIdAsync(userId);

            t_Treasury treasury = await _dbContext.t_treasury.FirstOrDefaultAsync(m => m.fk_user_id == user.CustomUserId);
            return new TreasuryLoadResponse()
            {
                TreasuryState = treasury.treasury_state,
               // TotalCoin = treasury.treasury_total,
            };
        }
    }
}
    #endregion