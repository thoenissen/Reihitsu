using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.ArchitectureTests.Helpers;

namespace Reihitsu.ArchitectureTests.General;

/// <summary>
/// Guards that no file outside the squad's notes folder names a note in it. Notes are temporary input for
/// improving the squad and are deleted once folded into a rule, so a reference to one would point at a file
/// that is going away; a rule states its reason itself, in a process change doc. The folder's own
/// <c>README.md</c> describes the folder and is permanent, so it may be referenced. A squad run's working
/// folder under <c>sdd/work/</c> is deleted before its pull request is merged, so its handovers may name the
/// run's note
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
    /// The folder of the squad runs' temporary working docs relative to the repository root, which the scan
    /// skips because it is deleted before a run's pull request is merged
    /// </summary>
    private const string RunWorkFolder = "sdd/work/";

    /// <summary>
    /// Version-control directory the scan never enters, next to the build output that
    /// <see cref="BuildOutputDirectories"/> decides
    /// </summary>
    private const string VersionControlDirectory = ".git";

    /// <summary>
    /// Extensions of the text files the scan reads
    /// </summary>
    private static readonly string[] _scannedExtensions = [".md", ".cs", ".sh", ".yml", ".yaml", ".json", ".txt", ".props", ".targets", ".csproj"];

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
        var repositoryRoot = RepositoryRoot.Find();

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
        using (var fixture = new FixtureDirectory())
        {
            fixture.WriteFile("AGENTS.md", $"See `{NotesFolder}2026-01-01-run.md` for details.\n");
            fixture.WriteFile(Path.Combine("squad", "roles", "role.md"), $"Read [the note](../{NotesSegment}/topic.md).\n");

            // Act
            var findings = ScanForNoteReferences(fixture.Path);

            // Assert
            Assert.HasCount(2, findings);
            Assert.Contains(finding => finding.StartsWith("AGENTS.md:1:", StringComparison.Ordinal), findings);
            Assert.Contains(finding => finding.StartsWith("squad/roles/role.md:1:", StringComparison.Ordinal), findings);
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
        using (var fixture = new FixtureDirectory())
        {
            fixture.WriteFile(Path.Combine("squad", "lead.md"), $"Write `{NotesFolder}<yyyy-mm-dd>-<slug>.md` in the form [{NotesSegment}/{NotesReadme}]({NotesSegment}/{NotesReadme}).\n");
            fixture.WriteFile(Path.Combine("squad", NotesSegment, "2026-01-01-run.md"), $"Follows {NotesSegment}/2025-12-31-run.md.\n");
            fixture.WriteFile(Path.Combine("src", "Sample", "obj", "Sample.md"), $"`{NotesFolder}2026-01-01-run.md`\n");
            fixture.WriteFile(Path.Combine("src", "Sample", "bin", "Sample.md"), $"`{NotesFolder}2026-01-01-run.md`\n");

            // Act
            var findings = ScanForNoteReferences(fixture.Path);

            // Assert
            Assert.IsEmpty(findings);
        }
    }

    /// <summary>
    /// Verifies that the scan accepts a run handover under <c>sdd/work/</c> that names a note, since the run's
    /// working folder is deleted before its pull request is merged
    /// </summary>
    [TestMethod]
    public void ScanIgnoresNoteReferenceInRunHandover()
    {
        // Arrange
        using (var fixture = new FixtureDirectory())
        {
            fixture.WriteFile(Path.Combine("sdd", "work", "sample-run", "01-test-implementer.md"), $"The run's note will be `{NotesFolder}2026-01-01-sample-run.md`.\n");

            // Act
            var findings = ScanForNoteReferences(fixture.Path);

            // Assert
            Assert.IsEmpty(findings);
        }
    }

    /// <summary>
    /// Verifies that the scan accepts note references anywhere below <c>sdd/work/</c>: directly in the folder,
    /// one level deeper than a run's folder, and in another scanned file type
    /// </summary>
    [TestMethod]
    public void ScanIgnoresNoteReferencesAnywhereBelowSddWork()
    {
        // Arrange
        using (var fixture = new FixtureDirectory())
        {
            fixture.WriteFile(Path.Combine("sdd", "work", "overview.md"), $"`{NotesFolder}2026-01-01-run.md`\n");
            fixture.WriteFile(Path.Combine("sdd", "work", "sample-run", "scratch", "Probe.cs"), $"// ../../../../squad/{NotesSegment}/2026-01-01-run.md\n");

            // Act
            var findings = ScanForNoteReferences(fixture.Path);

            // Assert
            Assert.IsEmpty(findings);
        }
    }

    /// <summary>
    /// Verifies that a note reference in a permanent file still fails the scan while a run handover under
    /// <c>sdd/work/</c> names a note at the same time, and that only the permanent files are reported
    /// </summary>
    [TestMethod]
    public void ScanDetectsNoteReferenceInPermanentFilesBesideRunHandover()
    {
        // Arrange
        using (var fixture = new FixtureDirectory())
        {
            fixture.WriteFile(Path.Combine("sdd", "work", "sample-run", "00-lead.md"), $"`{NotesFolder}2026-01-01-sample-run.md`\n");
            fixture.WriteFile(Path.Combine("sdd", "README.md"), $"See `{NotesFolder}2026-01-01-sample-run.md`.\n");
            fixture.WriteFile("AGENTS.md", $"See `{NotesFolder}2026-01-01-sample-run.md`.\n");

            // Act
            var findings = ScanForNoteReferences(fixture.Path);

            // Assert
            Assert.HasCount(2, findings);
            Assert.Contains(finding => finding.StartsWith("sdd/README.md:1:", StringComparison.Ordinal), findings);
            Assert.Contains(finding => finding.StartsWith("AGENTS.md:1:", StringComparison.Ordinal), findings);
        }
    }

    /// <summary>
    /// Verifies that the scan reports note references in paths that only share the text prefix
    /// <c>sdd/work</c> without lying in the folder <c>sdd/work/</c>
    /// </summary>
    [TestMethod]
    public void ScanDetectsNoteReferenceInPathsSharingSddWorkPrefix()
    {
        // Arrange
        using (var fixture = new FixtureDirectory())
        {
            fixture.WriteFile(Path.Combine("sdd", "workflow.md"), $"`{NotesFolder}2026-01-01-run.md`\n");
            fixture.WriteFile(Path.Combine("sdd", "work-archive", "run.md"), $"`{NotesFolder}2026-01-01-run.md`\n");

            // Act
            var findings = ScanForNoteReferences(fixture.Path);

            // Assert
            Assert.HasCount(2, findings);
            Assert.Contains(finding => finding.StartsWith("sdd/workflow.md:1:", StringComparison.Ordinal), findings);
            Assert.Contains(finding => finding.StartsWith("sdd/work-archive/run.md:1:", StringComparison.Ordinal), findings);
        }
    }

    /// <summary>
    /// Verifies that the scan reports note references in a folder named <c>work</c> that is not the
    /// repository's <c>sdd/work/</c>: one outside <c>sdd</c>, and one below a nested <c>sdd</c> folder
    /// </summary>
    [TestMethod]
    public void ScanDetectsNoteReferenceInWorkFolderOutsideRepositorySddWork()
    {
        // Arrange
        using (var fixture = new FixtureDirectory())
        {
            fixture.WriteFile(Path.Combine("src", "Sample", "work", "Sample.md"), $"`{NotesFolder}2026-01-01-run.md`\n");
            fixture.WriteFile(Path.Combine("documentation", "sdd", "work", "page.md"), $"`{NotesFolder}2026-01-01-run.md`\n");

            // Act
            var findings = ScanForNoteReferences(fixture.Path);

            // Assert
            Assert.HasCount(2, findings);
            Assert.Contains(finding => finding.StartsWith("src/Sample/work/Sample.md:1:", StringComparison.Ordinal), findings);
            Assert.Contains(finding => finding.StartsWith("documentation/sdd/work/page.md:1:", StringComparison.Ordinal), findings);
        }
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Scans every text file under <paramref name="root"/> outside the notes folder and the runs' working folder
    /// for references to a note
    /// </summary>
    /// <param name="root">Directory to scan</param>
    /// <returns>One <c>path:line: reference</c> entry per reference, with the path relative to <paramref name="root"/></returns>
    private static List<string> ScanForNoteReferences(string root)
    {
        var findings = new List<string>();

        foreach (var filePath in EnumerateScannedFiles(root))
        {
            var relativePath = Path.GetRelativePath(root, filePath).Replace('\\', '/');

            if (relativePath.StartsWith(NotesFolder, StringComparison.Ordinal)
                || relativePath.StartsWith(RunWorkFolder, StringComparison.Ordinal))
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
            var directoryName = Path.GetFileName(subdirectory);

            if (string.Equals(directoryName, VersionControlDirectory, StringComparison.OrdinalIgnoreCase)
                || BuildOutputDirectories.IsBuildOutputDirectoryName(directoryName))
            {
                continue;
            }

            foreach (var filePath in EnumerateScannedFiles(subdirectory))
            {
                yield return filePath;
            }
        }
    }

    #endregion // Methods
}