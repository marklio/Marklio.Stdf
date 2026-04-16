using Marklio.Stdf;
using Marklio.Stdf.Records;

namespace Marklio.Stdf.Tests;

#pragma warning disable STDF0001

public class SoftwareBinSummaryGeneratorTests
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

        var result = records.AsEnumerable().GenerateSoftwareBinSummaries().ToList();
        Assert.Equal(3, result.Count);
        Assert.DoesNotContain(result, r => r is Sbr);
    }

    [Fact]
    public void PrrWithoutSoftwareBin_Skipped()
    {
        var records = new StdfRecord[]
        {
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Prr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 1, SoftwareBin = null, PartFlag = 0 },
        };

        var result = records.AsEnumerable().GenerateSoftwareBinSummaries(SummaryScope.HeadSite).ToList();
        Assert.DoesNotContain(result, r => r is Sbr);
    }

    [Fact]
    public void SingleBin_GeneratesSbr()
    {
        var records = new StdfRecord[]
        {
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Prr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 1, SoftwareBin = 10, PartFlag = 0 },
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Prr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 1, SoftwareBin = 10, PartFlag = 0 },
            new Wrr { HeadNumber = 1 },
            new Mrr(),
        };

        var result = records.AsEnumerable().GenerateSoftwareBinSummaries(SummaryScope.HeadSite).ToList();
        var sbrs = result.OfType<Sbr>().ToList();
        Assert.Single(sbrs);
        Assert.Equal((byte)1, sbrs[0].HeadNumber);
        Assert.Equal((byte)1, sbrs[0].SiteNumber);
        Assert.Equal((ushort)10, sbrs[0].SoftwareBin);
        Assert.Equal(2u, sbrs[0].BinCount);
        Assert.Equal('P', sbrs[0].BinPassFail);
    }

    [Fact]
    public void MultipleBins_GeneratesOneSbrPerBin()
    {
        var records = new StdfRecord[]
        {
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Prr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 1, SoftwareBin = 1, PartFlag = 0 },
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Prr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 2, SoftwareBin = 2, PartFlag = 0x08 },
        };

        var result = records.AsEnumerable().GenerateSoftwareBinSummaries(SummaryScope.HeadSite).ToList();
        var sbrs = result.OfType<Sbr>().ToList();
        Assert.Equal(2, sbrs.Count);

        var bin1 = sbrs.Single(s => s.SoftwareBin == 1);
        Assert.Equal(1u, bin1.BinCount);
        Assert.Equal('P', bin1.BinPassFail);

        var bin2 = sbrs.Single(s => s.SoftwareBin == 2);
        Assert.Equal(1u, bin2.BinCount);
        Assert.Equal('F', bin2.BinPassFail);
    }

    [Fact]
    public void ExistingSbr_NotDuplicated()
    {
        var records = new StdfRecord[]
        {
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Prr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 1, SoftwareBin = 1, PartFlag = 0 },
            new Sbr { HeadNumber = 1, SiteNumber = 1, SoftwareBin = 1, BinCount = 99 },
        };

        var result = records.AsEnumerable().GenerateSoftwareBinSummaries(SummaryScope.HeadSite).ToList();
        var sbrs = result.OfType<Sbr>().ToList();
        Assert.Single(sbrs);
        Assert.Equal(99u, sbrs[0].BinCount);
    }

    [Fact]
    public void InsertedBeforeWrr()
    {
        var records = new StdfRecord[]
        {
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Prr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 1, SoftwareBin = 1, PartFlag = 0 },
            new Wrr { HeadNumber = 1 },
            new Mrr(),
        };

        var result = records.AsEnumerable().GenerateSoftwareBinSummaries(SummaryScope.HeadSite).ToList();
        int sbrIndex = result.FindIndex(r => r is Sbr);
        int wrrIndex = result.FindIndex(r => r is Wrr);
        Assert.True(sbrIndex < wrrIndex, "SBR should appear before WRR");
    }

    [Fact]
    public void ScopeAll_GeneratesAllLevels()
    {
        var records = new StdfRecord[]
        {
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Prr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 1, SoftwareBin = 1, PartFlag = 0 },
            new Pir { HeadNumber = 1, SiteNumber = 2 },
            new Prr { HeadNumber = 1, SiteNumber = 2, HardwareBin = 1, SoftwareBin = 1, PartFlag = 0 },
        };

        var result = records.AsEnumerable().GenerateSoftwareBinSummaries(SummaryScope.All).ToList();
        var sbrs = result.OfType<Sbr>().ToList();

        Assert.Contains(sbrs, s => s.HeadNumber == 1 && s.SiteNumber == 1);
        Assert.Contains(sbrs, s => s.HeadNumber == 1 && s.SiteNumber == 2);
        Assert.Contains(sbrs, s => s.HeadNumber == 1 && s.SiteNumber == 255);
        Assert.Contains(sbrs, s => s.HeadNumber == 255 && s.SiteNumber == 255);
        Assert.Equal(4, sbrs.Count);
    }

    [Fact]
    public void ScopeOverall_SingleAggregate()
    {
        var records = new StdfRecord[]
        {
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Prr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 1, SoftwareBin = 1, PartFlag = 0 },
            new Pir { HeadNumber = 2, SiteNumber = 1 },
            new Prr { HeadNumber = 2, SiteNumber = 1, HardwareBin = 1, SoftwareBin = 1, PartFlag = 0 },
        };

        var result = records.AsEnumerable().GenerateSoftwareBinSummaries(SummaryScope.Overall).ToList();
        var sbr = result.OfType<Sbr>().Single();
        Assert.Equal((byte)255, sbr.HeadNumber);
        Assert.Equal((byte)255, sbr.SiteNumber);
        Assert.Equal(2u, sbr.BinCount);
    }

    [Fact]
    public async Task AsyncVersion_MatchesSync()
    {
        var records = new StdfRecord[]
        {
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Prr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 1, SoftwareBin = 1, PartFlag = 0 },
        };

        var syncResult = records.AsEnumerable().GenerateSoftwareBinSummaries().ToList();

        var asyncResult = new List<StdfRecord>();
        await foreach (var rec in ToAsync(records).GenerateSoftwareBinSummaries())
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
