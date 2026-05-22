namespace MVCCallWebAPI.Models;

public class ShoppingCart
{
    public List<CartItem> Items { get; set; }
        = new List<CartItem>();

    public decimal TotalPrice
    {
        get
        {
            return Items.Sum(i => i.Total);
        }
    }
}