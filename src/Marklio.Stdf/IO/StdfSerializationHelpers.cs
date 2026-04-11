// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections;
using System.Text;

namespace Marklio.Stdf.IO;

/// <summary>
/// Shared read/write helper methods used by source-generated STDF record types.
/// </summary>
internal static class StdfSerializationHelpers
{
    // --- Read Helpers ---

    internal static ushort ReadU2(ref SequenceReader<byte> r, Endianness e)
    {
        Span<byte> buf = stackalloc byte[2]; r.TryCopyTo(buf); r.Advance(2);
        return e == Endianness.LittleEndian ? BinaryPrimitives.ReadUInt16LittleEndian(buf) : BinaryPrimitives.ReadUInt16BigEndian(buf);
    }

    internal static short ReadI2(ref SequenceReader<byte> r, Endianness e)
    {
        Span<byte> buf = stackalloc byte[2]; r.TryCopyTo(buf); r.Advance(2);
        return e == Endianness.LittleEndian ? BinaryPrimitives.ReadInt16LittleEndian(buf) : BinaryPrimitives.ReadInt16BigEndian(buf);
    }

    internal static uint ReadU4(ref SequenceReader<byte> r, Endianness e)
    {
        Span<byte> buf = stackalloc byte[4]; r.TryCopyTo(buf); r.Advance(4);
        return e == Endianness.LittleEndian ? BinaryPrimitives.ReadUInt32LittleEndian(buf) : BinaryPrimitives.ReadUInt32BigEndian(buf);
    }

    internal static int ReadI4(ref SequenceReader<byte> r, Endianness e)
    {
        Span<byte> buf = stackalloc byte[4]; r.TryCopyTo(buf); r.Advance(4);
        return e == Endianness.LittleEndian ? BinaryPrimitives.ReadInt32LittleEndian(buf) : BinaryPrimitives.ReadInt32BigEndian(buf);
    }

    internal static float ReadR4(ref SequenceReader<byte> r, Endianness e)
    {
        Span<byte> buf = stackalloc byte[4]; r.TryCopyTo(buf); r.Advance(4);
        return e == Endianness.LittleEndian ? BinaryPrimitives.ReadSingleLittleEndian(buf) : BinaryPrimitives.ReadSingleBigEndian(buf);
    }

    internal static double ReadR8(ref SequenceReader<byte> r, Endianness e)
    {
        Span<byte> buf = stackalloc byte[8]; r.TryCopyTo(buf); r.Advance(8);
        return e == Endianness.LittleEndian ? BinaryPrimitives.ReadDoubleLittleEndian(buf) : BinaryPrimitives.ReadDoubleBigEndian(buf);
    }

    internal static ulong ReadU8(ref SequenceReader<byte> r, Endianness e)
    {
        Span<byte> buf = stackalloc byte[8]; r.TryCopyTo(buf); r.Advance(8);
        return e == Endianness.LittleEndian ? BinaryPrimitives.ReadUInt64LittleEndian(buf) : BinaryPrimitives.ReadUInt64BigEndian(buf);
    }

    internal static long ReadI8(ref SequenceReader<byte> r, Endianness e)
    {
        Span<byte> buf = stackalloc byte[8]; r.TryCopyTo(buf); r.Advance(8);
        return e == Endianness.LittleEndian ? BinaryPrimitives.ReadInt64LittleEndian(buf) : BinaryPrimitives.ReadInt64BigEndian(buf);
    }

    internal static DateTime ReadDateTime(ref SequenceReader<byte> r, Endianness e)
    {
        uint secs = ReadU4(ref r, e);
        return secs == 0 ? default : new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(secs);
    }

    internal static string ReadCn(ref SequenceReader<byte> r)
    {
        r.TryRead(out byte len);
        if (len == 0) return string.Empty;
        Span<byte> buf = stackalloc byte[len];
        r.TryCopyTo(buf); r.Advance(len);
        return Encoding.ASCII.GetString(buf);
    }

    internal static string ReadCf(ref SequenceReader<byte> r, int length)
    {
        Span<byte> buf = length <= 512 ? stackalloc byte[length] : new byte[length];
        r.TryCopyTo(buf); r.Advance(length);
        return Encoding.ASCII.GetString(buf).TrimEnd();
    }

    internal static string ReadSn(ref SequenceReader<byte> r, Endianness e)
    {
        ushort len = ReadU2(ref r, e);
        if (len == 0) return string.Empty;
        Span<byte> buf = len <= 512 ? stackalloc byte[len] : new byte[len];
        r.TryCopyTo(buf); r.Advance(len);
        return Encoding.ASCII.GetString(buf);
    }

    internal static byte[]? ReadBn(ref SequenceReader<byte> r)
    {
        r.TryRead(out byte len);
        if (len == 0) return Array.Empty<byte>();
        var buf = new byte[len]; r.TryCopyTo(buf); r.Advance(len); return buf;
    }

    internal static BitArray? ReadDn(ref SequenceReader<byte> r, Endianness e)
    {
        ushort bitCount = ReadU2(ref r, e);
        if (bitCount == 0) return new BitArray(0);
        int byteCount = (bitCount + 7) / 8;
        var buf = new byte[byteCount]; r.TryCopyTo(buf); r.Advance(byteCount);
        return new BitArray(buf) { Length = bitCount };
    }

    internal static byte[] ReadU1Array(ref SequenceReader<byte> r, int count) { var a = new byte[count]; r.TryCopyTo(a); r.Advance(count); return a; }
    internal static ushort[] ReadU2Array(ref SequenceReader<byte> r, int count, Endianness e) { var a = new ushort[count]; for (int i = 0; i < count; i++) a[i] = ReadU2(ref r, e); return a; }
    internal static uint[] ReadU4Array(ref SequenceReader<byte> r, int count, Endianness e) { var a = new uint[count]; for (int i = 0; i < count; i++) a[i] = ReadU4(ref r, e); return a; }
    internal static ulong[] ReadU8Array(ref SequenceReader<byte> r, int count, Endianness e) { var a = new ulong[count]; for (int i = 0; i < count; i++) a[i] = ReadU8(ref r, e); return a; }
    internal static sbyte[] ReadI1Array(ref SequenceReader<byte> r, int count) { var a = new sbyte[count]; for (int i = 0; i < count; i++) { r.TryRead(out byte b); a[i] = (sbyte)b; } return a; }
    internal static short[] ReadI2Array(ref SequenceReader<byte> r, int count, Endianness e) { var a = new short[count]; for (int i = 0; i < count; i++) a[i] = ReadI2(ref r, e); return a; }
    internal static int[] ReadI4Array(ref SequenceReader<byte> r, int count, Endianness e) { var a = new int[count]; for (int i = 0; i < count; i++) a[i] = ReadI4(ref r, e); return a; }
    internal static long[] ReadI8Array(ref SequenceReader<byte> r, int count, Endianness e) { var a = new long[count]; for (int i = 0; i < count; i++) a[i] = ReadI8(ref r, e); return a; }
    internal static float[] ReadR4Array(ref SequenceReader<byte> r, int count, Endianness e) { var a = new float[count]; for (int i = 0; i < count; i++) a[i] = ReadR4(ref r, e); return a; }
    internal static double[] ReadR8Array(ref SequenceReader<byte> r, int count, Endianness e) { var a = new double[count]; for (int i = 0; i < count; i++) a[i] = ReadR8(ref r, e); return a; }
    internal static string[] ReadCnArray(ref SequenceReader<byte> r, int count) { var a = new string[count]; for (int i = 0; i < count; i++) a[i] = ReadCn(ref r); return a; }
    internal static string[] ReadSnArray(ref SequenceReader<byte> r, int count, Endianness e) { var a = new string[count]; for (int i = 0; i < count; i++) a[i] = ReadSn(ref r, e); return a; }

    internal static byte[] ReadNibbleArray(ref SequenceReader<byte> r, int count)
    {
        var a = new byte[count]; int bc = (count + 1) / 2;
        for (int i = 0; i < bc; i++) { r.TryRead(out byte b); int idx = i * 2; if (idx < count) a[idx] = (byte)(b & 0x0F); if (idx + 1 < count) a[idx + 1] = (byte)((b >> 4) & 0x0F); }
        return a;
    }

    // --- Write Helpers ---

    internal static void WriteU1(IBufferWriter<byte> w, byte v) { var s = w.GetSpan(1); s[0] = v; w.Advance(1); }
    internal static void WriteI1(IBufferWriter<byte> w, sbyte v) { var s = w.GetSpan(1); s[0] = (byte)v; w.Advance(1); }

    internal static void WriteU2(IBufferWriter<byte> w, ushort v, Endianness e)
    {
        var s = w.GetSpan(2);
        if (e == Endianness.LittleEndian) BinaryPrimitives.WriteUInt16LittleEndian(s, v);
        else BinaryPrimitives.WriteUInt16BigEndian(s, v);
        w.Advance(2);
    }

    internal static void WriteI2(IBufferWriter<byte> w, short v, Endianness e)
    {
        var s = w.GetSpan(2);
        if (e == Endianness.LittleEndian) BinaryPrimitives.WriteInt16LittleEndian(s, v);
        else BinaryPrimitives.WriteInt16BigEndian(s, v);
        w.Advance(2);
    }

    internal static void WriteU4(IBufferWriter<byte> w, uint v, Endianness e)
    {
        var s = w.GetSpan(4);
        if (e == Endianness.LittleEndian) BinaryPrimitives.WriteUInt32LittleEndian(s, v);
        else BinaryPrimitives.WriteUInt32BigEndian(s, v);
        w.Advance(4);
    }

    internal static void WriteI4(IBufferWriter<byte> w, int v, Endianness e)
    {
        var s = w.GetSpan(4);
        if (e == Endianness.LittleEndian) BinaryPrimitives.WriteInt32LittleEndian(s, v);
        else BinaryPrimitives.WriteInt32BigEndian(s, v);
        w.Advance(4);
    }

    internal static void WriteR4(IBufferWriter<byte> w, float v, Endianness e)
    {
        var s = w.GetSpan(4);
        if (e == Endianness.LittleEndian) BinaryPrimitives.WriteSingleLittleEndian(s, v);
        else BinaryPrimitives.WriteSingleBigEndian(s, v);
        w.Advance(4);
    }

    internal static void WriteR8(IBufferWriter<byte> w, double v, Endianness e)
    {
        var s = w.GetSpan(8);
        if (e == Endianness.LittleEndian) BinaryPrimitives.WriteDoubleLittleEndian(s, v);
        else BinaryPrimitives.WriteDoubleBigEndian(s, v);
        w.Advance(8);
    }

    internal static void WriteU8(IBufferWriter<byte> w, ulong v, Endianness e)
    {
        var s = w.GetSpan(8);
        if (e == Endianness.LittleEndian) BinaryPrimitives.WriteUInt64LittleEndian(s, v);
        else BinaryPrimitives.WriteUInt64BigEndian(s, v);
        w.Advance(8);
    }

    internal static void WriteI8(IBufferWriter<byte> w, long v, Endianness e)
    {
        var s = w.GetSpan(8);
        if (e == Endianness.LittleEndian) BinaryPrimitives.WriteInt64LittleEndian(s, v);
        else BinaryPrimitives.WriteInt64BigEndian(s, v);
        w.Advance(8);
    }

    internal static void WriteC1(IBufferWriter<byte> w, char v) { var s = w.GetSpan(1); s[0] = (byte)v; w.Advance(1); }

    internal static void WriteCn(IBufferWriter<byte> w, string? v)
    {
        if (v != null && v.Length > 255) throw new InvalidOperationException($"String length {v.Length} exceeds maximum Cn length of 255 characters.");
        byte len = (byte)(v?.Length ?? 0); var s = w.GetSpan(1 + len); s[0] = len;
        if (len > 0) Encoding.ASCII.GetBytes(v!, s.Slice(1, len));
        w.Advance(1 + len);
    }

    internal static void WriteSn(IBufferWriter<byte> w, string? v, Endianness e)
    {
        ushort len = (ushort)(v?.Length ?? 0); var s = w.GetSpan(2 + len);
        if (e == Endianness.LittleEndian) BinaryPrimitives.WriteUInt16LittleEndian(s, len);
        else BinaryPrimitives.WriteUInt16BigEndian(s, len);
        if (len > 0) Encoding.ASCII.GetBytes(v!, s.Slice(2, len));
        w.Advance(2 + len);
    }

    internal static void WriteCf(IBufferWriter<byte> w, string? v, int length)
    {
        var s = w.GetSpan(length); s.Slice(0, length).Fill((byte)' ');
        if (v != null) { int cl = Math.Min(v.Length, length); Encoding.ASCII.GetBytes(v.AsSpan(0, cl), s); }
        w.Advance(length);
    }

    internal static void WriteBn(IBufferWriter<byte> w, byte[]? v)
    {
        if (v != null && v.Length > 255) throw new InvalidOperationException($"Byte array length {v.Length} exceeds maximum Bn length of 255 bytes.");
        byte len = (byte)(v?.Length ?? 0); var s = w.GetSpan(1 + len); s[0] = len;
        if (len > 0) v.AsSpan().CopyTo(s.Slice(1));
        w.Advance(1 + len);
    }

    internal static void WriteDn(IBufferWriter<byte> w, BitArray? v, Endianness e)
    {
        if (v != null && v.Length > ushort.MaxValue) throw new InvalidOperationException($"BitArray length {v.Length} exceeds maximum Dn bit count of {ushort.MaxValue}.");
        ushort bc = (ushort)(v?.Length ?? 0); int byc = (bc + 7) / 8;
        var s = w.GetSpan(2 + byc);
        if (e == Endianness.LittleEndian) BinaryPrimitives.WriteUInt16LittleEndian(s, bc);
        else BinaryPrimitives.WriteUInt16BigEndian(s, bc);
        if (byc > 0 && v != null) { var bytes = new byte[byc]; v.CopyTo(bytes, 0); bytes.AsSpan().CopyTo(s.Slice(2)); }
        w.Advance(2 + byc);
    }

    internal static void WriteN1(IBufferWriter<byte> w, byte v) { var s = w.GetSpan(1); s[0] = (byte)(v & 0x0F); w.Advance(1); }

    internal static void WriteDateTime(IBufferWriter<byte> w, DateTime v, Endianness e)
    {
        uint secs = Types.StdfDateTime.ToStdf(v);
        WriteU4(w, secs, e);
    }

    internal static void WriteU1Array(IBufferWriter<byte> w, byte[] a) { var s = w.GetSpan(a.Length); a.AsSpan().CopyTo(s); w.Advance(a.Length); }
    internal static void WriteU2Array(IBufferWriter<byte> w, ushort[] a, Endianness e) { foreach (var v in a) WriteU2(w, v, e); }
    internal static void WriteU4Array(IBufferWriter<byte> w, uint[] a, Endianness e) { foreach (var v in a) WriteU4(w, v, e); }
    internal static void WriteU8Array(IBufferWriter<byte> w, ulong[] a, Endianness e) { foreach (var v in a) WriteU8(w, v, e); }
    internal static void WriteI1Array(IBufferWriter<byte> w, sbyte[] a) { var s = w.GetSpan(a.Length); for (int i = 0; i < a.Length; i++) s[i] = (byte)a[i]; w.Advance(a.Length); }
    internal static void WriteI2Array(IBufferWriter<byte> w, short[] a, Endianness e) { foreach (var v in a) WriteI2(w, v, e); }
    internal static void WriteI4Array(IBufferWriter<byte> w, int[] a, Endianness e) { foreach (var v in a) WriteI4(w, v, e); }
    internal static void WriteI8Array(IBufferWriter<byte> w, long[] a, Endianness e) { foreach (var v in a) WriteI8(w, v, e); }
    internal static void WriteR4Array(IBufferWriter<byte> w, float[] a, Endianness e) { foreach (var v in a) WriteR4(w, v, e); }
    internal static void WriteR8Array(IBufferWriter<byte> w, double[] a, Endianness e) { foreach (var v in a) WriteR8(w, v, e); }
    internal static void WriteCnArray(IBufferWriter<byte> w, string[] a) { foreach (var v in a) WriteCn(w, v); }
    internal static void WriteSnArray(IBufferWriter<byte> w, string[] a, Endianness e) { foreach (var v in a) WriteSn(w, v, e); }

    internal static void WriteNibbleArray(IBufferWriter<byte> w, byte[] a)
    {
        int bc = (a.Length + 1) / 2; var s = w.GetSpan(bc); s.Slice(0, bc).Clear();
        for (int i = 0; i < a.Length; i++) { int bi = i / 2; if (i % 2 == 0) s[bi] = (byte)(a[i] & 0x0F); else s[bi] |= (byte)((a[i] & 0x0F) << 4); }
        w.Advance(bc);
    }
}
