using Xunit;
using Philiprehberger.CronExpression;

namespace Philiprehberger.CronExpression.Tests;

public class DescribeTests
{
    [Fact]
    public void Describe_EveryFiveMinutes()
    {
        var schedule = Cron.Parse("*/5 * * * *");
        Assert.Equal("Every 5 minutes", schedule.Describe());
    }

    [Fact]
    public void Describe_AtSpecificTime()
    {
        var schedule = Cron.Parse("0 9 * * *");
        Assert.Equal("At 09:00", schedule.Describe());
    }

    [Fact]
    public void Describe_AtTimeOnWeekdays()
    {
        var schedule = Cron.Parse("0 9 * * 1-5");
        Assert.Equal("At 09:00 on Monday through Friday", schedule.Describe());
    }

    [Fact]
    public void Describe_MidnightOnJanFirst()
    {
        var schedule = Cron.Parse("0 0 1 1 *");
        Assert.Equal("At 00:00 on January 1st", schedule.Describe());
    }

    [Fact]
    public void Describe_EveryMinute()
    {
        var schedule = Cron.Parse("* * * * *");
        Assert.Equal("Every minute", schedule.Describe());
    }

    [Fact]
    public void Describe_EveryHourAtMinuteZero()
    {
        var schedule = Cron.Parse("0 * * * *");
        Assert.Equal("Every hour at minute 00", schedule.Describe());
    }

    [Fact]
    public void Describe_WithSeconds()
    {
        var schedule = Cron.Parse("*/30 * * * * *");
        Assert.Equal("Every 30 seconds", schedule.Describe());
    }

    [Fact]
    public void Describe_SpecificTimeWithSecond()
    {
        var schedule = Cron.Parse("15 30 9 * * *");
        Assert.Equal("At 09:30:15", schedule.Describe());
    }

    [Fact]
    public void Describe_OnSpecificDaysOfWeek()
    {
        var schedule = Cron.Parse("0 12 * * 1,3,5");
        Assert.Equal("At 12:00 on Monday, Wednesday and Friday", schedule.Describe());
    }

    [Fact]
    public void Describe_OnSpecificMonth()
    {
        var schedule = Cron.Parse("0 0 15 6 *");
        Assert.Equal("At 00:00 on June 15th", schedule.Describe());
    }
}
