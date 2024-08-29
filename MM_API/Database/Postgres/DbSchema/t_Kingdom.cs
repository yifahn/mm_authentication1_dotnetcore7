using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SharedGameFramework.Game.Kingdom.Map;

namespace MM_API.Database.Postgres.DbSchema
{
    public class t_Kingdom
    {
        [Key]
        public int kingdom_id { get; set; }

        
        public int fk_user_id { get; set; }

        [ForeignKey("fk_user_id")]
        public t_User user { get; set; }

        public ICollection<t_Character> characters { get; set; }

        public t_Treasury treasury { get; set; }

        public t_Armoury armoury { get; set; }

        public string kingdom_name { get; set; }

        [Column(TypeName = "jsonb")]
        public Map kingdom_map { get; set; }
    }
}
