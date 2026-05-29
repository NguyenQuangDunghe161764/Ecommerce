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
        private const string BaseUrl = "https://localhost:7208/api/address";

        public AddressController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // ================= INDEX =================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("JWT");

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            var request = new HttpRequestMessage(HttpMethod.Get, BaseUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return RedirectToAction("Login", "Account");

            if (!response.IsSuccessStatusCode)
                return View(new List<AddressViewModel>());

            var json = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<List<AddressViewModel>>(json)
                         ?? new List<AddressViewModel>();

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
                return View(model);

            var token = HttpContext.Session.GetString("JWT");

            if (string.IsNullOrEmpty(token))
            {
                ViewBag.Error = "Bạn chưa đăng nhập hoặc token đã hết hạn.";
                return View(model);
            }

            var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var json = JsonConvert.SerializeObject(model);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                ViewBag.Error = string.IsNullOrWhiteSpace(error)
                    ? $"Lỗi API: {(int)response.StatusCode} {response.StatusCode}"
                    : error;

                return View(model);
            }

            return RedirectToAction("Index");
        }

        // ================= SET DEFAULT =================
        [HttpPost]
        public async Task<IActionResult> SetDefault(int id)
        {
            var token = HttpContext.Session.GetString("JWT");

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            var request = new HttpRequestMessage(HttpMethod.Put, $"{BaseUrl}/{id}/default");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                TempData["Error"] = "Không thể đặt mặc định!";

            return RedirectToAction("Index");
        }

        // ================= DELETE =================
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var token = HttpContext.Session.GetString("JWT");

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            var request = new HttpRequestMessage(HttpMethod.Delete, $"{BaseUrl}/{id}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                TempData["Error"] = "Xóa thất bại!";

            return RedirectToAction("Index");
        }
    }
}