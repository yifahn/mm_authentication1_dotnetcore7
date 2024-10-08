﻿using MM_API.Database.Postgres.DbSchema;
using System.Text;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace MM_API.Services
{
    public interface ITreasuryService
    {
    }
    #region Production
    public class TreasuryService : ITreasuryService
    {
        private readonly MM_DbContext _dbContext;


        public TreasuryService(MM_DbContext dbContext)
        {
            _dbContext = dbContext;

        }
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