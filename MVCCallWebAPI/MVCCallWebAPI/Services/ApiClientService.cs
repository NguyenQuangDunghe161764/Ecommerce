using MVCCallWebAPI.DTOs;
using System.Net.Http.Headers;

public class ApiClientService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _contextAccessor;

    public ApiClientService(IHttpClientFactory factory, IHttpContextAccessor accessor)
    {
        _httpClient = factory.CreateClient();
        _contextAccessor = accessor;
    }

    public async Task<List<ProductDto>> GetProducts()
    {
        var token = _contextAccessor.HttpContext.Session.GetString("JWT");

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        return await _httpClient.GetFromJsonAsync<List<ProductDto>>(
            "https://localhost:5001/api/products"
        );
    }
}