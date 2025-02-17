using GLTest.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GLTest.Infrastructure.Persistence
{
  
        public class ApplicationDbContext : DbContext
        {
            public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
            public ApplicationDbContext() { }
            public DbSet<Company> Companies { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Company>()
                    .HasIndex(c => c.Isin)
                    .IsUnique(); // Enforce Unique ISIN

                modelBuilder.Entity<Company>()
                    .Property(c => c.Isin)
                    .HasMaxLength(12);
            }

        public override int SaveChanges()
        {
            UpdateAuditFields();
            return base.SaveChanges();
        }
   
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateAuditFields();
            return await base.SaveChangesAsync(cancellationToken);           
        }

        private void UpdateAuditFields()
        {
            foreach (var entry in ChangeTracker.Entries<Company>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.DateCreated = DateTime.UtcNow;
                }

                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.DateUpdated = DateTime.UtcNow;                  
                }
            }
        }

    }   
}
