using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// DTR — Datalog Text Record (50, 30).
/// Contains free-form text for the data log.
/// </summary>
[StdfRecord(50, 30)]
public partial record class Dtr
{
    /// <summary>
    /// The datalog text. Always present (non-optional).
    /// [STDF: TEXT_DAT, C*n]
    /// </summary>
    public string TextData { get; set; }
}
