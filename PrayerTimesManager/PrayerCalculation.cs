using PrayerTimesManager.Enums;
using System.Collections;

namespace PrayerTimesManager;

/// <summary>
/// Provides configuration of custom calculation parameters and lookups of the built-in
/// <see cref="PrayerCalculationMethod"/> definitions.
/// </summary>
public sealed class PrayerCalculation
{
    private readonly Hashtable paramMethods = [];

    /// <summary>Sets the custom Fajr angle, in degrees.</summary>
    /// <param name="angle">The Fajr angle, in degrees, below the horizon.</param>
    public void SetFajrAngle(double angle)
    {
        paramMethods[PrayerTimes.FAJR] = angle;
    }

    /// <summary>Sets the custom Maghrib angle or number of minutes after sunset.</summary>
    /// <param name="angleOrMinsAfterSunset">The Maghrib angle, in degrees, or minutes after sunset.</param>
    public void SetMaghribAngleOrMins(double angleOrMinsAfterSunset)
    {
        paramMethods[PrayerTimes.MAGHRIB] = angleOrMinsAfterSunset;
    }

    /// <summary>Sets the custom Isha angle or number of minutes after Maghrib.</summary>
    /// <param name="angleOrMinsAfterMaghrib">The Isha angle, in degrees, or minutes after Maghrib.</param>
    public void SetIshaAngleOrMins(double angleOrMinsAfterMaghrib)
    {
        paramMethods[PrayerTimes.ISHA] = angleOrMinsAfterMaghrib;
    }

    /// <summary>Gets a lookup table mapping each <see cref="PrayerCalculationMethods"/> code to itself.</summary>
    public static Hashtable PrayerCalculationsCodes => new()
    {
        [(short)PrayerCalculationMethods.JAFARI] = (short)PrayerCalculationMethods.JAFARI,
        [(short)PrayerCalculationMethods.KARACHI] = (short)PrayerCalculationMethods.KARACHI,
        [(short)PrayerCalculationMethods.ISNA] = (short)PrayerCalculationMethods.ISNA,
        [(short)PrayerCalculationMethods.MWL] = (short)PrayerCalculationMethods.MWL,
        [(short)PrayerCalculationMethods.MAKKAH] = (short)PrayerCalculationMethods.MAKKAH,
        [(short)PrayerCalculationMethods.EGYPT] = (short)PrayerCalculationMethods.EGYPT,
        [(short)PrayerCalculationMethods.TEHRAN] = (short)PrayerCalculationMethods.TEHRAN,
        [(short)PrayerCalculationMethods.GULF] = (short)PrayerCalculationMethods.GULF,
        [(short)PrayerCalculationMethods.KUWAIT] = (short)PrayerCalculationMethods.KUWAIT,
        [(short)PrayerCalculationMethods.QATAR] = (short)PrayerCalculationMethods.QATAR,
        [(short)PrayerCalculationMethods.SINGAPORE] = (short)PrayerCalculationMethods.SINGAPORE,
        [(short)PrayerCalculationMethods.FRANCE] = (short)PrayerCalculationMethods.FRANCE,
        [(short)PrayerCalculationMethods.TURKEY] = (short)PrayerCalculationMethods.TURKEY,
        [(short)PrayerCalculationMethods.RUSSIA] = (short)PrayerCalculationMethods.RUSSIA,
        [(short)PrayerCalculationMethods.MOONSIGHTING] = (short)PrayerCalculationMethods.MOONSIGHTING,
        [(short)PrayerCalculationMethods.ALGERIA] = (short)PrayerCalculationMethods.ALGERIA,
        [(short)PrayerCalculationMethods.BASQUE] = (short)PrayerCalculationMethods.BASQUE,
        [(short)PrayerCalculationMethods.JAKIM] = (short)PrayerCalculationMethods.JAKIM,
        [(short)PrayerCalculationMethods.TUNISIA] = (short)PrayerCalculationMethods.TUNISIA,
        [(short)PrayerCalculationMethods.CUSTOM] = (short)PrayerCalculationMethods.CUSTOM,
        [(short)PrayerCalculationMethods.INDONESIA] = (short)PrayerCalculationMethods.INDONESIA
    };

    /// <summary>Gets a dictionary mapping each calculation method name to its <see cref="PrayerCalculationMethod"/> definition.</summary>
    public static Dictionary<string, PrayerCalculationMethod> PrayerCalculations
    {
        get
        {
            Dictionary<string, PrayerCalculationMethod> methods = new(StringComparer.OrdinalIgnoreCase)
            {
                {
                    PrayerCalculationMethods.MWL.ToString(),
                    PrayerCalculationMethod.Mwl
                },
                {
                    PrayerCalculationMethods.ISNA.ToString(),
                    PrayerCalculationMethod.Isna
                },
                {
                    PrayerCalculationMethods.EGYPT.ToString(),
                    PrayerCalculationMethod.Egypt
                },
                {
                    PrayerCalculationMethods.MAKKAH.ToString(),
                    PrayerCalculationMethod.Makkah
                },
                {
                    PrayerCalculationMethods.KARACHI.ToString(),
                    PrayerCalculationMethod.Karachi
                },
                {
                    PrayerCalculationMethods.TEHRAN.ToString(),
                    PrayerCalculationMethod.Tehran
                },
                {
                    PrayerCalculationMethods.JAFARI.ToString(),
                    PrayerCalculationMethod.Jafari
                },
                {
                    PrayerCalculationMethods.GULF.ToString(),
                    PrayerCalculationMethod.Gulf
                },
                {
                    PrayerCalculationMethods.KUWAIT.ToString(),
                    PrayerCalculationMethod.Kuwait
                },
                {
                    PrayerCalculationMethods.QATAR.ToString(),
                    PrayerCalculationMethod.Qatar
                },
                {
                    PrayerCalculationMethods.SINGAPORE.ToString(),
                    PrayerCalculationMethod.Singapore
                },
                {
                    PrayerCalculationMethods.FRANCE.ToString(),
                    PrayerCalculationMethod.France
                },
                {
                    PrayerCalculationMethods.TURKEY.ToString(),
                    PrayerCalculationMethod.Turkey
                },
                {
                    PrayerCalculationMethods.RUSSIA.ToString(),
                    PrayerCalculationMethod.Russia
                },
                {
                    PrayerCalculationMethods.MOONSIGHTING.ToString(),
                    PrayerCalculationMethod.Moonsighting
                },
                {
                    PrayerCalculationMethods.ALGERIA.ToString(),
                    PrayerCalculationMethod.Algeria
                },
                {
                    PrayerCalculationMethods.BASQUE.ToString(),
                    PrayerCalculationMethod.Basque
                },
                {
                    PrayerCalculationMethods.JAKIM.ToString(),
                    PrayerCalculationMethod.JAKIM
                },
                {
                    PrayerCalculationMethods.TUNISIA.ToString(),
                    PrayerCalculationMethod.Tunisia
                },
                {
                    PrayerCalculationMethods.INDONESIA.ToString(),
                    PrayerCalculationMethod.Indonesia
                },
                {
                    PrayerCalculationMethods.CUSTOM.ToString(),
                    PrayerCalculationMethod.Custom
                }
            };

            return methods;
        }
    }
}