using System.Collections;
using PrayerTimesManager;
using PrayerTimesManager.Enums;

namespace PrayerTimesManagerApp;

public static class PrayerTimesExamples
{
    public static void RunAllExamples()
    {
        BasicHashtableExample();
        TypedResultExample();
        NowAndTomorrowExample();
        TimeProviderExample();
        InputRecordExample();
        DifferentFormatsExample();
        CustomSettingsExample();
    }

    private static void BasicHashtableExample()
    {
        Console.WriteLine("=== Basic Hashtable output ===");

        var prayerTimes = new PrayerTimes(PrayerCalculationMethods.MWL, Schools.STANDARD);
        Hashtable times = prayerTimes.GetTimes(
            latitude: 51.5074,
            longitude: -0.1278,
            dateTime: new DateTime(2024, 6, 15),
            timeZone: TimeZoneInfo.Utc);

        PrintHashtable(times);
    }

    private static void TypedResultExample()
    {
        Console.WriteLine("=== Typed PrayerTimesResult output ===");

        var prayerTimes = new PrayerTimes(PrayerCalculationMethods.MWL, Schools.STANDARD);
        PrayerTimesResult result = prayerTimes.GetTimesResult(
            latitude: 51.5074,
            longitude: -0.1278,
            dateTime: new DateTime(2024, 6, 15),
            timeZone: TimeZoneInfo.Utc);

        PrintResult(result);
    }

    private static void NowAndTomorrowExample()
    {
        Console.WriteLine("=== Now and Tomorrow (typed results, device local timezone) ===");

        var prayerTimes = new PrayerTimes();
        PrayerTimesResult today = prayerTimes.NowResult(51.5074, -0.1278);
        PrayerTimesResult tomorrow = prayerTimes.TomorrowResult(51.5074, -0.1278);

        Console.WriteLine($"Today Fajr:    {today.Fajr}");
        Console.WriteLine($"Tomorrow Fajr: {tomorrow.Fajr}");
    }

    private static void TimeProviderExample()
    {
        Console.WriteLine("=== Injecting a custom TimeProvider ===");

        var customTimeProvider = TimeProvider.System;
        var prayerTimes = new PrayerTimes(timeProvider: customTimeProvider);
        PrayerTimesResult today = prayerTimes.NowResult(51.5074, -0.1278);

        Console.WriteLine($"Today Fajr: {today.Fajr}");
    }

    private static void InputRecordExample()
    {
        Console.WriteLine("=== Using PrayerTimesInputs record ===");

        var prayerTimes = new PrayerTimes();
        var inputs = new PrayerTimesInputs(
            Latitude: 21.3891,
            Longitude: 39.8579,
            DateTime: new DateTime(2024, 6, 15),
            TimeZone: TimeZoneInfo.Utc,
            Elevation: 300,
            LatitudeAdjustmentMethod: LatitudeAdjustmentMethods.LATITUDE_ADJUSTMENT_METHOD_ANGLE,
            Format: TimeFormats.TIME_FORMAT_24H);

        PrayerTimesResult result = prayerTimes.GetTimesResult(inputs);
        PrintResult(result);
    }

    private static void DifferentFormatsExample()
    {
        Console.WriteLine("=== 12h vs 24h vs Float output ===");

        var prayerTimes = new PrayerTimes();
        DateTime date = new(2024, 6, 15);

        PrayerTimesResult result12h = prayerTimes.GetTimesResult(
            51.5074, -0.1278, date, TimeZoneInfo.Utc, format: TimeFormats.TIME_FORMAT_12H);

        PrayerTimesResult result24h = prayerTimes.GetTimesResult(
            51.5074, -0.1278, date, TimeZoneInfo.Utc, format: TimeFormats.TIME_FORMAT_24H);

        PrayerTimesResult resultFloat = prayerTimes.GetTimesResult(
            51.5074, -0.1278, date, TimeZoneInfo.Utc, format: TimeFormats.TIME_FORMAT_FLOAT);

        PrayerTimesResult resultIso = prayerTimes.GetTimesResult(
            51.5074, -0.1278, date, TimeZoneInfo.Utc, format: TimeFormats.TIME_FORMAT_ISO8601);

        Console.WriteLine($"12h:   {result12h.Fajr}");
        Console.WriteLine($"24h:   {result24h.Fajr}");
        Console.WriteLine($"Float: {resultFloat.Fajr}");
        Console.WriteLine($"ISO:   {resultIso.Fajr}");
    }

    private static void CustomSettingsExample()
    {
        Console.WriteLine("=== Custom method with tune offsets ===");

        var prayerTimes = new PrayerTimes(PrayerCalculationMethods.CUSTOM, Schools.STANDARD);
        prayerTimes.SetCustomMethod(PrayerCalculationMethod.Custom);
        prayerTimes.SetTuneTimeOffset(new Dictionary<string, double>
        {
            [PrayerTimes.FAJR] = 2,
            [PrayerTimes.ZHUHR] = 1,
            [PrayerTimes.MAGHRIB] = -1
        });

        PrayerTimesResult result = prayerTimes.GetTimesResult(
            51.5074, -0.1278, new DateTime(2024, 6, 15), TimeZoneInfo.Utc);

        PrintResult(result);
    }

    private static void PrintHashtable(Hashtable times)
    {
        foreach (DictionaryEntry entry in times)
        {
            Console.WriteLine($"{entry.Key}: {entry.Value}");
        }
    }

    private static void PrintResult(PrayerTimesResult result)
    {
        Console.WriteLine($"Imsak:   {result.Imsak}");
        Console.WriteLine($"Fajr:    {result.Fajr}");
        Console.WriteLine($"Sunrise: {result.Sunrise}");
        Console.WriteLine($"Dhuhr:   {result.Dhuhr}");
        Console.WriteLine($"Asr:     {result.Asr}");
        Console.WriteLine($"Sunset:  {result.Sunset}");
        Console.WriteLine($"Maghrib: {result.Maghrib}");
        Console.WriteLine($"Isha:    {result.Isha}");
        Console.WriteLine($"Midnight:{result.Midnight}");
    }
}
