using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.ArchitectureTests.General;

/// <summary>
/// Guards that no file outside the squad's notes folder names a note in it. Notes are temporary input for
/// improving the squad and are deleted once folded into a rule, so a reference to one would point at a file
/// that is going away; a rule states its reason itself, in a process change doc. The folder's own
/// <c>README.md</c> describes the folder and is permanent, so it may be referenced
/// </summary>
[TestClass]
public sealed class SquadNoteReferenceTests
{
    #region Constants

    /// <summary>
    /// Name of the folder segment that holds the notes, kept apart from any file name in this source so that
    /// the scan of the repository does not find this file
    /// </summary>
    private const string NotesSegment = "notes";

    /// <summary>
    /// The notes folder relative to the repository root
    /// </summary>
    private const string NotesFolder = $"squad/{NotesSegment}/";

    /// <summary>
    /// The permanent description of the notes folder, which may be referenced
    /// </summary>
    private const string NotesReadme = "README.md";

    /// <summary>
    /// Extensions of the text files the scan reads
    /// </summary>
    private static readonly string[] _scannedExtensions = [".md", ".cs", ".sh", ".yml", ".yaml", ".json", ".txt", ".props", ".targets", ".csproj"];

    /// <summary>
    /// Directory names the scan never enters: version control and build output
    /// </summary>
    private static readonly string[] _skippedDirectories = [".git", "bin", "obj"];

    /// <summary>
    /// Matches a reference to a Markdown file in a notes folder, by repository path or by relative path, and
    /// captures the file name; a placeholder such as <c>&lt;slug&gt;</c> does not start with a name character
    /// and is not matched
    /// </summary>
    private static readonly Regex _noteReferencePattern = new($@"(?<![A-Za-z0-9_.-]){NotesSegment}/(?<name>[A-Za-z0-9][A-Za-z0-9._-]*\.md)");

    #endregion // Constants

    #region Tests

    /// <summary>
    /// Verifies that no file of the repository outside the notes folder names a note
    /// </summary>
    [TestMethod]
    public void RepositoryFilesReferenceNoSquadNote()
    {
        // Arrange
        var repositoryRoot = FindRepositoryRoot();

        // Act
        var findings = ScanForNoteReferences(repositoryRoot);

        // Assert
        Assert.IsEmpty(findings, $"Files outside {NotesFolder} name a note, which is temporary:{Environment.NewLine}{string.Join(Environment.NewLine, findings)}");
    }

    /// <summary>
    /// Verifies that the scan reports a note named by its repository path and a note named by a relative link
    /// </summary>
    [TestMethod]
    public void ScanDetectsRepositoryPathAndRelativeLink()
    {
        // Arrange
        var root = Directory.CreateTempSubdirectory("ReihitsuSquadNoteTest").FullName;

        try
        {
            WriteFile(root, "AGENTS.md", $"See `{NotesFolder}2026-01-01-run.md` for details.\n");
            WriteFile(Path.Combine(root, "squad", "roles"), "role.md", $"Read [the note](../{NotesSegment}/topic.md).\n");

            // Act
            var findings = ScanForNoteReferences(root);

            // Assert
            Assert.HasCount(2, findings);
            Assert.Contains(finding => finding.StartsWith("AGENTS.md:1:", StringComparison.Ordinal), findings);
            Assert.Contains(finding => finding.StartsWith("squad/roles/role.md:1:", StringComparison.Ordinal), findings);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>
    /// Verifies that the scan accepts the folder's README, a placeholder file name, a reference between two
    /// notes, and build output
    /// </summary>
    [TestMethod]
    public void ScanIgnoresReadmePlaceholdersNotesAndBuildOutput()
    {
        // Arrange
        var root = Directory.CreateTempSubdirectory("ReihitsuSquadNoteTest").FullName;

        try
        {
            WriteFile(Path.Combine(root, "squad"), "lead.md", $"Write `{NotesFolder}<yyyy-mm-dd>-<slug>.md` in the form [{NotesSegment}/{NotesReadme}]({NotesSegment}/{NotesReadme}).\n");
            WriteFile(Path.Combine(root, "squad", NotesSegment), "2026-01-01-run.md", $"Follows {NotesSegment}/2025-12-31-run.md.\n");
            WriteFile(Path.Combine(root, "src", "Sample", "obj"), "Sample.md", $"`{NotesFolder}2026-01-01-run.md`\n");
            WriteFile(Path.Combine(root, "src", "Sample", "bin"), "Sample.md", $"`{NotesFolder}2026-01-01-run.md`\n");

            // Act
            var findings = ScanForNoteReferences(root);

            // Assert
            Assert.IsEmpty(findings);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Scans every text file under <paramref name="root"/> outside the notes folder for references to a note
    /// </summary>
    /// <param name="root">Directory to scan</param>
    /// <returns>One <c>path:line: reference</c> entry per reference, with the path relative to <paramref name="root"/></returns>
    private static List<string> ScanForNoteReferences(string root)
    {
        var findings = new List<string>();

        foreach (var filePath in EnumerateScannedFiles(root))
        {
            var relativePath = Path.GetRelativePath(root, filePath).Replace('\\', '/');

            if (relativePath.StartsWith(NotesFolder, StringComparison.Ordinal))
            {
                continue;
            }

            var lines = File.ReadAllLines(filePath);

            for (var lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                foreach (Match match in _noteReferencePattern.Matches(lines[lineIndex]))
                {
                    if (string.Equals(match.Groups["name"].Value, NotesReadme, StringComparison.Ordinal) == false)
                    {
                        findings.Add($"{relativePath}:{lineIndex + 1}: {match.Value}");
                    }
                }
            }
        }

        return findings;
    }

    /// <summary>
    /// Enumerates the text files with a scanned extension below <paramref name="directory"/>, skipping version
    /// control and build output
    /// </summary>
    /// <param name="directory">Directory to enumerate</param>
    /// <returns>The full paths of the files to scan</returns>
    private static IEnumerable<string> EnumerateScannedFiles(string directory)
    {
        foreach (var filePath in Directory.EnumerateFiles(directory))
        {
            if (_scannedExtensions.Contains(Path.GetExtension(filePath), StringComparer.OrdinalIgnoreCase))
            {
                yield return filePath;
            }
        }

        foreach (var subdirectory in Directory.EnumerateDirectories(directory))
        {
            if (_skippedDirectories.Contains(Path.GetFileName(subdirectory), StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            foreach (var filePath in EnumerateScannedFiles(subdirectory))
            {
                yield return filePath;
            }
        }
    }

    /// <summary>
    /// Writes a fixture file, creating its directory when needed
    /// </summary>
    /// <param name="directory">Directory of the file</param>
    /// <param name="fileName">File name</param>
    /// <param name="content">File content</param>
    private static void WriteFile(string directory, string fileName, string content)
    {
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, fileName), content);
    }

    /// <summary>
    /// Locates the repository root by walking up from the test assembly's directory to the solution file
    /// </summary>
    /// <returns>The full path of the repository root</returns>
    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current != null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Reihitsu.sln")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the Reihitsu repository root.");
    }

    #endregion // Methods
}