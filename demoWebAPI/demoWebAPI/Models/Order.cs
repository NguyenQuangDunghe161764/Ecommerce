using System;
using System.Collections.Generic;
using demoWebAPI.Models.Enums;
namespace demoWebAPI.Models;

public partial class Order
{
    public int Id { get; set; }

    public string UserId { get; set; }

    public virtual ApplicationUser User { get; set; }

    public DateTime? OrderDate { get; set; }

    public decimal TotalAmount { get; set; }

    public string? Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }

    public string? ZaloPayAppTransId { get; set; }

    // Mã giảm giá đã áp (nếu có) và số tiền được giảm
    public string? CouponCode { get; set; }

    public decimal DiscountAmount { get; set; }

    public virtual ICollection<Orderdetail> Orderdetails { get; set; } = new List<Orderdetail>();

}
