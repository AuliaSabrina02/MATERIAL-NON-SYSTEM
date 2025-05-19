using System.ComponentModel.DataAnnotations;
namespace SEMB_ERP.Models
{
    public class TempPalletModel
    {
        public int? id_temp { get; set; }
        public string? pallet_no { get; set; }
        public string? request_no { get; set; }
        public string? sesa_id { get; set; }
    }
}
