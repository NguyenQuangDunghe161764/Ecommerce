using Microsoft.AspNetCore.Mvc;

namespace MVCCallWebAPI.Services
{
    public class CartService : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
