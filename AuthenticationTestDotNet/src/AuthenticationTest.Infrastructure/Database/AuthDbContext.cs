using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthenticationTest.src.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace AuthenticationTest.Infrastructure.Database
{
    public class AuthDbContext : DbContext
    {
        public DbSet<User> User { get; set; }
        public DbSet<Token> Token { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // ✅ REPLACE WITH YOUR ACTUAL MYSQL CONNECTION STRING
                var connectionString = "Server=localhost;Database=ShopWeb;User=root;Password=Trungdn123!;";

                optionsBuilder.UseMySql(connectionString,
                    ServerVersion.AutoDetect(connectionString));
            }

            base.OnConfiguring(optionsBuilder);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.FirstName);
                entity.Property(e => e.LastName);
                entity.Property(e => e.Email).IsRequired();
                entity.Property(e => e.Hash).IsRequired();
                entity.Property(e => e.CreatedAt);
                entity.Property(e => e.UpdatedAt);

                entity.HasIndex(e => e.Email).IsUnique();

                entity.HasMany(u => u.Tokens)       // Navigation: User has many Tokens
                      .WithOne(t => t.User)        // Token has one User
                      .HasForeignKey(t => t.UserId) // Foreign key is Token.UserId
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Token>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.RefreshToken).IsRequired();
                entity.Property(e => e.ExpiresIn).IsRequired();
                entity.Property(e => e.CreatedAt);
                entity.Property(e => e.UpdatedAt);
            });
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }


    }
}
