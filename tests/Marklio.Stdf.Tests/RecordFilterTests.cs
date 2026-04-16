using Marklio.Stdf;
using Marklio.Stdf.Records;

namespace Marklio.Stdf.Tests;

public class RecordFilterTests
{
    [Fact]
    public void NonHeadSiteRecords_AlwaysPassThrough()
    {
        var records = new StdfRecord[]
        {
            new Far { CpuType = 2, StdfVersion = 4 },
            new Mir(),
            new Mrr(),
        };

        var result = records.AsEnumerable().FilterByHeadSite(1, 1).ToList();
        Assert.Equal(3, result.Count);
        Assert.IsType<Far>(result[0]);
        Assert.IsType<Mir>(result[1]);
        Assert.IsType<Mrr>(result[2]);
    }

    [Fact]
    public void Ptr_MatchingHeadAndSite_PassesThrough()
    {
        var records = new StdfRecord[]
        {
            new Ptr { TestNumber = 1, HeadNumber = 1, SiteNumber = 1, Result = 1.0f },
        };

        var result = records.AsEnumerable().FilterByHeadSite(1, 1).ToList();
        Assert.Single(result);
        Assert.IsType<Ptr>(result[0]);
    }

    [Fact]
    public void Ptr_NonMatchingHead_FilteredOut()
    {
        var records = new StdfRecord[]
        {
            new Ptr { TestNumber = 1, HeadNumber = 2, SiteNumber = 1, Result = 1.0f },
        };

        var result = records.AsEnumerable().FilterByHeadSite(1, 1).ToList();
        Assert.Empty(result);
    }

    [Fact]
    public void Ptr_NonMatchingSite_FilteredOut()
    {
        var records = new StdfRecord[]
        {
            new Ptr { TestNumber = 1, HeadNumber = 1, SiteNumber = 2, Result = 1.0f },
        };

        var result = records.AsEnumerable().FilterByHeadSite(1, 1).ToList();
        Assert.Empty(result);
    }

    [Fact]
    public void NullHead_FiltersOnlySite()
    {
        var records = new StdfRecord[]
        {
            new Ptr { TestNumber = 1, HeadNumber = 1, SiteNumber = 1, Result = 1.0f },
            new Ptr { TestNumber = 2, HeadNumber = 2, SiteNumber = 1, Result = 2.0f },
            new Ptr { TestNumber = 3, HeadNumber = 1, SiteNumber = 2, Result = 3.0f },
        };

        var result = records.AsEnumerable().FilterByHeadSite(null, 1).ToList();
        Assert.Equal(2, result.Count);
        Assert.Equal(1u, ((Ptr)result[0]).TestNumber);
        Assert.Equal(2u, ((Ptr)result[1]).TestNumber);
    }

    [Fact]
    public void NullSite_FiltersOnlyHead()
    {
        var records = new StdfRecord[]
        {
            new Ptr { TestNumber = 1, HeadNumber = 1, SiteNumber = 1, Result = 1.0f },
            new Ptr { TestNumber = 2, HeadNumber = 1, SiteNumber = 2, Result = 2.0f },
            new Ptr { TestNumber = 3, HeadNumber = 2, SiteNumber = 1, Result = 3.0f },
        };

        var result = records.AsEnumerable().FilterByHeadSite(1, null).ToList();
        Assert.Equal(2, result.Count);
        Assert.Equal(1u, ((Ptr)result[0]).TestNumber);
        Assert.Equal(2u, ((Ptr)result[1]).TestNumber);
    }

    [Fact]
    public void BothNull_PassesEverything()
    {
        var records = new StdfRecord[]
        {
            new Far { CpuType = 2, StdfVersion = 4 },
            new Ptr { TestNumber = 1, HeadNumber = 1, SiteNumber = 1, Result = 1.0f },
            new Ptr { TestNumber = 2, HeadNumber = 2, SiteNumber = 3, Result = 2.0f },
            new Mrr(),
        };

        var result = records.AsEnumerable().FilterByHeadSite(null, null).ToList();
        Assert.Equal(4, result.Count);
    }

    [Fact]
    public void SummaryHead255_AlwaysPassesThroughWhenFilteringByHead()
    {
        var records = new StdfRecord[]
        {
            new Pcr { HeadNumber = 255, SiteNumber = 255, PartCount = 100 },
            new Pcr { HeadNumber = 1, SiteNumber = 1, PartCount = 50 },
            new Pcr { HeadNumber = 2, SiteNumber = 2, PartCount = 50 },
        };

        var result = records.AsEnumerable().FilterByHeadSite(1, null).ToList();
        Assert.Equal(2, result.Count);
        Assert.Equal((byte)255, ((Pcr)result[0]).HeadNumber);
        Assert.Equal((byte)1, ((Pcr)result[1]).HeadNumber);
    }

    [Fact]
    public void SummarySite255_AlwaysPassesThroughWhenFilteringBySite()
    {
        var records = new StdfRecord[]
        {
            new Hbr { HeadNumber = 1, SiteNumber = 255, HardwareBin = 1, BinCount = 100 },
            new Hbr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 1, BinCount = 50 },
            new Hbr { HeadNumber = 1, SiteNumber = 2, HardwareBin = 1, BinCount = 50 },
        };

        var result = records.AsEnumerable().FilterByHeadSite(1, 1).ToList();
        Assert.Equal(2, result.Count);
        Assert.Equal((byte)255, ((Hbr)result[0]).SiteNumber);
        Assert.Equal((byte)1, ((Hbr)result[1]).SiteNumber);
    }

    [Fact]
    public void HeadOnlyRecord_FilteredByHeadOnly()
    {
        var records = new StdfRecord[]
        {
            new Wir { HeadNumber = 1 },
            new Wir { HeadNumber = 2 },
        };

        // Site filter is irrelevant for IHeadRecord-only types
        var result = records.AsEnumerable().FilterByHeadSite(1, 5).ToList();
        Assert.Single(result);
        Assert.Equal((byte)1, ((Wir)result[0]).HeadNumber);
    }

    [Fact]
    public async Task AsyncVersion_MatchesSync()
    {
        var records = new StdfRecord[]
        {
            new Far { CpuType = 2, StdfVersion = 4 },
            new Ptr { TestNumber = 1, HeadNumber = 1, SiteNumber = 1, Result = 1.0f },
            new Ptr { TestNumber = 2, HeadNumber = 2, SiteNumber = 1, Result = 2.0f },
            new Mrr(),
        };

        var syncResult = records.AsEnumerable().FilterByHeadSite(1, 1).ToList();

        var asyncResult = new List<StdfRecord>();
        await foreach (var rec in ToAsync(records).FilterByHeadSite(1, 1))
            asyncResult.Add(rec);

        Assert.Equal(syncResult.Count, asyncResult.Count);
        for (int i = 0; i < syncResult.Count; i++)
            Assert.Equal(syncResult[i].GetType(), asyncResult[i].GetType());
    }

    [Fact]
    public void ErrorAndUnknownRecords_AlwaysPassThrough()
    {
        var records = new StdfRecord[]
        {
            new ErrorRecord { Severity = ErrorSeverity.Warning, Code = "TEST", Message = "test" },
            new UnknownRecord(99, 99) { RawData = ReadOnlyMemory<byte>.Empty },
        };

        var result = records.AsEnumerable().FilterByHeadSite(1, 1).ToList();
        Assert.Equal(2, result.Count);
        Assert.IsType<ErrorRecord>(result[0]);
        Assert.IsType<UnknownRecord>(result[1]);
    }

    private static async IAsyncEnumerable<StdfRecord> ToAsync(params StdfRecord[] records)
    {
        foreach (var r in records) yield return r;
        await Task.CompletedTask;
    }
}
