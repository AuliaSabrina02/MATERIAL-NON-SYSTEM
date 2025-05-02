using System.ComponentModel.DataAnnotations;
namespace SEMB_ERP.Models
{
    public class BinModel
    {
        public string? BinId { get; set; }
        public string? BinName { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
    }
}