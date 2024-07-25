using Database.Postgres.DbSchema;
using System.Text;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace MM_API.Services
{
    public interface ISoupkitchenService
    {
    }
    #region Production
    public class SoupkitchenService : ISoupkitchenService
    {

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