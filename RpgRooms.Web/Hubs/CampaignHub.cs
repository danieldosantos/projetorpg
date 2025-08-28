using System;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RpgRooms.Core.Entities;
using RpgRooms.Core.Services;
using RpgRooms.Infrastructure;

namespace RpgRooms.Web.Hubs;

public class CampaignHub : Hub
{
    private readonly CampaignService _campaignService;
    private readonly ApplicationDbContext _db;

    public CampaignHub(CampaignService campaignService, ApplicationDbContext db)
    {
        _campaignService = campaignService;
        _db = db;
    }

    public async Task JoinCampaignGroup(int campaignId)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            return;

        var campaign = await _db.Campaigns
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == campaignId);

        if (campaign is null || campaign.Status == CampaignStatus.Finalized)
            return;

        if (!_campaignService.IsMemberOfCampaign(campaign, userId))
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

    public async Task SendMessage(int campaignId, string content, bool sentAsCharacter)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
            return;

        var campaign = await _db.Campaigns
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == campaignId);

        if (campaign is null || campaign.Status == CampaignStatus.Finalized)
        {
            await Clients.Caller.SendAsync("SystemNotice", "Campaign has been finalized.");
            return;
        }

        if (!_campaignService.IsMemberOfCampaign(campaign, userId))
            return;

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
