using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace User.Infrastructure.Data
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        {
        }

        public DbSet<Domain.Entities.User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Domain.Entities.User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.IsActive).IsRequired();

                entity.HasIndex(e => e.Email).IsUnique();

                // Seed data
                entity.HasData(
     new Domain.Entities.User
     {
         Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
         Email = "admin@test.com",
         FirstName = "Admin",
         LastName = "User",
         PhoneNumber = "+1234567890",
         IsActive = true,
         CreatedAt = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc),
         UpdatedAt = null,
         KeycloakUserId = null
     },
     new Domain.Entities.User
     {
         Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
         Email = "user@test.com",
         FirstName = "Regular",
         LastName = "User",
         PhoneNumber = "+1234567891",
         IsActive = true,
         CreatedAt = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc),
         UpdatedAt = null,
         KeycloakUserId = null
     }
 );

            });
        }
    }
}
