using System.ComponentModel.DataAnnotations;

namespace demoWebAPI.DTOs;

public class CheckoutDto
{
    [Required]
    public List<CheckoutItemDto> Items { get; set; } = new();
}

public class CheckoutItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class CheckoutResponseDto
{
    public int OrderId { get; set; }
    public decimal TotalAmount { get; set; }
}

public class ZaloPayCreateRequestDto
{
    public int OrderId { get; set; }
}

public class ZaloPayCreateResponseDto
{
    public string OrderUrl { get; set; } = string.Empty;
    public string AppTransId { get; set; } = string.Empty;
}
