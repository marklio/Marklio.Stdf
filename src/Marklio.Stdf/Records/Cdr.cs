using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// CDR — Chain Description Record (1, 94).
/// V4-2007. Describes a scan chain including master/slave clocks and cell list.
/// Uses S*n for CELL_LST entries (2-byte length prefix strings).
/// </summary>
[StdfRecord(1, 94)]
public partial record struct Cdr
{
    [BitField] public byte ContinuationFlag { get; set; }
    public ushort CdrIndex { get; set; }
    public string? ChainName { get; set; }
    public uint? ChainLength { get; set; }
    public ushort? ScanInPin { get; set; }
    public ushort? ScanOutPin { get; set; }

    [WireCount("mclk")] private byte MasterClockCount => throw new NotSupportedException();
    [CountedArray("mclk")] public ushort[]? MasterClocks { get; set; }

    [WireCount("sclk")] private byte SlaveClockCount => throw new NotSupportedException();
    [CountedArray("sclk")] public ushort[]? SlaveClocks { get; set; }

    public byte? InversionValue { get; set; }

    [WireCount("cell")] private ushort CellCount => throw new NotSupportedException();
    [CountedArray("cell"), Sn] public string[]? CellList { get; set; }
}
