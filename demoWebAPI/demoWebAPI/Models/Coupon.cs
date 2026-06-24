using demoWebAPI.Models.Enums;

namespace demoWebAPI.Models
{
    public class Coupon
    {
        public int Id { get; set; }

        // Mã giảm giá (duy nhất, vd: SALE50)
        public string Code { get; set; } = null!;

        public DiscountType DiscountType { get; set; }

        // % hoặc số tiền tùy DiscountType
        public decimal DiscountValue { get; set; }

        // Giá trị đơn tối thiểu để áp mã
        public decimal MinOrderAmount { get; set; }

        // Mức giảm tối đa (áp dụng cho loại Percentage). null = không giới hạn
        public decimal? MaxDiscountAmount { get; set; }

        // Tổng số lần được dùng. null = không giới hạn
        public int? UsageLimit { get; set; }

        public int UsedCount { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? CreatedDate { get; set; }
    }
}
