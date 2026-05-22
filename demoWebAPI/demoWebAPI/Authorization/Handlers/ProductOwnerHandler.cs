using demoWebAPI.Authorization.Requirements;
using demoWebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace demoWebAPI.Authorization.Handlers
{

    public class ProductOwnerHandler
    : AuthorizationHandler<
        ProductOwnerRequirement,
        Product>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            ProductOwnerRequirement requirement,
            Product resource)
        {
            // ADMIN => FULL ACCESS
            if (context.User.IsInRole("Admin"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // CURRENT USER ID
            var userId =
                context.User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            // OWNER CHECK
            if (resource.OwnerId == userId)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
