namespace Reihitsu.Cli.Abstractions;

/// <summary>
/// Abstracts diff generation for testability
/// </summary>
internal interface IDiffGenerator
{
    #region Methods

    /// <summary>
    /// Generates a unified diff between original and formatted content
    /// </summary>
    /// <param name="filePath">The full path of the file, from which the diff header names the file</param>
    /// <param name="originalContent">The original file content</param>
    /// <param name="formattedContent">The formatted file content</param>
    /// <returns>A string containing the unified diff output</returns>
    string Generate(string filePath, string originalContent, string formattedContent);

    #endregion // Methods
}