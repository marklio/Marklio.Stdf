using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// DTR — Datalog Text Record (50, 30).
/// Contains a single line of text for the datalog.
/// </summary>
[StdfRecord(50, 30)]
public partial record struct Dtr
{
    /// <summary>Text data.</summary>
    public string TextData { get; set; }
}
