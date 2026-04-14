using Marklio.Stdf;
using Marklio.Stdf.Attributes;
using Marklio.Stdf.IO;

namespace Marklio.Stdf.Tests;

public class FoundationTests
{
    [Fact]
    public void Endianness_HasExpectedValues()
    {
        Assert.Equal((byte)1, (byte)Endianness.BigEndian);
        Assert.Equal((byte)2, (byte)Endianness.LittleEndian);
    }

    [Fact]
    public void UnknownRecord_PreservesRawData()
    {
        byte[] data = [0x01, 0x02, 0x03];
        var record = new UnknownRecord(99, 1) { RawData = data };

        Assert.Equal(99, record.RecordType);
        Assert.Equal(1, record.RecordSubType);
        Assert.Equal(3, record.RawData.Length);
        Assert.True(record.RawData.Span.SequenceEqual(data));
    }

    [Fact]
    public void StdfRecord_SupportsPatternMatching()
    {
        StdfRecord record = new UnknownRecord(50, 10) { RawData = ReadOnlyMemory<byte>.Empty };

        Assert.Equal(50, record.RecordType);
        Assert.Equal(10, record.RecordSubType);
        var typed = Assert.IsType<UnknownRecord>(record);
        Assert.Equal(50, typed.RecordType);
    }

    [Fact]
    public void StdfWriterOptions_HasSensibleDefaults()
    {
        var options = new StdfWriterOptions();
        Assert.Equal(Endianness.LittleEndian, options.Endianness);
        Assert.Equal(StdfCompression.None, options.Compression);
    }

    [Theory]
    [InlineData(new byte[] { 0x34, 0x12 }, Endianness.LittleEndian, (ushort)0x1234)]
    [InlineData(new byte[] { 0x12, 0x34 }, Endianness.BigEndian, (ushort)0x1234)]
    public void EndianPrimitives_ReadUInt16(byte[] source, Endianness endianness, ushort expected)
    {
        Assert.Equal(expected, EndianAwarePrimitives.ReadUInt16(source, endianness));
    }

    [Theory]
    [InlineData(new byte[] { 0x78, 0x56, 0x34, 0x12 }, Endianness.LittleEndian, 0x12345678u)]
    [InlineData(new byte[] { 0x12, 0x34, 0x56, 0x78 }, Endianness.BigEndian, 0x12345678u)]
    public void EndianPrimitives_ReadUInt32(byte[] source, Endianness endianness, uint expected)
    {
        Assert.Equal(expected, EndianAwarePrimitives.ReadUInt32(source, endianness));
    }

    [Fact]
    public void EndianPrimitives_WriteUInt16_RoundTrips()
    {
        Span<byte> buffer = stackalloc byte[2];
        EndianAwarePrimitives.WriteUInt16(buffer, 0xABCD, Endianness.LittleEndian);
        Assert.Equal(0xABCD, EndianAwarePrimitives.ReadUInt16(buffer, Endianness.LittleEndian));

        EndianAwarePrimitives.WriteUInt16(buffer, 0xABCD, Endianness.BigEndian);
        Assert.Equal(0xABCD, EndianAwarePrimitives.ReadUInt16(buffer, Endianness.BigEndian));
    }

    [Fact]
    public void EndianPrimitives_DateTime_RoundTrips()
    {
        var dt = new DateTime(2024, 6, 15, 12, 0, 0, DateTimeKind.Utc);
        Span<byte> buffer = stackalloc byte[4];

        EndianAwarePrimitives.WriteDateTime(buffer, dt, Endianness.LittleEndian);
        var result = EndianAwarePrimitives.ReadDateTime(buffer, Endianness.LittleEndian);

        Assert.Equal(dt, result);
    }

    [Fact]
    public void EndianPrimitives_DateTime_DefaultIsZero()
    {
        Span<byte> buffer = stackalloc byte[4];
        EndianAwarePrimitives.WriteDateTime(buffer, default, Endianness.LittleEndian);
        Assert.True(buffer.SequenceEqual(stackalloc byte[4]));
        Assert.Equal(default, EndianAwarePrimitives.ReadDateTime(buffer, Endianness.LittleEndian));
    }

    [Fact]
    public void Attributes_CanBeInstantiated()
    {
        var stdfRecord = new StdfRecordAttribute(1, 10);
        Assert.Equal(1, stdfRecord.RecordType);
        Assert.Equal(10, stdfRecord.RecordSubType);

        var wireCount = new WireCountAttribute("grp");
        Assert.Equal("grp", wireCount.GroupName);

        var countedArray = new CountedArrayAttribute("grp");
        Assert.Equal("grp", countedArray.GroupName);

        var fixedStr = new FixedStringAttribute(8);
        Assert.Equal(8, fixedStr.Length);

        // Marker attributes (no properties to test, just instantiation)
        _ = new StdfDateTimeAttribute();
        _ = new BitFieldAttribute();
        _ = new BitArrayAttribute();
        _ = new NibbleAttribute();
        _ = new C1Attribute();
        _ = new BitEncodedAttribute();
    }
}
