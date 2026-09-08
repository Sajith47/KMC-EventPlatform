using System.Net.Http.Json;

namespace KMC.ParticipantRegistrationService.Services
{
    // Lightweight DTO matching the response shape of the Event Management Service.
    public class RemoteEventDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public int Capacity { get; set; }
        public int OrganizerId { get; set; }
    }

    // This client demonstrates inter-service communication in the SOA design:
    // the Participant Registration Service calls the Event Management Service
    // over HTTP/API to confirm an event exists before accepting a registration.
    public class EventServiceClient
    {
        private readonly HttpClient _http;

        public EventServiceClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<RemoteEventDto?> GetEventAsync(int eventId)
        {
            var response = await _http.GetAsync($"api/events/{eventId}");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<RemoteEventDto>();
        }
    }
}
