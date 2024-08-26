using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using MM_API.Database.Postgres.DbSchema;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;



namespace MM_API
{
    public class MM_DbContext : IdentityDbContext<IdentityUser>
    {
        public MM_DbContext(DbContextOptions<MM_DbContext> options) : base(options) { }

        [AllowNull]
        public DbSet<t_User> t_user { get; set; }
        [AllowNull]
        public DbSet<t_Session> t_session { get; set; }
        [AllowNull]
        public DbSet<t_Kingdom> t_kingdom { get; set; }
        [AllowNull]
        public DbSet<t_Character> t_character { get; set; }
        [AllowNull]
        public DbSet<t_Armoury> t_armoury { get; set; }
        [AllowNull]
        public DbSet<t_Treasury> t_treasury { get; set; }
        [AllowNull]
        public DbSet<t_Soupkitchen> t_soupkitchen { get; set; }

    }
}
