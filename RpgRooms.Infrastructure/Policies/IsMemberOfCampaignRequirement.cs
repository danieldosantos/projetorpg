using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RpgRooms.Infrastructure;

namespace RpgRooms.Infrastructure.Policies;

public class IsMemberOfCampaignRequirement : IAuthorizationRequirement { }

public class IsMemberOfCampaignHandler : AuthorizationHandler<IsMemberOfCampaignRequirement>
{
    private readonly ApplicationDbContext _db;

    public IsMemberOfCampaignHandler(ApplicationDbContext db)
    {
        _db = db;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, IsMemberOfCampaignRequirement requirement)
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
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == campaignId.Value);

        if (campaign is null)
            return;

        if (campaign.Members.Any(m => m.UserId == userId))
            context.Succeed(requirement);
    }
}

