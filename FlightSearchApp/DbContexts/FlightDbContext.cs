using FlightSearchApp.Models;
using Microsoft.EntityFrameworkCore;

namespace FlightSearchApp.DbContexts
{
    public class FlightDbContext : DbContext
    {
        public FlightDbContext(DbContextOptions options) : base(options) { }

        public DbSet<Flight> Flights { get; set; }
    }
}
