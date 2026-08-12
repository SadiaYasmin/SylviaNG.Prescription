using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Infrastructure.Data;
using SylviaNG.Prescription.Infrastructure.Services;

namespace SylviaNG.Prescription.Tests.Services;

/// <summary>
/// SequenceGenerator's atomicity guarantee — a single Postgres
/// "INSERT ... ON CONFLICT ... DO UPDATE ... RETURNING" round trip, not a read-then-write —
/// cannot be exercised against EF Core's InMemory provider: InMemory has no relational SQL
/// translator at all, so <c>Database.SqlQueryRaw</c> against it throws
/// <see cref="NotSupportedException"/> (verified: swapping the connection below for
/// <c>UseInMemoryDatabase</c> makes every test in this class fail with exactly that
/// exception). This test instead runs against the same local Postgres dev database the rest
/// of the app uses (see appsettings.Development.json's connection string, duplicated here
/// since the Tests project doesn't load the API project's configuration) — it requires that
/// database to be reachable, which it is in this dev environment (also required for
/// `dotnet ef database update`). A unique CounterKey per test instance means it never
/// collides with real application data, and each test cleans up its own rows afterward.
///
/// This does verify the real code path end-to-end (no read-then-write anywhere in
/// SequenceGenerator), but it does NOT prove atomicity under true concurrent load — doing
/// that would require firing overlapping requests at a shared connection pool, which is out
/// of scope for a unit-test-style suite. Confidence in the atomicity claim rests on
/// Postgres's own guarantee for `INSERT ... ON CONFLICT ... DO UPDATE`, not on this test.
/// </summary>
public class SequenceGeneratorTests : IAsyncLifetime
{
    private const string ConnectionString = "Host=localhost;Port=5432;Database=prescriptionms;Username=postgres;Password=postgres";

    private readonly string _counterKey = $"test-counter-{Guid.NewGuid():N}";
    private ApplicationDBContext _context = null!;
    private SequenceGenerator _generator = null!;

    public Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDBContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        _context = new ApplicationDBContext(options);
        _generator = new SequenceGenerator(_context);
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        // Clean up this test's own rows only — never touch anything else in the dev DB.
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"DELETE FROM \"SequenceCounters\" WHERE \"CounterKey\" = {_counterKey}");
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task GetNextAsync_CalledTwiceForSamePeriod_ShouldReturnOneThenTwo()
    {
        // Act
        var first = await _generator.GetNextAsync(_counterKey, "2026");
        var second = await _generator.GetNextAsync(_counterKey, "2026");

        // Assert
        first.Should().Be(1);
        second.Should().Be(2);
    }

    [Fact]
    public async Task GetNextAsync_CalledWithDifferentPeriodKey_ShouldStartBackAtOne()
    {
        // Act: exhaust period "2026" to 2, then ask for a different period on the same
        // counter key.
        await _generator.GetNextAsync(_counterKey, "2026");
        await _generator.GetNextAsync(_counterKey, "2026");
        var otherPeriodFirst = await _generator.GetNextAsync(_counterKey, "2027");

        // Assert
        otherPeriodFirst.Should().Be(1);
    }

    [Fact]
    public async Task GetNextAsync_DifferentCounterKeys_ShouldBeIndependentSequences()
    {
        // Arrange
        var otherCounterKey = $"{_counterKey}-other";

        try
        {
            // Act
            var a1 = await _generator.GetNextAsync(_counterKey, "2026");
            var b1 = await _generator.GetNextAsync(otherCounterKey, "2026");
            var a2 = await _generator.GetNextAsync(_counterKey, "2026");

            // Assert
            a1.Should().Be(1);
            b1.Should().Be(1);
            a2.Should().Be(2);
        }
        finally
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE FROM \"SequenceCounters\" WHERE \"CounterKey\" = {otherCounterKey}");
        }
    }
}
