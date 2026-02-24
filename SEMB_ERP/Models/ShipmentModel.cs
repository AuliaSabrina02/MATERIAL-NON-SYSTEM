using System.ComponentModel.DataAnnotations;
namespace SEMB_ERP.Models
{
    public class ShipmentModel
    {
        [Key]
        public int? id_shipment { get; set; }
        public string? id_upload { get; set; }
        public string? project_name { get; set; }
        public string? stage_name { get; set; }
        public string? wo_no { get; set; }
        public string? partno { get; set; }
        public string? revision { get; set; }
        public Decimal? qty { get; set; }
        public string? is_coated { get; set; }
        public DateTime? ship_date { get; set; }
        public string? status_shipment { get; set; }
        public string? inserted_by { get; set; }
        public string? inserted_by_name { get; set; }
        public string? pic_department { get; set; }
        public DateTime? received_date { get; set; }
        public string? received_by { get; set; }
        public string? received_by_name { get; set; }
        public string? storage_dest { get; set; }
        public string? gatepass_no { get; set; }
        public int? is_error { get; set; }
        public DateTime? closed_date { get; set; }
        public string? closed_by { get; set; }
        public string? closed_by_name { get; set; }
    }
}
