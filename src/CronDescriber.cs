namespace Philiprehberger.CronExpression;

/// <summary>
/// Generates human-readable descriptions of cron schedules.
/// </summary>
internal static class CronDescriber
{
    private static readonly string[] MonthNames =
    {
        "", "January", "February", "March", "April", "May", "June",
        "July", "August", "September", "October", "November", "December"
    };

    private static readonly string[] DayNames =
    {
        "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"
    };

    /// <summary>
    /// Produces a human-readable description from parsed cron fields.
    /// </summary>
    internal static string Describe(CronField? second, CronField minute, CronField hour, CronField dayOfMonth, CronField month, CronField dayOfWeek)
    {
        var parts = new List<string>();

        // Time part
        var timePart = DescribeTime(second, minute, hour);
        parts.Add(timePart);

        // Day of month constraint
        if (!dayOfMonth.IsWildcard())
        {
            var values = dayOfMonth.GetValues();
            if (values.Count == 1)
            {
                parts.Add($"on day {values[0]}");
            }
            else
            {
                parts.Add($"on days {FormatList(values.Select(v => v.ToString()))}");
            }
        }

        // Day of week constraint
        if (!dayOfWeek.IsWildcard())
        {
            var values = dayOfWeek.GetValues();
            var dayDescriptions = values.Select(v => DayNames[v]).ToList();

            if (IsConsecutiveRange(values))
            {
                parts.Add($"on {DayNames[values[0]]} through {DayNames[values[^1]]}");
            }
            else if (dayDescriptions.Count == 1)
            {
                parts.Add($"on {dayDescriptions[0]}");
            }
            else
            {
                parts.Add($"on {FormatList(dayDescriptions)}");
            }
        }

        // Month constraint
        if (!month.IsWildcard())
        {
            var values = month.GetValues();
            if (values.Count == 1)
            {
                // If day of month is also set, combine for a nicer output
                if (!dayOfMonth.IsWildcard() && dayOfMonth.GetValues().Count == 1)
                {
                    // Remove the "on day N" part and replace with "on Month Nth"
                    var dayVal = dayOfMonth.GetValues()[0];
                    parts.RemoveAll(p => p.StartsWith("on day ", StringComparison.Ordinal));
                    parts.Add($"on {MonthNames[values[0]]} {FormatOrdinal(dayVal)}");
                }
                else
                {
                    parts.Add($"in {MonthNames[values[0]]}");
                }
            }
            else
            {
                var monthDescriptions = values.Select(v => MonthNames[v]);
                parts.Add($"in {FormatList(monthDescriptions)}");
            }
        }

        return string.Join(" ", parts);
    }

    private static string DescribeTime(CronField? second, CronField minute, CronField hour)
    {
        bool minuteWild = minute.IsWildcard();
        bool hourWild = hour.IsWildcard();
        bool secondWild = second?.IsWildcard() ?? true;

        // Handle step patterns in the token
        if (second != null && !secondWild && second.Token.StartsWith("*/", StringComparison.Ordinal))
        {
            var step = second.Token[2..];
            return $"Every {step} seconds";
        }

        if (minuteWild && hourWild)
        {
            if (second != null && !secondWild)
            {
                return DescribeSpecificSeconds(second);
            }
            return "Every minute";
        }

        if (!minuteWild && minute.Token.StartsWith("*/", StringComparison.Ordinal))
        {
            var step = minute.Token[2..];
            return $"Every {step} minutes";
        }

        if (!hourWild && hour.Token.StartsWith("*/", StringComparison.Ordinal))
        {
            var step = hour.Token[2..];
            var minuteValues = minute.GetValues();
            var min = minuteValues.Count == 1 ? minuteValues[0] : 0;
            return $"Every {step} hours at minute {min}";
        }

        if (!minuteWild && hourWild)
        {
            var minuteValues = minute.GetValues();
            if (minuteValues.Count == 1)
                return $"Every hour at minute {minuteValues[0]:D2}";
            return $"Every hour at minutes {FormatList(minuteValues.Select(m => m.ToString("D2")))}";
        }

        if (minuteWild && !hourWild)
        {
            var hourValues = hour.GetValues();
            if (hourValues.Count == 1)
                return $"Every minute of hour {hourValues[0]:D2}";
            return $"Every minute during hours {FormatList(hourValues.Select(h => h.ToString("D2")))}";
        }

        // Specific time
        var mins = minute.GetValues();
        var hours = hour.GetValues();

        if (mins.Count == 1 && hours.Count == 1)
        {
            var secondPart = "";
            if (second != null && !secondWild)
            {
                var secs = second.GetValues();
                if (secs.Count == 1)
                    secondPart = $":{secs[0]:D2}";
            }
            return $"At {hours[0]:D2}:{mins[0]:D2}{secondPart}";
        }

        if (hours.Count == 1)
        {
            return $"At {FormatList(mins.Select(m => $"{hours[0]:D2}:{m:D2}"))}";
        }

        if (mins.Count == 1)
        {
            return $"At minute {mins[0]:D2} of hours {FormatList(hours.Select(h => h.ToString("D2")))}";
        }

        return $"At minutes {FormatList(mins.Select(m => m.ToString()))} of hours {FormatList(hours.Select(h => h.ToString()))}";
    }

    private static string DescribeSpecificSeconds(CronField second)
    {
        var values = second.GetValues();
        if (values.Count == 1)
            return $"At second {values[0]} of every minute";
        return $"At seconds {FormatList(values.Select(v => v.ToString()))} of every minute";
    }

    private static bool IsConsecutiveRange(IReadOnlyList<int> values)
    {
        if (values.Count < 2) return false;
        for (int i = 1; i < values.Count; i++)
        {
            if (values[i] != values[i - 1] + 1) return false;
        }
        return true;
    }

    private static string FormatOrdinal(int number)
    {
        var suffix = (number % 100) switch
        {
            11 or 12 or 13 => "th",
            _ => (number % 10) switch
            {
                1 => "st",
                2 => "nd",
                3 => "rd",
                _ => "th"
            }
        };
        return $"{number}{suffix}";
    }

    private static string FormatList(IEnumerable<string> items)
    {
        var list = items.ToList();
        return list.Count switch
        {
            0 => "",
            1 => list[0],
            2 => $"{list[0]} and {list[1]}",
            _ => $"{string.Join(", ", list[..^1])} and {list[^1]}"
        };
    }
}
