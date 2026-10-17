using System;
using System.IO;
using System.Text;

namespace Reihitsu.ArchitectureTests.Sdd;

/// <summary>
/// Creates and cleans up a temporary repository-shaped directory holding fixture specs and sources for one
/// <see cref="SpecConsistencyTests"/> test
/// </summary>
internal sealed class SpecFixtureDirectory : IDisposable
{
    #region Properties

    /// <summary>
    /// Full path of the temporary directory
    /// </summary>
    public string Path { get; } = Directory.CreateTempSubdirectory("ReihitsuSpecTest").FullName;

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Writes a fixture file, creating any intermediate directories it needs
    /// </summary>
    /// <param name="relativePath">File path relative to the temporary directory, with forward slashes</param>
    /// <param name="content">File content</param>
    public void WriteFile(string relativePath, string content)
    {
        var fullPath = System.IO.Path.Combine(Path, relativePath);

        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(fullPath));
        File.WriteAllText(fullPath, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    /// <summary>
    /// Writes a spec with the given code-owners cell and requirement IDs
    /// </summary>
    /// <param name="relativePath">Spec path relative to <c>sdd/specs</c></param>
    /// <param name="codeOwners">Content of the <c>Code owners</c> cell</param>
    /// <param name="requirementIds">Requirement IDs, one heading each</param>
    public void WriteSpec(string relativePath, string codeOwners, params string[] requirementIds)
    {
        var builder = new StringBuilder();

        builder.Append("# Spec\n\n| | |\n|---|---|\n| ID prefix | X |\n| Status | Active |\n")
               .Append($"| Code owners | {codeOwners} |\n")
               .Append("| Uses | — |\n\n## Requirements\n");

        foreach (var requirementId in requirementIds)
        {
            builder.Append($"\n### {requirementId} — Title\n\nThe formatter MUST do it.\n");
        }

        WriteFile($"sdd/specs/{relativePath}", builder.ToString());
    }

    #endregion // Methods

    #region IDisposable

    /// <inheritdoc/>
    public void Dispose()
    {
        if (Directory.Exists(Path))
        {
            Directory.Delete(Path, recursive: true);
        }
    }

    #endregion // IDisposable
}