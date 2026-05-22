using Microsoft.AspNetCore.Mvc;

namespace demoWebAPI.Controllers
{
    public class OrdersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
