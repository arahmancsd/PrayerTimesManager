using PrayersTimeManager;

namespace PrayerTimesManager.Tests;

[TestClass]
public class PrayerCalculationMethodTests
{
    [TestMethod]
    public void Mwl_HasExpectedFajrAndIshaAngles()
    {
        var method = PrayerCalculationMethod.Mwl;

        Assert.AreEqual(18d, method.Param[PrayerTimes.FAJR]);
        Assert.AreEqual(17d, method.Param[PrayerTimes.ISHA]);
    }

    [TestMethod]
    public void Makkah_Isha_IsMinutesAfterMaghrib()
    {
        var method = PrayerCalculationMethod.Makkah;

        Assert.AreEqual(18.5d, method.Param[PrayerTimes.FAJR]);
        Assert.AreEqual("90 min", method.Param[PrayerTimes.ISHA]);
    }

    [TestMethod]
    public void Jafari_SetsMidnightModeToJafari()
    {
        var method = PrayerCalculationMethod.Jafari;

        Assert.IsTrue(method.Param.ContainsKey(PrayerTimes.MIDNIGHT));
        Assert.AreEqual(14d, method.Param[PrayerTimes.ISHA]);
    }

    [TestMethod]
    public void Custom_IsConfiguredWithDefaultAngles()
    {
        var method = PrayerCalculationMethod.Custom;

        Assert.AreEqual(15, method.Param[PrayerTimes.FAJR]);
        Assert.AreEqual(15, method.Param[PrayerTimes.ISHA]);
    }

    [TestMethod]
    public void PrayerCalculations_ContainsAllExpectedMethods()
    {
        var calculations = PrayerCalculation.PrayerCalculations;
        var expectedMethods = Enum.GetValues<PrayersTimeManager.Enums.PrayerCalculationMethods>()
            .Where(m => m != PrayersTimeManager.Enums.PrayerCalculationMethods.DEFAULT);

        foreach (var method in expectedMethods)
        {
            string key = method.ToString();
            Assert.IsTrue(calculations.ContainsKey(key), $"Expected method '{key}' to be present in PrayerCalculations.");
        }
    }

    [TestMethod]
    public void PrayerCalculationsCodes_MapsEnumValuesToThemselves()
    {
        var codes = PrayerCalculation.PrayerCalculationsCodes;

        Assert.AreEqual((short)PrayersTimeManager.Enums.PrayerCalculationMethods.MWL, codes[(short)PrayersTimeManager.Enums.PrayerCalculationMethods.MWL]);
    }
}
