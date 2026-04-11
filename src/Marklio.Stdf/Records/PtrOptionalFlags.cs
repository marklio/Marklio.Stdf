namespace Marklio.Stdf.Records;

/// <summary>
/// Optional data flags for <see cref="Ptr"/> (OPT_FLAG).
/// Indicates which optional limit and scaling fields are valid.
/// </summary>
[Flags]
public enum PtrOptionalFlags : byte
{
    /// <summary>No flags set - all optional fields are valid.</summary>
    None = 0,

    /// <summary>RES_SCAL value is invalid.</summary>
    ResultScaleInvalid = 0x01,

    /// <summary>No low test limit (LO_LIMIT is not meaningful).</summary>
    NoLowLimit = 0x02,

    /// <summary>No high test limit (HI_LIMIT is not meaningful).</summary>
    NoHighLimit = 0x04,

    /// <summary>No low specification limit (LO_SPEC is not meaningful).</summary>
    NoLowSpecLimit = 0x08,

    /// <summary>No high specification limit (HI_SPEC is not meaningful).</summary>
    NoHighSpecLimit = 0x10,

    /// <summary>LLM_SCAL and HLM_SCAL values are invalid.</summary>
    LimitScalesInvalid = 0x20,
}