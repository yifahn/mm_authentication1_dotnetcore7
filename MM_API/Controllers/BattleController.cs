using Microsoft.AspNetCore.Mvc;
using MM_API.Services;

namespace MM_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BattleboardController : ControllerBase
    {
        private readonly IBattleboardService _battleboardService;
        public BattleboardController(IBattleboardService battleboardService)
        {
            _battleboardService = battleboardService;
        }


    }
}