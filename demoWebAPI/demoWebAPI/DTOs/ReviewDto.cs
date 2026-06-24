using System.ComponentModel.DataAnnotations;

namespace demoWebAPI.DTOs
{
    // Tạo / cập nhật đánh giá
    public class CreateReviewDto
    {
        [Range(1, 5, ErrorMessage = "Số sao phải từ 1 đến 5")]
        public int Rating { get; set; }

        [MaxLength(2000)]
        public string? Comment { get; set; }
    }

    // Một đánh giá trả về client
    public class ReviewResponseDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime? CreatedDate { get; set; }
    }

    // Tổng hợp đánh giá của 1 sản phẩm
    public class ReviewSummaryDto
    {
        public int ProductId { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        // Phân bố số lượng theo từng mức sao (1..5)
        public Dictionary<int, int> RatingBreakdown { get; set; } = new();
        public List<ReviewResponseDto> Reviews { get; set; } = new();
    }
}
