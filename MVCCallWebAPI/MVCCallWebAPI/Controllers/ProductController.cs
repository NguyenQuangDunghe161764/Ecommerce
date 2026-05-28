using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVCCallWebAPI.Services.Interface;
using MVCCallWebAPI.ViewModels;

public class ProductController : Controller
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly HttpClient _httpClient;

    public ProductController(
        IProductService productService,
        ICategoryService categoryService,
        HttpClient httpClient)
    {
        _productService = productService;
        _categoryService = categoryService;
        _httpClient = httpClient;
    }

    public async Task<IActionResult> Index(
        string keyword,
        int? categoryId,
        int page = 1)
    {
        var result =
            await _productService.GetProductsAsync(
                keyword,
                categoryId,
                page,
                5);

        ViewBag.Categories =
            await _categoryService.GetCategoriesAsync();

        return View(result);
    }
    public async Task<IActionResult> Details(int id)
    {
        var product =
            await _productService.GetProductByIdAsync(id);

        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }
    [HttpGet]
    [Permission("Product.Create")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Categories =
            await _categoryService.GetCategoriesAsync();

        return View();
    }

    [HttpPost]
    [Permission("Product.Create")]
    public async Task<IActionResult> Create(
        [FromForm] CreateProductViewModel model)
    {
        var token =
            HttpContext.Session.GetString("JWT");

        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers
                .AuthenticationHeaderValue(
                    "Bearer",
                    token);

        var content = new MultipartFormDataContent();

        content.Add(
            new StringContent(model.Name),
            "Name");

        content.Add(
            new StringContent(model.Description ?? ""),
            "Description");

        content.Add(
            new StringContent(model.Price.ToString()),
            "Price");

        content.Add(
            new StringContent(model.Stock.ToString()),
            "Stock");

        content.Add(
            new StringContent(model.CategoryId.ToString()),
            "CategoryId");

        // IMAGES
        if (model.Images != null)
        {
            foreach (var image in model.Images)
            {
                if (image == null || image.Length <= 0)
                    continue;

                var streamContent =
                    new StreamContent(image.OpenReadStream());

                streamContent.Headers.ContentType =
                    new MediaTypeHeaderValue(image.ContentType);

                content.Add(
                    streamContent,
                    "Images",
                    image.FileName);
            }
        }

        var response =
    await _httpClient.PostAsync(
        "https://localhost:7208/api/products",
        content);

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Error =
                await response.Content.ReadAsStringAsync();

            ViewBag.Categories =
                await _categoryService.GetCategoriesAsync();

            return View(model);
        }

        // LẤY PRODUCT VỪA TẠO
        var createdProduct =
            await response.Content
                .ReadFromJsonAsync<ProductViewModel>();

        return RedirectToAction(
            "Details",
            new { id = createdProduct.Id });
    }
    [HttpGet]
    [Permission("Product.Edit")]
    public async Task<IActionResult> Edit(int id)
    {
        var product =
            await _productService.GetProductByIdAsync(id);
        Console.WriteLine(product.MainImageUrl);

        Console.WriteLine(product.Images?.Count);
        if (product == null)
        {
            return NotFound();
        }

        ViewBag.Categories =
            await _categoryService.GetCategoriesAsync();

        return View(product);
    }
    [HttpPost]
    [Permission("Product.Edit")]
    public async Task<IActionResult> Edit(ProductViewModel model)
    {
        var token = GetAccessTokenFromSession();
        if (string.IsNullOrWhiteSpace(token))
        {
            TempData["Error"] = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
            return RedirectToAction("Login", "Account");
        }

        var content = new MultipartFormDataContent();

        content.Add(
            new StringContent(model.Name ?? ""),
            "Name");

        content.Add(
            new StringContent(model.Price.ToString()),
            "Price");

        content.Add(
            new StringContent(model.Stock.ToString()),
            "Stock");

        content.Add(
            new StringContent(model.CategoryId.ToString()),
            "CategoryId");

        content.Add(
            new StringContent(model.Description ?? ""),
            "Description");

        if (!string.IsNullOrEmpty(model.MainImageUrl))
        {
            content.Add(
                new StringContent(model.MainImageUrl),
                "MainImageUrl");
        }

        // DELETE IMAGES
        if (model.DeletedImages != null)
        {
            foreach (var img in model.DeletedImages)
            {
                content.Add(
                    new StringContent(img),
                    "DeletedImages");
            }
        }

        // NEW IMAGES
        if (model.Images != null)
        {
            foreach (var image in model.Images)
            {
                var streamContent =
                    new StreamContent(image.OpenReadStream());

                streamContent.Headers.ContentType =
                    new MediaTypeHeaderValue(image.ContentType);

                content.Add(
                    streamContent,
                    "Images",
                    image.FileName);
            }
        }

        var request = new HttpRequestMessage(
            HttpMethod.Put,
            $"https://localhost:7208/api/products/{model.Id}")
        {
            Content = content
        };
        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            var refreshed = await TryRefreshAccessTokenAsync();

            if (refreshed)
            {
                var retryContent = new MultipartFormDataContent();

                retryContent.Add(
                    new StringContent(model.Name ?? ""),
                    "Name");

                retryContent.Add(
                    new StringContent(model.Price.ToString()),
                    "Price");

                retryContent.Add(
                    new StringContent(model.Stock.ToString()),
                    "Stock");

                retryContent.Add(
                    new StringContent(model.CategoryId.ToString()),
                    "CategoryId");

                retryContent.Add(
                    new StringContent(model.Description ?? ""),
                    "Description");

                if (!string.IsNullOrEmpty(model.MainImageUrl))
                {
                    retryContent.Add(
                        new StringContent(model.MainImageUrl),
                        "MainImageUrl");
                }

                if (model.DeletedImages != null)
                {
                    foreach (var img in model.DeletedImages)
                    {
                        retryContent.Add(
                            new StringContent(img),
                            "DeletedImages");
                    }
                }

                if (model.Images != null)
                {
                    foreach (var image in model.Images)
                    {
                        var streamContent =
                            new StreamContent(image.OpenReadStream());

                        streamContent.Headers.ContentType =
                            new MediaTypeHeaderValue(image.ContentType);

                        retryContent.Add(
                            streamContent,
                            "Images",
                            image.FileName);
                    }
                }

                var retryToken = GetAccessTokenFromSession();
                var retryRequest = new HttpRequestMessage(
                    HttpMethod.Put,
                    $"https://localhost:7208/api/products/{model.Id}")
                {
                    Content = retryContent
                };
                retryRequest.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", retryToken);

                response = await _httpClient.SendAsync(retryRequest);
            }
            else
            {
                TempData["Error"] = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Account");
            }
        }

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            HttpContext.Session.Remove("JWT");
            HttpContext.Session.Remove("AccessToken");
            TempData["Error"] = "Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.";
            return RedirectToAction("Login", "Account");
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorContent =
                await response.Content.ReadAsStringAsync();

            ViewBag.Error =
                string.IsNullOrWhiteSpace(errorContent)
                    ? $"Update failed ({(int)response.StatusCode})"
                    : errorContent;

            // Reload current data so image list is not lost on failed save
            var existingProduct =
                await _productService.GetProductByIdAsync(model.Id);

            if (existingProduct != null)
            {
                model.ExistingImages =
                    existingProduct.ExistingImages ?? new List<string>();

                model.MainImageUrl =
                    string.IsNullOrWhiteSpace(model.MainImageUrl)
                        ? existingProduct.MainImageUrl
                        : model.MainImageUrl;

                model.CategoryId =
                    model.CategoryId == 0
                        ? existingProduct.CategoryId
                        : model.CategoryId;

                if (string.IsNullOrWhiteSpace(model.Description))
                {
                    model.Description = existingProduct.Description;
                }
            }

            ViewBag.Categories =
                await _categoryService.GetCategoriesAsync();

            return View(model);
        }

        return RedirectToAction(
            "Details",
            new { id = model.Id });
    }

    private async Task<bool> TryRefreshAccessTokenAsync()
    {
        var refreshToken =
            HttpContext.Session.GetString("RefreshToken");

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return false;
        }

        var refreshResponse =
            await _httpClient.PostAsJsonAsync(
                "https://localhost:7208/api/auth/refresh",
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
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", newAccessToken);

        return true;
    }

    private string? GetAccessTokenFromSession()
    {
        var token = HttpContext.Session.GetString("JWT");
        if (string.IsNullOrWhiteSpace(token))
        {
            token = HttpContext.Session.GetString("AccessToken");
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        token = token.Trim();
        if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            token = token.Substring("Bearer ".Length).Trim();
        }

        return string.IsNullOrWhiteSpace(token) ? null : token;
    }
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var product =
            await _productService.GetProductByIdAsync(id);

        if (product == null)
        {
            return NotFound();
        }

        return View(product);
    }
    [HttpPost]
    public async Task<IActionResult> Delete(ProductViewModel model)
    {
        var token =
            HttpContext.Session.GetString("JWT");

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        var response =
            await _httpClient.DeleteAsync(
                $"https://localhost:7208/api/products/{model.Id}");

        return RedirectToAction("Index");
    }
}