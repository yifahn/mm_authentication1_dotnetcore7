using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MM_API.Services;
using SharedNetworkFramework.Game.Kingdom.Map;
using SharedNetworkFramework.Game.Character;
using SharedNetworkFramework.Game.Character.Inventory;
using SharedNetworkFramework.Game.Character.Sheet;
using SharedNetworkFramework.Game.Character.State;

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
                    return StatusCode(500, "Unexpected Error Occurred"); 
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Load map failed: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }
        [Authorize(Policy = "UserPolicy")]
        [HttpPatch("updatecharacterinventory")]
        public async Task<ActionResult<IInventoryUpdateResponse>> UpdateCharacterInventory([FromBody] InventoryUpdatePayload inventoryUpdatePayload)//[FromBody] MapLoadPayload payload
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _characterService.UpdateCharacterInventory(inventoryUpdatePayload);
                if (result is ISheetUpdateResponse)
                {
                    return Ok(result);
                }
                else
                {
                    return StatusCode(500, "Unexpected Error Occurred"); 
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Load map failed: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }
        [Authorize(Policy = "UserPolicy")]
        [HttpPatch("updatecharactersheet")]
        public async Task<ActionResult<ISheetUpdateResponse>> UpdateCharacterSheet([FromBody] SheetUpdatePayload sheetUpdatePayload)//[FromBody] MapLoadPayload payload
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _characterService.UpdateCharacterSheet(sheetUpdatePayload);
                if (result is ISheetUpdateResponse)
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
        [Authorize(Policy = "UserPolicy")]
        [HttpPatch("updatecharacterstate")]
        public async Task<ActionResult<IStateUpdateResponse>> UpdateCharacterState(StateUpdatePayload stateUpdatePayload)//[FromBody] MapLoadPayload payload
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _characterService.UpdateCharacterState(stateUpdatePayload);
                if (result is IStateUpdateResponse)
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