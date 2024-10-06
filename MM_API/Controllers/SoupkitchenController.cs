using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MM_API.Services;
using SharedGameFramework.Game.Soupkitchen;
using SharedNetworkFramework.Game.Soupkitchen;


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
        //localhost:5223/soupkitchen/claimsoup
        [Authorize(Policy = "UserPolicy")]
        [HttpPost("claimsoup")]
        public async Task<ActionResult<ISoupkitchenClaimResponse>> ClaimSoup()//[FromBody] MapLoadPayload payload
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _soupkitchenService.ClaimSoup();
                if (result is ISoupkitchenClaimResponse)
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
                System.Diagnostics.Debug.WriteLine($"Claim soup failed: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }


    } 
}