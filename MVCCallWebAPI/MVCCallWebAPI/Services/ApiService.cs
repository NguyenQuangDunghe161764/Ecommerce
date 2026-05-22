using MVCCallWebAPI.Services.Interface;
using Newtonsoft.Json;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace MVCCallWebAPI.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiService> _logger;
    private readonly IHttpContextAccessor _accessor;

    public ApiService(
        HttpClient httpClient,
        ILogger<ApiService> logger,
        IHttpContextAccessor accessor)
    {
        _httpClient = httpClient;
        _logger = logger;
        _accessor = accessor;
    }

    // GET
    public async Task<T?> GetAsync<T>(string endpoint)
    {
        try
        {
            Console.WriteLine(endpoint);

            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);

            // attach bearer token from session if available
            try
            {
                var token = _accessor.HttpContext?.Session.GetString("JWT");
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }
            catch { }

            var response = await _httpClient.SendAsync(request);

            Console.WriteLine(response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                return default;
            }

            var json =
                await response.Content.ReadAsStringAsync();

            Console.WriteLine(json);

            return JsonConvert.DeserializeObject<T>(json);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);

            _logger.LogError(ex, "GET Error");

            return default;
        }
    }

    // POST
    public async Task<T?> PostAsync<T>(
        string endpoint,
        object data)
    {
        try
        {
            var json = JsonConvert.SerializeObject(data);

            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = content
            };

            try
            {
                var token = _accessor.HttpContext?.Session.GetString("JWT");
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }
            catch { }

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return default;
            }

            var result = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "POST Error");

            return default;
        }
    }

    // PUT
    public async Task<bool> UpdateAsync(
        string endpoint,
        object data)
    {
        try
        {
            var json = JsonConvert.SerializeObject(data);

            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            var request = new HttpRequestMessage(HttpMethod.Put, endpoint)
            {
                Content = content
            };

            try
            {
                var token = _accessor.HttpContext?.Session.GetString("JWT");
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }
            catch { }

            var response = await _httpClient.SendAsync(request);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PUT Error");

            return false;
        }
    }

    // DELETE
    public async Task<bool> DeleteAsync(string endpoint)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);

            try
            {
                var token = _accessor.HttpContext?.Session.GetString("JWT");
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }
            catch { }

            var response = await _httpClient.SendAsync(request);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DELETE Error");

            return false;
        }
    }

}