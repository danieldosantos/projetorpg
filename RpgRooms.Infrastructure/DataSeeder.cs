using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using RpgRooms.Core.Entities;
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

            var campaign = new Campaign { Name = "Sample Campaign", GameMasterId = admin.Id, GameMaster = admin };
            context.Campaigns.Add(campaign);

            await context.SaveChangesAsync();
        }
    }
}
