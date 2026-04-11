namespace Marklio.Stdf.Types;

/// <summary>
/// Helpers for converting between STDF U*4 Unix timestamps and <see cref="DateTime"/>.
/// </summary>
public static class StdfDateTime
{
    private static readonly DateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// The maximum <see cref="DateTime"/> representable in a STDF U*4 timestamp
    /// (<c>uint.MaxValue</c> seconds past the Unix epoch: 2106-02-07T06:28:15Z).
    /// </summary>
    public static readonly DateTime MaxValue = UnixEpoch.AddSeconds(uint.MaxValue);

    /// <summary>Converts a STDF U*4 Unix timestamp to <see cref="DateTime"/>.</summary>
    public static DateTime FromStdf(uint seconds) =>
        seconds == 0 ? default : UnixEpoch.AddSeconds(seconds);

    /// <summary>
    /// Converts a <see cref="DateTime"/> to a STDF U*4 Unix timestamp.
    /// </summary>
    /// <remarks>
    /// <c>default(DateTime)</c> is mapped to 0 (meaning "not specified" in STDF).
    /// The valid range is 1970-01-01T00:00:01Z through 2106-02-07T06:28:15Z.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is before the Unix epoch or after <see cref="MaxValue"/>.
    /// </exception>
    public static uint ToStdf(DateTime value)
    {
        if (value == default)
            return 0u;

        DateTime utc = value.ToUniversalTime();
        double totalSeconds = (utc - UnixEpoch).TotalSeconds;

        if (totalSeconds < 0)
            throw new ArgumentOutOfRangeException(nameof(value),
                value,
                $"STDF timestamps cannot represent dates before the Unix epoch (1970-01-01). Got: {value:O}");

        if (totalSeconds > uint.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(value),
                value,
                $"STDF timestamps cannot represent dates after 2106-02-07T06:28:15Z. Got: {value:O}");

        return (uint)totalSeconds;
    }
}
