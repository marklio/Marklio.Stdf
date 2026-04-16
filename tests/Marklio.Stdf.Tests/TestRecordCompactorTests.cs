using Marklio.Stdf;
using Marklio.Stdf.Records;

namespace Marklio.Stdf.Tests;

public class TestRecordCompactorTests
{
    [Fact]
    public void FirstPtr_PassesThroughUnchanged()
    {
        var ptr = MakePtr(testNumber: 1);
        var result = new StdfRecord[] { ptr }.AsEnumerable().CompactTestRecords().ToList();

        Assert.Single(result);
        var p = Assert.IsType<Ptr>(result[0]);
        Assert.Equal("Vcc", p.TestText);
        Assert.Equal("V", p.Units);
        Assert.Equal(0.5f, p.LowLimit);
        Assert.Equal(2.0f, p.HighLimit);
    }

    [Fact]
    public void SecondPtr_SameTestNumber_HasStaticFieldsStripped()
    {
        var ptr1 = MakePtr(testNumber: 1);
        var ptr2 = MakePtr(testNumber: 1, result: 1.8f);

        var result = new StdfRecord[] { ptr1, ptr2 }.AsEnumerable().CompactTestRecords().ToList();

        Assert.Equal(2, result.Count);

        // First passes through unchanged
        var first = Assert.IsType<Ptr>(result[0]);
        Assert.Equal("Vcc", first.TestText);
        Assert.Equal("V", first.Units);

        // Second has static fields stripped
        var second = Assert.IsType<Ptr>(result[1]);
        Assert.Null(second.TestText);
        Assert.Null(second.AlarmId);
        Assert.Null(second.OptionalFlags);
        Assert.Null(second.ResultExponent);
        Assert.Null(second.LowLimitExponent);
        Assert.Null(second.HighLimitExponent);
        Assert.Null(second.LowLimit);
        Assert.Null(second.HighLimit);
        Assert.Null(second.Units);
        Assert.Null(second.ResultFormatString);
        Assert.Null(second.LowLimitFormatString);
        Assert.Null(second.HighLimitFormatString);
        Assert.Null(second.LowSpecLimit);
        Assert.Null(second.HighSpecLimit);

        // Dynamic fields preserved
        Assert.Equal(1.8f, second.Result);
        Assert.Equal((uint)1, second.TestNumber);
    }

    [Fact]
    public void DifferentTestNumbers_AreIndependent()
    {
        var ptr1 = MakePtr(testNumber: 1);
        var ptr2 = MakePtr(testNumber: 2, testText: "Icc", units: "A");

        var result = new StdfRecord[] { ptr1, ptr2 }.AsEnumerable().CompactTestRecords().ToList();

        Assert.Equal(2, result.Count);
        var first = Assert.IsType<Ptr>(result[0]);
        Assert.Equal("Vcc", first.TestText);
        var second = Assert.IsType<Ptr>(result[1]);
        Assert.Equal("Icc", second.TestText);
        Assert.Equal("A", second.Units);
    }

    [Fact]
    public void Ftr_CompactsCorrectly()
    {
        var ftr1 = MakeFtr(testNumber: 10);
        var ftr2 = MakeFtr(testNumber: 10);

        var result = new StdfRecord[] { ftr1, ftr2 }.AsEnumerable().CompactTestRecords().ToList();

        var first = Assert.IsType<Ftr>(result[0]);
        Assert.Equal("scan_test", first.VectorName);

        var second = Assert.IsType<Ftr>(result[1]);
        Assert.Null(second.VectorName);
        Assert.Null(second.TimeSet);
        Assert.Null(second.OpCode);
        Assert.Null(second.TestText);
        Assert.Null(second.AlarmId);
        Assert.Null(second.ProgramText);
        Assert.Null(second.ResultText);
        Assert.Null(second.PatternGeneratorNumber);
    }

    [Fact]
    public void Mpr_CompactsCorrectly()
    {
        var mpr1 = MakeMpr(testNumber: 20);
        var mpr2 = MakeMpr(testNumber: 20);

        var result = new StdfRecord[] { mpr1, mpr2 }.AsEnumerable().CompactTestRecords().ToList();

        var first = Assert.IsType<Mpr>(result[0]);
        Assert.Equal("MultiPin", first.TestText);

        var second = Assert.IsType<Mpr>(result[1]);
        Assert.Null(second.TestText);
        Assert.Null(second.AlarmId);
        Assert.Null(second.OptionalFlags);
        Assert.Null(second.ResultExponent);
        Assert.Null(second.LowLimitExponent);
        Assert.Null(second.HighLimitExponent);
        Assert.Null(second.LowLimit);
        Assert.Null(second.HighLimit);
        Assert.Null(second.StartingCondition);
        Assert.Null(second.ConditionIncrement);
        Assert.Null(second.Units);
        Assert.Null(second.UnitsInput);
        Assert.Null(second.ResultFormatString);
        Assert.Null(second.LowLimitFormatString);
        Assert.Null(second.HighLimitFormatString);
        Assert.Null(second.LowSpecLimit);
        Assert.Null(second.HighSpecLimit);
    }

    [Fact]
    public void NonTestRecords_PassThrough()
    {
        var far = new Far { CpuType = 2, StdfVersion = 4 };
        var pir = new Pir { HeadNumber = 1, SiteNumber = 1 };

        var result = new StdfRecord[] { far, pir }.AsEnumerable().CompactTestRecords().ToList();

        Assert.Equal(2, result.Count);
        Assert.IsType<Far>(result[0]);
        Assert.IsType<Pir>(result[1]);
    }

    [Fact]
    public void PtrAndFtr_SameTestNumber_TrackedIndependently()
    {
        var ptr = MakePtr(testNumber: 1);
        var ftr = MakeFtr(testNumber: 1);

        var result = new StdfRecord[] { ptr, ftr }.AsEnumerable().CompactTestRecords().ToList();

        // Both should pass through unchanged since they're different record types
        var p = Assert.IsType<Ptr>(result[0]);
        Assert.Equal("Vcc", p.TestText);
        var f = Assert.IsType<Ftr>(result[1]);
        Assert.Equal("scan_test", f.VectorName);
    }

    [Fact]
    public async Task AsyncVersion_Works()
    {
        var ptr1 = MakePtr(testNumber: 1);
        var ptr2 = MakePtr(testNumber: 1, result: 1.8f);

        var result = new List<StdfRecord>();
        await foreach (var rec in ToAsync(ptr1, ptr2).CompactTestRecords())
            result.Add(rec);

        Assert.Equal(2, result.Count);
        var first = Assert.IsType<Ptr>(result[0]);
        Assert.Equal("Vcc", first.TestText);
        var second = Assert.IsType<Ptr>(result[1]);
        Assert.Null(second.TestText);
        Assert.Equal(1.8f, second.Result);
    }

    private static Ptr MakePtr(
        uint testNumber,
        float result = 1.5f,
        string testText = "Vcc",
        string units = "V") => new()
    {
        TestNumber = testNumber,
        HeadNumber = 1,
        SiteNumber = 1,
        Result = result,
        TestText = testText,
        AlarmId = "AL01",
        OptionalFlags = 0x00,
        ResultExponent = -3,
        LowLimitExponent = -3,
        HighLimitExponent = -3,
        LowLimit = 0.5f,
        HighLimit = 2.0f,
        Units = units,
        ResultFormatString = "%7.3f",
        LowLimitFormatString = "%7.3f",
        HighLimitFormatString = "%7.3f",
        LowSpecLimit = 0.0f,
        HighSpecLimit = 3.0f,
    };

    private static Ftr MakeFtr(uint testNumber) => new()
    {
        TestNumber = testNumber,
        HeadNumber = 1,
        SiteNumber = 1,
        VectorName = "scan_test",
        TimeSet = "ts1",
        OpCode = "nop",
        TestText = "Func Test",
        AlarmId = "FA01",
        ProgramText = "prog1",
        ResultText = "pass",
        PatternGeneratorNumber = 1,
    };

    private static Mpr MakeMpr(uint testNumber) => new()
    {
        TestNumber = testNumber,
        HeadNumber = 1,
        SiteNumber = 1,
        TestText = "MultiPin",
        AlarmId = "MP01",
        OptionalFlags = 0x00,
        ResultExponent = -3,
        LowLimitExponent = -3,
        HighLimitExponent = -3,
        LowLimit = 0.1f,
        HighLimit = 5.0f,
        StartingCondition = 0.0f,
        ConditionIncrement = 0.1f,
        Units = "V",
        UnitsInput = "mA",
        ResultFormatString = "%7.3f",
        LowLimitFormatString = "%7.3f",
        HighLimitFormatString = "%7.3f",
        LowSpecLimit = 0.0f,
        HighSpecLimit = 6.0f,
    };

    private static async IAsyncEnumerable<StdfRecord> ToAsync(params StdfRecord[] records)
    {
        foreach (var r in records) { await Task.Yield(); yield return r; }
    }
}
