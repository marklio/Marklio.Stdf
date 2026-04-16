using Marklio.Stdf;
using Marklio.Stdf.Records;

namespace Marklio.Stdf.Tests;

public class TestRecordExpanderTests
{
    [Fact]
    public void FirstPtr_PassesThroughUnchanged()
    {
        var ptr = MakeFullPtr(testNumber: 1);
        var result = new StdfRecord[] { ptr }.AsEnumerable().ExpandTestRecords().ToList();

        Assert.Single(result);
        var p = Assert.IsType<Ptr>(result[0]);
        Assert.Equal("Vcc", p.TestText);
        Assert.Equal("V", p.Units);
    }

    [Fact]
    public void Expand_FillsNullFields_FromFirstOccurrence()
    {
        var ptr1 = MakeFullPtr(testNumber: 1);
        var ptr2 = new Ptr
        {
            TestNumber = 1,
            HeadNumber = 1,
            SiteNumber = 1,
            Result = 1.8f,
            // All static fields are null
        };

        var result = new StdfRecord[] { ptr1, ptr2 }.AsEnumerable().ExpandTestRecords().ToList();

        var expanded = Assert.IsType<Ptr>(result[1]);
        Assert.Equal(1.8f, expanded.Result);
        Assert.Equal("Vcc", expanded.TestText);
        Assert.Equal("AL01", expanded.AlarmId);
        Assert.Equal((byte)0x00, expanded.OptionalFlags);
        Assert.Equal((sbyte)-3, expanded.ResultExponent);
        Assert.Equal((sbyte)-3, expanded.LowLimitExponent);
        Assert.Equal((sbyte)-3, expanded.HighLimitExponent);
        Assert.Equal(0.5f, expanded.LowLimit);
        Assert.Equal(2.0f, expanded.HighLimit);
        Assert.Equal("V", expanded.Units);
        Assert.Equal("%7.3f", expanded.ResultFormatString);
        Assert.Equal("%7.3f", expanded.LowLimitFormatString);
        Assert.Equal("%7.3f", expanded.HighLimitFormatString);
        Assert.Equal(0.0f, expanded.LowSpecLimit);
        Assert.Equal(3.0f, expanded.HighSpecLimit);
    }

    [Fact]
    public void Expand_DoesNotOverwrite_NonNullValues()
    {
        var ptr1 = MakeFullPtr(testNumber: 1);
        var ptr2 = new Ptr
        {
            TestNumber = 1,
            HeadNumber = 1,
            SiteNumber = 1,
            Result = 1.8f,
            TestText = "OverriddenText",
            Units = "mV",
        };

        var result = new StdfRecord[] { ptr1, ptr2 }.AsEnumerable().ExpandTestRecords().ToList();

        var expanded = Assert.IsType<Ptr>(result[1]);
        Assert.Equal("OverriddenText", expanded.TestText);
        Assert.Equal("mV", expanded.Units);
        // Other null fields filled from template
        Assert.Equal("AL01", expanded.AlarmId);
        Assert.Equal(0.5f, expanded.LowLimit);
    }

    [Fact]
    public void DifferentTestNumbers_AreIndependent()
    {
        var ptr1 = MakeFullPtr(testNumber: 1);
        var ptr2 = MakeFullPtr(testNumber: 2, testText: "Icc", units: "A");
        var ptr3 = new Ptr { TestNumber = 1, HeadNumber = 1, SiteNumber = 1, Result = 0.9f };
        var ptr4 = new Ptr { TestNumber = 2, HeadNumber = 1, SiteNumber = 1, Result = 0.3f };

        var result = new StdfRecord[] { ptr1, ptr2, ptr3, ptr4 }
            .AsEnumerable().ExpandTestRecords().ToList();

        var expanded3 = Assert.IsType<Ptr>(result[2]);
        Assert.Equal("Vcc", expanded3.TestText);
        Assert.Equal("V", expanded3.Units);

        var expanded4 = Assert.IsType<Ptr>(result[3]);
        Assert.Equal("Icc", expanded4.TestText);
        Assert.Equal("A", expanded4.Units);
    }

    [Fact]
    public void Ftr_ExpandsCorrectly()
    {
        var ftr1 = MakeFullFtr(testNumber: 10);
        var ftr2 = new Ftr { TestNumber = 10, HeadNumber = 1, SiteNumber = 1 };

        var result = new StdfRecord[] { ftr1, ftr2 }.AsEnumerable().ExpandTestRecords().ToList();

        var expanded = Assert.IsType<Ftr>(result[1]);
        Assert.Equal("scan_test", expanded.VectorName);
        Assert.Equal("ts1", expanded.TimeSet);
        Assert.Equal("nop", expanded.OpCode);
        Assert.Equal("Func Test", expanded.TestText);
        Assert.Equal("FA01", expanded.AlarmId);
        Assert.Equal("prog1", expanded.ProgramText);
        Assert.Equal("pass", expanded.ResultText);
        Assert.Equal((byte)1, expanded.PatternGeneratorNumber);
    }

    [Fact]
    public void Mpr_ExpandsCorrectly()
    {
        var mpr1 = MakeFullMpr(testNumber: 20);
        var mpr2 = new Mpr { TestNumber = 20, HeadNumber = 1, SiteNumber = 1 };

        var result = new StdfRecord[] { mpr1, mpr2 }.AsEnumerable().ExpandTestRecords().ToList();

        var expanded = Assert.IsType<Mpr>(result[1]);
        Assert.Equal("MultiPin", expanded.TestText);
        Assert.Equal("MP01", expanded.AlarmId);
        Assert.Equal((byte)0x00, expanded.OptionalFlags);
        Assert.Equal(0.1f, expanded.LowLimit);
        Assert.Equal(5.0f, expanded.HighLimit);
        Assert.Equal(0.0f, expanded.StartingCondition);
        Assert.Equal(0.1f, expanded.ConditionIncrement);
        Assert.Equal("V", expanded.Units);
        Assert.Equal("mA", expanded.UnitsInput);
    }

    [Fact]
    public void NonTestRecords_PassThrough()
    {
        var far = new Far { CpuType = 2, StdfVersion = 4 };
        var pir = new Pir { HeadNumber = 1, SiteNumber = 1 };

        var result = new StdfRecord[] { far, pir }.AsEnumerable().ExpandTestRecords().ToList();

        Assert.Equal(2, result.Count);
        Assert.IsType<Far>(result[0]);
        Assert.IsType<Pir>(result[1]);
    }

    [Fact]
    public void CompactThenExpand_RoundTrips()
    {
        var ptr1 = MakeFullPtr(testNumber: 1);
        var ptr2 = MakeFullPtr(testNumber: 1, result: 1.8f);
        var ptr3 = MakeFullPtr(testNumber: 2, testText: "Icc", units: "A");
        var ftr1 = MakeFullFtr(testNumber: 10);
        var ftr2 = MakeFullFtr(testNumber: 10);
        var mpr1 = MakeFullMpr(testNumber: 20);
        var mpr2 = MakeFullMpr(testNumber: 20);

        var original = new StdfRecord[] { ptr1, ptr2, ptr3, ftr1, ftr2, mpr1, mpr2 };

        var roundTripped = original.AsEnumerable()
            .CompactTestRecords()
            .ExpandTestRecords()
            .ToList();

        Assert.Equal(original.Length, roundTripped.Count);

        // Verify PTR static fields restored
        var rPtr1 = Assert.IsType<Ptr>(roundTripped[0]);
        Assert.Equal("Vcc", rPtr1.TestText);
        Assert.Equal("V", rPtr1.Units);

        var rPtr2 = Assert.IsType<Ptr>(roundTripped[1]);
        Assert.Equal("Vcc", rPtr2.TestText);
        Assert.Equal("V", rPtr2.Units);
        Assert.Equal(1.8f, rPtr2.Result);

        var rPtr3 = Assert.IsType<Ptr>(roundTripped[2]);
        Assert.Equal("Icc", rPtr3.TestText);
        Assert.Equal("A", rPtr3.Units);

        // Verify FTR static fields restored
        var rFtr2 = Assert.IsType<Ftr>(roundTripped[4]);
        Assert.Equal("scan_test", rFtr2.VectorName);
        Assert.Equal("ts1", rFtr2.TimeSet);

        // Verify MPR static fields restored
        var rMpr2 = Assert.IsType<Mpr>(roundTripped[6]);
        Assert.Equal("MultiPin", rMpr2.TestText);
        Assert.Equal("V", rMpr2.Units);
    }

    [Fact]
    public async Task AsyncVersion_Works()
    {
        var ptr1 = MakeFullPtr(testNumber: 1);
        var ptr2 = new Ptr { TestNumber = 1, HeadNumber = 1, SiteNumber = 1, Result = 1.8f };

        var result = new List<StdfRecord>();
        await foreach (var rec in ToAsync(ptr1, ptr2).ExpandTestRecords())
            result.Add(rec);

        Assert.Equal(2, result.Count);
        var expanded = Assert.IsType<Ptr>(result[1]);
        Assert.Equal("Vcc", expanded.TestText);
        Assert.Equal("V", expanded.Units);
        Assert.Equal(1.8f, expanded.Result);
    }

    private static Ptr MakeFullPtr(
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

    private static Ftr MakeFullFtr(uint testNumber) => new()
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

    private static Mpr MakeFullMpr(uint testNumber) => new()
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
