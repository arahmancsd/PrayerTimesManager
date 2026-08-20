using PrayerTimesManager.Enums;
using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PrayerTimesManager;

public partial class PrayerTimes
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

    public const string INVALID_TIME = "-----";

    public Dictionary<string, PrayerCalculationMethod> CalculationMethodList { get; private set; }
    public Hashtable CalculationMethodCodeList { get; private set; }
    
    private const string MinSuffix = "min";
    private const string ClockwiseDirection = "ccw";

    private DateTimeOffset _date;
    private PrayerCalculationMethods _method = PrayerCalculationMethods.MWL;
    private Schools _school = Schools.DEFAULT;
    private MidnightModes _midnightMode = MidnightModes.DEFAULT;
    private LatitudeAdjustmentMethods _latitudeAdjustmentMethod;
    private Shafaq _shafaq = Isha.shafaq; // Only valid for METHOD_MOONSIGHTING
    private TimeFormats _timeFormat;
    private double _latitude;
    private double _longitude;
    private double _elevation;
    private readonly int? _asrShadowFactor;
    private Dictionary<string, string> _settings;
    private double _timeZone;
    private Dictionary<string, double> _tuneTimesOffset;

    private readonly TimeProvider _timeProvider;

    public PrayerTimes(
        PrayerCalculationMethods method = PrayerCalculationMethods.DEFAULT,
        Schools school = Schools.DEFAULT,
        int? asrShadowFactor = null,
        TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;

        SetPrayerCalculationMethods();

        _method = method;
        _school = school;
        _asrShadowFactor = asrShadowFactor;

        LoadSettings();
    }

    private void LoadSettings()
    {
        _settings = new Dictionary<string, string>
        {
            [IMSAK] = CalculationMethodList[_method.ToString()]?.Param[IMSAK]?.ToString() ?? "10 min",
            [FAJR] = CalculationMethodList[_method.ToString()]?.Param[FAJR]?.ToString() ?? "0",
            [ZHUHR] = CalculationMethodList[_method.ToString()]?.Param[ZHUHR]?.ToString() ?? "0 min",
            [ISHA] = CalculationMethodList[_method.ToString()]?.Param[ISHA]?.ToString() ?? "0",
            [MAGHRIB] = CalculationMethodList[_method.ToString()]?.Param[MAGHRIB]?.ToString() ?? "0 min",
        };

        var isMidnight = CalculationMethodList[_method.ToString()]?.Param[MIDNIGHT] != null;

        if (isMidnight && (MidnightModes)CalculationMethodList[_method.ToString()].Param[MIDNIGHT] == MidnightModes.JAFARI)
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
        CalculationMethodList = PrayerCalculation.PrayerCalculations;
        CalculationMethodCodeList = PrayerCalculation.PrayerCalculationsCodes;
    }

    public void SetMethod(PrayerCalculationMethods method = PrayerCalculationMethods.MWL)
    {
        _method = method;
        LoadSettings();
    }

    public void SetCustomMethod(PrayerCalculationMethod method)
    {
        SetMethod(PrayerCalculationMethods.CUSTOM);
        CalculationMethodList[_method.ToString()] = method;
        LoadSettings();
    }

    public void SetTuneTimeOffset(Dictionary<string, double> offset)
    {
        _tuneTimesOffset = offset;
    }

    public void SetShafaq(Shafaq shafaq)
    {
        _shafaq = shafaq;
    }

    public void SetSchool(Schools school)
    {
        SetAsrJuristicMethod(school);
    }

    public void SetAsrJuristicMethod(Schools school)
    {
        _school = school;
    }

    public void SetMidnightMode(MidnightModes mode = MidnightModes.STANDARD)
    {
        _midnightMode = mode;
    }

    public void SetTimeFormat(TimeFormats format = TimeFormats.DEFAULT)
    {
        _timeFormat = format;
    }

    public void SetLatitudeAdjustmentMethod(LatitudeAdjustmentMethods method = LatitudeAdjustmentMethods.DEFAULT)
    {
        _latitudeAdjustmentMethod = method;
    }

    private static double Evaluate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0;

        // Strip the "min" suffix so "10 min" and "10.5 min" parse cleanly.
        string cleaned = value.Replace(MinSuffix, "", StringComparison.OrdinalIgnoreCase).Trim();

        if (double.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
            return result;

        // Fallback: extract the leading signed decimal number (e.g. "18.5°" -> 18.5).
        Match match = EvaulateRegEx().Match(cleaned);
        if (match.Success && double.TryParse(match.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
            return result;

        return 0;
    }

    private double RiseSetAngle()
    {
        double angle = 0.0347 * Math.Sqrt(_elevation); // an approximation
        return 0.833 + angle;
    }

    private int AsrFactor()
    {
        if (_asrShadowFactor.HasValue)
            return _asrShadowFactor.Value;
        return (int)_school;
    }

    public static double ToJulianDate(DateTimeOffset date)
    {
        return date.DateTime.ToOADate() + 2415018.5;
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
        return GetTimes(latitude, longitude, _timeProvider.GetLocalNow(), timeZone, elevation, latitudeAdjustmentMethod, midnightMode, format);
    }

    public Hashtable Now(PrayerTimesInputs inputs) =>
        GetTimes(
            inputs.Latitude,
            inputs.Longitude,
            _timeProvider.GetLocalNow(),
            inputs.TimeZone,
            inputs.Elevation,
            inputs.LatitudeAdjustmentMethod,
            inputs.MidnightMode,
            inputs.Format);

    public Hashtable Tomorrow(
        double latitude,
        double longitude,
        TimeZoneInfo? timeZone = null,
        double? elevation = null,
        LatitudeAdjustmentMethods latitudeAdjustmentMethod = LatitudeAdjustmentMethods.DEFAULT,
        MidnightModes midnightMode = MidnightModes.DEFAULT,
        TimeFormats format = TimeFormats.DEFAULT)
    {
        return GetTimes(latitude, longitude, _timeProvider.GetLocalNow().AddDays(1), timeZone, elevation, latitudeAdjustmentMethod, midnightMode, format);
    }

    public Hashtable Tomorrow(PrayerTimesInputs inputs) =>
        GetTimes(
            inputs.Latitude,
            inputs.Longitude,
            _timeProvider.GetLocalNow().AddDays(1),
            inputs.TimeZone,
            inputs.Elevation,
            inputs.LatitudeAdjustmentMethod,
            inputs.MidnightMode,
            inputs.Format);

    public Hashtable GetTimes(
        double latitude,
        double longitude,
        DateTimeOffset? dateTime = null,
        TimeZoneInfo? timeZone = null,
        double? elevation = null,
        LatitudeAdjustmentMethods latitudeAdjustmentMethod = LatitudeAdjustmentMethods.DEFAULT,
        MidnightModes midnightMode = MidnightModes.DEFAULT,
        TimeFormats format = TimeFormats.DEFAULT)
    {
        _latitude = latitude;
        _longitude = longitude;
        _elevation = elevation == null ? 0 : 1 * elevation.Value;

        SetTimeFormat(format);
        SetLatitudeAdjustmentMethod(latitudeAdjustmentMethod);
        SetMidnightMode(midnightMode);

        SetDate(dateTime);
        SetTimeZone(timeZone);

        return ComputeTimes();
    }

    public Hashtable GetTimes(PrayerTimesInputs inputs) =>
        GetTimes(
            inputs.Latitude,
            inputs.Longitude,
            inputs.DateTime,
            inputs.TimeZone,
            inputs.Elevation,
            inputs.LatitudeAdjustmentMethod,
            inputs.MidnightMode,
            inputs.Format);

    public PrayerTimesResult GetTimesResult(
        double latitude,
        double longitude,
        DateTimeOffset? dateTime = null,
        TimeZoneInfo? timeZone = null,
        double? elevation = null,
        LatitudeAdjustmentMethods latitudeAdjustmentMethod = LatitudeAdjustmentMethods.DEFAULT,
        MidnightModes midnightMode = MidnightModes.DEFAULT,
        TimeFormats format = TimeFormats.DEFAULT)
    {
        Hashtable times = GetTimes(latitude, longitude, dateTime, timeZone, elevation, latitudeAdjustmentMethod, midnightMode, format);
        return ToPrayerTimesResult(times);
    }

    public PrayerTimesResult GetTimesResult(PrayerTimesInputs inputs) =>
        GetTimesResult(
            inputs.Latitude,
            inputs.Longitude,
            inputs.DateTime,
            inputs.TimeZone,
            inputs.Elevation,
            inputs.LatitudeAdjustmentMethod,
            inputs.MidnightMode,
            inputs.Format);

    public PrayerTimesResult NowResult(
        double latitude,
        double longitude,
        TimeZoneInfo? timeZone = null,
        double? elevation = null,
        LatitudeAdjustmentMethods latitudeAdjustmentMethod = LatitudeAdjustmentMethods.DEFAULT,
        MidnightModes midnightMode = MidnightModes.DEFAULT,
        TimeFormats format = TimeFormats.DEFAULT)
    {
        return GetTimesResult(latitude, longitude, _timeProvider.GetLocalNow(), timeZone, elevation, latitudeAdjustmentMethod, midnightMode, format);
    }

    public PrayerTimesResult NowResult(PrayerTimesInputs inputs) =>
        GetTimesResult(
            inputs.Latitude,
            inputs.Longitude,
            _timeProvider.GetLocalNow(),
            inputs.TimeZone,
            inputs.Elevation,
            inputs.LatitudeAdjustmentMethod,
            inputs.MidnightMode,
            inputs.Format);

    public PrayerTimesResult TomorrowResult(
        double latitude,
        double longitude,
        TimeZoneInfo? timeZone = null,
        double? elevation = null,
        LatitudeAdjustmentMethods latitudeAdjustmentMethod = LatitudeAdjustmentMethods.DEFAULT,
        MidnightModes midnightMode = MidnightModes.DEFAULT,
        TimeFormats format = TimeFormats.DEFAULT)
    {
        return GetTimesResult(latitude, longitude, _timeProvider.GetLocalNow().AddDays(1), timeZone, elevation, latitudeAdjustmentMethod, midnightMode, format);
    }

    public PrayerTimesResult TomorrowResult(PrayerTimesInputs inputs) =>
        GetTimesResult(
            inputs.Latitude,
            inputs.Longitude,
            _timeProvider.GetLocalNow().AddDays(1),
            inputs.TimeZone,
            inputs.Elevation,
            inputs.LatitudeAdjustmentMethod,
            inputs.MidnightMode,
            inputs.Format);

    private PrayerTimesResult ToPrayerTimesResult(Hashtable times) => new(
        times[IMSAK]?.ToString() ?? INVALID_TIME,
        times[FAJR]?.ToString() ?? INVALID_TIME,
        times[SUNRISE]?.ToString() ?? INVALID_TIME,
        times[ZHUHR]?.ToString() ?? INVALID_TIME,
        times[ASR]?.ToString() ?? INVALID_TIME,
        times[SUNSET]?.ToString() ?? INVALID_TIME,
        times[MAGHRIB]?.ToString() ?? INVALID_TIME,
        times[ISHA]?.ToString() ?? INVALID_TIME,
        times[MIDNIGHT]?.ToString() ?? INVALID_TIME);

    private void SetDate(DateTimeOffset? date)
    {
        _date = date ?? _timeProvider.GetLocalNow();
    }

    private void SetTimeZone(TimeZoneInfo? tz)
    {
        if (tz != null)
        {
            _timeZone = tz.GetUtcOffset(_date).TotalHours;
        }
        else
        {
            _timeZone = _date.Offset.TotalHours;
        }
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

        if (_midnightMode == MidnightModes.JAFARI)
            times[MIDNIGHT] = Convert.ToDouble(times[SUNSET]) + TimeDiff(Convert.ToDouble(times[SUNSET]), Convert.ToDouble(times[FAJR])) / 2d;
        else
            times[MIDNIGHT] = Convert.ToDouble(times[SUNSET]) + TimeDiff(Convert.ToDouble(times[SUNSET]), Convert.ToDouble(times[SUNRISE])) / 2d;

        if (_method == PrayerCalculationMethods.MOONSIGHTING)
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
            times[key] = GetFormattedTime(Convert.ToDouble(times[key]), _timeFormat);
        }

        return times;
    }

    private Hashtable TuneTimes(Hashtable times)
    {
        if (_tuneTimesOffset != null && _tuneTimesOffset.Count > 0)
        {
            List<string> keys = GetKeys(times);
            foreach (string key in keys)
            {
                if (_tuneTimesOffset.ContainsKey(key))
                {
                    times[key] = Convert.ToDouble(times[key]) + _tuneTimesOffset[key] / 60d;
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
        string suffix = (_timeFormat == TimeFormats.TIME_FORMAT_12H) ? suffixes[hours < 12 ? 0 : 1] : string.Empty;
        string hour = (format == TimeFormats.TIME_FORMAT_24H || format == TimeFormats.TIME_FORMAT_ISO8601) ? TwoDigitsFormat(hours) : ((hours + 12 - 1) % 12 + 1).ToString();
        string twoDigitMinutes = TwoDigitsFormat(minutes);

        if (format == TimeFormats.TIME_FORMAT_ISO8601)
            return $"{hour}:{twoDigitMinutes}:00";

        return $"{hour}:{twoDigitMinutes}" + string.Format("{0}", !string.IsNullOrEmpty(suffix) ? " " + suffix : string.Empty);
    }

    public string TwoDigitsFormat(double num) => (num < 10) ? "0" + num : num + "";
    private Hashtable MoonsightingRecalculation(Hashtable times)
    {
        var fajrMS = new Fajr(_date.DateTime, _latitude);
        times[FAJR] = Convert.ToDouble(times[SUNRISE]) - (fajrMS.GetMinutesBeforeSunrise() / 60d);

        if (IsMin(_settings[IMSAK]))
            times[IMSAK] = Convert.ToDouble(times[FAJR]) - Evaluate(_settings[IMSAK]) / 60d;

        var ishaMS = new Isha(_date.DateTime, _latitude, _shafaq);
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
            times[key] = (double)times[key] + _timeZone - (_longitude / 15);

        if (_latitudeAdjustmentMethod != LatitudeAdjustmentMethods.LATITUDE_ADJUSTMENT_METHOD_NONE)
        {
            times = AdjustHighLatitudes(times);
        }
        if (IsMin(_settings[IMSAK]))
            times[IMSAK] = Convert.ToDouble(times[FAJR]) - Evaluate(_settings[IMSAK]) / 60;
        if (IsMin(_settings[MAGHRIB]))
            times[MAGHRIB] = Convert.ToDouble(times[SUNSET]) + Evaluate(_settings[MAGHRIB]) / 60;
        if (IsMin(_settings[ISHA]))
            times[ISHA] = Convert.ToDouble(times[MAGHRIB]) + Evaluate(_settings[ISHA]) / 60;
        times[ZHUHR] = Convert.ToDouble(times[ZHUHR]) + Evaluate(_settings[ZHUHR]) / 60;

        return times;
    }

    private Hashtable AdjustHighLatitudes(Hashtable times)
    {
        double nightTime = TimeDiff((double)times[SUNSET], (double)times[SUNRISE]);
        if (!IsMin(_settings[IMSAK]))
            times[IMSAK] = AdjustHLTime((double)times[IMSAK], (double)times[SUNRISE], Evaluate(_settings[IMSAK]), nightTime, ClockwiseDirection);
        times[FAJR] = AdjustHLTime((double)times[FAJR], (double)times[SUNRISE], Evaluate(_settings[FAJR]), nightTime, ClockwiseDirection);
        if (!IsMin(_settings[ISHA]))
            times[ISHA] = AdjustHLTime((double)times[ISHA], (double)times[SUNSET], Evaluate(_settings[ISHA]), nightTime);
        if (!IsMin(_settings[MAGHRIB]))
            times[MAGHRIB] = AdjustHLTime((double)times[MAGHRIB], (double)times[SUNSET], Evaluate(_settings[MAGHRIB]), nightTime);
        return times;
    }

    private double AdjustHLTime(double time, double baseTime, double angle, double night, string direction = null)
    {
        double portion = NightPortion(angle, night);
        double diff = (direction == ClockwiseDirection) ? TimeDiff(time, baseTime) : TimeDiff(baseTime, time);
        if (double.IsNaN(time) || diff > portion)
            time = baseTime + (direction == ClockwiseDirection ? (-portion) : portion);
        return time;
    }

    private double NightPortion(double angle, double night)
    {
        LatitudeAdjustmentMethods method = _latitudeAdjustmentMethod;
        double portion = 1d / 2d;
        if (method == LatitudeAdjustmentMethods.LATITUDE_ADJUSTMENT_METHOD_ANGLE)
            portion = 1d / 60d * angle;
        if (method == LatitudeAdjustmentMethods.LATITUDE_ADJUSTMENT_METHOD_ONESEVENTH)
            portion = 1d / 7d;
        return portion * night;
    }

    private bool IsMin(string str) =>
        !string.IsNullOrEmpty(str) && str.IndexOf(MinSuffix, StringComparison.OrdinalIgnoreCase) > -1;

    private double TimeDiff(double c1, double c2)
    {
        return DMath.FixHour(c2 - c1); ;
    }

    private Hashtable ComputePrayerTimes(Hashtable times)
    {
        times = DayPortion(times);
        double imsak = SunAngleTime(Evaluate(_settings[IMSAK]), (double)times[IMSAK], ClockwiseDirection);
        double sunrise = SunAngleTime(RiseSetAngle(), (double)times[SUNRISE], ClockwiseDirection);
        double fajr = SunAngleTime(Evaluate(_settings[FAJR]), (double)times[FAJR], ClockwiseDirection);
        double dhuhr = MidDay((double)times[ZHUHR]);
        double asr = AsrTime(AsrFactor(), (double)times[ASR]);
        double sunset = SunAngleTime(RiseSetAngle(), (double)times[SUNSET]);
        double maghrib = IsMin(_settings[MAGHRIB]) ? sunset : SunAngleTime(Evaluate(_settings[MAGHRIB]), (double)times[MAGHRIB]);
        double isha = SunAngleTime(Evaluate(_settings[ISHA]), (double)times[ISHA]);
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
        double julianDate = JulianDate(_date.Year, _date.Month, _date.Day) - _longitude / (15 * 24);
        double dec1 = SunPosition(julianDate + time).Declination;
        double noon = MidDay(time);
        double p1 = -DMath.Sin(angle) - DMath.Sin(dec1) * DMath.Sin(_latitude);
        double p2 = DMath.Cos(dec1) * DMath.Cos(_latitude);
        double cosRange = (p1 / p2);
        if (cosRange > 1)
            cosRange = 1;
        if (cosRange < -1)
            cosRange = -1;
        double t = (1d / 15d) * DMath.Arccos(cosRange);

        return noon + (direction == ClockwiseDirection ? -t : t);
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
        double julianDate = JulianDate(_date.Year, _date.Month, _date.Day) - _longitude / (15 * 24);
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
        double julianDate = JulianDate(_date.Year, _date.Month, _date.Day) - _longitude / (15 * 24);
        double dec1 = SunPosition(julianDate + time).Declination;
        double angle = -DMath.Arccot(factor + DMath.Tan(Math.Abs(_latitude - dec1)));
        return SunAngleTime(angle, time);
    }

    [GeneratedRegex(@"^-?\d+(\.\d+)?")]
    private static partial Regex EvaulateRegEx();
}