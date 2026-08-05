using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Infrastructure.Data;

namespace SylviaNG.Prescription.Tests.TestHelpers;

/// <summary>
/// Builds a fresh EF Core InMemory-backed <see cref="ApplicationDBContext"/> for handlers
/// (Staff Management) that read/write the many-to-many StaffDoctors join table directly
/// via <c>IUnitOfWork.Context</c> rather than through a dedicated repository.
/// </summary>
public static class InMemoryDbContextFactory
{
    public static ApplicationDBContext Create()
    {
        var options = new DbContextOptionsBuilder<ApplicationDBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDBContext(options);
    }
}
