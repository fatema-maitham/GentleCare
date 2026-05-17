using ClinicReportingApp.Services;

var builder = WebApplication.CreateBuilder(args);

// ── MVC ───────────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

// ── Session (used to store JWT token between requests) ────────────────────────
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ── HttpClient for API calls ──────────────────────────────────────────────────
builder.Services.AddHttpClient<IClinicApiService, ClinicApiService>();

// ── App Services ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<ITokenService, TokenService>();

// ── HttpContextAccessor (needed to read session in services) ─────────────────
builder.Services.AddHttpContextAccessor();


Console.WriteLine(">>> Services registered");

var app = builder.Build();

// ── Middleware Pipeline ───────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();          // Must come before MapControllerRoute
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
