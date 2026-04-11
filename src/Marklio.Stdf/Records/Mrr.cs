using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// MRR — Master Results Record (1, 20).
/// Contains global test results and closing information.
/// </summary>
[StdfRecord(1, 20)]
public partial record struct Mrr
{
    [StdfDateTime] public DateTime FinishTime { get; set; }
    [C1] public char? DispositionCode { get; set; }
    public string? UserDescription { get; set; }
    public string? ExecDescription { get; set; }
}
