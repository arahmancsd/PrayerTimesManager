namespace PrayerTimesManager;

/// <summary>
/// Represents the calculated prayer times for a single day, formatted as strings.
/// </summary>
/// <param name="Imsak">The Imsak time.</param>
/// <param name="Fajr">The Fajr (dawn) prayer time.</param>
/// <param name="Sunrise">The sunrise time.</param>
/// <param name="Dhuhr">The Dhuhr (noon) prayer time.</param>
/// <param name="Asr">The Asr (afternoon) prayer time.</param>
/// <param name="Sunset">The sunset time.</param>
/// <param name="Maghrib">The Maghrib (sunset) prayer time.</param>
/// <param name="Isha">The Isha (night) prayer time.</param>
/// <param name="Midnight">The Islamic midnight time.</param>
public sealed record PrayerTimesResult(
    string Imsak,
    string Fajr,
    string Sunrise,
    string Dhuhr,
    string Asr,
    string Sunset,
    string Maghrib,
    string Isha,
    string Midnight);
