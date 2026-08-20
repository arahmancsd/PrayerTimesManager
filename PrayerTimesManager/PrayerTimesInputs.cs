using PrayerTimesManager.Enums;

namespace PrayerTimesManager;

/// <summary>
/// Encapsulates the location, date, and formatting inputs required to calculate prayer times.
/// </summary>
/// <param name="Latitude">The latitude of the location, in degrees.</param>
/// <param name="Longitude">The longitude of the location, in degrees.</param>
/// <param name="DateTime">The date for which prayer times should be calculated. Defaults to the current local date/time if not specified.</param>
/// <param name="TimeZone">The time zone of the location. Defaults to the offset of <paramref name="DateTime"/> if not specified.</param>
/// <param name="Elevation">The elevation of the location, in meters, used to adjust the sunrise/sunset angle.</param>
/// <param name="LatitudeAdjustmentMethod">The method used to adjust prayer times for high latitude locations.</param>
/// <param name="MidnightMode">The method used to calculate the Islamic midnight time.</param>
/// <param name="Format">The format used to render the calculated prayer times.</param>
public record PrayerTimesInputs(
    double Latitude,
    double Longitude,
    DateTimeOffset? DateTime = null,
    TimeZoneInfo? TimeZone = null,
    double? Elevation = null,
    LatitudeAdjustmentMethods LatitudeAdjustmentMethod = LatitudeAdjustmentMethods.DEFAULT,
    MidnightModes MidnightMode = MidnightModes.DEFAULT,
    TimeFormats Format = TimeFormats.DEFAULT);
