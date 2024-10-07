using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using MM_API.Database.Postgres.DbSchema;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using MM_API.Database.Postgres;



namespace MM_API
{
    public class MM_DbContext : IdentityDbContext<ApplicationUser>
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

            var adminUserId = "5de48f8c-0a70-4e21-9cc0-798ff818fdc3"; //test admin account id

            var roleAdminId = "70ff5865-335d-4d60-9851-d91499c5505c"; //admin role id
            var roleUserId = "520d6e0e-235c-47c2-a1d8-5078f7b3fa43"; //user role id


            // var t_UserIdString = "536c6b38-68c2-4e80-af71-f8854e2d6cdb";
            int t_UserIdInt_Admin = -999; //t_user id for admin
            int t_UserIdInt_User = -9999; //t_user id for testuser

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

            var hasher = new PasswordHasher<ApplicationUser>();

            modelBuilder.Entity<t_User>().HasData(new t_User
            {
                user_id = t_UserIdInt_Admin,
                user_name = "yifahnadmin",
            });

            modelBuilder.Entity<ApplicationUser>().HasData(new ApplicationUser
            {
                Id = adminUserId,
                CustomUserId = t_UserIdInt_Admin,
                UserName = "yifahnadmin",
                NormalizedUserName = "YIFAHNADMIN",
                Email = "yifahnadmin@gmail.com",
                NormalizedEmail = "YIFAHNADMIN@GMAIL.COM",
                PasswordHash = hasher.HashPassword(null, "s3cur4p4$$w0rd") //development password: secure password elsewhere rather than codebase - github reasons
            });

            //modelBuilder.Entity<t_User>().HasData(new t_User
            //{
            //    user_id = t_UserIdInt_User,
            //    user_name = "yifahnuser",
            //});

            //modelBuilder.Entity<ApplicationUser>().HasData(new ApplicationUser
            //{
            //    ServerId = userUserId,
            //    CustomUserId = t_UserIdInt_User,
            //    UserName = "yifahnuser",
            //    NormalizedUserName = "YIFAHNUSER",
            //    Email = "yifahnuser@gmail.com",
            //    NormalizedEmail = "YIFAHNUSER@GMAIL.COM",
            //    PasswordHash = hasher.HashPassword(null, "s3cur4p4$$w0rd") //development password: secure password elsewhere rather than codebase - github reasons
            //});

            // Seed the relation between the user and the 'Admin' role
            modelBuilder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
            {
                RoleId = roleAdminId,
                UserId = adminUserId
            });
            //modelBuilder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
            //{
            //    RoleId = roleUserId,
            //    UserId = userUserId
            //});
        }
    }
}
