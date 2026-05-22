using System;
using System.Collections.Generic;

namespace demoWebAPI.Models;

public partial class Order
{
    public int Id { get; set; }

    public string UserId { get; set; }

    public virtual ApplicationUser User { get; set; }

    public DateTime? OrderDate { get; set; }

    public decimal TotalAmount { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<Orderdetail> Orderdetails { get; set; } = new List<Orderdetail>();

}
