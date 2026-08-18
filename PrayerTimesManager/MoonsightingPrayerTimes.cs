namespace PrayerTimesManager;

public class MoonsightingPrayerTimes
{
    public double _latitude;
    public DateTime _date;
    protected double a, b, c, d;
    protected int _dyy;
    public string? _hemisphere;
    public MoonsightingPrayerTimes(DateTime date, double latitude)
    {
        _date = date;
        _latitude = latitude;
        GetDyy();
    }

    public int GetDyy()
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

public class Fajr : MoonsightingPrayerTimes
{
    public Fajr(DateTime date, double latitude) : base(date, latitude)
    {
        a = 75d + 28.65 / 55d * Math.Abs(_latitude);
        b = 75 + 19.44 / 55 * Math.Abs(_latitude);
        c = 75 + 32.74 / 55 * Math.Abs(_latitude);
        d = 75 + 48.1 / 55 * Math.Abs(_latitude);
    }

    public int GetMinutesBeforeSunrise()
    {
        return (int)Math.Round(GetMinutes());
    }
}

public enum Shafaq
{
    SHAFAQ_AHMER,
    SHAFAQ_ABYAD,
    SHAFAQ_GENERAL
}

public sealed class Isha : MoonsightingPrayerTimes
{
    public const Shafaq shafaq = Shafaq.SHAFAQ_GENERAL;
    public Isha(DateTime date, double latitude, Shafaq shafaq = Shafaq.SHAFAQ_GENERAL) : base(date, latitude)
    {
        SetShafaq(shafaq);
    }

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

    public int GetMinutesAfterSunset()
    {
        return (int)Math.Round(GetMinutes());
    }
}
