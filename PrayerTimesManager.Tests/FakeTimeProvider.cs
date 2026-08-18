namespace PrayerTimesManager.Tests;

public sealed class FakeTimeProvider : TimeProvider
{
    private DateTimeOffset _now;

    public FakeTimeProvider(DateTimeOffset now, TimeZoneInfo? localTimeZone = null)
    {
        _now = now;
        LocalTimeZone = localTimeZone ?? TimeZoneInfo.Local;
    }

    public override DateTimeOffset GetUtcNow() => _now;

    public override TimeZoneInfo LocalTimeZone { get; }

    public void SetUtcNow(DateTimeOffset now) => _now = now;
}
