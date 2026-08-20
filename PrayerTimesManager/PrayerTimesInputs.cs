using PrayerTimesManager.Enums;

namespace PrayerTimesManager;

public record PrayerTimesInputs(
    double Latitude,
    double Longitude,
    DateTimeOffset? DateTime = null,
    TimeZoneInfo? TimeZone = null,
    double? Elevation = null,
    LatitudeAdjustmentMethods LatitudeAdjustmentMethod = LatitudeAdjustmentMethods.DEFAULT,
    MidnightModes MidnightMode = MidnightModes.DEFAULT,
    TimeFormats Format = TimeFormats.DEFAULT);
