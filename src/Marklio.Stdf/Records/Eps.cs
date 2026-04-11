using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// EPS — End Program Section Record (20, 20).
/// Marks the end of a program section. Has no fields.
/// </summary>
[StdfRecord(20, 20)]
public partial record struct Eps
{
}
