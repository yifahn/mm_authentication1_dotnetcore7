using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using MM_API.Database.Postgres.DbSchema;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;



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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var userId = "5de48f8c-0a70-4e21-9cc0-798ff818fdc3"; //test admin account

            //var claimId = "1e72b6d4-b68d-40c7-b4cb-eb8be7b249a9";

            var roleUserId = "520d6e0e-235c-47c2-a1d8-5078f7b3fa43";
            var roleAdminId = "5de48f8c-0a70-4e21-9cc0-798ff818fdc3";

            modelBuilder.Entity<IdentityRole>().HasData(new IdentityRole
            {
                Id = roleAdminId,
                Name = "Admin",
                NormalizedName = "ADMIN"
            });
            modelBuilder.Entity<IdentityRole>().HasData(new IdentityRole
            {
                Id = roleUserId,
                Name = "User",
                NormalizedName = "USER"
            });


            // Hash the password for the user
            var hasher = new PasswordHasher<IdentityUser>();

            // Seed the user 'yifahn'
            modelBuilder.Entity<IdentityUser>().HasData(new IdentityUser
            {
                Id = userId,
                UserName = "yifahnadmin",
                NormalizedUserName = "YIFAHNADMIN",
                Email = "yifahn@gmail.com",
                NormalizedEmail = "YIFAHN@GMAIL.COM",
                PasswordHash = hasher.HashPassword(null, "s3cur4p4$$w0rd")
            });

            // Seed the relation between the user and the 'Admin' role
            modelBuilder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
            {
                RoleId = roleAdminId,
                UserId = userId
            });
            modelBuilder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
            {
                RoleId = roleUserId,
                UserId = userId
            });
        }


    }
}
