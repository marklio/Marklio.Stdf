using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// Version Update Record (VUR) — type 0, subtype 30.
/// Introduced in V4-2007. Lists the names of STDF specification extensions
/// or updates used by this file so readers can determine compatibility.
/// </summary>
[StdfRecord(0, 30)]
public partial record struct Vur
{
    [WireCount("upd")] private byte UpdateCount => throw new NotSupportedException();

    /// <summary>
    /// Names of STDF specification updates used by this file. [STDF: UPD_NAM, kxC*n]
    /// </summary>
    /// <remarks>Serialized as a counted array with a leading U*1 count field (UPD_CNT) via the [CountedArray] attribute.</remarks>
    [CountedArray("upd")] public string[]? UpdateNames { get; set; }
}
