using Microsoft.AspNetCore.Mvc;

public class PaymentController : Controller
{
    public IActionResult FakePay(int orderId)
    {
        ViewBag.OrderId = orderId;

        return View();
    }

    public IActionResult Success(int orderId)
    {
        TempData["Success"] =
            "Thanh toán thành công";

        return RedirectToAction(
            "Details",
            "Order",
            new { id = orderId });
    }

    public IActionResult Fail(int orderId)
    {
        TempData["Error"] =
            "Thanh toán thất bại";

        return RedirectToAction(
            "Index",
            "Cart");
    }
}