using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MM_API.Services;
using MonoMonarchNetworkFramework.Game.Character;
using MonoMonarchNetworkFramework;

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

        //[Authorize(Policy = "UserPolicy")]
        //[HttpGet("loadbattleboard")]
        //public async Task<ActionResult<ICharacterLoadResponse>> LoadCharacter()//[FromBody] MapLoadPayload payload
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }
        //    try
        //    {
        //        var result = await _characterService.LoadCharacter();
        //        if (result is CharacterLoadResponse)
        //        {

        //            return Ok(result);
        //        }
        //        else if (result is ErrorResponse)
        //        {
        //            return StatusCode(400, result); //incorrect error code - unsure how to handle 
        //        }
        //        else return StatusCode(500);
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine($"Character load failed: {ex.Message}");
        //        return StatusCode(500);
        //    }
        //}
    }
}