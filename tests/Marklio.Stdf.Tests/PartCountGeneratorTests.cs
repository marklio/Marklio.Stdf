using Marklio.Stdf;
using Marklio.Stdf.Records;

namespace Marklio.Stdf.Tests;

#pragma warning disable STDF0001

public class PartCountGeneratorTests
{
    [Fact]
    public void NoTestData_NoSummariesGenerated()
    {
        var records = new StdfRecord[]
        {
            new Far { CpuType = 2, StdfVersion = 4 },
            new Mir(),
            new Mrr(),
        };

        var result = records.AsEnumerable().GeneratePartCounts().ToList();
        Assert.Equal(3, result.Count);
        Assert.DoesNotContain(result, r => r is Pcr);
    }

    [Fact]
    public void SingleHeadSite_GeneratesPcr()
    {
        var records = new StdfRecord[]
        {
            new Far { CpuType = 2, StdfVersion = 4 },
            new Mir(),
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Prr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 1, PartFlag = 0 },
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Prr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 1, PartFlag = 0 },
            new Wrr { HeadNumber = 1 },
            new Mrr(),
        };

        var result = records.AsEnumerable().GeneratePartCounts(SummaryScope.HeadSite).ToList();
        var pcrs = result.OfType<Pcr>().ToList();
        Assert.Single(pcrs);
        Assert.Equal((byte)1, pcrs[0].HeadNumber);
        Assert.Equal((byte)1, pcrs[0].SiteNumber);
        Assert.Equal(2u, pcrs[0].PartCount);
        Assert.Equal(2u, pcrs[0].GoodCount);
        Assert.Equal(0u, pcrs[0].AbortCount);
    }

    [Fact]
    public void InsertedBeforeWrr()
    {
        var records = new StdfRecord[]
        {
            new Far { CpuType = 2, StdfVersion = 4 },
            new Mir(),
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Prr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 1, PartFlag = 0 },
            new Wrr { HeadNumber = 1 },
            new Mrr(),
        };

        var result = records.AsEnumerable().GeneratePartCounts(SummaryScope.HeadSite).ToList();
        int pcrIndex = result.FindIndex(r => r is Pcr);
        int wrrIndex = result.FindIndex(r => r is Wrr);
        Assert.True(pcrIndex < wrrIndex, "PCR should appear before WRR");
    }

    [Fact]
    public void InsertedBeforeMrr_WhenNoWrr()
    {
        var records = new StdfRecord[]
        {
            new Far { CpuType = 2, StdfVersion = 4 },
            new Mir(),
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Prr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 1, PartFlag = 0 },
            new Mrr(),
        };

        var result = records.AsEnumerable().GeneratePartCounts(SummaryScope.HeadSite).ToList();
        int pcrIndex = result.FindIndex(r => r is Pcr);
        int mrrIndex = result.FindIndex(r => r is Mrr);
        Assert.True(pcrIndex < mrrIndex, "PCR should appear before MRR");
    }

    [Fact]
    public void InsertedAtEnd_WhenNoWrrOrMrr()
    {
        var records = new StdfRecord[]
        {
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Prr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 1, PartFlag = 0 },
        };

        var result = records.AsEnumerable().GeneratePartCounts(SummaryScope.HeadSite).ToList();
        Assert.IsType<Pcr>(result[^1]);
    }

    [Fact]
    public void ExistingPcr_NotDuplicated()
    {
        var records = new StdfRecord[]
        {
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Prr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 1, PartFlag = 0 },
            new Pcr { HeadNumber = 1, SiteNumber = 1, PartCount = 1 },
        };

        var result = records.AsEnumerable().GeneratePartCounts(SummaryScope.HeadSite).ToList();
        var pcrs = result.OfType<Pcr>().ToList();
        Assert.Single(pcrs);
        Assert.Equal(1u, pcrs[0].PartCount); // original, not regenerated
    }

    [Fact]
    public void FailedAndAbortedParts_CountedCorrectly()
    {
        var records = new StdfRecord[]
        {
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Prr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 1, PartFlag = 0 }, // pass
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Prr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 2, PartFlag = 0x08 }, // failed
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Prr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 3, PartFlag = 0x04 }, // abort
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Prr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 4, PartFlag = 0x10 }, // no pass/fail
        };

        var result = records.AsEnumerable().GeneratePartCounts(SummaryScope.Overall).ToList();
        var pcr = result.OfType<Pcr>().Single();
        Assert.Equal((byte)255, pcr.HeadNumber);
        Assert.Equal((byte)255, pcr.SiteNumber);
        Assert.Equal(4u, pcr.PartCount);
        Assert.Equal(1u, pcr.GoodCount); // only the first part
        Assert.Equal(1u, pcr.AbortCount);
        Assert.Null(pcr.RetestCount);
        Assert.Null(pcr.FunctionalCount);
    }

    [Fact]
    public void ScopeAll_GeneratesAllLevels()
    {
        var records = new StdfRecord[]
        {
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Prr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 1, PartFlag = 0 },
            new Pir { HeadNumber = 1, SiteNumber = 2 },
            new Prr { HeadNumber = 1, SiteNumber = 2, HardwareBin = 1, PartFlag = 0 },
        };

        var result = records.AsEnumerable().GeneratePartCounts(SummaryScope.All).ToList();
        var pcrs = result.OfType<Pcr>().ToList();

        // HeadSite: (1,1) and (1,2)
        Assert.Contains(pcrs, p => p.HeadNumber == 1 && p.SiteNumber == 1);
        Assert.Contains(pcrs, p => p.HeadNumber == 1 && p.SiteNumber == 2);
        // Head: (1, 255)
        Assert.Contains(pcrs, p => p.HeadNumber == 1 && p.SiteNumber == 255);
        // Overall: (255, 255)
        Assert.Contains(pcrs, p => p.HeadNumber == 255 && p.SiteNumber == 255);
        Assert.Equal(4, pcrs.Count);
    }

    [Fact]
    public void ScopeHead_AggregatesAcrossSites()
    {
        var records = new StdfRecord[]
        {
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Prr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 1, PartFlag = 0 },
            new Pir { HeadNumber = 1, SiteNumber = 2 },
            new Prr { HeadNumber = 1, SiteNumber = 2, HardwareBin = 1, PartFlag = 0 },
        };

        var result = records.AsEnumerable().GeneratePartCounts(SummaryScope.Head).ToList();
        var pcr = result.OfType<Pcr>().Single();
        Assert.Equal((byte)1, pcr.HeadNumber);
        Assert.Equal((byte)255, pcr.SiteNumber);
        Assert.Equal(2u, pcr.PartCount);
    }

    [Fact]
    public async Task AsyncVersion_MatchesSync()
    {
        var records = new StdfRecord[]
        {
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Prr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 1, PartFlag = 0 },
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Prr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 2, PartFlag = 0x08 },
        };

        var syncResult = records.AsEnumerable().GeneratePartCounts().ToList();

        var asyncResult = new List<StdfRecord>();
        await foreach (var rec in ToAsync(records).GeneratePartCounts())
            asyncResult.Add(rec);

        Assert.Equal(syncResult.Count, asyncResult.Count);
        for (int i = 0; i < syncResult.Count; i++)
            Assert.Equal(syncResult[i].GetType(), asyncResult[i].GetType());
    }

    private static async IAsyncEnumerable<StdfRecord> ToAsync(params StdfRecord[] records)
    {
        foreach (var r in records) yield return r;
        await Task.CompletedTask;
    }
}
