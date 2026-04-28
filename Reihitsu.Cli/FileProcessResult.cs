namespace Reihitsu.Cli;

/// <summary>
/// Represents per-file processing counters that can be aggregated by the caller
/// </summary>
internal readonly record struct FileProcessResult(int ChangedFileCount, int SkippedSyntaxErrorCount, int ErrorFileCount)
{
    /// <summary>
    /// No file changed and no error occurred
    /// </summary>
    public static readonly FileProcessResult NoChange = new(0, 0, 0);

    /// <summary>
    /// The processed file changed successfully
    /// </summary>
    public static readonly FileProcessResult Changed = new(1, 0, 0);

    /// <summary>
    /// The processed file was skipped due to syntax errors
    /// </summary>
    public static readonly FileProcessResult SkippedSyntaxError = new(0, 1, 0);

    /// <summary>
    /// The processed file encountered an error
    /// </summary>
    public static readonly FileProcessResult Error = new(0, 0, 1);
}