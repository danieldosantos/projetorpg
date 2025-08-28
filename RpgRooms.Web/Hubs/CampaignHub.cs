using Microsoft.AspNetCore.SignalR;

namespace RpgRooms.Web.Hubs;

public class CampaignHub : Hub
{
    public async Task SendMessage(int campaignId, string user, string message)
    {
        await Clients.Group($"campaign-{campaignId}").SendAsync("ReceiveMessage", user, message);
    }

    public async Task JoinCampaign(int campaignId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"campaign-{campaignId}");
    }
}
