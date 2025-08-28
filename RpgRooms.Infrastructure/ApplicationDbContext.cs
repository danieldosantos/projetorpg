using Microsoft.EntityFrameworkCore;
using RpgRooms.Core.Entities;

namespace RpgRooms.Infrastructure;

public class ApplicationDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Campaign>()
            .HasOne(c => c.GameMaster)
            .WithMany()
            .HasForeignKey(c => c.GameMasterId);

        modelBuilder.Entity<Campaign>()
            .HasMany(c => c.Players)
            .WithMany(u => u.Campaigns)
            .UsingEntity(j => j.ToTable("CampaignPlayers"));
    }
}
