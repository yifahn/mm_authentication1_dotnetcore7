using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SharedGameFramework.Game.Armoury;
using System.Diagnostics.CodeAnalysis;
using MM_API.Database.Postgres.DbSchema;
using SharedGameFramework.Game.Armoury.Equipment;

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
        public string armoury_weapons { get; set; }

        [Column(TypeName = "jsonb")]
        public string armoury_armour { get; set; }

        [Column(TypeName = "jsonb")]
        public string armoury_jewellery { get; set; }


        //[Column(TypeName = "jsonb")]
        //public List<BaseWeapon> armoury_weapons { get; set; }

        //[Column(TypeName = "jsonb")]
        //public List<BaseArmour> armoury_armour { get; set; }

        //[Column(TypeName = "jsonb")]
        //public List<BaseJewellery> armoury_jewellery { get; set; }
    }
}
//public int fk_kingdom_id { get; set; }

//[ForeignKey("fk_kingdom_id")]
//public t_Kingdom kingdom { get; set; }

//[AllowNull]
//[Column(TypeName = "jsonb")]
//public EquipmentInventory armoury_inventory { get; set; }