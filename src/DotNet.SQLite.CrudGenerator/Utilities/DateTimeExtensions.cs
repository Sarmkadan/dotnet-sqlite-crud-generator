#nullable enable
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
    /// <param name="dateTime">The date to check.</param>
    /// <returns><see langword="true"/> if the date is in the past; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="dateTime"/> is <see cref="DateTime.MinValue"/> or <see cref="DateTime.MaxValue"/>.</exception>
    public static bool IsInPast(this DateTime dateTime)
    {
        if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(dateTime), "DateTime value is out of valid range.");
        }

        return dateTime < DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if a datetime is in the future.
    /// </summary>
    /// <param name="dateTime">The date to check.</param>
    /// <returns><see langword="true"/> if the date is in the future; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="dateTime"/> is <see cref="DateTime.MinValue"/> or <see cref="DateTime.MaxValue"/>.</exception>
    public static bool IsInFuture(this DateTime dateTime)
    {
        if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(dateTime), "DateTime value is out of valid range.");
        }

        return dateTime > DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the beginning of the day (midnight).
    /// </summary>
    /// <param name="dateTime">The date to get the beginning of day for.</param>
    /// <returns>A <see cref="DateTime"/> representing midnight of the same day.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="dateTime"/> is <see cref="DateTime.MinValue"/> or <see cref="DateTime.MaxValue"/>.</exception>
    public static DateTime BeginningOfDay(this DateTime dateTime)
    {
        if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(dateTime), "DateTime value is out of valid range.");
        }

        return dateTime.Date;
    }

    /// <summary>
    /// Gets the end of the day (23:59:59.999).
    /// </summary>
    /// <param name="dateTime">The date to get the end of day for.</param>
    /// <returns>A <see cref="DateTime"/> representing 23:59:59.999 of the same day.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="dateTime"/> is <see cref="DateTime.MinValue"/> or <see cref="DateTime.MaxValue"/>.</exception>
    public static DateTime EndOfDay(this DateTime dateTime)
    {
        if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(dateTime), "DateTime value is out of valid range.");
        }

        return dateTime.Date.AddDays(1).AddMilliseconds(-1);
    }

    /// <summary>
    /// Gets the first day of the month for the given date.
    /// </summary>
    /// <param name="dateTime">The date to get the first day of month for.</param>
    /// <returns>A <see cref="DateTime"/> representing the first day of the month.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="dateTime"/> is <see cref="DateTime.MinValue"/> or <see cref="DateTime.MaxValue"/>.</exception>
    public static DateTime FirstDayOfMonth(this DateTime dateTime)
    {
        if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(dateTime), "DateTime value is out of valid range.");
        }

        return new DateTime(dateTime.Year, dateTime.Month, 1);
    }

    /// <summary>
    /// Gets the last day of the month for the given date.
    /// </summary>
    /// <param name="dateTime">The date to get the last day of month for.</param>
    /// <returns>A <see cref="DateTime"/> representing the last day of the month.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="dateTime"/> is <see cref="DateTime.MinValue"/> or <see cref="DateTime.MaxValue"/>.</exception>
    public static DateTime LastDayOfMonth(this DateTime dateTime)
    {
        if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(dateTime), "DateTime value is out of valid range.");
        }

        return dateTime.FirstDayOfMonth().AddMonths(1).AddDays(-1);
    }

    /// <summary>
    /// Gets the first day of the quarter for the given date.
    /// </summary>
    /// <param name="dateTime">The date to get the first day of quarter for.</param>
    /// <returns>A <see cref="DateTime"/> representing the first day of the quarter.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="dateTime"/> is <see cref="DateTime.MinValue"/> or <see cref="DateTime.MaxValue"/>.</exception>
    public static DateTime FirstDayOfQuarter(this DateTime dateTime)
    {
        if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(dateTime), "DateTime value is out of valid range.");
        }

        var month = ((dateTime.Month - 1) / 3) * 3 + 1;
        return new DateTime(dateTime.Year, month, 1);
    }

    /// <summary>
    /// Gets the last day of the quarter for the given date.
    /// </summary>
    /// <param name="dateTime">The date to get the last day of quarter for.</param>
    /// <returns>A <see cref="DateTime"/> representing the last day of the quarter.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="dateTime"/> is <see cref="DateTime.MinValue"/> or <see cref="DateTime.MaxValue"/>.</exception>
    public static DateTime LastDayOfQuarter(this DateTime dateTime)
    {
        if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(dateTime), "DateTime value is out of valid range.");
        }

        return dateTime.FirstDayOfQuarter().AddMonths(3).AddDays(-1);
    }

    /// <summary>
    /// Gets the first day of the year for the given date.
    /// </summary>
    /// <param name="dateTime">The date to get the first day of year for.</param>
    /// <returns>A <see cref="DateTime"/> representing January 1st of the same year.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="dateTime"/> is <see cref="DateTime.MinValue"/> or <see cref="DateTime.MaxValue"/>.</exception>
    public static DateTime FirstDayOfYear(this DateTime dateTime)
    {
        if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(dateTime), "DateTime value is out of valid range.");
        }

        return new DateTime(dateTime.Year, 1, 1);
    }

    /// <summary>
    /// Gets the last day of the year for the given date.
    /// </summary>
    /// <param name="dateTime">The date to get the last day of year for.</param>
    /// <returns>A <see cref="DateTime"/> representing December 31st of the same year.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="dateTime"/> is <see cref="DateTime.MinValue"/> or <see cref="DateTime.MaxValue"/>.</exception>
    public static DateTime LastDayOfYear(this DateTime dateTime)
    {
        if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(dateTime), "DateTime value is out of valid range.");
        }

        return new DateTime(dateTime.Year, 12, 31);
    }

    /// <summary>
    /// Gets the next occurrence of a specific day of the week.
    /// </summary>
    /// <param name="dateTime">The starting date.</param>
    /// <param name="dayOfWeek">The target day of the week to find.</param>
    /// <returns>A <see cref="DateTime"/> representing the next occurrence of the specified day of the week.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="dateTime"/> is <see cref="DateTime.MinValue"/> or <see cref="DateTime.MaxValue"/>.</exception>
    public static DateTime NextOccurrenceOf(this DateTime dateTime, DayOfWeek dayOfWeek)
    {
        if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(dateTime), "DateTime value is out of valid range.");
        }

        var daysUntilTarget = ((int)dayOfWeek - (int)dateTime.DayOfWeek + 7) % 7;
        if (daysUntilTarget == 0)
        {
            daysUntilTarget = 7;
        }

        return dateTime.AddDays(daysUntilTarget);
    }

    /// <summary>
    /// Gets the previous occurrence of a specific day of the week.
    /// </summary>
    /// <param name="dateTime">The starting date.</param>
    /// <param name="dayOfWeek">The target day of the week to find.</param>
    /// <returns>A <see cref="DateTime"/> representing the previous occurrence of the specified day of the week.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="dateTime"/> is <see cref="DateTime.MinValue"/> or <see cref="DateTime.MaxValue"/>.</exception>
    public static DateTime PreviousOccurrenceOf(this DateTime dateTime, DayOfWeek dayOfWeek)
    {
        if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(dateTime), "DateTime value is out of valid range.");
        }

        var daysUntilTarget = ((int)dateTime.DayOfWeek - (int)dayOfWeek + 7) % 7;
        if (daysUntilTarget == 0)
        {
            daysUntilTarget = 7;
        }

        return dateTime.AddDays(-daysUntilTarget);
    }

    /// <summary>
    /// Formats a datetime using a specific format string with fallback.
    /// </summary>
    /// <param name="dateTime">The date to format.</param>
    /// <param name="format">The format string to use.</param>
    /// <param name="fallback">The fallback string to return if formatting fails. Defaults to "N/A".</param>
    /// <returns>The formatted date string or the fallback value if formatting fails.</returns>
    public static string Format(this DateTime dateTime, string format, string fallback = "N/A")
    {
        ArgumentNullException.ThrowIfNull(format);

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
    /// <param name="dateTime">The date to get relative time for.</param>
    /// <returns>A human-readable string representing the relative time.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="dateTime"/> is <see cref="DateTime.MinValue"/> or <see cref="DateTime.MaxValue"/>.</exception>
    public static string ToRelativeTime(this DateTime dateTime)
    {
        if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(dateTime), "DateTime value is out of valid range.");
        }

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
    /// <param name="dateTime">The first date to compare.</param>
    /// <param name="other">The second date to compare.</param>
    /// <returns><see langword="true"/> if both dates are on the same day; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when either <paramref name="dateTime"/> or <paramref name="other"/> is <see cref="DateTime.MinValue"/> or <see cref="DateTime.MaxValue"/>.</exception>
    public static bool IsSameDay(this DateTime dateTime, DateTime other)
    {
        if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(dateTime), "DateTime value is out of valid range.");
        }

        if (other == DateTime.MinValue || other == DateTime.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(other), "DateTime value is out of valid range.");
        }

        return dateTime.Date == other.Date;
    }

    /// <summary>
    /// Checks if a datetime is a weekend (Saturday or Sunday).
    /// </summary>
    /// <param name="dateTime">The date to check.</param>
    /// <returns><see langword="true"/> if the date is a weekend; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="dateTime"/> is <see cref="DateTime.MinValue"/> or <see cref="DateTime.MaxValue"/>.</exception>
    public static bool IsWeekend(this DateTime dateTime)
    {
        if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(dateTime), "DateTime value is out of valid range.");
        }

        return dateTime.DayOfWeek == DayOfWeek.Saturday || dateTime.DayOfWeek == DayOfWeek.Sunday;
    }

    /// <summary>
    /// Checks if a datetime is a weekday (Monday-Friday).
    /// </summary>
    /// <param name="dateTime">The date to check.</param>
    /// <returns><see langword="true"/> if the date is a weekday; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="dateTime"/> is <see cref="DateTime.MinValue"/> or <see cref="DateTime.MaxValue"/>.</exception>
    public static bool IsWeekday(this DateTime dateTime)
    {
        return !dateTime.IsWeekend();
    }

    /// <summary>
    /// Gets the age in years from a birth date to a given date.
    /// </summary>
    /// <param name="birthDate">The birth date.</param>
    /// <param name="asOfDate">The date to calculate age as of. If null, uses current UTC date.</param>
    /// <returns>The age in years.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="birthDate"/> is <see cref="DateTime.MinValue"/> or <see cref="DateTime.MaxValue"/> or if <paramref name="asOfDate"/> is <see cref="DateTime.MinValue"/> or <see cref="DateTime.MaxValue"/>.</exception>
    public static int GetAge(this DateTime birthDate, DateTime? asOfDate = null)
    {
        if (birthDate == DateTime.MinValue || birthDate == DateTime.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(birthDate), "Birth date is out of valid range.");
        }

        var today = asOfDate ?? DateTime.UtcNow;
        if (today == DateTime.MinValue || today == DateTime.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(asOfDate), "As-of date is out of valid range.");
        }

        var age = today.Year - birthDate.Year;

        if (birthDate.Date > today.AddYears(-age))
        {
            age--;
        }

        return age;
    }

    /// <summary>
    /// Rounds a datetime to the nearest specified interval.
    /// </summary>
    /// <param name="dateTime">The date to round.</param>
    /// <param name="interval">The time span interval to round to.</param>
    /// <returns>A <see cref="DateTime"/> rounded to the nearest interval.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="dateTime"/> is <see cref="DateTime.MinValue"/> or <see cref="DateTime.MaxValue"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="interval"/> is zero or negative.</exception>
    public static DateTime RoundToNearest(this DateTime dateTime, TimeSpan interval)
    {
        if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(dateTime), "DateTime value is out of valid range.");
        }

        if (interval == TimeSpan.Zero || interval < TimeSpan.Zero)
        {
            throw new ArgumentException("Interval must be positive and non-zero.", nameof(interval));
        }

        var halfInterval = new TimeSpan(interval.Ticks / 2);
        return new DateTime(((dateTime.Ticks + halfInterval.Ticks) / interval.Ticks) * interval.Ticks);
    }

    /// <summary>
    /// Converts a datetime to ISO 8601 format string.
    /// </summary>
    /// <param name="dateTime">The date to convert.</param>
    /// <returns>An ISO 8601 formatted date string.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="dateTime"/> is <see cref="DateTime.MinValue"/> or <see cref="DateTime.MaxValue"/>.</exception>
    public static string ToIso8601(this DateTime dateTime)
    {
        if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(dateTime), "DateTime value is out of valid range.");
        }

        return dateTime.ToString("O");
    }

    /// <summary>
    /// Checks if a datetime is between two other datetimes (inclusive).
    /// </summary>
    /// <param name="dateTime">The date to check.</param>
    /// <param name="start">The start of the range (inclusive).</param>
    /// <param name="end">The end of the range (inclusive).</param>
    /// <returns><see langword="true"/> if the date is between start and end; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="dateTime"/> is <see cref="DateTime.MinValue"/> or <see cref="DateTime.MaxValue"/>.</exception>
    public static bool IsBetween(this DateTime dateTime, DateTime start, DateTime end)
    {
        if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(dateTime), "DateTime value is out of valid range.");
        }

        return dateTime >= start && dateTime <= end;
    }
}