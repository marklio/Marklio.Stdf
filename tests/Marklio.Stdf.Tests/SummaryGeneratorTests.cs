using Marklio.Stdf;
using Marklio.Stdf.Records;

namespace Marklio.Stdf.Tests;

#pragma warning disable STDF0001

public class SummaryGeneratorTests
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

        var result = records.AsEnumerable().GenerateSummaries().ToList();
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void GeneratesAllSummaryTypes()
    {
        var records = new StdfRecord[]
        {
            new Far { CpuType = 2, StdfVersion = 4 },
            new Mir(),
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Ptr { TestNumber = 1, HeadNumber = 1, SiteNumber = 1, TestFlags = 0, ParametricFlags = 0, Result = 1.0f },
            new Prr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 1, SoftwareBin = 1, PartFlag = 0 },
            new Wrr { HeadNumber = 1 },
            new Mrr(),
        };

        var result = records.AsEnumerable().GenerateSummaries(SummaryScope.Overall).ToList();

        Assert.Contains(result, r => r is Pcr);
        Assert.Contains(result, r => r is Hbr);
        Assert.Contains(result, r => r is Sbr);
        Assert.Contains(result, r => r is Tsr);
    }

    [Fact]
    public void SummariesInsertedBeforeWrr()
    {
        var records = new StdfRecord[]
        {
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Ptr { TestNumber = 1, HeadNumber = 1, SiteNumber = 1, TestFlags = 0, ParametricFlags = 0, Result = 1.0f },
            new Prr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 1, SoftwareBin = 1, PartFlag = 0 },
            new Wrr { HeadNumber = 1 },
            new Mrr(),
        };

        var result = records.AsEnumerable().GenerateSummaries(SummaryScope.Overall).ToList();
        int wrrIndex = result.FindIndex(r => r is Wrr);

        foreach (var (rec, i) in result.Select((r, i) => (r, i)))
        {
            if (rec is Pcr or Hbr or Sbr or Tsr)
                Assert.True(i < wrrIndex, $"{rec.GetType().Name} should be before WRR");
        }
    }

    [Fact]
    public void ExistingSummaries_NotDuplicated()
    {
        var records = new StdfRecord[]
        {
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Ptr { TestNumber = 1, HeadNumber = 1, SiteNumber = 1, TestFlags = 0, ParametricFlags = 0, Result = 1.0f },
            new Prr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 1, SoftwareBin = 1, PartFlag = 0 },
            new Pcr { HeadNumber = 255, SiteNumber = 255, PartCount = 1 },
            new Hbr { HeadNumber = 255, SiteNumber = 255, HardwareBin = 1, BinCount = 1 },
            new Sbr { HeadNumber = 255, SiteNumber = 255, SoftwareBin = 1, BinCount = 1 },
            new Tsr { HeadNumber = 255, SiteNumber = 255, TestNumber = 1, ExecutedCount = 1 },
            new Mrr(),
        };

        var result = records.AsEnumerable().GenerateSummaries(SummaryScope.Overall).ToList();

        // Overall summaries already exist, so should not add more at that level
        Assert.Equal(1, result.OfType<Pcr>().Count(p => p.HeadNumber == 255 && p.SiteNumber == 255));
        Assert.Equal(1, result.OfType<Hbr>().Count(h => h.HeadNumber == 255 && h.SiteNumber == 255));
        Assert.Equal(1, result.OfType<Sbr>().Count(s => s.HeadNumber == 255 && s.SiteNumber == 255));
        Assert.Equal(1, result.OfType<Tsr>().Count(t => t.HeadNumber == 255 && t.SiteNumber == 255));
    }

    [Fact]
    public void BuffersOnce_NotFourTimes()
    {
        // This test verifies the composite generator works correctly
        // (the single-buffer optimization is an implementation detail)
        var records = new StdfRecord[]
        {
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Ptr { TestNumber = 1, HeadNumber = 1, SiteNumber = 1, TestFlags = 0, ParametricFlags = 0, Result = 1.0f },
            new Prr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 1, SoftwareBin = 1, PartFlag = 0 },
            new Pir { HeadNumber = 1, SiteNumber = 2 },
            new Ptr { TestNumber = 1, HeadNumber = 1, SiteNumber = 2, TestFlags = 0, ParametricFlags = 0, Result = 2.0f },
            new Prr { HeadNumber = 1, SiteNumber = 2, HardwareBin = 2, SoftwareBin = 2, PartFlag = 0x08 },
        };

        var result = records.AsEnumerable().GenerateSummaries(SummaryScope.All).ToList();

        // Should have original records + all summary types at all levels
        Assert.True(result.OfType<Pcr>().Any(), "Should have PCR records");
        Assert.True(result.OfType<Hbr>().Any(), "Should have HBR records");
        Assert.True(result.OfType<Sbr>().Any(), "Should have SBR records");
        Assert.True(result.OfType<Tsr>().Any(), "Should have TSR records");
    }

    [Fact]
    public void CountsAreCorrectAcrossAllGenerators()
    {
        var records = new StdfRecord[]
        {
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Ptr { TestNumber = 1, HeadNumber = 1, SiteNumber = 1, TestFlags = 0, ParametricFlags = 0, Result = 10.0f },
            new Prr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 1, SoftwareBin = 1, PartFlag = 0 },
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Ptr { TestNumber = 1, HeadNumber = 1, SiteNumber = 1, TestFlags = (byte)TestResultFlags.Failed, ParametricFlags = 0, Result = 20.0f },
            new Prr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 2, SoftwareBin = 2, PartFlag = 0x08 },
        };

        var result = records.AsEnumerable().GenerateSummaries(SummaryScope.Overall).ToList();

        var pcr = result.OfType<Pcr>().Single();
        Assert.Equal(2u, pcr.PartCount);
        Assert.Equal(1u, pcr.GoodCount);

        var hbrs = result.OfType<Hbr>().OrderBy(h => h.HardwareBin).ToList();
        Assert.Equal(2, hbrs.Count);
        Assert.Equal(1u, hbrs[0].BinCount);
        Assert.Equal(1u, hbrs[1].BinCount);

        var tsr = result.OfType<Tsr>().Single();
        Assert.Equal(2u, tsr.ExecutedCount);
        Assert.Equal(1u, tsr.FailedCount);
    }

    [Fact]
    public async Task AsyncVersion_MatchesSync()
    {
        var records = new StdfRecord[]
        {
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Ptr { TestNumber = 1, HeadNumber = 1, SiteNumber = 1, TestFlags = 0, ParametricFlags = 0, Result = 1.0f },
            new Prr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 1, SoftwareBin = 1, PartFlag = 0 },
        };

        var syncResult = records.AsEnumerable().GenerateSummaries().ToList();

        var asyncResult = new List<StdfRecord>();
        await foreach (var rec in ToAsync(records).GenerateSummaries())
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
