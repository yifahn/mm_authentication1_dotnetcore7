using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

using MM_API.Database.Postgres.DbSchema;

using MonoMonarchGameFramework.Game.Treasury;
using MonoMonarchGameFramework.Game.Treasury.Currency;

namespace MM_API.Database.Postgres.DbSchema
{
    public class t_Treasury
    {
        [Key]
        public int treasury_id { get; set; }

        public int fk_user_id { get; set; }

        [ForeignKey("fk_user_id")]
        public t_User User { get; set; }



        [Column(TypeName = "jsonb")] 
        public string treasury_state { get; set; } // why is this null?
        public long treasury_total { get; set; }
        public DateTimeOffset treasury_updated_at_datetime { get; set; }
        public int treasury_updated_at_as_gametick { get; set; }
        
    }
}


//public decimal treasury_coin { get; set; } //stored in server as exponent notation, displayed to user as suffix notation

//public decimal treasury_gainrate { get; set; }

//public decimal treasury_multiplier { get; set; }
//[Key]
//public int treasury_id { get; set; }

//public int fk_kingdom_id { get; set; }

//[ForeignKey("fk_kingdom_id")]
//public t_Kingdom kingdom { get; set; }

//public BigInteger treasury_coin { get; set; }

//public double treasury_gainrate { get; set; }

//public double treasury_multiplier { get; set; }