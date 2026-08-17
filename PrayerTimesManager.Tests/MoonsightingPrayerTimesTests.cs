using PrayersTimeManager;

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

        Assert.AreEqual("north", ms.hemisphere);
    }

    [TestMethod]
    public void MoonsightingPrayerTimes_SouthernHemisphere_SetsHemisphereToSouth()
    {
        var ms = new Fajr(new DateTime(2024, 6, 15), -20);

        Assert.AreEqual("south", ms.hemisphere);
    }
}
