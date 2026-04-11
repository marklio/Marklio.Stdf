namespace Marklio.Stdf;

/// <summary>Configuration options for <see cref="StdfFile"/> write operations.</summary>
public sealed class StdfWriterOptions
{
    /// <summary>Target endianness for the output file. Defaults to <see cref="Endianness.LittleEndian"/>.</summary>
    public Endianness Endianness { get; init; } = Endianness.LittleEndian;

    /// <summary>Compression to apply to the output. Defaults to <see cref="StdfCompression.None"/>.</summary>
    public StdfCompression Compression { get; init; } = StdfCompression.None;
}
