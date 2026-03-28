using Xunit;
using Philiprehberger.CronExpression;

namespace Philiprehberger.CronExpression.Tests;

public class CronScheduleTests
{
    [Fact]
    public void NextOccurrence_ReturnsCorrectTime()
    {
        var schedule = Cron.Parse("0 9 * * *");
        var after = new DateTimeOffset(2026, 3, 27, 8, 0, 0, TimeSpan.Zero);
        var next = schedule.NextOccurrence(after);
        Assert.Equal(9, next.Hour);
        Assert.Equal(0, next.Minute);
    }

    [Fact]
    public void PreviousOccurrence_ReturnsCorrectTime()
    {
        var schedule = Cron.Parse("0 9 * * *");
        var before = new DateTimeOffset(2026, 3, 27, 10, 0, 0, TimeSpan.Zero);
        var prev = schedule.PreviousOccurrence(before);
        Assert.Equal(9, prev.Hour);
        Assert.Equal(0, prev.Minute);
        Assert.Equal(27, prev.Day);
    }

    [Fact]
    public void GetOccurrences_ReturnsAllInRange()
    {
        var schedule = Cron.Parse("0 0 1 * *");
        var start = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2026, 3, 31, 23, 59, 59, TimeSpan.Zero);
        var occurrences = schedule.GetOccurrences(start, end).ToList();
        Assert.Equal(3, occurrences.Count);
    }

    [Fact]
    public void ToString_ReturnsExpression()
    {
        var schedule = Cron.Parse("*/5 * * * *");
        Assert.Equal("*/5 * * * *", schedule.ToString());
    }

    [Fact]
    public void IsMatch_DayOfWeekNames()
    {
        var schedule = Cron.Parse("0 0 * * MON");
        // 2026-03-30 is a Monday
        var time = new DateTimeOffset(2026, 3, 30, 0, 0, 0, TimeSpan.Zero);
        Assert.True(schedule.IsMatch(time));
    }

    [Fact]
    public void IsMatch_MonthNames()
    {
        var schedule = Cron.Parse("0 0 1 JAN *");
        var time = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        Assert.True(schedule.IsMatch(time));
    }
}
