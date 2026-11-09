namespace Reihitsu.Cli;

/// <summary>
/// Represents per-file processing counters that can be aggregated by the caller
/// </summary>
/// <param name="ChangedFileCount">Number of files that changed</param>
/// <param name="SkippedSyntaxErrorCount">Number of files skipped due to syntax errors</param>
/// <param name="SkippedEncodingCount">Number of files skipped because they could not be decoded</param>
/// <param name="SkippedGeneratedCount">Number of files skipped because they are marked as generated source</param>
/// <param name="ErrorFileCount">Number of files that encountered an error</param>
/// <param name="SkippedUnsupportedCount">Number of files skipped because the formatter refuses them as unsupported</param>
internal readonly record struct FileProcessResult(int ChangedFileCount, int SkippedSyntaxErrorCount, int SkippedEncodingCount, int SkippedGeneratedCount, int ErrorFileCount, int SkippedUnsupportedCount)
{
    #region Constants

    /// <summary>
    /// No file changed and no error occurred
    /// </summary>
    public static readonly FileProcessResult NoChange = new(0, 0, 0, 0, 0, 0);

    /// <summary>
    /// The processed file changed successfully
    /// </summary>
    public static readonly FileProcessResult Changed = new(1, 0, 0, 0, 0, 0);

    /// <summary>
    /// The processed file was skipped due to syntax errors
    /// </summary>
    public static readonly FileProcessResult SkippedSyntaxError = new(0, 1, 0, 0, 0, 0);

    /// <summary>
    /// The processed file was skipped because it could not be decoded using its detected encoding
    /// </summary>
    public static readonly FileProcessResult SkippedEncoding = new(0, 0, 1, 0, 0, 0);

    /// <summary>
    /// The processed file was skipped because it is marked as generated source
    /// </summary>
    public static readonly FileProcessResult SkippedGenerated = new(0, 0, 0, 1, 0, 0);

    /// <summary>
    /// The processed file encountered an error
    /// </summary>
    public static readonly FileProcessResult Error = new(0, 0, 0, 0, 1, 0);

    /// <summary>
    /// The processed file was skipped because the formatter refuses it as unsupported
    /// </summary>
    public static readonly FileProcessResult SkippedUnsupported = new(0, 0, 0, 0, 0, 1);

    #endregion // Constants
}