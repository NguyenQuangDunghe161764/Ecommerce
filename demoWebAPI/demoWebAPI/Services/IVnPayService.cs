using demoWebAPI.Models;
using Microsoft.AspNetCore.Http;

namespace demoWebAPI.Services
{
    public record VnPayReturnResult(
        bool Success,
        int OrderId,
        string ResponseCode,
        string Message);

    public interface IVnPayService
    {
        // Tạo URL chuyển hướng sang cổng VNPay cho 1 đơn hàng.
        string CreatePaymentUrl(Order order, string clientIpAddress);

        // Xác thực và phân tích dữ liệu VNPay trả về (query string).
        VnPayReturnResult ProcessReturn(IQueryCollection query);
    }
}
