using MVCCallWebAPI.DTOs;
using MVCCallWebAPI.Helpers;
using System.Net.Http.Headers;

public class ApiClientService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IConfiguration _configuration;

    public ApiClientService(
        IHttpClientFactory factory,
        IHttpContextAccessor accessor,
        IConfiguration configuration)
    {
        _httpClient = factory.CreateClient();
        _contextAccessor = accessor;
        _configuration = configuration;
    }

    public async Task<List<ProductDto>> GetProducts()
    {
        var token = _contextAccessor.HttpContext.Session.GetString("JWT");

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        return await _httpClient.GetFromJsonAsync<List<ProductDto>>(
            ApiConfig.ApiUrl(_configuration, "api/products"));
    }
}