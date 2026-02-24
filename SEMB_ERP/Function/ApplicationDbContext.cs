using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SEMB_ERP.Models;

namespace SEMB_ERP.Function
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<OrderListModel> v_order { get; set; }
        public DbSet<StoragebinListModel> v_order_sbin { get; set; }
        public DbSet<RequestListModel> v_request { get; set; }
        public DbSet<RequestDetailModel> v_request_detail { get; set; }
        public DbSet<NonConfModel> V_NON_CONF { get; set; }
        public DbSet<BlockBinModel> v_block_bin { get; set; }
        public DbSet<VarianceModel> v_picking_variance { get; set; }
        public DbSet<PalletModel> v_pallet_header { get; set; }
        public DbSet<AgingMovementModel> v_order_aging { get; set; }
        public DbSet<ShipmentModel> v_shipment { get; set; }

    }

  
}
