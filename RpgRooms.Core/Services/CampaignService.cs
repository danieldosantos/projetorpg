using RpgRooms.Core.Entities;
using System.Linq;

namespace RpgRooms.Core.Services;

public class CampaignService
{
    public const int MaxPlayers = 50;

    public void AddPlayer(Campaign campaign, User player)
    {
        if (campaign.IsFinished)
            throw new InvalidOperationException("Campaign is finished.");

        if (campaign.Players.Count >= MaxPlayers)
            throw new InvalidOperationException("Campaign reached maximum players.");

        if (campaign.Players.Any(p => p.Id == player.Id))
            return;

        campaign.Players.Add(player);
    }

    public void FinalizeCampaign(Campaign campaign, User user)
    {
        if (campaign.GameMasterId != user.Id)
            throw new UnauthorizedAccessException("Only GM can finalize.");

        campaign.IsFinished = true;
    }
}
