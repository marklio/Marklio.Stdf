using Marklio.Stdf;
using Marklio.Stdf.Records;

namespace Marklio.Stdf.Tests;

#pragma warning disable STDF0001

public class TestSummaryGeneratorTests
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

        var result = records.AsEnumerable().GenerateTestSummaries().ToList();
        Assert.Equal(3, result.Count);
        Assert.DoesNotContain(result, r => r is Tsr);
    }

    [Fact]
    public void PtrRecords_GenerateTsrWithStats()
    {
        var records = new StdfRecord[]
        {
            new Ptr { TestNumber = 100, HeadNumber = 1, SiteNumber = 1, TestFlags = 0, ParametricFlags = 0, Result = 1.5f, TestText = "Voltage" },
            new Ptr { TestNumber = 100, HeadNumber = 1, SiteNumber = 1, TestFlags = 0, ParametricFlags = 0, Result = 2.5f },
            new Ptr { TestNumber = 100, HeadNumber = 1, SiteNumber = 1, TestFlags = 0, ParametricFlags = 0, Result = 3.0f },
            new Wrr { HeadNumber = 1 },
            new Mrr(),
        };

        var result = records.AsEnumerable().GenerateTestSummaries(SummaryScope.HeadSite).ToList();
        var tsr = result.OfType<Tsr>().Single();
        Assert.Equal((byte)1, tsr.HeadNumber);
        Assert.Equal((byte)1, tsr.SiteNumber);
        Assert.Equal(100u, tsr.TestNumber);
        Assert.Equal('P', tsr.TestType);
        Assert.Equal("Voltage", tsr.TestName);
        Assert.Equal(3u, tsr.ExecutedCount);
        Assert.Equal(0u, tsr.FailedCount);
        Assert.Equal(0u, tsr.AlarmCount);
        Assert.Equal(1.5f, tsr.ResultMin);
        Assert.Equal(3.0f, tsr.ResultMax);
        Assert.Equal(7.0f, tsr.ResultSum);
        Assert.NotNull(tsr.ResultSumOfSquares);
        Assert.Equal((byte)0, tsr.OptionalFlags); // all stats valid
    }

    [Fact]
    public void PtrFailedAndAlarm_CountedCorrectly()
    {
        var records = new StdfRecord[]
        {
            new Ptr { TestNumber = 1, HeadNumber = 1, SiteNumber = 1, TestFlags = (byte)(TestResultFlags.Failed | TestResultFlags.PassFailValid), ParametricFlags = 0, Result = 1.0f },
            new Ptr { TestNumber = 1, HeadNumber = 1, SiteNumber = 1, TestFlags = (byte)TestResultFlags.Alarm, ParametricFlags = 0, Result = 2.0f },
            new Ptr { TestNumber = 1, HeadNumber = 1, SiteNumber = 1, TestFlags = 0, ParametricFlags = 0, Result = 3.0f },
        };

        var result = records.AsEnumerable().GenerateTestSummaries(SummaryScope.HeadSite).ToList();
        var tsr = result.OfType<Tsr>().Single();
        Assert.Equal(3u, tsr.ExecutedCount);
        Assert.Equal(1u, tsr.FailedCount);
        Assert.Equal(1u, tsr.AlarmCount);
    }

    [Fact]
    public void FtrRecords_NoResultStats()
    {
        var records = new StdfRecord[]
        {
            new Ftr { TestNumber = 200, HeadNumber = 1, SiteNumber = 1, TestFlags = 0, TestText = "Continuity" },
            new Ftr { TestNumber = 200, HeadNumber = 1, SiteNumber = 1, TestFlags = (byte)TestResultFlags.Failed },
        };

        var result = records.AsEnumerable().GenerateTestSummaries(SummaryScope.HeadSite).ToList();
        var tsr = result.OfType<Tsr>().Single();
        Assert.Equal('F', tsr.TestType);
        Assert.Equal("Continuity", tsr.TestName);
        Assert.Equal(2u, tsr.ExecutedCount);
        Assert.Equal(1u, tsr.FailedCount);
        // FTR has no result values — stats should be marked invalid
        Assert.NotNull(tsr.OptionalFlags);
        Assert.NotEqual((byte)0, tsr.OptionalFlags);
    }

    [Fact]
    public void MprRecords_StatsFromReturnResults()
    {
        var records = new StdfRecord[]
        {
            new Mpr { TestNumber = 300, HeadNumber = 1, SiteNumber = 1, TestFlags = 0, ParametricFlags = 0, ReturnResults = [1.0f, 2.0f], TestText = "Multi" },
            new Mpr { TestNumber = 300, HeadNumber = 1, SiteNumber = 1, TestFlags = 0, ParametricFlags = 0, ReturnResults = [3.0f] },
        };

        var result = records.AsEnumerable().GenerateTestSummaries(SummaryScope.HeadSite).ToList();
        var tsr = result.OfType<Tsr>().Single();
        Assert.Equal('M', tsr.TestType);
        Assert.Equal("Multi", tsr.TestName);
        Assert.Equal(2u, tsr.ExecutedCount);
        Assert.Equal(1.0f, tsr.ResultMin);
        Assert.Equal(3.0f, tsr.ResultMax);
        Assert.Equal(6.0f, tsr.ResultSum); // 1+2+3
    }

    [Fact]
    public void ExistingTsr_NotDuplicated()
    {
        var records = new StdfRecord[]
        {
            new Ptr { TestNumber = 1, HeadNumber = 1, SiteNumber = 1, TestFlags = 0, ParametricFlags = 0, Result = 1.0f },
            new Tsr { HeadNumber = 1, SiteNumber = 1, TestNumber = 1, ExecutedCount = 99 },
        };

        var result = records.AsEnumerable().GenerateTestSummaries(SummaryScope.HeadSite).ToList();
        var tsrs = result.OfType<Tsr>().ToList();
        Assert.Single(tsrs);
        Assert.Equal(99u, tsrs[0].ExecutedCount); // original, not regenerated
    }

    [Fact]
    public void InsertedBeforeWrr()
    {
        var records = new StdfRecord[]
        {
            new Ptr { TestNumber = 1, HeadNumber = 1, SiteNumber = 1, TestFlags = 0, ParametricFlags = 0, Result = 1.0f },
            new Wrr { HeadNumber = 1 },
            new Mrr(),
        };

        var result = records.AsEnumerable().GenerateTestSummaries(SummaryScope.HeadSite).ToList();
        int tsrIndex = result.FindIndex(r => r is Tsr);
        int wrrIndex = result.FindIndex(r => r is Wrr);
        Assert.True(tsrIndex < wrrIndex, "TSR should appear before WRR");
    }

    [Fact]
    public void ScopeAll_GeneratesAllLevels()
    {
        var records = new StdfRecord[]
        {
            new Ptr { TestNumber = 1, HeadNumber = 1, SiteNumber = 1, TestFlags = 0, ParametricFlags = 0, Result = 1.0f },
            new Ptr { TestNumber = 1, HeadNumber = 1, SiteNumber = 2, TestFlags = 0, ParametricFlags = 0, Result = 2.0f },
        };

        var result = records.AsEnumerable().GenerateTestSummaries(SummaryScope.All).ToList();
        var tsrs = result.OfType<Tsr>().ToList();

        // HeadSite: (1,1,test1) and (1,2,test1)
        Assert.Contains(tsrs, t => t.HeadNumber == 1 && t.SiteNumber == 1);
        Assert.Contains(tsrs, t => t.HeadNumber == 1 && t.SiteNumber == 2);
        // Head: (1,255,test1)
        Assert.Contains(tsrs, t => t.HeadNumber == 1 && t.SiteNumber == 255);
        // Overall: (255,255,test1)
        Assert.Contains(tsrs, t => t.HeadNumber == 255 && t.SiteNumber == 255);
        Assert.Equal(4, tsrs.Count);
    }

    [Fact]
    public void ScopeOverall_AggregatesAllHeadsSites()
    {
        var records = new StdfRecord[]
        {
            new Ptr { TestNumber = 1, HeadNumber = 1, SiteNumber = 1, TestFlags = 0, ParametricFlags = 0, Result = 1.0f },
            new Ptr { TestNumber = 1, HeadNumber = 2, SiteNumber = 1, TestFlags = 0, ParametricFlags = 0, Result = 5.0f },
        };

        var result = records.AsEnumerable().GenerateTestSummaries(SummaryScope.Overall).ToList();
        var tsr = result.OfType<Tsr>().Single();
        Assert.Equal((byte)255, tsr.HeadNumber);
        Assert.Equal((byte)255, tsr.SiteNumber);
        Assert.Equal(2u, tsr.ExecutedCount);
        Assert.Equal(1.0f, tsr.ResultMin);
        Assert.Equal(5.0f, tsr.ResultMax);
    }

    [Fact]
    public void PtrWithNullResult_ExcludedFromStats()
    {
        var records = new StdfRecord[]
        {
            new Ptr { TestNumber = 1, HeadNumber = 1, SiteNumber = 1, TestFlags = 0, ParametricFlags = 0, Result = 2.0f },
            new Ptr { TestNumber = 1, HeadNumber = 1, SiteNumber = 1, TestFlags = 0, ParametricFlags = 0, Result = null },
        };

        var result = records.AsEnumerable().GenerateTestSummaries(SummaryScope.HeadSite).ToList();
        var tsr = result.OfType<Tsr>().Single();
        Assert.Equal(2u, tsr.ExecutedCount);
        Assert.Equal(2.0f, tsr.ResultMin);
        Assert.Equal(2.0f, tsr.ResultMax);
        Assert.Equal(2.0f, tsr.ResultSum);
    }

    [Fact]
    public async Task AsyncVersion_MatchesSync()
    {
        var records = new StdfRecord[]
        {
            new Ptr { TestNumber = 1, HeadNumber = 1, SiteNumber = 1, TestFlags = 0, ParametricFlags = 0, Result = 1.0f },
            new Ftr { TestNumber = 2, HeadNumber = 1, SiteNumber = 1, TestFlags = 0 },
        };

        var syncResult = records.AsEnumerable().GenerateTestSummaries().ToList();

        var asyncResult = new List<StdfRecord>();
        await foreach (var rec in ToAsync(records).GenerateTestSummaries())
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
