using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;

namespace MM_API.Database.Postgres.DbSchema
{
    public class t_Treasury
    {
        [Key]
        public int treasury_id { get; set; }

        public int fk_kingdom_id { get; set; }

        [ForeignKey("fk_kingdom_id")]
        public t_Kingdom kingdom { get; set; }

        public BigInteger treasury_coin { get; set; }

        public double treasury_gainrate { get; set; }

        public double treasury_multiplier { get; set; }
    }
}
