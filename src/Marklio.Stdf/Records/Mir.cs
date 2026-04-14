using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// Master Information Record (MIR) — type 1, subtype 10.
/// Contains all the global setup and identification information for a test lot.
/// There is exactly one MIR per STDF file, and it stores tester configuration,
/// lot identity, and program details.
/// </summary>
[StdfRecord(1, 10)]
public partial record class Mir
{
    /// <summary>
    /// Date and time of job setup. [STDF: SETUP_T, U*4]
    /// </summary>
    /// <remarks>
    /// Wire format is a 32-bit unsigned Unix epoch timestamp, mapped to <see cref="DateTime"/> via the [StdfDateTime] attribute.
    /// Valid range: 1970-01-01T00:00:01Z to 2106-02-07T06:28:15Z. <c>default(DateTime)</c> maps to 0 ("not specified").
    /// </remarks>
    [StdfDateTime] public DateTime SetupTime { get; set; }

    /// <summary>
    /// Date and time lot testing started. [STDF: START_T, U*4]
    /// </summary>
    /// <remarks>
    /// Wire format is a 32-bit unsigned Unix epoch timestamp, mapped to <see cref="DateTime"/> via the [StdfDateTime] attribute.
    /// Valid range: 1970-01-01T00:00:01Z to 2106-02-07T06:28:15Z. <c>default(DateTime)</c> maps to 0 ("not specified").
    /// </remarks>
    [StdfDateTime] public DateTime StartTime { get; set; }

    /// <summary>
    /// Tester station number. [STDF: STAT_NUM, U*1]
    /// </summary>
    public byte StationNumber { get; set; }

    /// <summary>
    /// Test mode code (e.g. 'P' = production, 'D' = development, 'E' = engineering). [STDF: MODE_COD, C*1]
    /// </summary>
    /// <remarks>Wire format is a single ASCII byte, mapped to <see cref="char"/> via the [C1] attribute.</remarks>
    [C1] public char ModeCode { get; set; }

    /// <summary>
    /// Retest code indicating lot disposition for retesting. [STDF: RTST_COD, C*1]
    /// </summary>
    /// <remarks>Wire format is a single ASCII byte, mapped to <see cref="char"/> via the [C1] attribute.</remarks>
    [C1] public char RetestCode { get; set; }

    /// <summary>
    /// Data protection code. [STDF: PROT_COD, C*1]
    /// </summary>
    /// <remarks>Wire format is a single ASCII byte, mapped to <see cref="char"/> via the [C1] attribute.</remarks>
    [C1] public char ProtectionCode { get; set; }

    /// <summary>
    /// Burn-in time in minutes. [STDF: BURN_TIM, U*2]
    /// </summary>
    public ushort? BurnInTime { get; set; }

    /// <summary>
    /// Command mode code. [STDF: CMOD_COD, C*1]
    /// </summary>
    /// <remarks>Wire format is a single ASCII byte, mapped to <see cref="char"/> via the [C1] attribute.</remarks>
    [C1] public char? CommandModeCode { get; set; }

    /// <summary>
    /// Lot identification. [STDF: LOT_ID, C*n]
    /// </summary>
    public string? LotId { get; set; }

    /// <summary>
    /// Part type or product ID. [STDF: PART_TYP, C*n]
    /// </summary>
    public string? PartType { get; set; }

    /// <summary>
    /// Name of the tester node. [STDF: NODE_NAM, C*n]
    /// </summary>
    public string? NodeName { get; set; }

    /// <summary>
    /// Tester type or model. [STDF: TSTR_TYP, C*n]
    /// </summary>
    public string? TesterType { get; set; }

    /// <summary>
    /// Job name (test program name). [STDF: JOB_NAM, C*n]
    /// </summary>
    public string? JobName { get; set; }

    /// <summary>
    /// Job revision. [STDF: JOB_REV, C*n]
    /// </summary>
    public string? JobRevision { get; set; }

    /// <summary>
    /// Sublot identification. [STDF: SBLOT_ID, C*n]
    /// </summary>
    public string? SublotId { get; set; }

    /// <summary>
    /// Operator name or ID. [STDF: OPER_NAM, C*n]
    /// </summary>
    public string? OperatorName { get; set; }

    /// <summary>
    /// Tester executive software type. [STDF: EXEC_TYP, C*n]
    /// </summary>
    public string? ExecType { get; set; }

    /// <summary>
    /// Tester executive software version. [STDF: EXEC_VER, C*n]
    /// </summary>
    public string? ExecVersion { get; set; }

    /// <summary>
    /// Test phase or step code. [STDF: TEST_COD, C*n]
    /// </summary>
    public string? TestCode { get; set; }

    /// <summary>
    /// Test temperature (string because it may include units). [STDF: TST_TEMP, C*n]
    /// </summary>
    public string? TestTemperature { get; set; }

    /// <summary>
    /// Generic user-supplied text. [STDF: USER_TXT, C*n]
    /// </summary>
    public string? UserText { get; set; }

    /// <summary>
    /// Name of auxiliary data file. [STDF: AUX_FILE, C*n]
    /// </summary>
    public string? AuxiliaryFile { get; set; }

    /// <summary>
    /// Package type. [STDF: PKG_TYP, C*n]
    /// </summary>
    public string? PackageType { get; set; }

    /// <summary>
    /// Product family identification. [STDF: FAMLY_ID, C*n]
    /// </summary>
    public string? FamilyId { get; set; }

    /// <summary>
    /// Date code for the package or lot. [STDF: DATE_COD, C*n]
    /// </summary>
    public string? DateCode { get; set; }

    /// <summary>
    /// Test facility identifier. [STDF: FACIL_ID, C*n]
    /// </summary>
    public string? FacilityId { get; set; }

    /// <summary>
    /// Test floor identifier. [STDF: FLOOR_ID, C*n]
    /// </summary>
    public string? FloorId { get; set; }

    /// <summary>
    /// Fabrication process identifier. [STDF: PROC_ID, C*n]
    /// </summary>
    public string? ProcessId { get; set; }

    /// <summary>
    /// Operation frequency or step. [STDF: OPER_FRQ, C*n]
    /// </summary>
    public string? OperationFrequency { get; set; }

    /// <summary>
    /// Test specification name. [STDF: SPEC_NAM, C*n]
    /// </summary>
    public string? SpecificationName { get; set; }

    /// <summary>
    /// Test specification version. [STDF: SPEC_VER, C*n]
    /// </summary>
    public string? SpecificationVersion { get; set; }

    /// <summary>
    /// Test flow identifier. [STDF: FLOW_ID, C*n]
    /// </summary>
    public string? FlowId { get; set; }

    /// <summary>
    /// Test setup identifier. [STDF: SETUP_ID, C*n]
    /// </summary>
    public string? SetupId { get; set; }

    /// <summary>
    /// Device design revision. [STDF: DSGN_REV, C*n]
    /// </summary>
    public string? DesignRevision { get; set; }

    /// <summary>
    /// Engineering lot identifier. [STDF: ENG_ID, C*n]
    /// </summary>
    public string? EngineeringId { get; set; }

    /// <summary>
    /// ROM code identifier. [STDF: ROM_COD, C*n]
    /// </summary>
    public string? RomCode { get; set; }

    /// <summary>
    /// Tester serial number. [STDF: SERL_NUM, C*n]
    /// </summary>
    public string? SerialNumber { get; set; }

    /// <summary>
    /// Supervisor name or ID. [STDF: SUPR_NAM, C*n]
    /// </summary>
    public string? SupervisorName { get; set; }
}
