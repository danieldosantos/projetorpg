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
        var gm = new User { Id = 1, UserName = "GM" };
        var campaign = new Campaign { Id = 1, Name = "Test", GameMasterId = gm.Id };

        for (int i = 0; i < CampaignService.MaxPlayers; i++)
        {
            service.AddPlayer(campaign, new User { Id = i + 2 });
        }

        Assert.Equal(CampaignService.MaxPlayers, campaign.Players.Count);
        Assert.Throws<InvalidOperationException>(() =>
            service.AddPlayer(campaign, new User { Id = 100 }));
    }

    [Fact]
    public void OnlyGameMasterCanFinalize()
    {
        var service = new CampaignService();
        var gm = new User { Id = 1 };
        var player = new User { Id = 2 };
        var campaign = new Campaign { GameMasterId = gm.Id };

        service.FinalizeCampaign(campaign, gm);
        Assert.True(campaign.IsFinished);

        campaign.IsFinished = false;
        Assert.Throws<UnauthorizedAccessException>(() =>
            service.FinalizeCampaign(campaign, player));
    }

    [Fact]
    public void CannotAddPlayersAfterFinalization()
    {
        var service = new CampaignService();
        var gm = new User { Id = 1 };
        var campaign = new Campaign { GameMasterId = gm.Id, IsFinished = true };

        Assert.Throws<InvalidOperationException>(() =>
            service.AddPlayer(campaign, new User { Id = 2 }));
    }
}
