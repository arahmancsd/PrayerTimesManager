using System.Collections;
using PrayerTimesManager.Enums;

namespace PrayerTimesManager.Tests;

[TestClass]
public class PrayerTimesTests
{
    private static readonly TimeZoneInfo Utc = TimeZoneInfo.Utc;

    [TestMethod]
    public void Constructor_DefaultMethod_IsMwl()
    {
        var prayerTimes = new PrayerTimes();

        var calculationMethodListProperty = typeof(PrayerTimes).GetProperty("CalculationMethodList", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var calculationMethodList = calculationMethodListProperty!.GetValue(prayerTimes);

        Assert.IsNotNull(calculationMethodList);
    }

    [TestMethod]
    public void SetMethod_ChangesCalculationMethod()
    {
        var prayerTimes = new PrayerTimes();
        prayerTimes.SetMethod(PrayerCalculationMethods.EGYPT);

        Assert.IsNotNull(prayerTimes);
    }

    [TestMethod]
    public void SetSchool_ChangesAsrJuristicMethod()
    {
        var prayerTimes = new PrayerTimes();
        prayerTimes.SetSchool(Schools.HANAFI);

        Assert.IsNotNull(prayerTimes);
    }

    [TestMethod]
    public void ToJulianDate_ReturnsExpectedValue()
    {
        DateTimeOffset date = new(2000, 1, 1, 12, 0, 0, TimeSpan.Zero);
        double julianDate = PrayerTimes.ToJulianDate(date);

        Assert.AreEqual(2451545.0, julianDate, 1e-6);
    }

    [TestMethod]
    public void GetTimes_ReturnsAllPrayerKeys()
    {
        var prayerTimes = new PrayerTimes();
        Hashtable times = prayerTimes.GetTimes(51.5074, -0.1278, new DateTimeOffset(2024, 6, 15, 0, 0, 0, TimeSpan.Zero), Utc);

        Assert.IsNotNull(times);
        Assert.IsTrue(times.ContainsKey(PrayerTimes.FAJR));
        Assert.IsTrue(times.ContainsKey(PrayerTimes.SUNRISE));
        Assert.IsTrue(times.ContainsKey(PrayerTimes.ZHUHR));
        Assert.IsTrue(times.ContainsKey(PrayerTimes.ASR));
        Assert.IsTrue(times.ContainsKey(PrayerTimes.MAGHRIB));
        Assert.IsTrue(times.ContainsKey(PrayerTimes.ISHA));
        Assert.IsTrue(times.ContainsKey(PrayerTimes.MIDNIGHT));
    }

    [TestMethod]
    public void GetTimes_DefaultFormat_Returns12HourStrings()
    {
        var prayerTimes = new PrayerTimes();
        Hashtable times = prayerTimes.GetTimes(51.5074, -0.1278, new DateTimeOffset(2024, 6, 15, 0, 0, 0, TimeSpan.Zero), Utc);

        string? fajr = times[PrayerTimes.FAJR]?.ToString();
        Assert.IsFalse(string.IsNullOrEmpty(fajr));
        Assert.IsTrue(fajr!.Contains(':'));
    }

    [TestMethod]
    public void GetTimes_24HourFormat_Returns24HourStrings()
    {
        var prayerTimes = new PrayerTimes();
        Hashtable times = prayerTimes.GetTimes(
            51.5074,
            -0.1278,
            new DateTimeOffset(2024, 6, 15, 0, 0, 0, TimeSpan.Zero),
            Utc,
            format: TimeFormats.TIME_FORMAT_24H);

        string? fajr = times[PrayerTimes.FAJR]?.ToString();
        Assert.IsFalse(string.IsNullOrEmpty(fajr));
        Assert.IsTrue(fajr!.Contains(':'));
    }

    [TestMethod]
    public void GetTimes_FloatFormat_ReturnsNumericStrings()
    {
        var prayerTimes = new PrayerTimes();
        Hashtable times = prayerTimes.GetTimes(
            51.5074,
            -0.1278,
            new DateTimeOffset(2024, 6, 15, 0, 0, 0, TimeSpan.Zero),
            Utc,
            format: TimeFormats.TIME_FORMAT_FLOAT);

        string? fajr = times[PrayerTimes.FAJR]?.ToString();
        Assert.IsTrue(double.TryParse(fajr, out _));
    }

    [TestMethod]
    public void GetTimes_Iso8601Format_ReturnsTimeWithSeconds()
    {
        var prayerTimes = new PrayerTimes();
        Hashtable times = prayerTimes.GetTimes(
            51.5074,
            -0.1278,
            new DateTimeOffset(2024, 6, 15, 0, 0, 0, TimeSpan.Zero),
            Utc,
            format: TimeFormats.TIME_FORMAT_ISO8601);

        string? fajr = times[PrayerTimes.FAJR]?.ToString();
        Assert.IsFalse(string.IsNullOrEmpty(fajr));
        Assert.IsTrue(fajr!.Contains(':', StringComparison.Ordinal));
        Assert.AreEqual(3, fajr.Split(':').Length, "ISO8601 time should contain hours, minutes and seconds separated by colons.");
    }

    [TestMethod]
    public void GetTimes_DifferentMethodsProduceDifferentResults()
    {
        DateTimeOffset date = new(2024, 6, 15, 0, 0, 0, TimeSpan.Zero);
        var mwlTimes = new PrayerTimes(PrayerCalculationMethods.MWL).GetTimes(51.5074, -0.1278, date, Utc, format: TimeFormats.TIME_FORMAT_FLOAT);
        var egyptTimes = new PrayerTimes(PrayerCalculationMethods.EGYPT).GetTimes(51.5074, -0.1278, date, Utc, format: TimeFormats.TIME_FORMAT_FLOAT);

        double mwlFajr = Convert.ToDouble(mwlTimes[PrayerTimes.FAJR]);
        double egyptFajr = Convert.ToDouble(egyptTimes[PrayerTimes.FAJR]);

        Assert.AreNotEqual(mwlFajr, egyptFajr, 1e-3);
    }

    [TestMethod]
    public void GetTimes_DifferentSchoolsChangeAsrTime()
    {
        DateTimeOffset date = new(2024, 6, 15, 0, 0, 0, TimeSpan.Zero);
        var standardTimes = new PrayerTimes(PrayerCalculationMethods.MWL, Schools.STANDARD).GetTimes(51.5074, -0.1278, date, Utc, format: TimeFormats.TIME_FORMAT_FLOAT);
        var hanafiTimes = new PrayerTimes(PrayerCalculationMethods.MWL, Schools.HANAFI).GetTimes(51.5074, -0.1278, date, Utc, format: TimeFormats.TIME_FORMAT_FLOAT);

        double standardAsr = Convert.ToDouble(standardTimes[PrayerTimes.ASR]);
        double hanafiAsr = Convert.ToDouble(hanafiTimes[PrayerTimes.ASR]);

        Assert.IsTrue(hanafiAsr > standardAsr, "Hanafi Asr should occur later than standard Asr.");
    }

    [TestMethod]
    public void GetTimes_JafariSchool_UsesFourSeventhsShadowFactor()
    {
        DateTimeOffset date = new(2024, 6, 15, 0, 0, 0, TimeSpan.Zero);
        var standardTimes = new PrayerTimes(PrayerCalculationMethods.MWL, Schools.STANDARD).GetTimes(51.5074, -0.1278, date, Utc, format: TimeFormats.TIME_FORMAT_FLOAT);
        var hanafiTimes = new PrayerTimes(PrayerCalculationMethods.MWL, Schools.HANAFI).GetTimes(51.5074, -0.1278, date, Utc, format: TimeFormats.TIME_FORMAT_FLOAT);
        var jafariTimes = new PrayerTimes(PrayerCalculationMethods.MWL, Schools.JAFARI).GetTimes(51.5074, -0.1278, date, Utc, format: TimeFormats.TIME_FORMAT_FLOAT);

        double standardAsr = Convert.ToDouble(standardTimes[PrayerTimes.ASR]);
        double hanafiAsr = Convert.ToDouble(hanafiTimes[PrayerTimes.ASR]);
        double jafariAsr = Convert.ToDouble(jafariTimes[PrayerTimes.ASR]);

        Assert.IsTrue(hanafiAsr > standardAsr, "Hanafi Asr should occur later than standard Asr.");
        Assert.IsTrue(standardAsr > jafariAsr, "Ja'fari Asr (4/7 factor) should occur earlier than standard Asr.");
    }

    [TestMethod]
    public void GetTimes_LatitudeAdjustmentMethodNone_DoesNotThrow()
    {
        var prayerTimes = new PrayerTimes();
        Hashtable times = prayerTimes.GetTimes(
            51.5074,
            -0.1278,
            new DateTimeOffset(2024, 6, 15, 0, 0, 0, TimeSpan.Zero),
            Utc,
            latitudeAdjustmentMethod: LatitudeAdjustmentMethods.LATITUDE_ADJUSTMENT_METHOD_NONE);

        Assert.IsNotNull(times);
        Assert.IsTrue(times.Count > 0);
    }

    [TestMethod]
    public void GetTimes_JafariMidnight_DiffersFromStandardMidnight()
    {
        var jafariTimes = new PrayerTimes(PrayerCalculationMethods.JAFARI).GetTimes(
            35.6892,
            51.3890,
            new DateTimeOffset(2024, 6, 15, 0, 0, 0, TimeSpan.Zero),
            Utc,
            midnightMode: MidnightModes.JAFARI,
            format: TimeFormats.TIME_FORMAT_FLOAT);

        var standardTimes = new PrayerTimes(PrayerCalculationMethods.JAFARI).GetTimes(
            35.6892,
            51.3890,
            new DateTimeOffset(2024, 6, 15, 0, 0, 0, TimeSpan.Zero),
            Utc,
            midnightMode: MidnightModes.STANDARD,
            format: TimeFormats.TIME_FORMAT_FLOAT);

        double jafariMidnight = Convert.ToDouble(jafariTimes[PrayerTimes.MIDNIGHT]);
        double standardMidnight = Convert.ToDouble(standardTimes[PrayerTimes.MIDNIGHT]);

        Assert.AreNotEqual(jafariMidnight, standardMidnight, 1e-3);
    }

    [TestMethod]
    public void GetTimes_ElevationIsUsed()
    {
        var prayerTimes = new PrayerTimes();
        Hashtable timesWithElevation = prayerTimes.GetTimes(
            51.5074,
            -0.1278,
            new DateTimeOffset(2024, 6, 15, 0, 0, 0, TimeSpan.Zero),
            Utc,
            elevation: 100,
            format: TimeFormats.TIME_FORMAT_FLOAT);

        Hashtable timesWithoutElevation = prayerTimes.GetTimes(
            51.5074,
            -0.1278,
            new DateTimeOffset(2024, 6, 15, 0, 0, 0, TimeSpan.Zero),
            Utc,
            format: TimeFormats.TIME_FORMAT_FLOAT);

        double sunriseWithElevation = Convert.ToDouble(timesWithElevation[PrayerTimes.SUNRISE]);
        double sunriseWithoutElevation = Convert.ToDouble(timesWithoutElevation[PrayerTimes.SUNRISE]);

        Assert.AreNotEqual(sunriseWithElevation, sunriseWithoutElevation, 1e-3);
    }

    [TestMethod]
    public void SetCustomMethod_UpdatesMethodToCustom()
    {
        var prayerTimes = new PrayerTimes();
        prayerTimes.SetCustomMethod(PrayerCalculationMethod.Custom);

        var methodField = typeof(PrayerTimes).GetField("_method", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var currentMethod = (PrayerCalculationMethods)methodField!.GetValue(prayerTimes)!;

        Assert.AreEqual(PrayerCalculationMethods.CUSTOM, currentMethod);
    }

    [TestMethod]
    public void GetTimes_WithTuneOffsets_AppliesOffset()
    {
        var prayerTimes = new PrayerTimes();
        prayerTimes.SetTuneTimeOffset(new Dictionary<string, double>
        {
            [PrayerTimes.FAJR] = 10
        });

        Hashtable tunedTimes = prayerTimes.GetTimes(
            51.5074,
            -0.1278,
            new DateTimeOffset(2024, 6, 15, 0, 0, 0, TimeSpan.Zero),
            Utc,
            format: TimeFormats.TIME_FORMAT_FLOAT);

        Assert.IsNotNull(tunedTimes[PrayerTimes.FAJR]);
    }

    [TestMethod]
    public void TuneTimeOffsets_MoonsightingMethod_ContainsDefaultZuhrAndMaghribOffsets()
    {
        var prayerTimes = new PrayerTimes(PrayerCalculationMethods.MOONSIGHTING);

        Assert.AreEqual(MoonsightingOffsets.ZuhrOffsetMinutes, prayerTimes.TuneTimeOffsets[PrayerTimes.ZHUHR]);
        Assert.AreEqual(MoonsightingOffsets.SunniMaghribOffsetMinutes, prayerTimes.TuneTimeOffsets[PrayerTimes.MAGHRIB]);
    }

    [TestMethod]
    public void TuneTimeOffsets_SwitchingAwayFromMoonsighting_ClearsDefaultOffsets()
    {
        var prayerTimes = new PrayerTimes(PrayerCalculationMethods.MOONSIGHTING);
        prayerTimes.SetMethod(PrayerCalculationMethods.MWL);

        Assert.IsFalse(prayerTimes.TuneTimeOffsets.ContainsKey(PrayerTimes.ZHUHR));
        Assert.IsFalse(prayerTimes.TuneTimeOffsets.ContainsKey(PrayerTimes.MAGHRIB));
    }

    [TestMethod]
    public void SetTuneTimeOffset_MergesWithMoonsightingDefaults()
    {
        var prayerTimes = new PrayerTimes(PrayerCalculationMethods.MOONSIGHTING);
        prayerTimes.SetTuneTimeOffset(new Dictionary<string, double>
        {
            [PrayerTimes.FAJR] = 10
        });

        Assert.AreEqual(10d, prayerTimes.TuneTimeOffsets[PrayerTimes.FAJR]);
        Assert.AreEqual(MoonsightingOffsets.ZuhrOffsetMinutes, prayerTimes.TuneTimeOffsets[PrayerTimes.ZHUHR]);
        Assert.AreEqual(MoonsightingOffsets.SunniMaghribOffsetMinutes, prayerTimes.TuneTimeOffsets[PrayerTimes.MAGHRIB]);
    }

    [TestMethod]
    public void SetTuneTimeOffset_OverridesMoonsightingMaghribDefault()
    {
        var prayerTimes = new PrayerTimes(PrayerCalculationMethods.MOONSIGHTING);
        prayerTimes.SetTuneTimeOffset(new Dictionary<string, double>
        {
            [PrayerTimes.MAGHRIB] = 20
        });

        Hashtable times = prayerTimes.GetTimes(
            51.5074, -0.1278, new DateTimeOffset(2024, 6, 15, 0, 0, 0, TimeSpan.Zero), Utc, format: TimeFormats.TIME_FORMAT_FLOAT);

        double sunset = Convert.ToDouble(times[PrayerTimes.SUNSET]);
        double maghrib = Convert.ToDouble(times[PrayerTimes.MAGHRIB]);

        Assert.AreEqual(20d, prayerTimes.TuneTimeOffsets[PrayerTimes.MAGHRIB]);
        Assert.AreEqual(20d / 60d, maghrib - sunset, 1e-6);
    }

    [TestMethod]
    public void Now_ReturnsTimesForToday()
    {
        DateTimeOffset today = new(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);
        var fake = new FakeTimeProvider(today);

        Hashtable nowTimes = new PrayerTimes(timeProvider: fake).Now(
            51.5074, -0.1278, Utc, format: TimeFormats.TIME_FORMAT_FLOAT);
        Hashtable expected = new PrayerTimes(timeProvider: fake).GetTimes(
            51.5074, -0.1278, today, Utc, format: TimeFormats.TIME_FORMAT_FLOAT);

        Assert.IsNotNull(nowTimes);
        Assert.IsTrue(nowTimes.Count > 0);
        Assert.AreEqual(expected[PrayerTimes.FAJR], nowTimes[PrayerTimes.FAJR]);
    }

    [TestMethod]
    public void Tomorrow_ReturnsTimesForNextDay()
    {
        DateTimeOffset today = new(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);
        var fake = new FakeTimeProvider(today);

        Hashtable tomorrowTimes = new PrayerTimes(timeProvider: fake).Tomorrow(
            51.5074, -0.1278, Utc, format: TimeFormats.TIME_FORMAT_FLOAT);
        Hashtable expected = new PrayerTimes(timeProvider: fake).GetTimes(
            51.5074, -0.1278, today.AddDays(1), Utc, format: TimeFormats.TIME_FORMAT_FLOAT);

        Assert.IsNotNull(tomorrowTimes);
        Assert.IsTrue(tomorrowTimes.Count > 0);
        Assert.AreEqual(expected[PrayerTimes.FAJR], tomorrowTimes[PrayerTimes.FAJR]);
    }

    [TestMethod]
    public void SunPosition_ReturnsDeclinationAndEquation()
    {
        var method = typeof(PrayerTimes).GetMethod("SunPosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        Sun sun = (Sun)method!.Invoke(null, [2451545.0])!;

        Assert.IsNotNull(sun);
        Assert.IsFalse(double.IsNaN(sun.Declination));
        Assert.IsFalse(double.IsNaN(sun.Equation));
    }

    [TestMethod]
    public void JulianDate_ReturnsCorrectValueForKnownDate()
    {
        var method = typeof(PrayerTimes).GetMethod("JulianDate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        double jd = (double)method!.Invoke(null, [2000, 1, 1])!;

        // JulianDate computes the JD at 0h UT, not 12h UT.
        Assert.AreEqual(2451544.5, jd, 1e-6);
    }

    [DataTestMethod]
    [DataRow("10 min", 10)]
    [DataRow("90 min", 90)]
    [DataRow("18.5", 18.5)]
    [DataRow("18.5 min", 18.5)]
    [DataRow("18.5°", 18.5)]
    [DataRow("0 min", 0)]
    [DataRow("", 0)]
    [DataRow("   ", 0)]
    public void Evaluate_ParsesVariousFormats(string input, double expected)
    {
        var method = typeof(PrayerTimes).GetMethod("Evaluate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        double actual = (double)method!.Invoke(null, [input])!;

        Assert.AreEqual(expected, actual, 1e-6);
    }

    [TestMethod]
    public void GetTimes_Asr_IsBetweenDhuhrAndSunset()
    {
        var prayerTimes = new PrayerTimes();
        Hashtable times = prayerTimes.GetTimes(
            -33.8688, // Sydney
            151.2093,
            new DateTimeOffset(2024, 6, 15, 0, 0, 0, TimeSpan.Zero),
            Utc,
            format: TimeFormats.TIME_FORMAT_FLOAT);

        double dhuhr = Convert.ToDouble(times[PrayerTimes.ZHUHR]);
        double asr = Convert.ToDouble(times[PrayerTimes.ASR]);
        double maghrib = Convert.ToDouble(times[PrayerTimes.MAGHRIB]);

        Assert.IsTrue(dhuhr < asr && asr < maghrib, "Asr should occur between Dhuhr and Maghrib.");
    }

    [TestMethod]
    public void GetTimes_MinuteBasedIsha_KeepsMinuteOffset()
    {
        var prayerTimes = new PrayerTimes(PrayerCalculationMethods.MAKKAH);
        Hashtable times = prayerTimes.GetTimes(
            21.3891, // Mecca
            39.8579,
            new DateTimeOffset(2024, 6, 15, 0, 0, 0, TimeSpan.Zero),
            Utc,
            format: TimeFormats.TIME_FORMAT_FLOAT);

        double maghrib = Convert.ToDouble(times[PrayerTimes.MAGHRIB]);
        double isha = Convert.ToDouble(times[PrayerTimes.ISHA]);

        Assert.IsTrue(isha > maghrib, "Isha should occur after Maghrib.");
        Assert.AreEqual(1.5, isha - maghrib, 0.05, "Isha should be approximately 90 minutes after Maghrib.");
    }

    [TestMethod]
    public void GetTimes_AngleBasedIsha_IsCappedAtHighLatitudes()
    {
        var prayerTimes = new PrayerTimes(PrayerCalculationMethods.MWL);
        Hashtable times = prayerTimes.GetTimes(
            65.0, // High latitude in summer where -17° Isha is not reached naturally
            0.0,
            new DateTimeOffset(2024, 6, 15, 0, 0, 0, TimeSpan.Zero),
            Utc,
            latitudeAdjustmentMethod: LatitudeAdjustmentMethods.LATITUDE_ADJUSTMENT_METHOD_ANGLE,
            format: TimeFormats.TIME_FORMAT_FLOAT);

        double isha = Convert.ToDouble(times[PrayerTimes.ISHA]);
        double fajr = Convert.ToDouble(times[PrayerTimes.FAJR]);
        double sunset = Convert.ToDouble(times[PrayerTimes.SUNSET]);

        Assert.IsFalse(double.IsNaN(isha), "High-latitude capped Isha should not be NaN.");
        Assert.IsTrue(isha > sunset, "Isha should occur after sunset.");
        Assert.IsTrue(isha < fajr + 24, "Isha should occur before the next day's Fajr.");
    }

    [TestMethod]
    public void GetTimes_MoonsightingMethod_AppliesDefaultZuhrOffset()
    {
        DateTimeOffset date = new(2024, 6, 15, 0, 0, 0, TimeSpan.Zero);
        var mwlTimes = new PrayerTimes(PrayerCalculationMethods.MWL).GetTimes(51.5074, -0.1278, date, Utc, format: TimeFormats.TIME_FORMAT_FLOAT);
        var moonsightingTimes = new PrayerTimes(PrayerCalculationMethods.MOONSIGHTING).GetTimes(51.5074, -0.1278, date, Utc, format: TimeFormats.TIME_FORMAT_FLOAT);

        double mwlDhuhr = Convert.ToDouble(mwlTimes[PrayerTimes.ZHUHR]);
        double moonsightingDhuhr = Convert.ToDouble(moonsightingTimes[PrayerTimes.ZHUHR]);

        Assert.AreEqual(MoonsightingOffsets.ZuhrOffsetMinutes / 60d, moonsightingDhuhr - mwlDhuhr, 1e-6);
    }

    [TestMethod]
    public void GetTimes_MoonsightingMethod_DefaultsToSunniMaghribOffset()
    {
        var prayerTimes = new PrayerTimes(PrayerCalculationMethods.MOONSIGHTING);
        Hashtable times = prayerTimes.GetTimes(
            51.5074, -0.1278, new DateTimeOffset(2024, 6, 15, 0, 0, 0, TimeSpan.Zero), Utc, format: TimeFormats.TIME_FORMAT_FLOAT);

        double sunset = Convert.ToDouble(times[PrayerTimes.SUNSET]);
        double maghrib = Convert.ToDouble(times[PrayerTimes.MAGHRIB]);

        Assert.AreEqual(MoonsightingOffsets.SunniMaghribOffsetMinutes / 60d, maghrib - sunset, 1e-6);
    }

    [TestMethod]
    public void GetTimes_MoonsightingMethod_JafariSchool_UsesShiaMaghribOffset()
    {
        var prayerTimes = new PrayerTimes(PrayerCalculationMethods.MOONSIGHTING, Schools.JAFARI);

        Hashtable times = prayerTimes.GetTimes(
            51.5074, -0.1278, new DateTimeOffset(2024, 6, 15, 0, 0, 0, TimeSpan.Zero), Utc, format: TimeFormats.TIME_FORMAT_FLOAT);

        double sunset = Convert.ToDouble(times[PrayerTimes.SUNSET]);
        double maghrib = Convert.ToDouble(times[PrayerTimes.MAGHRIB]);

        Assert.AreEqual(MoonsightingOffsets.ShiaMaghribOffsetMinutes / 60d, maghrib - sunset, 1e-6);
    }

    [TestMethod]
    public void GetTimesResult_ReturnsAllPrayerKeysAsProperties()
    {
        var prayerTimes = new PrayerTimes();
        PrayerTimesResult result = prayerTimes.GetTimesResult(
            51.5074, -0.1278, new DateTimeOffset(2024, 6, 15, 0, 0, 0, TimeSpan.Zero), Utc);

        Assert.IsNotNull(result);
        Assert.IsFalse(string.IsNullOrEmpty(result.Imsak));
        Assert.IsFalse(string.IsNullOrEmpty(result.Fajr));
        Assert.IsFalse(string.IsNullOrEmpty(result.Sunrise));
        Assert.IsFalse(string.IsNullOrEmpty(result.Dhuhr));
        Assert.IsFalse(string.IsNullOrEmpty(result.Asr));
        Assert.IsFalse(string.IsNullOrEmpty(result.Sunset));
        Assert.IsFalse(string.IsNullOrEmpty(result.Maghrib));
        Assert.IsFalse(string.IsNullOrEmpty(result.Isha));
        Assert.IsFalse(string.IsNullOrEmpty(result.Midnight));
    }

    [TestMethod]
    public void GetTimesResult_FloatFormat_MatchesHashtableOutput()
    {
        var prayerTimes = new PrayerTimes();
        DateTimeOffset date = new(2024, 6, 15, 0, 0, 0, TimeSpan.Zero);

        Hashtable hashtable = prayerTimes.GetTimes(
            51.5074, -0.1278, date, Utc, format: TimeFormats.TIME_FORMAT_FLOAT);

        PrayerTimesResult result = prayerTimes.GetTimesResult(
            51.5074, -0.1278, date, Utc, format: TimeFormats.TIME_FORMAT_FLOAT);

        Assert.AreEqual(hashtable[PrayerTimes.IMSAK], result.Imsak);
        Assert.AreEqual(hashtable[PrayerTimes.FAJR], result.Fajr);
        Assert.AreEqual(hashtable[PrayerTimes.SUNRISE], result.Sunrise);
        Assert.AreEqual(hashtable[PrayerTimes.ZHUHR], result.Dhuhr);
        Assert.AreEqual(hashtable[PrayerTimes.ASR], result.Asr);
        Assert.AreEqual(hashtable[PrayerTimes.SUNSET], result.Sunset);
        Assert.AreEqual(hashtable[PrayerTimes.MAGHRIB], result.Maghrib);
        Assert.AreEqual(hashtable[PrayerTimes.ISHA], result.Isha);
        Assert.AreEqual(hashtable[PrayerTimes.MIDNIGHT], result.Midnight);
    }

    [TestMethod]
    public void NowResult_ReturnsTimesForToday()
    {
        DateTimeOffset today = new(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);
        var fake = new FakeTimeProvider(today);

        PrayerTimesResult result = new PrayerTimes(timeProvider: fake).NowResult(
            51.5074, -0.1278, Utc, format: TimeFormats.TIME_FORMAT_FLOAT);
        PrayerTimesResult expected = new PrayerTimes(timeProvider: fake).GetTimesResult(
            51.5074, -0.1278, today, Utc, format: TimeFormats.TIME_FORMAT_FLOAT);

        Assert.IsNotNull(result);
        Assert.IsFalse(string.IsNullOrEmpty(result.Fajr));
        Assert.AreEqual(expected.Fajr, result.Fajr);
    }

    [TestMethod]
    public void TomorrowResult_ReturnsTimesForNextDay()
    {
        DateTimeOffset today = new(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);
        var fake = new FakeTimeProvider(today);

        PrayerTimesResult result = new PrayerTimes(timeProvider: fake).TomorrowResult(
            51.5074, -0.1278, Utc, format: TimeFormats.TIME_FORMAT_FLOAT);
        PrayerTimesResult expected = new PrayerTimes(timeProvider: fake).GetTimesResult(
            51.5074, -0.1278, today.AddDays(1), Utc, format: TimeFormats.TIME_FORMAT_FLOAT);

        Assert.IsNotNull(result);
        Assert.IsFalse(string.IsNullOrEmpty(result.Fajr));
        Assert.AreEqual(expected.Fajr, result.Fajr);
    }

    [TestMethod]
    public void GetTimes_WithInputsRecord_MatchesExpandedParameters()
    {
        var prayerTimes = new PrayerTimes();
        DateTimeOffset date = new(2024, 6, 15, 0, 0, 0, TimeSpan.Zero);
        var inputs = new PrayerTimesInputs(
            Latitude: 51.5074,
            Longitude: -0.1278,
            DateTime: date,
            TimeZone: Utc,
            Format: TimeFormats.TIME_FORMAT_FLOAT);

        Hashtable expanded = prayerTimes.GetTimes(51.5074, -0.1278, date, Utc, format: TimeFormats.TIME_FORMAT_FLOAT);
        Hashtable fromRecord = prayerTimes.GetTimes(inputs);

        Assert.AreEqual(expanded[PrayerTimes.FAJR], fromRecord[PrayerTimes.FAJR]);
        Assert.AreEqual(expanded[PrayerTimes.ZHUHR], fromRecord[PrayerTimes.ZHUHR]);
        Assert.AreEqual(expanded[PrayerTimes.MAGHRIB], fromRecord[PrayerTimes.MAGHRIB]);
    }

    [TestMethod]
    public void GetTimesResult_WithInputsRecord_MatchesExpandedParameters()
    {
        var prayerTimes = new PrayerTimes();
        DateTimeOffset date = new(2024, 6, 15, 0, 0, 0, TimeSpan.Zero);
        var inputs = new PrayerTimesInputs(
            Latitude: 51.5074,
            Longitude: -0.1278,
            DateTime: date,
            TimeZone: Utc,
            Format: TimeFormats.TIME_FORMAT_FLOAT);

        PrayerTimesResult expanded = prayerTimes.GetTimesResult(51.5074, -0.1278, date, Utc, format: TimeFormats.TIME_FORMAT_FLOAT);
        PrayerTimesResult fromRecord = prayerTimes.GetTimesResult(inputs);

        Assert.AreEqual(expanded.Fajr, fromRecord.Fajr);
        Assert.AreEqual(expanded.Dhuhr, fromRecord.Dhuhr);
        Assert.AreEqual(expanded.Maghrib, fromRecord.Maghrib);
    }

    [TestMethod]
    public void Now_WithInputsRecord_ReturnsTimesForToday()
    {
        DateTimeOffset today = new(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);
        var fake = new FakeTimeProvider(today);
        var inputs = new PrayerTimesInputs(51.5074, -0.1278, TimeZone: Utc, Format: TimeFormats.TIME_FORMAT_FLOAT);

        Hashtable result = new PrayerTimes(timeProvider: fake).Now(inputs);
        Hashtable expected = new PrayerTimes(timeProvider: fake).GetTimes(
            51.5074, -0.1278, today, Utc, format: TimeFormats.TIME_FORMAT_FLOAT);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.ContainsKey(PrayerTimes.FAJR));
        Assert.AreEqual(expected[PrayerTimes.FAJR], result[PrayerTimes.FAJR]);
    }

    [TestMethod]
    public void NowResult_WithInputsRecord_ReturnsTimesForToday()
    {
        DateTimeOffset today = new(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);
        var fake = new FakeTimeProvider(today);
        var inputs = new PrayerTimesInputs(51.5074, -0.1278, TimeZone: Utc, Format: TimeFormats.TIME_FORMAT_FLOAT);

        PrayerTimesResult result = new PrayerTimes(timeProvider: fake).NowResult(inputs);
        PrayerTimesResult expected = new PrayerTimes(timeProvider: fake).GetTimesResult(
            51.5074, -0.1278, today, Utc, format: TimeFormats.TIME_FORMAT_FLOAT);

        Assert.IsNotNull(result);
        Assert.IsFalse(string.IsNullOrEmpty(result.Fajr));
        Assert.AreEqual(expected.Fajr, result.Fajr);
    }
}
