using Marklio.Stdf;
using Marklio.Stdf.Records;

namespace Marklio.Stdf.Tests;

public class OrderingValidatorTests
{
    [Fact]
    public async Task ValidStream_ProducesNoErrors()
    {
        var records = ToAsync(
            new Far { CpuType = 2, StdfVersion = 4 },
            new Mir(),
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Ptr { HeadNumber = 1, SiteNumber = 1, TestNumber = 100 },
            new Prr { HeadNumber = 1, SiteNumber = 1 },
            new Mrr());

        var result = await Collect(records.ValidateOrdering());

        Assert.DoesNotContain(result, r => r is ErrorRecord);
        Assert.Equal(6, result.Count);
    }

    [Fact]
    public async Task MissingFar_ProducesError()
    {
        var records = ToAsync(new Mir());

        var result = await Collect(records.ValidateOrdering());

        var error = Assert.Single(result.OfType<ErrorRecord>());
        Assert.Equal("STDF_ORDER_NO_FAR", error.Code);
        Assert.Equal(ErrorSeverity.Error, error.Severity);
    }

    [Fact]
    public async Task MissingMir_ProducesError()
    {
        var records = ToAsync(
            new Far { CpuType = 2, StdfVersion = 4 },
            new Pir { HeadNumber = 1, SiteNumber = 1 });

        var result = await Collect(records.ValidateOrdering());

        var error = Assert.Single(result.OfType<ErrorRecord>());
        Assert.Equal("STDF_ORDER_MIR_EXPECTED", error.Code);
    }

    [Fact]
    public async Task PtrWithoutOpenPir_ProducesError()
    {
        var records = ToAsync(
            new Far { CpuType = 2, StdfVersion = 4 },
            new Mir(),
            new Ptr { HeadNumber = 1, SiteNumber = 1, TestNumber = 100 });

        var result = await Collect(records.ValidateOrdering());

        var error = Assert.Single(result.OfType<ErrorRecord>());
        Assert.Equal("STDF_ORDER_NO_PIR", error.Code);
    }

    [Fact]
    public async Task DuplicatePir_ProducesError()
    {
        var records = ToAsync(
            new Far { CpuType = 2, StdfVersion = 4 },
            new Mir(),
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Pir { HeadNumber = 1, SiteNumber = 1 });

        var result = await Collect(records.ValidateOrdering());

        var error = Assert.Single(result.OfType<ErrorRecord>());
        Assert.Equal("STDF_ORDER_DUPLICATE_PIR", error.Code);
    }

    [Fact]
    public async Task PrrWithoutMatchingPir_ProducesError()
    {
        var records = ToAsync(
            new Far { CpuType = 2, StdfVersion = 4 },
            new Mir(),
            new Prr { HeadNumber = 1, SiteNumber = 1 });

        var result = await Collect(records.ValidateOrdering());

        var error = Assert.Single(result.OfType<ErrorRecord>());
        Assert.Equal("STDF_ORDER_NO_MATCHING_PIR", error.Code);
    }

    [Fact]
    public async Task TestRecordAfterSummary_ProducesError()
    {
        var records = ToAsync(
            new Far { CpuType = 2, StdfVersion = 4 },
            new Mir(),
            new Pcr { HeadNumber = 255, SiteNumber = 255 },
            new Pir { HeadNumber = 1, SiteNumber = 1 });

        var result = await Collect(records.ValidateOrdering());

        var error = Assert.Single(result.OfType<ErrorRecord>());
        Assert.Equal("STDF_ORDER_TEST_AFTER_SUMMARY", error.Code);
    }

    [Fact]
    public async Task RecordAfterMrr_ProducesError()
    {
        var records = ToAsync(
            new Far { CpuType = 2, StdfVersion = 4 },
            new Mir(),
            new Mrr(),
            new Pcr { HeadNumber = 255, SiteNumber = 255 });

        var result = await Collect(records.ValidateOrdering());

        var error = Assert.Single(result.OfType<ErrorRecord>());
        Assert.Equal("STDF_ORDER_RECORD_AFTER_MRR", error.Code);
    }

    [Fact]
    public async Task AtrBetweenFarAndMir_IsAllowed()
    {
        var records = ToAsync(
            new Far { CpuType = 2, StdfVersion = 4 },
            new Atr { CommandLine = "audit" },
            new Mir(),
            new Mrr());

        var result = await Collect(records.ValidateOrdering());

        Assert.DoesNotContain(result, r => r is ErrorRecord);
        Assert.Equal(4, result.Count);
    }

    [Fact]
    public void SyncVersion_Works()
    {
        var records = new StdfRecord[]
        {
            new Far { CpuType = 2, StdfVersion = 4 },
            new Mir(),
            new Mrr(),
        };

        var result = records.AsEnumerable().ValidateOrdering().ToList();

        Assert.DoesNotContain(result, r => r is ErrorRecord);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task ErrorRecordBeforeOffendingRecord()
    {
        var records = ToAsync(
            new Far { CpuType = 2, StdfVersion = 4 },
            new Mir(),
            new Ptr { HeadNumber = 1, SiteNumber = 1, TestNumber = 1 });

        var result = await Collect(records.ValidateOrdering());

        // Error should appear before the PTR
        int errorIdx = result.FindIndex(r => r is ErrorRecord);
        int ptrIdx = result.FindIndex(r => r is Ptr);
        Assert.True(errorIdx < ptrIdx, "ErrorRecord should appear before the offending record.");
    }

    [Fact]
    public async Task UnpairedBps_ProducesError()
    {
        var records = ToAsync(
            new Far { CpuType = 2, StdfVersion = 4 },
            new Mir(),
            new Eps());

        var result = await Collect(records.ValidateOrdering());

        var error = Assert.Single(result.OfType<ErrorRecord>());
        Assert.Equal("STDF_ORDER_UNPAIRED_BPS", error.Code);
    }

    private static async IAsyncEnumerable<StdfRecord> ToAsync(params StdfRecord[] records)
    {
        foreach (var r in records) yield return r;
        await Task.CompletedTask;
    }

    private static async Task<List<StdfRecord>> Collect(IAsyncEnumerable<StdfRecord> source)
    {
        var list = new List<StdfRecord>();
        await foreach (var r in source)
            list.Add(r);
        return list;
    }
}
