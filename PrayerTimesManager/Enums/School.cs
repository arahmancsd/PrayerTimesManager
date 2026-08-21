namespace PrayerTimesManager.Enums;

/// <summary>
/// Specifies the juristic school used to determine the Asr shadow factor.
/// </summary>
public enum Schools
{
    /// <summary>Shafi'i, Maliki, and Hanbali schools (shadow factor of 1).</summary>
    STANDARD = 1,
    /// <summary>Hanafi school (shadow factor of 2).</summary>
    HANAFI = 2,
    /// <summary>Ja'fari (Shi'a) school (shadow factor of 4/7).</summary>
    JAFARI = 3,
    /// <summary>The default school, equivalent to <see cref="STANDARD"/>.</summary>
    DEFAULT = STANDARD
}