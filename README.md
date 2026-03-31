# Philiprehberger.CronExpression

[![CI](https://github.com/philiprehberger/dotnet-cron-expression/actions/workflows/ci.yml/badge.svg)](https://github.com/philiprehberger/dotnet-cron-expression/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Philiprehberger.CronExpression.svg)](https://www.nuget.org/packages/Philiprehberger.CronExpression)
[![Last updated](https://img.shields.io/github/last-commit/philiprehberger/dotnet-cron-expression)](https://github.com/philiprehberger/dotnet-cron-expression/commits/main)

Parse, validate, and evaluate cron expressions with next/previous occurrence calculation, shortcut aliases, fluent builder, and human-readable descriptions.

## Installation

```bash
dotnet add package Philiprehberger.CronExpression
```

## Usage

```csharp
using Philiprehberger.CronExpression;

// Parse a cron expression
var schedule = Cron.Parse("*/15 9-17 * * MON-FRI");

// Get the next occurrence
DateTimeOffset next = schedule.NextOccurrence(DateTimeOffset.UtcNow);

// Get the previous occurrence
DateTimeOffset prev = schedule.PreviousOccurrence(DateTimeOffset.UtcNow);
```

### Shortcut Aliases

```csharp
var daily = Cron.Parse("@daily");       // 0 0 * * *
var hourly = Cron.Parse("@hourly");     // 0 * * * *
var weekly = Cron.Parse("@weekly");     // 0 0 * * 0
var monthly = Cron.Parse("@monthly");   // 0 0 1 * *
var yearly = Cron.Parse("@yearly");     // 0 0 1 1 *
```

### Seconds Field

```csharp
// 6-field format: second minute hour day-of-month month day-of-week
var schedule = Cron.Parse("*/30 * * * * *"); // Every 30 seconds
var next = schedule.NextOccurrence(DateTimeOffset.UtcNow);
```

### Fluent Builder

```csharp
// Build expressions programmatically
var schedule = new CronBuilder()
    .At(9, 30)
    .OnDaysOfWeek(DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday)
    .Build();

// Step expressions
var every5Min = new CronBuilder().Every(5).Minutes().Build();
var every2Hr = new CronBuilder().Every(2).Hours().Build();
```

### Human-Readable Descriptions

```csharp
var schedule = Cron.Parse("*/5 * * * *");
Console.WriteLine(schedule.Describe()); // "Every 5 minutes"

var weekday = Cron.Parse("0 9 * * 1-5");
Console.WriteLine(weekday.Describe()); // "At 09:00 on Monday through Friday"

var yearly = Cron.Parse("0 0 1 1 *");
Console.WriteLine(yearly.Describe()); // "At 00:00 on January 1st"
```

### Quick Matching

```csharp
// Check if a time matches a cron expression
bool matches = Cron.IsMatch("0 9 * * *", DateTimeOffset.UtcNow);
```

### Listing Occurrences

```csharp
var schedule = Cron.Parse("0 0 1 * *"); // First of every month

var start = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
var end = new DateTimeOffset(2026, 12, 31, 23, 59, 59, TimeSpan.Zero);

foreach (var occurrence in schedule.GetOccurrences(start, end))
{
    Console.WriteLine(occurrence);
}
```

### Safe Parsing

```csharp
if (Cron.TryParse("*/5 * * * *", out var schedule))
{
    Console.WriteLine(schedule.NextOccurrence(DateTimeOffset.UtcNow));
}
```

## API

### `Cron`

| Method | Description |
|--------|-------------|
| `Parse(string)` | Parse a cron expression (5-field, 6-field, or alias) |
| `TryParse(string, out CronSchedule)` | Try to parse without throwing |
| `IsMatch(string, DateTimeOffset)` | Check if a time matches an expression |

### `CronSchedule`

| Method | Description |
|--------|-------------|
| `IsMatch(DateTimeOffset)` | Check if a time matches this schedule |
| `NextOccurrence(DateTimeOffset)` | Get the next matching time after the given time |
| `PreviousOccurrence(DateTimeOffset)` | Get the previous matching time before the given time |
| `GetOccurrences(DateTimeOffset, DateTimeOffset)` | List all matching times in a range |
| `Describe()` | Get a human-readable description of the schedule |
| `HasSeconds` | Whether the schedule uses 6-field format with seconds |

### `CronBuilder`

| Method | Description |
|--------|-------------|
| `EveryMinute()` | Set minute field to wildcard |
| `EveryHour()` | Set minute to 0 |
| `AtMinute(int)` | Set a specific minute |
| `AtHour(int)` | Set a specific hour |
| `At(int, int)` | Set hour and minute |
| `OnDaysOfWeek(params DayOfWeek[])` | Set specific days of week |
| `OnDaysOfMonth(params int[])` | Set specific days of month |
| `OnMonths(params int[])` | Set specific months |
| `Every(int).Minutes()` | Set step interval on minutes |
| `Every(int).Hours()` | Set step interval on hours |
| `Build()` | Build and return a `CronSchedule` |

### Supported Aliases

| Alias | Equivalent |
|-------|-----------|
| `@yearly` / `@annually` | `0 0 1 1 *` |
| `@monthly` | `0 0 1 * *` |
| `@weekly` | `0 0 * * 0` |
| `@daily` / `@midnight` | `0 0 * * *` |
| `@hourly` | `0 * * * *` |

## Development

```bash
dotnet build src/Philiprehberger.CronExpression.csproj --configuration Release
```

## Support

If you find this project useful:

⭐ [Star the repo](https://github.com/philiprehberger/dotnet-cron-expression)

🐛 [Report issues](https://github.com/philiprehberger/dotnet-cron-expression/issues?q=is%3Aissue+is%3Aopen+label%3Abug)

💡 [Suggest features](https://github.com/philiprehberger/dotnet-cron-expression/issues?q=is%3Aissue+is%3Aopen+label%3Aenhancement)

❤️ [Sponsor development](https://github.com/sponsors/philiprehberger)

🌐 [All Open Source Projects](https://philiprehberger.com/open-source-packages)

💻 [GitHub Profile](https://github.com/philiprehberger)

🔗 [LinkedIn Profile](https://www.linkedin.com/in/philiprehberger)

## License

[MIT](LICENSE)
