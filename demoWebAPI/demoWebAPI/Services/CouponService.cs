using demoWebAPI.Models;
using demoWebAPI.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace demoWebAPI.Services
{
    public class CouponService : ICouponService
    {
        private readonly EcomDbContext _context;

        public CouponService(EcomDbContext context)
        {
            _context = context;
        }

        public async Task<CouponValidationResult> ValidateAsync(
            string code, decimal orderAmount)
        {
            if (string.IsNullOrWhiteSpace(code))
                return new(false, "Vui lòng nhập mã giảm giá.", null, 0);

            var coupon = await _context.Coupons
                .FirstOrDefaultAsync(c => c.Code == code);

            if (coupon == null)
                return new(false, "Mã giảm giá không tồn tại.", null, 0);

            if (!coupon.IsActive)
                return new(false, "Mã giảm giá đã bị vô hiệu hóa.", null, 0);

            var now = DateTime.UtcNow;

            if (coupon.StartDate.HasValue && now < coupon.StartDate.Value)
                return new(false, "Mã giảm giá chưa đến thời gian sử dụng.", null, 0);

            if (coupon.EndDate.HasValue && now > coupon.EndDate.Value)
                return new(false, "Mã giảm giá đã hết hạn.", null, 0);

            if (coupon.UsageLimit.HasValue
                && coupon.UsedCount >= coupon.UsageLimit.Value)
                return new(false, "Mã giảm giá đã hết lượt sử dụng.", null, 0);

            if (orderAmount < coupon.MinOrderAmount)
                return new(false,
                    $"Đơn hàng tối thiểu {coupon.MinOrderAmount:N0} để dùng mã này.",
                    null, 0);

            decimal discount = coupon.DiscountType == DiscountType.Percentage
                ? orderAmount * coupon.DiscountValue / 100m
                : coupon.DiscountValue;

            if (coupon.MaxDiscountAmount.HasValue
                && discount > coupon.MaxDiscountAmount.Value)
                discount = coupon.MaxDiscountAmount.Value;

            // Không giảm quá giá trị đơn
            if (discount > orderAmount)
                discount = orderAmount;

            discount = Math.Round(discount, 2);

            return new(true, null, coupon, discount);
        }
    }
}
