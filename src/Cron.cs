namespace Philiprehberger.CronExpression;

/// <summary>
/// Provides static methods for parsing and evaluating standard 5-field cron expressions.
/// Supported format: minute hour day-of-month month day-of-week.
/// </summary>
public static class Cron
{
    /// <summary>
    /// Parses a standard 5-field cron expression.
    /// </summary>
    /// <param name="expression">The cron expression string (e.g. "*/5 * * * *").</param>
    /// <returns>A <see cref="CronSchedule"/> representing the parsed schedule.</returns>
    /// <exception cref="CronParseException">Thrown when the expression is invalid.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="expression"/> is null.</exception>
    public static CronSchedule Parse(string expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        var trimmed = expression.Trim();
        var fields = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (fields.Length != 5)
        {
            throw new CronParseException(
                $"Expected 5 fields but got {fields.Length}. Format: minute hour day-of-month month day-of-week.",
                expression);
        }

        var minute = CronField.Parse(fields[0], 0, 59, FieldType.Minute, expression, 0);
        var hour = CronField.Parse(fields[1], 0, 23, FieldType.Hour, expression, 1);
        var dayOfMonth = CronField.Parse(fields[2], 1, 31, FieldType.DayOfMonth, expression, 2);
        var month = CronField.Parse(fields[3], 1, 12, FieldType.Month, expression, 3);
        var dayOfWeek = CronField.Parse(fields[4], 0, 6, FieldType.DayOfWeek, expression, 4);

        return new CronSchedule(trimmed, minute, hour, dayOfMonth, month, dayOfWeek);
    }

    /// <summary>
    /// Attempts to parse a cron expression without throwing.
    /// </summary>
    /// <param name="expression">The cron expression string.</param>
    /// <param name="schedule">When this method returns true, contains the parsed schedule.</param>
    /// <returns>True if parsing succeeded; otherwise false.</returns>
    public static bool TryParse(string? expression, out CronSchedule? schedule)
    {
        schedule = null;

        if (string.IsNullOrWhiteSpace(expression))
            return false;

        try
        {
            schedule = Parse(expression);
            return true;
        }
        catch (CronParseException)
        {
            return false;
        }
    }

    /// <summary>
    /// Checks whether the given time matches a cron expression.
    /// </summary>
    /// <param name="expression">The cron expression string.</param>
    /// <param name="time">The time to check.</param>
    /// <returns>True if the time matches the schedule.</returns>
    /// <exception cref="CronParseException">Thrown when the expression is invalid.</exception>
    public static bool IsMatch(string expression, DateTimeOffset time)
    {
        var schedule = Parse(expression);
        return schedule.IsMatch(time);
    }
}
