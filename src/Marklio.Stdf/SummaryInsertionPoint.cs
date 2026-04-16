using Marklio.Stdf.Records;

namespace Marklio.Stdf;

/// <summary>
/// Shared logic for finding the insertion point for generated summary records.
/// Summary records are inserted before the earliest of the first WRR or first MRR, or at end.
/// </summary>
internal static class SummaryInsertionPoint
{
    /// <summary>
    /// Returns the index at which new summary records should be inserted.
    /// Uses the earliest of the first WRR or first MRR in the stream.
    /// </summary>
    internal static int Find(List<StdfRecord> records)
    {
        int firstWrr = -1;
        int firstMrr = -1;

        for (int i = 0; i < records.Count; i++)
        {
            if (firstWrr == -1 && records[i] is Wrr)
                firstWrr = i;
            else if (firstMrr == -1 && records[i] is Mrr)
                firstMrr = i;

            if (firstWrr >= 0 && firstMrr >= 0)
                break;
        }

        return (firstWrr, firstMrr) switch
        {
            ( >= 0, >= 0) => Math.Min(firstWrr, firstMrr),
            ( >= 0, _) => firstWrr,
            (_, >= 0) => firstMrr,
            _ => records.Count,
        };
    }
}
