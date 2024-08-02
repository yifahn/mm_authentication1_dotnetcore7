using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SharedGameFramework.Game.Character;


namespace Database.Postgres.DbSchema
{
    public class t_Character
    {
        [Key]
        public int character_id { get; set; }

        [ForeignKey("kingdom")]
        public int fk_kingdom_id { get; set; }

        public t_Kingdom kingdom { get; set; }

        public t_Soupkitchen soupkitchen { get; set; }
        public int political_points { get; set; }

        public string character_name { get; set; }

        [Column(TypeName = "jsonb")]
        public Character_Inventory character_inventory { get; set; }

        [Column(TypeName = "jsonb")]
        public CharacterSheet character_sheet { get; set; }

        [Column(TypeName = "jsonb")]
        public CharacterState character_state { get; set; }
    }
}
