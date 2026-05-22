using Microsoft.AspNetCore.Mvc;
using MVCCallWebAPI.Models;
using MVCCallWebAPI.Services.Interface;
using Newtonsoft.Json;

public class CartController : Controller
{
    private readonly IProductService _productService;

    public CartController(
        IProductService productService)
    {
        _productService = productService;
    }

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

        return Redirect(Request.Headers["Referer"].ToString());
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
}