using Database.Postgres.DbSchema;
using System.Text;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace MM_API.Services
{
    public interface IBattleboardService
    {
    }
    #region Production
    public class BattleboardService : IBattleboardService
    {

    }
    #endregion
    #region Development
    public class TestBattleboardService : IBattleboardService
    {
        private readonly MM_DbContext _dbContext;

        public TestBattleboardService(MM_DbContext dbContext)
        {
            _dbContext = dbContext;
        }



    }
}
    #endregion