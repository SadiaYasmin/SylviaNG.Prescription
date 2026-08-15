using FluentAssertions;
using SylviaNG.Prescription.Application.Features.Analytics;

namespace SylviaNG.Prescription.Tests.Handlers.Analytics;

public class AnalyticsMathTests
{
    [Fact]
    public void PercentChange_ZeroPreviousZeroCurrent_ShouldBeZero()
    {
        AnalyticsMath.PercentChange(0, 0).Should().Be(0);
    }

    [Fact]
    public void PercentChange_ZeroPreviousNonZeroCurrent_ShouldBeNullNoBaseline()
    {
        AnalyticsMath.PercentChange(5, 0).Should().BeNull();
    }

    [Fact]
    public void PercentChange_NormalIncrease_ShouldRoundToNearestPercent()
    {
        AnalyticsMath.PercentChange(150, 100).Should().Be(50);
    }

    [Fact]
    public void PercentChange_Decrease_ShouldBeNegative()
    {
        AnalyticsMath.PercentChange(50, 100).Should().Be(-50);
    }

    [Fact]
    public void SafeDivide_ZeroDenominator_ShouldBeZeroNotThrow()
    {
        AnalyticsMath.SafeDivide(10, 0).Should().Be(0);
    }

    [Fact]
    public void SafeDivide_ShouldRoundToRequestedDecimals()
    {
        AnalyticsMath.SafeDivide(1, 3, 2).Should().Be(0.33);
    }
}
