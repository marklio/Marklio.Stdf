using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Marklio.Stdf.Records;

namespace Marklio.Stdf;

/// <summary>
/// Extension methods for compacting test records by removing redundant static fields
/// after the first occurrence of each test number.
/// </summary>
[Experimental("STDF0001", UrlFormat = "https://github.com/marklio/Marklio.Stdf")]
public static class TestRecordCompactor
{
    /// <summary>
    /// Removes redundant static fields from PTR, FTR, and MPR records after the first
    /// occurrence for a given test number. Per the STDF spec, static fields only need to
    /// appear once per test number; subsequent records can omit them.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is useful for reducing file size when writing STDF data. The first record for
    /// each test number passes through unchanged; subsequent records for the same test number
    /// have their static fields (test text, limits, units, etc.) set to <c>null</c>.
    /// </para>
    /// <para>
    /// Non-test records pass through unchanged. Use
    /// <see cref="TestRecordExpander.ExpandTestRecords(IEnumerable{StdfRecord})"/>
    /// to restore the removed fields.
    /// </para>
    /// </remarks>
    /// <param name="source">The STDF record stream to process.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable of <see cref="StdfRecord"/> with redundant static fields removed from subsequent test records.</returns>
    public static async IAsyncEnumerable<StdfRecord> CompactTestRecords(
        this IAsyncEnumerable<StdfRecord> source,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var seenPtrs = new HashSet<uint>();
        var seenFtrs = new HashSet<uint>();
        var seenMprs = new HashSet<uint>();

        await foreach (var rec in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            yield return CompactRecord(rec, seenPtrs, seenFtrs, seenMprs);
        }
    }

    /// <summary>
    /// Synchronous version of <see cref="CompactTestRecords(IAsyncEnumerable{StdfRecord}, CancellationToken)"/>.
    /// </summary>
    /// <inheritdoc cref="CompactTestRecords(IAsyncEnumerable{StdfRecord}, CancellationToken)" path="/remarks"/>
    /// <param name="source">The STDF record stream to process.</param>
    /// <returns>An enumerable of <see cref="StdfRecord"/> with redundant static fields removed from subsequent test records.</returns>
    public static IEnumerable<StdfRecord> CompactTestRecords(this IEnumerable<StdfRecord> source)
    {
        var seenPtrs = new HashSet<uint>();
        var seenFtrs = new HashSet<uint>();
        var seenMprs = new HashSet<uint>();

        foreach (var rec in source)
        {
            yield return CompactRecord(rec, seenPtrs, seenFtrs, seenMprs);
        }
    }

    private static StdfRecord CompactRecord(
        StdfRecord rec,
        HashSet<uint> seenPtrs,
        HashSet<uint> seenFtrs,
        HashSet<uint> seenMprs)
    {
        return rec switch
        {
            Ptr ptr => seenPtrs.Add(ptr.TestNumber) ? ptr : CompactPtr(ptr),
            Ftr ftr => seenFtrs.Add(ftr.TestNumber) ? ftr : CompactFtr(ftr),
            Mpr mpr => seenMprs.Add(mpr.TestNumber) ? mpr : CompactMpr(mpr),
            _ => rec,
        };
    }

    private static Ptr CompactPtr(Ptr ptr) => ptr with
    {
        TestText = null,
        AlarmId = null,
        OptionalFlags = null,
        ResultExponent = null,
        LowLimitExponent = null,
        HighLimitExponent = null,
        LowLimit = null,
        HighLimit = null,
        Units = null,
        ResultFormatString = null,
        LowLimitFormatString = null,
        HighLimitFormatString = null,
        LowSpecLimit = null,
        HighSpecLimit = null,
    };

    private static Ftr CompactFtr(Ftr ftr) => ftr with
    {
        VectorName = null,
        TimeSet = null,
        OpCode = null,
        TestText = null,
        AlarmId = null,
        ProgramText = null,
        ResultText = null,
        PatternGeneratorNumber = null,
    };

    private static Mpr CompactMpr(Mpr mpr) => mpr with
    {
        TestText = null,
        AlarmId = null,
        OptionalFlags = null,
        ResultExponent = null,
        LowLimitExponent = null,
        HighLimitExponent = null,
        LowLimit = null,
        HighLimit = null,
        StartingCondition = null,
        ConditionIncrement = null,
        Units = null,
        UnitsInput = null,
        ResultFormatString = null,
        LowLimitFormatString = null,
        HighLimitFormatString = null,
        LowSpecLimit = null,
        HighSpecLimit = null,
    };
}
