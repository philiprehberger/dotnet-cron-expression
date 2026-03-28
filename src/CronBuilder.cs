namespace Philiprehberger.CronExpression;

/// <summary>
/// Fluent builder for constructing cron expressions programmatically.
/// </summary>
public sealed class CronBuilder
{
    private string _minute = "*";
    private string _hour = "*";
    private string _dayOfMonth = "*";
    private string _month = "*";
    private string _dayOfWeek = "*";

    /// <summary>
    /// Creates a new <see cref="CronBuilder"/> with all fields set to wildcard (*).
    /// </summary>
    public CronBuilder() { }

    /// <summary>
    /// Sets the schedule to run every minute (the default).
    /// </summary>
    /// <returns>This builder instance for chaining.</returns>
    public CronBuilder EveryMinute()
    {
        _minute = "*";
        return this;
    }

    /// <summary>
    /// Sets the schedule to run every hour at minute 0.
    /// </summary>
    /// <returns>This builder instance for chaining.</returns>
    public CronBuilder EveryHour()
    {
        _minute = "0";
        return this;
    }

    /// <summary>
    /// Sets the schedule to run at a specific minute of each hour.
    /// </summary>
    /// <param name="minute">The minute (0-59).</param>
    /// <returns>This builder instance for chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="minute"/> is out of range.</exception>
    public CronBuilder AtMinute(int minute)
    {
        if (minute < 0 || minute > 59)
            throw new ArgumentOutOfRangeException(nameof(minute), minute, "Minute must be between 0 and 59.");
        _minute = minute.ToString();
        return this;
    }

    /// <summary>
    /// Sets the schedule to run at a specific hour.
    /// </summary>
    /// <param name="hour">The hour (0-23).</param>
    /// <returns>This builder instance for chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="hour"/> is out of range.</exception>
    public CronBuilder AtHour(int hour)
    {
        if (hour < 0 || hour > 23)
            throw new ArgumentOutOfRangeException(nameof(hour), hour, "Hour must be between 0 and 23.");
        _hour = hour.ToString();
        return this;
    }

    /// <summary>
    /// Sets the schedule to run at a specific time (hour and minute).
    /// </summary>
    /// <param name="hour">The hour (0-23).</param>
    /// <param name="minute">The minute (0-59).</param>
    /// <returns>This builder instance for chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when parameters are out of range.</exception>
    public CronBuilder At(int hour, int minute)
    {
        AtHour(hour);
        AtMinute(minute);
        return this;
    }

    /// <summary>
    /// Sets the schedule to run on specific days of the week.
    /// </summary>
    /// <param name="days">The days of the week.</param>
    /// <returns>This builder instance for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when no days are specified.</exception>
    public CronBuilder OnDaysOfWeek(params DayOfWeek[] days)
    {
        if (days.Length == 0)
            throw new ArgumentException("At least one day must be specified.", nameof(days));

        _dayOfWeek = string.Join(",", days.Select(d => (int)d).OrderBy(d => d));
        return this;
    }

    /// <summary>
    /// Sets the schedule to run on specific days of the month.
    /// </summary>
    /// <param name="days">The days of the month (1-31).</param>
    /// <returns>This builder instance for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when no days are specified.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a day is out of range.</exception>
    public CronBuilder OnDaysOfMonth(params int[] days)
    {
        if (days.Length == 0)
            throw new ArgumentException("At least one day must be specified.", nameof(days));

        foreach (var day in days)
        {
            if (day < 1 || day > 31)
                throw new ArgumentOutOfRangeException(nameof(days), day, "Day of month must be between 1 and 31.");
        }

        _dayOfMonth = string.Join(",", days.OrderBy(d => d));
        return this;
    }

    /// <summary>
    /// Sets the schedule to run on specific months.
    /// </summary>
    /// <param name="months">The months (1-12).</param>
    /// <returns>This builder instance for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when no months are specified.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a month is out of range.</exception>
    public CronBuilder OnMonths(params int[] months)
    {
        if (months.Length == 0)
            throw new ArgumentException("At least one month must be specified.", nameof(months));

        foreach (var month in months)
        {
            if (month < 1 || month > 12)
                throw new ArgumentOutOfRangeException(nameof(months), month, "Month must be between 1 and 12.");
        }

        _month = string.Join(",", months.OrderBy(m => m));
        return this;
    }

    /// <summary>
    /// Creates a step builder for constructing "every N" expressions.
    /// </summary>
    /// <param name="interval">The step interval.</param>
    /// <returns>A <see cref="CronStepBuilder"/> for specifying the field.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="interval"/> is less than 1.</exception>
    public CronStepBuilder Every(int interval)
    {
        if (interval < 1)
            throw new ArgumentOutOfRangeException(nameof(interval), interval, "Interval must be at least 1.");
        return new CronStepBuilder(this, interval);
    }

    /// <summary>
    /// Builds and parses the constructed cron expression.
    /// </summary>
    /// <returns>A <see cref="CronSchedule"/> representing the built schedule.</returns>
    public CronSchedule Build()
    {
        var expression = $"{_minute} {_hour} {_dayOfMonth} {_month} {_dayOfWeek}";
        return Cron.Parse(expression);
    }

    /// <summary>
    /// Returns the cron expression string that would be built.
    /// </summary>
    /// <returns>The cron expression string.</returns>
    public override string ToString()
    {
        return $"{_minute} {_hour} {_dayOfMonth} {_month} {_dayOfWeek}";
    }

    internal void SetMinute(string value) => _minute = value;
    internal void SetHour(string value) => _hour = value;
}

/// <summary>
/// Intermediate builder for "every N minutes/hours" step expressions.
/// </summary>
public sealed class CronStepBuilder
{
    private readonly CronBuilder _builder;
    private readonly int _interval;

    internal CronStepBuilder(CronBuilder builder, int interval)
    {
        _builder = builder;
        _interval = interval;
    }

    /// <summary>
    /// Applies the step interval to the minutes field (e.g. Every(5).Minutes() = "*/5").
    /// </summary>
    /// <returns>The parent <see cref="CronBuilder"/> for continued chaining.</returns>
    public CronBuilder Minutes()
    {
        _builder.SetMinute($"*/{_interval}");
        return _builder;
    }

    /// <summary>
    /// Applies the step interval to the hours field (e.g. Every(2).Hours() = "0 */2").
    /// Sets minute to 0 if it is currently wildcard.
    /// </summary>
    /// <returns>The parent <see cref="CronBuilder"/> for continued chaining.</returns>
    public CronBuilder Hours()
    {
        _builder.SetHour($"*/{_interval}");
        _builder.SetMinute("0");
        return _builder;
    }
}
