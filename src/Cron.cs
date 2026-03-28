namespace Philiprehberger.CronExpression;

/// <summary>
/// Provides static methods for parsing and evaluating cron expressions.
/// Supports standard 5-field format (minute hour day-of-month month day-of-week),
/// optional 6-field format with seconds (second minute hour day-of-month month day-of-week),
/// and shortcut aliases (@yearly, @monthly, @weekly, @daily, @midnight, @hourly).
/// </summary>
public static class Cron
{
    private static readonly Dictionary<string, string> Aliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["@yearly"] = "0 0 1 1 *",
        ["@annually"] = "0 0 1 1 *",
        ["@monthly"] = "0 0 1 * *",
        ["@weekly"] = "0 0 * * 0",
        ["@daily"] = "0 0 * * *",
        ["@midnight"] = "0 0 * * *",
        ["@hourly"] = "0 * * * *"
    };

    /// <summary>
    /// Parses a cron expression. Accepts 5-field (standard), 6-field (with seconds),
    /// or shortcut aliases (@yearly, @monthly, @weekly, @daily, @midnight, @hourly).
    /// </summary>
    /// <param name="expression">The cron expression string (e.g. "*/5 * * * *" or "@daily").</param>
    /// <returns>A <see cref="CronSchedule"/> representing the parsed schedule.</returns>
    /// <exception cref="CronParseException">Thrown when the expression is invalid.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="expression"/> is null.</exception>
    public static CronSchedule Parse(string expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        var trimmed = expression.Trim();

        // Handle shortcut aliases
        if (Aliases.TryGetValue(trimmed, out var expanded))
        {
            return ParseFields(expanded, trimmed);
        }

        return ParseFields(trimmed, trimmed);
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

    private static CronSchedule ParseFields(string fieldExpression, string originalExpression)
    {
        var fields = fieldExpression.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (fields.Length == 5)
        {
            var minute = CronField.Parse(fields[0], 0, 59, FieldType.Minute, originalExpression, 0);
            var hour = CronField.Parse(fields[1], 0, 23, FieldType.Hour, originalExpression, 1);
            var dayOfMonth = CronField.Parse(fields[2], 1, 31, FieldType.DayOfMonth, originalExpression, 2);
            var month = CronField.Parse(fields[3], 1, 12, FieldType.Month, originalExpression, 3);
            var dayOfWeek = CronField.Parse(fields[4], 0, 6, FieldType.DayOfWeek, originalExpression, 4);

            return new CronSchedule(originalExpression, minute, hour, dayOfMonth, month, dayOfWeek);
        }

        if (fields.Length == 6)
        {
            var second = CronField.Parse(fields[0], 0, 59, FieldType.Second, originalExpression, 0);
            var minute = CronField.Parse(fields[1], 0, 59, FieldType.Minute, originalExpression, 1);
            var hour = CronField.Parse(fields[2], 0, 23, FieldType.Hour, originalExpression, 2);
            var dayOfMonth = CronField.Parse(fields[3], 1, 31, FieldType.DayOfMonth, originalExpression, 3);
            var month = CronField.Parse(fields[4], 1, 12, FieldType.Month, originalExpression, 4);
            var dayOfWeek = CronField.Parse(fields[5], 0, 6, FieldType.DayOfWeek, originalExpression, 5);

            return new CronSchedule(originalExpression, second, minute, hour, dayOfMonth, month, dayOfWeek);
        }

        throw new CronParseException(
            $"Expected 5 or 6 fields but got {fields.Length}. Format: minute hour day-of-month month day-of-week (or second minute hour day-of-month month day-of-week).",
            originalExpression);
    }
}
