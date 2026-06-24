using System.Net;
using System.Net.Mail;
using demoWebAPI.Options;
using Microsoft.Extensions.Options;

namespace demoWebAPI.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IOptions<EmailSettings> settings,
            ILogger<EmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendAsync(string toEmail, string subject, string htmlBody)
        {
            // Demo-safe: nếu chưa cấu hình SMTP thì chỉ log, không ném lỗi
            if (!_settings.Enabled)
            {
                _logger.LogInformation(
                    "Email disabled. Bỏ qua gửi tới {To} - {Subject}", toEmail, subject);
                return;
            }

            try
            {
                using var message = new MailMessage
                {
                    From = new MailAddress(_settings.FromEmail, _settings.FromName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };
                message.To.Add(toEmail);

                using var client = new SmtpClient(_settings.Host, _settings.Port)
                {
                    EnableSsl = _settings.EnableSsl,
                    Credentials = new NetworkCredential(
                        _settings.UserName, _settings.Password)
                };

                await client.SendMailAsync(message);
                _logger.LogInformation("Đã gửi email tới {To}", toEmail);
            }
            catch (Exception ex)
            {
                // Không để lỗi email làm hỏng luồng đặt hàng
                _logger.LogError(ex, "Gửi email thất bại tới {To}", toEmail);
            }
        }

        public Task SendOrderConfirmationAsync(
            string toEmail,
            string customerName,
            int orderId,
            decimal totalAmount)
        {
            var subject = $"Xác nhận đơn hàng #{orderId}";
            var body = $@"
                <h2>Cảm ơn bạn đã đặt hàng!</h2>
                <p>Xin chào {WebUtility.HtmlEncode(customerName)},</p>
                <p>Đơn hàng <strong>#{orderId}</strong> của bạn đã được tiếp nhận.</p>
                <p>Tổng thanh toán: <strong>{totalAmount:N0} đ</strong></p>
                <p>Chúng tôi sẽ xử lý và giao hàng sớm nhất.</p>
                <hr/>
                <p style='color:#888'>Ecommerce Shop</p>";

            return SendAsync(toEmail, subject, body);
        }
    }
}
