using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MM_API.Services;
using MonoMonarchNetworkFramework;
using MonoMonarchNetworkFramework.Game.Kingdom;
using MonoMonarchNetworkFramework.Game.Kingdom.Map;


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
        [Authorize(Policy = "UserPolicy")]
        [HttpGet("loadkingdom")]
        public async Task<ActionResult<IKingdomLoadResponse>> LoadKingdom()//[FromBody] MapLoadPayload payload
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _kingdomService.LoadKingdomAsync();
                if (result is KingdomLoadResponse)
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
                System.Diagnostics.Debug.WriteLine($"Kingdom load failed: {ex.Message}");
                return StatusCode(500);
            }
        }

        [Authorize(Policy = "UserPolicy")]
        [HttpPatch("updatemap")]
        public async Task<ActionResult<IMapUpdateResponse>> UpdateMap([FromBody] MapUpdatePayload payload)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _kingdomService.UpdateMapAsync(payload);
                if (result is MapUpdateResponse)
                {
                    return Ok(result);
                }
                else if (result is ErrorResponse)
                {
                    return StatusCode(400, $"{result}"); //incorrect error code - unsure how to handle 
                }
                else return StatusCode(500);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Update map failed: {ex.Message}");
                return StatusCode(500);
            }
        }

    }
}

////localhost:5223/kingdom/loadmap
//[Authorize(Policy = "UserPolicy")]
//[HttpGet("loadmap")]
//public async Task<ActionResult<IMapLoadResponse>> LoadMap()//[FromBody] MapLoadPayload payload
//{
//    if (!ModelState.IsValid)
//    {
//        return BadRequest(ModelState);
//    }
//    try
//    {
//        var result = await _kingdomService.LoadMap();
//        if (result is IMapLoadResponse)
//        {
//            return Ok(result);
//        }
//        else
//        {
//            return StatusCode(500, "Unexpected Error Occurred"); //incorrect error code - unsure how to handle 
//        }
//    }
//    catch (Exception ex)
//    {
//        System.Diagnostics.Debug.WriteLine($"Load map failed: {ex.Message}");
//        return StatusCode(500, "Internal Server Error");
//    }
//}