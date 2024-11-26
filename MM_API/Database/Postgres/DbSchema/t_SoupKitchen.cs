using MM_API.Database.Postgres.DbSchema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MM_API.Database.Postgres.DbSchema
{
    public class t_Soupkitchen
    {
        [Key]
        public int soupkitchen_id { get; set; }

        public int fk_user_id { get; set; }

        [ForeignKey("fk_user_id")]
        public t_User User { get; set; }
        public DateTimeOffset soupkitchen_updated_at_datetime { get; set; }
        public int soupkitchen_updated_at_as_gametick { get; set; }

        [Column(TypeName = "jsonb")]
        public string soupkitchen_state { get; set; }
    }
}


//[Key]
//public int soupkitchen_id { get; set; }

//public int fk_character_id { get; set; }

//[ForeignKey("fk_character_id")]
//public t_Character character { get; set; }

//public int soupkitchen_cooldown_days { get; set; }
//public int soupkitchen_cooldown_hours { get; set; }
//public int soupkitchen_cooldown_minutes { get; set; }
//public int soupkitchen_cooldown_seconds { get; set; }