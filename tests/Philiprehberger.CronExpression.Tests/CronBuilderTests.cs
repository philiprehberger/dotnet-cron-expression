using Xunit;
using Philiprehberger.CronExpression;

namespace Philiprehberger.CronExpression.Tests;

public class CronBuilderTests
{
    [Fact]
    public void EveryMinute_ProducesWildcardExpression()
    {
        var schedule = new CronBuilder().EveryMinute().Build();
        Assert.Equal("* * * * *", schedule.Expression);
    }

    [Fact]
    public void EveryHour_ProducesZeroMinute()
    {
        var schedule = new CronBuilder().EveryHour().Build();
        Assert.Equal("0 * * * *", schedule.Expression);
    }

    [Fact]
    public void AtMinuteAndHour_ProducesSpecificTime()
    {
        var schedule = new CronBuilder().At(9, 30).Build();
        Assert.Equal("30 9 * * *", schedule.Expression);
    }

    [Fact]
    public void OnDaysOfWeek_ProducesCorrectField()
    {
        var schedule = new CronBuilder()
            .At(9, 0)
            .OnDaysOfWeek(DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday)
            .Build();
        Assert.Equal("0 9 * * 1,3,5", schedule.Expression);
    }

    [Fact]
    public void OnMonths_ProducesCorrectField()
    {
        var schedule = new CronBuilder()
            .AtMinute(0)
            .AtHour(0)
            .OnDaysOfMonth(1)
            .OnMonths(1, 6)
            .Build();
        Assert.Equal("0 0 1 1,6 *", schedule.Expression);
    }

    [Fact]
    public void Every5Minutes_ProducesStepExpression()
    {
        var schedule = new CronBuilder().Every(5).Minutes().Build();
        Assert.Equal("*/5 * * * *", schedule.Expression);
    }

    [Fact]
    public void Every2Hours_ProducesStepExpression()
    {
        var schedule = new CronBuilder().Every(2).Hours().Build();
        Assert.Equal("0 */2 * * *", schedule.Expression);
    }

    [Fact]
    public void AtMinute_OutOfRange_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CronBuilder().AtMinute(60));
    }

    [Fact]
    public void AtHour_OutOfRange_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CronBuilder().AtHour(24));
    }

    [Fact]
    public void OnDaysOfWeek_Empty_Throws()
    {
        Assert.Throws<ArgumentException>(() => new CronBuilder().OnDaysOfWeek());
    }

    [Fact]
    public void Build_ProducesValidSchedule()
    {
        var schedule = new CronBuilder()
            .At(14, 30)
            .OnDaysOfWeek(DayOfWeek.Tuesday)
            .Build();

        var time = new DateTimeOffset(2026, 4, 7, 14, 30, 0, TimeSpan.Zero); // Tuesday
        Assert.True(schedule.IsMatch(time));
    }
}
