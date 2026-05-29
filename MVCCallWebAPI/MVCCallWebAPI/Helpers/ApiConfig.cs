namespace MVCCallWebAPI.Helpers;

public static class ApiConfig
{
    public static string GetBaseUrl(IConfiguration configuration)
    {
        var url = configuration["ApiSettings:BaseUrl"];
        if (string.IsNullOrWhiteSpace(url))
        {
            return "https://localhost:7208";
        }

        return url.TrimEnd('/');
    }

    public static string ApiUrl(IConfiguration configuration, string path)
    {
        var baseUrl = GetBaseUrl(configuration);
        path = path.TrimStart('/');
        return $"{baseUrl}/{path}";
    }

    public static string ImageUrl(IConfiguration configuration, string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return string.Empty;
        }

        if (relativePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            return relativePath;
        }

        return $"{GetBaseUrl(configuration)}{relativePath}";
    }
}
