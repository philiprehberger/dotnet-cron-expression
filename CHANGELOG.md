# Changelog

## 0.3.0 (2026-03-31)

- Add timezone-aware scheduling with `TimeZoneInfo` parameter on `NextOccurrence` and `GetOccurrences`
- Add `ExclusionCalendar` class for blackout date filtering on occurrence calculations
- Add Nth weekday support in day-of-week field (`#N` patterns, e.g. `5#3` for third Friday)
- Handle DST spring-forward and fall-back transitions correctly in timezone mode

## 0.2.1 (2026-03-31)

- Standardize README to 3-badge format with emoji Support section
- Update CI actions to v5 for Node.js 24 compatibility

## 0.2.0 (2026-03-27)

- Add optional seconds field support for 6-field cron expressions
- Add shortcut aliases (@yearly, @monthly, @weekly, @daily, @hourly)
- Add fluent CronBuilder API for programmatic expression construction
- Add Describe() method for human-readable cron descriptions

## 0.1.2 (2026-03-26)

- Add Sponsor badge and fix License link format in README

## 0.1.1 (2026-03-23)

- Fix NuGet badge URL format

## 0.1.0 (2026-03-22)

- Initial release
- Parse and validate standard 5-field cron expressions
- Support wildcards, ranges, steps, lists, day names, and month names
- Calculate next and previous occurrences from any point in time
- List all occurrences within a date range
- Static convenience methods for quick matching
