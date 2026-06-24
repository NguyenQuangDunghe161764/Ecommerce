using System.ComponentModel.DataAnnotations;

namespace demoWebAPI.DTOs
{
    public class UpdateOrderStatusDto
    {
        [Required]
        public string Status { get; set; } = string.Empty;
    }

    public class AdminOrderListItemDto
    {
        public int Id { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public DateTime? OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Status { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public int ItemCount { get; set; }
    }

    public class AdminOrderDetailDto
    {
        public int Id { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public DateTime? OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Status { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public string? ZaloPayAppTransId { get; set; }
        public List<AdminOrderLineDto> Items { get; set; } = new();
    }

    public class AdminOrderLineDto
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }
}
