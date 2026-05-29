using Microsoft.AspNetCore.Mvc;
using MVCCallWebAPI.Helpers;
using MVCCallWebAPI.Models;
using MVCCallWebAPI.Services.Interface;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

public class CartController : Controller
{
    private readonly IProductService _productService;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public CartController(
        IProductService productService,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _productService = productService;
        _httpClient = httpClientFactory.CreateClient();
        _configuration = configuration;
    }

    private string Api(string path) => ApiConfig.ApiUrl(_configuration, path);

    // VIEW CART
    public IActionResult Index()
    {
        var cart = GetCartFromSession();

        return View(cart);
    }

    // ADD TO CART
    public async Task<IActionResult> AddToCart(int productId)
    {
        var product = await _productService.GetProductByIdAsync(productId);

        if (product == null)
        {
            return NotFound();
        }

        var cart = GetCartFromSession();

        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);

        if (existingItem != null)
        {
            existingItem.Quantity++;
        }
        else
        {
            cart.Items.Add(new CartItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Price = product.Price,
                Quantity = 1
            });
        }

        SaveCartToSession(cart);

        return RedirectToAction("Index", "Product");
    }
    // REMOVE
    public IActionResult Remove(int productId)
    {
        var cart = GetCartFromSession();

        var item =
            cart.Items.FirstOrDefault(
                i => i.ProductId == productId);

        if (item != null)
        {
            cart.Items.Remove(item);
        }

        SaveCartToSession(cart);

        return RedirectToAction("Index");
    }

    // CLEAR CART
    public IActionResult Clear()
    {
        HttpContext.Session.Remove("Cart");

        return RedirectToAction("Index");
    }

    // UPDATE QUANTITY
    [HttpPost]
    public IActionResult UpdateQuantity(
        int productId,
        int quantity)
    {
        var cart = GetCartFromSession();

        var item =
            cart.Items.FirstOrDefault(
                i => i.ProductId == productId);

        if (item != null)
        {
            item.Quantity = quantity;
        }

        SaveCartToSession(cart);

        return RedirectToAction("Index");
    }

    // SESSION METHODS

    private void SaveCartToSession(
        ShoppingCart cart)
    {
        var json =
            JsonConvert.SerializeObject(cart);

        HttpContext.Session.SetString(
            "Cart",
            json);
    }

    private ShoppingCart GetCartFromSession()
    {
        var json =
            HttpContext.Session.GetString("Cart");

        return string.IsNullOrEmpty(json)
            ? new ShoppingCart()
            : JsonConvert
                .DeserializeObject<ShoppingCart>(
                    json);
    }

    [HttpPost]
    public async Task<IActionResult> Checkout()
    {
        var cart = GetCartFromSession();

        if (cart.Items == null || !cart.Items.Any())
        {
            TempData["Error"] = "Giỏ hàng trống.";
            return RedirectToAction("Index");
        }

        var token = GetAccessTokenFromSession();
        if (string.IsNullOrWhiteSpace(token))
        {
            TempData["Error"] = "Vui lòng đăng nhập để thanh toán.";
            return RedirectToAction("Login", "Account");
        }

        var checkoutPayload = new
        {
            items = cart.Items.Select(i => new
            {
                productId = i.ProductId,
                quantity = i.Quantity
            })
        };

        var checkoutRequest = new HttpRequestMessage(
            HttpMethod.Post,
            Api("api/orders/checkout"))
        {
            Content = JsonContent.Create(checkoutPayload)
        };
        checkoutRequest.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var checkoutResponse = await _httpClient.SendAsync(checkoutRequest);
        if (checkoutResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            if (!await TryRefreshAccessTokenAsync())
            {
                TempData["Error"] = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Account");
            }

            checkoutRequest.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", GetAccessTokenFromSession());
            checkoutResponse = await _httpClient.SendAsync(checkoutRequest);
        }

        if (!checkoutResponse.IsSuccessStatusCode)
        {
            var err = await checkoutResponse.Content.ReadAsStringAsync();
            TempData["Error"] = $"Không tạo được đơn hàng: {err}";
            return RedirectToAction("Index");
        }

        using var checkoutDoc = JsonDocument.Parse(
            await checkoutResponse.Content.ReadAsStringAsync());
        var orderId = checkoutDoc.RootElement.GetProperty("orderId").GetInt32();

        var zaloRequest = new HttpRequestMessage(
            HttpMethod.Post,
            Api("api/zalopay/create"))
        {
            Content = JsonContent.Create(new { orderId })
        };
        zaloRequest.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", GetAccessTokenFromSession());

        var zaloResponse = await _httpClient.SendAsync(zaloRequest);
        if (!zaloResponse.IsSuccessStatusCode)
        {
            var err = await zaloResponse.Content.ReadAsStringAsync();
            TempData["Error"] = $"Không tạo được thanh toán ZaloPay: {err}";
            return RedirectToAction("Index");
        }

        using var zaloDoc = JsonDocument.Parse(
            await zaloResponse.Content.ReadAsStringAsync());
        var orderUrl = zaloDoc.RootElement.GetProperty("orderUrl").GetString();

        if (string.IsNullOrWhiteSpace(orderUrl))
        {
            TempData["Error"] = "ZaloPay không trả về link thanh toán.";
            return RedirectToAction("Index");
        }

        return Redirect(orderUrl);
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