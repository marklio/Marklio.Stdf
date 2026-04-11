using System.Buffers;
using Marklio.Stdf;
using Marklio.Stdf.Records;

namespace Marklio.Stdf.Tests;

public class MirTests
{
    [Fact]
    public void Mir_Deserialize_RequiredFields()
    {
        // SetupTime(4) + StartTime(4) + StationNumber(1) + ModeCode(1) + RetestCode(1) + ProtectionCode(1) = 12 bytes
        var writer = new ArrayBufferWriter<byte>();
        // SetupTime = 2024-06-15 12:00:00 UTC = 1718452800
        var setupBytes = new byte[4];
        BinaryPrimitives_WriteUInt32LE(setupBytes, 1718452800);
        var startBytes = new byte[4];
        BinaryPrimitives_WriteUInt32LE(startBytes, 1718452800);

        byte[] data = [
            .. setupBytes,           // SetupTime U*4
            .. startBytes,           // StartTime U*4
            0x01,                    // StationNumber U*1
            (byte)'P',              // ModeCode C*1
            (byte)'N',              // RetestCode C*1
            (byte)'0',              // ProtectionCode C*1
        ];

        var seq = new ReadOnlySequence<byte>(data);
        var reader = new SequenceReader<byte>(seq);

        var mir = Mir.Deserialize(ref reader, Endianness.LittleEndian);

        Assert.Equal(1, mir.StationNumber);
        Assert.Equal('P', mir.ModeCode);
        Assert.Equal('N', mir.RetestCode);
        Assert.Equal('0', mir.ProtectionCode);
        Assert.Null(mir.BurnInTime);
        Assert.Null(mir.LotId);
    }

    [Fact]
    public void Mir_RoundTrip_WithOptionalFields()
    {
        // Build a MIR with some optional fields
        byte[] data = [
            0x00, 0x9C, 0x6B, 0x66, // SetupTime
            0x00, 0x9C, 0x6B, 0x66, // StartTime
            0x01,                    // StationNumber
            (byte)'P',              // ModeCode
            (byte)'N',              // RetestCode
            (byte)'0',              // ProtectionCode
            0x3C, 0x00,             // BurnInTime = 60 (U*2)
            (byte)'A',              // CommandModeCode C*1
            0x05, (byte)'L', (byte)'O', (byte)'T', (byte)'0', (byte)'1',  // LotId C*n "LOT01"
        ];

        var seq = new ReadOnlySequence<byte>(data);
        var reader = new SequenceReader<byte>(seq);
        var mir = Mir.Deserialize(ref reader, Endianness.LittleEndian);

        Assert.Equal((ushort)60, mir.BurnInTime);
        Assert.Equal('A', mir.CommandModeCode);
        Assert.Equal("LOT01", mir.LotId);

        // Round-trip
        var output = new ArrayBufferWriter<byte>();
        mir.Serialize(output, Endianness.LittleEndian);
        Assert.Equal(data, output.WrittenSpan.ToArray());
    }

    [Fact]
    public void Mir_NewRecord_WritesAllSetFields()
    {
        var mir = new Mir
        {
            SetupTime = new DateTime(2024, 6, 15, 12, 0, 0, DateTimeKind.Utc),
            StartTime = new DateTime(2024, 6, 15, 12, 0, 0, DateTimeKind.Utc),
            StationNumber = 1,
            ModeCode = 'P',
            RetestCode = 'N',
            ProtectionCode = ' ',
        };

        var output = new ArrayBufferWriter<byte>();
        mir.Serialize(output, Endianness.LittleEndian);

        // Should write all fields including trailing optionals (since all presence bits are set)
        Assert.True(output.WrittenCount >= 12);
    }

    private static void BinaryPrimitives_WriteUInt32LE(Span<byte> dest, uint value)
    {
        dest[0] = (byte)value;
        dest[1] = (byte)(value >> 8);
        dest[2] = (byte)(value >> 16);
        dest[3] = (byte)(value >> 24);
    }
}
