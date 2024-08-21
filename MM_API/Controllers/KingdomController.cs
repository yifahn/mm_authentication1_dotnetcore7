using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MM_API.Services;
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

        //localhost:5223/kingdom/newmap
        [Authorize]
        [HttpPost("newmap")]
        public async Task<ActionResult<IMapNewResponse>> NewMap([FromBody] MapNewPayload payload)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _kingdomService.NewMap(payload);
                if (result is IMapNewResponse)
                {
                    System.Diagnostics.Debug.WriteLine($"REACHED HERE SUCCESS");
                    return Ok(result);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"REACHED HERE #1");
                    return StatusCode(500, "Unexpected Error Occurred"); //incorrect error code - unsure how to handle 
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"REACHED HERE #2");
                System.Diagnostics.Debug.WriteLine($"New map failed: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }
        //localhost:5223/kingdom/loadmap
        [HttpPost("loadmap")]
        public async Task<ActionResult<IMapLoadResponse>> LoadMap([FromBody] MapLoadPayload payload)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _kingdomService.LoadMap(payload);
                if (result is IMapLoadResponse)
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
        [HttpPost("updatemap")]
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