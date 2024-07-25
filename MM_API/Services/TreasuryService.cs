using Database.Postgres.DbSchema;
using System.Text;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace MM_API.Services
{
    public interface ITreasuryService
    {
    }
    #region Production
    public class TreasuryService : ITreasuryService
    {

    }
    #endregion
    #region Development
    public class TestTreasuryService : ITreasuryService
    {
        private readonly MM_DbContext _dbContext;

        public TestTreasuryService(MM_DbContext dbContext)
        {
            _dbContext = dbContext;
        }



    }
}
    #endregion