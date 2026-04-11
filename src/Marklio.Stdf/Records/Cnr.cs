using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// CNR — Cell Name Record (1, 92).
/// V4-2007. Provides a mapping from chain number and bit position to a cell name.
/// CELL_NAM uses S*n encoding (2-byte length prefix).
/// </summary>
[StdfRecord(1, 92)]
public partial record struct Cnr
{
    public ushort ChainNumber { get; set; }
    public uint BitPosition { get; set; }
    [Sn] public string? CellName { get; set; }
}
