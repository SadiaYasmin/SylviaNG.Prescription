using SylviaNG.Prescription.Application.Interfaces.Externals;
using SylviaNG.Prescription.Infrastructure.Services;

namespace SylviaNG.Prescription.Infrastructure.Extensions
{
    public static class KeycloakExtensions
    {
        public static IServiceCollection AddKeycloakClients(this IServiceCollection services, IConfiguration configuration)
        {
            var authority = configuration["Keycloak:Authority"]
                ?? throw new InvalidOperationException("Keycloak:Authority is not configured.");
            var realm = configuration["Keycloak:Realm"]
                ?? throw new InvalidOperationException("Keycloak:Realm is not configured.");

            var realmSuffix = $"/realms/{realm}";
            var serverRoot = authority.EndsWith(realmSuffix, StringComparison.OrdinalIgnoreCase)
                ? authority[..^realmSuffix.Length]
                : authority;

            services.AddHttpClient<IKeycloakTokenClient, KeycloakTokenClient>(client =>
            {
                client.BaseAddress = new Uri(authority.TrimEnd('/') + "/");
            });

            services.AddHttpClient<IKeycloakAdminClient, KeycloakAdminClient>(client =>
            {
                client.BaseAddress = new Uri(serverRoot.TrimEnd('/') + "/");
            });

            return services;
        }
    }
}
