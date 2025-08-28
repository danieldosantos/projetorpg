using Microsoft.EntityFrameworkCore;
using RpgRooms.Core.Entities;

namespace RpgRooms.Infrastructure;

public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        await context.Database.MigrateAsync();

        if (!context.Users.Any())
        {
            var admin = new User { UserName = "admin", PasswordHash = "admin", IsGameMaster = true };
            context.Users.Add(admin);

            var campaign = new Campaign { Name = "Sample Campaign", GameMaster = admin };
            context.Campaigns.Add(campaign);

            await context.SaveChangesAsync();
        }
    }
}
