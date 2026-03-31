using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SEMB_ERP.Models
{
    [Table("pallet_problems")]
    public class PalletProblem
    {
        [Key]
        [Column("id")]
        public int id { get; set; }

        [Required]
        [Column("pallet_no")]
        [StringLength(100)]
        public string pallet_no { get; set; }

        [Required]
        [Column("req_no")]
        [StringLength(100)]
        public string req_no { get; set; }

        [Required]
        [Column("date")]
        public DateTime date { get; set; }

        [Required]
        [Column("issue")]
        public string issue { get; set; }

        [Column("created_by")]
        [StringLength(50)]
        public string created_by { get; set; }

        [Column("created_date")]
        public DateTime created_date { get; set; }

        [Column("updated_by")]
        [StringLength(50)]
        public string? updated_by { get; set; }

        [Column("updated_date")]
        public DateTime? updated_date { get; set; }

        [Column("status")]
        [StringLength(20)]
        public string status { get; set; }
    }
}