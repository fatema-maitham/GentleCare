using ClinicReportingApp.Services;
using ClinicReportingApp.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add MVC services.
builder.Services.AddControllersWithViews();


// Session info storage if needed by MVC pages.
builder.Services.AddSession();


//Register Cookie Authentication
builder.Services
 .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
 .AddCookie(options =>
 {
     options.LoginPath = "/Auth/Login";
     options.AccessDeniedPath = "/Auth/AccessDenied";
     options.AccessDeniedPath = "/Auth/Logout";
     options.ExpireTimeSpan = TimeSpan.FromHours(1);
     options.SlidingExpiration = false;
 });

//  HttpContextAccessor (needed to read session in services)
builder.Services.AddHttpContextAccessor();



// HttpClient for the public lookup page that calls the Web API.
builder.Services.AddHttpClient<IClinicApiService, ClinicApiService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7117/");
});

//  App Services 
builder.Services.AddScoped<ITokenService, TokenService>();

var app = builder.Build();

//Middleware Pipeline (as in Lab 4.3)
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");



app.Run();



