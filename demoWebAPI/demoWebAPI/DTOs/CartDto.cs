using System.ComponentModel.DataAnnotations;

namespace demoWebAPI.DTOs
{
    // Thêm sản phẩm vào giỏ
    public class AddToCartDto
    {
        [Required]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; } = 1;
    }

    // Cập nhật số lượng 1 dòng trong giỏ
    public class UpdateCartItemDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }
    }

    // Một dòng trong giỏ trả về client
    public class CartItemResponseDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public int Stock { get; set; }
        public decimal LineTotal { get; set; }
    }

    // Toàn bộ giỏ hàng trả về client
    public class CartResponseDto
    {
        public List<CartItemResponseDto> Items { get; set; } = new();
        public int TotalQuantity { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
