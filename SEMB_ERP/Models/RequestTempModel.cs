using System.ComponentModel.DataAnnotations;
namespace SEMB_ERP.Models
{
    public class RequestTempModel
    {
        public int? id_temp { get; set; }
        public int? id_order { get; set; }
        public string? partno { get; set; }
        public decimal? qty { get; set; }
        public decimal? stock_qty { get; set; }
        public decimal? picked_qty { get; set; }
        public decimal? available_qty { get; set; }
        public string? status_stock { get; set; }
    }
}
