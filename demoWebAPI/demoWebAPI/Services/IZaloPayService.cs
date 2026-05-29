using demoWebAPI.Models;

namespace demoWebAPI.Services;

public interface IZaloPayService
{
    Task<ZaloPayCreateResult> CreatePaymentAsync(Order order, string appUser);

    bool VerifyCallback(string data, string mac);

    ZaloPayCallbackData? ParseCallbackData(string data);
}

public class ZaloPayCreateResult
{
    public bool Success { get; set; }
    public string? OrderUrl { get; set; }
    public string? AppTransId { get; set; }
    public string? Message { get; set; }
}

public class ZaloPayCallbackData
{
    public string AppTransId { get; set; } = string.Empty;
    public long Amount { get; set; }
    public long ZpTransId { get; set; }
}
