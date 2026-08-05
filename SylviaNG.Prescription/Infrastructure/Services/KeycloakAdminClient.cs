using Microsoft.Extensions.Configuration;
using SylviaNG.Prescription.Application.Common.Models;
using SylviaNG.Prescription.Application.Interfaces.Externals;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace SylviaNG.Prescription.Infrastructure.Services
{
    /// <summary>
    /// Calls Keycloak's Admin REST API (create user, assign realm role, force a
    /// temporary password) using the `prescriptionms-backend` client's own service
    /// account (client_credentials grant, granted the realm-management "manage-users"
    /// client role). This is what backs Admin-only account creation/reset (US-004) —
    /// it never uses a human admin's own session token.
    /// </summary>
    public class KeycloakAdminClient : IKeycloakAdminClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _realm;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private string? _cachedServiceToken;
        private DateTimeOffset _serviceTokenExpiresAt = DateTimeOffset.MinValue;

        public KeycloakAdminClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _realm = configuration["Keycloak:Realm"] ?? throw new ArgumentNullException("Keycloak:Realm");
            _clientId = configuration["Keycloak:ClientId"] ?? throw new ArgumentNullException("Keycloak:ClientId");
            _clientSecret = configuration["Keycloak:ClientSecret"] ?? throw new ArgumentNullException("Keycloak:ClientSecret");
        }

        public async Task<KeycloakCreatedUser> CreateUserAsync(string username, string? email, string firstName, string lastName, string temporaryPassword, string realmRole)
        {
            await AuthorizeAsync();

            var body = new
            {
                username,
                email,
                // Required: Keycloak's declarative User Profile treats a missing
                // firstName/lastName as an implicit "account not fully set up" state
                // that silently blocks direct-grant (ROPC) login, without showing up
                // in the stored requiredActions list. Always send both.
                firstName,
                lastName,
                enabled = true,
                emailVerified = true,
                // NOT marked temporary: Keycloak's direct-grant (ROPC) login flow used by
                // this app cannot satisfy an interactive "update password" required action,
                // so a temporary credential would leave the account unable to log in at all.
                // See Doc/auth-jwt.md for the follow-up self-service change-password item.
                credentials = new[]
                {
                    new { type = "password", value = temporaryPassword, temporary = false }
                }
            };

            using var response = await _httpClient.PostAsJsonAsync($"admin/realms/{_realm}/users", body);
            response.EnsureSuccessStatusCode();

            var location = response.Headers.Location
                ?? throw new InvalidOperationException("Keycloak did not return a Location header for the created user.");
            var keycloakId = location.Segments[^1];

            await AssignRealmRoleAsync(keycloakId, realmRole);

            return new KeycloakCreatedUser { KeycloakId = keycloakId };
        }

        public async Task SetTemporaryPasswordAsync(string keycloakId, string newPassword)
        {
            await AuthorizeAsync();

            var body = new { type = "password", value = newPassword, temporary = false };

            using var response = await _httpClient.PutAsJsonAsync(
                $"admin/realms/{_realm}/users/{keycloakId}/reset-password", body);
            response.EnsureSuccessStatusCode();
        }

        public async Task SetUserEnabledAsync(string keycloakId, bool enabled)
        {
            await AuthorizeAsync();

            var body = new { enabled };

            using var response = await _httpClient.PutAsJsonAsync(
                $"admin/realms/{_realm}/users/{keycloakId}", body);
            response.EnsureSuccessStatusCode();
        }

        private async Task AssignRealmRoleAsync(string keycloakId, string realmRole)
        {
            using var roleResponse = await _httpClient.GetAsync($"admin/realms/{_realm}/roles/{realmRole}");
            roleResponse.EnsureSuccessStatusCode();
            var role = await roleResponse.Content.ReadFromJsonAsync<KeycloakRoleRepresentation>()
                ?? throw new InvalidOperationException($"Keycloak realm role '{realmRole}' was not found.");

            using var assignResponse = await _httpClient.PostAsJsonAsync(
                $"admin/realms/{_realm}/users/{keycloakId}/role-mappings/realm",
                new[] { role });
            assignResponse.EnsureSuccessStatusCode();
        }

        private async Task AuthorizeAsync()
        {
            if (_cachedServiceToken != null && DateTimeOffset.UtcNow < _serviceTokenExpiresAt)
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _cachedServiceToken);
                return;
            }

            var form = new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = _clientId,
                ["client_secret"] = _clientSecret
            };

            using var response = await _httpClient.PostAsync(
                $"realms/{_realm}/protocol/openid-connect/token",
                new FormUrlEncodedContent(form));
            response.EnsureSuccessStatusCode();

            var token = await response.Content.ReadFromJsonAsync<ServiceTokenResponse>()
                ?? throw new InvalidOperationException("Keycloak did not return a service-account token.");

            _cachedServiceToken = token.access_token;
            _serviceTokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(token.expires_in - 10);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _cachedServiceToken);
        }

        private class ServiceTokenResponse
        {
            public string access_token { get; set; } = string.Empty;
            public int expires_in { get; set; }
        }

        private class KeycloakRoleRepresentation
        {
            public string id { get; set; } = string.Empty;
            public string name { get; set; } = string.Empty;
        }
    }
}
