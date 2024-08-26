using MM_API.Database.Postgres.DbSchema;
using System.Text;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace MM_API.Services
{
    public interface ICharacterService
    {
    }
    #region Production
    public class CharacterService : ICharacterService
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



    }
}
    #endregion