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
        public double? qty { get; set; }
        public string? uom { get; set; }
        public string? supplier_name { get; set; }
        public string? pic { get; set; }
        public string? category_issue { get; set; }
        public string? detail_issue { get; set; }
        public string? file_doc { get; set; }
        public string? created_by { get; set; }
    }
}
