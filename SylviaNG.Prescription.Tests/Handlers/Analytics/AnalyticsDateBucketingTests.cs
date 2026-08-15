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
}
