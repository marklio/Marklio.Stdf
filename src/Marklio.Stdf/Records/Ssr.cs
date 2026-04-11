using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// SSR — Scan Structure Record (1, 93).
/// V4-2007. Defines a scan structure (named set of chains).
/// </summary>
[StdfRecord(1, 93)]
public partial record struct Ssr
{
    /// <summary>
    /// Name of the scan structure.
    /// [STDF: SSR_NAM, C*n]
    /// </summary>
    public string? SsrName { get; set; }

    [WireCount("chn")] private ushort ChainCount => throw new NotSupportedException();

    /// <summary>
    /// List of chain numbers (CDR indexes) in this scan structure.
    /// [STDF: CHN_LIST, kxU*2]
    /// </summary>
    [CountedArray("chn")] public ushort[]? ChainList { get; set; }
}
