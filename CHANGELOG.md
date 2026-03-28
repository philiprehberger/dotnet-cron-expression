# Changelog

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
