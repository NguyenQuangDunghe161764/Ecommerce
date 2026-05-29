//using Microsoft.AspNetCore.Mvc;
//using Newtonsoft.Json;
//using System.Net;
//using System.Net.Http.Headers;

//public class CheckoutController : Controller
//{
//    private readonly HttpClient _httpClient;

//    public CheckoutController(IHttpClientFactory httpClientFactory)
//    {
//        _httpClient = httpClientFactory.CreateClient();
//        _httpClient.BaseAddress = new Uri("https://localhost:7157/"); // Thay bằng URL Web API của bạn
//    }

//    public async Task<IActionResult> Index()
//    {
//        var token = HttpContext.Session.GetString("JWT");
//        if (string.IsNullOrEmpty(token))
//        {
//            return RedirectToAction("Login", "Account");
//        }

//        // 1. Gọi Web API lấy danh sách địa chỉ của User
//        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
//        var response = await _httpClient.GetAsync("api/Address");

//        var addresses = new List<Address>();
//        if (response.IsSuccessStatusCode)
//        {
//            var json = await response.Content.ReadAsStringAsync();
//            addresses = JsonConvert.DeserializeObject<List<Address>>(json);
//        }

//        // 2. Giả lập tính tổng tiền hàng từ Giỏ hàng (Bạn thay bằng logic lấy giỏ hàng thực tế của bạn nhé)
//        decimal subTotal = 550000; // Ví dụ: 550.000 đ

//        var viewModel = new CheckoutViewModel
//        {
//            Addresses = addresses,
//            SubTotal = subTotal
//        };

//        return View(viewModel);
//    }
//}