using Marklio.Stdf.IO;
using Marklio.Stdf.Types;

namespace Marklio.Stdf.Tests;

public class StdfDateTimeTests
{
    [Fact]
    public void DefaultDateTime_SerializesToZero()
    {
        Assert.Equal(0u, StdfDateTime.ToStdf(default));
    }

    [Fact]
    public void Zero_DeserializesToDefault()
    {
        Assert.Equal(default, StdfDateTime.FromStdf(0));
    }

    [Fact]
    public void DefaultDateTime_RoundTrips()
    {
        uint wire = StdfDateTime.ToStdf(default);
        DateTime result = StdfDateTime.FromStdf(wire);
        Assert.Equal(default(DateTime), result);
    }

    [Fact]
    public void ValidDate_RoundTrips()
    {
        var dt = new DateTime(2024, 6, 15, 12, 0, 0, DateTimeKind.Utc);
        uint wire = StdfDateTime.ToStdf(dt);
        DateTime result = StdfDateTime.FromStdf(wire);
        Assert.Equal(dt, result);
    }

    [Fact]
    public void UnixEpochPlusOne_RoundTrips()
    {
        var dt = new DateTime(1970, 1, 1, 0, 0, 1, DateTimeKind.Utc);
        uint wire = StdfDateTime.ToStdf(dt);
        Assert.Equal(1u, wire);
        Assert.Equal(dt, StdfDateTime.FromStdf(wire));
    }

    [Fact]
    public void MaxValidDate_RoundTrips()
    {
        DateTime maxDt = StdfDateTime.MaxValue;
        uint wire = StdfDateTime.ToStdf(maxDt);
        Assert.Equal(uint.MaxValue, wire);
        Assert.Equal(maxDt, StdfDateTime.FromStdf(wire));
    }

    [Fact]
    public void Pre1970_Throws()
    {
        var dt = new DateTime(1969, 12, 31, 23, 59, 59, DateTimeKind.Utc);
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => StdfDateTime.ToStdf(dt));
        Assert.Contains("1970", ex.Message);
    }

    [Fact]
    public void FarFuture_Throws()
    {
        var dt = new DateTime(2200, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => StdfDateTime.ToStdf(dt));
        Assert.Contains("2106", ex.Message);
    }

    [Fact]
    public void DateTimeMinValue_MapsToZero()
    {
        // default(DateTime) == DateTime.MinValue, so this should map to 0.
        Assert.Equal(0u, StdfDateTime.ToStdf(DateTime.MinValue));
    }

    [Fact]
    public void EndianPrimitives_WriteDateTime_Pre1970_Throws()
    {
        var dt = new DateTime(1969, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var buffer = new byte[4];
        Assert.Throws<ArgumentOutOfRangeException>(
            () => EndianAwarePrimitives.WriteDateTime(buffer, dt, Endianness.LittleEndian));
    }

    [Fact]
    public void EndianPrimitives_WriteDateTime_FarFuture_Throws()
    {
        var dt = new DateTime(2200, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var buffer = new byte[4];
        Assert.Throws<ArgumentOutOfRangeException>(
            () => EndianAwarePrimitives.WriteDateTime(buffer, dt, Endianness.LittleEndian));
    }
}
