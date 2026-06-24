using System.Globalization;
using demoWebAPI.Models;
using demoWebAPI.Options;
using Microsoft.Extensions.Options;

namespace demoWebAPI.Services
{
    public class VnPayService : IVnPayService
    {
        private readonly VnPaySettings _settings;

        public VnPayService(IOptions<VnPaySettings> settings)
        {
            _settings = settings.Value;
        }

        public string CreatePaymentUrl(Order order, string clientIpAddress)
        {
            var now = DateTime.UtcNow.AddHours(7); // giờ VN
            var vnp = new VnPayLibrary();

            vnp.AddRequestData("vnp_Version", _settings.Version);
            vnp.AddRequestData("vnp_Command", _settings.Command);
            vnp.AddRequestData("vnp_TmnCode", _settings.TmnCode);
            // VNPay yêu cầu số tiền nhân 100, không phần thập phân
            vnp.AddRequestData("vnp_Amount",
                ((long)(order.TotalAmount * 100)).ToString(CultureInfo.InvariantCulture));
            vnp.AddRequestData("vnp_CreateDate", now.ToString("yyyyMMddHHmmss"));
            vnp.AddRequestData("vnp_CurrCode", _settings.CurrCode);
            vnp.AddRequestData("vnp_IpAddr",
                string.IsNullOrEmpty(clientIpAddress) ? "127.0.0.1" : clientIpAddress);
            vnp.AddRequestData("vnp_Locale", _settings.Locale);
            vnp.AddRequestData("vnp_OrderInfo", $"Thanh toan don hang {order.Id}");
            vnp.AddRequestData("vnp_OrderType", "other");
            vnp.AddRequestData("vnp_ReturnUrl", _settings.ReturnUrl);
            vnp.AddRequestData("vnp_TxnRef", order.Id.ToString());

            return vnp.CreateRequestUrl(_settings.BaseUrl, _settings.HashSecret);
        }

        public VnPayReturnResult ProcessReturn(IQueryCollection query)
        {
            var vnp = new VnPayLibrary();
            foreach (var (key, value) in query)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                    vnp.AddResponseData(key, value.ToString());
            }

            var inputHash = query["vnp_SecureHash"].ToString();
            var txnRef = vnp.GetResponseData("vnp_TxnRef");
            var responseCode = vnp.GetResponseData("vnp_ResponseCode") ?? "";

            int.TryParse(txnRef, out var orderId);

            var valid = vnp.ValidateSignature(inputHash, _settings.HashSecret);
            if (!valid)
                return new(false, orderId, responseCode, "Chữ ký không hợp lệ.");

            // "00" = giao dịch thành công
            if (responseCode != "00")
                return new(false, orderId, responseCode, "Thanh toán không thành công.");

            return new(true, orderId, responseCode, "Thanh toán thành công.");
        }
    }
}
