namespace MVCCallWebAPI.Services.Interface;

public interface IApiService
{
    Task<T?> GetAsync<T>(string endpoint);

    Task<T?> PostAsync<T>(string endpoint, object data);

    Task<bool> UpdateAsync(string endpoint, object data);

    Task<bool> DeleteAsync(string endpoint);
}