# DateTimeExtensions

A set of extension methods that add common date‑time manipulation and formatting capabilities to the `System.DateTime` type. The methods are pure, stateless, and safe to call from multiple threads.

## API

### IsInPast
- **Purpose**: Determines whether the supplied date‑time occurs earlier than the current local date‑time.
- **Parameters**: `this DateTime value` – the date‑time to evaluate.
- **Return value**: `true` if `value` is in the past; otherwise `false`.
- **Exceptions**: None.

### IsInFuture
- **Purpose**: Determines whether the supplied date‑time occurs later than the current local date‑time.
- **Parameters**: `this DateTime value` – the date‑time to evaluate.
- **Return value**: `true` if `value` is in the future; otherwise `false`.
- **Exceptions**: None.

### BeginningOfDay
- **Purpose**: Returns a new `DateTime` representing the start of the day (00:00:00) for the supplied date‑time.
- **Parameters**: `this DateTime value` – the date‑time whose day start is required.
- **Return value**: A `DateTime` with the same date as `value` and time set to 00:00:00.
- **Exceptions**: None.

### EndOfDay
- **Purpose**: Returns a new `DateTime` representing the end of the day (23:59:59.9999999) for the supplied date‑time.
- **Parameters**: `this DateTime value` – the date‑time whose day end is required.
- **Return value**: A `DateTime` with the same date as `value` and time set to the last tick of the day.
- **Exceptions**: None.

### FirstDayOfMonth
- **Purpose**: Returns a new `DateTime` representing the first day of the month (00:00:00) for the supplied date‑time.
- **Parameters**: `this DateTime value` – the date‑time whose month start is required.
- **Return value**: A `DateTime` with the day set to 1 and time set to 00:00:00, preserving the month and year of `value`.
- **Exceptions**: None.

### LastDayOfMonth
- **Purpose**: Returns a new `DateTime` representing the last day of the month (end of day) for the supplied date‑time.
- **Parameters**: `this DateTime value` – the date‑time whose month end is required.
- **Return value**: A `DateTime` set to the final tick of the month containing `value`.
- **Exceptions**: None.

### FirstDayOfQuarter
- **Purpose**: Returns a new `DateTime` representing the first day of the quarter (00:00:00) for the supplied date‑time.
- **Parameters**: `this DateTime value` – the date‑time whose quarter start is required.
- **Return value**: A `DateTime` with the month set to the first month of the quarter (Jan, Apr, Jul, Oct), day set to 1, and time set to 00:00:00.
- **Exceptions**: None.

### LastDayOfQuarter
- **Purpose**: Returns a new `DateTime` representing the last day of the quarter (end of day) for the supplied date‑time.
- **Parameters**: `this DateTime value` – the date‑time whose quarter end is required.
- **Return value**: A `DateTime` set to the final tick of the quarter containing `value`.
- **Exceptions**: None.

### FirstDayOfYear
- **Purpose**: Returns a new `DateTime` representing the first day of the year (00:00:00) for the supplied date‑time.
- **Parameters**: `this DateTime value` – the date‑time whose year start is required.
- **Return value**: A `DateTime` with month set to 1, day set to 1, and time set to 00:00:00.
- **Exceptions**: None.

### LastDayOfYear
- **Purpose**: Returns a new `DateTime` representing the last day of the year (end of day) for the supplied date‑time.
- **Parameters**: `this DateTime value` – the date‑time whose year end is required.
- **Return value**: A `DateTime` set to the final tick of the year containing `value`.
- **Exceptions**: None.

### NextOccurrenceOf
- **Purpose**: Returns the next occurrence of a specified day of the week after the supplied date‑time.
- **Parameters**: 
  - `this DateTime source` – the date‑time from which to search.
  - `DayOfWeek dayOfWeek` – the target day of the week.
- **Return value**: A `DateTime` representing the next `dayOfWeek` that occurs after `source`. If `source` already falls on `dayOfWeek`, the result is the following week’s occurrence.
- **Exceptions**: None.

### PreviousOccurrenceOf
- **Purpose**: Returns the previous occurrence of a specified day of the week before the supplied date‑time.
- **Parameters**: 
  - `this DateTime source` – the date‑time from which to search.
  - `DayOfWeek dayOfWeek` – the target day of the week.
- **Return value**: A `DateTime` representing the most recent `dayOfWeek` that occurs before `source`. If `source` already falls on `dayOfWeek`, the result is the previous week’s occurrence.
- **Exceptions**: None.

### Format
- **Purpose**: Formats the supplied date‑time using a custom format string.
- **Parameters**: 
  - `this DateTime value` – the date‑time to format.
  - `string format` – a standard or custom format string as accepted by `DateTime.ToString(string)`.
- **Return value**: A string representation of `value` according to `format`.
- **Exceptions**: 
  - `ArgumentNullException` if `format` is `null`.
  - `FormatException` if `format` is invalid.

### ToRelativeTime
- **Purpose**: Produces a human‑readable relative time string (e.g., “2 hours ago”, “in 5 minutes”) based on the difference between the supplied date‑time and the current local date‑time.
- **Parameters**: `this DateTime value` – the date‑time to relativize.
- **Return value**: A string describing the approximate time difference.
- **Exceptions**: None.

### IsSameDay
- **Purpose**: Determines whether two date‑time values fall on the same calendar day.
- **Parameters**: 
  - `this DateTime value` – the first date‑time.
  - `DateTime other` – the second date‑time to compare against.
- **Return value**: `true` if both values share the same year, month, and day; otherwise `false`.
- **Exceptions**: None.

### IsWeekend
- **Purpose**: Checks whether the supplied date‑time falls on a Saturday or Sunday.
- **Parameters**: `this DateTime value` – the date‑time to evaluate.
- **Return value**: `true` if `value.DayOfWeek` is `Saturday` or `Sunday`; otherwise `false`.
- **Exceptions**: None.

### IsWeekday
- **Purpose**: Checks whether the supplied date‑time falls on a Monday through Friday.
- **Parameters**: `this DateTime value` – the date‑time to evaluate.
- **Return value**: `true` if `value.DayOfWeek` is Monday, Tuesday, Wednesday, Thursday, or Friday; otherwise `false`.
- **Exceptions**: None.

### GetAge
- **Purpose**: Calculates the age in years based on the supplied birth date relative to the current local date.
- **Parameters**: `this DateTime birthDate` – the date of birth.
- **Return value**: An integer representing the number of full years elapsed.
- **Exceptions**: 
  - `ArgumentOutOfRangeException` if `birthDate` is later than the current date.

### RoundToNearest
- **Purpose**: Rounds the supplied date‑time to the nearest interval of a given `TimeSpan`.
- **Parameters**: 
  - `this DateTime value` – the date‑time to round.
  - `TimeSpan interval` – the rounding interval (must be greater than zero).
- **Return value**: A `DateTime` rounded to the nearest `interval`. Ties are rounded away from zero (up).
- **Exceptions**: 
  - `ArgumentOutOfRangeException` if `interval` is less than or equal to `TimeSpan.Zero`.

### ToIso8601
- **Purpose**: Returns an ISO 8601‑compliant string representation of the supplied date‑time.
- **Parameters**: `this DateTime value` – the date‑time to format.
- **Return value**: A string in the format `yyyy-MM-ddTHH:mm:ss.fffK` (where `K` represents the Kind designator: Z for UTC, +hh:mm or -hh:mm for local offset).
- **Exceptions**: None.

## Usage

```csharp
using System;
using static DateTimeExtensions; // assuming the class is imported as a static namespace

class Program
{
    static void Main()
    {
        DateTime now = DateTime.Now;

        // Check if a date is in the past and get a friendly relative time string.
        DateTime yesterday = now.AddDays(-1);
        if (yesterday.IsInPast())
        {
            Console.WriteLine($"Yesterday was {yesterday.ToRelativeTime()}");
        }

        // Determine the first day of the current quarter and format it as ISO 8601.
        DateTime quarterStart = now.FirstDayOfQuarter();
        string iso = quarterStart.ToIso8601();
        Console.WriteLine($"Quarter starts at: {iso}");
    }
}
```

```csharp
using System;

class Example
{
    static void Main()
    {
        DateTime birth = new DateTime(1990, 5, 23);
        int age = birth.GetAge();
        Console.WriteLine($"Age: {age}");

        DateTime meeting = new DateTime(2025, 10, 15, 14, 30, 0);
        // Find the next Tuesday after the meeting date.
        DateTime nextTuesday = meeting.NextOccurrenceOf(DayOfWeek.Tuesday);
        Console.WriteLine($"Next Tuesday: {nextTuesday:F}");
    }
}
```

## Notes

- All extension methods are **pure**: they do not modify the instance on which they are invoked and have no side effects.
- Because they rely only on the supplied arguments and, where applicable, `DateTime.Now`, they are **thread‑safe** as long as the caller does not pass mutable shared state that is changed concurrently.
- Methods that depend on `DateTime.Now` (`IsInPast`, `IsInFuture`, `ToRelativeTime`, `GetAge`) may produce different results across calls if the system clock changes; consider injecting a custom provider for deterministic testing.
- `GetAge` throws when the birth date is in the future; callers should validate input before invoking the method if such a scenario is possible.
- `RoundToNearest` expects a positive `TimeSpan`; supplying a zero or negative interval results in an `ArgumentOutOfRangeException`.
- The `Format` method mirrors the behavior of `DateTime.ToString(string)`; invalid format strings trigger a `FormatException`, and a `null` format triggers an `ArgumentNullException`.
- Results from `BeginningOfDay`, `EndOfDay`, `FirstDayOfMonth`, etc., always have the **Kind** property unchanged from the source value; if UTC semantics are required, convert the source to `DateTime.UtcNow` before calling these methods.
