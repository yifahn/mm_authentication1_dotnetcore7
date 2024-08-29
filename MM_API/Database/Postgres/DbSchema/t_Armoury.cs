using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SharedGameFramework.Game.Armoury;


namespace MM_API.Database.Postgres.DbSchema
{
    public class t_Armoury
    {
        [Key]
        public int armoury_id { get; set; }

        
        public int fk_kingdom_id { get; set; }

        [ForeignKey("fk_kingdom_id")]
        public t_Kingdom kingdom { get; set; }

        [Column(TypeName = "jsonb")]
        public Armoury_Inventory armoury_inventory { get; set; }
    }
}
