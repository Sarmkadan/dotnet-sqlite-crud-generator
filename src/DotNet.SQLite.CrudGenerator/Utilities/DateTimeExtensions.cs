// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNet.SQLite.CrudGenerator.Utilities;

/// <summary>
/// Extension methods for DateTime operations.
/// Provides utilities for date/time calculations, formatting, and comparisons.
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Checks if a datetime is in the past.
    /// </summary>
    public static bool IsInPast(this DateTime dateTime)
    {
        return dateTime < DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if a datetime is in the future.
    /// </summary>
    public static bool IsInFuture(this DateTime dateTime)
    {
        return dateTime > DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the beginning of the day (midnight).
    /// </summary>
    public static DateTime BeginningOfDay(this DateTime dateTime)
    {
        return dateTime.Date;
    }

    /// <summary>
    /// Gets the end of the day (23:59:59.999).
    /// </summary>
    public static DateTime EndOfDay(this DateTime dateTime)
    {
        return dateTime.Date.AddDays(1).AddMilliseconds(-1);
    }

    /// <summary>
    /// Gets the first day of the month for the given date.
    /// </summary>
    public static DateTime FirstDayOfMonth(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, 1);
    }

    /// <summary>
    /// Gets the last day of the month for the given date.
    /// </summary>
    public static DateTime LastDayOfMonth(this DateTime dateTime)
    {
        return dateTime.FirstDayOfMonth().AddMonths(1).AddDays(-1);
    }

    /// <summary>
    /// Gets the first day of the quarter for the given date.
    /// </summary>
    public static DateTime FirstDayOfQuarter(this DateTime dateTime)
    {
        var month = ((dateTime.Month - 1) / 3) * 3 + 1;
        return new DateTime(dateTime.Year, month, 1);
    }

    /// <summary>
    /// Gets the last day of the quarter for the given date.
    /// </summary>
    public static DateTime LastDayOfQuarter(this DateTime dateTime)
    {
        return dateTime.FirstDayOfQuarter().AddMonths(3).AddDays(-1);
    }

    /// <summary>
    /// Gets the first day of the year for the given date.
    /// </summary>
    public static DateTime FirstDayOfYear(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, 1, 1);
    }

    /// <summary>
    /// Gets the last day of the year for the given date.
    /// </summary>
    public static DateTime LastDayOfYear(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, 12, 31);
    }

    /// <summary>
    /// Gets the next occurrence of a specific day of the week.
    /// </summary>
    public static DateTime NextOccurrenceOf(this DateTime dateTime, DayOfWeek dayOfWeek)
    {
        var daysUntilTarget = ((int)dayOfWeek - (int)dateTime.DayOfWeek + 7) % 7;
        if (daysUntilTarget == 0)
            daysUntilTarget = 7;

        return dateTime.AddDays(daysUntilTarget);
    }

    /// <summary>
    /// Gets the previous occurrence of a specific day of the week.
    /// </summary>
    public static DateTime PreviousOccurrenceOf(this DateTime dateTime, DayOfWeek dayOfWeek)
    {
        var daysUntilTarget = ((int)dateTime.DayOfWeek - (int)dayOfWeek + 7) % 7;
        if (daysUntilTarget == 0)
            daysUntilTarget = 7;

        return dateTime.AddDays(-daysUntilTarget);
    }

    /// <summary>
    /// Formats a datetime using a specific format string with fallback.
    /// </summary>
    public static string Format(this DateTime dateTime, string format, string fallback = "N/A")
    {
        try
        {
            return dateTime.ToString(format);
        }
        catch
        {
            return fallback;
        }
    }

    /// <summary>
    /// Gets a human-readable relative time string (e.g., "2 hours ago").
    /// </summary>
    public static string ToRelativeTime(this DateTime dateTime)
    {
        var now = DateTime.UtcNow;
        var diff = now - dateTime;

        return diff.TotalSeconds switch
        {
            < 60 => "just now",
            < 3600 => $"{(int)diff.TotalMinutes} minutes ago",
            < 86400 => $"{(int)diff.TotalHours} hours ago",
            < 604800 => $"{(int)diff.TotalDays} days ago",
            < 2592000 => $"{(int)(diff.TotalDays / 7)} weeks ago",
            < 31536000 => $"{(int)(diff.TotalDays / 30)} months ago",
            _ => $"{(int)(diff.TotalDays / 365)} years ago"
        };
    }

    /// <summary>
    /// Checks if two datetimes are on the same day (ignoring time component).
    /// </summary>
    public static bool IsSameDay(this DateTime dateTime, DateTime other)
    {
        return dateTime.Date == other.Date;
    }

    /// <summary>
    /// Checks if a datetime is a weekend (Saturday or Sunday).
    /// </summary>
    public static bool IsWeekend(this DateTime dateTime)
    {
        return dateTime.DayOfWeek == DayOfWeek.Saturday || dateTime.DayOfWeek == DayOfWeek.Sunday;
    }

    /// <summary>
    /// Checks if a datetime is a weekday (Monday-Friday).
    /// </summary>
    public static bool IsWeekday(this DateTime dateTime)
    {
        return !dateTime.IsWeekend();
    }

    /// <summary>
    /// Gets the age in years from a birth date to a given date.
    /// </summary>
    public static int GetAge(this DateTime birthDate, DateTime? asOfDate = null)
    {
        var today = asOfDate ?? DateTime.UtcNow;
        var age = today.Year - birthDate.Year;

        if (birthDate.Date > today.AddYears(-age))
            age--;

        return age;
    }

    /// <summary>
    /// Rounds a datetime to the nearest specified interval.
    /// </summary>
    public static DateTime RoundToNearest(this DateTime dateTime, TimeSpan interval)
    {
        var halfInterval = new TimeSpan(interval.Ticks / 2);
        return new DateTime(((dateTime.Ticks + halfInterval.Ticks) / interval.Ticks) * interval.Ticks);
    }

    /// <summary>
    /// Converts a datetime to ISO 8601 format string.
    /// </summary>
    public static string ToIso8601(this DateTime dateTime)
    {
        return dateTime.ToString("O");
    }

    /// <summary>
    /// Checks if a datetime is between two other datetimes (inclusive).
    /// </summary>
    public static bool IsBetween(this DateTime dateTime, DateTime start, DateTime end)
    {
        return dateTime >= start && dateTime <= end;
    }
}
