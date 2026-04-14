using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// PRR — Part Results Record (5, 20).
/// Contains the results of testing a single part. Paired with a PIR.
/// </summary>
[StdfRecord(5, 20)]
public partial record class Prr : IHeadSiteRecord
{
    /// <summary>
    /// Test head number. [STDF: HEAD_NUM, U*1]
    /// </summary>
    public byte HeadNumber { get; set; }

    /// <summary>
    /// Test site number. [STDF: SITE_NUM, U*1]
    /// </summary>
    public byte SiteNumber { get; set; }

    /// <summary>
    /// Part result flags. Bit-encoded: bit 0=new part ID, bit 1=part tested at multiple test heads,
    /// bit 2=abnormal end of testing, bit 3=failed part, bit 4=no pass/fail indication. [STDF: PART_FLG, B*1]
    /// </summary>
    /// <remarks>Wire format is a single bit-field byte, mapped via the [BitField] attribute.</remarks>
    [BitField] public byte PartFlag { get; set; }

    /// <summary>
    /// Number of tests executed on this part. [STDF: NUM_TEST, U*2]
    /// </summary>
    public ushort NumTestsExecuted { get; set; }

    /// <summary>
    /// Hardware bin number the part was sorted into. [STDF: HARD_BIN, U*2]
    /// </summary>
    public ushort HardwareBin { get; set; }

    /// <summary>
    /// Software bin number. [STDF: SOFT_BIN, U*2]
    /// </summary>
    public ushort? SoftwareBin { get; set; }

    /// <summary>
    /// X coordinate of the part on the wafer. [STDF: X_COORD, I*2]
    /// </summary>
    public short? XCoordinate { get; set; }

    /// <summary>
    /// Y coordinate of the part on the wafer. [STDF: Y_COORD, I*2]
    /// </summary>
    public short? YCoordinate { get; set; }

    /// <summary>
    /// Elapsed test time in milliseconds. [STDF: TEST_T, U*4]
    /// </summary>
    public uint? TestTime { get; set; }

    /// <summary>
    /// Part identification string. [STDF: PART_ID, C*n]
    /// </summary>
    public string? PartId { get; set; }

    /// <summary>
    /// Descriptive text about the part. [STDF: PART_TXT, C*n]
    /// </summary>
    public string? PartText { get; set; }

    /// <summary>
    /// Bit-encoded part repair/fix information. [STDF: PART_FIX, B*n]
    /// </summary>
    /// <remarks>Wire format is a length-prefixed byte array, mapped via the [BitEncoded] attribute.</remarks>
    [BitEncoded] public byte[]? PartFix { get; set; }

    /// <summary>Gets <see cref="PartFlag"/> as a typed enum. See <see cref="PartFlag"/> for the raw value.</summary>
    public PartResultFlags PartFlagEnum => (PartResultFlags)PartFlag;
}
