using FluentAssertions;
using SylviaNG.Prescription.Application.Features.Analytics;

namespace SylviaNG.Prescription.Tests.Handlers.Analytics;

public class AnalyticsDateBucketingTests
{
    [Fact]
    public void BucketKey_Day_ShouldReturnIsoDate()
    {
        AnalyticsDateBucketing.BucketKey(new DateTime(2026, 3, 7), AnalyticsGranularity.Day).Should().Be("2026-03-07");
    }

    [Fact]
    public void BucketKey_Month_ShouldReturnYearMonth()
    {
        AnalyticsDateBucketing.BucketKey(new DateTime(2026, 3, 7), AnalyticsGranularity.Month).Should().Be("2026-03");
    }

    [Theory]
    [InlineData("2024-01-01", "2024-01-01")] // Monday itself
    [InlineData("2024-01-07", "2024-01-01")] // Sunday -> Monday of the same week
    [InlineData("2024-01-06", "2024-01-01")] // Saturday -> Monday of the same week
    [InlineData("2023-12-31", "2023-12-25")] // Sunday spanning a year boundary
    public void BucketKey_Week_ShouldReturnMondayOfThatWeek(string inputDate, string expectedMonday)
    {
        var date = DateTime.Parse(inputDate);

        AnalyticsDateBucketing.BucketKey(date, AnalyticsGranularity.Week).Should().Be(expectedMonday);
    }

    [Fact]
    public void BuildTrend_ShouldSkipNullDatesAndSortAscendingByBucketKey()
    {
        var items = new List<DateTime?>
        {
            new DateTime(2026, 3, 3),
            null,
            new DateTime(2026, 3, 1),
            new DateTime(2026, 3, 3),
        };

        var trend = AnalyticsDateBucketing.BuildTrend(items, d => d, AnalyticsGranularity.Day);

        trend.Should().HaveCount(2);
        trend[0].BucketKey.Should().Be("2026-03-01");
        trend[0].Count.Should().Be(1);
        trend[1].BucketKey.Should().Be("2026-03-03");
        trend[1].Count.Should().Be(2);
    }

    [Fact]
    public void ToBangladeshTime_ShouldAddSixHours()
    {
        AnalyticsDateBucketing.ToBangladeshTime(new DateTime(2026, 3, 7, 20, 0, 0))
            .Should().Be(new DateTime(2026, 3, 8, 2, 0, 0));
    }

    [Fact]
    public void BuildTrendZeroFilled_ShouldZeroFillEveryDayInRangeChronologically()
    {
        var items = new List<DateTime?> { new DateTime(2026, 3, 3, 12, 0, 0) };

        var trend = AnalyticsDateBucketing.BuildTrendZeroFilled(
            items, d => d, AnalyticsGranularity.Day,
            new DateTime(2026, 3, 1), new DateTime(2026, 3, 5));

        trend.Select(p => p.BucketKey).Should().Equal("2026-03-01", "2026-03-02", "2026-03-03", "2026-03-04", "2026-03-05");
        trend.Select(p => p.Count).Should().Equal(0, 0, 1, 0, 0);
    }

    [Fact]
    public void BuildTrendZeroFilled_ShouldShiftBucketBoundaryToBangladeshTime()
    {
        // 23:30 UTC on Mar 3rd is 05:30 BDT on Mar 4th — must land in the Mar 4th bucket, not Mar 3rd.
        var items = new List<DateTime?> { new DateTime(2026, 3, 3, 23, 30, 0) };

        var trend = AnalyticsDateBucketing.BuildTrendZeroFilled(
            items, d => d, AnalyticsGranularity.Day,
            new DateTime(2026, 3, 3), new DateTime(2026, 3, 4));

        trend.Single(p => p.BucketKey == "2026-03-04").Count.Should().Be(1);
        trend.Single(p => p.BucketKey == "2026-03-03").Count.Should().Be(0);
    }

    [Fact]
    public void BuildTrendZeroFilled_ShouldZeroFillEveryMonthInRange()
    {
        var trend = AnalyticsDateBucketing.BuildTrendZeroFilled(
            new List<DateTime?>(), d => d, AnalyticsGranularity.Month,
            new DateTime(2026, 1, 15), new DateTime(2026, 4, 2));

        trend.Select(p => p.BucketKey).Should().Equal("2026-01", "2026-02", "2026-03", "2026-04");
        trend.Select(p => p.Count).Should().AllBeEquivalentTo(0);
    }

    [Fact]
    public void ParseBucketKey_ShouldParseDayAndMonthKeys()
    {
        AnalyticsDateBucketing.ParseBucketKey("2026-03-07").Should().Be(new DateTime(2026, 3, 7));
        AnalyticsDateBucketing.ParseBucketKey("2026-03").Should().Be(new DateTime(2026, 3, 1));
    }

    [Fact]
    public void GetDefaultRange_ShouldReturnTrailingWindowSizedToGranularity()
    {
        var now = new DateTime(2026, 3, 31);

        AnalyticsDateBucketing.GetDefaultRange(AnalyticsGranularity.Day, now).Start.Should().Be(now.AddDays(-29));
        AnalyticsDateBucketing.GetDefaultRange(AnalyticsGranularity.Week, now).Start.Should().Be(now.AddDays(-77));
        AnalyticsDateBucketing.GetDefaultRange(AnalyticsGranularity.Month, now).Start.Should().Be(now.AddMonths(-11));
    }
}
