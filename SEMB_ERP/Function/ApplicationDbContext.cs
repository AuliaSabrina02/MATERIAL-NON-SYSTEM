using Microsoft.EntityFrameworkCore;
using SEMB_ERP.Models;

namespace SEMB_ERP.Function
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<OrderListModel> v_order { get; set; }
        public DbSet<RequestListModel> v_request { get; set; }
        public DbSet<NonConfModel> V_NON_CONF { get; set; }
        public DbSet<BlockBinModel> v_block_bin { get; set; }
        public DbSet<VarianceModel> v_picking_variance { get; set; }

    }
}
