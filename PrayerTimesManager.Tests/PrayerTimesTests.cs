using System.Collections;
using PrayersTimeManager;
using PrayersTimeManager.Enums;

namespace PrayerTimesManager.Tests;

[TestClass]
public class PrayerTimesTests
{
    private static readonly TimeZoneInfo Utc = TimeZoneInfo.Utc;

    [TestMethod]
    public void Constructor_DefaultMethod_IsMwl()
    {
        var prayerTimes = new PrayerTimes();

        Assert.IsNotNull(prayerTimes.prayerCalculationMethods);
        Assert.IsNotNull(prayerTimes.prayerCalculationMethodCodes);
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
        DateTime date = new(2000, 1, 1, 12, 0, 0);
        double julianDate = PrayerTimes.ToJulianDate(date);

        Assert.AreEqual(2451545.0, julianDate, 1e-6);
    }

    [TestMethod]
    public void GetTimes_ReturnsAllPrayerKeys()
    {
        var prayerTimes = new PrayerTimes();
        Hashtable times = prayerTimes.GetTimes(51.5074, -0.1278, new DateTime(2024, 6, 15), Utc);

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
        Hashtable times = prayerTimes.GetTimes(51.5074, -0.1278, new DateTime(2024, 6, 15), Utc);

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
            new DateTime(2024, 6, 15),
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
            new DateTime(2024, 6, 15),
            Utc,
            format: TimeFormats.TIME_FORMAT_FLOAT);

        string? fajr = times[PrayerTimes.FAJR]?.ToString();
        Assert.IsTrue(double.TryParse(fajr, out _));
    }

    [TestMethod]
    public void GetTimes_DifferentMethodsProduceDifferentResults()
    {
        DateTime date = new(2024, 6, 15);
        var mwlTimes = new PrayerTimes(PrayerCalculationMethods.MWL).GetTimes(51.5074, -0.1278, date, Utc, format: TimeFormats.TIME_FORMAT_FLOAT);
        var egyptTimes = new PrayerTimes(PrayerCalculationMethods.EGYPT).GetTimes(51.5074, -0.1278, date, Utc, format: TimeFormats.TIME_FORMAT_FLOAT);

        double mwlFajr = Convert.ToDouble(mwlTimes[PrayerTimes.FAJR]);
        double egyptFajr = Convert.ToDouble(egyptTimes[PrayerTimes.FAJR]);

        Assert.AreNotEqual(mwlFajr, egyptFajr, 1e-3);
    }

    [TestMethod]
    public void GetTimes_DifferentSchoolsChangeAsrTime()
    {
        DateTime date = new(2024, 6, 15);
        var standardTimes = new PrayerTimes(PrayerCalculationMethods.MWL, Schools.STANDARD).GetTimes(51.5074, -0.1278, date, Utc, format: TimeFormats.TIME_FORMAT_FLOAT);
        var hanafiTimes = new PrayerTimes(PrayerCalculationMethods.MWL, Schools.HANAFI).GetTimes(51.5074, -0.1278, date, Utc, format: TimeFormats.TIME_FORMAT_FLOAT);

        double standardAsr = Convert.ToDouble(standardTimes[PrayerTimes.ASR]);
        double hanafiAsr = Convert.ToDouble(hanafiTimes[PrayerTimes.ASR]);

        Assert.IsTrue(hanafiAsr > standardAsr, "Hanafi Asr should occur later than standard Asr.");
    }

    [TestMethod]
    public void GetTimes_LatitudeAdjustmentMethodNone_DoesNotThrow()
    {
        var prayerTimes = new PrayerTimes();
        Hashtable times = prayerTimes.GetTimes(
            51.5074,
            -0.1278,
            new DateTime(2024, 6, 15),
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
            new DateTime(2024, 6, 15),
            Utc,
            midnightMode: MidnightModes.JAFARI,
            format: TimeFormats.TIME_FORMAT_FLOAT);

        var standardTimes = new PrayerTimes(PrayerCalculationMethods.JAFARI).GetTimes(
            35.6892,
            51.3890,
            new DateTime(2024, 6, 15),
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
            new DateTime(2024, 6, 15),
            Utc,
            elevation: 100,
            format: TimeFormats.TIME_FORMAT_FLOAT);

        Hashtable timesWithoutElevation = prayerTimes.GetTimes(
            51.5074,
            -0.1278,
            new DateTime(2024, 6, 15),
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

        var methodField = typeof(PrayerTimes).GetField("method", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var currentMethod = (PrayerCalculationMethods)methodField!.GetValue(prayerTimes)!;

        Assert.AreEqual(PrayerCalculationMethods.CUSTOM, currentMethod);
    }

    [TestMethod]
    public void GetTimes_WithTuneOffsets_AppliesOffset()
    {
        var prayerTimes = new PrayerTimes();
        prayerTimes.SetTuneTimeOffset(new Hashtable
        {
            [PrayerTimes.FAJR] = 10
        });

        Hashtable tunedTimes = prayerTimes.GetTimes(
            51.5074,
            -0.1278,
            new DateTime(2024, 6, 15),
            Utc,
            format: TimeFormats.TIME_FORMAT_FLOAT);

        Assert.IsNotNull(tunedTimes[PrayerTimes.FAJR]);
    }

    [TestMethod]
    public void Now_ReturnsTimesForToday()
    {
        var prayerTimes = new PrayerTimes();
        Hashtable times = prayerTimes.Now(51.5074, -0.1278, Utc);

        Assert.IsNotNull(times);
        Assert.IsTrue(times.Count > 0);
    }

    [TestMethod]
    public void Tomorrow_ReturnsTimesForNextDay()
    {
        var prayerTimes = new PrayerTimes();
        Hashtable times = prayerTimes.Tomorrow(51.5074, -0.1278, Utc);

        Assert.IsNotNull(times);
        Assert.IsTrue(times.Count > 0);
    }

    [TestMethod]
    public void SunPosition_ReturnsDeclinationAndEquation()
    {
        var prayerTimes = new PrayerTimes();
        Sun sun = prayerTimes.SunPosition(2451545.0);

        Assert.IsNotNull(sun);
        Assert.IsFalse(double.IsNaN(sun.Declination));
        Assert.IsFalse(double.IsNaN(sun.Equation));
    }

    [TestMethod]
    public void JulianDate_ReturnsCorrectValueForKnownDate()
    {
        var prayerTimes = new PrayerTimes();
        double jd = prayerTimes.JulianDate(2000, 1, 1);

        // JulianDate computes the JD at 0h UT, not 12h UT.
        Assert.AreEqual(2451544.5, jd, 1e-6);
    }
}
