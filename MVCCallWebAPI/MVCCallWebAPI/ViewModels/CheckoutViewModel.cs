using System.ComponentModel.DataAnnotations;

namespace MVCCallWebAPI.Models
{
    public class CheckoutViewModel
    {
        public List<AddressClientModel> Addresses { get; set; } = new List<AddressClientModel>();

        public List<CartItemClientModel> CartItems { get; set; } = new List<CartItemClientModel>();

        public decimal SubTotal { get; set; } // Tổng tiền hàng (chưa ship)
        public decimal ShippingFee { get; set; } // Phí ship
        public decimal TotalOrder => SubTotal + ShippingFee; // Tổng thanh toán = Tiền hàng + Ship

        [Required(ErrorMessage = "Vui lòng chọn địa chỉ nhận hàng.")]
        public int SelectedAddressId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán.")]
        public string PaymentMethod { get; set; } = "COD"; // Mặc định là COD
    }

    public class AddressClientModel
    {
        public int Id { get; set; }
        public string ReceiverName { get; set; }
        public string PhoneNumber { get; set; }
        public string Province { get; set; }
        public string District { get; set; }
        public string Ward { get; set; }
        public string StreetAddress { get; set; }
        public bool IsDefault { get; set; }
        public string UserId { get; set; }
    }

    // --- Lớp phụ hiển thị tóm tắt sản phẩm đang mua ở trang Checkout (Tùy chọn) ---
    public class CartItemClientModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice => Quantity * Price;
    }
}