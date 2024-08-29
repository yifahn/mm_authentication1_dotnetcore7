using Microsoft.AspNetCore.Identity;
using MM_API.Database.Postgres.DbSchema;
using System.ComponentModel.DataAnnotations.Schema;

namespace MM_API.Database.Postgres
{
    public class ApplicationUser : IdentityUser
    {
        public int CustomUserId { get; set; }

        [ForeignKey("CustomUserId")]
        public t_User User { get; set; }
    }
}
