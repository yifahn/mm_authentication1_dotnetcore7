using MM_API.Database.Postgres.DbSchema;
using System.Text;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace MM_API.Services
{
    public interface ISoupkitchenService
    {
    }
    #region Production
    public class SoupkitchenService : ISoupkitchenService
    {
        private readonly MM_DbContext _dbContext;


        public SoupkitchenService(MM_DbContext dbContext)
        {
            _dbContext = dbContext;

        }
    }
    #endregion
    #region Development
    public class TestSoupkitchenService : ISoupkitchenService
    {
        private readonly MM_DbContext _dbContext;


        public TestSoupkitchenService(MM_DbContext dbContext)
        {
            _dbContext = dbContext;

        }



    }
}
    #endregion