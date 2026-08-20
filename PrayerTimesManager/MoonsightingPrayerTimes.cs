namespace PrayerTimesManager;

/// <summary>
/// Base class implementing the Moonsighting Committee Worldwide seasonal adjustment method
/// for calculating Fajr and Isha times based on the day of the year and latitude.
/// </summary>
public class MoonsightingPrayerTimes
{
    /// <summary>The latitude of the location, in degrees.</summary>
    protected double _latitude;
    /// <summary>The date for which the calculation is performed.</summary>
    protected DateTime _date;
    /// <summary>Seasonal minutes coefficients used by the interpolation formula.</summary>
    protected double a, b, c, d;
    /// <summary>The day of the year, adjusted relative to the winter/summer solstice for the applicable hemisphere.</summary>
    protected int _dyy;
    /// <summary>The hemisphere ("north" or "south") of the location, derived from <see cref="_latitude"/>.</summary>
    public string? _hemisphere;
    /// <summary>Initializes a new instance of the <see cref="MoonsightingPrayerTimes"/> class.</summary>
    /// <param name="date">The date for which the calculation is performed.</param>
    /// <param name="latitude">The latitude of the location, in degrees.</param>
    public MoonsightingPrayerTimes(DateTime date, double latitude)
    {
        _date = date;
        _latitude = latitude;
        GetDyy();
    }

    /// <summary>
    /// Computes and returns the day of the year adjusted relative to the winter/summer solstice,
    /// depending on which hemisphere the location is in.
    /// </summary>
    /// <returns>The adjusted day of the year.</returns>
    private int GetDyy()
    {
        int year = _date.Year;
        DateTime dateDyyZero;
        if (_latitude > 0)
        { // Northern Hemisphere
            _hemisphere = "north";
            dateDyyZero = new DateTime(year, 12, 21, 12, 0, 0);
        }
        else
        { // Southern Hemisphere
            _hemisphere = "south";
            dateDyyZero = new DateTime(year, 6, 21, 12, 0, 0);
        }

        int diff = _date.Date.Subtract(dateDyyZero.Date).Days;
        int daysInYear = DateTime.IsLeapYear(year) ? 366 : 365;

        _dyy = diff >= 0 ? diff : daysInYear + diff;

        return _dyy;
    }
    /// <summary>Computes the seasonal number of minutes using piecewise linear interpolation over the year.</summary>
    /// <returns>The interpolated number of minutes.</returns>
    protected double GetMinutes()
    {
        if (_dyy < 91)
            return a + (b - a) / 91 * _dyy; // '91 DAYS SPAN
        else if (_dyy < 137)
            return b + (c - b) / 46 * (_dyy - 91); // '46 DAYS SPAN
        else if (_dyy < 183)
            return c + (d - c) / 46 * (_dyy - 137); // '46 DAYS SPAN
        else if (_dyy < 229)
            return d + (c - d) / 46 * (_dyy - 183); // '46 DAYS SPAN
        else if (_dyy < 275)
            return c + (b - c) / 46 * (_dyy - 229); // '46 DAYS SPAN
        else if (_dyy >= 275)
            return b + (a - b) / 91 * (_dyy - 275); // ' 91 DAYS SPAN
        return 0;
    }
}

/// <summary>
/// Calculates the number of minutes before sunrise for Fajr using the Moonsighting Committee
/// Worldwide seasonal adjustment method.
/// </summary>
public class Fajr : MoonsightingPrayerTimes
{
    /// <summary>Initializes a new instance of the <see cref="Fajr"/> class.</summary>
    /// <param name="date">The date for which the calculation is performed.</param>
    /// <param name="latitude">The latitude of the location, in degrees.</param>
    public Fajr(DateTime date, double latitude) : base(date, latitude)
    {
        a = 75d + 28.65 / 55d * Math.Abs(_latitude);
        b = 75 + 19.44 / 55 * Math.Abs(_latitude);
        c = 75 + 32.74 / 55 * Math.Abs(_latitude);
        d = 75 + 48.1 / 55 * Math.Abs(_latitude);
    }

    /// <summary>Gets the number of minutes before sunrise at which Fajr occurs.</summary>
    /// <returns>The number of minutes before sunrise.</returns>
    public int GetMinutesBeforeSunrise()
    {
        return (int)Math.Round(GetMinutes());
    }
}

/// <summary>
/// Specifies the twilight color used to determine the Isha time under the Moonsighting Committee
/// Worldwide calculation method.
/// </summary>
public enum Shafaq
{
    /// <summary>Reddish twilight (Shafaq Ahmer).</summary>
    SHAFAQ_AHMER,
    /// <summary>Whitish twilight (Shafaq Abyad).</summary>
    SHAFAQ_ABYAD,
    /// <summary>General twilight, used when no specific color is required.</summary>
    SHAFAQ_GENERAL
}

/// <summary>
/// Calculates the number of minutes after sunset for Isha using the Moonsighting Committee
/// Worldwide seasonal adjustment method.
/// </summary>
public sealed class Isha : MoonsightingPrayerTimes
{
    /// <summary>The default <see cref="Shafaq"/> value used when none is specified.</summary>
    public const Shafaq shafaq = Shafaq.SHAFAQ_GENERAL;
    /// <summary>Initializes a new instance of the <see cref="Isha"/> class.</summary>
    /// <param name="date">The date for which the calculation is performed.</param>
    /// <param name="latitude">The latitude of the location, in degrees.</param>
    /// <param name="shafaq">The twilight color used to determine the Isha time.</param>
    public Isha(DateTime date, double latitude, Shafaq shafaq = Shafaq.SHAFAQ_GENERAL) : base(date, latitude)
    {
        SetShafaq(shafaq);
    }

    /// <summary>Sets the seasonal minutes coefficients based on the specified twilight color.</summary>
    /// <param name="shafaq">The twilight color used to determine the Isha time.</param>
    public void SetShafaq(Shafaq shafaq)
    {
        switch (shafaq)
        {
            case Shafaq.SHAFAQ_AHMER:
                a = 62 + 17.4 / 55.0 * Math.Abs(_latitude);
                b = (62 - 7.16 / 55.0 * Math.Abs(_latitude));
                c = 62 + 5.12 / 55.0 * Math.Abs(_latitude);
                d = 62 + 19.44 / 55.0 * Math.Abs(_latitude);
                break;
            case Shafaq.SHAFAQ_ABYAD:
                a = 75 + 25.6 / 55.0 * Math.Abs(_latitude);
                b = 75 + 7.16 / 55.0 * Math.Abs(_latitude);
                c = 75 + 36.84 / 55.0 * Math.Abs(_latitude);
                d = 75 + 81.84 / 55.0 * Math.Abs(_latitude);
                break;
            default:
                a = 75 + 25.6 / 55.0 * Math.Abs(_latitude);
                c = 75 - 9.21 / 55.0 * Math.Abs(_latitude);
                b = 75 + 2.05 / 55.0 * Math.Abs(_latitude);
                d = 75 + 6.14 / 55.0 * Math.Abs(_latitude);
                break;
        }
    }

    /// <summary>Gets the number of minutes after sunset at which Isha occurs.</summary>
    /// <returns>The number of minutes after sunset.</returns>
    public int GetMinutesAfterSunset()
    {
        return (int)Math.Round(GetMinutes());
    }
}

/// <summary>
/// Specifies which juristic convention is used to determine the actual (Shari'ah-compliant)
/// sunset offset applied to Maghrib under the Moonsighting Committee Worldwide calculation method.
/// </summary>
public enum MoonsightingMaghribType
{
    /// <summary>Sunni convention: actual sunset occurs <see cref="MoonsightingOffsets.SunniMaghribOffsetMinutes"/> minutes after theoretical sunset.</summary>
    SUNNI,
    /// <summary>Shi'a convention: actual sunset occurs <see cref="MoonsightingOffsets.ShiaMaghribOffsetMinutes"/> minutes after theoretical sunset.</summary>
    SHIA,
    /// <summary>The default Maghrib convention, equivalent to <see cref="SUNNI"/>.</summary>
    DEFAULT = SUNNI
}

/// <summary>
/// Provides the default Zuhr and Maghrib offsets, in minutes, defined by the Moonsighting
/// Committee Worldwide method. See https://www.moonsighting.com/how-we.html for details.
/// </summary>
public static class MoonsightingOffsets
{
    /// <summary>The default number of minutes after zenith at which Zuhr occurs (5 minutes).</summary>
    public const double ZuhrOffsetMinutes = 5d;
    /// <summary>The default number of minutes after theoretical sunset at which Maghrib occurs under the Sunni convention (3 minutes).</summary>
    public const double SunniMaghribOffsetMinutes = 3d;
    /// <summary>The default number of minutes after theoretical sunset at which Maghrib occurs under the Shi'a convention (17 minutes).</summary>
    public const double ShiaMaghribOffsetMinutes = 17d;

    /// <summary>Gets the Maghrib offset, in minutes, for the specified <see cref="MoonsightingMaghribType"/>.</summary>
    /// <param name="maghribType">The Maghrib convention.</param>
    /// <returns>The offset, in minutes, after theoretical sunset.</returns>
    public static double GetMaghribOffsetMinutes(MoonsightingMaghribType maghribType) =>
        maghribType == MoonsightingMaghribType.SHIA ? ShiaMaghribOffsetMinutes : SunniMaghribOffsetMinutes;
}
