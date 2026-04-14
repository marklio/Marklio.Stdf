namespace Marklio.Stdf.Attributes;

/// <summary>
/// Marks a partial record class as an STDF record type.
/// The source generator uses this to generate serialization/deserialization code.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class StdfRecordAttribute : Attribute
{
    /// <summary>STDF record type code (REC_TYP).</summary>
    public byte RecordType { get; }

    /// <summary>STDF record sub-type code (REC_SUB).</summary>
    public byte RecordSubType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="StdfRecordAttribute"/> class
    /// with the specified STDF record type and sub-type codes.
    /// </summary>
    /// <param name="recordType">The STDF record type code (REC_TYP).</param>
    /// <param name="recordSubType">The STDF record sub-type code (REC_SUB).</param>
    public StdfRecordAttribute(byte recordType, byte recordSubType)
    {
        RecordType = recordType;
        RecordSubType = recordSubType;
    }
}
