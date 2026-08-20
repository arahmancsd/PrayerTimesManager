namespace PrayerTimesManager;

/// <summary>
/// Represents the position of the sun at a given moment, as used by the prayer time calculations.
/// </summary>
public sealed class Sun
{
    /// <summary>Gets or sets the sun's declination, in degrees.</summary>
    public double Declination { get; set; }
    /// <summary>Gets or sets the equation of time, in hours.</summary>
    public double Equation { get; set; }
}
