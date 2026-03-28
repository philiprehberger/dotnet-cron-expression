namespace Philiprehberger.CronExpression;

/// <summary>
/// Represents a single parsed field of a cron expression.
/// Handles wildcards, specific values, ranges, steps, lists, and name aliases.
/// </summary>
internal sealed class CronField
{
    private static readonly Dictionary<string, int> DayNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["SUN"] = 0, ["MON"] = 1, ["TUE"] = 2, ["WED"] = 3,
        ["THU"] = 4, ["FRI"] = 5, ["SAT"] = 6
    };

    private static readonly Dictionary<string, int> MonthNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["JAN"] = 1, ["FEB"] = 2, ["MAR"] = 3, ["APR"] = 4,
        ["MAY"] = 5, ["JUN"] = 6, ["JUL"] = 7, ["AUG"] = 8,
        ["SEP"] = 9, ["OCT"] = 10, ["NOV"] = 11, ["DEC"] = 12
    };

    private readonly HashSet<int> _values;

    /// <summary>
    /// Gets the minimum allowed value for this field.
    /// </summary>
    public int Min { get; }

    /// <summary>
    /// Gets the maximum allowed value for this field.
    /// </summary>
    public int Max { get; }

    /// <summary>
    /// Gets the original token string for this field.
    /// </summary>
    public string Token { get; }

    private CronField(HashSet<int> values, int min, int max, string token)
    {
        _values = values;
        Min = min;
        Max = max;
        Token = token;
    }

    /// <summary>
    /// Checks whether the given value matches this field.
    /// </summary>
    /// <param name="value">The value to test.</param>
    /// <returns>True if the value is contained in the set of matching values.</returns>
    public bool Contains(int value) => _values.Contains(value);

    /// <summary>
    /// Gets all matching values in ascending order.
    /// </summary>
    /// <returns>Sorted matching values.</returns>
    public IReadOnlyList<int> GetValues() => _values.Order().ToList();

    /// <summary>
    /// Returns true if this field matches all values in its range (i.e. is a wildcard).
    /// </summary>
    public bool IsWildcard()
    {
        for (int i = Min; i <= Max; i++)
        {
            if (!_values.Contains(i)) return false;
        }
        return true;
    }

    /// <summary>
    /// Parses a cron field token.
    /// </summary>
    /// <param name="token">The field token string (e.g. "*/5", "1-5", "MON,WED,FRI").</param>
    /// <param name="min">Minimum allowed value for this field.</param>
    /// <param name="max">Maximum allowed value for this field.</param>
    /// <param name="fieldType">The type of field for name resolution.</param>
    /// <param name="expression">The full expression string for error messages.</param>
    /// <param name="fieldIndex">The field index for error messages.</param>
    /// <returns>A parsed <see cref="CronField"/>.</returns>
    public static CronField Parse(string token, int min, int max, FieldType fieldType, string expression, int fieldIndex)
    {
        var values = new HashSet<int>();

        foreach (var part in token.Split(','))
        {
            ParsePart(part.Trim(), min, max, fieldType, expression, fieldIndex, values);
        }

        if (values.Count == 0)
        {
            throw new CronParseException($"Field {fieldIndex} produced no valid values from '{token}'.", expression, fieldIndex);
        }

        return new CronField(values, min, max, token);
    }

    private static void ParsePart(string part, int min, int max, FieldType fieldType, string expression, int fieldIndex, HashSet<int> values)
    {
        if (part == "*")
        {
            for (int i = min; i <= max; i++)
                values.Add(i);
            return;
        }

        // Handle step: */N or range/N
        if (part.Contains('/'))
        {
            var stepParts = part.Split('/', 2);
            int step = ParseInt(stepParts[1], expression, fieldIndex);
            if (step <= 0)
                throw new CronParseException($"Step value must be positive in '{part}'.", expression, fieldIndex);

            int rangeMin, rangeMax;
            if (stepParts[0] == "*")
            {
                rangeMin = min;
                rangeMax = max;
            }
            else if (stepParts[0].Contains('-'))
            {
                (rangeMin, rangeMax) = ParseRange(stepParts[0], min, max, fieldType, expression, fieldIndex);
            }
            else
            {
                rangeMin = ResolveValue(stepParts[0], fieldType, expression, fieldIndex);
                rangeMax = max;
            }

            for (int i = rangeMin; i <= rangeMax; i += step)
                values.Add(i);
            return;
        }

        // Handle range: N-M
        if (part.Contains('-'))
        {
            var (rMin, rMax) = ParseRange(part, min, max, fieldType, expression, fieldIndex);
            for (int i = rMin; i <= rMax; i++)
                values.Add(i);
            return;
        }

        // Single value
        int val = ResolveValue(part, fieldType, expression, fieldIndex);
        if (val < min || val > max)
            throw new CronParseException($"Value {val} is out of range [{min}-{max}] in field {fieldIndex}.", expression, fieldIndex);
        values.Add(val);
    }

    private static (int Min, int Max) ParseRange(string part, int min, int max, FieldType fieldType, string expression, int fieldIndex)
    {
        var rangeParts = part.Split('-', 2);
        int rMin = ResolveValue(rangeParts[0], fieldType, expression, fieldIndex);
        int rMax = ResolveValue(rangeParts[1], fieldType, expression, fieldIndex);

        if (rMin < min || rMax > max || rMin > rMax)
            throw new CronParseException($"Invalid range '{part}' in field {fieldIndex}.", expression, fieldIndex);

        return (rMin, rMax);
    }

    private static int ResolveValue(string token, FieldType fieldType, string expression, int fieldIndex)
    {
        if (fieldType == FieldType.DayOfWeek && DayNames.TryGetValue(token, out int dayVal))
            return dayVal;

        if (fieldType == FieldType.Month && MonthNames.TryGetValue(token, out int monthVal))
            return monthVal;

        return ParseInt(token, expression, fieldIndex);
    }

    private static int ParseInt(string token, string expression, int fieldIndex)
    {
        if (!int.TryParse(token, out int result))
            throw new CronParseException($"Invalid value '{token}' in field {fieldIndex}.", expression, fieldIndex);
        return result;
    }
}

/// <summary>
/// Identifies the type of a cron field for name alias resolution.
/// </summary>
internal enum FieldType
{
    /// <summary>Second field (0-59).</summary>
    Second,
    /// <summary>Minute field (0-59).</summary>
    Minute,
    /// <summary>Hour field (0-23).</summary>
    Hour,
    /// <summary>Day of month field (1-31).</summary>
    DayOfMonth,
    /// <summary>Month field (1-12).</summary>
    Month,
    /// <summary>Day of week field (0-6, Sunday=0).</summary>
    DayOfWeek
}
