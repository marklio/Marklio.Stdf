using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// SSR — Scan Structure Record (1, 93).
/// V4-2007. Defines a scan structure with a name and a list of chain numbers.
/// </summary>
[StdfRecord(1, 93)]
public partial record struct Ssr
{
    public string? SsrName { get; set; }

    [WireCount("chn")] private ushort ChainCount => throw new NotSupportedException();
    [CountedArray("chn")] public ushort[]? ChainList { get; set; }
}
