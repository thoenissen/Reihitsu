namespace Reihitsu.Cli;

/// <summary>
/// Identifies generated-file names that repository CLI commands must leave untouched
/// </summary>
internal static class GeneratedFileUtilities
{
    #region Methods

    /// <summary>
    /// Determines whether the specified file is a generated file that should be skipped
    /// </summary>
    /// <param name="filePath">The file path to check</param>
    /// <returns><see langword="true"/> if the file is a generated file; otherwise, <see langword="false"/></returns>
    internal static bool IsGeneratedFile(string filePath)
    {
        return filePath.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase)
               || filePath.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase)
               || filePath.EndsWith(".g.i.cs", StringComparison.OrdinalIgnoreCase);
    }

    #endregion // Methods
}