namespace PrayerTimesManager.Enums;

/// <summary>
/// Specifies the prayer time calculation method, which determines the Fajr and Isha angles
/// (and other parameters) used by <see cref="PrayerCalculationMethod"/>.
/// </summary>
public enum PrayerCalculationMethods
{
    /// <summary>Shia Ithna-Ashari, Leva Institute, Qum.</summary>
    JAFARI = 0,
    /// <summary>University of Islamic Sciences, Karachi.</summary>
    KARACHI = 1,
    /// <summary>Islamic Society of North America (ISNA).</summary>
    ISNA = 2,
    /// <summary>Muslim World League.</summary>
    MWL = 3,
    /// <summary>Umm Al-Qura University, Makkah.</summary>
    MAKKAH = 4,
    /// <summary>Egyptian General Authority of Survey.</summary>
    EGYPT = 5,
    /// <summary>Institute of Geophysics, University of Tehran.</summary>
    TEHRAN = 7,
    /// <summary>Gulf Region.</summary>
    GULF = 8,
    /// <summary>Kuwait.</summary>
    KUWAIT = 9,
    /// <summary>Qatar.</summary>
    QATAR = 10,
    /// <summary>Majlis Ugama Islam Singapura, Singapore.</summary>
    SINGAPORE = 11,
    /// <summary>Union Organization Islamic de France.</summary>
    FRANCE = 12,
    /// <summary>Diyanet İşleri Başkanlığı, Turkey.</summary>
    TURKEY = 13,
    /// <summary>Spiritual Administration of Muslims of Russia.</summary>
    RUSSIA = 14,
    /// <summary>Moonsighting Committee Worldwide (Moonsighting.com).</summary>
    MOONSIGHTING = 15,
    /// <summary>Algerian Minister of Religious Affairs and Wakfs.</summary>
    ALGERIA = 16,
    /// <summary>Basque Country.</summary>
    BASQUE = 17,
    /// <summary>(JAKIM) Jabatan Kemajuan Islam Malaysia.</summary>
    JAKIM = 18,
    /// <summary>Tunisian Ministry of Religious Affairs.</summary>
    TUNISIA = 19,
    /// <summary>A custom, user-defined calculation method.</summary>
    CUSTOM = 20,
    /// <summary>Indonesia (Kementerian Agama).</summary>
    INDONESIA = 21,
    /// <summary>The default calculation method, equivalent to <see cref="MWL"/>.</summary>
    DEFAULT = MWL
}
