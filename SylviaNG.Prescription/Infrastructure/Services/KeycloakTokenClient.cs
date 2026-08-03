using Microsoft.Extensions.Configuration;
using SylviaNG.Prescription.Application.Common.Models;
using SylviaNG.Prescription.Application.Interfaces.Externals;
using System.Net.Http.Json;

namespace SylviaNG.Prescription.Infrastructure.Services
{
    /// <summary>
    /// Talks to Keycloak's own token endpoint (Resource Owner Password Credentials
    /// grant for login, refresh grant, and the logout/revocation endpoint) using the
    /// confidential `prescriptionms-backend` client. The Angular SPA never talks to
    /// Keycloak directly and never sees the client secret.
    /// </summary>
    public class KeycloakTokenClient : IKeycloakTokenClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _clientId;
        private readonly string _clientSecret;

        public KeycloakTokenClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _clientId = configuration["Keycloak:ClientId"] ?? throw new ArgumentNullException("Keycloak:ClientId");
            _clientSecret = configuration["Keycloak:ClientSecret"] ?? throw new ArgumentNullException("Keycloak:ClientSecret");
        }

        public async Task<KeycloakTokenResult?> PasswordGrantAsync(string username, string password)
        {
            var form = new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = _clientId,
                ["client_secret"] = _clientSecret,
                ["username"] = username,
                ["password"] = password
            };

            return await PostTokenRequestAsync(form);
        }

        public async Task<KeycloakTokenResult?> RefreshAsync(string refreshToken)
        {
            var form = new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["client_id"] = _clientId,
                ["client_secret"] = _clientSecret,
                ["refresh_token"] = refreshToken
            };

            return await PostTokenRequestAsync(form);
        }

        public async Task LogoutAsync(string refreshToken)
        {
            var form = new Dictionary<string, string>
            {
                ["client_id"] = _clientId,
                ["client_secret"] = _clientSecret,
                ["refresh_token"] = refreshToken
            };

            using var response = await _httpClient.PostAsync(
                "protocol/openid-connect/logout",
                new FormUrlEncodedContent(form));

            response.EnsureSuccessStatusCode();
        }

        private async Task<KeycloakTokenResult?> PostTokenRequestAsync(Dictionary<string, string> form)
        {
            using var response = await _httpClient.PostAsync(
                "protocol/openid-connect/token",
                new FormUrlEncodedContent(form));

            if (!response.IsSuccessStatusCode)
                return null;

            var payload = await response.Content.ReadFromJsonAsync<KeycloakTokenResponse>();
            if (payload == null)
                return null;

            return new KeycloakTokenResult
            {
                AccessToken = payload.access_token,
                RefreshToken = payload.refresh_token,
                ExpiresIn = payload.expires_in
            };
        }

        private class KeycloakTokenResponse
        {
            public string access_token { get; set; } = string.Empty;
            public string refresh_token { get; set; } = string.Empty;
            public int expires_in { get; set; }
        }
    }
}
