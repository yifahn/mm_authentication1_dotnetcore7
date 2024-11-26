using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MM_API.Services;
using MonoMonarchNetworkFramework.Game.Kingdom.Map;
using MonoMonarchNetworkFramework.Game.Character;
using MonoMonarchNetworkFramework.Game.Character.Inventory;
using MonoMonarchNetworkFramework.Game.Character.Sheet;
using MonoMonarchNetworkFramework.Game.Character.State;
using MonoMonarchNetworkFramework;

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
                var result = await _characterService.LoadCharacterAsync();
                if (result is CharacterLoadResponse)
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
                System.Diagnostics.Debug.WriteLine($"Character load failed: {ex.Message}");
                return StatusCode(500);
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
                if (result is InventoryUpdateResponse)
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
                System.Diagnostics.Debug.WriteLine($"Charater update failed: {ex.Message}");
                return StatusCode(500);
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
                if (result is SheetUpdateResponse)
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
                System.Diagnostics.Debug.WriteLine($"Sheet update failed: {ex.Message}");
                return StatusCode(500);
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
                if (result is StateUpdateResponse)
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
                System.Diagnostics.Debug.WriteLine($"State update failed: {ex.Message}");
                return StatusCode(500);
            }
        }
    }
}