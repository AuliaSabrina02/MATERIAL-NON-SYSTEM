using System.ComponentModel.DataAnnotations;
namespace SEMB_ERP.Models
{
    public class UserDetailModel
    {
        [Key]
        public int? id_user { get; set; }
        public string? sesa_id { get; set; }
        public string? name { get; set; }
        public string? email { get; set; }
        public string? level { get; set; }
        public string? role { get; set; }
        public string? department { get; set; }
        public string? plant { get; set; }
        public string? manager_sesa_id { get; set; }
        public string? manager_name { get; set; }
        public string? manager_email { get; set; }
        public string? other_dept { get; set; }
    }
}
