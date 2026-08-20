using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SylviaNG.Prescription.Application.Interfaces.Externals;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Application.Services
{
    /// <summary>
    /// Seeds exactly one Admin account on startup, so a fresh environment has a way to log in
    /// without any credential being committed to source control. Reads username/password from
    /// configuration (AdminSeed:Username/AdminSeed:Password) — real values live only in the
    /// gitignored appsettings.Development.json, never in appsettings.json or this file. Idempotent:
    /// skips silently if a local User with that username already exists, so it's safe to run on
    /// every startup.
    /// </summary>
    public class AdminAccountSeeder
    {
        private readonly IUserRepository _userRepository;
        private readonly IKeycloakAdminClient _adminClient;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AdminAccountSeeder> _logger;

        public AdminAccountSeeder(
            IUserRepository userRepository,
            IKeycloakAdminClient adminClient,
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            ILogger<AdminAccountSeeder> logger)
        {
            _userRepository = userRepository;
            _adminClient = adminClient;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            var username = _configuration["AdminSeed:Username"];
            var password = _configuration["AdminSeed:Password"];

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                _logger.LogInformation("AdminSeed:Username/AdminSeed:Password not configured — skipping admin account seeding.");
                return;
            }

            if (await _userRepository.ExistsByUsernameAsync(username))
            {
                return;
            }

            // Keycloak's declarative User Profile requires email on this realm (same class of
            // "account not fully set up" trap as missing firstName/lastName) — always send one.
            var email = $"{username}@prescriptionms.local";
            var created = await _adminClient.CreateUserAsync(username, email, firstName: username, lastName: "Admin", UserRoleEnum.Admin.ToString());

            // Unlike Doctor/Staff accounts (invite-email, no password ever issued by this
            // backend), this seeded admin must be usable immediately on a fresh environment
            // with no email interaction — set the configured password directly.
            await _adminClient.SetTemporaryPasswordAsync(created.KeycloakId, password);

            var user = new User
            {
                KeycloakId = created.KeycloakId,
                Username = username,
                Email = email,
                Role = UserRoleEnum.Admin,
                IsActive = true
            };
            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Seeded initial admin account '{Username}'.", username);
        }
    }
}
