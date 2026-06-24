namespace demoWebAPI.Models
{
    public class WishlistItem
    {
        public int Id { get; set; }

        public string UserId { get; set; } = null!;

        public int ProductId { get; set; }

        public DateTime? CreatedDate { get; set; }

        public virtual ApplicationUser User { get; set; } = null!;

        public virtual Product Product { get; set; } = null!;
    }
}
