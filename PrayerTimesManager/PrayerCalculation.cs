using PrayerTimesManager.Enums;
using System.Collections;

namespace PrayerTimesManager;

public sealed class PrayerCalculation
{
    private readonly Hashtable paramMethods = [];

    public void SetFajrAngle(double angle)
    {
        paramMethods[PrayerTimes.FAJR] = angle;
    }

    public void SetMaghribAngleOrMins(double angleOrMinsAfterSunset)
    {
        paramMethods[PrayerTimes.MAGHRIB] = angleOrMinsAfterSunset;
    }

    public void SetIshaAngleOrMins(double angleOrMinsAfterMaghrib)
    {
        paramMethods[PrayerTimes.ISHA] = angleOrMinsAfterMaghrib;
    }

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