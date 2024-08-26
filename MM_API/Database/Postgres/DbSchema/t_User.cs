using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace MM_API.Database.Postgres.DbSchema
{
    public class t_User 
    {
        [Key]
        public int user_id { get; set; }

        public t_Kingdom kingdom { get; set; }

        public ICollection<t_Session> sessions { get; set; }

        public string user_name { get; set; }

        //public string user_fb_uuid { get; set; }
    }
}
