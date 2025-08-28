using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RpgRooms.Core.Entities;
using RpgRooms.Core.Services;
using RpgRooms.Core.Policies;
using RpgRooms.Infrastructure;
using RpgRooms.Web.Hubs;
using System.Security.Claims;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

var connection = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=rpgrooms.db";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite(connection);
    var provider = builder.Configuration.GetValue<string>("DatabaseProvider");
    if (provider == "SqlServer")
        options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection"));
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddSignalR();
builder.Services.AddScoped<CampaignService>();
builder.Services.AddScoped<IAuthorizationHandler, IsGmOfCampaignHandler>();
builder.Services.AddScoped<IAuthorizationHandler, IsMemberOfCampaignHandler>();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("IsGameMaster", policy => policy.RequireClaim("IsGameMaster", "True"));
    options.AddPolicy("IsGmOfCampaign", policy => policy.Requirements.Add(new IsGmOfCampaignRequirement()));
    options.AddPolicy("IsMemberOfCampaign", policy => policy.Requirements.Add(new IsMemberOfCampaignRequirement()));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    await DataSeeder.SeedAsync(db, userManager, app.Environment);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/api/campaigns/{id}/join-requests", async (int id, JoinRequestDto dto, UserManager<ApplicationUser> userManager, CampaignService service, ApplicationDbContext db, ClaimsPrincipal principal) =>
{
    var user = await userManager.GetUserAsync(principal);
    if (user is null)
        return Results.Unauthorized();

    var campaign = await db.Campaigns
        .Include(c => c.Members)
        .Include(c => c.JoinRequests)
        .FirstOrDefaultAsync(c => c.Id == id);

    if (campaign is null)
        return Results.NotFound();

    try
    {
        service.RequestToJoin(campaign, user, dto.Message);
        await db.SaveChangesAsync();
        return Results.Ok();
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Forbid();
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
}).RequireAuthorization();

app.MapPost("/api/campaigns/{id}/join-requests/{requestId}/approve", async (int id, int requestId, UserManager<ApplicationUser> userManager, CampaignService service, ApplicationDbContext db, ClaimsPrincipal principal) =>
{
    var user = await userManager.GetUserAsync(principal);
    if (user is null)
        return Results.Unauthorized();

    var campaign = await db.Campaigns
        .Include(c => c.Members)
        .Include(c => c.JoinRequests)
        .ThenInclude(r => r.User)
        .FirstOrDefaultAsync(c => c.Id == id);

    if (campaign is null)
        return Results.NotFound();

    var request = campaign.JoinRequests.FirstOrDefault(r => r.Id == requestId);
    if (request is null)
        return Results.NotFound();

    try
    {
        service.ApproveRequest(campaign, request, user);
        await db.SaveChangesAsync();
        return Results.Ok();
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Forbid();
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
}).RequireAuthorization("IsGmOfCampaign");

app.MapPost("/api/campaigns/{id}/join-requests/{requestId}/reject", async (int id, int requestId, UserManager<ApplicationUser> userManager, CampaignService service, ApplicationDbContext db, ClaimsPrincipal principal) =>
{
    var user = await userManager.GetUserAsync(principal);
    if (user is null)
        return Results.Unauthorized();

    var campaign = await db.Campaigns
        .Include(c => c.JoinRequests)
        .FirstOrDefaultAsync(c => c.Id == id);

    if (campaign is null)
        return Results.NotFound();

    var request = campaign.JoinRequests.FirstOrDefault(r => r.Id == requestId);
    if (request is null)
        return Results.NotFound();

    try
    {
        service.RejectRequest(campaign, request, user);
        await db.SaveChangesAsync();
        return Results.Ok();
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Forbid();
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
}).RequireAuthorization("IsGmOfCampaign");

app.MapDelete("/api/campaigns/{id}/members/{userId}", async (int id, string userId, UserManager<ApplicationUser> userManager, CampaignService service, ApplicationDbContext db, ClaimsPrincipal principal) =>
{
    var user = await userManager.GetUserAsync(principal);
    if (user is null)
        return Results.Unauthorized();

    var campaign = await db.Campaigns
        .Include(c => c.Members)
        .FirstOrDefaultAsync(c => c.Id == id);

    if (campaign is null)
        return Results.NotFound();

    try
    {
        service.RemoveMember(campaign, userId, user);
        await db.SaveChangesAsync();
        return Results.Ok();
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Forbid();
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
}).RequireAuthorization("IsGmOfCampaign");

app.MapPost("/api/campaigns/{id}/recruitment-toggle", async (int id, UserManager<ApplicationUser> userManager, CampaignService service, ApplicationDbContext db, ClaimsPrincipal principal) =>
{
    var user = await userManager.GetUserAsync(principal);
    if (user is null)
        return Results.Unauthorized();

    var campaign = await db.Campaigns
        .Include(c => c.Members)
        .FirstOrDefaultAsync(c => c.Id == id);

    if (campaign is null)
        return Results.NotFound();

    try
    {
        service.ToggleRecruitment(campaign, user);
        await db.SaveChangesAsync();
        return Results.Ok(new { campaign.IsRecruiting });
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Forbid();
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
}).RequireAuthorization("IsGmOfCampaign");

app.MapBlazorHub();
app.MapHub<CampaignHub>("/campaignHub");
app.MapFallbackToPage("/_Host");

app.Run();

record JoinRequestDto(string Message);
