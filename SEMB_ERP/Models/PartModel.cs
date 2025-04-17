using System.ComponentModel.DataAnnotations;
namespace SEMB_ERP.Models
{
    public class PartModel
    {
        public string? partno { get; set; }
        public string? sbin { get; set; }
        public string? partname { get; set; }
        public string? qty { get; set; }
        public string? status { get; set; }
    }
}
