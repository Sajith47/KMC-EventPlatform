using System.Net.Http.Headers;
using System.Net.Http.Json;
using KMC.WebClient.Models;

namespace KMC.WebClient.Services
{
    // Consumes the Participant Registration Service.
    public class RegistrationApiClient
    {
        private readonly HttpClient _http;

        public RegistrationApiClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<ApiResult<RegistrationDto>> RegisterAsync(int eventId, string name, string email, string? phone)
        {
            var response = await _http.PostAsJsonAsync("api/registrations", new
            {
                eventId,
                participantName = name,
                participantEmail = email,
                participantPhone = phone
            });

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<RegistrationDto>();
                return new ApiResult<RegistrationDto> { Success = true, Data = data };
            }

            var error = await response.Content.ReadAsStringAsync();
            return new ApiResult<RegistrationDto> { Success = false, Error = string.IsNullOrWhiteSpace(error) ? response.ReasonPhrase : error };
        }

        public async Task<List<RegistrationDto>> GetByEventAsync(int eventId, string token)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"api/registrations/event/{eventId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode) return new();
            return await response.Content.ReadFromJsonAsync<List<RegistrationDto>>() ?? new();
        }

        public async Task<List<RegistrationDto>> GetMyRegistrationsAsync(string email)
        {
            var response = await _http.GetAsync($"api/registrations/my-registrations?email={Uri.EscapeDataString(email)}");
            if (!response.IsSuccessStatusCode) return new();
            return await response.Content.ReadFromJsonAsync<List<RegistrationDto>>() ?? new();
        }

        public async Task<ApiResult<bool>> CancelRegistrationAsync(int id, string email)
        {
            var response = await _http.DeleteAsync($"api/registrations/{id}?email={Uri.EscapeDataString(email)}");
            if (response.IsSuccessStatusCode)
            {
                return new ApiResult<bool> { Success = true, Data = true };
            }
            var error = await response.Content.ReadAsStringAsync();
            return new ApiResult<bool> { Success = false, Error = string.IsNullOrWhiteSpace(error) ? response.ReasonPhrase : error };
        }
    }
}
