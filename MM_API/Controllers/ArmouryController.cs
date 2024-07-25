using Microsoft.AspNetCore.Mvc;
using MM_API.Services;

namespace MM_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ArmouryController : ControllerBase
    {
        private readonly IArmouryService _armouryService;
        public ArmouryController(IArmouryService armouryService)
        {
            _armouryService = armouryService;
        }


    }
}