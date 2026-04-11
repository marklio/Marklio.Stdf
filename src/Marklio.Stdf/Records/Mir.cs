using Marklio.Stdf.Attributes;

namespace Marklio.Stdf.Records;

/// <summary>
/// MIR — Master Information Record (1, 10).
/// Contains global information about the test program and environment.
/// </summary>
[StdfRecord(1, 10)]
public partial record struct Mir
{
    [StdfDateTime] public DateTime SetupTime { get; set; }
    [StdfDateTime] public DateTime StartTime { get; set; }
    public byte StationNumber { get; set; }
    [C1] public char ModeCode { get; set; }
    [C1] public char RetestCode { get; set; }
    [C1] public char ProtectionCode { get; set; }
    public ushort? BurnInTime { get; set; }
    [C1] public char? CommandModeCode { get; set; }
    public string? LotId { get; set; }
    public string? PartType { get; set; }
    public string? NodeName { get; set; }
    public string? TesterType { get; set; }
    public string? JobName { get; set; }
    public string? JobRevision { get; set; }
    public string? SublotId { get; set; }
    public string? OperatorName { get; set; }
    public string? ExecType { get; set; }
    public string? ExecVersion { get; set; }
    public string? TestCode { get; set; }
    public string? TestTemperature { get; set; }
    public string? UserText { get; set; }
    public string? AuxiliaryFile { get; set; }
    public string? PackageType { get; set; }
    public string? FamilyId { get; set; }
    public string? DateCode { get; set; }
    public string? FacilityId { get; set; }
    public string? FloorId { get; set; }
    public string? ProcessId { get; set; }
    public string? OperationFrequency { get; set; }
    public string? SpecificationName { get; set; }
    public string? SpecificationVersion { get; set; }
    public string? FlowId { get; set; }
    public string? SetupId { get; set; }
    public string? DesignRevision { get; set; }
    public string? EngineeringId { get; set; }
    public string? RomCode { get; set; }
    public string? SerialNumber { get; set; }
    public string? SupervisorName { get; set; }
}
