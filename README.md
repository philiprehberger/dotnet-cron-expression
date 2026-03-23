# Philiprehberger.CronExpression

[![CI](https://github.com/philiprehberger/dotnet-cron-expression/actions/workflows/ci.yml/badge.svg)](https://github.com/philiprehberger/dotnet-cron-expression/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Philiprehberger.CronExpression)](https://www.nuget.org/packages/Philiprehberger.CronExpression)
[![License](https://img.shields.io/github/license/philiprehberger/dotnet-cron-expression)](LICENSE)

Parse, validate, and evaluate cron expressions with next/previous occurrence calculation and schedule listing.

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

| Method | Description |
|--------|-------------|
| `Cron.Parse(string)` | Parse a cron expression, throws on invalid input |
| `Cron.TryParse(string, out CronSchedule)` | Try to parse a cron expression without throwing |
| `Cron.IsMatch(string, DateTimeOffset)` | Check if a time matches a cron expression |
| `CronSchedule.NextOccurrence(DateTimeOffset)` | Get the next matching time after the given time |
| `CronSchedule.PreviousOccurrence(DateTimeOffset)` | Get the previous matching time before the given time |
| `CronSchedule.GetOccurrences(DateTimeOffset, DateTimeOffset)` | List all matching times in a range |

## Development

```bash
dotnet build src/Philiprehberger.CronExpression.csproj --configuration Release
```

## License

MIT
