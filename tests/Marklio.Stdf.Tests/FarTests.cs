using System.Buffers;
using Marklio.Stdf;
using Marklio.Stdf.Records;

namespace Marklio.Stdf.Tests;

public class FarTests
{
    [Fact]
    public void Far_Deserialize_ReadsBothFields()
    {
        var data = new ReadOnlySequence<byte>([0x02, 0x04]);
        var reader = new SequenceReader<byte>(data);

        var far = Far.Deserialize(ref reader, Endianness.LittleEndian);

        Assert.Equal(2, far.CpuType);
        Assert.Equal(4, far.StdfVersion);
    }

    [Fact]
    public void Far_Deserialize_EmptyInput_ReturnsDefaults()
    {
        var data = new ReadOnlySequence<byte>([]);
        var reader = new SequenceReader<byte>(data);

        var far = Far.Deserialize(ref reader, Endianness.LittleEndian);

        Assert.Equal(0, far.CpuType);
        Assert.Equal(0, far.StdfVersion);
    }

    [Fact]
    public void Far_Deserialize_PartialInput_ReadsAvailable()
    {
        var data = new ReadOnlySequence<byte>([0x01]);
        var reader = new SequenceReader<byte>(data);

        var far = Far.Deserialize(ref reader, Endianness.LittleEndian);

        Assert.Equal(1, far.CpuType);
        Assert.Equal(0, far.StdfVersion);
    }

    [Fact]
    public void Far_RoundTrip_ByteExact()
    {
        byte[] original = [0x02, 0x04];
        var data = new ReadOnlySequence<byte>(original);
        var reader = new SequenceReader<byte>(data);

        var far = Far.Deserialize(ref reader, Endianness.LittleEndian);

        var output = new ArrayBufferWriter<byte>();
        far.Serialize(output, Endianness.LittleEndian);

        Assert.Equal(original, output.WrittenSpan.ToArray());
    }

    [Fact]
    public void Far_Serialize_NewRecord_WritesAllFields()
    {
        var far = new Far { CpuType = 2, StdfVersion = 4 };

        var output = new ArrayBufferWriter<byte>();
        far.Serialize(output, Endianness.LittleEndian);

        Assert.Equal([0x02, 0x04], output.WrittenSpan.ToArray());
    }

    [Fact]
    public void Far_ImplementsIStdfRecord()
    {
        // Verify the generated IStdfRecord implementation via the static interface
        Assert.Equal(0, GetRecordType<Far>());
        Assert.Equal(10, GetRecordSubType<Far>());
    }

    private static byte GetRecordType<T>() where T : IStdfRecord => T.RecordType;
    private static byte GetRecordSubType<T>() where T : IStdfRecord => T.RecordSubType;
}
