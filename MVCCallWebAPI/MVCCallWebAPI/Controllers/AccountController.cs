using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVCCallWebAPI.DTOs;
using MVCCallWebAPI.Helpers;
using MVCCallWebAPI.Models;
using MVCCallWebAPI.ViewModels;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
[AllowAnonymous]
public class AccountController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public AccountController(
        IHttpClientFactory factory,
        IConfiguration configuration)
    {
        _httpClient = factory.CreateClient();
        _configuration = configuration;
    }

    private string Api(string path) => ApiConfig.ApiUrl(_configuration, path);
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
            Api("api/auth/login"),
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
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            ViewBag.Error = "Login failed: token was not returned by API.";
            return View(model);
        }

        HttpContext.Session.SetString("JWT", accessToken);
        HttpContext.Session.SetString("AccessToken", accessToken);
        HttpContext.Session.SetString("RefreshToken", refreshToken ?? string.Empty);
        HttpContext.Session.SetString("UserName", userName ?? string.Empty);
        var roleList = new List<string>();
        if (!string.IsNullOrWhiteSpace(role))
        {
            roleList.AddRange(
                role.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        }

        try
        {
            using var rolesDoc = System.Text.Json.JsonDocument.Parse(content);
            if (rolesDoc.RootElement.TryGetProperty("roles", out var rolesProp) &&
                rolesProp.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                roleList.Clear();
                foreach (var r in rolesProp.EnumerateArray())
                {
                    var roleName = r.GetString();
                    if (!string.IsNullOrWhiteSpace(roleName))
                    {
                        roleList.Add(roleName);
                    }
                }
            }
        }
        catch
        {
        }

        HttpContext.Session.SetString("Role", roleList.FirstOrDefault() ?? string.Empty);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, userName ?? string.Empty)
        };

        foreach (var roleName in roleList.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            claims.Add(new Claim(ClaimTypes.Role, roleName));
        }

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
    public async Task<IActionResult> Logout()
    {
        HttpContext.Session.Remove("JWT");
        HttpContext.Session.Remove("AccessToken");
        HttpContext.Session.Remove("RefreshToken");
        HttpContext.Session.Remove("UserName");
        HttpContext.Session.Remove("Role");

        HttpContext.Session.Clear();

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
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
            Api("api/auth/register"),
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
    [HttpGet]
    public async Task<IActionResult> EditProfile()
    {
        var token = GetAccessTokenFromSession();
        if (string.IsNullOrWhiteSpace(token))
        {
            TempData["Error"] = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
            return RedirectToAction("Login");
        }

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            Api("api/account/profile"));
        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request);
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            var refreshed = await TryRefreshAccessTokenAsync();
            if (!refreshed)
            {
                TempData["Error"] = "Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login");
            }

            var retryRequest = new HttpRequestMessage(
                HttpMethod.Get,
                Api("api/account/profile"));
            retryRequest.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", GetAccessTokenFromSession());
            response = await _httpClient.SendAsync(retryRequest);
        }

        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = $"Không thể tải thông tin profile ({(int)response.StatusCode}).";
            return View(new ProfileViewModel());
        }

        var profile = await response.Content.ReadFromJsonAsync<ProfileViewModel>();
        return View(profile ?? new ProfileViewModel());
    }
    [HttpPost]
    public async Task<IActionResult> EditProfile(
    ProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var token = GetAccessTokenFromSession();
        if (string.IsNullOrWhiteSpace(token))
        {
            TempData["Error"] = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
            return RedirectToAction("Login");
        }

        var request = new HttpRequestMessage(
            HttpMethod.Put,
            Api("api/account/profile"))
        {
            Content = JsonContent.Create(new
            {
                fullName = model.FullName,
                phoneNumber = model.PhoneNumber,
                address = BuildAddress(model)
            })
        };
        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            var refreshed = await TryRefreshAccessTokenAsync();
            if (!refreshed)
            {
                TempData["Error"] = "Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login");
            }

            var retryRequest = new HttpRequestMessage(
                HttpMethod.Put,
                Api("api/account/profile"))
            {
                Content = JsonContent.Create(new
                {
                    fullName = model.FullName,
                    phoneNumber = model.PhoneNumber,
                    address = BuildAddress(model)
                })
            };
            retryRequest.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", GetAccessTokenFromSession());
            response = await _httpClient.SendAsync(retryRequest);
        }

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Error =
                $"Cập nhật profile thất bại ({(int)response.StatusCode})";
            return View(model);
        }

        TempData["Success"] =
            "Profile updated successfully";

        return RedirectToAction("EditProfile");
    }

    private static string? BuildAddress(ProfileViewModel model)
    {
        if (!string.IsNullOrWhiteSpace(model.Address))
        {
            return model.Address.Trim();
        }

        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(model.AddressDetail))
        {
            parts.Add(model.AddressDetail.Trim());
        }

        if (!string.IsNullOrWhiteSpace(model.WardName))
        {
            parts.Add(model.WardName.Trim());
        }

        if (!string.IsNullOrWhiteSpace(model.DistrictName))
        {
            parts.Add(model.DistrictName.Trim());
        }

        if (!string.IsNullOrWhiteSpace(model.ProvinceName))
        {
            parts.Add(model.ProvinceName.Trim());
        }

        return parts.Count == 0 ? null : string.Join(", ", parts);
    }

    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View(new ChangePasswordViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var token = GetAccessTokenFromSession();
        if (string.IsNullOrWhiteSpace(token))
        {
            TempData["Error"] = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
            return RedirectToAction("Login");
        }

        var payload = new
        {
            currentPassword = model.CurrentPassword,
            newPassword = model.NewPassword
        };

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            Api("api/account/change-password"))
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request);
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            var refreshed = await TryRefreshAccessTokenAsync();
            if (!refreshed)
            {
                TempData["Error"] = "Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login");
            }

            var retryRequest = new HttpRequestMessage(
                HttpMethod.Post,
                Api("api/account/change-password"))
            {
                Content = JsonContent.Create(payload)
            };
            retryRequest.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", GetAccessTokenFromSession());
            response = await _httpClient.SendAsync(retryRequest);
        }

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Error = "Đổi mật khẩu thất bại. Vui lòng kiểm tra mật khẩu hiện tại.";
            return View(model);
        }

        TempData["Success"] = "Đổi mật khẩu thành công.";
        return RedirectToAction(nameof(ChangePassword));
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
                Api("api/auth/refresh"),
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
        return true;
    }
}