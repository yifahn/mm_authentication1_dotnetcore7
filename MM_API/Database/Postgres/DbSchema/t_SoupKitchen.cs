using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MM_API.Database.Postgres.DbSchema
{
    public class t_Soupkitchen
    {
        [Key]
        public int soupkitchen_id { get; set; }

        [ForeignKey("character")]
        public int fk_character_id { get; set; }

        public t_Character character { get; set; }
        public int soupkitchen_cooldown_days { get; set; }
        public int soupkitchen_cooldown_hours { get; set; }
        public int soupkitchen_cooldown_minutes { get; set; }
        public int soupkitchen_cooldown_seconds { get; set; }
    }
}
