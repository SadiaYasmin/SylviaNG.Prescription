namespace SylviaNG.Prescription.Application.Features.Analytics
{
    /// <summary>
    /// Trend-chart bucketing granularity (US-074). Mirrors the reference prototype's
    /// Day/Week/Month toggle exactly — see <see cref="AnalyticsDateBucketing"/>.
    /// </summary>
    public enum AnalyticsGranularity
    {
        Day,
        Week,
        Month
    }
}
