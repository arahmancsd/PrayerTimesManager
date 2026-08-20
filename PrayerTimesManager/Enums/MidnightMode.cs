namespace PrayerTimesManager.Enums;

/// <summary>
/// Specifies the method used to calculate the Islamic midnight time.
/// </summary>
public enum MidnightModes
{
    /// <summary>Midnight is the midpoint between sunset and sunrise.</summary>
    STANDARD = 0, // Mid Sunset to Sunrise
    /// <summary>Midnight is the midpoint between sunset and Fajr.</summary>
    JAFARI = 1, // Mid Sunset to Fajr
    /// <summary>The default midnight mode, equivalent to <see cref="STANDARD"/>.</summary>
    DEFAULT = STANDARD
}
