using Chainly.Data.Constants;
using Microsoft.AspNetCore.Authorization;

namespace Chainly.Filters;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    public PermissionAuthorizationHandler()
    {
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User is null)
            return;

        var canAccess = context.User.Claims.Any(c => c.Type == CustomClaimTypes.Permission && c.Value == requirement.Permission);
        if (canAccess)
        {
            context.Succeed(requirement);
            return;
        }
    }
}