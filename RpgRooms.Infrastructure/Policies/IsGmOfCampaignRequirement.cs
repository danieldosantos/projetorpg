using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RpgRooms.Infrastructure;

namespace RpgRooms.Infrastructure.Policies;

public class IsGmOfCampaignRequirement : IAuthorizationRequirement { }

public class IsGmOfCampaignHandler : AuthorizationHandler<IsGmOfCampaignRequirement>
{
    private readonly ApplicationDbContext _db;

    public IsGmOfCampaignHandler(ApplicationDbContext db)
    {
        _db = db;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, IsGmOfCampaignRequirement requirement)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            return;

        int? campaignId = null;

        if (context.Resource is HttpContext httpContext)
        {
            if (httpContext.Request.RouteValues.TryGetValue("id", out var value) && int.TryParse(value?.ToString(), out var id))
                campaignId = id;
        }
        else if (context.Resource is HubInvocationContext hubContext)
        {
            if (hubContext.HubMethodArguments.FirstOrDefault() is int id)
                campaignId = id;
        }

        if (campaignId is null)
            return;

        var campaign = await _db.Campaigns
            .FirstOrDefaultAsync(c => c.Id == campaignId.Value);

        if (campaign?.OwnerUserId == userId)
            context.Succeed(requirement);
    }
}

