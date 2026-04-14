using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// CDR — Chain Description Record (1, 94).
/// V4-2007. Describes a single scan chain. Supports continuation.
/// Has three independent count groups: "mclk" (master clocks), "sclk" (slave clocks), and "cell" (cell list).
/// </summary>
[StdfRecord(1, 94)]
public partial record class Cdr
{
    /// <summary>
    /// Continuation flag. Bit 0: if set, this record continues in the next CDR record.
    /// [STDF: CONT_FLG, B*1]
    /// </summary>
    [BitField] public byte ContinuationFlag { get; set; }

    /// <summary>
    /// Unique chain index within the file.
    /// [STDF: CDR_INDX, U*2]
    /// </summary>
    public ushort CdrIndex { get; set; }

    /// <summary>
    /// Name of this scan chain. Optional.
    /// [STDF: CHN_NAM, C*n]
    /// </summary>
    public string? ChainName { get; set; }

    /// <summary>
    /// Total number of bits in the chain. Optional.
    /// [STDF: CHN_LEN, U*4]
    /// </summary>
    public uint? ChainLength { get; set; }

    /// <summary>
    /// PMR index for the scan-in pin. Optional.
    /// [STDF: SIN_PIN, U*2]
    /// </summary>
    public ushort? ScanInPin { get; set; }

    /// <summary>
    /// PMR index for the scan-out pin. Optional.
    /// [STDF: SOUT_PIN, U*2]
    /// </summary>
    public ushort? ScanOutPin { get; set; }

    [WireCount("mclk")] private byte MasterClockCount => throw new NotSupportedException();

    /// <summary>
    /// PMR indexes for master clock pins. Independent count group "mclk".
    /// [STDF: MSTR_CLK, kxU*2]
    /// </summary>
    [CountedArray("mclk")] public ushort[]? MasterClocks { get; set; }

    [WireCount("sclk")] private byte SlaveClockCount => throw new NotSupportedException();

    /// <summary>
    /// PMR indexes for slave clock pins. Independent count group "sclk".
    /// [STDF: SLAV_CLK, kxU*2]
    /// </summary>
    [CountedArray("sclk")] public ushort[]? SlaveClocks { get; set; }

    /// <summary>
    /// Inversion value (0 or 1). Optional.
    /// [STDF: INV_VAL, U*1]
    /// </summary>
    public byte? InversionValue { get; set; }

    [WireCount("cell")] private ushort CellCount => throw new NotSupportedException();

    /// <summary>
    /// Names of scan cells in the chain. Independent count group "cell".
    /// [STDF: CELL_LST, kxS*n]
    /// </summary>
    /// <remarks>
    /// Wire format uses S*n (2-byte length prefix) rather than C*n (1-byte prefix).
    /// </remarks>
    [CountedArray("cell"), Sn] public string[]? CellList { get; set; }
}
