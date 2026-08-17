using PrayersTimeManager.Enums;
using System.Collections;

namespace PrayersTimeManager;

public sealed record PrayerCalculationMethod
{
    public int Id { get; init; }
    public string Name { get; init; }
    public Hashtable Param { get; init; }
    public Hashtable? Location { get; init; }
    public string Description { get; init; } = string.Empty;

    private PrayerCalculationMethod(int id, string name, Hashtable param, string description, Hashtable? location)
    {
        Id = id;
        Name = name;
        Param = param;
        Location = location;
        Description = description;
    }

    public static PrayerCalculationMethod Jafari => new(id: 0, name: "Shia Ithna-Ashari, Leva Institute, Qum",
        param: new Hashtable()
        {
            [PrayerTimes.FAJR] = 16d,
            [PrayerTimes.ISHA] = 14d,
            [PrayerTimes.MAGHRIB] = 4d,
            [PrayerTimes.MIDNIGHT] = MidnightModes.JAFARI
        },
        location: new Hashtable()
        {
            ["latitude"] = 34.6415764d,
            ["longitude"] = 50.8746035d,
        },
        description: string.Format("({0} {1})", "Fajr angle 18", "Isha angle 14")
        );
    public static PrayerCalculationMethod Karachi => new(id: 1, name: "University of Islamic Sciences, Karachi",
        param: new Hashtable()
        {
            [PrayerTimes.FAJR] = 18d,
            [PrayerTimes.ISHA] = 18d
        },
        location: new Hashtable()
        {
            ["latitude"] = 24.8614622d,
            ["longitude"] = 67.0099388d,
        },
        description: string.Format("({0} {1})", "Fajr angle 18", "Isha angle 18")
        );
    public static PrayerCalculationMethod Isna => new(id: 2, name: "Islamic Society of North America (ISNA)",
        param: new Hashtable()
        {
            [PrayerTimes.FAJR] = 15d,
            [PrayerTimes.ISHA] = 15d
        },
        location: new Hashtable()
        {
            ["latitude"] = 39.70421229999999d,
            ["longitude"] = -86.39943869999999d,
        },
        description: string.Format("({0} {1})", "Fajr angle 15", "Isha angle 15")
        );
    public static PrayerCalculationMethod Mwl => new(id: 3, name: "Muslim World League",
        param: new Hashtable()
        {
            [PrayerTimes.FAJR] = 18d,
            [PrayerTimes.ISHA] = 17d
        },
        location: new Hashtable()
        {
            ["latitude"] = 51.5194682d,
            ["longitude"] = -0.1360365d,
        },
        description: string.Format("({0} {1})", "Fajr angle 18", "Isha angle 17")
        );
    public static PrayerCalculationMethod Makkah => new(id: 4, name: "Umm Al-Qura University, Makkah",
        param: new Hashtable()
        {
            [PrayerTimes.FAJR] = 18.5d,
            [PrayerTimes.ISHA] = "90 min"
        },
        location: new Hashtable()
        {
            ["latitude"] = 21.3890824d,
            ["longitude"] = 39.8579118d,
        },
        description: string.Format("({0} {1})", "Fajr angle 18.5", "Isha angle 90")
        );
    public static PrayerCalculationMethod Egypt => new(id: 5, name: "Egyptian General Authority of Survey",
        param: new()
        {
            [PrayerTimes.FAJR] = 19.5d,
            [PrayerTimes.ISHA] = 17.5d
        }, location: new()
        {
            ["latitude"] = 30.0444196d,
            ["longitude"] = 31.2357116d,
        },
        description: string.Format("({0} {1})", "Fajr angle 19.5", "Isha angle 17.5")
        );
    public static PrayerCalculationMethod Tehran => new(id: 7, name: "Institute of Geophysics, University of Tehran",
        param: new Hashtable()
        {
            [PrayerTimes.FAJR] = 17.7d,
            [PrayerTimes.ISHA] = 14d,
            [PrayerTimes.MAGHRIB] = 4.5d,
            [PrayerTimes.MIDNIGHT] = MidnightModes.JAFARI
        },
        location: new Hashtable()
        {
            ["latitude"] = 35.6891975d,
            ["longitude"] = 51.3889736d,
        },
        description: string.Format("({0} {1})", "Fajr angle 17.5", "Isha angle 14")
        );
    public static PrayerCalculationMethod Gulf => new(id: 8, name: "Gulf Region",
        param: new Hashtable()
        {
            [PrayerTimes.FAJR] = 19.5d,
            [PrayerTimes.ISHA] = "90 min"
        },
        location: new Hashtable()
        {
            ["latitude"] = 25.2048493d,
            ["longitude"] = 55.2707828d,
        },
        description: string.Format("({0} {1})", "Fajr angle 19.5", "Isha angle 90")
        );
    public static PrayerCalculationMethod Kuwait => new(id: 9, name: "Kuwait",
        param: new Hashtable()
        {
            [PrayerTimes.FAJR] = 18d,
            [PrayerTimes.ISHA] = 17.5d
        },
        location: new Hashtable()
        {
            ["latitude"] = 29.375859d,
            ["longitude"] = 47.9774052d,
        },
        description: string.Format("({0} {1})", "Fajr angle 18", "Isha angle 17.5"));
    public static PrayerCalculationMethod Qatar => new(id: 10, name: "Qatar",
        param: new Hashtable()
        {
            [PrayerTimes.FAJR] = 18d,
            [PrayerTimes.ISHA] = "90 min"
        },
        location: new Hashtable()
        {
            ["latitude"] = 25.2854473d,
            ["longitude"] = 51.5310398d,
        },
        description: string.Format("({0} {1})", "Fajr angle 18", "Isha angle 90"));
    public static PrayerCalculationMethod Singapore => new(id: 11, name: "Majlis Ugama Islam Singapura, Singapore",
            param: new Hashtable()
            {
                [PrayerTimes.FAJR] = 20d,
                [PrayerTimes.ISHA] = 18d
            },
            location: new Hashtable()
            {
                ["latitude"] = 1.352083d,
                ["longitude"] = 103.819836d,
            },
            description: string.Format("({0} {1})", "Fajr angle 20", "Isha angle 18")
            );
    public static PrayerCalculationMethod France => new(id: 12, name: "Union Organization Islamic de France",
        param: new Hashtable()
        {
            [PrayerTimes.FAJR] = 12d,
            [PrayerTimes.ISHA] = 12d
        },
        location: new Hashtable()
        {
            ["latitude"] = 48.856614d,
            ["longitude"] = 2.3522219d,
        },
        description: string.Format("({0} {1})", "Fajr angle 12", "Isha angle 12")
        );
    public static PrayerCalculationMethod Turkey => new(id: 13, name: "Diyanet İşleri Başkanlığı, Turkey",
        param: new Hashtable()
        {
            [PrayerTimes.FAJR] = 18d,
            [PrayerTimes.ISHA] = 17d
        },
        location: new Hashtable()
        {
            ["latitude"] = 39.9333635d,
            ["longitude"] = 32.8597419d,
        },
        description: string.Format("({0} {1})", "Fajr angle 18", "Isha angle 17")
        );
    public static PrayerCalculationMethod Russia => new(id: 14, name: "Spiritual Administration of Muslims of Russia",
        param: new Hashtable()
        {
            [PrayerTimes.FAJR] = 16d,
            [PrayerTimes.ISHA] = 15d
        },
        location: new Hashtable()
        {
            ["latitude"] = 54.73479099999999d,
            ["longitude"] = 55.9578555d,
        },
        description: string.Format("{0}", "Fajr angle 16, Isha angle 15")
        );
    public static PrayerCalculationMethod Moonsighting => new(id: 15, name: "Moonsighting Committee Worldwide (Moonsighting.com)",
        param: new Hashtable()
        {
            ["shafaq"] = Isha.shafaq
        },
        location: null,
        description: string.Format("{0}", "Fajr angle 18, Isha angle 18. Also uses seasonal adjustement values")
        );
    public static PrayerCalculationMethod Algeria => new(id: 16, name: "Algerian Minister of Religious Affairs and Wakfs",
        param: new Hashtable()
        {
            [PrayerTimes.FAJR] = 18d,
            [PrayerTimes.ISHA] = 17d
        },
        location: new Hashtable()
        {
            ["latitude"] = 36.625374d,
            ["longitude"] = 2.727458d,
        },
        description: string.Format("{0}", "Fajr angle 18, Isha angle 17")
        );
    public static PrayerCalculationMethod Basque => new(id: 17, name: "Basque Country",
        param: new Hashtable()
        {
            [PrayerTimes.FAJR] = 15d,
            [PrayerTimes.ISHA] = 15d
        },
        location: new Hashtable()
        {
            ["latitude"] = 43.25694d,
            ["longitude"] = -2.92361d,
        },
        description: string.Format("{0}", "Fajr angle 15, Isha angle 15")
        );
    public static PrayerCalculationMethod JAKIM => new(id: 18, name: "(JAKIM) Jabatan Kemajuan Islam Malaysia",
        param: new Hashtable()
        {
            [PrayerTimes.FAJR] = 20d,
            [PrayerTimes.ISHA] = 18d
        },
        location: new Hashtable()
        {
            ["latitude"] = 2.926361d,
            ["longitude"] = 101.696445d,
        },
        description: string.Format("{0}", "Fajr angle 20, Isha angle 18")
        );
    public static PrayerCalculationMethod Tunisia => new(id: 19, name: "Tunisian Ministry of Religious Affairs",
        param: new Hashtable()
        {
            [PrayerTimes.FAJR] = 18d,
            [PrayerTimes.ISHA] = 17d
        },
        location: new Hashtable()
        {
            ["latitude"] = 35.6833333d,
            ["longitude"] = 10.7d,
        },
        description: string.Format("{0}", "Fajr angle 18, Isha angle 17."));
    public static PrayerCalculationMethod Custom => new(id: 20, name: "Custom",
        param: new Hashtable()
        {
            [PrayerTimes.FAJR] = 15,
            [PrayerTimes.ISHA] = 15,
        },
        location: new Hashtable()
        {
            ["latitude"] = 54.73479099999999d,
            ["longitude"] = 55.9578555d,
        },
        description: string.Empty);
    public static PrayerCalculationMethod Indonesia => new(id: 21, name: "Indonesia (Kementerian Agama)",
        param: new Hashtable()
        {
            [PrayerTimes.FAJR] = 20d,
            [PrayerTimes.ISHA] = 18d
        },
        location: new Hashtable()
        {
            ["latitude"] = 6.200000d,
            ["longitude"] = 106.816666d,
        },
        description: string.Format("{0}", "Fajr angle 20, Isha angle 18.")
        );
}