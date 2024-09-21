using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SharedGameFramework.Game.Armoury;
using System.Diagnostics.CodeAnalysis;
using MM_API.Database.Postgres.DbSchema;

namespace MM_API.Database.Postgres.DbSchema
{
    public class t_Armoury
    {
        [Key]
        public int armoury_id { get; set; }

        public int fk_user_id { get; set; }

        [ForeignKey("fk_user_id")]
        public t_User User { get; set; }

        [Column(TypeName = "jsonb")]
        public string armoury_inventory { get; set; }
    }
}
//public int fk_kingdom_id { get; set; }

//[ForeignKey("fk_kingdom_id")]
//public t_Kingdom kingdom { get; set; }

//[AllowNull]
//[Column(TypeName = "jsonb")]
//public ArmouryInventory armoury_inventory { get; set; }