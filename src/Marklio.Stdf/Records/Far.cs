using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// File Attributes Record (FAR) — type 0, subtype 10.
/// Must be the first record in every STDF file. Identifies the CPU type
/// (byte ordering) and STDF specification version used by the file.
/// </summary>
[StdfRecord(0, 10)]
public partial record class Far
{
    /// <summary>
    /// CPU type that wrote the file (1 = big-endian, e.g. SPARC; 2 = little-endian, e.g. x86).
    /// Used to determine byte ordering for all subsequent records. [STDF: CPU_TYP, U*1]
    /// </summary>
    public byte CpuType { get; set; }

    /// <summary>
    /// STDF specification version number (always 4 for V4 files). [STDF: STDF_VER, U*1]
    /// </summary>
    public byte StdfVersion { get; set; }
}
