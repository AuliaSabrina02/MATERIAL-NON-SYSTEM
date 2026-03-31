using System.ComponentModel.DataAnnotations;
namespace SEMB_ERP.Models
{
    public class BlockBinDetailModel
    {
        public int? no { get; set; }
        public string partno { get; set; }
        public string sbin { get; set; }
        public DateTime? variance_date { get; set; }
    }
}
