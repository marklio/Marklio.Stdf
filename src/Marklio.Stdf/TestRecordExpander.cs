using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Marklio.Stdf.Records;

namespace Marklio.Stdf;

/// <summary>
/// Extension methods for expanding test records by restoring static fields
/// from the first occurrence of each test number.
/// </summary>
[Experimental("STDF0001", UrlFormat = "https://github.com/marklio/Marklio.Stdf")]
public static class TestRecordExpander
{
    /// <summary>
    /// Restores static fields on PTR, FTR, and MPR records from the first occurrence
    /// of each test number. Fields that are already non-null are not overwritten.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the inverse of <see cref="TestRecordCompactor.CompactTestRecords(IEnumerable{StdfRecord})"/>.
    /// The first record for each test number (per record type) is stored as a template.
    /// Subsequent records for the same test number have their null static fields filled
    /// in from the stored template.
    /// </para>
    /// <para>
    /// Non-test records pass through unchanged.
    /// </para>
    /// </remarks>
    public static async IAsyncEnumerable<StdfRecord> ExpandTestRecords(
        this IAsyncEnumerable<StdfRecord> source,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var ptrTemplates = new Dictionary<uint, Ptr>();
        var ftrTemplates = new Dictionary<uint, Ftr>();
        var mprTemplates = new Dictionary<uint, Mpr>();

        await foreach (var rec in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            yield return ExpandRecord(rec, ptrTemplates, ftrTemplates, mprTemplates);
        }
    }

    /// <summary>
    /// Synchronous version of <see cref="ExpandTestRecords(IAsyncEnumerable{StdfRecord}, CancellationToken)"/>.
    /// </summary>
    /// <inheritdoc cref="ExpandTestRecords(IAsyncEnumerable{StdfRecord}, CancellationToken)" path="/remarks"/>
    public static IEnumerable<StdfRecord> ExpandTestRecords(this IEnumerable<StdfRecord> source)
    {
        var ptrTemplates = new Dictionary<uint, Ptr>();
        var ftrTemplates = new Dictionary<uint, Ftr>();
        var mprTemplates = new Dictionary<uint, Mpr>();

        foreach (var rec in source)
        {
            yield return ExpandRecord(rec, ptrTemplates, ftrTemplates, mprTemplates);
        }
    }

    private static StdfRecord ExpandRecord(
        StdfRecord rec,
        Dictionary<uint, Ptr> ptrTemplates,
        Dictionary<uint, Ftr> ftrTemplates,
        Dictionary<uint, Mpr> mprTemplates)
    {
        switch (rec)
        {
            case Ptr ptr:
                if (!ptrTemplates.TryGetValue(ptr.TestNumber, out var ptrTemplate))
                {
                    ptrTemplates[ptr.TestNumber] = ptr;
                    return ptr;
                }
                return ExpandPtr(ptr, ptrTemplate);

            case Ftr ftr:
                if (!ftrTemplates.TryGetValue(ftr.TestNumber, out var ftrTemplate))
                {
                    ftrTemplates[ftr.TestNumber] = ftr;
                    return ftr;
                }
                return ExpandFtr(ftr, ftrTemplate);

            case Mpr mpr:
                if (!mprTemplates.TryGetValue(mpr.TestNumber, out var mprTemplate))
                {
                    mprTemplates[mpr.TestNumber] = mpr;
                    return mpr;
                }
                return ExpandMpr(mpr, mprTemplate);

            default:
                return rec;
        }
    }

    private static Ptr ExpandPtr(Ptr ptr, Ptr template) => ptr with
    {
        TestText = ptr.TestText ?? template.TestText,
        AlarmId = ptr.AlarmId ?? template.AlarmId,
        OptionalFlags = ptr.OptionalFlags ?? template.OptionalFlags,
        ResultExponent = ptr.ResultExponent ?? template.ResultExponent,
        LowLimitExponent = ptr.LowLimitExponent ?? template.LowLimitExponent,
        HighLimitExponent = ptr.HighLimitExponent ?? template.HighLimitExponent,
        LowLimit = ptr.LowLimit ?? template.LowLimit,
        HighLimit = ptr.HighLimit ?? template.HighLimit,
        Units = ptr.Units ?? template.Units,
        ResultFormatString = ptr.ResultFormatString ?? template.ResultFormatString,
        LowLimitFormatString = ptr.LowLimitFormatString ?? template.LowLimitFormatString,
        HighLimitFormatString = ptr.HighLimitFormatString ?? template.HighLimitFormatString,
        LowSpecLimit = ptr.LowSpecLimit ?? template.LowSpecLimit,
        HighSpecLimit = ptr.HighSpecLimit ?? template.HighSpecLimit,
    };

    private static Ftr ExpandFtr(Ftr ftr, Ftr template) => ftr with
    {
        VectorName = ftr.VectorName ?? template.VectorName,
        TimeSet = ftr.TimeSet ?? template.TimeSet,
        OpCode = ftr.OpCode ?? template.OpCode,
        TestText = ftr.TestText ?? template.TestText,
        AlarmId = ftr.AlarmId ?? template.AlarmId,
        ProgramText = ftr.ProgramText ?? template.ProgramText,
        ResultText = ftr.ResultText ?? template.ResultText,
        PatternGeneratorNumber = ftr.PatternGeneratorNumber ?? template.PatternGeneratorNumber,
    };

    private static Mpr ExpandMpr(Mpr mpr, Mpr template) => mpr with
    {
        TestText = mpr.TestText ?? template.TestText,
        AlarmId = mpr.AlarmId ?? template.AlarmId,
        OptionalFlags = mpr.OptionalFlags ?? template.OptionalFlags,
        ResultExponent = mpr.ResultExponent ?? template.ResultExponent,
        LowLimitExponent = mpr.LowLimitExponent ?? template.LowLimitExponent,
        HighLimitExponent = mpr.HighLimitExponent ?? template.HighLimitExponent,
        LowLimit = mpr.LowLimit ?? template.LowLimit,
        HighLimit = mpr.HighLimit ?? template.HighLimit,
        StartingCondition = mpr.StartingCondition ?? template.StartingCondition,
        ConditionIncrement = mpr.ConditionIncrement ?? template.ConditionIncrement,
        Units = mpr.Units ?? template.Units,
        UnitsInput = mpr.UnitsInput ?? template.UnitsInput,
        ResultFormatString = mpr.ResultFormatString ?? template.ResultFormatString,
        LowLimitFormatString = mpr.LowLimitFormatString ?? template.LowLimitFormatString,
        HighLimitFormatString = mpr.HighLimitFormatString ?? template.HighLimitFormatString,
        LowSpecLimit = mpr.LowSpecLimit ?? template.LowSpecLimit,
        HighSpecLimit = mpr.HighSpecLimit ?? template.HighSpecLimit,
    };
}
