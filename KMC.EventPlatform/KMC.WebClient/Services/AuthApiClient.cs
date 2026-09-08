using System.Net.Http.Headers;
using System.Net.Http.Json;
using KMC.WebClient.Models;

namespace KMC.WebClient.Services
{
    public class ApiResult<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Error { get; set; }
    }

    // Consumes the User & Authentication Service.
    public class AuthApiClient
    {
        private readonly HttpClient _http;

        public AuthApiClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<ApiResult<AuthResult>> RegisterAsync(string fullName, string email, string password, string role)
        {
            var response = await _http.PostAsJsonAsync("api/auth/register", new { fullName, email, password, role });
            return await ReadResult<AuthResult>(response);
        }

        public async Task<ApiResult<AuthResult>> LoginAsync(string email, string password)
        {
            var response = await _http.PostAsJsonAsync("api/auth/login", new { email, password });
            return await ReadResult<AuthResult>(response);
        }

        public async Task<ApiResult<bool>> UpdateProfileAsync(string fullName, string token)
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, "api/auth/profile")
            {
                Content = JsonContent.Create(new { fullName })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _http.SendAsync(request);
            if (response.IsSuccessStatusCode)
                return new ApiResult<bool> { Success = true, Data = true };

            var error = await response.Content.ReadAsStringAsync();
            return new ApiResult<bool> { Success = false, Error = string.IsNullOrWhiteSpace(error) ? response.ReasonPhrase : error };
        }

        public async Task<ApiResult<bool>> ChangePasswordAsync(string currentPassword, string newPassword, string token)
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, "api/auth/change-password")
            {
                Content = JsonContent.Create(new { currentPassword, newPassword })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _http.SendAsync(request);
            if (response.IsSuccessStatusCode)
                return new ApiResult<bool> { Success = true, Data = true };

            var error = await response.Content.ReadAsStringAsync();
            return new ApiResult<bool> { Success = false, Error = string.IsNullOrWhiteSpace(error) ? response.ReasonPhrase : error };
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
