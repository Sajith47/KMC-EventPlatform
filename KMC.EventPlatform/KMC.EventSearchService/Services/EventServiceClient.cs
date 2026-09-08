using System.Net.Http.Json;
using KMC.EventSearchService.DTOs;

namespace KMC.EventSearchService.Services
{
    // The Event Search Service does not own its own event data. Instead, it
    // composes results by calling the Event Management Service's API - a
    // typical pattern in Service-Oriented Computing where services are
    // reused rather than duplicating data ownership.
    public class EventServiceClient
    {
        private readonly HttpClient _http;

        public EventServiceClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<EventDto>> GetAllEventsAsync()
        {
            var events = await _http.GetFromJsonAsync<List<EventDto>>("api/events");
            return events ?? new List<EventDto>();
        }
    }
}
