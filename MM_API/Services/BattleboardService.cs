using Database.Postgres.DbSchema;
using System.Text;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace MM_API.Services
{
    public interface IBattleboardService
    {
    }
    #region Production
    public class BattleboardService : IBattleboardService
    {
        private readonly MM_DbContext _dbContext;


        public BattleboardService(MM_DbContext dbContext)
        {
            _dbContext = dbContext;

        }
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