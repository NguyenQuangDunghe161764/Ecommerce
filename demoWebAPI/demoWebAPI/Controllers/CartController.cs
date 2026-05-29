using Microsoft.AspNetCore.Mvc;

namespace demoWebAPI.Controllers
{
    public class CartController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Checkout()
        {
            // FAKE ORDER ID
            int orderId =
                new Random().Next(1000, 9999);

            return RedirectToAction(
                "FakePay",
                "Payment",
                new { orderId = orderId });
        }
    }
}