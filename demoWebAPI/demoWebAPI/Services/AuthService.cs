using demoWebAPI.DTOs;
using demoWebAPI.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;

public class AuthService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly HttpClient _httpClient;

    public AuthService(
        IHttpContextAccessor httpContextAccessor,
        HttpClient httpClient)
    {
        _httpContextAccessor = httpContextAccessor;
        _httpClient = httpClient;
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        var session =
            _httpContextAccessor.HttpContext!.Session;

        var accessToken =
            session.GetString("AccessToken");

        var refreshToken =
            session.GetString("RefreshToken");

        // ATTACH ACCESS TOKEN
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        // TEST TOKEN
        var testResponse =
            await _httpClient.GetAsync(
                "https://localhost:7208/api/products");

        // TOKEN OK
        if (testResponse.StatusCode !=
            System.Net.HttpStatusCode.Unauthorized)
        {
            return accessToken;
        }

        // TOKEN EXPIRED -> REFRESH
        var refreshResponse =
            await _httpClient.PostAsJsonAsync(
                "https://localhost:7208/api/auth/refresh",
                new
                {
                    refreshToken = refreshToken
                });

        // REFRESH FAILED
        if (!refreshResponse.IsSuccessStatusCode)
        {
            session.Clear();
            return null;
        }

        // GET NEW TOKEN
        var result =
            await refreshResponse.Content
                .ReadFromJsonAsync<RefreshResponse>();

        // SAVE NEW TOKEN
        session.SetString(
            "AccessToken",
            result!.AccessToken);

        return result.AccessToken;
    }
}