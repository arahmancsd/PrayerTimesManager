namespace PrayerTimesManager.Tests;

[TestClass]
public class MoonsightingPrayerTimesTests
{
    [TestMethod]
    public void Fajr_GetMinutesBeforeSunrise_ReturnsPositiveValue()
    {
        var fajr = new Fajr(new DateTime(2024, 6, 15), 51.5);
        int minutes = fajr.GetMinutesBeforeSunrise();

        Assert.IsTrue(minutes > 0);
    }

    [TestMethod]
    public void Isha_GetMinutesAfterSunset_DefaultShafaq_ReturnsPositiveValue()
    {
        var isha = new Isha(new DateTime(2024, 6, 15), 51.5);
        int minutes = isha.GetMinutesAfterSunset();

        Assert.IsTrue(minutes > 0);
    }

    [TestMethod]
    public void Isha_GetMinutesAfterSunset_AhmerShafaq_ReturnsPositiveValue()
    {
        var isha = new Isha(new DateTime(2024, 6, 15), 51.5, Shafaq.SHAFAQ_AHMER);
        int minutes = isha.GetMinutesAfterSunset();

        Assert.IsTrue(minutes > 0);
    }

    [TestMethod]
    public void Isha_GetMinutesAfterSunset_AbyadShafaq_ReturnsPositiveValue()
    {
        var isha = new Isha(new DateTime(2024, 6, 15), 51.5, Shafaq.SHAFAQ_ABYAD);
        int minutes = isha.GetMinutesAfterSunset();

        Assert.IsTrue(minutes > 0);
    }

    [TestMethod]
    public void Fajr_DifferentLatitudesProduceDifferentMinutes()
    {
        var fajrLowLat = new Fajr(new DateTime(2024, 6, 15), 10);
        var fajrHighLat = new Fajr(new DateTime(2024, 6, 15), 60);

        int lowLatMinutes = fajrLowLat.GetMinutesBeforeSunrise();
        int highLatMinutes = fajrHighLat.GetMinutesBeforeSunrise();

        Assert.AreNotEqual(lowLatMinutes, highLatMinutes);
    }

    [TestMethod]
    public void MoonsightingPrayerTimes_NorthernHemisphere_SetsHemisphereToNorth()
    {
        var ms = new Fajr(new DateTime(2024, 6, 15), 20);

        Assert.AreEqual("north", ms._hemisphere);
    }

    [TestMethod]
    public void MoonsightingPrayerTimes_SouthernHemisphere_SetsHemisphereToSouth()
    {
        var ms = new Fajr(new DateTime(2024, 6, 15), -20);

        Assert.AreEqual("south", ms._hemisphere);
    }

    [TestMethod]
    public void GetDyy_IsIndependentOfTimeOfDay()
    {
        var morning = new Fajr(new DateTime(2024, 6, 15, 8, 0, 0), 51.5);
        var evening = new Fajr(new DateTime(2024, 6, 15, 22, 0, 0), 51.5);

        Assert.AreEqual(morning.GetMinutesBeforeSunrise(), evening.GetMinutesBeforeSunrise());
    }

    [TestMethod]
    public void GetDyy_LeapYear_WrapsCorrectly()
    {
        var fajrSolstice = new Fajr(new DateTime(2024, 12, 21), 51.5);
        var fajrNextDay = new Fajr(new DateTime(2024, 12, 22), 51.5);

        var dyyField = typeof(MoonsightingPrayerTimes).GetField("_dyy", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        int dyySolstice = (int)dyyField!.GetValue(fajrSolstice)!;
        int dyyNextDay = (int)dyyField!.GetValue(fajrNextDay)!;

        Assert.AreEqual(0, dyySolstice);
        Assert.AreEqual(1, dyyNextDay);
    }
}
