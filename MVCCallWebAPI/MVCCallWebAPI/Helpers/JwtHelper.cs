using System.IdentityModel.Tokens.Jwt;

public static class JwtHelper
{
    public static bool HasPermission(string jwt, string permission)
    {
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwt);

        var claims = token.Claims.ToList();

        var permissions = claims
            .Where(c =>
                c.Type == "Permission" ||
                c.Type == "permission" ||
                c.Type == "permissions")
            .SelectMany(c => c.Value.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(x => x.Trim());

        return permissions.Contains(permission);
    }
}