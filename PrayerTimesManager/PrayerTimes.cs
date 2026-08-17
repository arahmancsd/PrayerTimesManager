using PrayersTimeManager.Enums;
using System.Collections;

namespace PrayersTimeManager;

public class PrayerTimes
{
    public const string IMSAK = "Imsak";
    public const string FAJR = "Fajr";
    public const string SUNRISE = "Sunrise";
    public const string ZHUHR = "Dhuhr";
    public const string ASR = "Asr";
    public const string SUNSET = "Sunset";
    public const string MAGHRIB = "Maghrib";
    public const string ISHA = "Isha";
    public const string MIDNIGHT = "Midnight";

    public const string TIME_FORMAT_24H = "24h"; // 24-hour format
    public const string TIME_FORMAT_12H = "12h"; // 12-hour format
    public const string TIME_FORMAT_12hNS = "12hNS"; // 12-hour format with no suffix
    public const string TIME_FORMAT_FLOAT = "Float"; // floating point number
    public const string TIME_FORMAT_ISO8601 = "iso8601";

    public const string INVALID_TIME = "-----";
    //public static string[] FivePrayersTimeName = { "Fajr", "Dhuhr", "Asr", "Maghrib", "Isha" };
    //public static readonly string[] hijriMonthsName = new string[] { "Muharram", "Safar", "Rabiul-Awwal", "Rabi-uthani", "Jumadi-ul-Awwal", "Jumadi-uthani", "Rajab", "Sha’ban", "Ramadan", "Shawwal", "Zhul-Q’ada", "Zhul-Hijja" };

    public Dictionary<string, PrayerCalculationMethod> prayerCalculationMethods;
    public Hashtable prayerCalculationMethodCodes;

    private DateTime date;
    private PrayerCalculationMethods method = PrayerCalculationMethods.MWL;
    private Schools school = Schools.DEFAULT;
    private MidnightModes midnightMode = MidnightModes.DEFAULT;

    private LatitudeAdjustmentMethods latitudeAdjustmentMethod;
    private TimeFormats timeFormat;
    private Shafaq shafaq = Isha.shafaq; // Only valid for METHOD_MOONSIGHTING

    private double latitude;
    private double longitude;

    private double elevation;
    private int? asrShadowFactor;

    private Hashtable settings;
    //private int timeZone;   // time-zone
    private double timeZone;
    private Hashtable tuneTimesOffset;

    public PrayerTimes(PrayerCalculationMethods method = PrayerCalculationMethods.DEFAULT, Schools school = Schools.DEFAULT, int? asrShadowFactor = null)
    {
        SetPrayerCalculationMethods();

        this.method = method;
        this.school = school;
        this.asrShadowFactor = asrShadowFactor;

        LoadSettings();
    }

    private void LoadSettings()
    {
        settings = new Hashtable
        {
            [IMSAK] = prayerCalculationMethods[method.ToString()]?.Param[IMSAK] ?? "10 min",
            [FAJR] = prayerCalculationMethods[method.ToString()]?.Param[FAJR] ?? "0",
            [ZHUHR] = prayerCalculationMethods[method.ToString()]?.Param[ZHUHR] ?? "0 min",
            [ISHA] = prayerCalculationMethods[method.ToString()]?.Param[ISHA] ?? "0",
            [MAGHRIB] = prayerCalculationMethods[method.ToString()]?.Param[MAGHRIB] ?? "0 min",
        };

        var isMidnight = prayerCalculationMethods[method.ToString()]?.Param[MIDNIGHT] != null;

        if (isMidnight && (MidnightModes)prayerCalculationMethods[method.ToString()].Param[MIDNIGHT] == MidnightModes.JAFARI)
        {
            SetMidnightMode(MidnightModes.JAFARI);
        }
        else
        {
            SetMidnightMode(MidnightModes.STANDARD);
        }
    }

    private void SetPrayerCalculationMethods()
    {
        prayerCalculationMethods = PrayerCalculation.PrayerCalculations;
        prayerCalculationMethodCodes = PrayerCalculation.PrayerCalculationsCodes;
    }

    public void SetMethod(PrayerCalculationMethods method = PrayerCalculationMethods.MWL)
    {
        this.method = method;
        LoadSettings();
    }

    public void SetCustomMethod(PrayerCalculationMethod method)
    {
        SetMethod(PrayerCalculationMethods.CUSTOM);
        prayerCalculationMethods[this.method.ToString()] = method;
        LoadSettings();
    }

    public void SetTuneTimeOffset(Hashtable offset)
    {
        this.tuneTimesOffset = offset;
    }

    public void SetShafaq(Shafaq shafaq)
    {
        this.shafaq = shafaq;
    }

    public void SetSchool(Schools school)
    {
        this.SetAsrJuristicMethod(school);
    }

    public void SetAsrJuristicMethod(Schools school)
    {
        this.school = school;
    }

    public void SetMidnightMode(MidnightModes mode = MidnightModes.STANDARD)
    {
        midnightMode = mode;
    }

    public void SetTimeFormat(TimeFormats format = TimeFormats.DEFAULT)
    {
        this.timeFormat = format;
    }

    public void SetLatitudeAdjustmentMethod(LatitudeAdjustmentMethods method = LatitudeAdjustmentMethods.DEFAULT)
    {
        this.latitudeAdjustmentMethod = method;
    }

    private double Evaluate(string value)
    {
        if (double.TryParse(value, out double doubleVal))
            return doubleVal;

        string result = string.Empty;
        for (int i = 0; i < value.Length; i++)
        {
            if (double.TryParse(value[i].ToString(), out doubleVal))
            {
                result += doubleVal;
            }
        }
        return Convert.ToDouble(result);
    }

    private double RiseSetAngle()
    {
        double angle = 0.0347 * Math.Sqrt(elevation); // an approximation
        return 0.833 + angle;
    }

    private int AsrFactor()
    {
        if (asrShadowFactor.HasValue)
            return asrShadowFactor.Value;
        return (short)school;
        //if (school == School.STANDARD)
        //    return 1;
        //else if (school == School.HANAFI)
        //    return 2;
        //else
        //    return 0;
    }

    public static double ToJulianDate(DateTime date)
    {
        return date.ToOADate() + 2415018.5;
    }

    public Hashtable Now(
        double latitude, 
        double longitude,
        TimeZoneInfo? timeZone = null, 
        double? elevation = null,
        LatitudeAdjustmentMethods latitudeAdjustmentMethod = LatitudeAdjustmentMethods.DEFAULT, 
        MidnightModes midnightMode = MidnightModes.DEFAULT, 
        TimeFormats format = TimeFormats.DEFAULT)
    {
        return GetTimes(latitude, longitude, DateTime.Now, timeZone, elevation, latitudeAdjustmentMethod, midnightMode, format);
    }

    public Hashtable Tomorrow(
        double latitude, 
        double longitude,
        TimeZoneInfo? timeZone = null, 
        double? elevation = null,
        LatitudeAdjustmentMethods latitudeAdjustmentMethod = LatitudeAdjustmentMethods.DEFAULT, 
        MidnightModes midnightMode = MidnightModes.DEFAULT, 
        TimeFormats format = TimeFormats.DEFAULT)
    {
        return GetTimes(latitude, longitude, DateTime.Now.AddDays(1), timeZone, elevation, latitudeAdjustmentMethod, midnightMode, format);
    }

    public Hashtable GetTimes(
        double latitude, 
        double longitude, 
        DateTime? dateTime = null,
        TimeZoneInfo? timeZone = null,
        double? elevation = null,
        LatitudeAdjustmentMethods latitudeAdjustmentMethod = LatitudeAdjustmentMethods.DEFAULT, 
        MidnightModes midnightMode = MidnightModes.DEFAULT, 
        TimeFormats format = TimeFormats.DEFAULT)
    {
        this.latitude = latitude;
        this.longitude = longitude;
        this.elevation = elevation == null ? 0 : 1 * elevation.Value;

        SetTimeFormat(format);
        SetLatitudeAdjustmentMethod(latitudeAdjustmentMethod);
        SetMidnightMode(midnightMode);

        SetDate(dateTime);
        SetTimeZone(timeZone);

        return ComputeTimes();
    }

    private void SetDate(DateTime? date)
    {
        this.date = date ?? DateTime.Now;
    }

    private void SetTimeZone(TimeZoneInfo? tz)
    {
        timeZone = (tz ?? TimeZoneInfo.Local).GetUtcOffset(date).TotalHours;
    }

    private Hashtable ComputeTimes()
    {
        var times = new Hashtable()
        {
            [IMSAK] = 5d,
            [FAJR] = 5d,
            [SUNRISE] = 6d,
            [ZHUHR] = 12d,
            [ASR] = 13d,
            [SUNSET] = 18d,
            [MAGHRIB] = 18d,
            [ISHA] = 18d
        };

        times = ComputePrayerTimes(times);
        times = AdjustTimes(times);

        if (midnightMode == MidnightModes.JAFARI)
            times[MIDNIGHT] = Convert.ToDouble(times[SUNSET]) + TimeDiff(Convert.ToDouble(times[SUNSET]), Convert.ToDouble(times[FAJR])) / 2d;
        else
            times[MIDNIGHT] = Convert.ToDouble(times[SUNSET]) + TimeDiff(Convert.ToDouble(times[SUNSET]), Convert.ToDouble(times[SUNRISE])) / 2d;

        if (method == PrayerCalculationMethods.MOONSIGHTING)
            times = MoonsightingRecalculation(times);

        times = TuneTimes(times);
        times = ModifyFormats(times);
        return times;
    }

    private Hashtable ModifyFormats(Hashtable times)
    {
        List<string> keys = GetKeys(times);
        foreach (string key in keys)
        {
            times[key] = GetFormattedTime(Convert.ToDouble(times[key]), timeFormat);
        }

        return times;
    }

    private Hashtable TuneTimes(Hashtable times)
    {
        if (tuneTimesOffset != null && tuneTimesOffset.Count > 0)
        {
            List<string> keys = GetKeys(times);
            foreach (string key in keys)
            {
                if (tuneTimesOffset.ContainsKey(key))
                {
                    times[key] = Convert.ToDouble(times[key]) + Convert.ToDouble(tuneTimesOffset[key]) / 60d;
                }

            }
        }
        return times;
    }

    private string GetFormattedTime(double time, TimeFormats format)
    {
        if (double.IsNaN(time))
            return INVALID_TIME;
        if (format == TimeFormats.TIME_FORMAT_FLOAT)
            return time.ToString();
        string[] suffixes = ["AM", "PM"];
        time = DMath.FixHour(time + 0.5 / 60d);

        double hours = Math.Floor(time);
        double minutes = Math.Floor((time - hours) * 60d);
        string suffix = (timeFormat == TimeFormats.TIME_FORMAT_12H) ? suffixes[hours < 12 ? 0 : 1] : string.Empty;
        string hour = (format == TimeFormats.TIME_FORMAT_24H) ? TwoDigitsFormat(hours) : ((hours + 12 - 1) % 12 + 1).ToString();
        string twoDigitMinutes = TwoDigitsFormat(minutes);

        return $"{hour}:{twoDigitMinutes}" + string.Format("{0}", !string.IsNullOrEmpty(suffix) ? " " + suffix : string.Empty);
    }

    public string TwoDigitsFormat(double num) => (num < 10) ? "0" + num : num + "";
    private Hashtable MoonsightingRecalculation(Hashtable times)
    {
        var fajrMS = new Fajr(date, latitude);
        times[FAJR] = Convert.ToDouble(times[SUNRISE]) - (fajrMS.GetMinutesBeforeSunrise() / 60d);

        if (IsMin(settings[IMSAK].ToString()))
            times[IMSAK] = Convert.ToDouble(times[FAJR]) - Evaluate(settings[IMSAK].ToString()) / 60d;

        var ishaMS = new Isha(date, latitude, shafaq);
        times[ISHA] = Convert.ToDouble(times[SUNSET]) + (ishaMS.GetMinutesAfterSunset() / 60d);

        return times;
    }

    private List<string> GetKeys(Hashtable times)
    {
        List<string> keys = new List<string>();
        foreach (DictionaryEntry time in times)
            keys.Add(time.Key.ToString());
        return keys;
    }

    private Hashtable AdjustTimes(Hashtable times)
    {
        List<string> keys = GetKeys(times);
        foreach (string key in keys)
            times[key] = (double)times[key] + timeZone - (longitude / 15);

        if (latitudeAdjustmentMethod != LatitudeAdjustmentMethods.LATITUDE_ADJUSTMENT_METHOD_NONE)
        {
            times = AdjustHighLatitudes(times);
        }
        if (IsMin(settings[IMSAK].ToString()))
            times[IMSAK] = Convert.ToDouble(times[FAJR]) - Evaluate(settings[IMSAK].ToString()) / 60;
        if (IsMin(settings[MAGHRIB].ToString()))
            times[MAGHRIB] = Convert.ToDouble(times[SUNSET]) + Evaluate(settings[MAGHRIB].ToString()) / 60;
        if (IsMin(settings[ISHA].ToString()))
            times[ISHA] = Convert.ToDouble(times[MAGHRIB]) + Evaluate(settings[ISHA].ToString()) / 60;
        times[ZHUHR] = Convert.ToDouble(times[ZHUHR]) + Evaluate(settings[ZHUHR].ToString()) / 60;

        return times;
    }

    private Hashtable AdjustHighLatitudes(Hashtable times)
    {
        double nightTime = TimeDiff((double)times[SUNSET], (double)times[SUNRISE]);
        times[IMSAK] = AdjustHLTime((double)times[IMSAK], (double)times[SUNRISE], Evaluate(settings[IMSAK].ToString()), nightTime, "ccw");
        times[FAJR] = AdjustHLTime((double)times[FAJR], (double)times[SUNRISE], Evaluate(settings[FAJR].ToString()), nightTime, "ccw");
        times[ISHA] = AdjustHLTime((double)times[ISHA], (double)times[SUNSET], Evaluate(settings[ISHA].ToString()), nightTime);
        times[MAGHRIB] = AdjustHLTime((double)times[MAGHRIB], (double)times[SUNSET], Evaluate(settings[MAGHRIB].ToString()), nightTime);
        return times;
    }

    private double AdjustHLTime(double time, double baseTime, double angle, double night, string direction = null)
    {
        double portion = NightPortion(angle, night);
        double diff = (direction == "ccw") ? TimeDiff(time, baseTime) : TimeDiff(baseTime, time);
        if (double.IsNaN(time) || diff > portion)
            time = baseTime + (direction == "ccw" ? (-portion) : portion);
        return time;
    }

    private double NightPortion(double angle, double night)
    {
        LatitudeAdjustmentMethods method = latitudeAdjustmentMethod;
        double portion = 1d / 2d;
        if (method == LatitudeAdjustmentMethods.LATITUDE_ADJUSTMENT_METHOD_ANGLE)
            portion = 1d / 60d * angle;
        if (method == LatitudeAdjustmentMethods.LATITUDE_ADJUSTMENT_METHOD_ONESEVENTH)
            portion = 1d / 7d;
        return portion * night;
    }

    private bool IsMin(string str)
    {
        if (str.IndexOf("min") > -1)
            return true;
        return false;
    }

    private double TimeDiff(double c1, double c2)
    {
        return DMath.FixHour(c2 - c1); ;
    }

    private Hashtable ComputePrayerTimes(Hashtable times)
    {
        times = DayPortion(times);
        double imsak = SunAngleTime(Evaluate(settings[IMSAK].ToString()), (double)times[IMSAK], "ccw");
        double sunrise = SunAngleTime(RiseSetAngle(), (double)times[SUNRISE], "ccw");
        double fajr = SunAngleTime(Evaluate(settings[FAJR].ToString()), (double)times[FAJR], "ccw");
        double dhuhr = MidDay((double)times[ZHUHR]);
        double asr = AsrTime(AsrFactor(), (double)times[ASR]);
        double sunset = SunAngleTime(RiseSetAngle(), (double)times[SUNSET]);
        double maghrib = IsMin(settings[MAGHRIB].ToString()) ? sunset : SunAngleTime(Evaluate(settings[MAGHRIB].ToString()), (double)times[MAGHRIB]);
        //double maghrib = sunset;// SunAngleTime(Evaluate(settings[MAGHRIB].ToString()), (double)times[MAGHRIB]);
        double isha = SunAngleTime(Evaluate(settings[ISHA].ToString()), (double)times[ISHA]);
        return new Hashtable()
        {
            [FAJR] = fajr,
            [SUNRISE] = sunrise,
            [ZHUHR] = dhuhr,
            [ASR] = asr,
            [SUNSET] = sunset,
            [MAGHRIB] = maghrib,
            [ISHA] = isha,
            [IMSAK] = imsak,
        };
    }

    public Hashtable DayPortion(Hashtable times)
    {
        List<string> keys = GetKeys(times);
        foreach (string key in keys)
            times[key] = (double)times[key] / 24;

        return times;
    }

    private double SunAngleTime(double angle, double time, string direction = null)
    {
        double julianDate = JulianDate(date.Year, date.Month, date.Day) - longitude / (15 * 24);
        double dec1 = SunPosition(julianDate + time).Declination;
        double noon = MidDay(time);
        double p1 = -DMath.Sin(angle) - DMath.Sin(dec1) * DMath.Sin(latitude);
        double p2 = DMath.Cos(dec1) * DMath.Cos(latitude);
        double cosRange = (p1 / p2);
        if (cosRange > 1)
            cosRange = 1;
        if (cosRange < -1)
            cosRange = -1;
        double t = (1d / 15d) * DMath.Arccos(cosRange);

        return noon + (direction == "ccw" ? -t : t);
    }

    public double JulianDate(int year, int month, int day)
    {
        if (month <= 2)
        {
            year -= 1;
            month += 12;
        }
        double A = (double)Math.Floor(year / 100.0);
        double B = 2 - A + Math.Floor(A / 4);

        double JD = Math.Floor(365.25 * (year + 4716)) + Math.Floor(30.6001 * (month + 1)) + day + B - 1524.5;
        return JD;
    }

    private double MidDay(double time)
    {
        double julianDate = JulianDate(date.Year, date.Month, date.Day) - longitude / (15 * 24);
        double eqt = SunPosition(julianDate + time).Equation;
        double noon = DMath.FixHour(12 - eqt);
        return noon;
    }

    public Sun SunPosition(double jd)
    {
        double D = jd - 2451545.0;
        double g = DMath.FixAngle(357.529 + 0.98560028 * D);
        double q = DMath.FixAngle(280.459 + 0.98564736 * D);
        double L = DMath.FixAngle(q + 1.915 * DMath.Sin(g) + 0.020 * DMath.Sin(2 * g));

        double R = 1.00014 - 0.01671 * DMath.Cos(g) - 0.00014 * DMath.Cos(2 * g);
        double e = 23.439 - 0.00000036 * D;

        double RA = DMath.Arctan2(DMath.Cos(e) * DMath.Sin(L), DMath.Cos(L)) / 15;
        double eqt = q / 15 - DMath.FixHour(RA);
        double dec1 = DMath.Arcsin(DMath.Sin(e) * DMath.Sin(L));

        return new Sun() { Declination = dec1, Equation = eqt };
    }

    private double AsrTime(int factor, double time)
    {
        double julianDate = JulianDate(date.Year, date.Month, date.Day) - longitude / (15 * 24);
        double dec1 = SunPosition(julianDate + time).Declination;
        double angle = -DMath.Arccot(factor + DMath.Tan(Math.Abs(latitude - dec1)));
        return SunAngleTime(angle, time);
    }
}