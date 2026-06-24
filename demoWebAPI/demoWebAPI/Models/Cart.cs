namespace demoWebAPI.Models
{
    public class Cart
    {
        public int Id { get; set; }

        public string UserId { get; set; } = null!;

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public virtual ApplicationUser User { get; set; } = null!;

        public virtual ICollection<CartItem> CartItems { get; set; }
            = new List<CartItem>();
    }
}
