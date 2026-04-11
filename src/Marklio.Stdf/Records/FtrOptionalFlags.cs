namespace Marklio.Stdf.Records;

/// <summary>
/// Optional data flags for <see cref="Ftr"/> (OPT_FLAG).
/// Indicates which optional failure-detail fields are valid.
/// </summary>
[Flags]
public enum FtrOptionalFlags : byte
{
    /// <summary>No flags set — all optional fields are valid.</summary>
    None = 0,

    /// <summary>CYC_CNT (cycle count) is invalid.</summary>
    CycleCountInvalid = 0x01,

    /// <summary>REL_VADR (relative vector address) is invalid.</summary>
    RelativeAddressInvalid = 0x02,

    /// <summary>RPT_CNT (repeat count) is invalid.</summary>
    RepeatCountInvalid = 0x04,

    /// <summary>NUM_FAIL (fail count) is invalid.</summary>
    FailCountInvalid = 0x08,

    /// <summary>XFAIL_AD and YFAIL_AD (failure addresses) are invalid.</summary>
    FailAddressInvalid = 0x10,

    /// <summary>VECT_OFF (vector offset) is invalid.</summary>
    VectorOffsetInvalid = 0x20,
}
