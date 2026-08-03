using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Audit;

namespace SylviaNG.Prescription.Domain.Entities;

/// <summary>
/// Local profile/authorization linkage record for an authenticated account.
/// Credentials themselves live in Keycloak (the identity store) — this table
/// never stores a password, only the Keycloak identity link plus the app-side
/// role and status used for business queries and authorization.
/// </summary>
public class User : Audit
{
    public long UserId { get; set; }
    public string KeycloakId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public UserRoleEnum Role { get; set; }
    public bool IsActive { get; set; } = true;
}
