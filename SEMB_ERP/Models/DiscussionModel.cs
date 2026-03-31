using System.ComponentModel.DataAnnotations;
namespace SEMB_ERP.Models
{
    public class DiscussionModel
    {
        public int Id { get; set; }
        public int IdOrder { get; set; }
        public string SesaId { get; set; }
        public string Message { get; set; }
        public int? ParentId { get; set; } // untuk reply
        public DateTime CreatedDate { get; set; }
        public string UserName { get; set; }
    }
}
