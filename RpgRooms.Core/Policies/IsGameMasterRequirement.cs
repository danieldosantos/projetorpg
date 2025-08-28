using Microsoft.AspNetCore.Authorization;

namespace RpgRooms.Core.Policies;

public class IsGameMasterRequirement : IAuthorizationRequirement { }

public class IsGameMasterHandler : AuthorizationHandler<IsGameMasterRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IsGameMasterRequirement requirement)
    {
        if (context.User.HasClaim("IsGameMaster", "True"))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
