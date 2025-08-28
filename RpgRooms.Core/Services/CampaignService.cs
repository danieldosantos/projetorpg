using RpgRooms.Core.Entities;
using System;
using System.Linq;

namespace RpgRooms.Core.Services;

public class CampaignService
{
    public bool IsMemberOfCampaign(Campaign campaign, string userId)
        => campaign.Members.Any(m => m.UserId == userId);

    public void AddPlayer(Campaign campaign, ApplicationUser player)
    {
        if (campaign.Status == CampaignStatus.Finalized)
            throw new InvalidOperationException("Campaign is finished.");

        if (campaign.MaxPlayers > 50)
            throw new InvalidOperationException("Max players cannot exceed 50.");

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

        if (campaign.Members.Count >= campaign.MaxPlayers || campaign.Members.Count >= 50)
            campaign.IsRecruiting = false;
    }

    public void FinalizeCampaign(Campaign campaign, ApplicationUser user)
    {
        if (campaign.OwnerUserId != user.Id)
            throw new UnauthorizedAccessException("Only owner can finalize.");

        campaign.Status = CampaignStatus.Finalized;
        campaign.FinalizedAt = DateTime.UtcNow;
    }

    public JoinRequest RequestToJoin(Campaign campaign, ApplicationUser user, string message)
    {
        if (!campaign.IsRecruiting)
            throw new InvalidOperationException("Campaign is not recruiting.");

        if (campaign.MaxPlayers > 50)
            throw new InvalidOperationException("Max players cannot exceed 50.");

        if (campaign.Members.Any(m => m.UserId == user.Id))
            throw new InvalidOperationException("User already a member.");

        if (campaign.JoinRequests.Any(r => r.UserId == user.Id && r.Status == JoinRequestStatus.Pending))
            throw new InvalidOperationException("Join request already pending.");

        var request = new JoinRequest
        {
            CampaignId = campaign.Id,
            UserId = user.Id,
            User = user,
            Message = message,
            Status = JoinRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        campaign.JoinRequests.Add(request);
        return request;
    }

    public void ApproveRequest(Campaign campaign, JoinRequest request, ApplicationUser user)
    {
        if (campaign.OwnerUserId != user.Id)
            throw new UnauthorizedAccessException("Only owner can approve.");

        if (request.CampaignId != campaign.Id)
            throw new InvalidOperationException("Request does not belong to campaign.");

        if (request.Status != JoinRequestStatus.Pending)
            throw new InvalidOperationException("Request already processed.");

        if (request.User == null)
            throw new InvalidOperationException("Request has no user.");

        AddPlayer(campaign, request.User);

        request.Status = JoinRequestStatus.Approved;
        request.RespondedAt = DateTime.UtcNow;

        if (campaign.Members.Count >= campaign.MaxPlayers || campaign.Members.Count >= 50)
            campaign.IsRecruiting = false;
    }

    public void RejectRequest(Campaign campaign, JoinRequest request, ApplicationUser user)
    {
        if (campaign.OwnerUserId != user.Id)
            throw new UnauthorizedAccessException("Only owner can reject.");

        if (request.CampaignId != campaign.Id)
            throw new InvalidOperationException("Request does not belong to campaign.");

        if (request.Status != JoinRequestStatus.Pending)
            throw new InvalidOperationException("Request already processed.");

        request.Status = JoinRequestStatus.Rejected;
        request.RespondedAt = DateTime.UtcNow;
    }

    public void RemoveMember(Campaign campaign, string userId, ApplicationUser user)
    {
        if (campaign.OwnerUserId != user.Id)
            throw new UnauthorizedAccessException("Only owner can remove members.");

        var member = campaign.Members.FirstOrDefault(m => m.UserId == userId);
        if (member == null)
            throw new InvalidOperationException("Member not found.");

        campaign.Members.Remove(member);
    }

    public void ToggleRecruitment(Campaign campaign, ApplicationUser user)
    {
        if (campaign.OwnerUserId != user.Id)
            throw new UnauthorizedAccessException("Only owner can toggle recruitment.");

        if (campaign.MaxPlayers > 50)
            throw new InvalidOperationException("Max players cannot exceed 50.");

        if (campaign.Members.Count >= campaign.MaxPlayers || campaign.Members.Count >= 50)
        {
            campaign.IsRecruiting = false;
            return;
        }

        campaign.IsRecruiting = !campaign.IsRecruiting;
    }
}
