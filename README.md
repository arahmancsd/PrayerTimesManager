# PrayerTimesManager

[![NuGet](https://img.shields.io/nuget/v/PrayerTimesManager.svg)](https://www.nuget.org/packages/PrayerTimesManager/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE.txt)

A lightweight .NET library for calculating Islamic prayer times (Imsak, Fajr, Sunrise, Dhuhr, Asr, Sunset, Maghrib, Isha, and Midnight) for any location, date, and time zone.

It supports 20+ built-in calculation methods used around the world (MWL, ISNA, Umm Al-Qura, Egypt, Karachi, Moonsighting Committee, and more), configurable juristic schools (Standard/Hanafi) for Asr, latitude adjustment methods for high-latitude locations, custom methods, and multiple output formats.

## Features

- Accurate astronomical calculation of prayer times based on latitude, longitude, elevation, and date
- 20+ built-in calculation methods (e.g. `MWL`, `ISNA`, `EGYPT`, `KARACHI`, `MAKKAH`, `MOONSIGHTING`, `TURKEY`, `RUSSIA`, `INDONESIA`, ...) plus support for defining your own custom method
- Asr juristic school selection (`STANDARD` / `HANAFI`)
- High-latitude adjustment methods (angle-based, one-seventh, middle-of-the-night, or none)
- Multiple time output formats: 24-hour, 12-hour, 12-hour without suffix, floating point, or ISO 8601
- Per-prayer time tuning/offsets
- Strongly-typed `PrayerTimesResult` output, or raw `Hashtable` if you need the original keys
- Convenience `NowResult` / `TomorrowResult` helpers, with `TimeProvider` support for testability
- Targets `net10.0` with nullable reference types enabled

## Installation

Install the [PrayerTimesManager](https://www.nuget.org/packages/PrayerTimesManager) package from NuGet:

```bash
dotnet add package PrayerTimesManager
```

Or via the NuGet Package Manager:

```powershell
Install-Package PrayerTimesManager
```

## Getting started

```csharp
using PrayerTimesManager;
using PrayerTimesManager.Enums;
using PrayersTimeManager.Enums;

var prayerTimes = new PrayerTimes(PrayerCalculationMethods.MWL, Schools.STANDARD);

PrayerTimesResult result = prayerTimes.GetTimesResult(
    latitude: 51.5074,
    longitude: -0.1278,
    dateTime: new DateTime(2024, 6, 15),
    timeZone: TimeZoneInfo.Utc);

Console.WriteLine($"Fajr:    {result.Fajr}");
Console.WriteLine($"Sunrise: {result.Sunrise}");
Console.WriteLine($"Dhuhr:   {result.Dhuhr}");
Console.WriteLine($"Asr:     {result.Asr}");
Console.WriteLine($"Maghrib: {result.Maghrib}");
Console.WriteLine($"Isha:    {result.Isha}");
```

### Getting times for today and tomorrow

```csharp
var prayerTimes = new PrayerTimes();

PrayerTimesResult today = prayerTimes.NowResult(latitude: 51.5074, longitude: -0.1278);
PrayerTimesResult tomorrow = prayerTimes.TomorrowResult(latitude: 51.5074, longitude: -0.1278);
```

### Using the `PrayerTimesInputs` record

```csharp
var inputs = new PrayerTimesInputs(
    Latitude: 21.4225,
    Longitude: 39.8262,
    DateTime: new DateTime(2024, 6, 15),
    TimeZone: TimeZoneInfo.Utc,
    Format: TimeFormats.TIME_FORMAT_24H);

var prayerTimes = new PrayerTimes(PrayerCalculationMethods.MAKKAH);
PrayerTimesResult result = prayerTimes.GetTimesResult(inputs);
```

### Raw `Hashtable` output

If you need the original key-based output (e.g. `PrayerTimes.FAJR`, `PrayerTimes.ISHA`), use `GetTimes` instead of `GetTimesResult`:

```csharp
Hashtable times = prayerTimes.GetTimes(latitude: 51.5074, longitude: -0.1278);
Console.WriteLine(times[PrayerTimes.FAJR]);
```

### Choosing a calculation method and school

```csharp
var prayerTimes = new PrayerTimes(PrayerCalculationMethods.ISNA, Schools.HANAFI);

// or change them later
prayerTimes.SetMethod(PrayerCalculationMethods.EGYPT);
prayerTimes.SetSchool(Schools.HANAFI);
```

### Custom calculation methods

```csharp
var customMethod = PrayerCalculationMethod.Custom with
{
    Param = new Hashtable
    {
        [PrayerTimes.FAJR] = 16d,
        [PrayerTimes.ISHA] = 15d
    }
};

prayerTimes.SetCustomMethod(customMethod);
```

### Fine-tuning individual prayer times

```csharp
prayerTimes.SetTuneTimeOffset(new Dictionary<string, double>
{
    [PrayerTimes.FAJR] = 2,   // add 2 minutes to Fajr
    [PrayerTimes.ISHA] = -3   // subtract 3 minutes from Isha
});
```

### Testable time-dependent calls

`PrayerTimes` accepts a `TimeProvider`, so `NowResult`/`TomorrowResult` can be tested deterministically:

```csharp
var fakeTime = new FakeTimeProvider(startDateTime: new DateTimeOffset(2024, 6, 15, 0, 0, 0, TimeSpan.Zero));
var prayerTimes = new PrayerTimes(timeProvider: fakeTime);
```

## Calculation methods

| Method | Description |
| --- | --- |
| `JAFARI` | Shia Ithna-Ashari, Leva Institute, Qum |
| `KARACHI` | University of Islamic Sciences, Karachi |
| `ISNA` | Islamic Society of North America |
| `MWL` | Muslim World League (default) |
| `MAKKAH` | Umm Al-Qura University, Makkah |
| `EGYPT` | Egyptian General Authority of Survey |
| `TEHRAN` | Institute of Geophysics, University of Tehran |
| `GULF` | Gulf Region |
| `KUWAIT` | Kuwait |
| `QATAR` | Qatar |
| `SINGAPORE` | Majlis Ugama Islam Singapura, Singapore |
| `FRANCE` | Union Organization Islamic de France |
| `TURKEY` | Diyanet İşleri Başkanlığı, Turkey |
| `RUSSIA` | Spiritual Administration of Muslims of Russia |
| `MOONSIGHTING` | Moonsighting Committee Worldwide |
| `ALGERIA` | Algerian Minister of Religious Affairs and Wakfs |
| `BASQUE` | Basque Country |
| `JAKIM` | Jabatan Kemajuan Islam Malaysia |
| `TUNISIA` | Tunisian Ministry of Religious Affairs |
| `INDONESIA` | Indonesia (Kementerian Agama) |
| `CUSTOM` | User-defined parameters |

## Requirements

- .NET 10.0 or later

## Repository

Source code and issue tracker: <https://github.com/arahmancsd/PrayerTimesManager>

## License

This project is licensed under the [MIT License](LICENSE.txt).
