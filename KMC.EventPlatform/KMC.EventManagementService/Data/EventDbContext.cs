using Microsoft.EntityFrameworkCore;
using KMC.EventManagementService.Models;

namespace KMC.EventManagementService.Data
{
    public class EventDbContext : DbContext
    {
        public EventDbContext(DbContextOptions<EventDbContext> options) : base(options) { }

        public DbSet<Event> Events => Set<Event>();
    }
}
