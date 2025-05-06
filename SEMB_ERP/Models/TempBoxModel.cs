using System.ComponentModel.DataAnnotations;
namespace SEMB_ERP.Models
{
    public class TempBoxModel
    {
        public int? id_temp_box { get; set; }
        public string? box_id { get; set; }
        public decimal? qty { get; set; }
        public string? box_id_consol { get; set; }
    }
}
