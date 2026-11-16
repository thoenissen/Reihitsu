using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Core;
using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline;

namespace Reihitsu.ArchitectureTests.Formatter;

/// <summary>
/// Guards the formatter phase ownership map in the repository instructions
/// </summary>
[TestClass]
public sealed class PhaseOwnershipDocumentationTests
{
    #region Constants

    /// <summary>
    /// Marker before the ownership-map block
    /// </summary>
    private const string StartMarker = "<!-- formatter-phase-ownership:start -->";

    /// <summary>
    /// Marker after the ownership-map block
    /// </summary>
    private const string EndMarker = "<!-- formatter-phase-ownership:end -->";

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies that the map in <c>AGENTS.md</c> lists every runtime phase exactly once in order
    /// </summary>
    [TestMethod]
    public void OwnershipMapMatchesRuntimePhaseOrder()
    {
        // Arrange
        var repositoryRoot = FindRepositoryRoot();
        var expectedBlock = ExtractOwnershipMap(Path.Combine(repositoryRoot, "AGENTS.md"));

        // Act
        var phaseNames = GetRuntimePhaseNames();

        // Assert
        Assert.HasCount(13, phaseNames);

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
    /// Verifies that every backticked name in the map's guard column resolves to a type or member declared in the
    /// formatter or core assembly, or to a syntax kind, so a renamed or deleted guard cannot stay in the map
    /// </summary>
    [TestMethod]
    public void OwnershipMapGuardNamesResolveToDeclarations()
    {
        // Arrange
        var repositoryRoot = FindRepositoryRoot();
        var block = ExtractOwnershipMap(Path.Combine(repositoryRoot, "AGENTS.md"));
        var types = typeof(FormattingPipeline).Assembly.GetTypes()
                                                       .Concat(typeof(CasingUtilities).Assembly.GetTypes())
                                                       .ToArray();

        // Act
        var unresolved = GetGuardColumnNames(block).Where(name => IsDeclared(name, types) == false)
                                                   .ToList();

        // Assert
        Assert.IsEmpty(unresolved, $"Guard names without a declaration: {string.Join(", ", unresolved)}");
    }

    /// <summary>
    /// Collects the identifier-shaped backticked names of the guard column, the third column of each table row
    /// </summary>
    /// <param name="block">Ownership-map block</param>
    /// <returns>Distinct names, such as <c>Type</c>, <c>Member</c>, or <c>Type.Member</c></returns>
    private static List<string> GetGuardColumnNames(string block)
    {
        var names = new List<string>();

        foreach (var line in block.Split('\n'))
        {
            var cells = line.Split('|');

            if (cells.Length < 5
                || line.StartsWith("|---", StringComparison.Ordinal)
                || cells[1].Trim() == "Phase")
            {
                continue;
            }

            foreach (Match match in Regex.Matches(cells[3], "`(?<name>[A-Za-z_][A-Za-z0-9_]*(?:\\.[A-Za-z_][A-Za-z0-9_]*)*)`"))
            {
                names.Add(match.Groups["name"].Value);
            }
        }

        Assert.IsNotEmpty(names, "The ownership map's guard column names no guard.");

        return names.Distinct(StringComparer.Ordinal)
                    .ToList();
    }

    /// <summary>
    /// Determines whether a guard name resolves to a declaration: <c>Type.Member</c> to a member of a type of that
    /// name, a single name to a type or a member of any type, or to a <see cref="SyntaxKind"/> member
    /// </summary>
    /// <param name="name">Guard name</param>
    /// <param name="types">Types of the formatter and core assemblies</param>
    /// <returns><see langword="true"/> when the name resolves; otherwise, <see langword="false"/></returns>
    private static bool IsDeclared(string name, Type[] types)
    {
        const BindingFlags allMembers = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        var segments = name.Split('.');

        if (segments.Length == 2)
        {
            return types.Any(type => type.Name == segments[0]
                                     && type.GetMember(segments[1], allMembers).Length > 0);
        }

        if (segments.Length == 1)
        {
            return Enum.TryParse<SyntaxKind>(name, out _)
                   || types.Any(type => type.Name == name
                                        || type.GetMember(name, allMembers).Length > 0);
        }

        return false;
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
    private static List<string> GetRuntimePhaseNames()
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