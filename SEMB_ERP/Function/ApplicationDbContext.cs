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
        public DbSet<NonConfModel> V_NON_CONF { get; set; }

    }
}
