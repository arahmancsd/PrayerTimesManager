namespace PrayerTimesManager;

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
