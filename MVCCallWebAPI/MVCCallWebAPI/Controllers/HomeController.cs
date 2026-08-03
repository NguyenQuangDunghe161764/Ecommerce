using Microsoft.AspNetCore.Mvc;
using MVCCallWebAPI.Services.Interface;
using MVCCallWebAPI.ViewModels;
using System.Diagnostics;

namespace MVCCallWebAPI.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public HomeController(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index(string? keyword, int? categoryId, int page = 1, int pageSize = 8)
        {
            try
            {
                // 1. Lấy danh sách danh mục (đã sửa hàm chuẩn)
                var categories = await _categoryService.GetCategoriesAsync();
                ViewBag.Categories = categories;

                // 2. Lấy danh sách sản phẩm (truyền đủ tham số theo IProductService)
                var productsResult = await _productService.GetProductsAsync(
                    keyword: keyword,
                    categoryId: categoryId,
                    page: page,
                    pageSize: pageSize
                );

                var products = productsResult?.Items ?? new List<ProductViewModel>();

                return View(products);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Không thể kết nối đến hệ thống API. Vui lòng thử lại sau!";
                return View(new List<ProductViewModel>());
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}