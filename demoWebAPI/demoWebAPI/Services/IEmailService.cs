namespace demoWebAPI.Services
{
    public interface IEmailService
    {
        Task SendAsync(string toEmail, string subject, string htmlBody);

        // Gửi email xác nhận đặt hàng
        Task SendOrderConfirmationAsync(
            string toEmail,
            string customerName,
            int orderId,
            decimal totalAmount);
    }
}
