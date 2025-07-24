using Microsoft.EntityFrameworkCore;
using ShoreCommandCenter.Models;

namespace ShoreCommandCenter.Data;

public class FleetDbContext : DbContext
{
    public FleetDbContext(DbContextOptions<FleetDbContext> options) : base(options)
    {
    }

    public DbSet<Ship> Ships { get; set; }
    public DbSet<Deployment> Deployments { get; set; }
    public DbSet<FleetDeployment> FleetDeployments { get; set; }
    public DbSet<ShipMetrics> ShipMetrics { get; set; }
    public DbSet<UpdateRequest> UpdateRequests { get; set; }
    public DbSet<ApiKey> ApiKeys { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Ship configuration
        modelBuilder.Entity<Ship>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Offline");
            entity.Property(e => e.CurrentVersion).HasMaxLength(50);
            entity.Property(e => e.TargetVersion).HasMaxLength(50);
            entity.Property(e => e.Location).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.ConfigurationJson).HasColumnType("NVARCHAR(MAX)");

            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.LastSeen);
        });

        // Deployment configuration
        modelBuilder.Entity<Deployment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWID()");
            entity.Property(e => e.ShipId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ContainerImage).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Version).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Pending");
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.ErrorMessage).HasColumnType("NVARCHAR(MAX)");

            entity.HasOne<Ship>()
                .WithMany()
                .HasForeignKey(e => e.ShipId);

            entity.HasIndex(e => e.ShipId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ScheduledFor);
        });

        // Fleet Deployment configuration
        modelBuilder.Entity<FleetDeployment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWID()");
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ContainerImage).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Version).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Planning");
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.ShipFilter).HasColumnType("NVARCHAR(MAX)");

            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ScheduledFor);
        });

        // Ship Metrics configuration
        modelBuilder.Entity<ShipMetrics>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ShipId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.CpuPercent).HasPrecision(5, 2);
            entity.Property(e => e.MemoryPercent).HasPrecision(5, 2);
            entity.Property(e => e.DiskPercent).HasPrecision(5, 2);
            entity.Property(e => e.MetricsJson).HasColumnType("NVARCHAR(MAX)");

            entity.HasOne<Ship>()
                .WithMany()
                .HasForeignKey(e => e.ShipId);

            entity.HasIndex(e => e.ShipId);
            entity.HasIndex(e => e.Timestamp);
        });

        // Update Request configuration
        modelBuilder.Entity<UpdateRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWID()");
            entity.Property(e => e.ShipId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.PackageUrl).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.Version).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Priority).HasMaxLength(20).HasDefaultValue("Normal");
            entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Pending");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.Description).HasMaxLength(500);

            entity.HasOne<Ship>()
                .WithMany()
                .HasForeignKey(e => e.ShipId);

            entity.HasIndex(e => e.ShipId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Priority);
        });

        // API Key configuration
        modelBuilder.Entity<ApiKey>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("NEWID()");
            entity.Property(e => e.Key).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ShipId).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => e.Key).IsUnique();
            entity.HasIndex(e => e.ShipId);
        });

        // Seed initial data
        modelBuilder.Entity<Ship>().HasData(
            new Ship
            {
                Id = "OCEAN_EXPLORER_001",
                Name = "Ocean Explorer",
                Status = "Offline",
                Location = "Caribbean",
                ConfigurationJson = """{"timezone": "UTC-4", "maintenanceWindows": [{"start": "02:00", "end": "06:00", "days": ["Sunday", "Wednesday"]}]}""",
                CreatedAt = DateTime.UtcNow
            },
            new Ship
            {
                Id = "SEA_ADVENTURE_002",
                Name = "Sea Adventure",
                Status = "Offline",
                Location = "Mediterranean",
                ConfigurationJson = """{"timezone": "UTC+1", "maintenanceWindows": [{"start": "03:00", "end": "07:00", "days": ["Sunday", "Wednesday"]}]}""",
                CreatedAt = DateTime.UtcNow
            },
            new Ship
            {
                Id = "ATLANTIC_VOYAGER_003",
                Name = "Atlantic Voyager",
                Status = "Offline",
                Location = "Atlantic",
                ConfigurationJson = """{"timezone": "UTC-3", "maintenanceWindows": [{"start": "01:00", "end": "05:00", "days": ["Sunday", "Wednesday"]}]}""",
                CreatedAt = DateTime.UtcNow
            }
        );
    }
}
