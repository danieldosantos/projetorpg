using RpgRooms.Core.Entities;
using System;
using System.Linq;

namespace RpgRooms.Core.Services;

public class CampaignService
{
    public void AddPlayer(Campaign campaign, ApplicationUser player)
    {
        if (campaign.Status == CampaignStatus.Finalized)
            throw new InvalidOperationException("Campaign is finished.");

        if (campaign.Members.Count >= campaign.MaxPlayers)
            throw new InvalidOperationException("Campaign reached maximum players.");

        if (campaign.Members.Any(m => m.UserId == player.Id))
            return;

        campaign.Members.Add(new CampaignMember
        {
            CampaignId = campaign.Id,
            UserId = player.Id,
            User = player,
            JoinedAt = DateTime.UtcNow
        });
    }

    public void FinalizeCampaign(Campaign campaign, ApplicationUser user)
    {
        if (campaign.OwnerUserId != user.Id)
            throw new UnauthorizedAccessException("Only owner can finalize.");

        campaign.Status = CampaignStatus.Finalized;
        campaign.FinalizedAt = DateTime.UtcNow;
    }
}
