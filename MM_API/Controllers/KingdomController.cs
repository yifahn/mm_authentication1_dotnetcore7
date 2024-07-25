using Microsoft.AspNetCore.Mvc;
using MM_API.Services;

namespace MM_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class KingdomController : Controller
    {
        private readonly IKingdomService _kingdomService;
        public KingdomController(IKingdomService kingdomService)
        {
            _kingdomService = kingdomService;
        }



    }
}