using System.Net.Http.Headers;
using System.Net.Http.Json;
using KMC.WebClient.Models;

namespace KMC.WebClient.Services
{
    // Consumes the Event Management Service.
    public class EventApiClient
    {
        private readonly HttpClient _http;

        public EventApiClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<EventDto>> GetAllAsync()
        {
            return await _http.GetFromJsonAsync<List<EventDto>>("api/events") ?? new();
        }

        public async Task<EventDto?> GetByIdAsync(int id)
        {
            var response = await _http.GetAsync($"api/events/{id}");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<EventDto>();
        }

        public async Task<List<EventDto>> GetByOrganizerAsync(int organizerId, string token)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"api/events/organizer/{organizerId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode) return new();
            return await response.Content.ReadFromJsonAsync<List<EventDto>>() ?? new();
        }

        public async Task<ApiResult<EventDto>> CreateAsync(EventFormViewModel form, string token)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "api/events")
            {
                Content = JsonContent.Create(new
                {
                    title = form.Title,
                    description = form.Description,
                    eventType = form.EventType,
                    eventDate = form.EventDate,
                    venue = form.Venue,
                    capacity = form.Capacity
                })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _http.SendAsync(request);
            return await ReadResult<EventDto>(response);
        }

        public async Task<ApiResult<EventDto>> UpdateAsync(int id, EventFormViewModel form, string token)
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, $"api/events/{id}")
            {
                Content = JsonContent.Create(new
                {
                    title = form.Title,
                    description = form.Description,
                    eventType = form.EventType,
                    eventDate = form.EventDate,
                    venue = form.Venue,
                    capacity = form.Capacity
                })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _http.SendAsync(request);
            return await ReadResult<EventDto>(response);
        }

        public async Task<bool> DeleteAsync(int id, string token)
        {
            using var request = new HttpRequestMessage(HttpMethod.Delete, $"api/events/{id}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        private static async Task<ApiResult<T>> ReadResult<T>(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<T>();
                return new ApiResult<T> { Success = true, Data = data };
            }
            var error = await response.Content.ReadAsStringAsync();
            return new ApiResult<T> { Success = false, Error = string.IsNullOrWhiteSpace(error) ? response.ReasonPhrase : error };
        }
    }
}
