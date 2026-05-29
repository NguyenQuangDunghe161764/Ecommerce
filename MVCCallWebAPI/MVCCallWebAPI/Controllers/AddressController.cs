using Microsoft.AspNetCore.Mvc;
using MVCCallWebAPI.ViewModels;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace MVCCallWebAPI.Controllers
{
    public class AddressController : Controller
    {
        private readonly HttpClient _httpClient;

        public AddressController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // ================= INDEX =================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("JWT");

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync("https://localhost:7208/api/address");

            if (!response.IsSuccessStatusCode)
            {
                return View(new List<AddressViewModel>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<List<AddressViewModel>>(json);

            if (result == null)
            {
                result = new List<AddressViewModel>();
            }

            return View(result);
        }

        // ================= CREATE GET =================
        [HttpGet]
        public IActionResult Create()
        {
            return View(new AddressViewModel());
        }

        // ================= CREATE POST =================
        [HttpPost]
        public async Task<IActionResult> Create(AddressViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var token = HttpContext.Session.GetString("JWT");

            // Kiểm tra an toàn nếu chưa đăng nhập hoặc Session mất Token
            if (string.IsNullOrEmpty(token))
            {
                ViewBag.Error = "Phiên đăng nhập đã hết hạn hoặc không tìm thấy Token (JWT). Vui lòng đăng nhập lại!";
                return View(model);
            }

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://localhost:7208/api/address", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();

                // CẢI TIẾN: Nếu API trả về chuỗi lỗi trống, phân tích theo StatusCode hệ thống
                if (string.IsNullOrWhiteSpace(errorContent))
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        ViewBag.Error = "Tài khoản không có quyền truy cập hoặc Token không hợp lệ (Mã lỗi: 401 Unauthorized).";
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        ViewBag.Error = "Không tìm thấy đường dẫn gọi API. Vui lòng kiểm tra lại URL (Mã lỗi: 404 Not Found).";
                    }
                    else
                    {
                        ViewBag.Error = $"API trả về lỗi hệ thống không xác định (Mã lỗi: {(int)response.StatusCode} {response.StatusCode}).";
                    }
                }
                else
                {
                    // Nếu API trả về chuỗi text lỗi cụ thể
                    ViewBag.Error = errorContent;
                }

                return View(model);
            }

            return RedirectToAction("Index");
        }

        // ================= SET DEFAULT =================
        [HttpPost] // THÊM ATTRIBUTE: Đồng bộ với Method="POST" của form trong Index.cshtml
        public async Task<IActionResult> SetDefault(int id)
        {
            var token = HttpContext.Session.GetString("JWT");

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            await _httpClient.PutAsync($"https://localhost:7208/api/address/{id}/default", null);

            return RedirectToAction("Index");
        }

        // ================= DELETE =================
        [HttpPost] // THÊM ATTRIBUTE: Đồng bộ với Method="POST" của form trong Index.cshtml
        public async Task<IActionResult> Delete(int id)
        {
            var token = HttpContext.Session.GetString("JWT");

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            await _httpClient.DeleteAsync($"https://localhost:7208/api/address/{id}");

            return RedirectToAction("Index");
        }
    }
}