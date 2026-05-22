using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using MVCCallWebAPI.DTOs;
using MVCCallWebAPI.Models;
using MVCCallWebAPI.ViewModels;
using System.Net.Http.Json;
[AllowAnonymous]
public class AccountController : Controller
{
    private readonly HttpClient _httpClient;
    
    public AccountController(IHttpClientFactory factory)
    {
        _httpClient = factory.CreateClient();
    }
    public IActionResult AccessDenied()
        {
            return View();
        }
    // LOGIN VIEW
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    // LOGIN POST
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "https://localhost:7208/api/auth/login",
            model
        );

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Error = "Login failed";
            return View();
        }

        var content = await response.Content.ReadAsStringAsync();

        // Try to parse JSON flexibly to find token/role fields
        string accessToken = null;
        string refreshToken = null;
        string userName = null;
        string role = null;

        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(content);
            var root = doc.RootElement;

            // helper local func
            static bool TryGetString(System.Text.Json.JsonElement el, string[] names, out string value)
            {
                foreach (var name in names)
                {
                    if (el.TryGetProperty(name, out var prop))
                    {
                        if (prop.ValueKind == System.Text.Json.JsonValueKind.String)
                        {
                            value = prop.GetString();
                            return true;
                        }
                        else if (prop.ValueKind == System.Text.Json.JsonValueKind.Array)
                        {
                            var items = prop.EnumerateArray().Select(p => p.GetString()).Where(s => s != null);
                            value = string.Join(",", items);
                            return true;
                        }
                    }
                }

                value = null;
                return false;
            }

            // top-level
            TryGetString(root, new[] { "accessToken", "access_token", "token", "jwt" }, out accessToken);
            TryGetString(root, new[] { "refreshToken", "refresh_token" }, out refreshToken);
            TryGetString(root, new[] { "userName", "username", "name" }, out userName);
            TryGetString(root, new[] { "role", "roles" }, out role);

            // some APIs wrap data under 'data' or 'result'
            if ((accessToken == null || role == null) && root.TryGetProperty("data", out var dataEl))
            {
                TryGetString(dataEl, new[] { "accessToken", "access_token", "token", "jwt" }, out accessToken);
                TryGetString(dataEl, new[] { "refreshToken", "refresh_token" }, out refreshToken);
                TryGetString(dataEl, new[] { "userName", "username", "name" }, out userName);
                TryGetString(dataEl, new[] { "role", "roles" }, out role);
            }
            if ((accessToken == null || role == null) && root.TryGetProperty("result", out var resultEl))
            {
                TryGetString(resultEl, new[] { "accessToken", "access_token", "token", "jwt" }, out accessToken);
                TryGetString(resultEl, new[] { "refreshToken", "refresh_token" }, out refreshToken);
                TryGetString(resultEl, new[] { "userName", "username", "name" }, out userName);
                TryGetString(resultEl, new[] { "role", "roles" }, out role);
            }
        }
        catch
        {
            // ignore parse errors
        }

        // If still null, try strong-typed deserialization to known DTO
        if (accessToken == null)
        {
            try
            {
                var dto = System.Text.Json.JsonSerializer.Deserialize<LoginResponse>(content, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (dto != null)
                {
                    accessToken ??= dto.accessToken;
                    refreshToken ??= dto.refreshToken;
                    userName ??= dto.userName;
                    role ??= dto.role;
                }
            }
            catch
            {
            }
        }

        // store in session
        HttpContext.Session.SetString("JWT", accessToken ?? string.Empty);
        HttpContext.Session.SetString("RefreshToken", refreshToken ?? string.Empty);
        HttpContext.Session.SetString("UserName", userName ?? string.Empty);
        HttpContext.Session.SetString("Role", role ?? string.Empty);

        // Create authentication cookie so [Authorize] works with Role checks
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, userName ?? string.Empty),
            new Claim(ClaimTypes.Role, role ?? string.Empty)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        // If role is missing, add debug info so you can inspect response in UI during development
        if (string.IsNullOrEmpty(role))
        {
            TempData["LoginDebug"] = content.Length > 1000 ? content.Substring(0, 1000) + "..." : content;
        }

        return RedirectToAction("Index", "Home");
    }

    // LOGOUT
    public IActionResult Logout()
    {
        HttpContext.Session.Remove("JWT");
        return RedirectToAction("Login");
    }
    // GET: REGISTER
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    // POST: REGISTER
    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model); 
        }
        var response = await _httpClient.PostAsJsonAsync(
            "https://localhost:7208/api/auth/register",
            model
        );


        if (!response.IsSuccessStatusCode)
        {
            var errors = await response.Content.ReadFromJsonAsync<List<IdentityErrorResponse>>();

            ViewBag.Error = errors != null
                ? string.Join("<br/>", errors.Select(e => "• " + e.Description))
                : "Register failed";

            return View(model);
        }
        return RedirectToAction("Login", "Account");
    }
}