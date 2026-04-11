namespace Marklio.Stdf.Records;

/// <summary>
/// Test result flags shared by <see cref="Ptr"/> and <see cref="Ftr"/> (TEST_FLG).
/// Indicates alarm conditions, result validity, and pass/fail status.
/// </summary>
[Flags]
public enum TestResultFlags : byte
{
    /// <summary>No flags set.</summary>
    None = 0,

    /// <summary>Alarm detected during testing.</summary>
    Alarm = 0x01,

    /// <summary>Test result is invalid.</summary>
    ResultInvalid = 0x02,

    /// <summary>Test result is unreliable.</summary>
    ResultUnreliable = 0x04,

    /// <summary>Test timed out.</summary>
    Timeout = 0x08,

    /// <summary>Test was not executed.</summary>
    NotExecuted = 0x10,

    /// <summary>Test was aborted.</summary>
    Aborted = 0x20,

    /// <summary>Pass/fail indication is valid.</summary>
    PassFailValid = 0x40,

    /// <summary>Test failed.</summary>
    Failed = 0x80,
}