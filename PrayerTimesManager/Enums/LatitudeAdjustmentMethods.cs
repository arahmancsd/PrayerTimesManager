namespace PrayerTimesManager.Enums;

/// <summary>
/// Specifies the method used to adjust prayer times for locations at high latitudes,
/// where the standard sun-angle based calculation may not be valid.
/// </summary>
public enum LatitudeAdjustmentMethods
{
    /// <summary>No adjustment is applied (standard solar calculation).</summary>
    LATITUDE_ADJUSTMENT_METHOD_NONE = 0,
    /// <summary>Fajr and Isha are calculated as the midpoint of the night between sunset and sunrise.</summary>
    LATITUDE_ADJUSTMENT_METHOD_MOTN = 1, // MIDDLE_OF_THE_NIGHT
    /// <summary>Fajr and Isha are calculated as one seventh of the night from sunset to sunrise.</summary>
    LATITUDE_ADJUSTMENT_METHOD_ONESEVENTH = 2, // ONE_SEVENTH
    /// <summary>Fajr and Isha are calculated as a portion of the night based on the sun angle divided by 60.</summary>
    LATITUDE_ADJUSTMENT_METHOD_ANGLE = 3, // angle/60th of night
    /// <summary>The default latitude adjustment method, equivalent to <see cref="LATITUDE_ADJUSTMENT_METHOD_ANGLE"/>.</summary>
    DEFAULT = LATITUDE_ADJUSTMENT_METHOD_ANGLE

    //{ "None", "No adjustment needed (standard solar calculation)" },
    //{ "MiddleOfNight", "Fajr and Isha calculated as midpoint between sunset and sunrise" },
    //{ "OneSeventh", "Fajr and Isha calculated as 1/7th of the night from sunset to sunrise" },
    //{ "NearestLatitude_45", "Fajr and Isha based on prayer times at 45° latitude" },
    //{ "NearestLatitude_48", "Fajr and Isha based on prayer times at 48° latitude" },
    //{ "NearestDay", "Fajr and Isha based on the last day with normal sunrise/sunset" }
}
