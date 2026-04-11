namespace Marklio.Stdf;

/// <summary>Compression format for STDF file writing.</summary>
public enum StdfCompression
{
    /// <summary>No compression.</summary>
    None = 0,

    /// <summary>Gzip compression.</summary>
    Gzip = 1,

    /// <summary>Bzip2 compression.</summary>
    Bzip2 = 2,
}
