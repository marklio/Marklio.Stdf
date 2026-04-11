using System.Buffers;
using Marklio.Stdf;
using Marklio.Stdf.Records;

namespace Marklio.Stdf.Tests;

public class PtrTests
{
    [Fact]
    public void Ptr_Deserialize_BasicFields()
    {
        byte[] data = [
            0x01, 0x00, 0x00, 0x00, // TestNumber U*4 = 1
            0x01,                    // HeadNumber U*1
            0x01,                    // SiteNumber U*1
            0x00,                    // TestFlags B*1
            0x00,                    // ParametricFlags B*1
            0x00, 0x00, 0x80, 0x3F, // Result R*4 = 1.0f (LE)
        ];

        var seq = new ReadOnlySequence<byte>(data);
        var reader = new SequenceReader<byte>(seq);

        var ptr = Ptr.Deserialize(ref reader, Endianness.LittleEndian);

        Assert.Equal(1u, ptr.TestNumber);
        Assert.Equal(1, ptr.HeadNumber);
        Assert.Equal(1, ptr.SiteNumber);
        Assert.Equal(0, ptr.TestFlags);
        Assert.Equal(0, ptr.ParametricFlags);
        Assert.Equal(1.0f, ptr.Result);
    }

    [Fact]
    public void Ptr_RoundTrip_ByteExact()
    {
        byte[] data = [
            0x01, 0x00, 0x00, 0x00, // TestNumber
            0x01,                    // HeadNumber
            0x02,                    // SiteNumber
            0x40,                    // TestFlags
            0x00,                    // ParametricFlags
            0xCD, 0xCC, 0x8C, 0x3F, // Result = 1.1f
            0x06, (byte)'T', (byte)'e', (byte)'s', (byte)'t', (byte)'_', (byte)'1', // TestText "Test_1"
        ];

        var seq = new ReadOnlySequence<byte>(data);
        var reader = new SequenceReader<byte>(seq);
        var ptr = Ptr.Deserialize(ref reader, Endianness.LittleEndian);

        Assert.Equal(1u, ptr.TestNumber);
        Assert.Equal("Test_1", ptr.TestText);

        var output = new ArrayBufferWriter<byte>();
        ptr.Serialize(output, Endianness.LittleEndian);
        Assert.Equal(data, output.WrittenSpan.ToArray());
    }
}
