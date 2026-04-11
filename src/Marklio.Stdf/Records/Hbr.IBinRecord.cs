namespace Marklio.Stdf.Records;

public partial record struct Hbr
{
    /// <summary>
    /// Gets the bin number. This is an <see cref="IBinRecord"/> convenience alias for <see cref="HardwareBin"/>.
    /// </summary>
    public ushort BinNumber => HardwareBin;

    /// <summary>
    /// Gets the pass/fail indication. This is an <see cref="IBinRecord"/> convenience alias for <see cref="BinPassFail"/>.
    /// </summary>
    public char? PassFail => BinPassFail;
}
