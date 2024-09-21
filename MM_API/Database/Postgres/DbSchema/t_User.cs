using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity;
using MM_API.Database.Postgres.DbSchema;

namespace MM_API.Database.Postgres.DbSchema
{
    public class t_User 
    {
        [Key]
        public int user_id { get; set; }

        public string user_name { get; set; }
    }
}

//[ForeignKey("IdentityUser")]
//public string AspNetUserId { get; set; }
//public IdentityUser IdentityUser { get; set; }

//public string user_fb_uuid { get; set; }

//public t_Kingdom kingdom { get; set; }

//public ICollection<t_Session> sessions { get; set; }