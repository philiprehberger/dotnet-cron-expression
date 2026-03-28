using Xunit;
using Philiprehberger.CronExpression;

namespace Philiprehberger.CronExpression.Tests;

public class SecondsFieldTests
{
    [Fact]
    public void Parse_SixFields_ParsesWithSeconds()
    {
        var schedule = Cron.Parse("30 */5 * * * *");
        Assert.True(schedule.HasSeconds);
    }

    [Fact]
    public void Parse_FiveFields_HasNoSeconds()
    {
        var schedule = Cron.Parse("*/5 * * * *");
        Assert.False(schedule.HasSeconds);
    }

    [Fact]
    public void IsMatch_WithSeconds_MatchesSecond()
    {
        var schedule = Cron.Parse("30 0 9 * * *");
        var time = new DateTimeOffset(2026, 3, 27, 9, 0, 30, TimeSpan.Zero);
        Assert.True(schedule.IsMatch(time));
    }

    [Fact]
    public void IsMatch_WithSeconds_DoesNotMatchWrongSecond()
    {
        var schedule = Cron.Parse("30 0 9 * * *");
        var time = new DateTimeOffset(2026, 3, 27, 9, 0, 0, TimeSpan.Zero);
        Assert.False(schedule.IsMatch(time));
    }

    [Fact]
    public void NextOccurrence_WithSeconds_AdvancesBySecond()
    {
        var schedule = Cron.Parse("*/15 * * * * *");
        var after = new DateTimeOffset(2026, 3, 27, 9, 0, 0, TimeSpan.Zero);
        var next = schedule.NextOccurrence(after);
        Assert.Equal(15, next.Second);
        Assert.Equal(0, next.Minute);
    }

    [Fact]
    public void Parse_SixFields_StepSeconds()
    {
        var schedule = Cron.Parse("*/10 */5 * * * *");
        Assert.True(schedule.HasSeconds);
        var time = new DateTimeOffset(2026, 3, 27, 12, 10, 20, TimeSpan.Zero);
        Assert.True(schedule.IsMatch(time));
    }
}
