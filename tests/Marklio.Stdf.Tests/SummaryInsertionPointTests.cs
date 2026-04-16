using Marklio.Stdf;
using Marklio.Stdf.Records;

namespace Marklio.Stdf.Tests;

#pragma warning disable STDF0001

public class SummaryInsertionPointTests
{
    [Fact]
    public void WrrBeforeMrr_ReturnsWrrIndex()
    {
        var records = new List<StdfRecord>
        {
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Prr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 1, SoftwareBin = 1, PartFlag = 0 },
            new Wrr { HeadNumber = 1 },
            new Mrr(),
        };

        int index = SummaryInsertionPoint.Find(records);

        Assert.Equal(2, index);
        Assert.IsType<Wrr>(records[index]);
    }

    [Fact]
    public void MrrBeforeWrr_ReturnsMrrIndex()
    {
        var records = new List<StdfRecord>
        {
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Prr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 1, SoftwareBin = 1, PartFlag = 0 },
            new Mrr(),
            new Wrr { HeadNumber = 1 },
        };

        int index = SummaryInsertionPoint.Find(records);

        Assert.Equal(2, index);
        Assert.IsType<Mrr>(records[index]);
    }

    [Fact]
    public void OnlyWrr_ReturnsWrrIndex()
    {
        var records = new List<StdfRecord>
        {
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Prr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 1, SoftwareBin = 1, PartFlag = 0 },
            new Wrr { HeadNumber = 1 },
        };

        int index = SummaryInsertionPoint.Find(records);

        Assert.Equal(2, index);
    }

    [Fact]
    public void OnlyMrr_ReturnsMrrIndex()
    {
        var records = new List<StdfRecord>
        {
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Prr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 1, SoftwareBin = 1, PartFlag = 0 },
            new Mrr(),
        };

        int index = SummaryInsertionPoint.Find(records);

        Assert.Equal(2, index);
    }

    [Fact]
    public void NeitherWrrNorMrr_ReturnsEnd()
    {
        var records = new List<StdfRecord>
        {
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Prr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 1, SoftwareBin = 1, PartFlag = 0 },
        };

        int index = SummaryInsertionPoint.Find(records);

        Assert.Equal(records.Count, index);
    }

    [Fact]
    public void MrrBeforeWrr_SummariesInsertedBeforeMrr()
    {
        var records = new StdfRecord[]
        {
            new Pir { HeadNumber = 1, SiteNumber = 1 },
            new Ptr { TestNumber = 1, HeadNumber = 1, SiteNumber = 1, TestFlags = 0, ParametricFlags = 0, Result = 1.0f },
            new Prr { HeadNumber = 1, SiteNumber = 1, HardwareBin = 1, SoftwareBin = 1, PartFlag = 0 },
            new Mrr(),
            new Wrr { HeadNumber = 1 },
        };

        var result = records.AsEnumerable().GenerateSummaries(SummaryScope.Overall).ToList();
        int mrrIndex = result.FindIndex(r => r is Mrr);

        foreach (var (rec, i) in result.Select((r, i) => (r, i)))
        {
            if (rec is Pcr or Hbr or Sbr or Tsr)
                Assert.True(i < mrrIndex, $"{rec.GetType().Name} at index {i} should be before MRR at index {mrrIndex}");
        }
    }
}
