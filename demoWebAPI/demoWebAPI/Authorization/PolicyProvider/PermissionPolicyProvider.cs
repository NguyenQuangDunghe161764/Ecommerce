using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

public class PermissionPolicyProvider
    : DefaultAuthorizationPolicyProvider
{
    public PermissionPolicyProvider(
        IOptions<AuthorizationOptions> options)
        : base(options)
    {
    }

    public override async Task<AuthorizationPolicy?>
     GetPolicyAsync(string policyName)
    {
        var policy =
            new AuthorizationPolicyBuilder();

        policy.RequireAuthenticatedUser();

        policy.AddRequirements(
            new PermissionRequirement(policyName));

        return await Task.FromResult(
            policy.Build());
    }
}