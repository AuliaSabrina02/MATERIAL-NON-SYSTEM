using System.ComponentModel.DataAnnotations;

namespace SEMB_ERP.Models
{
    public class MaterialReturnModel
    {
        public int id_return { get; set; }

        // Kolom Baru
        public string request_no { get; set; }
        public string project_name { get; set; }
        public string reason_return { get; set; }
        public string storage_requirement { get; set; }
        public string image_support { get; set; } // Menyimpan nama file gambar

        // Kolom Lama
        public string po_no { get; set; }
        public string partno { get; set; }
        public decimal qty_return { get; set; }
        public string uom { get; set; }
        public string condition { get; set; }
        public string pic { get; set; }
        public DateTime? return_date { get; set; }
        public DateTime? update_date { get; set; }
        public int? status { get; set; }
        public string status_desc { get; set; }
        public string name { get; set; }
    }
}