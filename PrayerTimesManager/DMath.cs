namespace PrayerTimesManager;

/// <summary>
/// Provides trigonometric helper functions that operate on and return degrees instead of radians,
/// along with angle normalization utilities used throughout the prayer time calculations.
/// </summary>
public sealed class DMath
{
    /// <summary>Converts an angle expressed in degrees to radians.</summary>
    /// <param name="d">The angle, in degrees.</param>
    /// <returns>The equivalent angle, in radians.</returns>
    public static double Dtr(double d)
    {
        return (d * Math.PI) / 180.0;
    }
    /// <summary>Converts an angle expressed in radians to degrees.</summary>
    /// <param name="r">The angle, in radians.</param>
    /// <returns>The equivalent angle, in degrees.</returns>
    public static double Rtd(double r)
    {
        return (r * 180.0) / Math.PI;
    }
    /// <summary>Computes the sine of an angle expressed in degrees.</summary>
    /// <param name="d">The angle, in degrees.</param>
    /// <returns>The sine of the angle.</returns>
    public static double Sin(double d)
    {
        return Math.Sin(Dtr(d));
    }
    /// <summary>Computes the cosine of an angle expressed in degrees.</summary>
    /// <param name="d">The angle, in degrees.</param>
    /// <returns>The cosine of the angle.</returns>
    public static double Cos(double d)
    {
        return Math.Cos(Dtr(d));
    }
    /// <summary>Computes the tangent of an angle expressed in degrees.</summary>
    /// <param name="d">The angle, in degrees.</param>
    /// <returns>The tangent of the angle.</returns>
    public static double Tan(double d)
    {
        return Math.Tan(Dtr(d));
    }
    /// <summary>Computes the arcsine of a value and returns the result in degrees.</summary>
    /// <param name="d">The value whose arcsine is to be computed.</param>
    /// <returns>The angle, in degrees, whose sine is <paramref name="d"/>.</returns>
    public static double Arcsin(double d)
    {
        return Rtd(Math.Asin(d));
    }
    /// <summary>Computes the arccosine of a value and returns the result in degrees.</summary>
    /// <param name="d">The value whose arccosine is to be computed.</param>
    /// <returns>The angle, in degrees, whose cosine is <paramref name="d"/>.</returns>
    public static double Arccos(double d)
    {
        return Rtd(Math.Acos(d));
    }
    /// <summary>Computes the arctangent of a value and returns the result in degrees.</summary>
    /// <param name="d">The value whose arctangent is to be computed.</param>
    /// <returns>The angle, in degrees, whose tangent is <paramref name="d"/>.</returns>
    public static double Arctan(double d)
    {
        return Rtd(Math.Atan(d));
    }
    /// <summary>Computes the arccotangent of a value and returns the result in degrees.</summary>
    /// <param name="x">The value whose arccotangent is to be computed.</param>
    /// <returns>The angle, in degrees, whose cotangent is <paramref name="x"/>.</returns>
    public static double Arccot(double x)
    {
        return Rtd(Math.Atan(1 / x));
    }
    /// <summary>Computes the arctangent of <paramref name="y"/>/<paramref name="x"/> and returns the result in degrees.</summary>
    /// <param name="y">The y coordinate.</param>
    /// <param name="x">The x coordinate.</param>
    /// <returns>The angle, in degrees, between the positive x-axis and the point (<paramref name="x"/>, <paramref name="y"/>).</returns>
    public static double Arctan2(double y, double x)
    {
        return Rtd(Math.Atan2(y, x));
    }
    /// <summary>Normalizes a value into the range [0, <paramref name="b"/>).</summary>
    /// <param name="a">The value to normalize.</param>
    /// <param name="b">The exclusive upper bound of the range.</param>
    /// <returns>The normalized value.</returns>
    public static double Fix(double a, double b)
    {
        a = a - b * (Math.Floor(a / b));
        return (a < 0) ? a + b : a;
    }
    /// <summary>Normalizes an angle, in degrees, into the range [0, 360).</summary>
    /// <param name="a">The angle, in degrees, to normalize.</param>
    /// <returns>The normalized angle.</returns>
    public static double FixAngle(double a)
    {
        return Fix(a, 360);
    }
    /// <summary>Normalizes an hour value into the range [0, 24).</summary>
    /// <param name="a">The hour value to normalize.</param>
    /// <returns>The normalized hour value.</returns>
    public static double FixHour(double a)
    {
        return Fix(a, 24);
    }
}
