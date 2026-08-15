namespace SylviaNG.Prescription.Application.Features.Analytics
{
    /// <summary>
    /// Small shared math helpers used across every Analytics handler (US-072–078), ported
    /// from the reference prototype's <c>dateBuckets.js</c>/<c>analyticsService.js</c> so the
    /// "no baseline yet" and "divide by zero" edge cases match exactly everywhere they occur.
    /// </summary>
    public static class AnalyticsMath
    {
        /// <summary>
        /// Month-over-month style percent change. Null means "no baseline" (previous was
        /// zero and current is non-zero) — the frontend renders that as "New", not "0%" or
        /// an error. Zero previous with zero current is a real, renderable 0% (no change).
        /// </summary>
        public static double? PercentChange(int current, int previous)
        {
            if (previous == 0)
            {
                return current == 0 ? 0 : null;
            }

            return Math.Round((current - previous) / (double)previous * 100);
        }

        public static double SafeDivide(int numerator, int denominator, int decimals = 2)
        {
            if (denominator == 0)
            {
                return 0;
            }

            return Math.Round(numerator / (double)denominator, decimals);
        }
    }
}
