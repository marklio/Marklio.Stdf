namespace Marklio.Stdf;

/// <summary>
/// Specifies the byte order of multi-byte values in the STDF file.
/// Determined by the FAR record's CPU_TYPE field.
/// </summary>
public enum Endianness : byte
{
    /// <summary>Big-endian byte order (CPU_TYPE = 1).</summary>
    BigEndian = 1,

    /// <summary>Little-endian byte order (CPU_TYPE = 2).</summary>
    LittleEndian = 2,
}
