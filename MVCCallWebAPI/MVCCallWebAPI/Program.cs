using Microsoft.AspNetCore.Authentication.Cookies;
using MVCCallWebAPI.Services;
using MVCCallWebAPI.Services.Interface;

var builder = WebApplication.CreateBuilder(args);

// ================= CONFIG =================
var apiUrl = builder.Configuration.GetValue<string>("ApiSettings:BaseUrl")
             ?? throw new InvalidOperationException(
                 "Configuration value 'ApiSettings:BaseUrl' is missing or empty."
             );

// ================= MVC =================
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

// ================= AUTHENTICATION =================
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";

        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);

        options.SlidingExpiration = true;
    });

// ================= AUTHORIZATION =================
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CreateProduct",
        policy => policy.RequireRole("Admin", "Staff"));

    options.AddPolicy("DeleteProduct",
        policy => policy.RequireRole("Admin"));

    options.AddPolicy("ViewDashboard",
        policy => policy.RequireRole("Admin"));
});

// ================= HTTP CLIENT =================
builder.Services.AddHttpClient();

builder.Services
    .AddHttpClient<IApiService, ApiService>(client =>
    {
        client.BaseAddress = new Uri(apiUrl);

        client.Timeout = TimeSpan.FromSeconds(30);

        client.DefaultRequestHeaders.Add(
            "Accept",
            "application/json");
    })
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler
                    .DangerousAcceptAnyServerCertificateValidator
        };
    });

// ================= SESSION =================
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);

    options.Cookie.HttpOnly = true;

    options.Cookie.IsEssential = true;
});

// ================= DEPENDENCY INJECTION =================
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ApiClientService>();

builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddScoped<ICategoryService, CategoryService>();

builder.Services.AddScoped<IAdminService, AdminService>();

// ================= BUILD APP =================
var app = builder.Build();

// ================= PIPELINE =================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();

app.UseAuthorization();

// ================= ROUTES =================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();