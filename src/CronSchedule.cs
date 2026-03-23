namespace Philiprehberger.CronExpression;

/// <summary>
/// Represents a parsed cron schedule that can compute matching times.
/// </summary>
public sealed class CronSchedule
{
    private readonly CronField _minute;
    private readonly CronField _hour;
    private readonly CronField _dayOfMonth;
    private readonly CronField _month;
    private readonly CronField _dayOfWeek;

    /// <summary>
    /// Gets the original cron expression string.
    /// </summary>
    public string Expression { get; }

    internal CronSchedule(string expression, CronField minute, CronField hour, CronField dayOfMonth, CronField month, CronField dayOfWeek)
    {
        Expression = expression;
        _minute = minute;
        _hour = hour;
        _dayOfMonth = dayOfMonth;
        _month = month;
        _dayOfWeek = dayOfWeek;
    }

    /// <summary>
    /// Checks whether the given time matches this cron schedule.
    /// Comparison uses the minute, hour, day, month, and day-of-week components.
    /// </summary>
    /// <param name="time">The time to check.</param>
    /// <returns>True if the time matches.</returns>
    public bool IsMatch(DateTimeOffset time)
    {
        return _minute.Contains(time.Minute)
            && _hour.Contains(time.Hour)
            && _dayOfMonth.Contains(time.Day)
            && _month.Contains(time.Month)
            && _dayOfWeek.Contains((int)time.DayOfWeek);
    }

    /// <summary>
    /// Computes the next occurrence strictly after the given time.
    /// </summary>
    /// <param name="after">The reference time (exclusive).</param>
    /// <returns>The next matching <see cref="DateTimeOffset"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no occurrence is found within 4 years.</exception>
    public DateTimeOffset NextOccurrence(DateTimeOffset after)
    {
        var candidate = new DateTimeOffset(after.Year, after.Month, after.Day, after.Hour, after.Minute, 0, after.Offset)
            .AddMinutes(1);

        var limit = after.AddYears(4);

        while (candidate <= limit)
        {
            if (!_month.Contains(candidate.Month))
            {
                candidate = AdvanceToNextMonth(candidate);
                continue;
            }

            if (!_dayOfMonth.Contains(candidate.Day) || !_dayOfWeek.Contains((int)candidate.DayOfWeek))
            {
                candidate = candidate.AddDays(1);
                candidate = new DateTimeOffset(candidate.Year, candidate.Month, candidate.Day, 0, 0, 0, candidate.Offset);
                continue;
            }

            if (!_hour.Contains(candidate.Hour))
            {
                candidate = candidate.AddHours(1);
                candidate = new DateTimeOffset(candidate.Year, candidate.Month, candidate.Day, candidate.Hour, 0, 0, candidate.Offset);
                continue;
            }

            if (!_minute.Contains(candidate.Minute))
            {
                candidate = candidate.AddMinutes(1);
                continue;
            }

            return candidate;
        }

        throw new InvalidOperationException($"No next occurrence found within 4 years for expression '{Expression}'.");
    }

    /// <summary>
    /// Computes the previous occurrence strictly before the given time.
    /// </summary>
    /// <param name="before">The reference time (exclusive).</param>
    /// <returns>The previous matching <see cref="DateTimeOffset"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no occurrence is found within 4 years.</exception>
    public DateTimeOffset PreviousOccurrence(DateTimeOffset before)
    {
        var candidate = new DateTimeOffset(before.Year, before.Month, before.Day, before.Hour, before.Minute, 0, before.Offset)
            .AddMinutes(-1);

        var limit = before.AddYears(-4);

        while (candidate >= limit)
        {
            if (!_month.Contains(candidate.Month))
            {
                candidate = RetreatToPreviousMonth(candidate);
                continue;
            }

            if (!_dayOfMonth.Contains(candidate.Day) || !_dayOfWeek.Contains((int)candidate.DayOfWeek))
            {
                candidate = candidate.AddDays(-1);
                candidate = new DateTimeOffset(candidate.Year, candidate.Month, candidate.Day, 23, 59, 0, candidate.Offset);
                continue;
            }

            if (!_hour.Contains(candidate.Hour))
            {
                candidate = candidate.AddHours(-1);
                candidate = new DateTimeOffset(candidate.Year, candidate.Month, candidate.Day, candidate.Hour, 59, 0, candidate.Offset);
                continue;
            }

            if (!_minute.Contains(candidate.Minute))
            {
                candidate = candidate.AddMinutes(-1);
                continue;
            }

            return candidate;
        }

        throw new InvalidOperationException($"No previous occurrence found within 4 years for expression '{Expression}'.");
    }

    /// <summary>
    /// Enumerates all matching occurrences within the specified range (inclusive on both ends).
    /// </summary>
    /// <param name="start">The start of the range (inclusive).</param>
    /// <param name="end">The end of the range (inclusive).</param>
    /// <returns>An enumerable of matching times in chronological order.</returns>
    public IEnumerable<DateTimeOffset> GetOccurrences(DateTimeOffset start, DateTimeOffset end)
    {
        // Adjust start to the minute boundary
        var current = new DateTimeOffset(start.Year, start.Month, start.Day, start.Hour, start.Minute, 0, start.Offset);

        // Check the start itself
        if (IsMatch(current) && current >= start)
        {
            yield return current;
        }

        while (true)
        {
            try
            {
                current = NextOccurrence(current);
            }
            catch (InvalidOperationException)
            {
                yield break;
            }

            if (current > end)
                yield break;

            yield return current;
        }
    }

    private static DateTimeOffset AdvanceToNextMonth(DateTimeOffset dt)
    {
        int year = dt.Year;
        int month = dt.Month + 1;
        if (month > 12)
        {
            month = 1;
            year++;
        }
        return new DateTimeOffset(year, month, 1, 0, 0, 0, dt.Offset);
    }

    private static DateTimeOffset RetreatToPreviousMonth(DateTimeOffset dt)
    {
        int year = dt.Year;
        int month = dt.Month - 1;
        if (month < 1)
        {
            month = 12;
            year--;
        }
        int lastDay = DateTime.DaysInMonth(year, month);
        return new DateTimeOffset(year, month, lastDay, 23, 59, 0, dt.Offset);
    }

    /// <inheritdoc />
    public override string ToString() => Expression;
}
