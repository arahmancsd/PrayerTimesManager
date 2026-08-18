namespace PrayerTimesManager.Tests;

[TestClass]
public class DMathTests
{
    [TestMethod]
    public void Dtr_ConvertsDegreesToRadians()
    {
        double result = DMath.Dtr(180);
        Assert.AreEqual(Math.PI, result, 1e-10);
    }

    [TestMethod]
    public void Rtd_ConvertsRadiansToDegrees()
    {
        double result = DMath.Rtd(Math.PI);
        Assert.AreEqual(180, result, 1e-10);
    }

    [TestMethod]
    public void Sin_90Degrees_ReturnsOne()
    {
        double result = DMath.Sin(90);
        Assert.AreEqual(1, result, 1e-10);
    }

    [TestMethod]
    public void Cos_90Degrees_ReturnsZero()
    {
        double result = DMath.Cos(90);
        Assert.AreEqual(0, result, 1e-10);
    }

    [TestMethod]
    public void Tan_45Degrees_ReturnsOne()
    {
        double result = DMath.Tan(45);
        Assert.AreEqual(1, result, 1e-10);
    }

    [TestMethod]
    public void Arcsin_One_Returns90Degrees()
    {
        double result = DMath.Arcsin(1);
        Assert.AreEqual(90, result, 1e-10);
    }

    [TestMethod]
    public void Arccos_One_ReturnsZeroDegrees()
    {
        double result = DMath.Arccos(1);
        Assert.AreEqual(0, result, 1e-10);
    }

    [TestMethod]
    public void Arctan_One_Returns45Degrees()
    {
        double result = DMath.Arctan(1);
        Assert.AreEqual(45, result, 1e-10);
    }

    [TestMethod]
    public void Arccot_One_Returns45Degrees()
    {
        double result = DMath.Arccot(1);
        Assert.AreEqual(45, result, 1e-10);
    }

    [TestMethod]
    public void Arctan2_PositiveXAndY_Returns45Degrees()
    {
        double result = DMath.Arctan2(1, 1);
        Assert.AreEqual(45, result, 1e-10);
    }

    [TestMethod]
    public void FixAngle_WrapsAngleTo360Range()
    {
        Assert.AreEqual(10, DMath.FixAngle(370), 1e-10);
        Assert.AreEqual(350, DMath.FixAngle(-10), 1e-10);
    }

    [TestMethod]
    public void FixHour_WrapsHourTo24Range()
    {
        Assert.AreEqual(1, DMath.FixHour(25), 1e-10);
        Assert.AreEqual(23, DMath.FixHour(-1), 1e-10);
    }
}
