namespace Marklio.Stdf;

/// <summary>
/// Implemented by records that identify a test head (e.g., WIR, WRR, SDR, PIR, PRR, PTR, etc.).
/// </summary>
public interface IHeadRecord
{
    byte HeadNumber { get; }
}

/// <summary>
/// Implemented by records that identify a specific head and site combination
/// (e.g., PIR, PRR, PTR, MPR, FTR, HBR, SBR, PCR, TSR, STR).
/// </summary>
public interface IHeadSiteRecord : IHeadRecord
{
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
    ushort BinNumber { get; }
    uint BinCount { get; }
    char? PassFail { get; }
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
    uint TestNumber { get; }
}
