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
        /// <summary>Bangladesh has no DST, so a flat UTC+6 shift is always correct — the hospital operates in Dhaka.</summary>
        public const int BangladeshUtcOffsetHours = 6;

        public static DateTime ToBangladeshTime(DateTime utc) => utc.AddHours(BangladeshUtcOffsetHours);

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

        /// <summary>
        /// Default trailing window for a trend chart when the caller doesn't pin an explicit
        /// range: 30 days / 12 weeks / 12 months, all anchored to <paramref name="nowUtc"/>.
        /// </summary>
        public static (DateTime Start, DateTime End) GetDefaultRange(AnalyticsGranularity granularity, DateTime nowUtc)
        {
            return granularity switch
            {
                AnalyticsGranularity.Week => (nowUtc.AddDays(-7 * 11), nowUtc),
                AnalyticsGranularity.Month => (nowUtc.AddMonths(-11), nowUtc),
                _ => (nowUtc.AddDays(-29), nowUtc),
            };
        }

        /// <summary>
        /// Same as <see cref="BuildTrend{T}"/>, but converts every date to Bangladesh Time
        /// before bucketing (so day/week/month boundaries match the hospital's local
        /// calendar, not UTC), and zero-fills every bucket in
        /// [<paramref name="rangeStartUtc"/>, <paramref name="rangeEndUtc"/>] — including
        /// buckets with no matching items — so the chart never silently drops chronology.
        /// </summary>
        public static List<TrendPoint> BuildTrendZeroFilled<T>(
            IEnumerable<T> items,
            Func<T, DateTime?> dateSelector,
            AnalyticsGranularity granularity,
            DateTime rangeStartUtc,
            DateTime rangeEndUtc)
        {
            var counts = new Dictionary<string, int>();
            foreach (var item in items)
            {
                var date = dateSelector(item);
                if (date == null)
                {
                    continue;
                }

                var key = BucketKey(ToBangladeshTime(date.Value), granularity);
                counts[key] = counts.GetValueOrDefault(key) + 1;
            }

            var sequence = GenerateBucketSequence(ToBangladeshTime(rangeStartUtc), ToBangladeshTime(rangeEndUtc), granularity);

            return sequence
                .Select(key => new TrendPoint { BucketKey = key, Count = counts.GetValueOrDefault(key) })
                .ToList();
        }

        /// <summary>Every bucket key from <paramref name="start"/> to <paramref name="end"/> inclusive, in chronological order — the zero-fill backbone for <see cref="BuildTrendZeroFilled{T}"/>.</summary>
        private static List<string> GenerateBucketSequence(DateTime start, DateTime end, AnalyticsGranularity granularity)
        {
            var keys = new List<string>();
            if (start > end)
            {
                return keys;
            }

            switch (granularity)
            {
                case AnalyticsGranularity.Week:
                    var weekOffset = ((int)start.DayOfWeek + 6) % 7;
                    for (var w = start.Date.AddDays(-weekOffset); w <= end.Date; w = w.AddDays(7))
                    {
                        keys.Add(BucketKey(w, granularity));
                    }
                    break;

                case AnalyticsGranularity.Month:
                    for (var m = new DateTime(start.Year, start.Month, 1); m <= end.Date; m = m.AddMonths(1))
                    {
                        keys.Add(BucketKey(m, granularity));
                    }
                    break;

                default:
                    for (var d = start.Date; d <= end.Date; d = d.AddDays(1))
                    {
                        keys.Add(BucketKey(d, granularity));
                    }
                    break;
            }

            return keys;
        }

        /// <summary>Parses a bucket key back into a <see cref="DateTime"/> — "yyyy-MM-dd" for Day/Week, "yyyy-MM" (day defaults to the 1st) for Month.</summary>
        public static DateTime ParseBucketKey(string key)
        {
            var format = key.Length == 7 ? "yyyy-MM" : "yyyy-MM-dd";
            return DateTime.ParseExact(key, format, System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// The period immediately preceding <paramref name="from"/>, of the same duration as
        /// [<paramref name="from"/>, <paramref name="to"/>) — used to compute "vs previous
        /// period" comparisons for an arbitrary user-selected date range (Executive Summary).
        /// </summary>
        public static (DateTime Start, DateTime End) ResolvePreviousPeriod(DateTime from, DateTime to)
        {
            var duration = to - from;
            return (from - duration, from);
        }
    }
}
