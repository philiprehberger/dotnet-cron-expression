using Xunit;

namespace Philiprehberger.CronExpression.Tests;

public class CronParseTests
{
    [Fact]
    public void Parse_StandardFiveField_ReturnsSchedule()
    {
        var schedule = Cron.Parse("*/5 * * * *");
        Assert.Equal("*/5 * * * *", schedule.Expression);
        Assert.False(schedule.HasSeconds);
    }

    [Fact]
    public void Parse_SixField_WithSeconds()
    {
        var schedule = Cron.Parse("*/30 * * * * *");
        Assert.True(schedule.HasSeconds);
    }

    [Theory]
    [InlineData("@daily", "0 0 * * *")]
    [InlineData("@hourly", "0 * * * *")]
    [InlineData("@weekly", "0 0 * * 0")]
    [InlineData("@monthly", "0 0 1 * *")]
    [InlineData("@yearly", "0 0 1 1 *")]
    [InlineData("@annually", "0 0 1 1 *")]
    [InlineData("@midnight", "0 0 * * *")]
    public void Parse_Aliases(string alias, string expected)
    {
        var schedule = Cron.Parse(alias);
        // Alias expressions store the alias as the expression string
        Assert.NotNull(schedule);
    }

    [Fact]
    public void Parse_InvalidExpression_Throws()
    {
        Assert.Throws<CronParseException>(() => Cron.Parse("invalid"));
    }

    [Fact]
    public void Parse_NullExpression_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => Cron.Parse(null!));
    }

    [Fact]
    public void TryParse_ValidExpression_ReturnsTrue()
    {
        Assert.True(Cron.TryParse("0 0 * * *", out var schedule));
        Assert.NotNull(schedule);
    }

    [Fact]
    public void TryParse_InvalidExpression_ReturnsFalse()
    {
        Assert.False(Cron.TryParse("invalid", out var schedule));
        Assert.Null(schedule);
    }

    [Fact]
    public void TryParse_NullExpression_ReturnsFalse()
    {
        Assert.False(Cron.TryParse(null, out var schedule));
        Assert.Null(schedule);
    }
}

public class CronScheduleTests
{
    [Fact]
    public void IsMatch_MatchingTime_ReturnsTrue()
    {
        var schedule = Cron.Parse("0 9 * * *");
        var time = new DateTimeOffset(2026, 3, 15, 9, 0, 0, TimeSpan.Zero);
        Assert.True(schedule.IsMatch(time));
    }

    [Fact]
    public void IsMatch_NonMatchingTime_ReturnsFalse()
    {
        var schedule = Cron.Parse("0 9 * * *");
        var time = new DateTimeOffset(2026, 3, 15, 10, 0, 0, TimeSpan.Zero);
        Assert.False(schedule.IsMatch(time));
    }

    [Fact]
    public void NextOccurrence_ReturnsCorrectTime()
    {
        var schedule = Cron.Parse("0 9 * * *");
        var after = new DateTimeOffset(2026, 3, 15, 8, 0, 0, TimeSpan.Zero);
        var next = schedule.NextOccurrence(after);
        Assert.Equal(new DateTimeOffset(2026, 3, 15, 9, 0, 0, TimeSpan.Zero), next);
    }

    [Fact]
    public void NextOccurrence_SkipsToNextDay()
    {
        var schedule = Cron.Parse("0 9 * * *");
        var after = new DateTimeOffset(2026, 3, 15, 10, 0, 0, TimeSpan.Zero);
        var next = schedule.NextOccurrence(after);
        Assert.Equal(new DateTimeOffset(2026, 3, 16, 9, 0, 0, TimeSpan.Zero), next);
    }

    [Fact]
    public void PreviousOccurrence_ReturnsCorrectTime()
    {
        var schedule = Cron.Parse("0 9 * * *");
        var before = new DateTimeOffset(2026, 3, 15, 10, 0, 0, TimeSpan.Zero);
        var prev = schedule.PreviousOccurrence(before);
        Assert.Equal(new DateTimeOffset(2026, 3, 15, 9, 0, 0, TimeSpan.Zero), prev);
    }

    [Fact]
    public void GetOccurrences_ReturnsAllMatchesInRange()
    {
        var schedule = Cron.Parse("0 0 * * *"); // Midnight daily
        var start = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2026, 1, 3, 23, 59, 59, TimeSpan.Zero);
        var results = schedule.GetOccurrences(start, end).ToList();
        Assert.Equal(3, results.Count);
    }

    [Fact]
    public void Describe_ReturnsHumanReadable()
    {
        var schedule = Cron.Parse("*/5 * * * *");
        Assert.Equal("Every 5 minutes", schedule.Describe());
    }

    [Fact]
    public void ToString_ReturnsExpression()
    {
        var schedule = Cron.Parse("0 9 * * MON-FRI");
        Assert.Equal("0 9 * * MON-FRI", schedule.ToString());
    }
}

public class CronStaticMatchTests
{
    [Fact]
    public void IsMatch_StaticMethod_Works()
    {
        var time = new DateTimeOffset(2026, 3, 15, 9, 0, 0, TimeSpan.Zero);
        Assert.True(Cron.IsMatch("0 9 * * *", time));
        Assert.False(Cron.IsMatch("0 10 * * *", time));
    }
}

public class TimezoneSchedulingTests
{
    [Fact]
    public void NextOccurrence_WithTimeZone_ConvertsCorrectly()
    {
        var schedule = Cron.Parse("0 9 * * *");
        var utcTime = new DateTimeOffset(2026, 6, 15, 12, 0, 0, TimeSpan.Zero);
        var eastern = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");

        var next = schedule.NextOccurrence(utcTime, eastern);

        // 12:00 UTC = 08:00 EDT, so next 9:00 EDT should be same day
        Assert.Equal(9, next.Hour);
        Assert.Equal(15, next.Day);
    }

    [Fact]
    public void NextOccurrence_WithTimeZone_NullTimeZone_Throws()
    {
        var schedule = Cron.Parse("0 9 * * *");
        Assert.Throws<ArgumentNullException>(() =>
            schedule.NextOccurrence(DateTimeOffset.UtcNow, (TimeZoneInfo)null!));
    }

    [Fact]
    public void NextOccurrence_WithTimeZone_AfterDailySchedule_AdvancesToNextDay()
    {
        var schedule = Cron.Parse("0 9 * * *");
        var eastern = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
        // 15:00 UTC = 11:00 EDT (past 9:00 AM)
        var utcTime = new DateTimeOffset(2026, 6, 15, 15, 0, 0, TimeSpan.Zero);

        var next = schedule.NextOccurrence(utcTime, eastern);

        Assert.Equal(9, next.Hour);
        Assert.Equal(16, next.Day);
    }

    [Fact]
    public void GetOccurrences_WithTimeZone_ReturnsCorrectResults()
    {
        var schedule = Cron.Parse("0 0 * * *"); // Midnight
        var eastern = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
        var start = new DateTimeOffset(2026, 1, 1, 5, 0, 0, TimeSpan.Zero); // midnight EST
        var end = new DateTimeOffset(2026, 1, 4, 5, 0, 0, TimeSpan.Zero);

        var results = schedule.GetOccurrences(start, end, eastern).ToList();
        Assert.True(results.Count >= 3);
    }
}

public class ExclusionCalendarTests
{
    [Fact]
    public void Constructor_Empty_HasZeroCount()
    {
        var calendar = new ExclusionCalendar();
        Assert.Equal(0, calendar.Count);
    }

    [Fact]
    public void Constructor_WithDates_PopulatesSet()
    {
        var dates = new[] { new DateOnly(2026, 12, 25), new DateOnly(2026, 1, 1) };
        var calendar = new ExclusionCalendar(dates);
        Assert.Equal(2, calendar.Count);
    }

    [Fact]
    public void Add_NewDate_ReturnsTrue()
    {
        var calendar = new ExclusionCalendar();
        Assert.True(calendar.Add(new DateOnly(2026, 7, 4)));
        Assert.Equal(1, calendar.Count);
    }

    [Fact]
    public void Add_DuplicateDate_ReturnsFalse()
    {
        var calendar = new ExclusionCalendar();
        calendar.Add(new DateOnly(2026, 7, 4));
        Assert.False(calendar.Add(new DateOnly(2026, 7, 4)));
    }

    [Fact]
    public void Remove_ExistingDate_ReturnsTrue()
    {
        var calendar = new ExclusionCalendar(new[] { new DateOnly(2026, 7, 4) });
        Assert.True(calendar.Remove(new DateOnly(2026, 7, 4)));
        Assert.Equal(0, calendar.Count);
    }

    [Fact]
    public void IsExcluded_DateOnly_ReturnsTrueForExcludedDate()
    {
        var calendar = new ExclusionCalendar(new[] { new DateOnly(2026, 12, 25) });
        Assert.True(calendar.IsExcluded(new DateOnly(2026, 12, 25)));
        Assert.False(calendar.IsExcluded(new DateOnly(2026, 12, 26)));
    }

    [Fact]
    public void IsExcluded_DateTimeOffset_ReturnsTrueForExcludedDate()
    {
        var calendar = new ExclusionCalendar(new[] { new DateOnly(2026, 12, 25) });
        var time = new DateTimeOffset(2026, 12, 25, 9, 0, 0, TimeSpan.Zero);
        Assert.True(calendar.IsExcluded(time));
    }

    [Fact]
    public void Clear_RemovesAllDates()
    {
        var calendar = new ExclusionCalendar(new[] { new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 25) });
        calendar.Clear();
        Assert.Equal(0, calendar.Count);
    }

    [Fact]
    public void GetDates_ReturnsSortedDates()
    {
        var calendar = new ExclusionCalendar(new[]
        {
            new DateOnly(2026, 12, 25),
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 7, 4),
        });
        var dates = calendar.GetDates();
        Assert.Equal(3, dates.Count);
        Assert.Equal(new DateOnly(2026, 1, 1), dates[0]);
        Assert.Equal(new DateOnly(2026, 7, 4), dates[1]);
        Assert.Equal(new DateOnly(2026, 12, 25), dates[2]);
    }

    [Fact]
    public void AddRange_AddsMultipleDates()
    {
        var calendar = new ExclusionCalendar();
        calendar.AddRange(new[] { new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 25) });
        Assert.Equal(2, calendar.Count);
    }

    [Fact]
    public void NextOccurrence_WithExclusionCalendar_SkipsExcludedDates()
    {
        var schedule = Cron.Parse("0 9 * * *"); // Daily at 9 AM
        var calendar = new ExclusionCalendar(new[] { new DateOnly(2026, 3, 16) });

        var after = new DateTimeOffset(2026, 3, 15, 10, 0, 0, TimeSpan.Zero);
        var next = schedule.NextOccurrence(after, calendar);

        // Should skip March 16 and return March 17
        Assert.Equal(new DateTimeOffset(2026, 3, 17, 9, 0, 0, TimeSpan.Zero), next);
    }

    [Fact]
    public void GetOccurrences_WithExclusionCalendar_SkipsExcludedDates()
    {
        var schedule = Cron.Parse("0 0 * * *"); // Midnight daily
        var calendar = new ExclusionCalendar(new[] { new DateOnly(2026, 1, 2) });

        var start = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2026, 1, 3, 23, 59, 59, TimeSpan.Zero);
        var results = schedule.GetOccurrences(start, end, calendar).ToList();

        Assert.Equal(2, results.Count); // Jan 1, Jan 3 (skips Jan 2)
        Assert.Equal(1, results[0].Day);
        Assert.Equal(3, results[1].Day);
    }
}

public class NthWeekdayTests
{
    [Fact]
    public void Parse_NthWeekday_ThirdFriday()
    {
        var schedule = Cron.Parse("0 9 * * 5#3");
        Assert.NotNull(schedule);
    }

    [Fact]
    public void IsMatch_ThirdFriday_MatchesCorrectDate()
    {
        var schedule = Cron.Parse("0 9 * * 5#3");
        // 2026-03-20 is the third Friday of March 2026
        var thirdFriday = new DateTimeOffset(2026, 3, 20, 9, 0, 0, TimeSpan.Zero);
        Assert.True(schedule.IsMatch(thirdFriday));
    }

    [Fact]
    public void IsMatch_ThirdFriday_DoesNotMatchOtherFridays()
    {
        var schedule = Cron.Parse("0 9 * * 5#3");
        // 2026-03-06 is the first Friday of March 2026
        var firstFriday = new DateTimeOffset(2026, 3, 6, 9, 0, 0, TimeSpan.Zero);
        Assert.False(schedule.IsMatch(firstFriday));

        // 2026-03-13 is the second Friday
        var secondFriday = new DateTimeOffset(2026, 3, 13, 9, 0, 0, TimeSpan.Zero);
        Assert.False(schedule.IsMatch(secondFriday));
    }

    [Fact]
    public void IsMatch_SecondMonday_MatchesCorrectDate()
    {
        var schedule = Cron.Parse("0 0 * * 1#2");
        // 2026-03-09 is the second Monday of March 2026
        var secondMonday = new DateTimeOffset(2026, 3, 9, 0, 0, 0, TimeSpan.Zero);
        Assert.True(schedule.IsMatch(secondMonday));
    }

    [Fact]
    public void NextOccurrence_ThirdFriday_FindsCorrectDate()
    {
        var schedule = Cron.Parse("0 9 * * 5#3");
        var after = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero);
        var next = schedule.NextOccurrence(after);

        // Third Friday of March 2026 is the 20th
        Assert.Equal(new DateTimeOffset(2026, 3, 20, 9, 0, 0, TimeSpan.Zero), next);
    }

    [Fact]
    public void NextOccurrence_ThirdFriday_AfterThisMonth_GoesToNextMonth()
    {
        var schedule = Cron.Parse("0 9 * * 5#3");
        var after = new DateTimeOffset(2026, 3, 21, 0, 0, 0, TimeSpan.Zero);
        var next = schedule.NextOccurrence(after);

        // Third Friday of April 2026 is the 17th
        Assert.Equal(new DateTimeOffset(2026, 4, 17, 9, 0, 0, TimeSpan.Zero), next);
    }

    [Fact]
    public void PreviousOccurrence_ThirdFriday_FindsCorrectDate()
    {
        var schedule = Cron.Parse("0 9 * * 5#3");
        var before = new DateTimeOffset(2026, 3, 25, 0, 0, 0, TimeSpan.Zero);
        var prev = schedule.PreviousOccurrence(before);

        Assert.Equal(new DateTimeOffset(2026, 3, 20, 9, 0, 0, TimeSpan.Zero), prev);
    }

    [Fact]
    public void Parse_InvalidNthValue_Throws()
    {
        Assert.Throws<CronParseException>(() => Cron.Parse("0 9 * * 5#0"));
        Assert.Throws<CronParseException>(() => Cron.Parse("0 9 * * 5#6"));
    }

    [Fact]
    public void Parse_NthWeekday_WithDayName()
    {
        var schedule = Cron.Parse("0 9 * * FRI#3");
        var thirdFriday = new DateTimeOffset(2026, 3, 20, 9, 0, 0, TimeSpan.Zero);
        Assert.True(schedule.IsMatch(thirdFriday));
    }
}

public class CronBuilderTests
{
    [Fact]
    public void Build_Default_EveryMinute()
    {
        var schedule = new CronBuilder().Build();
        Assert.Equal("* * * * *", schedule.Expression);
    }

    [Fact]
    public void Build_AtSpecificTime()
    {
        var schedule = new CronBuilder().At(9, 30).Build();
        Assert.Equal("30 9 * * *", schedule.Expression);
    }

    [Fact]
    public void Build_WithDaysOfWeek()
    {
        var schedule = new CronBuilder()
            .At(9, 0)
            .OnDaysOfWeek(DayOfWeek.Monday, DayOfWeek.Friday)
            .Build();
        Assert.Equal("0 9 * * 1,5", schedule.Expression);
    }

    [Fact]
    public void Build_EveryNMinutes()
    {
        var schedule = new CronBuilder().Every(5).Minutes().Build();
        Assert.Equal("*/5 * * * *", schedule.Expression);
    }

    [Fact]
    public void Build_EveryNHours()
    {
        var schedule = new CronBuilder().Every(2).Hours().Build();
        Assert.Equal("0 */2 * * *", schedule.Expression);
    }

    [Fact]
    public void AtMinute_OutOfRange_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CronBuilder().AtMinute(60));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CronBuilder().AtMinute(-1));
    }

    [Fact]
    public void AtHour_OutOfRange_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CronBuilder().AtHour(24));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CronBuilder().AtHour(-1));
    }
}

public class CronFieldParsingTests
{
    [Fact]
    public void Parse_Ranges_Work()
    {
        var schedule = Cron.Parse("0 9-17 * * *");
        var time9 = new DateTimeOffset(2026, 3, 15, 9, 0, 0, TimeSpan.Zero);
        var time17 = new DateTimeOffset(2026, 3, 15, 17, 0, 0, TimeSpan.Zero);
        var time18 = new DateTimeOffset(2026, 3, 15, 18, 0, 0, TimeSpan.Zero);

        Assert.True(schedule.IsMatch(time9));
        Assert.True(schedule.IsMatch(time17));
        Assert.False(schedule.IsMatch(time18));
    }

    [Fact]
    public void Parse_Lists_Work()
    {
        var schedule = Cron.Parse("0,15,30,45 * * * *");
        var time0 = new DateTimeOffset(2026, 3, 15, 12, 0, 0, TimeSpan.Zero);
        var time15 = new DateTimeOffset(2026, 3, 15, 12, 15, 0, TimeSpan.Zero);
        var time10 = new DateTimeOffset(2026, 3, 15, 12, 10, 0, TimeSpan.Zero);

        Assert.True(schedule.IsMatch(time0));
        Assert.True(schedule.IsMatch(time15));
        Assert.False(schedule.IsMatch(time10));
    }

    [Fact]
    public void Parse_Steps_Work()
    {
        var schedule = Cron.Parse("*/15 * * * *");
        var time0 = new DateTimeOffset(2026, 3, 15, 12, 0, 0, TimeSpan.Zero);
        var time15 = new DateTimeOffset(2026, 3, 15, 12, 15, 0, TimeSpan.Zero);
        var time30 = new DateTimeOffset(2026, 3, 15, 12, 30, 0, TimeSpan.Zero);
        var time45 = new DateTimeOffset(2026, 3, 15, 12, 45, 0, TimeSpan.Zero);
        var time10 = new DateTimeOffset(2026, 3, 15, 12, 10, 0, TimeSpan.Zero);

        Assert.True(schedule.IsMatch(time0));
        Assert.True(schedule.IsMatch(time15));
        Assert.True(schedule.IsMatch(time30));
        Assert.True(schedule.IsMatch(time45));
        Assert.False(schedule.IsMatch(time10));
    }

    [Fact]
    public void Parse_DayNames_Work()
    {
        var schedule = Cron.Parse("0 9 * * MON-FRI");
        // 2026-03-16 is a Monday
        var monday = new DateTimeOffset(2026, 3, 16, 9, 0, 0, TimeSpan.Zero);
        // 2026-03-15 is a Sunday
        var sunday = new DateTimeOffset(2026, 3, 15, 9, 0, 0, TimeSpan.Zero);

        Assert.True(schedule.IsMatch(monday));
        Assert.False(schedule.IsMatch(sunday));
    }

    [Fact]
    public void Parse_MonthNames_Work()
    {
        var schedule = Cron.Parse("0 0 1 JAN *");
        var jan = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var feb = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero);

        Assert.True(schedule.IsMatch(jan));
        Assert.False(schedule.IsMatch(feb));
    }
}

public class CronDescribeTests
{
    [Theory]
    [InlineData("* * * * *", "Every minute")]
    [InlineData("*/5 * * * *", "Every 5 minutes")]
    [InlineData("0 0 * * *", "At 00:00")]
    [InlineData("0 9 * * 1-5", "At 09:00 on Monday through Friday")]
    [InlineData("0 0 1 1 *", "At 00:00 on January 1st")]
    public void Describe_ReturnsExpectedDescription(string expression, string expected)
    {
        var schedule = Cron.Parse(expression);
        Assert.Equal(expected, schedule.Describe());
    }
}

public class CronSecondsFieldTests
{
    [Fact]
    public void Parse_SixField_MatchesSecond()
    {
        var schedule = Cron.Parse("30 0 9 * * *");
        var match = new DateTimeOffset(2026, 3, 15, 9, 0, 30, TimeSpan.Zero);
        var noMatch = new DateTimeOffset(2026, 3, 15, 9, 0, 0, TimeSpan.Zero);

        Assert.True(schedule.IsMatch(match));
        Assert.False(schedule.IsMatch(noMatch));
    }

    [Fact]
    public void NextOccurrence_WithSeconds_AdvancesBySecond()
    {
        var schedule = Cron.Parse("*/10 * * * * *"); // Every 10 seconds
        var after = new DateTimeOffset(2026, 3, 15, 9, 0, 0, TimeSpan.Zero);
        var next = schedule.NextOccurrence(after);

        Assert.Equal(new DateTimeOffset(2026, 3, 15, 9, 0, 10, TimeSpan.Zero), next);
    }
}
