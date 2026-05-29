namespace demoWebAPI.Options;

public class ZaloPaySettings
{
    public string AppId { get; set; } = string.Empty;
    public string Key1 { get; set; } = string.Empty;
    public string Key2 { get; set; } = string.Empty;
    public string Endpoint { get; set; } = "https://sb-openapi.zalopay.vn";
    public string CallbackUrl { get; set; } = string.Empty;
    public string RedirectUrl { get; set; } = string.Empty;
}
