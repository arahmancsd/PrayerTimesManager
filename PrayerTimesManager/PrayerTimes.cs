using PrayerTimesManager.Enums;
using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PrayerTimesManager;

/// <summary>
/// Calculates Islamic prayer times (Imsak, Fajr, Sunrise, Dhuhr, Asr, Sunset, Maghrib, Isha, and Midnight)
/// for a given location and date, supporting multiple calculation methods, juristic schools,
/// latitude adjustment methods, and time formats.
/// </summary>
public partial class PrayerTimes
{
    /// <summary>The key used to identify the Imsak time.</summary>
    public const string IMSAK = "Imsak";
    /// <summary>The key used to identify the Fajr time.</summary>
    public const string FAJR = "Fajr";
    /// <summary>The key used to identify the Sunrise time.</summary>
    public const string SUNRISE = "Sunrise";
    /// <summary>The key used to identify the Dhuhr time.</summary>
    public const string ZHUHR = "Dhuhr";
    /// <summary>The key used to identify the Asr time.</summary>
    public const string ASR = "Asr";
    /// <summary>The key used to identify the Sunset time.</summary>
    public const string SUNSET = "Sunset";
    /// <summary>The key used to identify the Maghrib time.</summary>
    public const string MAGHRIB = "Maghrib";
    /// <summary>The key used to identify the Isha time.</summary>
    public const string ISHA = "Isha";
    /// <summary>The key used to identify the Midnight time.</summary>
    public const string MIDNIGHT = "Midnight";

    /// <summary>The placeholder value used to represent an invalid or unavailable time.</summary>
    public const string INVALID_TIME = "-----";

    private Dictionary<string, PrayerCalculationMethod> CalculationMethodList { get; set; } = [];

    /// <summary>
    /// Gets the currently active per-prayer time offsets, in minutes, applied after all other calculations.
    /// This includes any offsets set via <see cref="SetTuneTimeOffset"/> as well as the default offsets
    /// automatically applied for the <see cref="PrayerCalculationMethods.MOONSIGHTING"/> method
    /// (see <see cref="MoonsightingOffsets"/>).
    /// </summary>
    public IReadOnlyDictionary<string, double> TuneTimeOffsets => _tuneTimesOffset;
    
    private const string MinSuffix = "min";
    private const string ClockwiseDirection = "ccw";

    private DateTimeOffset _date;
    private PrayerCalculationMethods _method = PrayerCalculationMethods.MWL;
    private Schools _school = Schools.DEFAULT;
    private MidnightModes _midnightMode = MidnightModes.DEFAULT;
    private LatitudeAdjustmentMethods _latitudeAdjustmentMethod;
    private Shafaq _shafaq = Isha.shafaq; // Only valid for METHOD_MOONSIGHTING
    private MoonsightingMaghribType _moonsightingMaghribType = MoonsightingMaghribType.DEFAULT; // Only valid for METHOD_MOONSIGHTING
    private TimeFormats _timeFormat;
    private double _latitude;
    private double _longitude;
    private double _elevation;
    private readonly double? _asrShadowFactor;
    private Dictionary<string, string> _settings = [];
    private double _timeZone;
    private Dictionary<string, double> _tuneTimesOffset = [];
    private readonly HashSet<string> _autoTunedOffsetKeys = [];

    private readonly TimeProvider _timeProvider;

    /// <summary>Initializes a new instance of the <see cref="PrayerTimes"/> class.</summary>
    /// <param name="method">The calculation method used to determine the Fajr, Isha, and Maghrib parameters.</param>
    /// <param name="school">The juristic school used to determine the Asr shadow factor.</param>
    /// <param name="asrShadowFactor">An optional explicit Asr shadow factor overriding the one derived from <paramref name="school"/>.</param>
    /// <param name="timeProvider">An optional time provider used to obtain the current date/time. Defaults to <see cref="TimeProvider.System"/>.</param>
    public PrayerTimes(
        PrayerCalculationMethods method = PrayerCalculationMethods.DEFAULT,
        Schools school = Schools.DEFAULT,
        double? asrShadowFactor = null,
        TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;

        SetPrayerCalculationMethodList();

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

        if (_method == PrayerCalculationMethods.MOONSIGHTING)
            ApplyMoonsightingTuneOffsets();
        else
            ClearAutoTuneOffsets();
    }

    private void ApplyMoonsightingTuneOffsets()
    {
        SetAutoTuneOffset(ZHUHR, MoonsightingOffsets.ZuhrOffsetMinutes);
        SetAutoTuneOffset(MAGHRIB, MoonsightingOffsets.GetMaghribOffsetMinutes(_moonsightingMaghribType));
    }

    private void SetAutoTuneOffset(string key, double minutes)
    {
        _tuneTimesOffset[key] = minutes;
        _autoTunedOffsetKeys.Add(key);
    }

    private void ClearAutoTuneOffsets()
    {
        foreach (string key in _autoTunedOffsetKeys)
            _tuneTimesOffset.Remove(key);
        _autoTunedOffsetKeys.Clear();
    }

    private void SetPrayerCalculationMethodList()
    {
        CalculationMethodList = PrayerCalculation.PrayerCalculations;
    }

    /// <summary>Sets the calculation method used to determine the Fajr, Isha, and Maghrib parameters.</summary>
    /// <param name="method">The calculation method to use.</param>
    public void SetMethod(PrayerCalculationMethods method = PrayerCalculationMethods.MWL)
    {
        _method = method;
        LoadSettings();
    }

    /// <summary>Sets a custom calculation method, replacing the current <see cref="PrayerCalculationMethods.CUSTOM"/> entry.</summary>
    /// <param name="method">The custom calculation method definition to use.</param>
    public void SetCustomMethod(PrayerCalculationMethod method)
    {
        SetMethod(PrayerCalculationMethods.CUSTOM);
        CalculationMethodList[_method.ToString()] = method;
        LoadSettings();
    }

    /// <summary>
    /// Sets per-prayer time offsets, in minutes, applied after all other calculations. Entries are merged
    /// into any existing offsets (such as the defaults automatically applied for the
    /// <see cref="PrayerCalculationMethods.MOONSIGHTING"/> method), overriding the value for any matching key.
    /// </summary>
    /// <param name="offset">A dictionary mapping prayer time keys to their offset in minutes.</param>
    public void SetTuneTimeOffset(Dictionary<string, double> offset)
    {
        foreach ((string key, double minutes) in offset)
        {
            _tuneTimesOffset[key] = minutes;
            _autoTunedOffsetKeys.Remove(key);
        }
    }

    /// <summary>Sets the twilight color used to determine the Isha time under the Moonsighting calculation method.</summary>
    /// <param name="shafaq">The twilight color to use.</param>
    public void SetShafaq(Shafaq shafaq)
    {
        _shafaq = shafaq;
    }

    /// <summary>Sets the Maghrib sunset-offset convention used under the Moonsighting calculation method.</summary>
    /// <param name="maghribType">The Maghrib convention to use.</param>
    public void SetMoonsightingMaghribType(MoonsightingMaghribType maghribType)
    {
        _moonsightingMaghribType = maghribType;

        if (_method == PrayerCalculationMethods.MOONSIGHTING)
            SetAutoTuneOffset(MAGHRIB, MoonsightingOffsets.GetMaghribOffsetMinutes(_moonsightingMaghribType));
    }

    /// <summary>Sets the juristic school used to determine the Asr shadow factor.</summary>
    /// <param name="school">The juristic school to use.</param>
    public void SetSchool(Schools school)
    {
        SetAsrJuristicMethod(school);
    }

    /// <summary>Sets the juristic school used to determine the Asr shadow factor.</summary>
    /// <param name="school">The juristic school to use.</param>
    public void SetAsrJuristicMethod(Schools school)
    {
        _school = school;
    }

    /// <summary>Sets the method used to calculate the Islamic midnight time.</summary>
    /// <param name="mode">The midnight calculation mode to use.</param>
    public void SetMidnightMode(MidnightModes mode = MidnightModes.STANDARD)
    {
        _midnightMode = mode;
    }

    /// <summary>Sets the format used to render the calculated prayer times.</summary>
    /// <param name="format">The time format to use.</param>
    public void SetTimeFormat(TimeFormats format = TimeFormats.DEFAULT)
    {
        _timeFormat = format;
    }

    /// <summary>Sets the method used to adjust prayer times for high latitude locations.</summary>
    /// <param name="method">The latitude adjustment method to use.</param>
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

    private double AsrFactor()
    {
        if (_asrShadowFactor.HasValue)
            return _asrShadowFactor.Value;

        return _school switch
        {
            Schools.HANAFI => 2d,
            Schools.JAFARI => 4d / 7d,
            _ => 1d,
        };
    }

    /// <summary>Converts a <see cref="DateTimeOffset"/> to its equivalent Julian date.</summary>
    /// <param name="date">The date to convert.</param>
    /// <returns>The equivalent Julian date.</returns>
    public static double ToJulianDate(DateTimeOffset date)
    {
        return date.DateTime.ToOADate() + 2415018.5;
    }

    /// <summary>Calculates prayer times for the current date at the specified location.</summary>
    /// <param name="latitude">The latitude of the location, in degrees.</param>
    /// <param name="longitude">The longitude of the location, in degrees.</param>
    /// <param name="timeZone">The time zone of the location. Defaults to the local time zone if not specified.</param>
    /// <param name="elevation">The elevation of the location, in meters.</param>
    /// <param name="latitudeAdjustmentMethod">The method used to adjust prayer times for high latitude locations.</param>
    /// <param name="midnightMode">The method used to calculate the Islamic midnight time.</param>
    /// <param name="format">The format used to render the calculated prayer times.</param>
    /// <returns>A <see cref="Hashtable"/> mapping each prayer time key to its formatted value.</returns>
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

    /// <summary>Calculates prayer times for the current date using the specified inputs.</summary>
    /// <param name="inputs">The location and formatting inputs.</param>
    /// <returns>A <see cref="Hashtable"/> mapping each prayer time key to its formatted value.</returns>
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

    /// <summary>Calculates prayer times for tomorrow's date at the specified location.</summary>
    /// <param name="latitude">The latitude of the location, in degrees.</param>
    /// <param name="longitude">The longitude of the location, in degrees.</param>
    /// <param name="timeZone">The time zone of the location. Defaults to the local time zone if not specified.</param>
    /// <param name="elevation">The elevation of the location, in meters.</param>
    /// <param name="latitudeAdjustmentMethod">The method used to adjust prayer times for high latitude locations.</param>
    /// <param name="midnightMode">The method used to calculate the Islamic midnight time.</param>
    /// <param name="format">The format used to render the calculated prayer times.</param>
    /// <returns>A <see cref="Hashtable"/> mapping each prayer time key to its formatted value.</returns>
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

    /// <summary>Calculates prayer times for tomorrow's date using the specified inputs.</summary>
    /// <param name="inputs">The location and formatting inputs.</param>
    /// <returns>A <see cref="Hashtable"/> mapping each prayer time key to its formatted value.</returns>
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

    /// <summary>Calculates prayer times for the specified location and date.</summary>
    /// <param name="latitude">The latitude of the location, in degrees.</param>
    /// <param name="longitude">The longitude of the location, in degrees.</param>
    /// <param name="dateTime">The date for which prayer times should be calculated. Defaults to the current local date/time if not specified.</param>
    /// <param name="timeZone">The time zone of the location. Defaults to the offset of <paramref name="dateTime"/> if not specified.</param>
    /// <param name="elevation">The elevation of the location, in meters.</param>
    /// <param name="latitudeAdjustmentMethod">The method used to adjust prayer times for high latitude locations.</param>
    /// <param name="midnightMode">The method used to calculate the Islamic midnight time.</param>
    /// <param name="format">The format used to render the calculated prayer times.</param>
    /// <returns>A <see cref="Hashtable"/> mapping each prayer time key to its formatted value.</returns>
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

    /// <summary>Calculates prayer times using the specified inputs.</summary>
    /// <param name="inputs">The location, date, and formatting inputs.</param>
    /// <returns>A <see cref="Hashtable"/> mapping each prayer time key to its formatted value.</returns>
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

    /// <summary>Calculates prayer times for the specified location and date, returned as a strongly typed result.</summary>
    /// <param name="latitude">The latitude of the location, in degrees.</param>
    /// <param name="longitude">The longitude of the location, in degrees.</param>
    /// <param name="dateTime">The date for which prayer times should be calculated. Defaults to the current local date/time if not specified.</param>
    /// <param name="timeZone">The time zone of the location. Defaults to the offset of <paramref name="dateTime"/> if not specified.</param>
    /// <param name="elevation">The elevation of the location, in meters.</param>
    /// <param name="latitudeAdjustmentMethod">The method used to adjust prayer times for high latitude locations.</param>
    /// <param name="midnightMode">The method used to calculate the Islamic midnight time.</param>
    /// <param name="format">The format used to render the calculated prayer times.</param>
    /// <returns>The calculated prayer times.</returns>
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

    /// <summary>Calculates prayer times using the specified inputs, returned as a strongly typed result.</summary>
    /// <param name="inputs">The location, date, and formatting inputs.</param>
    /// <returns>The calculated prayer times.</returns>
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

    /// <summary>Calculates prayer times for the current date at the specified location, returned as a strongly typed result.</summary>
    /// <param name="latitude">The latitude of the location, in degrees.</param>
    /// <param name="longitude">The longitude of the location, in degrees.</param>
    /// <param name="timeZone">The time zone of the location. Defaults to the local time zone if not specified.</param>
    /// <param name="elevation">The elevation of the location, in meters.</param>
    /// <param name="latitudeAdjustmentMethod">The method used to adjust prayer times for high latitude locations.</param>
    /// <param name="midnightMode">The method used to calculate the Islamic midnight time.</param>
    /// <param name="format">The format used to render the calculated prayer times.</param>
    /// <returns>The calculated prayer times.</returns>
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

    /// <summary>Calculates prayer times for the current date using the specified inputs, returned as a strongly typed result.</summary>
    /// <param name="inputs">The location and formatting inputs.</param>
    /// <returns>The calculated prayer times.</returns>
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

    /// <summary>Calculates prayer times for tomorrow's date at the specified location, returned as a strongly typed result.</summary>
    /// <param name="latitude">The latitude of the location, in degrees.</param>
    /// <param name="longitude">The longitude of the location, in degrees.</param>
    /// <param name="timeZone">The time zone of the location. Defaults to the local time zone if not specified.</param>
    /// <param name="elevation">The elevation of the location, in meters.</param>
    /// <param name="latitudeAdjustmentMethod">The method used to adjust prayer times for high latitude locations.</param>
    /// <param name="midnightMode">The method used to calculate the Islamic midnight time.</param>
    /// <param name="format">The format used to render the calculated prayer times.</param>
    /// <returns>The calculated prayer times.</returns>
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

    /// <summary>Calculates prayer times for tomorrow's date using the specified inputs, returned as a strongly typed result.</summary>
    /// <param name="inputs">The location and formatting inputs.</param>
    /// <returns>The calculated prayer times.</returns>
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

    /// <summary>Formats a number as a two-digit string, zero-padding values less than 10.</summary>
    /// <param name="num">The number to format.</param>
    /// <returns>The formatted, two-digit string.</returns>
    private string TwoDigitsFormat(double num) => (num < 10) ? "0" + num : num + "";
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

    private static List<string> GetKeys(Hashtable times)
    {
        List<string> keys = [];
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

    private static double TimeDiff(double c1, double c2)
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

    /// <summary>Converts each time value in the hashtable from hours to a fractional portion of the day.</summary>
    /// <param name="times">A <see cref="Hashtable"/> mapping prayer time keys to values expressed in hours.</param>
    /// <returns>The same <see cref="Hashtable"/>, with each value converted to a fraction of a day.</returns>
    private static Hashtable DayPortion(Hashtable times)
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

    /// <summary>Computes the Julian date for the specified Gregorian calendar date.</summary>
    /// <param name="year">The year.</param>
    /// <param name="month">The month.</param>
    /// <param name="day">The day.</param>
    /// <returns>The equivalent Julian date.</returns>
    private static double JulianDate(int year, int month, int day)
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

    /// <summary>Computes the sun's declination and the equation of time for the given Julian date.</summary>
    /// <param name="jd">The Julian date.</param>
    /// <returns>A <see cref="Sun"/> instance describing the sun's position.</returns>
    private static Sun SunPosition(double jd)
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

    private double AsrTime(double factor, double time)
    {
        double julianDate = JulianDate(_date.Year, _date.Month, _date.Day) - _longitude / (15 * 24);
        double dec1 = SunPosition(julianDate + time).Declination;
        double angle = -DMath.Arccot(factor + DMath.Tan(Math.Abs(_latitude - dec1)));
        return SunAngleTime(angle, time);
    }

    [GeneratedRegex(@"^-?\d+(\.\d+)?")]
    private static partial Regex EvaulateRegEx();
}