using SylviaNG.Prescription.Application.Features.Analytics.Models;

namespace SylviaNG.Prescription.Application.Features.Analytics
{
    /// <summary>
    /// Server-side port of the reference prototype's <c>dateBuckets.js</c> (US-074/US-079).
    /// Buckets are plain strings — "yyyy-MM-dd" (Day), the Monday of that ISO week in
    /// "yyyy-MM-dd" form (Week), or "yyyy-MM" (Month) — all three sort correctly as plain
    /// strings, so callers never need a separate sort key.
    /// </summary>
    public static class AnalyticsDateBucketing
    {
        public static string BucketKey(DateTime date, AnalyticsGranularity granularity)
        {
            switch (granularity)
            {
                case AnalyticsGranularity.Week:
                    // .NET's DayOfWeek uses the same Sunday=0..Saturday=6 numbering as JS's
                    // Date.getDay(), so the prototype's (getDay()+6)%7 "days since Monday"
                    // offset ports over unchanged.
                    var offset = ((int)date.DayOfWeek + 6) % 7;
                    return date.AddDays(-offset).ToString("yyyy-MM-dd");

                case AnalyticsGranularity.Month:
                    return date.ToString("yyyy-MM");

                default:
                    return date.ToString("yyyy-MM-dd");
            }
        }

        /// <summary>
        /// Groups <paramref name="items"/> by <paramref name="dateSelector"/>'s bucket key,
        /// skipping items with a null date, and returns points sorted ascending by bucket key.
        /// </summary>
        public static List<TrendPoint> BuildTrend<T>(
            IEnumerable<T> items,
            Func<T, DateTime?> dateSelector,
            AnalyticsGranularity granularity)
        {
            var counts = new Dictionary<string, int>();
            foreach (var item in items)
            {
                var date = dateSelector(item);
                if (date == null)
                {
                    continue;
                }

                var key = BucketKey(date.Value, granularity);
                counts[key] = counts.GetValueOrDefault(key) + 1;
            }

            return counts
                .OrderBy(kvp => kvp.Key, StringComparer.Ordinal)
                .Select(kvp => new TrendPoint { BucketKey = kvp.Key, Count = kvp.Value })
                .ToList();
        }
    }
}
