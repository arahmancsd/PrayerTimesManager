namespace PrayersTimeManager;

public class DMath
{
    public static double Dtr(double d)
    {
        return (d * Math.PI) / 180.0;
    }
    public static double Rtd(double r)
    {
        return (r * 180.0) / Math.PI;
    }
    public static double Sin(double d)
    {
        return Math.Sin(Dtr(d));
    }
    public static double Cos(double d)
    {
        return Math.Cos(Dtr(d));
    }
    public static double Tan(double d)
    {
        return Math.Tan(Dtr(d));
    }
    public static double Arcsin(double d)
    {
        return Rtd(Math.Asin(d));
    }
    public static double Arccos(double d)
    {
        return Rtd(Math.Acos(d));
    }
    public static double Arctan(double d)
    {
        return Rtd(Math.Atan(d));
    }
    public static double Arccot(double x)
    {
        return Rtd(Math.Atan(1 / x));
    }
    public static double Arctan2(double y, double x)
    {
        return Rtd(Math.Atan2(y, x));
    }
    public static double Fix(double a, double b)
    {
        a = a - b * (Math.Floor(a / b));
        return (a < 0) ? a + b : a;
    }
    public static double FixAngle(double a)
    {
        return Fix(a, 360);
    }
    public static double FixHour(double a)
    {
        return Fix(a, 24);
    }
}
