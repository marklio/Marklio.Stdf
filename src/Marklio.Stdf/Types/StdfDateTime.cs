namespace Marklio.Stdf.Types;

/// <summary>
/// Helpers for converting between STDF U*4 Unix timestamps and <see cref="DateTime"/>.
/// </summary>
public static class StdfDateTime
{
    private static readonly DateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>Converts a STDF U*4 Unix timestamp to <see cref="DateTime"/>.</summary>
    public static DateTime FromStdf(uint seconds) =>
        seconds == 0 ? default : UnixEpoch.AddSeconds(seconds);

    /// <summary>Converts a <see cref="DateTime"/> to a STDF U*4 Unix timestamp.</summary>
    public static uint ToStdf(DateTime value) =>
        value == default ? 0u : (uint)(value.ToUniversalTime() - UnixEpoch).TotalSeconds;
}
