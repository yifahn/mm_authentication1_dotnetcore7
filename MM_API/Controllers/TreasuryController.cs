using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MM_API.Services;
using MonoMonarchNetworkFramework.Game.Kingdom;
using MonoMonarchNetworkFramework;
using MonoMonarchNetworkFramework.Game.Treasury;

namespace MM_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TreasuryController : ControllerBase
    {
        private readonly ITreasuryService _treasuryService;
        public TreasuryController(ITreasuryService treasuryService)
        {
            _treasuryService = treasuryService;
        }
        [Authorize(Policy = "UserPolicy")]
        [HttpGet("loadtreasury")]
        public async Task<ActionResult<IKingdomLoadResponse>> LoadTreasury()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _treasuryService.LoadTreasuryAsync();
                if (result is TreasuryLoadResponse)
                {

                    return Ok(result);
                }
                else if (result is ErrorResponse)
                {
                    return StatusCode(400, result);
                }
                else return StatusCode(500);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Treasury load failed: {ex.Message}");
                return StatusCode(500);
            }
        }

    }
}