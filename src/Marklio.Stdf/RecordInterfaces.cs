namespace Marklio.Stdf;

/// <summary>
/// Implemented by records that identify a test head (e.g., WIR, WRR, SDR, PIR, PRR, PTR, etc.).
/// </summary>
public interface IHeadRecord
{
    /// <summary>Gets the test head number (HEAD_NUM) that identifies the test head.</summary>
    byte HeadNumber { get; }
}

/// <summary>
/// Implemented by records that identify a specific head and site combination
/// (e.g., PIR, PRR, PTR, MPR, FTR, HBR, SBR, PCR, TSR, STR).
/// </summary>
public interface IHeadSiteRecord : IHeadRecord
{
    /// <summary>Gets the site number (SITE_NUM) within the test head.</summary>
    byte SiteNumber { get; }
}

/// <summary>
/// Implemented by bin classification records (HBR and SBR).
/// Enables pattern matching across hardware and software bin records.
/// </summary>
/// <example>
/// <code>
/// if (rec.Record is IBinRecord bin)
///     Console.WriteLine($"Bin {bin.BinNumber}: {bin.BinCount} parts ({bin.PassFail})");
/// </code>
/// </example>
public interface IBinRecord : IHeadSiteRecord
{
    /// <summary>Gets the bin number (HBIN_NUM or SBIN_NUM) for this bin classification.</summary>
    ushort BinNumber { get; }
    /// <summary>Gets the number of parts that fell into this bin (HBIN_CNT or SBIN_CNT).</summary>
    uint BinCount { get; }
    /// <summary>Gets the pass/fail indication for the bin: 'P' for pass, 'F' for fail, or <c>null</c> if unspecified.</summary>
    char? PassFail { get; }
    /// <summary>Gets the descriptive name of the bin, or <c>null</c> if not provided.</summary>
    string? BinName { get; }
}

/// <summary>
/// Implemented by test execution records (PTR, MPR, FTR, STR).
/// Enables pattern matching across all per-test-execution records.
/// </summary>
/// <example>
/// <code>
/// if (rec.Record is ITestRecord test)
///     Console.WriteLine($"Test {test.TestNumber} at head {test.HeadNumber} site {test.SiteNumber}");
/// </code>
/// </example>
public interface ITestRecord : IHeadSiteRecord
{
    /// <summary>Gets the test number (TEST_NUM) that uniquely identifies the test.</summary>
    uint TestNumber { get; }
}
