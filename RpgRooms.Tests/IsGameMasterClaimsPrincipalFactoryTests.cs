using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RpgRooms.Core.Entities;
using RpgRooms.Infrastructure;
using Xunit;

public class IsGameMasterClaimsPrincipalFactoryTests
{
    private static IsGameMasterClaimsPrincipalFactory CreateFactory()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        store.Setup(s => s.GetUserIdAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser u, CancellationToken _) => u.Id!);
        store.Setup(s => s.GetUserNameAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser u, CancellationToken _) => u.UserName!);

        var options = new OptionsWrapper<IdentityOptions>(new IdentityOptions());
        var userManager = new UserManager<ApplicationUser>(
            store.Object,
            options,
            passwordHasher: null,
            userValidators: null,
            passwordValidators: null,
            keyNormalizer: null,
            errors: null,
            services: null,
            logger: new Mock<ILogger<UserManager<ApplicationUser>>>().Object);

        var roleStore = new Mock<IRoleStore<IdentityRole>>();
        var roleManager = new RoleManager<IdentityRole>(
            roleStore.Object,
            roleValidators: null,
            keyNormalizer: null,
            errors: null,
            logger: new Mock<ILogger<RoleManager<IdentityRole>>>().Object);

        return new IsGameMasterClaimsPrincipalFactory(userManager, roleManager, options);
    }

    [Fact]
    public async Task Adds_IsGameMaster_Claim()
    {
        var factory = CreateFactory();
        var user = new ApplicationUser { Id = "1", UserName = "gm", IsGameMaster = true };

        var principal = await factory.CreateAsync(user);

        Assert.Contains(principal.Claims, c => c.Type == "IsGameMaster" && c.Value == "True");
    }

    [Fact]
    public async Task IsGameMaster_Policy_Succeeds_For_GameMaster()
    {
        var factory = CreateFactory();
        var user = new ApplicationUser { Id = "1", UserName = "gm", IsGameMaster = true };
        var principal = await factory.CreateAsync(user);

        var services = new ServiceCollection();
        services.AddAuthorization(options =>
        {
            options.AddPolicy("IsGameMaster", policy => policy.RequireClaim("IsGameMaster", "True"));
        });
        var provider = services.BuildServiceProvider();
        var auth = provider.GetRequiredService<IAuthorizationService>();

        var result = await auth.AuthorizeAsync(principal, null, "IsGameMaster");

        Assert.True(result.Succeeded);
    }
}
