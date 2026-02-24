using System.ComponentModel.DataAnnotations;
namespace SEMB_ERP.Models
{
    public class ShipmentTempModel
    {
        public string? id_upload { get; set; }
        public string? project_name { get; set; }
        public string? project_name_msg { get; set; }
        public string? stage_name { get; set; }
        public string? stage_name_msg { get; set; }
        public string? wo_no { get; set; }
        public string? wo_no_msg { get; set; }
        public string? partno { get; set; }
        public string? partno_msg { get; set; }
        public string? revision { get; set; }
        public string? revision_msg { get; set; }
        public string? qty { get; set; }
        public string? qty_msg { get; set; }
        public string? is_coated { get; set; }
        public string? is_coated_msg { get; set; }
        public string? ship_date { get; set; }
        public string? ship_date_msg { get; set; }
        public int? is_error { get; set; }
    }
}
