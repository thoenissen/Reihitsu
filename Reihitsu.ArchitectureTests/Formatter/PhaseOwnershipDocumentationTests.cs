using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline;

namespace Reihitsu.ArchitectureTests.Formatter;

/// <summary>
/// Guards the formatter phase ownership map shared by repository instruction files
/// </summary>
[TestClass]
public sealed class PhaseOwnershipDocumentationTests
{
    #region Constants

    /// <summary>
    /// Marker before the mirrored ownership-map block
    /// </summary>
    private const string StartMarker = "<!-- formatter-phase-ownership:start -->";

    /// <summary>
    /// Marker after the mirrored ownership-map block
    /// </summary>
    private const string EndMarker = "<!-- formatter-phase-ownership:end -->";

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies that every instruction file carries the same map and lists every runtime phase exactly once in order
    /// </summary>
    [TestMethod]
    public void OwnershipMapsAreIdenticalAndMatchRuntimePhaseOrder()
    {
        // Arrange
        var repositoryRoot = FindRepositoryRoot();
        var instructionPaths = new[]
                               {
                                   Path.Combine(repositoryRoot, "CLAUDE.md"),
                                   Path.Combine(repositoryRoot, "AGENTS.md"),
                                   Path.Combine(repositoryRoot, ".github", "copilot-instructions.md")
                               };
        var expectedBlock = ExtractOwnershipMap(instructionPaths[0]);
        var phaseNames = GetRuntimePhaseNames();

        // Act and assert
        foreach (var instructionPath in instructionPaths)
        {
            Assert.AreEqual(expectedBlock,
                            ExtractOwnershipMap(instructionPath),
                            $"Ownership map differs in {instructionPath}.");
        }

        Assert.HasCount(12, phaseNames);

        var previousPosition = -1;

        foreach (var phaseName in phaseNames)
        {
            var tableEntry = $"`{phaseName}`";
            var position = expectedBlock.IndexOf(tableEntry, StringComparison.Ordinal);

            Assert.AreEqual(1,
                            CountOccurrences(expectedBlock, tableEntry),
                            $"Expected exactly one ownership row for {phaseName}.");
            Assert.IsGreaterThan(previousPosition,
                                 position,
                                 $"Ownership row for {phaseName} is out of runtime order.");

            previousPosition = position;
        }
    }

    /// <summary>
    /// Counts non-overlapping occurrences of a value in text
    /// </summary>
    /// <param name="text">Text to search</param>
    /// <param name="value">Value to count</param>
    /// <returns>Number of occurrences</returns>
    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        var searchPosition = 0;

        while ((searchPosition = text.IndexOf(value, searchPosition, StringComparison.Ordinal)) >= 0)
        {
            count++;
            searchPosition += value.Length;
        }

        return count;
    }

    /// <summary>
    /// Extracts and normalizes the marked phase ownership block from an instruction file
    /// </summary>
    /// <param name="path">Instruction file path</param>
    /// <returns>The marked ownership block with LF line endings</returns>
    private static string ExtractOwnershipMap(string path)
    {
        var text = File.ReadAllText(path).Replace("\r\n", "\n", StringComparison.Ordinal);
        var start = text.IndexOf(StartMarker, StringComparison.Ordinal);
        var end = text.IndexOf(EndMarker, StringComparison.Ordinal);

        Assert.IsGreaterThanOrEqualTo(0, start, $"Missing start marker in {path}.");
        Assert.IsGreaterThan(start, end, $"Missing or misplaced end marker in {path}.");

        return text[start..(end + EndMarker.Length)];
    }

    /// <summary>
    /// Finds the repository root from the test output directory
    /// </summary>
    /// <returns>Absolute repository-root path</returns>
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

    /// <summary>
    /// Obtains the top-level runtime phase names through the pipeline observer
    /// </summary>
    /// <returns>Phase names in execution order</returns>
    private static IReadOnlyList<string> GetRuntimePhaseNames()
    {
        var tree = CSharpSyntaxTree.ParseText("internal class Sample { }");
        var phaseNames = new List<string>();

        FormattingPipeline.Execute(tree.GetRoot(),
                                   new FormattingContext("\n"),
                                   (phaseName, _, _) => phaseNames.Add(phaseName),
                                   CancellationToken.None);

        return phaseNames;
    }

    #endregion // Methods
}