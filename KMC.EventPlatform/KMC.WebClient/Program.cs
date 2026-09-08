using Microsoft.AspNetCore.Authentication.Cookies;
using KMC.WebClient.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();

var services = builder.Configuration.GetSection("Services");
builder.Services.AddHttpClient<AuthApiClient>(c => c.BaseAddress = new Uri(services["UserAuthBaseUrl"]!));
builder.Services.AddHttpClient<EventApiClient>(c => c.BaseAddress = new Uri(services["EventManagementBaseUrl"]!));
builder.Services.AddHttpClient<RegistrationApiClient>(c => c.BaseAddress = new Uri(services["RegistrationBaseUrl"]!));
builder.Services.AddHttpClient<SearchApiClient>(c => c.BaseAddress = new Uri(services["SearchBaseUrl"]!));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
