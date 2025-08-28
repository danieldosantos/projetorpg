using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RpgRooms.Core.Entities;
using RpgRooms.Infrastructure;

namespace RpgRooms.Web.Hubs;

public class CampaignHub : Hub
{
    private readonly ApplicationDbContext _db;

    public CampaignHub(ApplicationDbContext db)
    {
        _db = db;
    }

    [Authorize(Policy = "IsMemberOfCampaign")]
    public async Task JoinCampaignGroup(int campaignId)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            return;

        var campaign = await _db.Campaigns
            .FirstOrDefaultAsync(c => c.Id == campaignId);

        if (campaign is null || campaign.Status == CampaignStatus.Finalized)
            return;

        await Groups.AddToGroupAsync(Context.ConnectionId, $"campaign-{campaignId}");
        var name = Context.User?.Identity?.Name ?? userId;
        await Clients.Group($"campaign-{campaignId}")
            .SendAsync("SystemNotice", $"{name} joined the campaign.");
    }

    public async Task LeaveCampaignGroup(int campaignId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"campaign-{campaignId}");
        var name = Context.User?.Identity?.Name ?? string.Empty;
        await Clients.Group($"campaign-{campaignId}")
            .SendAsync("SystemNotice", $"{name} left the campaign.");
    }

    [Authorize(Policy = "IsMemberOfCampaign")]
    public async Task SendMessage(int campaignId, string content, bool sentAsCharacter)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            return;

        var campaign = await _db.Campaigns
            .FirstOrDefaultAsync(c => c.Id == campaignId);

        if (campaign is null || campaign.Status == CampaignStatus.Finalized)
        {
            await Clients.Caller.SendAsync("SystemNotice", "Campaign has been finalized.");
            return;
        }

        var displayName = Context.User?.Identity?.Name ?? userId;
        if (sentAsCharacter)
            displayName = $"{displayName} (Character)";

        var message = new ChatMessage
        {
            CampaignId = campaignId,
            UserId = userId,
            DisplayName = displayName,
            Message = content,
            SentAt = DateTime.UtcNow
        };

        _db.ChatMessages.Add(message);
        await _db.SaveChangesAsync();

        await Clients.Group($"campaign-{campaignId}")
            .SendAsync("ReceiveMessage", displayName, content);
    }

    public async Task NotifyCampaignFinalized(int campaignId)
    {
        await Clients.Group($"campaign-{campaignId}")
            .SendAsync("SystemNotice", "Campaign has been finalized.");
    }
}
