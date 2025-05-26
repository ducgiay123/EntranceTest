using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthenticationTest.src.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using AuthenticationTest.Core.src.Entities;
using AuthenticationTest.Core.src.Utilities;
using AuthenticationTest.Service.src.Services;

namespace AuthenticationTest.Infrastructure.Database
{
    public class AuthDbContext : DbContext
    {
        public DbSet<Users> User { get; set; }
        public DbSet<Tokens> Token { get; set; }

        public DbSet<Tasks> Tasks { get; set; }
        private readonly IConfiguration _configuration;

        public AuthDbContext(DbContextOptions<AuthDbContext> options, IConfiguration configuration)
            : base(options)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");

                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("PostgreSQL connection string is not configured in appsettings.json.");
                }

                optionsBuilder.UseNpgsql(connectionString);
            }

            base.OnConfiguring(optionsBuilder);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Users>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.FirstName).IsRequired();
                entity.Property(e => e.LastName).IsRequired();
                entity.Property(e => e.Email).IsRequired();
                entity.Property(e => e.Hash).IsRequired();
                entity.Property(e => e.CreatedAt);
                entity.Property(e => e.UpdatedAt);

                entity.HasIndex(e => e.Email).IsUnique();

                entity.HasMany(u => u.Tokens)       // Navigation: User has many Tokens
                      .WithOne(t => t.User)        // Token has one User
                      .HasForeignKey(t => t.UserId) // Foreign key is Token.UserId
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(u => u.Tasks)
                        .WithOne(t => t.User) // Assuming Task has a navigation property 'User' back to Users
                        .HasForeignKey(t => t.UserId) // Assuming Task has a foreign key 'UserId'
                        .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Tokens>(entity =>
            {
                entity.ToTable("Tokens");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.RefreshToken).IsRequired();
                entity.Property(e => e.ExpiresIn).IsRequired();
                entity.Property(e => e.CreatedAt);
                entity.Property(e => e.UpdatedAt);
            });

            modelBuilder.Entity<Tasks>(entity =>
            {
                entity.ToTable("Tasks");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Title)
                    .IsRequired()
                    ;

                entity.Property(e => e.Description)
                    .HasMaxLength(500);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasConversion<string>(); // Convert enum to string for PostgreSQL

                entity.Property(e => e.DueDate)
                    .IsRequired()
                    .HasColumnType("date");
            });
            string hashedPassword = AuthService.HashPasswordSync("trungdn123");
            modelBuilder.Entity<Users>().HasData(
                        new Users
                        {
                            Id = 1000, // Ensure this ID is unique and matches your database schema
                            FirstName = "Trung",
                            LastName = "Duc",
                            Email = "admin@gmail.com",
                            Hash = hashedPassword, // Replace with a securely hashed password
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            Role = UserRole.Admin, // Set the role to Admin
                                                   // No tasks are added here, as per your request
                                                   // Tokens collection will be empty by default
                        },
                // Additional User 1
                new Users
                {
                    Id = 1001, // Unique ID
                    FirstName = "Jane",
                    LastName = "Doe",
                    Email = "jane.doe@example.com",
                    Hash = hashedPassword, // Use the common hashed password
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Role = UserRole.User, // Assuming these are regular users
                },
                // Additional User 2
                new Users
                {
                    Id = 1002, // Unique ID
                    FirstName = "John",
                    LastName = "Smith",
                    Email = "john.smith@example.com",
                    Hash = hashedPassword, // Use the common hashed password
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Role = UserRole.User, // Assuming these are regular users
                },
                // Additional User 3
                new Users
                {
                    Id = 1003, // Unique ID
                    FirstName = "Alice",
                    LastName = "Johnson",
                    Email = "alice.j@example.com",
                    Hash = hashedPassword, // Use the common hashed password
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Role = UserRole.User, // Assuming these are regular users
                }
                    );
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());


        }


    }
}
