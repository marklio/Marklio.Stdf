using Marklio.Stdf;
using Marklio.Stdf.Records;

namespace Marklio.Stdf.Tests;

public class StructureValidatorTests
{
    [Fact]
    public async Task ValidStream_ProducesNoErrors()
    {
        var records = ToAsync(
            new Far { CpuType = 2, StdfVersion = 4 },
            new Mir(),
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Prr { HeadNumber = 1, SiteNumber = 1 },
            new Mrr());

        var result = await Collect(records.ValidateStructure());

        Assert.DoesNotContain(result, r => r is ErrorRecord);
    }

    [Fact]
    public async Task UnclosedPir_ProducesErrorAtEnd()
    {
        var records = ToAsync(
            new Far { CpuType = 2, StdfVersion = 4 },
            new Mir(),
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Mrr());

        var result = await Collect(records.ValidateStructure());

        var error = Assert.Single(result.OfType<ErrorRecord>());
        Assert.Equal("STDF_STRUCT_UNCLOSED_PIR", error.Code);
        // Should be after the last original record
        int lastOriginal = result.FindLastIndex(r => r is not ErrorRecord);
        int errorIdx = result.IndexOf(error);
        Assert.True(errorIdx > lastOriginal);
    }

    [Fact]
    public async Task MissingFar_ProducesErrorAtEnd()
    {
        var records = ToAsync(
            new Mir(),
            new Mrr());

        var result = await Collect(records.ValidateStructure());

        Assert.Contains(result.OfType<ErrorRecord>(), e => e.Code == "STDF_STRUCT_NO_FAR");
    }

    [Fact]
    public async Task MissingMir_ProducesErrorAtEnd()
    {
        var records = ToAsync(
            new Far { CpuType = 2, StdfVersion = 4 },
            new Mrr());

        var result = await Collect(records.ValidateStructure());

        Assert.Contains(result.OfType<ErrorRecord>(), e => e.Code == "STDF_STRUCT_NO_MIR");
    }

    [Fact]
    public async Task MissingMrr_ProducesErrorAtEnd()
    {
        var records = ToAsync(
            new Far { CpuType = 2, StdfVersion = 4 },
            new Mir());

        var result = await Collect(records.ValidateStructure());

        Assert.Contains(result.OfType<ErrorRecord>(), e => e.Code == "STDF_STRUCT_NO_MRR");
    }

    [Fact]
    public async Task PrrWithoutPir_ProducesError()
    {
        var records = ToAsync(
            new Far { CpuType = 2, StdfVersion = 4 },
            new Mir(),
            new Prr { HeadNumber = 1, SiteNumber = 1 },
            new Mrr());

        var result = await Collect(records.ValidateStructure());

        var error = Assert.Single(result.OfType<ErrorRecord>());
        Assert.Equal("STDF_STRUCT_NO_MATCHING_PIR", error.Code);
    }

    [Fact]
    public async Task DuplicatePir_ProducesError()
    {
        var records = ToAsync(
            new Far { CpuType = 2, StdfVersion = 4 },
            new Mir(),
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Mrr());

        var result = await Collect(records.ValidateStructure());

        // One duplicate PIR error + one unclosed PIR error (two open for same key, but HashSet dedupes
        // so only one unclosed PIR since the duplicate was rejected by the set)
        var errors = result.OfType<ErrorRecord>().ToList();
        Assert.Contains(errors, e => e.Code == "STDF_STRUCT_DUPLICATE_PIR");
        Assert.Contains(errors, e => e.Code == "STDF_STRUCT_UNCLOSED_PIR");
    }

    [Fact]
    public void SyncVersion_Works()
    {
        var records = new StdfRecord[]
        {
            new Far { CpuType = 2, StdfVersion = 4 },
            new Mir(),
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Prr { HeadNumber = 1, SiteNumber = 1 },
            new Mrr(),
        };

        var result = records.AsEnumerable().ValidateStructure().ToList();

        Assert.DoesNotContain(result, r => r is ErrorRecord);
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
