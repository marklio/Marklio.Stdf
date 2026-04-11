using System.Buffers.Binary;

namespace Marklio.Stdf.IO;

/// <summary>
/// Endianness-aware binary read/write primitives using
/// <see cref="BinaryPrimitives"/> for zero-allocation conversions.
/// </summary>
internal static class EndianAwarePrimitives
{
    private static readonly DateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    // --- Read ---

    public static ushort ReadUInt16(ReadOnlySpan<byte> source, Endianness endianness) =>
        endianness == Endianness.LittleEndian
            ? BinaryPrimitives.ReadUInt16LittleEndian(source)
            : BinaryPrimitives.ReadUInt16BigEndian(source);

    public static short ReadInt16(ReadOnlySpan<byte> source, Endianness endianness) =>
        endianness == Endianness.LittleEndian
            ? BinaryPrimitives.ReadInt16LittleEndian(source)
            : BinaryPrimitives.ReadInt16BigEndian(source);

    public static uint ReadUInt32(ReadOnlySpan<byte> source, Endianness endianness) =>
        endianness == Endianness.LittleEndian
            ? BinaryPrimitives.ReadUInt32LittleEndian(source)
            : BinaryPrimitives.ReadUInt32BigEndian(source);

    public static int ReadInt32(ReadOnlySpan<byte> source, Endianness endianness) =>
        endianness == Endianness.LittleEndian
            ? BinaryPrimitives.ReadInt32LittleEndian(source)
            : BinaryPrimitives.ReadInt32BigEndian(source);

    public static float ReadSingle(ReadOnlySpan<byte> source, Endianness endianness) =>
        endianness == Endianness.LittleEndian
            ? BinaryPrimitives.ReadSingleLittleEndian(source)
            : BinaryPrimitives.ReadSingleBigEndian(source);

    public static double ReadDouble(ReadOnlySpan<byte> source, Endianness endianness) =>
        endianness == Endianness.LittleEndian
            ? BinaryPrimitives.ReadDoubleLittleEndian(source)
            : BinaryPrimitives.ReadDoubleBigEndian(source);

    public static DateTime ReadDateTime(ReadOnlySpan<byte> source, Endianness endianness)
    {
        uint seconds = ReadUInt32(source, endianness);
        return seconds == 0 ? default : UnixEpoch.AddSeconds(seconds);
    }

    // --- Write ---

    public static void WriteUInt16(Span<byte> destination, ushort value, Endianness endianness)
    {
        if (endianness == Endianness.LittleEndian)
            BinaryPrimitives.WriteUInt16LittleEndian(destination, value);
        else
            BinaryPrimitives.WriteUInt16BigEndian(destination, value);
    }

    public static void WriteInt16(Span<byte> destination, short value, Endianness endianness)
    {
        if (endianness == Endianness.LittleEndian)
            BinaryPrimitives.WriteInt16LittleEndian(destination, value);
        else
            BinaryPrimitives.WriteInt16BigEndian(destination, value);
    }

    public static void WriteUInt32(Span<byte> destination, uint value, Endianness endianness)
    {
        if (endianness == Endianness.LittleEndian)
            BinaryPrimitives.WriteUInt32LittleEndian(destination, value);
        else
            BinaryPrimitives.WriteUInt32BigEndian(destination, value);
    }

    public static void WriteInt32(Span<byte> destination, int value, Endianness endianness)
    {
        if (endianness == Endianness.LittleEndian)
            BinaryPrimitives.WriteInt32LittleEndian(destination, value);
        else
            BinaryPrimitives.WriteInt32BigEndian(destination, value);
    }

    public static void WriteSingle(Span<byte> destination, float value, Endianness endianness)
    {
        if (endianness == Endianness.LittleEndian)
            BinaryPrimitives.WriteSingleLittleEndian(destination, value);
        else
            BinaryPrimitives.WriteSingleBigEndian(destination, value);
    }

    public static void WriteDouble(Span<byte> destination, double value, Endianness endianness)
    {
        if (endianness == Endianness.LittleEndian)
            BinaryPrimitives.WriteDoubleLittleEndian(destination, value);
        else
            BinaryPrimitives.WriteDoubleBigEndian(destination, value);
    }

    public static void WriteDateTime(Span<byte> destination, DateTime value, Endianness endianness)
    {
        uint seconds = value == default ? 0u : (uint)(value.ToUniversalTime() - UnixEpoch).TotalSeconds;
        WriteUInt32(destination, seconds, endianness);
    }
}
