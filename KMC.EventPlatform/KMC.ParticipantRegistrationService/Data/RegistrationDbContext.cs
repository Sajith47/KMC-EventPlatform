using Microsoft.EntityFrameworkCore;
using KMC.ParticipantRegistrationService.Models;

namespace KMC.ParticipantRegistrationService.Data
{
    public class RegistrationDbContext : DbContext
    {
        public RegistrationDbContext(DbContextOptions<RegistrationDbContext> options) : base(options) { }

        public DbSet<Registration> Registrations => Set<Registration>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // A participant can only register once per event (matched by email).
            modelBuilder.Entity<Registration>()
                .HasIndex(r => new { r.EventId, r.ParticipantEmail })
                .IsUnique();
        }
    }
}
