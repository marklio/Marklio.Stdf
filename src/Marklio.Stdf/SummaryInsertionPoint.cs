using Marklio.Stdf.Records;

namespace Marklio.Stdf;

/// <summary>
/// Shared logic for finding the insertion point for generated summary records.
/// Summary records are inserted before the first WRR, or before MRR, or at end.
/// </summary>
internal static class SummaryInsertionPoint
{
    /// <summary>
    /// Returns the index at which new summary records should be inserted.
    /// </summary>
    internal static int Find(List<StdfRecord> records)
    {
        for (int i = 0; i < records.Count; i++)
        {
            if (records[i] is Wrr)
                return i;
        }

        for (int i = 0; i < records.Count; i++)
        {
            if (records[i] is Mrr)
                return i;
        }

        return records.Count;
    }
}
