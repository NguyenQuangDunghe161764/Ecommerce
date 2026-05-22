using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace demoWebAPI.Authorization.Requirements
{
    public class ProductOwnerRequirement : IAuthorizationRequirement
    {
    }
}
