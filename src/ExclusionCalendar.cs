namespace Philiprehberger.CronExpression;

/// <summary>
/// Represents a set of blackout dates that should be skipped when computing cron occurrences.
/// </summary>
public sealed class ExclusionCalendar
{
    private readonly HashSet<DateOnly> _dates;

    /// <summary>
    /// Initializes a new empty <see cref="ExclusionCalendar"/>.
    /// </summary>
    public ExclusionCalendar()
    {
        _dates = new HashSet<DateOnly>();
    }

    /// <summary>
    /// Initializes a new <see cref="ExclusionCalendar"/> with the specified blackout dates.
    /// </summary>
    /// <param name="dates">The dates to exclude.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dates"/> is null.</exception>
    public ExclusionCalendar(IEnumerable<DateOnly> dates)
    {
        ArgumentNullException.ThrowIfNull(dates);
        _dates = new HashSet<DateOnly>(dates);
    }

    /// <summary>
    /// Gets the number of excluded dates.
    /// </summary>
    public int Count => _dates.Count;

    /// <summary>
    /// Adds a date to the exclusion set.
    /// </summary>
    /// <param name="date">The date to exclude.</param>
    /// <returns>True if the date was added; false if it was already present.</returns>
    public bool Add(DateOnly date) => _dates.Add(date);

    /// <summary>
    /// Adds multiple dates to the exclusion set.
    /// </summary>
    /// <param name="dates">The dates to exclude.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dates"/> is null.</exception>
    public void AddRange(IEnumerable<DateOnly> dates)
    {
        ArgumentNullException.ThrowIfNull(dates);
        foreach (var date in dates)
        {
            _dates.Add(date);
        }
    }

    /// <summary>
    /// Removes a date from the exclusion set.
    /// </summary>
    /// <param name="date">The date to remove.</param>
    /// <returns>True if the date was removed; false if it was not present.</returns>
    public bool Remove(DateOnly date) => _dates.Remove(date);

    /// <summary>
    /// Checks whether the specified date is excluded.
    /// </summary>
    /// <param name="date">The date to check.</param>
    /// <returns>True if the date is in the exclusion set.</returns>
    public bool IsExcluded(DateOnly date) => _dates.Contains(date);

    /// <summary>
    /// Checks whether the specified <see cref="DateTimeOffset"/> falls on an excluded date.
    /// </summary>
    /// <param name="time">The time to check.</param>
    /// <returns>True if the date portion is in the exclusion set.</returns>
    public bool IsExcluded(DateTimeOffset time) => _dates.Contains(DateOnly.FromDateTime(time.DateTime));

    /// <summary>
    /// Removes all dates from the exclusion set.
    /// </summary>
    public void Clear() => _dates.Clear();

    /// <summary>
    /// Gets all excluded dates in ascending order.
    /// </summary>
    /// <returns>A sorted list of excluded dates.</returns>
    public IReadOnlyList<DateOnly> GetDates() => _dates.Order().ToList();
}
