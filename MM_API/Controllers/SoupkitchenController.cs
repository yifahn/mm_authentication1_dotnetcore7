using Microsoft.AspNetCore.Mvc;
using MM_API.Services;

namespace MM_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SoupkitchenController : ControllerBase
    {
        private readonly ISoupkitchenService _soupkitchenService;
        public SoupkitchenController(ISoupkitchenService soupkitchenService)
        {
            _soupkitchenService = soupkitchenService;
        }


    }
}