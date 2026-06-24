namespace demoWebAPI.Models.Enums
{
    public enum DiscountType
    {
        // Giảm theo phần trăm giá trị đơn (DiscountValue = %)
        Percentage = 0,

        // Giảm số tiền cố định (DiscountValue = số tiền)
        Fixed = 1
    }
}
