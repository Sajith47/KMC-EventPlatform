using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using KMC.WebClient.Models;
using KMC.WebClient.Services;

namespace KMC.WebClient.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthApiClient _authApi;

        public AccountController(AuthApiClient authApi)
        {
            _authApi = authApi;
        }

        [HttpGet]
        public IActionResult Register() => View(new RegisterViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _authApi.RegisterAsync(model.FullName, model.Email, model.Password, model.Role);
            if (!result.Success || result.Data is null)
            {
                model.ErrorMessage = result.Error ?? "Registration failed.";
                return View(model);
            }

            await SignInAsync(result.Data);
            return RedirectToDefault(result.Data.Role);
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _authApi.LoginAsync(model.Email, model.Password);
            if (!result.Success || result.Data is null)
            {
                model.ErrorMessage = result.Error ?? "Invalid email or password.";
                return View(model);
            }

            await SignInAsync(result.Data);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToDefault(result.Data.Role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied() => View();

        private async Task SignInAsync(AuthResult auth)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, auth.UserId.ToString()),
                new(ClaimTypes.Name, auth.FullName),
                new(ClaimTypes.Email, auth.Email),
                new(ClaimTypes.Role, auth.Role),
                new("jwt_token", auth.Token)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = auth.ExpiresAt
            });
        }

        private IActionResult RedirectToDefault(string role) =>
            role == "Organizer" ? RedirectToAction("Index", "Events") : RedirectToAction("Index", "Home");
    }
}
