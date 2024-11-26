using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using System.Diagnostics.CodeAnalysis;
using MM_API.Database.Postgres.DbSchema;
using MonoMonarchGameFramework.Game.Armoury.Equipment;
using MonoMonarchGameFramework.Game.Character.Attribute;
using MonoMonarchGameFramework.Game.Character;

namespace MM_API.Database.Postgres.DbSchema
{
    public class t_Character
    {
        [Key]
        public int character_id { get; set; }
        public int fk_user_id { get; set; }
        [ForeignKey("fk_user_id")]
        public t_User User { get; set; }


        public string character_name { get; set; }
        public bool character_isalive { get; set; }


        [Column(TypeName = "jsonb")]
        public string character_state { get; set; }


        [Column(TypeName = "jsonb")]
        public string character_attributes { get; set; }


        [Column(TypeName = "jsonb")]
        public string character_weapons { get; set; }

        [Column(TypeName = "jsonb")]
        public string character_armour { get; set; }

        [Column(TypeName = "jsonb")]
        public string character_jewellery { get; set; }
    }
}

//[Column(TypeName = "jsonb")]
//public List<BaseAttribute> character_attributes { get; set; }


//[Column(TypeName = "jsonb")]
//public List<BaseWeapon> character_weapons { get; set; }

//[Column(TypeName = "jsonb")]
//public List<BaseArmour> character_armour { get; set; }

//[Column(TypeName = "jsonb")]
//public List<BaseJewellery> character_jewellery { get; set; }


//VALUES OF EQUIPMENT ARRAY IS NULL IN CHARACTER SERVICE CLASS IM TESTING? MY THOUGHTS ARE BECAUSE THIS IS ABSTRACT CLASS - CANNOT INSTANTIATE CONCRETE CLASS OBJECT, BUT ATTRIBUTE ISN'T NULL? investigate further...

//[Key]
//public int character_id { get; set; }
//public string character_name { get; set; }


//public int fk_kingdom_id { get; set; }

//[ForeignKey("fk_kingdom_id")]
//public t_Kingdom kingdom { get; set; }

//public t_Soupkitchen soupkitchen { get; set; }
//public int political_points { get; set; }
//[AllowNull]
//[Column(TypeName = "jsonb")]
//public CharacterInventory character_inventory { get; set; } //= new CharacterInventory();

//[Column(TypeName = "jsonb")]
//public CharacterSheet character_attributes { get; set; }

//[Column(TypeName = "jsonb")]
//public CharacterState character_state { get; set; }