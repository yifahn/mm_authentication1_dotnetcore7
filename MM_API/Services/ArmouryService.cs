using MM_API.Database.Postgres.DbSchema;
using System.Text;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace MM_API.Services
{
    public interface IArmouryService
    {
    }
    #region Production
    public class ArmouryService : IArmouryService
    {
        private readonly MM_DbContext _dbContext;


        public ArmouryService(MM_DbContext dbContext)
        {
            _dbContext = dbContext;

        }
    }
    #endregion
    #region Development
    public class TestArmouryService : IArmouryService
    {
        private readonly MM_DbContext _dbContext;


        public TestArmouryService(MM_DbContext dbContext)
        {
            _dbContext = dbContext;

        }



    }
}
    #endregion