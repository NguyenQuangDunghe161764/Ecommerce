using System.ComponentModel.DataAnnotations;
using demoWebAPI.Models.Enums;

namespace demoWebAPI.DTOs
{
    public class CreateCouponDto
    {
        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        public DiscountType DiscountType { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal DiscountValue { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MinOrderAmount { get; set; }

        public decimal? MaxDiscountAmount { get; set; }

        public int? UsageLimit { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class ApplyCouponDto
    {
        [Required]
        public string Code { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal OrderAmount { get; set; }
    }

    public class ApplyCouponResultDto
    {
        public bool IsValid { get; set; }
        public string? Message { get; set; }
        public string? Code { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
    }
}
