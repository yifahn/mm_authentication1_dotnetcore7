using SharedNetworkFramework.Authentication.Firebase.RefreshToken;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MM_API.Database.Postgres.DbSchema
{
    public class t_Session
    {
        [Key]
        public int session_id { get; set; }


        public int fk_user_id { get; set; }

        [ForeignKey("fk_user_id")]
        public t_User user { get; set; }

        //public string session_authtoken { get; set; }

        // public string session_sessiontoken { get; set; }
        [Column(TypeName = "jsonb")]
        public RefreshToken session_refreshtoken { get; set; }

        public DateTimeOffset session_loggedin { get; set; }

        public DateTimeOffset session_loggedout { get; set; }
    }
}
