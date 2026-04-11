using System.Collections;
using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// FTR — Functional Test Record (15, 20).
/// Contains the results of a single functional test execution.
/// </summary>
[StdfRecord(15, 20)]
public partial record struct Ftr : ITestRecord
{
    public uint TestNumber { get; set; }
    public byte HeadNumber { get; set; }
    public byte SiteNumber { get; set; }
    [BitField] public byte? TestFlags { get; set; }
    [BitField] public byte? OptionalFlags { get; set; }
    public uint? CycleCount { get; set; }
    public uint? RelativeVectorAddress { get; set; }
    public uint? RepeatCount { get; set; }
    public uint? FailCount { get; set; }
    public int? XFailAddress { get; set; }
    public int? YFailAddress { get; set; }
    public short? VectorOffset { get; set; }

    [WireCount("rtn")] private ushort ReturnCount => throw new NotSupportedException();
    [WireCount("pgm")] private ushort ProgramCount => throw new NotSupportedException();

    [CountedArray("rtn")] public ushort[]? ReturnIndexes { get; set; }
    [CountedArray("rtn"), Nibble] public byte[]? ReturnStates { get; set; }
    [CountedArray("pgm")] public ushort[]? ProgramIndexes { get; set; }
    [CountedArray("pgm"), Nibble] public byte[]? ProgramStates { get; set; }

    [BitArray] public BitArray? FailingPins { get; set; }
    public string? VectorName { get; set; }
    public string? TimeSet { get; set; }
    public string? OpCode { get; set; }
    public string? TestText { get; set; }
    public string? AlarmId { get; set; }
    public string? ProgramText { get; set; }
    public string? ResultText { get; set; }
    public byte? PatternGeneratorNumber { get; set; }
    [BitArray] public BitArray? SpinMap { get; set; }
}
