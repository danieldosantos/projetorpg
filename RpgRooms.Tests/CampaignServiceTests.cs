using RpgRooms.Core.Entities;
using RpgRooms.Core.Services;
using Xunit;

namespace RpgRooms.Tests;

public class CampaignServiceTests
{
    [Fact]
    public void AddPlayer_LimitsTo50()
    {
        var service = new CampaignService();
        var owner = new ApplicationUser { Id = "1", UserName = "Owner" };
        var campaign = new Campaign { Id = 1, Name = "Test", OwnerUserId = owner.Id, MaxPlayers = 50 };

        for (int i = 0; i < 50; i++)
        {
            service.AddPlayer(campaign, new ApplicationUser { Id = (i + 2).ToString() });
        }

        Assert.Equal(50, campaign.Members.Count);
        Assert.Throws<InvalidOperationException>(() =>
            service.AddPlayer(campaign, new ApplicationUser { Id = "100" }));
    }

    [Fact]
    public void OnlyOwnerCanFinalize()
    {
        var service = new CampaignService();
        var owner = new ApplicationUser { Id = "1" };
        var player = new ApplicationUser { Id = "2" };
        var campaign = new Campaign { OwnerUserId = owner.Id };

        service.FinalizeCampaign(campaign, owner);
        Assert.Equal(CampaignStatus.Finalized, campaign.Status);

        campaign.Status = CampaignStatus.Active;
        Assert.Throws<UnauthorizedAccessException>(() =>
            service.FinalizeCampaign(campaign, player));
    }

    [Fact]
    public void CannotAddPlayersAfterFinalization()
    {
        var service = new CampaignService();
        var owner = new ApplicationUser { Id = "1" };
        var campaign = new Campaign { OwnerUserId = owner.Id, Status = CampaignStatus.Finalized };

        Assert.Throws<InvalidOperationException>(() =>
            service.AddPlayer(campaign, new ApplicationUser { Id = "2" }));
    }
}
