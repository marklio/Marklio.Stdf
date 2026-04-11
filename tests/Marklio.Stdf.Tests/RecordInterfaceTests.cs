using Marklio.Stdf;
using Marklio.Stdf.Records;
using Xunit;

namespace Marklio.Stdf.Tests;

/// <summary>
/// Tests for shared record interfaces (IHeadRecord, IHeadSiteRecord, IBinRecord, ITestRecord).
/// </summary>
public class RecordInterfaceTests
{
    [Fact]
    public void IHeadRecord_MatchesHeadOnlyRecords()
    {
        IHeadRecord wir = new Wir { HeadNumber = 1 };
        IHeadRecord wrr = new Wrr { HeadNumber = 2 };
        IHeadRecord sdr = new Sdr { HeadNumber = 3 };

        Assert.Equal((byte)1, wir.HeadNumber);
        Assert.Equal((byte)2, wrr.HeadNumber);
        Assert.Equal((byte)3, sdr.HeadNumber);
    }

    [Fact]
    public void IHeadSiteRecord_MatchesSiteRecords()
    {
        IHeadSiteRecord pir = new Pir { HeadNumber = 1, SiteNumber = 0 };
        IHeadSiteRecord prr = new Prr { HeadNumber = 1, SiteNumber = 3 };
        IHeadSiteRecord pcr = new Pcr { HeadNumber = 255, SiteNumber = 0 };
        IHeadSiteRecord tsr = new Tsr { HeadNumber = 1, SiteNumber = 2 };

        Assert.Equal((byte)0, pir.SiteNumber);
        Assert.Equal((byte)3, prr.SiteNumber);
        Assert.Equal((byte)255, pcr.HeadNumber);
        Assert.Equal((byte)2, tsr.SiteNumber);
    }

    [Fact]
    public void ITestRecord_PatternMatchAcrossTestTypes()
    {
        object[] records =
        [
            new Ptr { TestNumber = 1001, HeadNumber = 1, SiteNumber = 0 },
            new Mpr { TestNumber = 1002, HeadNumber = 1, SiteNumber = 1 },
            new Ftr { TestNumber = 1003, HeadNumber = 1, SiteNumber = 2 },
        ];

        var testNumbers = new List<uint>();
        foreach (var rec in records)
        {
            if (rec is ITestRecord test)
                testNumbers.Add(test.TestNumber);
        }

        Assert.Equal([1001u, 1002u, 1003u], testNumbers);
    }

    [Fact]
    public void IBinRecord_PatternMatchAcrossHbrAndSbr()
    {
        object[] records =
        [
            new Hbr { HeadNumber = 1, SiteNumber = 0, HardwareBin = 1, BinCount = 100, BinPassFail = 'P', BinName = "Pass" },
            new Sbr { HeadNumber = 1, SiteNumber = 0, SoftwareBin = 10, BinCount = 50, BinPassFail = 'F', BinName = "Fail" },
            new Hbr { HeadNumber = 1, SiteNumber = 0, HardwareBin = 2, BinCount = 25, BinPassFail = 'F', BinName = "Leak" },
        ];

        var results = new List<(ushort Bin, uint Count, char? PF, string? Name)>();
        foreach (var rec in records)
        {
            if (rec is IBinRecord bin)
                results.Add((bin.BinNumber, bin.BinCount, bin.PassFail, bin.BinName));
        }

        Assert.Equal(3, results.Count);
        Assert.Equal((ushort)1, results[0].Bin);     // HBR.HardwareBin via IBinRecord.BinNumber
        Assert.Equal(100u, results[0].Count);
        Assert.Equal('P', results[0].PF);             // HBR.BinPassFail via IBinRecord.PassFail
        Assert.Equal("Pass", results[0].Name);

        Assert.Equal((ushort)10, results[1].Bin);     // SBR.SoftwareBin via IBinRecord.BinNumber
        Assert.Equal('F', results[1].PF);

        Assert.Equal((ushort)2, results[2].Bin);
        Assert.Equal("Leak", results[2].Name);
    }

    [Fact]
    public void ITestRecord_InheritsIHeadSiteRecord()
    {
        // ITestRecord : IHeadSiteRecord : IHeadRecord — full chain works
        var ptr = new Ptr { TestNumber = 42, HeadNumber = 1, SiteNumber = 5 };

        ITestRecord test = ptr;
        IHeadSiteRecord hs = ptr;
        IHeadRecord head = ptr;

        Assert.Equal(42u, test.TestNumber);
        Assert.Equal((byte)5, hs.SiteNumber);
        Assert.Equal((byte)1, head.HeadNumber);
    }

    [Fact]
    public void IBinRecord_InheritsIHeadSiteRecord()
    {
        var hbr = new Hbr { HeadNumber = 2, SiteNumber = 3, HardwareBin = 1, BinCount = 10 };

        IBinRecord bin = hbr;
        IHeadSiteRecord hs = hbr;

        Assert.Equal((ushort)1, bin.BinNumber);
        Assert.Equal((byte)3, hs.SiteNumber);
        Assert.Equal((byte)2, ((IHeadRecord)hbr).HeadNumber);
    }

    [Fact]
    public void Str_ImplementsITestRecord()
    {
        // STR is hand-rolled (readonly record struct with init) — verify interface works
        var str = new Str
        {
            TestNumber = 999,
            HeadNumber = 1,
            SiteNumber = 4,
        };

        ITestRecord test = str;
        Assert.Equal(999u, test.TestNumber);
        Assert.Equal((byte)4, test.SiteNumber);
    }
}
