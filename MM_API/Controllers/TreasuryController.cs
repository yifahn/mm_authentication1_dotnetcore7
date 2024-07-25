using Microsoft.AspNetCore.Mvc;
using MM_API.Services;

namespace MM_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TreasuryController : ControllerBase
    {
        private readonly ITreasuryService _treasuryService;
        public TreasuryController(ITreasuryService treasuryService)
        {
            _treasuryService = treasuryService;
        }


    }
}