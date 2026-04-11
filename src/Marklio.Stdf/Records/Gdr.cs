using System.Buffers;
using System.Buffers.Binary;
using System.Text;

namespace Marklio.Stdf.Records;

/// <summary>
/// GDR — Generic Data Record (50, 10).
/// Contains a variable-length list of typed fields using the STDF V*n encoding.
/// Each field is preceded by a type byte indicating the data type.
/// </summary>
public readonly record struct Gdr : IStdfRecord
{
    static byte IStdfRecord.RecordType => 50;
    static byte IStdfRecord.RecordSubType => 10;

    /// <summary>The generic data fields in this record.</summary>
    public GdrField[] Fields { get; init; }

    public Gdr()
    {
        Fields = [];
    }

    public static Gdr Deserialize(ref SequenceReader<byte> reader, Endianness endianness)
    {
        if (reader.Remaining < 2)
            return new Gdr();

        Span<byte> countBuf = stackalloc byte[2];
        reader.TryCopyTo(countBuf);
        reader.Advance(2);
        ushort fieldCount = endianness == Endianness.LittleEndian
            ? BinaryPrimitives.ReadUInt16LittleEndian(countBuf)
            : BinaryPrimitives.ReadUInt16BigEndian(countBuf);

        var fields = new GdrField[fieldCount];
        for (int i = 0; i < fieldCount; i++)
        {
            if (!reader.TryRead(out byte typeByte))
                break;

            fields[i] = typeByte switch
            {
                0 => new GdrField { Type = GdrFieldType.Padding },
                1 => ReadU1Field(ref reader),
                2 => ReadU2Field(ref reader, endianness),
                3 => ReadU4Field(ref reader, endianness),
                4 => ReadI1Field(ref reader),
                5 => ReadI2Field(ref reader, endianness),
                6 => ReadI4Field(ref reader, endianness),
                7 => ReadR4Field(ref reader, endianness),
                8 => ReadR8Field(ref reader, endianness),
                10 => ReadCnField(ref reader),
                11 => ReadBnField(ref reader),
                12 => ReadDnField(ref reader, endianness),
                13 => ReadN1Field(ref reader),
                _ => new GdrField { Type = GdrFieldType.Padding },
            };
        }

        return new Gdr { Fields = fields };
    }

    public void Serialize(IBufferWriter<byte> writer, Endianness endianness)
    {
        var span = writer.GetSpan(2);
        if (endianness == Endianness.LittleEndian)
            BinaryPrimitives.WriteUInt16LittleEndian(span, (ushort)Fields.Length);
        else
            BinaryPrimitives.WriteUInt16BigEndian(span, (ushort)Fields.Length);
        writer.Advance(2);

        foreach (var field in Fields)
        {
            var ts = writer.GetSpan(1);
            ts[0] = (byte)field.Type;
            writer.Advance(1);

            switch (field.Type)
            {
                case GdrFieldType.Padding:
                    break;
                case GdrFieldType.U1:
                    var u1s = writer.GetSpan(1); u1s[0] = (byte)field.Value!; writer.Advance(1);
                    break;
                case GdrFieldType.U2:
                    var u2s = writer.GetSpan(2);
                    if (endianness == Endianness.LittleEndian) BinaryPrimitives.WriteUInt16LittleEndian(u2s, (ushort)field.Value!);
                    else BinaryPrimitives.WriteUInt16BigEndian(u2s, (ushort)field.Value!);
                    writer.Advance(2);
                    break;
                case GdrFieldType.U4:
                    var u4s = writer.GetSpan(4);
                    if (endianness == Endianness.LittleEndian) BinaryPrimitives.WriteUInt32LittleEndian(u4s, (uint)field.Value!);
                    else BinaryPrimitives.WriteUInt32BigEndian(u4s, (uint)field.Value!);
                    writer.Advance(4);
                    break;
                case GdrFieldType.I1:
                    var i1s = writer.GetSpan(1); i1s[0] = (byte)(sbyte)field.Value!; writer.Advance(1);
                    break;
                case GdrFieldType.I2:
                    var i2s = writer.GetSpan(2);
                    if (endianness == Endianness.LittleEndian) BinaryPrimitives.WriteInt16LittleEndian(i2s, (short)field.Value!);
                    else BinaryPrimitives.WriteInt16BigEndian(i2s, (short)field.Value!);
                    writer.Advance(2);
                    break;
                case GdrFieldType.I4:
                    var i4s = writer.GetSpan(4);
                    if (endianness == Endianness.LittleEndian) BinaryPrimitives.WriteInt32LittleEndian(i4s, (int)field.Value!);
                    else BinaryPrimitives.WriteInt32BigEndian(i4s, (int)field.Value!);
                    writer.Advance(4);
                    break;
                case GdrFieldType.R4:
                    var r4s = writer.GetSpan(4);
                    if (endianness == Endianness.LittleEndian) BinaryPrimitives.WriteSingleLittleEndian(r4s, (float)field.Value!);
                    else BinaryPrimitives.WriteSingleBigEndian(r4s, (float)field.Value!);
                    writer.Advance(4);
                    break;
                case GdrFieldType.R8:
                    var r8s = writer.GetSpan(8);
                    if (endianness == Endianness.LittleEndian) BinaryPrimitives.WriteDoubleLittleEndian(r8s, (double)field.Value!);
                    else BinaryPrimitives.WriteDoubleBigEndian(r8s, (double)field.Value!);
                    writer.Advance(8);
                    break;
                case GdrFieldType.Cn:
                    var str = (string)field.Value!;
                    byte sLen = (byte)str.Length;
                    var cs = writer.GetSpan(1 + sLen); cs[0] = sLen;
                    if (sLen > 0) Encoding.ASCII.GetBytes(str, cs.Slice(1, sLen));
                    writer.Advance(1 + sLen);
                    break;
                case GdrFieldType.Bn:
                    var bArr = (byte[])field.Value!;
                    byte bLen = (byte)bArr.Length;
                    var bs = writer.GetSpan(1 + bLen); bs[0] = bLen;
                    if (bLen > 0) bArr.AsSpan().CopyTo(bs.Slice(1));
                    writer.Advance(1 + bLen);
                    break;
                case GdrFieldType.Dn:
                    var bits = (System.Collections.BitArray)field.Value!;
                    ushort bitCount = (ushort)bits.Length;
                    int byteCount = (bitCount + 7) / 8;
                    var ds = writer.GetSpan(2 + byteCount);
                    if (endianness == Endianness.LittleEndian) BinaryPrimitives.WriteUInt16LittleEndian(ds, bitCount);
                    else BinaryPrimitives.WriteUInt16BigEndian(ds, bitCount);
                    if (byteCount > 0) { var bytes = new byte[byteCount]; bits.CopyTo(bytes, 0); bytes.AsSpan().CopyTo(ds.Slice(2)); }
                    writer.Advance(2 + byteCount);
                    break;
                case GdrFieldType.N1:
                    var ns = writer.GetSpan(1); ns[0] = (byte)((byte)field.Value! & 0x0F); writer.Advance(1);
                    break;
            }
        }
    }

    // --- Deserialization helpers ---
    private static GdrField ReadU1Field(ref SequenceReader<byte> r)
    { r.TryRead(out byte v); return new GdrField { Type = GdrFieldType.U1, Value = v }; }
    private static GdrField ReadU2Field(ref SequenceReader<byte> r, Endianness e)
    { Span<byte> b = stackalloc byte[2]; r.TryCopyTo(b); r.Advance(2); return new GdrField { Type = GdrFieldType.U2, Value = e == Endianness.LittleEndian ? BinaryPrimitives.ReadUInt16LittleEndian(b) : BinaryPrimitives.ReadUInt16BigEndian(b) }; }
    private static GdrField ReadU4Field(ref SequenceReader<byte> r, Endianness e)
    { Span<byte> b = stackalloc byte[4]; r.TryCopyTo(b); r.Advance(4); return new GdrField { Type = GdrFieldType.U4, Value = e == Endianness.LittleEndian ? BinaryPrimitives.ReadUInt32LittleEndian(b) : BinaryPrimitives.ReadUInt32BigEndian(b) }; }
    private static GdrField ReadI1Field(ref SequenceReader<byte> r)
    { r.TryRead(out byte v); return new GdrField { Type = GdrFieldType.I1, Value = (sbyte)v }; }
    private static GdrField ReadI2Field(ref SequenceReader<byte> r, Endianness e)
    { Span<byte> b = stackalloc byte[2]; r.TryCopyTo(b); r.Advance(2); return new GdrField { Type = GdrFieldType.I2, Value = e == Endianness.LittleEndian ? BinaryPrimitives.ReadInt16LittleEndian(b) : BinaryPrimitives.ReadInt16BigEndian(b) }; }
    private static GdrField ReadI4Field(ref SequenceReader<byte> r, Endianness e)
    { Span<byte> b = stackalloc byte[4]; r.TryCopyTo(b); r.Advance(4); return new GdrField { Type = GdrFieldType.I4, Value = e == Endianness.LittleEndian ? BinaryPrimitives.ReadInt32LittleEndian(b) : BinaryPrimitives.ReadInt32BigEndian(b) }; }
    private static GdrField ReadR4Field(ref SequenceReader<byte> r, Endianness e)
    { Span<byte> b = stackalloc byte[4]; r.TryCopyTo(b); r.Advance(4); return new GdrField { Type = GdrFieldType.R4, Value = e == Endianness.LittleEndian ? BinaryPrimitives.ReadSingleLittleEndian(b) : BinaryPrimitives.ReadSingleBigEndian(b) }; }
    private static GdrField ReadR8Field(ref SequenceReader<byte> r, Endianness e)
    { Span<byte> b = stackalloc byte[8]; r.TryCopyTo(b); r.Advance(8); return new GdrField { Type = GdrFieldType.R8, Value = e == Endianness.LittleEndian ? BinaryPrimitives.ReadDoubleLittleEndian(b) : BinaryPrimitives.ReadDoubleBigEndian(b) }; }
    private static GdrField ReadCnField(ref SequenceReader<byte> r)
    { r.TryRead(out byte len); if (len == 0) return new GdrField { Type = GdrFieldType.Cn, Value = string.Empty }; Span<byte> b = stackalloc byte[len]; r.TryCopyTo(b); r.Advance(len); return new GdrField { Type = GdrFieldType.Cn, Value = Encoding.ASCII.GetString(b) }; }
    private static GdrField ReadBnField(ref SequenceReader<byte> r)
    { r.TryRead(out byte len); var b = new byte[len]; r.TryCopyTo(b); r.Advance(len); return new GdrField { Type = GdrFieldType.Bn, Value = b }; }
    private static GdrField ReadDnField(ref SequenceReader<byte> r, Endianness e)
    {
        Span<byte> lb = stackalloc byte[2]; r.TryCopyTo(lb); r.Advance(2);
        ushort bitCount = e == Endianness.LittleEndian ? BinaryPrimitives.ReadUInt16LittleEndian(lb) : BinaryPrimitives.ReadUInt16BigEndian(lb);
        int byteCount = (bitCount + 7) / 8;
        var buf = new byte[byteCount]; r.TryCopyTo(buf); r.Advance(byteCount);
        return new GdrField { Type = GdrFieldType.Dn, Value = new System.Collections.BitArray(buf) { Length = bitCount } };
    }
    private static GdrField ReadN1Field(ref SequenceReader<byte> r)
    { r.TryRead(out byte v); return new GdrField { Type = GdrFieldType.N1, Value = (byte)(v & 0x0F) }; }
}

/// <summary>Type code for GDR V*n fields.</summary>
public enum GdrFieldType : byte
{
    Padding = 0, U1 = 1, U2 = 2, U4 = 3, I1 = 4, I2 = 5, I4 = 6,
    R4 = 7, R8 = 8, Cn = 10, Bn = 11, Dn = 12, N1 = 13,
}

/// <summary>A single typed field within a GDR record.</summary>
public readonly record struct GdrField
{
    public GdrFieldType Type { get; init; }
    public object? Value { get; init; }
}
