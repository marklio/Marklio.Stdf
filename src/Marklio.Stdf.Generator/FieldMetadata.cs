using System;
using System.Collections.Generic;

namespace Marklio.Stdf.Generator;

/// <summary>
/// Represents metadata about a single field in an STDF record definition,
/// as extracted from the Roslyn syntax tree.
/// </summary>
internal sealed class FieldMetadata : IEquatable<FieldMetadata>
{
    public string PropertyName { get; set; } = "";
    public string ClrTypeName { get; set; } = "";
    public bool IsNullable { get; set; }
    public StdfFieldType StdfType { get; set; }
    public int DeclarationOrder { get; set; }
    public bool IsWireCount { get; set; }
    public string? WireCountGroup { get; set; }
    public string? CountedArrayGroup { get; set; }
    public bool IsNibble { get; set; }
    public bool IsBitArray { get; set; }
    public bool IsBitField { get; set; }
    public bool IsBitEncoded { get; set; }
    public bool IsStdfDateTime { get; set; }
    public bool IsC1 { get; set; }
    public bool IsSn { get; set; }
    public int FixedStringLength { get; set; }

    public bool Equals(FieldMetadata? other)
    {
        if (other is null) return false;
        return PropertyName == other.PropertyName
            && ClrTypeName == other.ClrTypeName
            && IsNullable == other.IsNullable
            && StdfType == other.StdfType
            && DeclarationOrder == other.DeclarationOrder
            && IsWireCount == other.IsWireCount
            && WireCountGroup == other.WireCountGroup
            && CountedArrayGroup == other.CountedArrayGroup
            && IsNibble == other.IsNibble
            && IsBitArray == other.IsBitArray
            && IsBitField == other.IsBitField
            && IsBitEncoded == other.IsBitEncoded
            && IsStdfDateTime == other.IsStdfDateTime
            && IsC1 == other.IsC1
            && IsSn == other.IsSn
            && FixedStringLength == other.FixedStringLength;
    }

    public override bool Equals(object? obj) => Equals(obj as FieldMetadata);

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + (PropertyName?.GetHashCode() ?? 0);
            hash = hash * 31 + DeclarationOrder;
            return hash;
        }
    }
}

/// <summary>
/// Resolved STDF wire type for a field.
/// </summary>
internal enum StdfFieldType
{
    U1,
    U2,
    U4,
    I1,
    I2,
    I4,
    R4,
    R8,
    C1,
    Cn,
    Cf,
    B1,
    Bn,
    Dn,
    N1,
    DateTime,
    // Array element types (used for counted arrays)
    ArrayU1,
    ArrayU2,
    ArrayU4,
    ArrayU8,
    ArrayI1,
    ArrayI2,
    ArrayI4,
    ArrayR4,
    ArrayR8,
    ArrayCn,
    ArrayN1,
    // Wire-only (count marker)
    WireCountU1,
    WireCountU2,
    WireCountU4,
    // 8-byte types
    U8,
    I8,
    ArrayI8,
    // V4-2007 S*n (2-byte length-prefixed string)
    Sn,
    ArraySn,
}

/// <summary>
/// Full metadata for a record type extracted by the analyzer.
/// </summary>
internal sealed class RecordMetadata : IEquatable<RecordMetadata>
{
    public string Namespace { get; set; } = "";
    public string TypeName { get; set; } = "";
    public byte RecordType { get; set; }
    public byte RecordSubType { get; set; }
    public List<FieldMetadata> Fields { get; set; } = new();

    public bool Equals(RecordMetadata? other)
    {
        if (other is null) return false;
        if (Namespace != other.Namespace || TypeName != other.TypeName
            || RecordType != other.RecordType || RecordSubType != other.RecordSubType)
            return false;
        if (Fields.Count != other.Fields.Count) return false;
        for (int i = 0; i < Fields.Count; i++)
        {
            if (!Fields[i].Equals(other.Fields[i])) return false;
        }
        return true;
    }

    public override bool Equals(object? obj) => Equals(obj as RecordMetadata);

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + (Namespace?.GetHashCode() ?? 0);
            hash = hash * 31 + (TypeName?.GetHashCode() ?? 0);
            hash = hash * 31 + RecordType;
            hash = hash * 31 + RecordSubType;
            return hash;
        }
    }
}
