using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using RpgRooms.Infrastructure;
using RpgRooms.Core.Services;
using RpgRooms.Web.Hubs;

var builder = WebApplication.CreateBuilder(args);

var connection = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=rpgrooms.db";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite(connection);
    var provider = builder.Configuration.GetValue<string>("DatabaseProvider");
    if (provider == "SqlServer")
        options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection"));
});

builder.Services.AddSignalR();
builder.Services.AddScoped<CampaignService>();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("IsGameMaster", policy => policy.RequireClaim("IsGameMaster", "True"));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await DataSeeder.SeedAsync(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapHub<CampaignHub>("/campaignHub");
app.MapFallbackToPage("/_Host");

app.Run();
