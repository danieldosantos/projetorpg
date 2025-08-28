using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RpgRooms.Core.Entities;

namespace RpgRooms.Infrastructure;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<CampaignMember> CampaignMembers => Set<CampaignMember>();
    public DbSet<JoinRequest> JoinRequests => Set<JoinRequest>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Campaign>()
            .HasOne(c => c.OwnerUser)
            .WithMany(u => u.OwnedCampaigns)
            .HasForeignKey(c => c.OwnerUserId);

        modelBuilder.Entity<Campaign>()
            .HasCheckConstraint("CK_Campaign_MaxPlayers", "MaxPlayers <= 50");

        modelBuilder.Entity<CampaignMember>()
            .HasOne(cm => cm.Campaign)
            .WithMany(c => c.Members)
            .HasForeignKey(cm => cm.CampaignId);

        modelBuilder.Entity<CampaignMember>()
            .HasOne(cm => cm.User)
            .WithMany(u => u.CampaignMemberships)
            .HasForeignKey(cm => cm.UserId);

        modelBuilder.Entity<JoinRequest>()
            .HasOne(j => j.Campaign)
            .WithMany(c => c.JoinRequests)
            .HasForeignKey(j => j.CampaignId);

        modelBuilder.Entity<JoinRequest>()
            .HasOne(j => j.User)
            .WithMany(u => u.JoinRequests)
            .HasForeignKey(j => j.UserId);

        modelBuilder.Entity<ChatMessage>()
            .HasOne(m => m.Campaign)
            .WithMany(c => c.ChatMessages)
            .HasForeignKey(m => m.CampaignId);

        modelBuilder.Entity<ChatMessage>()
            .HasOne(m => m.User)
            .WithMany(u => u.ChatMessages)
            .HasForeignKey(m => m.UserId);

        modelBuilder.Entity<AuditEntry>()
            .HasOne(a => a.Campaign)
            .WithMany(c => c.AuditEntries)
            .HasForeignKey(a => a.CampaignId);

        modelBuilder.Entity<AuditEntry>()
            .HasOne(a => a.User)
            .WithMany(u => u.AuditEntries)
            .HasForeignKey(a => a.UserId);
    }
}
