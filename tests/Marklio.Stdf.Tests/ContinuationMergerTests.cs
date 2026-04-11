using Marklio.Stdf;
using Marklio.Stdf.Records;

namespace Marklio.Stdf.Tests;

public class ContinuationMergerTests
{
    [Fact]
    public void SinglePsr_PassesThrough()
    {
        var psr = new Psr
        {
            ContinuationFlag = 0, // no continuation
            PsrIndex = 1,
            PsrName = "test",
            PatternBegin = [100UL, 200UL],
            PatternEnd = [150UL, 250UL],
        };
        var records = new[]
        {
            new StdfRecord(psr, 1, 90),
        };

        var result = records.AsEnumerable().MergeContinuations().ToList();
        Assert.Single(result);
        Assert.True(result[0].Is<Psr>(out var merged));
        Assert.Equal(1, merged.PsrIndex);
        Assert.Equal(new ulong[] { 100, 200 }, merged.PatternBegin);
    }

    [Fact]
    public void TwoPsrContinuations_MergesArrays()
    {
        var psr1 = new Psr
        {
            ContinuationFlag = 0x01, // continues
            PsrIndex = 5,
            PsrName = "scan1",
            TotalPatternCount = 4,
            PatternBegin = [10UL, 20UL],
            PatternEnd = [15UL, 25UL],
            PatternFiles = ["file1.pat", "file2.pat"],
        };
        var psr2 = new Psr
        {
            ContinuationFlag = 0, // final
            PsrIndex = 5,
            PatternBegin = [30UL, 40UL],
            PatternEnd = [35UL, 45UL],
            PatternFiles = ["file3.pat", "file4.pat"],
        };

        var records = new[]
        {
            new StdfRecord(psr1, 1, 90),
            new StdfRecord(psr2, 1, 90),
        };

        var result = records.AsEnumerable().MergeContinuations().ToList();
        Assert.Single(result);
        Assert.True(result[0].Is<Psr>(out var merged));
        Assert.Equal(5, merged.PsrIndex);
        Assert.Equal("scan1", merged.PsrName);
        Assert.Equal((ushort)4, merged.TotalPatternCount);
        Assert.Equal(0, merged.ContinuationFlag);
        Assert.Equal(new ulong[] { 10, 20, 30, 40 }, merged.PatternBegin);
        Assert.Equal(new ulong[] { 15, 25, 35, 45 }, merged.PatternEnd);
        Assert.Equal(new[] { "file1.pat", "file2.pat", "file3.pat", "file4.pat" }, merged.PatternFiles);
    }

    [Fact]
    public void ThreePsrContinuations_MergesAll()
    {
        var records = new[]
        {
            new StdfRecord(new Psr { ContinuationFlag = 0x01, PsrIndex = 1, PatternBegin = [1UL] }, 1, 90),
            new StdfRecord(new Psr { ContinuationFlag = 0x01, PsrIndex = 1, PatternBegin = [2UL] }, 1, 90),
            new StdfRecord(new Psr { ContinuationFlag = 0, PsrIndex = 1, PatternBegin = [3UL] }, 1, 90),
        };

        var result = records.AsEnumerable().MergeContinuations().ToList();
        Assert.Single(result);
        Assert.True(result[0].Is<Psr>(out var merged));
        Assert.Equal(new ulong[] { 1, 2, 3 }, merged.PatternBegin);
    }

    [Fact]
    public void NonContinuationRecords_PassThrough()
    {
        var records = new[]
        {
            new StdfRecord(new Far { CpuType = 2, StdfVersion = 4 }, 0, 10),
            new StdfRecord(new Pir { HeadNumber = 1, SiteNumber = 1 }, 5, 10),
            new StdfRecord(new Prr { HeadNumber = 1, SiteNumber = 1 }, 5, 20),
        };

        var result = records.AsEnumerable().MergeContinuations().ToList();
        Assert.Equal(3, result.Count);
        Assert.True(result[0].Is<Far>(out _));
        Assert.True(result[1].Is<Pir>(out _));
        Assert.True(result[2].Is<Prr>(out _));
    }

    [Fact]
    public void MixedRecordsWithContinuations_PreservesOrder()
    {
        var records = new[]
        {
            new StdfRecord(new Far { CpuType = 2, StdfVersion = 4 }, 0, 10),
            new StdfRecord(new Psr { ContinuationFlag = 0x01, PsrIndex = 1, PatternBegin = [1UL] }, 1, 90),
            new StdfRecord(new Psr { ContinuationFlag = 0, PsrIndex = 1, PatternBegin = [2UL] }, 1, 90),
            new StdfRecord(new Pir { HeadNumber = 1, SiteNumber = 1 }, 5, 10),
        };

        var result = records.AsEnumerable().MergeContinuations().ToList();
        Assert.Equal(3, result.Count);
        Assert.True(result[0].Is<Far>(out _));
        Assert.True(result[1].Is<Psr>(out var psr));
        Assert.Equal(new ulong[] { 1, 2 }, psr.PatternBegin);
        Assert.True(result[2].Is<Pir>(out _));
    }

    [Fact]
    public async Task AsyncVersion_Works()
    {
        var records = new[]
        {
            new StdfRecord(new Psr { ContinuationFlag = 0x01, PsrIndex = 1, PatternBegin = [1UL] }, 1, 90),
            new StdfRecord(new Psr { ContinuationFlag = 0, PsrIndex = 1, PatternBegin = [2UL] }, 1, 90),
        };

        var list = new List<StdfRecord>();
        await foreach (var rec in ToAsyncEnumerable(records).MergeContinuations())
            list.Add(rec);

        Assert.Single(list);
        Assert.True(list[0].Is<Psr>(out var merged));
        Assert.Equal(new ulong[] { 1, 2 }, merged.PatternBegin);
    }

    private static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(IEnumerable<T> source)
    {
        foreach (var item in source)
        {
            await Task.Yield();
            yield return item;
        }
    }
}
