using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MM_API.Services;
using SharedNetworkFramework.Game.Kingdom;
using SharedNetworkFramework.Game.Kingdom.Map;


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
                var result = await _kingdomService.LoadKingdom();
                if (result is IKingdomLoadResponse)
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

        //localhost:5223/kingdom/updatemap
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
                var result = await _kingdomService.UpdateMap(payload);
                if (result is IMapUpdateResponse)
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
                System.Diagnostics.Debug.WriteLine($"Update map failed: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
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