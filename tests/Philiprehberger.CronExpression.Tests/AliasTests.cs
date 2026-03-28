using Xunit;
using Philiprehberger.CronExpression;

namespace Philiprehberger.CronExpression.Tests;

public class AliasTests
{
    [Fact]
    public void Parse_Yearly_MatchesJanFirst()
    {
        var schedule = Cron.Parse("@yearly");
        var time = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        Assert.True(schedule.IsMatch(time));
    }

    [Fact]
    public void Parse_Yearly_DoesNotMatchOtherDays()
    {
        var schedule = Cron.Parse("@yearly");
        var time = new DateTimeOffset(2026, 6, 15, 12, 30, 0, TimeSpan.Zero);
        Assert.False(schedule.IsMatch(time));
    }

    [Fact]
    public void Parse_Monthly_MatchesFirstOfMonth()
    {
        var schedule = Cron.Parse("@monthly");
        var time = new DateTimeOffset(2026, 5, 1, 0, 0, 0, TimeSpan.Zero);
        Assert.True(schedule.IsMatch(time));
    }

    [Fact]
    public void Parse_Weekly_MatchesSunday()
    {
        var schedule = Cron.Parse("@weekly");
        // 2026-03-29 is a Sunday
        var time = new DateTimeOffset(2026, 3, 29, 0, 0, 0, TimeSpan.Zero);
        Assert.True(schedule.IsMatch(time));
    }

    [Fact]
    public void Parse_Daily_MatchesMidnight()
    {
        var schedule = Cron.Parse("@daily");
        var time = new DateTimeOffset(2026, 3, 27, 0, 0, 0, TimeSpan.Zero);
        Assert.True(schedule.IsMatch(time));
    }

    [Fact]
    public void Parse_Midnight_SameAsDaily()
    {
        var daily = Cron.Parse("@daily");
        var midnight = Cron.Parse("@midnight");
        var time = new DateTimeOffset(2026, 3, 27, 0, 0, 0, TimeSpan.Zero);
        Assert.Equal(daily.IsMatch(time), midnight.IsMatch(time));
    }

    [Fact]
    public void Parse_Hourly_MatchesTopOfHour()
    {
        var schedule = Cron.Parse("@hourly");
        var time = new DateTimeOffset(2026, 3, 27, 14, 0, 0, TimeSpan.Zero);
        Assert.True(schedule.IsMatch(time));
    }

    [Fact]
    public void Parse_Hourly_DoesNotMatchMidHour()
    {
        var schedule = Cron.Parse("@hourly");
        var time = new DateTimeOffset(2026, 3, 27, 14, 30, 0, TimeSpan.Zero);
        Assert.False(schedule.IsMatch(time));
    }

    [Fact]
    public void Parse_AliasIsCaseInsensitive()
    {
        var schedule = Cron.Parse("@DAILY");
        var time = new DateTimeOffset(2026, 3, 27, 0, 0, 0, TimeSpan.Zero);
        Assert.True(schedule.IsMatch(time));
    }
}
