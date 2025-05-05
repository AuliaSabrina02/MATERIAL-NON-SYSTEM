using System.ComponentModel.DataAnnotations;
namespace SEMB_ERP.Models
{
    public class PickingModel
    {
        public int? id_request { get; set; }
        public int? id_det { get; set; }
        public string? partno { get; set; }
        public string? sbin { get; set; }
        public decimal? req_qty { get; set; }
        public decimal? temp_qty { get; set; }
    }
}
