using MM_API.Database.Postgres.DbSchema;
using System.Text;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SharedNetworkFramework.Authentication.Logout;
using System.Security.Claims;
using SharedNetworkFramework.Game.Character;
using SharedNetworkFramework.Game.Kingdom.Map;
using SharedNetworkFramework.Game.Kingdom;
using SharedNetworkFramework.Game.Character.Sheet;
using SharedNetworkFramework.Game.Character.State;
using SharedNetworkFramework.Game.Character.Inventory;

namespace MM_API.Services
{
    public interface ICharacterService
    {
        public Task<ICharacterLoadResponse> LoadCharacter();
        public Task<ISheetUpdateResponse> UpdateCharacterSheet(SheetUpdatePayload sheetUpdatePayload);
        public Task<IStateUpdateResponse> UpdateCharacterState(StateUpdatePayload stateUploadPayload);
        public Task<IInventoryUpdateResponse> UpdateCharacterInventory(InventoryUpdatePayload inventoryUpdatePayload);
    }

    #region Production
    public class CharacterService //: ICharacterService
    {
        private readonly MM_DbContext _dbContext;

        public CharacterService(MM_DbContext dbContext)
        {
            _dbContext = dbContext;
        }
    }
    #endregion
    #region Development
    public class TestCharacterService : ICharacterService
    {
        private readonly MM_DbContext _dbContext;

        public TestCharacterService(MM_DbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<ICharacterLoadResponse> LoadCharacter()
        {
            try
            {

                return new CharacterLoadResponse
                {
                    
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Logout failed: {ex.Message}");
            }
            return null;
        }
        public async Task<IInventoryUpdateResponse> UpdateCharacterInventory(InventoryUpdatePayload inventoryUpdatePayload)
        {
            try
            {

                return new InventoryUpdateResponse
                {
                  
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Logout failed: {ex.Message}");
            }
            return null;
        }
        public async Task<ISheetUpdateResponse> UpdateCharacterSheet(SheetUpdatePayload sheetUpdatePayload)
        {
            try
            {

                return new SheetUpdateResponse
                {
                   
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Logout failed: {ex.Message}");
            }
            return null;
        }
        public async Task<IStateUpdateResponse> UpdateCharacterState(StateUpdatePayload stateUpdatePayload)
        {
            try
            {

                return new StateUpdateResponse
                {
                  
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Logout failed: {ex.Message}");
            }
            return null;
        }


    }
}
    #endregion