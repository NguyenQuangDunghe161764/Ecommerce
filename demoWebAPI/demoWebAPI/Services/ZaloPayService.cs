using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using demoWebAPI.Models;
using demoWebAPI.Options;
using Microsoft.Extensions.Options;

namespace demoWebAPI.Services;

public class ZaloPayService : IZaloPayService
{
    private readonly ZaloPaySettings _settings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<ZaloPayService> _logger;

    public ZaloPayService(
        IOptions<ZaloPaySettings> settings,
        HttpClient httpClient,
        ILogger<ZaloPayService> logger)
    {
        _settings = settings.Value;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ZaloPayCreateResult> CreatePaymentAsync(Order order, string appUser)
    {
        if (string.IsNullOrWhiteSpace(_settings.AppId) ||
            string.IsNullOrWhiteSpace(_settings.Key1))
        {
            return new ZaloPayCreateResult
            {
                Success = false,
                Message = "ZaloPay is not configured. Add AppId and Key1 in appsettings."
            };
        }

        var appTransId = GenerateAppTransId(order.Id);
        var appTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var amount = (long)order.TotalAmount;

        var redirectUrl =
            $"{_settings.RedirectUrl.TrimEnd('?')}?orderId={order.Id}";

        var embedData = JsonSerializer.Serialize(new
        {
            order_id = order.Id,
            redirecturl = redirectUrl
        });

        var item = BuildItemJson(order);
        var description = $"Thanh toan don hang #{order.Id}";

        var macData =
            $"{_settings.AppId}|{appTransId}|{appUser}|{amount}|{appTime}|{embedData}|{item}";
        var mac = ComputeHmac(_settings.Key1, macData);

        var form = new Dictionary<string, string>
        {
            ["app_id"] = _settings.AppId,
            ["app_user"] = appUser,
            ["app_trans_id"] = appTransId,
            ["app_time"] = appTime.ToString(),
            ["amount"] = amount.ToString(),
            ["item"] = item,
            ["embed_data"] = embedData,
            ["description"] = description,
            ["bank_code"] = "",
            ["mac"] = mac
        };

        if (!string.IsNullOrWhiteSpace(_settings.CallbackUrl))
        {
            form["callback_url"] = _settings.CallbackUrl;
        }

        using var content = new FormUrlEncodedContent(form);
        var response = await _httpClient.PostAsync(
            $"{_settings.Endpoint.TrimEnd('/')}/v2/create",
            content);

        var body = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("ZaloPay create response: {Body}", body);

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        var returnCode = root.TryGetProperty("return_code", out var rc)
            ? rc.GetInt32()
            : -1;

        if (returnCode != 1)
        {
            var msg = root.TryGetProperty("return_message", out var rm)
                ? rm.GetString()
                : "Create order failed";
            return new ZaloPayCreateResult
            {
                Success = false,
                Message = msg
            };
        }

        var orderUrl = root.TryGetProperty("order_url", out var ou)
            ? ou.GetString()
            : null;

        return new ZaloPayCreateResult
        {
            Success = true,
            OrderUrl = orderUrl,
            AppTransId = appTransId
        };
    }

    public bool VerifyCallback(string data, string mac)
    {
        if (string.IsNullOrWhiteSpace(_settings.Key2))
        {
            return false;
        }

        var expected = ComputeHmac(_settings.Key2, data);
        return string.Equals(expected, mac, StringComparison.OrdinalIgnoreCase);
    }

    public ZaloPayCallbackData? ParseCallbackData(string data)
    {
        try
        {
            using var doc = JsonDocument.Parse(data);
            var root = doc.RootElement;

            return new ZaloPayCallbackData
            {
                AppTransId = root.GetProperty("app_trans_id").GetString() ?? "",
                Amount = root.GetProperty("amount").GetInt64(),
                ZpTransId = root.TryGetProperty("zp_trans_id", out var zt)
                    ? zt.GetInt64()
                    : 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse ZaloPay callback data");
            return null;
        }
    }

    public static string GenerateAppTransId(int orderId)
    {
        return $"{DateTime.Now:yyMMdd}_{orderId}_{Random.Shared.Next(1000, 9999)}";
    }

    private static string BuildItemJson(Order order)
    {
        var items = order.Orderdetails.Select(d => new
        {
            itemid = d.ProductId.ToString(),
            itemname = $"Product {d.ProductId}",
            itemprice = (long)d.UnitPrice,
            itemquantity = d.Quantity
        });

        return JsonSerializer.Serialize(items);
    }

    private static string ComputeHmac(string key, string data)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}
