using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using RpgRooms.Core.Entities;
using System.Collections.Generic;
using System.Linq;

namespace RpgRooms.Infrastructure;

public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IHostEnvironment env)
    {
        await context.Database.MigrateAsync();

        if (!env.IsDevelopment())
            return;

        if (!userManager.Users.Any())
        {
            var admin = new ApplicationUser { UserName = "admin", IsGameMaster = true };
            await userManager.CreateAsync(admin, "admin");

            var players = new List<ApplicationUser>();
            for (int i = 1; i <= 10; i++)
            {
                var player = new ApplicationUser { UserName = $"player{i}" };
                await userManager.CreateAsync(player, $"player{i}");
                players.Add(player);
            }

            var recruiting = new Campaign
            {
                Name = "Recruiting Campaign",
                Description = "Campaign currently recruiting",
                OwnerUserId = admin.Id,
                OwnerUser = admin,
                Status = CampaignStatus.Active,
                IsRecruiting = true,
                MaxPlayers = 5,
                CreatedAt = DateTime.UtcNow
            };
            context.Campaigns.Add(recruiting);
            context.CampaignMembers.AddRange(
                new CampaignMember { Campaign = recruiting, User = admin, UserId = admin.Id },
                new CampaignMember { Campaign = recruiting, User = players[0], UserId = players[0].Id }
            );
            context.AuditEntries.Add(new AuditEntry { Campaign = recruiting, User = admin, UserId = admin.Id, Action = "Campaign created" });
            var recruitingMessages = new[]
            {
                new ChatMessage { Campaign = recruiting, User = admin, UserId = admin.Id, DisplayName = admin.UserName!, Message = "Welcome to the recruiting campaign!" },
                new ChatMessage { Campaign = recruiting, User = players[0], UserId = players[0].Id, DisplayName = players[0].UserName!, Message = "Glad to join!" },
                new ChatMessage { Campaign = recruiting, User = admin, UserId = admin.Id, DisplayName = admin.UserName!, Message = "Looking for one more player." }
            };
            context.ChatMessages.AddRange(recruitingMessages);
            foreach (var m in recruitingMessages)
                context.AuditEntries.Add(new AuditEntry { Campaign = recruiting, User = m.User, UserId = m.UserId, Action = $"Chat: {m.Message}" });

            var inProgress = new Campaign
            {
                Name = "In Progress Campaign",
                Description = "Campaign already in progress",
                OwnerUserId = admin.Id,
                OwnerUser = admin,
                Status = CampaignStatus.Active,
                IsRecruiting = false,
                MaxPlayers = 10,
                CreatedAt = DateTime.UtcNow
            };
            context.Campaigns.Add(inProgress);
            context.CampaignMembers.Add(new CampaignMember { Campaign = inProgress, User = admin, UserId = admin.Id });
            for (int i = 0; i < 9; i++)
            {
                var user = players[i];
                context.CampaignMembers.Add(new CampaignMember { Campaign = inProgress, User = user, UserId = user.Id });
            }
            context.AuditEntries.Add(new AuditEntry { Campaign = inProgress, User = admin, UserId = admin.Id, Action = "Campaign created" });
            var inProgressMessages = new[]
            {
                new ChatMessage { Campaign = inProgress, User = admin, UserId = admin.Id, DisplayName = admin.UserName!, Message = "Session starting soon." },
                new ChatMessage { Campaign = inProgress, User = players[1], UserId = players[1].Id, DisplayName = players[1].UserName!, Message = "Can't wait!" },
                new ChatMessage { Campaign = inProgress, User = players[2], UserId = players[2].Id, DisplayName = players[2].UserName!, Message = "I'm ready." }
            };
            context.ChatMessages.AddRange(inProgressMessages);
            foreach (var m in inProgressMessages)
                context.AuditEntries.Add(new AuditEntry { Campaign = inProgress, User = m.User, UserId = m.UserId, Action = $"Chat: {m.Message}" });

            var finalized = new Campaign
            {
                Name = "Finalized Campaign",
                Description = "Campaign that has concluded",
                OwnerUserId = admin.Id,
                OwnerUser = admin,
                Status = CampaignStatus.Finalized,
                IsRecruiting = false,
                MaxPlayers = 5,
                CreatedAt = DateTime.UtcNow,
                FinalizedAt = DateTime.UtcNow
            };
            context.Campaigns.Add(finalized);
            context.CampaignMembers.AddRange(
                new CampaignMember { Campaign = finalized, User = admin, UserId = admin.Id },
                new CampaignMember { Campaign = finalized, User = players[8], UserId = players[8].Id },
                new CampaignMember { Campaign = finalized, User = players[9], UserId = players[9].Id }
            );
            context.AuditEntries.Add(new AuditEntry { Campaign = finalized, User = admin, UserId = admin.Id, Action = "Campaign created" });
            var finalizedMessages = new[]
            {
                new ChatMessage { Campaign = finalized, User = admin, UserId = admin.Id, DisplayName = admin.UserName!, Message = "Thanks for playing!" },
                new ChatMessage { Campaign = finalized, User = players[8], UserId = players[8].Id, DisplayName = players[8].UserName!, Message = "Great campaign!" },
                new ChatMessage { Campaign = finalized, User = players[9], UserId = players[9].Id, DisplayName = players[9].UserName!, Message = "See you next time." }
            };
            context.ChatMessages.AddRange(finalizedMessages);
            foreach (var m in finalizedMessages)
                context.AuditEntries.Add(new AuditEntry { Campaign = finalized, User = m.User, UserId = m.UserId, Action = $"Chat: {m.Message}" });

            await context.SaveChangesAsync();
        }
    }
}
