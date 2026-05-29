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
        if (string.IsNullOrWhiteSpace(policyName) ||
            !policyName.Contains('.'))
        {
            return await base.GetPolicyAsync(policyName);
        }

        var policy = new AuthorizationPolicyBuilder();
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(new PermissionRequirement(policyName));

        return policy.Build();
    }
}