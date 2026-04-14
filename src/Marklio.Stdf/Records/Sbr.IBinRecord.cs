namespace Marklio.Stdf.Records;

public partial record class Sbr
{
    /// <summary>
    /// Gets the bin number. This is an <see cref="IBinRecord"/> convenience alias for <see cref="SoftwareBin"/>.
    /// </summary>
    public ushort BinNumber => SoftwareBin;

    /// <summary>
    /// Gets the pass/fail indication. This is an <see cref="IBinRecord"/> convenience alias for <see cref="BinPassFail"/>.
    /// </summary>
    public char? PassFail => BinPassFail;
}
