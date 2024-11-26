using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MM_API.Services;
using MonoMonarchNetworkFramework.Game.Kingdom;
using MonoMonarchNetworkFramework;
using MonoMonarchNetworkFramework.Game.Armoury;
using MonoMonarchNetworkFramework.Game.Character;

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

        [Authorize(Policy = "UserPolicy")]
        [HttpGet("loadarmoury")]
        public async Task<ActionResult<IArmouryLoadResponse>> LoadArmoury()//[FromBody] MapLoadPayload payload
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _armouryService.LoadArmouryAsync();
                if (result is ArmouryLoadResponse)
                {

                    return Ok(result);
                }
                else if (result is ErrorResponse)
                {
                    return StatusCode(400, result); //incorrect error code - unsure how to handle 
                }
                else return StatusCode(500);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Armoury load failed: {ex.Message}");
                return StatusCode(500);
            }
        }
    }
}