using System.Net.Http.Json;
using System.Web;
using KMC.WebClient.Models;

namespace KMC.WebClient.Services
{
    // Consumes the Event Search Service.
    public class SearchApiClient
    {
        private readonly HttpClient _http;

        public SearchApiClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<EventDto>> SearchAsync(string? type, DateTime? date, string? keyword)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            if (!string.IsNullOrWhiteSpace(type)) query["type"] = type;
            if (date.HasValue) query["date"] = date.Value.ToString("yyyy-MM-dd");
            if (!string.IsNullOrWhiteSpace(keyword)) query["keyword"] = keyword;

            var response = await _http.GetFromJsonAsync<List<EventDto>>($"api/search/events?{query}");
            return response ?? new();
        }

        public async Task<List<string>> GetTypesAsync()
        {
            var response = await _http.GetFromJsonAsync<List<string>>("api/search/types");
            return response ?? new();
        }
    }
}
