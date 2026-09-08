using Microsoft.AspNetCore.Mvc;
using KMC.EventSearchService.Services;

namespace KMC.EventSearchService.Controllers
{
    // Event Search Service
    // Enables the public to search and find events based on criteria such
    // as date, event type and keyword. Composes data from the Event
    // Management Service rather than owning event data itself.
    [ApiController]
    [Route("api/search")]
    public class SearchController : ControllerBase
    {
        private readonly EventServiceClient _eventClient;

        public SearchController(EventServiceClient eventClient)
        {
            _eventClient = eventClient;
        }

        // GET api/search/events?type=Concert&date=2026-09-01&keyword=music
        [HttpGet("events")]
        public async Task<IActionResult> SearchEvents(
            [FromQuery] string? type,
            [FromQuery] DateTime? date,
            [FromQuery] string? keyword)
        {
            var events = await _eventClient.GetAllEventsAsync();

            var results = events.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(type))
                results = results.Where(e => e.EventType.Equals(type, StringComparison.OrdinalIgnoreCase));

            if (date.HasValue)
                results = results.Where(e => e.EventDate.Date == date.Value.Date);

            if (!string.IsNullOrWhiteSpace(keyword))
                results = results.Where(e =>
                    e.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    e.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    e.Venue.Contains(keyword, StringComparison.OrdinalIgnoreCase));

            return Ok(results.OrderBy(e => e.EventDate).ToList());
        }

        // GET api/search/types
        // Returns the distinct set of event types currently available, to
        // help populate a filter dropdown on the client.
        [HttpGet("types")]
        public async Task<IActionResult> GetEventTypes()
        {
            var events = await _eventClient.GetAllEventsAsync();
            var types = events.Select(e => e.EventType).Distinct().OrderBy(t => t).ToList();
            return Ok(types);
        }
    }
}
