using Marklio.Stdf;
using Marklio.Stdf.Records;

namespace Marklio.Stdf.Tests;

public class UnknownRecordValidatorTests
{
    [Fact]
    public async Task UnknownRecord_ProducesErrorRecord()
    {
        var unknown = new UnknownRecord(99, 1) { RawData = ReadOnlyMemory<byte>.Empty };
        var records = ToAsync(unknown);

        var result = await Collect(records.RejectUnknownRecords());

        Assert.Equal(2, result.Count);
        var error = Assert.IsType<ErrorRecord>(result[0]);
        Assert.Equal("STDF_UNKNOWN_RECORD", error.Code);
        Assert.Equal(ErrorSeverity.Error, error.Severity);
        Assert.Contains("99", error.Message);
        Assert.Contains("1", error.Message);
        Assert.Same(unknown, error.SourceRecord);
        Assert.IsType<UnknownRecord>(result[1]);
    }

    [Fact]
    public async Task KnownRecords_PassThroughWithoutErrors()
    {
        var records = ToAsync(
            new Far { CpuType = 2, StdfVersion = 4 },
            new Mir(),
            new Mrr());

        var result = await Collect(records.RejectUnknownRecords());

        Assert.DoesNotContain(result, r => r is ErrorRecord);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task StreamWithNoUnknowns_HasNoErrorRecords()
    {
        var records = ToAsync(
            new Far { CpuType = 2, StdfVersion = 4 },
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Prr { HeadNumber = 1, SiteNumber = 1 });

        var result = await Collect(records.RejectUnknownRecords());

        Assert.Empty(result.OfType<ErrorRecord>());
    }

    [Fact]
    public void SyncVersion_Works()
    {
        var unknown = new UnknownRecord(200, 50) { RawData = ReadOnlyMemory<byte>.Empty };
        var records = new StdfRecord[] { unknown };

        var result = records.AsEnumerable().RejectUnknownRecords().ToList();

        Assert.Equal(2, result.Count);
        Assert.IsType<ErrorRecord>(result[0]);
        Assert.IsType<UnknownRecord>(result[1]);
    }

    [Fact]
    public async Task ErrorMessage_IncludesTypeAndSubtype()
    {
        var unknown = new UnknownRecord(42, 7) { RawData = ReadOnlyMemory<byte>.Empty };
        var records = ToAsync(unknown);

        var result = await Collect(records.RejectUnknownRecords());

        var error = result.OfType<ErrorRecord>().Single();
        Assert.Contains("42", error.Message);
        Assert.Contains("7", error.Message);
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
