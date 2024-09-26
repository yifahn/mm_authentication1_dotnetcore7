using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MM_API.Services;
using SharedNetworkFramework.Game.Kingdom.Map;
using SharedNetworkFramework.Game.Character;

namespace MM_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CharacterController : ControllerBase
    {
        private readonly ICharacterService _characterService;
        public CharacterController(ICharacterService characterService)
        {
            _characterService = characterService;
        }
        [Authorize(Policy = "UserPolicy")]
        [HttpGet("loadcharacter")]
        public async Task<ActionResult<ICharacterLoadResponse>> LoadCharacter()//[FromBody] MapLoadPayload payload
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _characterService.LoadCharacter();
                if (result is ICharacterLoadResponse)
                {
                    return Ok(result);
                }
                else
                {
                    return StatusCode(500, "Unexpected Error Occurred"); //incorrect error code - unsure how to handle 
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Load map failed: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }

    }
}