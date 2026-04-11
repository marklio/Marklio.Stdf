using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// VUR — Version Update Record (0, 30).
/// V4-2007. Identifies the updates/extensions to the STDF specification used in this file.
/// </summary>
[StdfRecord(0, 30)]
public partial record struct Vur
{
    [WireCount("upd")] private byte UpdateCount => throw new NotSupportedException();
    [CountedArray("upd")] public string[]? UpdateNames { get; set; }
}
