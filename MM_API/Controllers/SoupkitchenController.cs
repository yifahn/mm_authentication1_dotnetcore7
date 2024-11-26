using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MM_API.Services;
using MonoMonarchGameFramework.Game.Soupkitchen;
using MonoMonarchNetworkFramework;
using MonoMonarchNetworkFramework.Game.Kingdom;
using MonoMonarchNetworkFramework.Game.Soupkitchen;
using MonoMonarchNetworkFramework.Game.Soupkitchen.Claim;


namespace MM_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SoupkitchenController : Controller
    {
        private readonly ISoupkitchenService _soupkitchenService;
        public SoupkitchenController(ISoupkitchenService soupkitchenService)
        {
            _soupkitchenService = soupkitchenService;
        }
        [Authorize(Policy = "UserPolicy")]
        [HttpGet("loadsoupkitchen")]
        public async Task<ActionResult<ISoupkitchenLoadResponse>> LoadSoupkitchen()//[FromBody] MapLoadPayload payload
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _soupkitchenService.LoadSoupkitchenAsync();
                if (result is SoupkitchenLoadResponse)
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
                System.Diagnostics.Debug.WriteLine($"Soupkitchen load failed: {ex.Message}");
                return StatusCode(500);
            }
        }

        [Authorize(Policy = "UserPolicy")]
        [HttpPost("claimsoup")]
        public async Task<ActionResult<IClaimResponse>> ClaimSoup()//[FromBody] MapLoadPayload payload
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _soupkitchenService.ClaimSoup();
                if (result is ClaimResponse)
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
                System.Diagnostics.Debug.WriteLine($"Claim soup failed: {ex.Message}");
                return StatusCode(500);
            }
        }


    } 
}