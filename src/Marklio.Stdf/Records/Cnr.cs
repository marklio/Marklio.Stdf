using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// CNR — Cell Name Record (1, 92).
/// V4-2007. Maps a chain bit position to a scan cell name.
/// </summary>
[StdfRecord(1, 92)]
public partial record struct Cnr
{
    /// <summary>
    /// Chain number (references CDR chain index).
    /// [STDF: CHN_NUM, U*2]
    /// </summary>
    public ushort ChainNumber { get; set; }

    /// <summary>
    /// Bit position within the chain.
    /// [STDF: BIT_POS, U*4]
    /// </summary>
    public uint BitPosition { get; set; }

    /// <summary>
    /// Scan cell name.
    /// [STDF: CELL_NAM, S*n]
    /// </summary>
    /// <remarks>
    /// Wire format uses S*n (2-byte length prefix) rather than C*n (1-byte prefix).
    /// </remarks>
    [Sn] public string? CellName { get; set; }
}
