using Xunit;
using Philiprehberger.CronExpression;

namespace Philiprehberger.CronExpression.Tests;

public class CronParseTests
{
    [Fact]
    public void Parse_StandardFiveField_ReturnsSchedule()
    {
        var schedule = Cron.Parse("*/5 * * * *");
        Assert.NotNull(schedule);
        Assert.Equal("*/5 * * * *", schedule.Expression);
    }

    [Fact]
    public void Parse_Null_ThrowsArgumentNull()
    {
        Assert.Throws<ArgumentNullException>(() => Cron.Parse(null!));
    }

    [Fact]
    public void Parse_InvalidFieldCount_ThrowsCronParseException()
    {
        var ex = Assert.Throws<CronParseException>(() => Cron.Parse("* *"));
        Assert.Contains("Expected 5 or 6 fields", ex.Message);
    }

    [Fact]
    public void Parse_InvalidValue_ThrowsCronParseException()
    {
        Assert.Throws<CronParseException>(() => Cron.Parse("abc * * * *"));
    }

    [Fact]
    public void TryParse_ValidExpression_ReturnsTrue()
    {
        var result = Cron.TryParse("0 9 * * *", out var schedule);
        Assert.True(result);
        Assert.NotNull(schedule);
    }

    [Fact]
    public void TryParse_InvalidExpression_ReturnsFalse()
    {
        var result = Cron.TryParse("invalid", out var schedule);
        Assert.False(result);
        Assert.Null(schedule);
    }

    [Fact]
    public void TryParse_NullExpression_ReturnsFalse()
    {
        var result = Cron.TryParse(null, out var schedule);
        Assert.False(result);
        Assert.Null(schedule);
    }

    [Fact]
    public void IsMatch_MatchingTime_ReturnsTrue()
    {
        var time = new DateTimeOffset(2026, 3, 27, 9, 0, 0, TimeSpan.Zero);
        Assert.True(Cron.IsMatch("0 9 * * *", time));
    }

    [Fact]
    public void IsMatch_NonMatchingTime_ReturnsFalse()
    {
        var time = new DateTimeOffset(2026, 3, 27, 10, 0, 0, TimeSpan.Zero);
        Assert.False(Cron.IsMatch("0 9 * * *", time));
    }
}
