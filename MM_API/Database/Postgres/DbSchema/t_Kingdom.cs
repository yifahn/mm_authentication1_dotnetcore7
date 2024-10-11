using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using MM_API.Database.Postgres.DbSchema;
using SharedGameFramework.Game.Kingdom.Map;
using SharedGameFramework.Game.Kingdom.Map.BaseNode;

namespace MM_API.Database.Postgres.DbSchema
{
    public class t_Kingdom
    {//NodeType int representations GL==0, TC==1, H==2, L==3, F==4, R==5, B==6, MT==7, W==8
        [Key]
        public int kingdom_id { get; set; }
        public string kingdom_name { get; set; }

        public int fk_user_id { get; set; }

        [ForeignKey("fk_user_id")]
        public t_User User { get; set; }

        [Column(TypeName = "jsonb")]
        public string kingdom_map { get; set; }

        public int[] kingdom_num_node_types { get; set; }
    }
}
