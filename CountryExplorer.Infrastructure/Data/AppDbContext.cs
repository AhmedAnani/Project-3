using CountryExplorer.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CountryExplorer.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<TripBucketItem> TripBucketItems => Set<TripBucketItem>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Destination> Destinations => Set<Destination>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Role).HasConversion<string>();

            entity.HasQueryFilter(u => !u.IsDeleted);  
        });

        modelBuilder.Entity<TripBucketItem>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Status).HasConversion<string>();
            entity.Property(t => t.Notes).HasMaxLength(500);
            entity.HasIndex(t => new { t.UserId, t.CountryCode }); 


            entity.HasOne(t => t.User)
                  .WithMany(u => u.TripBucketItems)
                  .HasForeignKey(t => t.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasQueryFilter(t => !t.IsDeleted);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(r => r.Id);

            entity.HasOne(r => r.User)
                  .WithMany(u => u.RefreshTokens)
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasQueryFilter(r => !r.User.IsDeleted);

        });

        modelBuilder.Entity<Destination>(entity =>
        {
            entity.HasKey(d => d.Id);

            entity.Property(d => d.CountryCode)
                  .IsRequired()
                  .HasMaxLength(2);

            entity.Property(d => d.CityName)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(d => d.Description)
                  .HasMaxLength(200);

            // Store VibeTag flags as int for bitwise queries
            entity.Property(d => d.Tags)
                  .HasConversion<int>();

            // Prevent duplicate city+country combinations
            entity.HasIndex(d => new { d.CountryCode, d.CityName })
                  .IsUnique();
        });
    }
}