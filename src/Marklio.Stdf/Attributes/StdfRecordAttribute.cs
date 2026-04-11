namespace Marklio.Stdf.Attributes;

/// <summary>
/// Marks a partial record struct as an STDF record type.
/// The source generator uses this to generate serialization/deserialization code.
/// </summary>
[AttributeUsage(AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
public sealed class StdfRecordAttribute : Attribute
{
    /// <summary>STDF record type code (REC_TYP).</summary>
    public byte RecordType { get; }

    /// <summary>STDF record sub-type code (REC_SUB).</summary>
    public byte RecordSubType { get; }

    public StdfRecordAttribute(byte recordType, byte recordSubType)
    {
        RecordType = recordType;
        RecordSubType = recordSubType;
    }
}
