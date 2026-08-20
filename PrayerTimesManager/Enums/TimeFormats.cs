namespace PrayerTimesManager.Enums;

/// <summary>
/// Specifies the format used to render calculated prayer times as strings.
/// </summary>
public enum TimeFormats
{
    /// <summary>24-hour format, e.g. "13:45".</summary>
    TIME_FORMAT_24H,
    /// <summary>12-hour format with AM/PM suffix, e.g. "1:45 PM".</summary>
    TIME_FORMAT_12H,
    /// <summary>12-hour format without AM/PM suffix, e.g. "1:45".</summary>
    TIME_FORMAT_12hNS,
    /// <summary>Floating point number of hours, e.g. "13.75".</summary>
    TIME_FORMAT_FLOAT,
    /// <summary>ISO 8601 time format, e.g. "13:45:00".</summary>
    TIME_FORMAT_ISO8601,
    /// <summary>The default time format, equivalent to <see cref="TIME_FORMAT_12H"/>.</summary>
    DEFAULT = TIME_FORMAT_12H
}