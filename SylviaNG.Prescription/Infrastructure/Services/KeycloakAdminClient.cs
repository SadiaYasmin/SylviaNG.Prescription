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
        private readonly string _frontendRedirectUri;
        private string? _cachedServiceToken;
        private DateTimeOffset _serviceTokenExpiresAt = DateTimeOffset.MinValue;

        public KeycloakAdminClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _realm = configuration["Keycloak:Realm"] ?? throw new ArgumentNullException("Keycloak:Realm");
            _clientId = configuration["Keycloak:ClientId"] ?? throw new ArgumentNullException("Keycloak:ClientId");
            _clientSecret = configuration["Keycloak:ClientSecret"] ?? throw new ArgumentNullException("Keycloak:ClientSecret");
            _frontendRedirectUri = configuration["Keycloak:FrontendRedirectUri"] ?? throw new ArgumentNullException("Keycloak:FrontendRedirectUri");
        }

        public async Task<KeycloakCreatedUser> CreateUserAsync(string username, string email, string firstName, string lastName, string realmRole)
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
                // Not verified yet — the invite email's VERIFY_EMAIL required action is what
                // flips this once the invitee actually opens the link.
                emailVerified = false
                // No `credentials` block: this account has no password until the invitee sets
                // one via the invite email's UPDATE_PASSWORD required action (an interactive,
                // browser-based Keycloak flow — unlike the direct-grant/ROPC login this app's
                // own login form uses, that flow CAN satisfy required actions).
            };

            using var response = await _httpClient.PostAsJsonAsync($"admin/realms/{_realm}/users", body);

            string keycloakId;
            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                // A prior create that got this far in Keycloak but never finished committing
                // its local Users row (e.g. the API process was restarted mid-request) leaves
                // an orphaned Keycloak user with this exact username/email — every retry then
                // 409s here forever, since nothing here was ever persisted to retry against.
                // Re-adopt the existing Keycloak user instead of hard-failing.
                keycloakId = await FindUserIdByUsernameAsync(username)
                    ?? throw new InvalidOperationException($"Keycloak reported a username conflict for '{username}' but the existing user could not be found.");
            }
            else
            {
                response.EnsureSuccessStatusCode();
                var location = response.Headers.Location
                    ?? throw new InvalidOperationException("Keycloak did not return a Location header for the created user.");
                keycloakId = location.Segments[^1];
            }

            await AssignRealmRoleAsync(keycloakId, realmRole);

            return new KeycloakCreatedUser { KeycloakId = keycloakId };
        }

        private async Task<string?> FindUserIdByUsernameAsync(string username)
        {
            using var response = await _httpClient.GetAsync(
                $"admin/realms/{_realm}/users?username={Uri.EscapeDataString(username)}&exact=true");
            response.EnsureSuccessStatusCode();

            var users = await response.Content.ReadFromJsonAsync<List<KeycloakUserRepresentation>>();
            return users?.FirstOrDefault()?.id;
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

        public async Task SetUserEmailAsync(string keycloakId, string email)
        {
            await AuthorizeAsync();

            var body = new { email, emailVerified = true };

            using var response = await _httpClient.PutAsJsonAsync(
                $"admin/realms/{_realm}/users/{keycloakId}", body);
            response.EnsureSuccessStatusCode();
        }

        public async Task SendRequiredActionsEmailAsync(string keycloakId, IEnumerable<string> actions)
        {
            await AuthorizeAsync();

            var query = $"client_id={Uri.EscapeDataString(_clientId)}&redirect_uri={Uri.EscapeDataString(_frontendRedirectUri)}";
            using var response = await _httpClient.PutAsJsonAsync(
                $"admin/realms/{_realm}/users/{keycloakId}/execute-actions-email?{query}", actions);
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

        private class KeycloakUserRepresentation
        {
            public string id { get; set; } = string.Empty;
        }
    }
}
