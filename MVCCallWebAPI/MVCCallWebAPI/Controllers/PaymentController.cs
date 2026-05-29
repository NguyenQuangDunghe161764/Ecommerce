using Microsoft.AspNetCore.Mvc;
using MVCCallWebAPI.Helpers;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace MVCCallWebAPI.Controllers
{
    public class PaymentController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public PaymentController(
            IHttpClientFactory factory,
            IConfiguration configuration)
        {
            _httpClient = factory.CreateClient();
            _configuration = configuration;
        }

        private string Api(string path) => ApiConfig.ApiUrl(_configuration, path);

        [HttpGet]
        public IActionResult FakePay(int orderId)
        {
            ViewBag.OrderId = orderId;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Return(int orderId)
        {
            var token = GetAccessTokenFromSession();
            if (string.IsNullOrWhiteSpace(token))
            {
                TempData["Error"] = "Phiên đăng nhập đã hết hạn.";
                return RedirectToAction("Login", "Account");
            }

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                Api($"api/zalopay/status/{orderId}"));
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                if (!await TryRefreshAccessTokenAsync())
                {
                    return RedirectToAction("Login", "Account");
                }

                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", GetAccessTokenFromSession());
                response = await _httpClient.SendAsync(request);
            }

            ViewBag.OrderId = orderId;
            ViewBag.IsPaid = false;

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var status = doc.RootElement
                    .GetProperty("paymentStatus")
                    .GetString();

                ViewBag.IsPaid = string.Equals(
                    status,
                    "Paid",
                    StringComparison.OrdinalIgnoreCase);
            }

            if (ViewBag.IsPaid == true)
            {
                HttpContext.Session.Remove("Cart");
                TempData["Success"] = $"Thanh toán đơn hàng #{orderId} thành công.";
            }
            else
            {
                TempData["Error"] =
                    "Thanh toán chưa được xác nhận. Nếu đã trừ tiền, vui lòng đợi vài phút rồi tải lại trang.";
            }

            return View();
        }

        [HttpPost]
        public IActionResult Success(int orderId)
        {
            TempData["Success"] =
                $"Order #{orderId} paid successfully";

            return RedirectToAction("Index", "Cart");
        }

        [HttpPost]
        public IActionResult Fail(int orderId)
        {
            TempData["Error"] =
                $"Order #{orderId} payment failed";

            return RedirectToAction("Index", "Cart");
        }

        private string? GetAccessTokenFromSession()
        {
            var token = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrWhiteSpace(token))
            {
                token = HttpContext.Session.GetString("AccessToken");
            }

            return string.IsNullOrWhiteSpace(token) ? null : token.Trim();
        }

        private async Task<bool> TryRefreshAccessTokenAsync()
        {
            var refreshToken = HttpContext.Session.GetString("RefreshToken");
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return false;
            }

            var refreshResponse = await _httpClient.PostAsJsonAsync(
                Api("api/auth/refresh"),
                new { refreshToken });

            if (!refreshResponse.IsSuccessStatusCode)
            {
                return false;
            }

            using var doc = JsonDocument.Parse(
                await refreshResponse.Content.ReadAsStringAsync());

            if (!doc.RootElement.TryGetProperty("accessToken", out var tokenElement))
            {
                return false;
            }

            var newAccessToken = tokenElement.GetString();
            if (string.IsNullOrWhiteSpace(newAccessToken))
            {
                return false;
            }

            HttpContext.Session.SetString("JWT", newAccessToken);
            HttpContext.Session.SetString("AccessToken", newAccessToken);
            return true;
        }
    }
}
