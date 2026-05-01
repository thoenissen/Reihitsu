namespace Reihitsu.Cli.Abstractions;

/// <summary>
/// Abstracts diff generation for testability
/// </summary>
internal interface IDiffGenerator
{
    #region Members

    /// <summary>
    /// Generates a unified diff between original and formatted content
    /// </summary>
    /// <param name="filePath">The file path to display in the diff header</param>
    /// <param name="originalContent">The original file content</param>
    /// <param name="formattedContent">The formatted file content</param>
    /// <returns>A string containing the unified diff output</returns>
    string Generate(string filePath, string originalContent, string formattedContent);

    #endregion // Members
}