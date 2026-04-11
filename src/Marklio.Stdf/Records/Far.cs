using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// FAR — File Attributes Record (0, 10).
/// Must be the first record in every STDF file.
/// Identifies the CPU type (endianness) and STDF version.
/// </summary>
[StdfRecord(0, 10)]
public partial record struct Far
{
    /// <summary>CPU type that wrote this file (1=big-endian, 2=little-endian).</summary>
    public byte CpuType { get; set; }

    /// <summary>STDF version number (4 for V4).</summary>
    public byte StdfVersion { get; set; }
}
