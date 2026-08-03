using SylviaNG.Prescription.Application.Interfaces.Externals;
using SylviaNG.Prescription.Grpc.Generated.Core;
using SylviaNG.Prescription.Infrastructure.Services;

namespace SylviaNG.Prescription.Infrastructure.Extensions
{
    /// <summary>
    /// Extension methods for gRPC service registration
    /// </summary>
    public static class GrpcExtensions
    {
        /// <summary>
        /// Registers gRPC client services and channels
        /// </summary>
        public static IServiceCollection AddGrpcServices(this IServiceCollection services, IConfiguration configuration)
        {
            // ── gRPC: Core Service Channel ───────────────────────────────────────────
            var coreServiceUrl = configuration["GrpcServices:CoreService:Url"] ?? "http://localhost:7000";

            services.AddGrpcClient<CoreService.CoreServiceClient>(options =>
            {
                options.Address = new Uri(coreServiceUrl);
            });

            services.AddScoped<ICoreGrpcClient, CoreGrpcClient>();

            return services;
        }
    }
}
