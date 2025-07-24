using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Infrastructure.Data;

public class EmployeeDbContext : DbContext
{
    public EmployeeDbContext(DbContextOptions<EmployeeDbContext> options) : base(options)
    {
    }

    public DbSet<Employee> Employees { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Employee entity
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Position).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Department).IsRequired().HasMaxLength(150);
            entity.Property(e => e.HireDate).IsRequired();
            entity.Property(e => e.Salary).HasColumnType("decimal(18,2)");
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt);

            // Create indexes for performance
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Department);
            entity.HasIndex(e => e.Position);
            entity.HasIndex(e => e.IsActive);
        });

        // Seed data for cruise ship deployment
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        var employees = new List<Employee>
        {
            new Employee
            {
                Id = 1,
                FirstName = "Captain",
                LastName = "Smith",
                Email = "captain.smith@cruiseline.com",
                PhoneNumber = "+1-555-0101",
                Position = "Ship Captain",
                Department = "Navigation",
                HireDate = new DateTime(2020, 1, 15),
                Salary = 120000,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Employee
            {
                Id = 2,
                FirstName = "Sarah",
                LastName = "Johnson",
                Email = "sarah.johnson@cruiseline.com",
                PhoneNumber = "+1-555-0102",
                Position = "Chief Engineer",
                Department = "Engineering",
                HireDate = new DateTime(2019, 6, 10),
                Salary = 95000,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Employee
            {
                Id = 3,
                FirstName = "Mike",
                LastName = "Davis",
                Email = "mike.davis@cruiseline.com",
                PhoneNumber = "+1-555-0103",
                Position = "Security Chief",
                Department = "Security",
                HireDate = new DateTime(2021, 3, 20),
                Salary = 75000,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        modelBuilder.Entity<Employee>().HasData(employees);
    }
}
