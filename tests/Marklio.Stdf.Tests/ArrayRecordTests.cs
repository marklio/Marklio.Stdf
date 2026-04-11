using System.Buffers;
using Marklio.Stdf;
using Marklio.Stdf.Records;

namespace Marklio.Stdf.Tests;

public class ArrayRecordTests
{
    [Fact]
    public void Pgr_RoundTrip_WithArrays()
    {
        byte[] data = [
            0x05, 0x00,             // GroupIndex U*2 = 5
            0x04, (byte)'G', (byte)'r', (byte)'p', (byte)'1', // GroupName C*n = "Grp1"
            0x03, 0x00,             // PinCount U*2 = 3
            0x01, 0x00, 0x02, 0x00, 0x03, 0x00, // PinIndexes [1, 2, 3]
        ];

        var seq = new ReadOnlySequence<byte>(data);
        var reader = new SequenceReader<byte>(seq);
        var pgr = Pgr.Deserialize(ref reader, Endianness.LittleEndian);

        Assert.Equal(5, pgr.GroupIndex);
        Assert.Equal("Grp1", pgr.GroupName);
        Assert.NotNull(pgr.PinIndexes);
        Assert.Equal(3, pgr.PinIndexes!.Length);
        Assert.Equal([1, 2, 3], pgr.PinIndexes);

        // Round-trip
        var output = new ArrayBufferWriter<byte>();
        pgr.Serialize(output, Endianness.LittleEndian);
        Assert.Equal(data, output.WrittenSpan.ToArray());
    }

    [Fact]
    public void Rdr_RoundTrip_WithBins()
    {
        byte[] data = [
            0x03, 0x00,             // RetestBinCount U*2 = 3
            0x0A, 0x00, 0x14, 0x00, 0x1E, 0x00, // RetestBins [10, 20, 30]
        ];

        var seq = new ReadOnlySequence<byte>(data);
        var reader = new SequenceReader<byte>(seq);
        var rdr = Rdr.Deserialize(ref reader, Endianness.LittleEndian);

        Assert.NotNull(rdr.RetestBins);
        Assert.Equal([10, 20, 30], rdr.RetestBins);

        // Round-trip
        var output = new ArrayBufferWriter<byte>();
        rdr.Serialize(output, Endianness.LittleEndian);
        Assert.Equal(data, output.WrittenSpan.ToArray());
    }

    [Fact]
    public void Rdr_EmptyArray_RoundTrips()
    {
        byte[] data = [
            0x00, 0x00, // RetestBinCount = 0
        ];

        var seq = new ReadOnlySequence<byte>(data);
        var reader = new SequenceReader<byte>(seq);
        var rdr = Rdr.Deserialize(ref reader, Endianness.LittleEndian);

        // With count = 0, array should be null (count > 0 guard)
        Assert.Null(rdr.RetestBins);

        var output = new ArrayBufferWriter<byte>();
        rdr.Serialize(output, Endianness.LittleEndian);
        Assert.Equal(data, output.WrittenSpan.ToArray());
    }

    [Fact]
    public void Plr_RoundTrip_SharedCount()
    {
        byte[] data = [
            0x02, 0x00,                           // GroupCount = 2
            0x01, 0x00, 0x02, 0x00,               // GroupIndexes [1, 2]
            0x03, 0x00, 0x04, 0x00,               // GroupModes [3, 4]
            0x05, 0x06,                            // GroupRadixes [5, 6]
            0x02, (byte)'A', (byte)'B',            // ProgramChars[0] = "AB"
            0x02, (byte)'C', (byte)'D',            // ProgramChars[1] = "CD"
            0x01, (byte)'E',                       // ReturnChars[0] = "E"
            0x01, (byte)'F',                       // ReturnChars[1] = "F"
            0x01, (byte)'X',                       // ProgramCharsLong[0] = "X"
            0x01, (byte)'Y',                       // ProgramCharsLong[1] = "Y"
            0x01, (byte)'M',                       // ReturnCharsLong[0] = "M"
            0x01, (byte)'N',                       // ReturnCharsLong[1] = "N"
        ];

        var seq = new ReadOnlySequence<byte>(data);
        var reader = new SequenceReader<byte>(seq);
        var plr = Plr.Deserialize(ref reader, Endianness.LittleEndian);

        Assert.Equal([1, 2], plr.GroupIndexes);
        Assert.Equal([3, 4], plr.GroupModes);
        Assert.Equal([5, 6], plr.GroupRadixes);
        Assert.Equal(["AB", "CD"], plr.ProgramChars);
        Assert.Equal(["E", "F"], plr.ReturnChars);
        Assert.Equal(["X", "Y"], plr.ProgramCharsLong);
        Assert.Equal(["M", "N"], plr.ReturnCharsLong);

        // Round-trip
        var output = new ArrayBufferWriter<byte>();
        plr.Serialize(output, Endianness.LittleEndian);
        Assert.Equal(data, output.WrittenSpan.ToArray());
    }

    [Fact]
    public void Eps_RoundTrip_EmptyRecord()
    {
        byte[] data = [];

        var seq = new ReadOnlySequence<byte>(data);
        var reader = new SequenceReader<byte>(seq);
        var eps = Eps.Deserialize(ref reader, Endianness.LittleEndian);

        var output = new ArrayBufferWriter<byte>();
        eps.Serialize(output, Endianness.LittleEndian);
        Assert.Equal(0, output.WrittenCount);
    }
}
