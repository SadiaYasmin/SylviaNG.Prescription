using Microsoft.AspNetCore.Authorization;

namespace SylviaNG.Prescription.Application.Extensions
{
    public static class AuthorizationExtensions
    {
        public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
                options.AddPolicy("DoctorOnly", policy => policy.RequireRole("Doctor"));
                options.AddPolicy("StaffOnly", policy => policy.RequireRole("Staff"));
                options.AddPolicy("DoctorOrStaff", policy => policy.RequireRole("Doctor", "Staff"));
            });

            return services;
        }
    }
}
