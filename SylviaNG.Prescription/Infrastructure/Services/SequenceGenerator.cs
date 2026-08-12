using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Common.Services;
using SylviaNG.Prescription.Infrastructure.Data;

namespace SylviaNG.Prescription.Infrastructure.Services
{
    /// <summary>
    /// Postgres-backed <see cref="ISequenceGenerator"/> (Epic C). Atomicity comes entirely
    /// from a single "INSERT ... ON CONFLICT ... DO UPDATE ... RETURNING" round trip — NOT
    /// from a read-then-write or an app-level transaction/lock — so two concurrent callers
    /// for the same (counterKey, periodKey) are serialized by Postgres itself and can never
    /// both receive the same value. Relies on the unique index on
    /// (CounterKey, PeriodKey) configured in <c>SequenceCounterConfiguration</c> to give the
    /// upsert something to conflict on.
    /// </summary>
    public class SequenceGenerator : ISequenceGenerator
    {
        private const string UpsertSql = """
            INSERT INTO "SequenceCounters" ("CounterKey","PeriodKey","LastValue") VALUES ({0}, {1}, 1)
            ON CONFLICT ("CounterKey","PeriodKey") DO UPDATE SET "LastValue" = "SequenceCounters"."LastValue" + 1
            RETURNING "LastValue"
            """;

        private readonly ApplicationDBContext _context;

        public SequenceGenerator(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<int> GetNextAsync(string counterKey, string periodKey, CancellationToken cancellationToken = default)
        {
            // Parameterized via SqlQueryRaw's params array — counterKey/periodKey are never
            // string-interpolated into the SQL text, so this is not injectable.
            //
            // Materialize with ToListAsync + client-side Single rather than SingleAsync:
            // SingleAsync composes the raw SQL into a wrapping "SELECT ... LIMIT 2" query to
            // check for extra rows server-side, and EF Core rejects that composition for a
            // non-SELECT statement like this INSERT ("non-composable SQL"). ToListAsync
            // executes the statement as-is with no wrapping.
            var results = await _context.Database
                .SqlQueryRaw<int>(UpsertSql, counterKey, periodKey)
                .ToListAsync(cancellationToken);

            return results.Single();
        }
    }
}
