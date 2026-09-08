using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KMC.WebClient.Models;
using KMC.WebClient.Services;

namespace KMC.WebClient.Controllers
{
    // Organizer dashboard: create, edit, delete their own events and view registrants.
    [Authorize(Roles = "Organizer")]
    public class EventsController : Controller
    {
        private readonly EventApiClient _eventApi;
        private readonly RegistrationApiClient _registrationApi;

        public EventsController(EventApiClient eventApi, RegistrationApiClient registrationApi)
        {
            _eventApi = eventApi;
            _registrationApi = registrationApi;
        }

        public async Task<IActionResult> Index()
        {
            var events = await _eventApi.GetByOrganizerAsync(CurrentUserId, CurrentToken);
            return View(events);
        }

        [HttpGet]
        public IActionResult Create() => View(new EventFormViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventFormViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _eventApi.CreateAsync(model, CurrentToken);
            if (!result.Success)
            {
                model.ErrorMessage = result.Error ?? "Could not create event.";
                return View(model);
            }

            TempData["Message"] = "Event created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var ev = await _eventApi.GetByIdAsync(id);
            if (ev is null || ev.OrganizerId != CurrentUserId) return NotFound();

            return View(new EventFormViewModel
            {
                Id = ev.Id,
                Title = ev.Title,
                Description = ev.Description,
                EventType = ev.EventType,
                EventDate = ev.EventDate,
                Venue = ev.Venue,
                Capacity = ev.Capacity
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EventFormViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _eventApi.UpdateAsync(id, model, CurrentToken);
            if (!result.Success)
            {
                model.ErrorMessage = result.Error ?? "Could not update event.";
                return View(model);
            }

            TempData["Message"] = "Event updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _eventApi.DeleteAsync(id, CurrentToken);
            TempData["Message"] = "Event deleted.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Registrations(int id)
        {
            var ev = await _eventApi.GetByIdAsync(id);
            if (ev is null || ev.OrganizerId != CurrentUserId) return NotFound();

            var registrations = await _registrationApi.GetByEventAsync(id, CurrentToken);
            ViewBag.Event = ev;
            return View(registrations);
        }

        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        private string CurrentToken => User.FindFirst("jwt_token")!.Value;
    }
}
