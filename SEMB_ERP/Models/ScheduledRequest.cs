using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SEMB_ERP.Models
{
    [Table("scheduled_requests")]
    public class ScheduledRequest
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        // Pastikan nama di dalam [Column("...")] sesuai dengan nama kolom di tabel SQL Anda
        [Column("sesa_id")]
        public string SesaId { get; set; }

        [Column("user_email")]
        public string UserEmail { get; set; }

        [Column("scheduled_date")]
        public DateTime ScheduledDate { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("is_notified")]
        public bool IsNotified { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("is_completed")] // ✅ PASTIKAN INI ADA
        public bool? IsCompleted { get; set; }

        [Column("send_email")]
        public bool SendEmail { get; set; }
    }
}