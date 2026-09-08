using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KMC.WebClient.Models;
using KMC.WebClient.Services;

namespace KMC.WebClient.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly RegistrationApiClient _registrationApi;
        private readonly AuthApiClient _authApi;

        public DashboardController(RegistrationApiClient registrationApi, AuthApiClient authApi)
        {
            _registrationApi = registrationApi;
            _authApi = authApi;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? tab = "events", string? successMessage = null, string? errorMessage = null)
        {
            var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
            var name = User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
            var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

            var registrations = await _registrationApi.GetMyRegistrationsAsync(email);

            var vm = new DashboardViewModel
            {
                FullName = name,
                Email = email,
                Role = role,
                RegisteredEvents = registrations,
                ProfileModel = new ProfileViewModel { FullName = name },
                PasswordModel = new ChangePasswordViewModel(),
                SuccessMessage = successMessage,
                ErrorMessage = errorMessage
            };

            ViewBag.ActiveTab = tab;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
        {
            var token = User.FindFirstValue("jwt_token") ?? string.Empty;
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index), new { tab = "profile", errorMessage = "Invalid profile data." });
            }

            var result = await _authApi.UpdateProfileAsync(model.FullName, token);
            if (result.Success)
            {
                return RedirectToAction(nameof(Index), new { tab = "profile", successMessage = "Profile name updated successfully." });
            }

            return RedirectToAction(nameof(Index), new { tab = "profile", errorMessage = result.Error ?? "Failed to update profile." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var token = User.FindFirstValue("jwt_token") ?? string.Empty;
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index), new { tab = "profile", errorMessage = "Please check password requirements." });
            }

            var result = await _authApi.ChangePasswordAsync(model.CurrentPassword, model.NewPassword, token);
            if (result.Success)
            {
                return RedirectToAction(nameof(Index), new { tab = "profile", successMessage = "Password changed successfully." });
            }

            return RedirectToAction(nameof(Index), new { tab = "profile", errorMessage = result.Error ?? "Failed to change password." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelRegistration(int id)
        {
            var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
            var result = await _registrationApi.CancelRegistrationAsync(id, email);
            if (result.Success)
            {
                return RedirectToAction(nameof(Index), new { tab = "events", successMessage = "Event registration cancelled successfully." });
            }

            return RedirectToAction(nameof(Index), new { tab = "events", errorMessage = result.Error ?? "Failed to cancel registration." });
        }
    }
}
