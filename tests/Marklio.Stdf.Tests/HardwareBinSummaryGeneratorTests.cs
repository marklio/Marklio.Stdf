using Marklio.Stdf;
using Marklio.Stdf.Records;

namespace Marklio.Stdf.Tests;

#pragma warning disable STDF0001

public class HardwareBinSummaryGeneratorTests
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

        var result = records.AsEnumerable().GenerateHardwareBinSummaries().ToList();
        Assert.Equal(3, result.Count);
        Assert.DoesNotContain(result, r => r is Hbr);
    }

    [Fact]
    public void SingleBin_GeneratesHbr()
    {
        var records = new StdfRecord[]
        {
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Prr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 5, PartFlag = 0 },
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Prr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 5, PartFlag = 0 },
            new Wrr { HeadNumber = 1 },
            new Mrr(),
        };

        var result = records.AsEnumerable().GenerateHardwareBinSummaries(SummaryScope.HeadSite).ToList();
        var hbrs = result.OfType<Hbr>().ToList();
        Assert.Single(hbrs);
        Assert.Equal((byte)1, hbrs[0].HeadNumber);
        Assert.Equal((byte)1, hbrs[0].SiteNumber);
        Assert.Equal((ushort)5, hbrs[0].HardwareBin);
        Assert.Equal(2u, hbrs[0].BinCount);
        Assert.Equal('P', hbrs[0].BinPassFail);
    }

    [Fact]
    public void MultipleBins_GeneratesOneHbrPerBin()
    {
        var records = new StdfRecord[]
        {
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Prr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 1, PartFlag = 0 },
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Prr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 2, PartFlag = 0x08 },
        };

        var result = records.AsEnumerable().GenerateHardwareBinSummaries(SummaryScope.HeadSite).ToList();
        var hbrs = result.OfType<Hbr>().ToList();
        Assert.Equal(2, hbrs.Count);

        var bin1 = hbrs.Single(h => h.HardwareBin == 1);
        Assert.Equal(1u, bin1.BinCount);
        Assert.Equal('P', bin1.BinPassFail);

        var bin2 = hbrs.Single(h => h.HardwareBin == 2);
        Assert.Equal(1u, bin2.BinCount);
        Assert.Equal('F', bin2.BinPassFail);
    }

    [Fact]
    public void PassFailDetermination()
    {
        var records = new StdfRecord[]
        {
            // Bin 1: all pass
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Prr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 1, PartFlag = 0 },
            // Bin 2: has a failure
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Prr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 2, PartFlag = 0 },
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Prr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 2, PartFlag = 0x08 },
            // Bin 3: no pass/fail indication
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Prr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 3, PartFlag = 0x10 },
        };

        var result = records.AsEnumerable().GenerateHardwareBinSummaries(SummaryScope.Overall).ToList();
        var hbrs = result.OfType<Hbr>().ToList();

        Assert.Equal('P', hbrs.Single(h => h.HardwareBin == 1).BinPassFail);
        Assert.Equal('F', hbrs.Single(h => h.HardwareBin == 2).BinPassFail);
        Assert.Equal(' ', hbrs.Single(h => h.HardwareBin == 3).BinPassFail);
    }

    [Fact]
    public void ExistingHbr_NotDuplicated()
    {
        var records = new StdfRecord[]
        {
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Prr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 1, PartFlag = 0 },
            new Hbr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 1, BinCount = 99 },
        };

        var result = records.AsEnumerable().GenerateHardwareBinSummaries(SummaryScope.HeadSite).ToList();
        var hbrs = result.OfType<Hbr>().ToList();
        Assert.Single(hbrs);
        Assert.Equal(99u, hbrs[0].BinCount); // original, not regenerated
    }

    [Fact]
    public void InsertedBeforeWrr()
    {
        var records = new StdfRecord[]
        {
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Prr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 1, PartFlag = 0 },
            new Wrr { HeadNumber = 1 },
            new Mrr(),
        };

        var result = records.AsEnumerable().GenerateHardwareBinSummaries(SummaryScope.HeadSite).ToList();
        int hbrIndex = result.FindIndex(r => r is Hbr);
        int wrrIndex = result.FindIndex(r => r is Wrr);
        Assert.True(hbrIndex < wrrIndex, "HBR should appear before WRR");
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

        var result = records.AsEnumerable().GenerateHardwareBinSummaries(SummaryScope.All).ToList();
        var hbrs = result.OfType<Hbr>().ToList();

        // HeadSite: (1,1,bin1) and (1,2,bin1)
        Assert.Contains(hbrs, h => h.HeadNumber == 1 && h.SiteNumber == 1 && h.HardwareBin == 1);
        Assert.Contains(hbrs, h => h.HeadNumber == 1 && h.SiteNumber == 2 && h.HardwareBin == 1);
        // Head: (1,255,bin1)
        Assert.Contains(hbrs, h => h.HeadNumber == 1 && h.SiteNumber == 255 && h.HardwareBin == 1);
        // Overall: (255,255,bin1)
        Assert.Contains(hbrs, h => h.HeadNumber == 255 && h.SiteNumber == 255 && h.HardwareBin == 1);
        Assert.Equal(4, hbrs.Count);
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

        var result = records.AsEnumerable().GenerateHardwareBinSummaries(SummaryScope.Head).ToList();
        var hbr = result.OfType<Hbr>().Single();
        Assert.Equal((byte)1, hbr.HeadNumber);
        Assert.Equal((byte)255, hbr.SiteNumber);
        Assert.Equal(2u, hbr.BinCount);
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

        var syncResult = records.AsEnumerable().GenerateHardwareBinSummaries().ToList();

        var asyncResult = new List<StdfRecord>();
        await foreach (var rec in ToAsync(records).GenerateHardwareBinSummaries())
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
