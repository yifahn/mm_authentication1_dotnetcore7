using SharedNetworkFramework.Authentication.Firebase.RefreshToken;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace MM_API.Database.Postgres.DbSchema
{
    public class t_Session
    {
        [Key]
        public int session_id { get; set; }


        public int fk_user_id { get; set; }

        [ForeignKey("fk_user_id")]
        public t_User User { get; set; }


        //[AllowNull]
        [Column(TypeName = "jsonb")]
        public string refreshtoken { get; set; }


        //[AllowNull]
        public DateTimeOffset session_loggedin { get; set; }
        //[AllowNull]
        public DateTimeOffset session_loggedout { get; set; }
    }
}



//[Column(TypeName = "jsonb")]
//public RefreshToken session_refreshtoken { get; set; }