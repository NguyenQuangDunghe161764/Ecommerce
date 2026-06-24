using demoWebAPI.Models;

namespace demoWebAPI.Services
{
    public record CouponValidationResult(
        bool IsValid,
        string? Error,
        Coupon? Coupon,
        decimal DiscountAmount);

    public interface ICouponService
    {
        // Kiểm tra mã + tính số tiền giảm cho 1 giá trị đơn hàng.
        Task<CouponValidationResult> ValidateAsync(
            string code, decimal orderAmount);
    }
}
