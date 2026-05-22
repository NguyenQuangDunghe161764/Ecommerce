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

    public async Task<IActionResult> Index(string keyword, int? categoryId, int page = 1)
    {
        var result = await _productService.GetProductsAsync(
            keyword,
            categoryId,
            page,
            5
        );

        ViewBag.Categories = await _categoryService.GetCategoriesAsync()
                             ?? new List<CategoryViewModel>();

        ViewBag.Keyword = keyword;
        ViewBag.CategoryId = categoryId;
        ViewBag.Page = page;

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
    public async Task<IActionResult> Create(CreateProductViewModel model)
    {
        var token = HttpContext.Session.GetString("JWT");

        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.PostAsJsonAsync(
            "https://localhost:7208/api/products",
            model);

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Error = await response.Content.ReadAsStringAsync();
            ViewBag.Categories = await _categoryService.GetCategoriesAsync();
            return View(model);
        }

        return RedirectToAction("Index");
    }
}