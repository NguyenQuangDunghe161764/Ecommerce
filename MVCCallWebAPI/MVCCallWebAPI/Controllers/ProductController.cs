using System.Net.Http.Headers;
using System.Net.Http.Json;
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
                var streamContent =
                    new StreamContent(image.OpenReadStream());

                streamContent.Headers.ContentType =
                    new System.Net.Http.Headers
                        .MediaTypeHeaderValue(image.ContentType);

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

        return RedirectToAction("Index");
    }
}