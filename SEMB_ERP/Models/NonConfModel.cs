using System.ComponentModel.DataAnnotations;
namespace SEMB_ERP.Models
{
    public class NonConfModel
    {
        [Key]
        public int? id_non_conf { get; set; }
        public string? material_type { get; set; }
        public string? partno { get; set; }
        public string? po_no { get; set; }
        public decimal? qty { get; set; }
        public string? uom { get; set; }
        public string? supplier_name { get; set; }
        public string? pic { get; set; }
        public string? category_issue { get; set; }
        public string? detail_issue { get; set; }
        public string? file_doc { get; set; }
        public string? created_by { get; set; }
        public int? is_close { get; set; }
        public DateTime? close_date { get; set; }
        public string? closed_by { get; set; }
        public string? close_comment { get; set; }
        public string? pic_sesa { get; set; }
        public string? created_by_sesa { get; set; }
    }
}