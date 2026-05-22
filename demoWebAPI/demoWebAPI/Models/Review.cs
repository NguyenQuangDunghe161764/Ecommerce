using System;
using System.Collections.Generic;

namespace demoWebAPI.Models;

public partial class Review
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string UserId { get; set; }

    public virtual ApplicationUser User { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual Product Product { get; set; } = null!;

}
