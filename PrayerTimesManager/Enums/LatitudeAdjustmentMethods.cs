namespace PrayersTimeManager.Enums;

public enum LatitudeAdjustmentMethods
{
    LATITUDE_ADJUSTMENT_METHOD_NONE = 0,
    LATITUDE_ADJUSTMENT_METHOD_MOTN = 1, // MIDDLE_OF_THE_NIGHT
    LATITUDE_ADJUSTMENT_METHOD_ONESEVENTH = 2, // ONE_SEVENTH
    LATITUDE_ADJUSTMENT_METHOD_ANGLE = 3, // angle/60th of night
    DEFAULT = LATITUDE_ADJUSTMENT_METHOD_ANGLE

    //{ "None", "No adjustment needed (standard solar calculation)" },
    //{ "MiddleOfNight", "Fajr and Isha calculated as midpoint between sunset and sunrise" },
    //{ "OneSeventh", "Fajr and Isha calculated as 1/7th of the night from sunset to sunrise" },
    //{ "NearestLatitude_45", "Fajr and Isha based on prayer times at 45° latitude" },
    //{ "NearestLatitude_48", "Fajr and Isha based on prayer times at 48° latitude" },
    //{ "NearestDay", "Fajr and Isha based on the last day with normal sunrise/sunset" }
}
