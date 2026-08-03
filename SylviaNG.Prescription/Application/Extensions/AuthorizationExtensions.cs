using Microsoft.AspNetCore.Authorization;

namespace SylviaNG.Prescription.Application.Extensions
{
    public static class AuthorizationExtensions
    {
        public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                // Add prescription-specific authorization policies here
            });

            return services;
        }
    }
}
