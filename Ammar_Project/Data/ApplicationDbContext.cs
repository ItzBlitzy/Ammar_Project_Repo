using Ammar_Project.Models;
using Microsoft.EntityFrameworkCore;

namespace Ammar_Project.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) 
        { 
        }

        public DbSet<Booking>? Bookings { get; set; }
    }
}
