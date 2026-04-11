using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// EPS — End Program Section Record (20, 20).
/// Marks the end of a program section. Contains no data fields.
/// </summary>
[StdfRecord(20, 20)]
public partial record struct Eps
{
}
