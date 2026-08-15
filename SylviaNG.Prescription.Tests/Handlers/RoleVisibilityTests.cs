using FluentAssertions;
using SylviaNG.Prescription.Application.Features.Patients;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.Tests.TestHelpers;
using static SylviaNG.Prescription.Application.Common.CallerContextResolver;

namespace SylviaNG.Prescription.Tests.Handlers;

/// <summary>
/// US-084: a single consolidated place proving the Admin/Staff/Doctor patient-visibility
/// matrix end-to-end, alongside (not replacing) the per-handler tests that already exercise
/// individual query handlers. <see cref="PatientVisibilityScope"/> is the one shared
/// implementation of this matrix (Epic B), so testing it directly here covers
/// GetPatientList/GetPatientDetails/UpdatePatient's visibility rule in one place.
/// </summary>
public class RoleVisibilityTests
{
    private const long StaffAId = 1;
    private const long StaffBId = 2;
    private const long DoctorId = 10;
    private const long PatientRegisteredByStaffAId = 100;
    private const long PatientRegisteredByStaffBId = 200;

    private static Patient PatientRegisteredByStaffA() => new() { PatientId = PatientRegisteredByStaffAId, Name = "Rahim", Phone = "01700000000", RegisteredByStaffId = StaffAId };
    private static Patient PatientRegisteredByStaffB() => new() { PatientId = PatientRegisteredByStaffBId, Name = "Karim", Phone = "01711111111", RegisteredByStaffId = StaffBId };

    [Fact]
    public async Task Admin_SeesEveryPatient_RegardlessOfWhoRegisteredThem()
    {
        var context = InMemoryDbContextFactory.Create();
        var caller = new CallerContext(UserRoleEnum.Admin, StaffId: null, DoctorId: null);
        var patients = new[] { PatientRegisteredByStaffA(), PatientRegisteredByStaffB() }.AsQueryable();

        var scoped = await PatientVisibilityScope.ApplyAsync(patients, context, caller);

        scoped.Select(p => p.PatientId).Should().BeEquivalentTo([PatientRegisteredByStaffAId, PatientRegisteredByStaffBId]);
    }

    [Fact]
    public async Task Staff_SeesOnlyPatientsTheyThemselvesRegistered()
    {
        var context = InMemoryDbContextFactory.Create();
        var caller = new CallerContext(UserRoleEnum.Staff, StaffId: StaffAId, DoctorId: null);
        var patients = new[] { PatientRegisteredByStaffA(), PatientRegisteredByStaffB() }.AsQueryable();

        var scoped = await PatientVisibilityScope.ApplyAsync(patients, context, caller);

        scoped.Select(p => p.PatientId).Should().BeEquivalentTo([PatientRegisteredByStaffAId]);
    }

    [Fact]
    public async Task Doctor_SeesOnlyPatientsRegisteredByStaffAssignedToThem()
    {
        var context = InMemoryDbContextFactory.Create();
        context.StaffDoctors.Add(new StaffDoctor { StaffId = StaffAId, DoctorId = DoctorId });
        await context.SaveChangesAsync();

        var caller = new CallerContext(UserRoleEnum.Doctor, StaffId: null, DoctorId: DoctorId);
        var patients = new[] { PatientRegisteredByStaffA(), PatientRegisteredByStaffB() }.AsQueryable();

        var scoped = await PatientVisibilityScope.ApplyAsync(patients, context, caller);

        scoped.Select(p => p.PatientId).Should().BeEquivalentTo([PatientRegisteredByStaffAId]);
    }

    [Fact]
    public async Task Doctor_WithNoAssignedStaff_SeesNoPatients()
    {
        var context = InMemoryDbContextFactory.Create();
        var caller = new CallerContext(UserRoleEnum.Doctor, StaffId: null, DoctorId: DoctorId);
        var patients = new[] { PatientRegisteredByStaffA(), PatientRegisteredByStaffB() }.AsQueryable();

        var scoped = await PatientVisibilityScope.ApplyAsync(patients, context, caller);

        scoped.Should().BeEmpty();
    }

    [Fact]
    public async Task IsVisibleAsync_MatchesApplyAsyncForASinglePatient()
    {
        var context = InMemoryDbContextFactory.Create();
        var staffCaller = new CallerContext(UserRoleEnum.Staff, StaffId: StaffAId, DoctorId: null);

        (await PatientVisibilityScope.IsVisibleAsync(PatientRegisteredByStaffA(), context, staffCaller)).Should().BeTrue();
        (await PatientVisibilityScope.IsVisibleAsync(PatientRegisteredByStaffB(), context, staffCaller)).Should().BeFalse();
    }
}
