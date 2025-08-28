using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using RpgRooms.Core.Entities;
using RpgRooms.Core.Policies;
using RpgRooms.Core.Services;
using RpgRooms.Infrastructure;
using RpgRooms.Web.Hubs;
using Xunit;

namespace RpgRooms.Tests;

public class CampaignServiceTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

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

    [Fact]
    public void ApprovingRequestDisablesRecruitmentAt50()
    {
        var service = new CampaignService();
        var owner = new ApplicationUser { Id = "1" };
        var campaign = new Campaign { Id = 1, OwnerUserId = owner.Id, IsRecruiting = true, MaxPlayers = 50 };

        for (int i = 0; i < 49; i++)
        {
            service.AddPlayer(campaign, new ApplicationUser { Id = (i + 2).ToString() });
        }

        var newUser = new ApplicationUser { Id = "100" };
        var request = service.RequestToJoin(campaign, newUser, "let me in");

        service.ApproveRequest(campaign, request, owner);

        Assert.Equal(50, campaign.Members.Count);
        Assert.False(campaign.IsRecruiting);
        Assert.Equal(JoinRequestStatus.Approved, request.Status);
    }

    [Fact]
    public void CampaignService_ShouldNotExceedMaxPlayers()
    {
        using var db = CreateContext();
        var service = new CampaignService();
        var campaign = new Campaign { Id = 1, OwnerUserId = "1", MaxPlayers = 60 };
        db.Campaigns.Add(campaign);
        db.SaveChanges();

        Assert.Throws<InvalidOperationException>(() =>
            service.AddPlayer(campaign, new ApplicationUser { Id = "2" }));
    }

    [Fact]
    public void OnlyOwnerCanToggleRecruitment()
    {
        using var db = CreateContext();
        var service = new CampaignService();
        var owner = new ApplicationUser { Id = "1" };
        var campaign = new Campaign { Id = 1, OwnerUserId = owner.Id, MaxPlayers = 5, IsRecruiting = false };
        db.Campaigns.Add(campaign);
        db.SaveChanges();

        service.ToggleRecruitment(campaign, owner);
        Assert.True(campaign.IsRecruiting);

        var other = new ApplicationUser { Id = "2" };
        Assert.Throws<UnauthorizedAccessException>(() => service.ToggleRecruitment(campaign, other));
    }

    [Fact]
    public void ApproveJoinRequest_AddsMember_And_AutoDisablesAt50()
    {
        using var db = CreateContext();
        var service = new CampaignService();
        var owner = new ApplicationUser { Id = "1" };
        var campaign = new Campaign { Id = 1, OwnerUserId = owner.Id, IsRecruiting = true, MaxPlayers = 50 };
        db.Campaigns.Add(campaign);
        db.SaveChanges();

        for (int i = 0; i < 49; i++)
        {
            var player = new ApplicationUser { Id = (i + 2).ToString() };
            service.AddPlayer(campaign, player);
        }

        var newUser = new ApplicationUser { Id = "100" };
        var request = service.RequestToJoin(campaign, newUser, "let me in");
        service.ApproveRequest(campaign, request, owner);

        Assert.Contains(campaign.Members, m => m.UserId == newUser.Id);
        Assert.False(campaign.IsRecruiting);
        Assert.Equal(JoinRequestStatus.Approved, request.Status);
    }

    [Fact]
    public async Task FinalizeCampaign_MakesChatReadOnly()
    {
        using var db = CreateContext();
        var service = new CampaignService();
        var owner = new ApplicationUser { Id = "1", UserName = "Owner" };
        var campaign = new Campaign { Id = 1, OwnerUserId = owner.Id, Status = CampaignStatus.Active };
        service.AddPlayer(campaign, owner);
        db.Campaigns.Add(campaign);
        db.SaveChanges();

        service.FinalizeCampaign(campaign, owner);
        db.SaveChanges();

        var hub = new CampaignHub(db);
        var mockClients = new Mock<IHubCallerClients>();
        var mockCaller = new Mock<IClientProxy>();
        mockClients.Setup(c => c.Caller).Returns(mockCaller.Object);
        hub.Clients = mockClients.Object;

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, owner.Id),
            new Claim(ClaimTypes.Name, owner.UserName!)
        }, "Test"));
        var mockContext = new Mock<HubCallerContext>();
        mockContext.SetupGet(c => c.User).Returns(user);
        hub.Context = mockContext.Object;

        await hub.SendMessage(campaign.Id, "test", false);

        Assert.Empty(db.ChatMessages);
        mockCaller.Verify(c => c.SendAsync("SystemNotice", "Campaign has been finalized.", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task NonMember_CannotSendMessage()
    {
        using var db = CreateContext();
        var campaign = new Campaign { Id = 1, OwnerUserId = "owner" };
        campaign.Members.Add(new CampaignMember { CampaignId = campaign.Id, UserId = "member" });
        db.Campaigns.Add(campaign);
        db.SaveChanges();

        var requirement = new IsMemberOfCampaignRequirement();
        var handler = new IsMemberOfCampaignHandler(db);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "nonmember")
        }, "Test"));

        var hub = new Mock<Hub>();
        var hubContext = new HubInvocationContext(hub.Object, nameof(CampaignHub.SendMessage), new object[] { campaign.Id, "hi", false });

        var authContext = new AuthorizationHandlerContext(new[] { requirement }, user, hubContext);
        await handler.HandleAsync(authContext);

        Assert.False(authContext.HasSucceeded);
    }
}
