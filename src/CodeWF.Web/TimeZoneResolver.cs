namespace CodeWF.Web;

public interface ITimeZoneResolver
{
    DateTime NowOfTimeZone { get; }

    DateTime ToTimeZone(DateTime utcTime);
    DateTime ToUtc(DateTime userTime);
    IEnumerable<TimeZoneInfo> ListTimeZones();
    TimeSpan GetTimeSpanByZoneId(string timeZoneId);
}

public class BlogTimeZoneResolver(IBlogConfig blogConfig) : ITimeZoneResolver
{
    public TimeSpan UtcOffset { get; } = blogConfig.GeneralSettings.TimeZoneUtcOffset;

    public DateTime NowOfTimeZone => UtcToZoneTime(DateTime.UtcNow, UtcOffset);

    public DateTime ToTimeZone(DateTime utcTime)
    {
        return UtcToZoneTime(utcTime, UtcOffset);
    }

    public DateTime ToUtc(DateTime userTime)
    {
        return ZoneTimeToUtc(userTime, UtcOffset);
    }

    public IEnumerable<TimeZoneInfo> ListTimeZones()
    {
        return TimeZoneInfo.GetSystemTimeZones();
    }

    public TimeSpan GetTimeSpanByZoneId(string timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            return TimeSpan.Zero;
        }

        TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        return tz.BaseUtcOffset;
    }

    #region Private

    private DateTime UtcToZoneTime(DateTime utcTime, TimeSpan timeSpan)
    {
        TimeSpan span = ParseTimeZone(timeSpan, out TimeZoneInfo? tz);
        return tz is not null ? TimeZoneInfo.ConvertTimeFromUtc(utcTime, tz) : utcTime.AddTicks(span.Ticks);
    }

    private DateTime ZoneTimeToUtc(DateTime zoneTime, TimeSpan timeSpan)
    {
        TimeSpan span = ParseTimeZone(timeSpan, out TimeZoneInfo? tz);
        return tz is not null ? TimeZoneInfo.ConvertTimeToUtc(zoneTime, tz) : zoneTime.AddTicks(-1 * span.Ticks);
    }

    private TimeSpan ParseTimeZone(TimeSpan timeSpan, out TimeZoneInfo tz)
    {
        tz = ListTimeZones().FirstOrDefault(t => t.BaseUtcOffset == timeSpan);
        return timeSpan;
    }

    #endregion
}