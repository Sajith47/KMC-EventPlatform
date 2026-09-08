using Microsoft.AspNetCore.Mvc;
using KMC.WebClient.Models;
using KMC.WebClient.Services;

namespace KMC.WebClient.Controllers
{
    // Public-facing pages: browse/search events, view details, register.
    public class HomeController : Controller
    {
        private readonly SearchApiClient _searchApi;
        private readonly EventApiClient _eventApi;
        private readonly RegistrationApiClient _registrationApi;

        public HomeController(SearchApiClient searchApi, EventApiClient eventApi, RegistrationApiClient registrationApi)
        {
            _searchApi = searchApi;
            _eventApi = eventApi;
            _registrationApi = registrationApi;
        }

        public async Task<IActionResult> Index()
        {
            var upcoming = await _searchApi.SearchAsync(null, null, null);
            return View(upcoming.Take(6).ToList());
        }

        [HttpGet]
        public async Task<IActionResult> Search(string? keyword, string? eventType, string? date)
        {
            DateTime? parsedDate = null;
            if (!string.IsNullOrWhiteSpace(date) && DateTime.TryParse(date, out var d))
            {
                parsedDate = d;
            }

            var model = new SearchViewModel
            {
                Keyword = keyword,
                EventType = eventType,
                Date = parsedDate?.ToString("yyyy-MM-dd"),
                AvailableTypes = await _searchApi.GetTypesAsync(),
                Results = await _searchApi.SearchAsync(eventType, parsedDate, keyword)
            };
            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var ev = await _eventApi.GetByIdAsync(id);
            if (ev is null) return NotFound();
            return View(ev);
        }

        [HttpGet]
        public async Task<IActionResult> Register(int id)
        {
            var ev = await _eventApi.GetByIdAsync(id);
            if (ev is null) return NotFound();

            return View(new PublicRegistrationViewModel { EventId = ev.Id, EventTitle = ev.Title });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(PublicRegistrationViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _registrationApi.RegisterAsync(model.EventId, model.ParticipantName, model.ParticipantEmail, model.ParticipantPhone);
            if (!result.Success)
            {
                model.ErrorMessage = result.Error ?? "Registration failed.";
                return View(model);
            }

            model.SuccessMessage = "You have been registered for this event!";
            return View("RegisterConfirmation", model);
        }

        public IActionResult Privacy() => View();

        public IActionResult Error() => View();
    }
}
