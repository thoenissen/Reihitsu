using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Analyzer.Test.SelfHosting;

/// <summary>
/// Verifies the conventions every published rule page under <c>documentation/rules/</c> follows: the page links the
/// shared general notes, which own every limitation common to all rules, and its prose describes the analyzer and its
/// code fix only, because a user can install the analyzer package without the formatter
/// </summary>
/// <remarks>
/// The regex below only ever matches repository-owned rule documentation and carries no nested quantifier capable of
/// pathological backtracking, so it uses <see cref="Regex.InfiniteMatchTimeout"/>
/// </remarks>
[TestClass]
public class RuleDocumentationConventionTests
{
    #region Constants

    /// <summary>
    /// File name of the page that describes behavior shared by every rule
    /// </summary>
    private const string GeneralNotesFileName = "general-notes.md";

    /// <summary>
    /// Footer line every rule page ends with
    /// </summary>
    private const string FooterLine = "See [General notes](general-notes.md) for behavior that applies to every rule: comments and preprocessor directives, generated code, and turning a rule off.";

    #endregion // Constants

    #region Fields

    /// <summary>
    /// Regex for prose that mentions the formatter or its command-line tool
    /// </summary>
    private static readonly Regex _formatterMentionRegex = new(@"\bformatter\b|reihitsu-format", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, Regex.InfiniteMatchTimeout);

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Enumerates every published rule page
    /// </summary>
    /// <returns>Rule page paths, ordered by file name</returns>
    private static string[] GetRuleDocumentPaths()
    {
        return Directory.EnumerateFiles(GetRuleDocumentationDirectory(), "RH*.md", SearchOption.TopDirectoryOnly)
                        .OrderBy(path => path, StringComparer.Ordinal)
                        .ToArray();
    }

    /// <summary>
    /// Returns the lines of a rule page that lie outside fenced code blocks
    /// </summary>
    /// <param name="path">Rule page path</param>
    /// <returns>Prose lines with their one-based line numbers</returns>
    private static IEnumerable<(int LineNumber, string Text)> GetProseLines(string path)
    {
        var isInsideFence = false;
        var lineNumber = 0;

        foreach (var line in File.ReadLines(path))
        {
            lineNumber++;

            if (line.StartsWith("```", StringComparison.Ordinal))
            {
                isInsideFence = isInsideFence == false;

                continue;
            }

            if (isInsideFence == false)
            {
                yield return (lineNumber, line);
            }
        }
    }

    /// <summary>
    /// Finds the rule documentation directory
    /// </summary>
    /// <returns>Absolute path of <c>documentation/rules</c></returns>
    private static string GetRuleDocumentationDirectory()
    {
        var currentDirectory = new DirectoryInfo(AppContext.BaseDirectory);

        while (currentDirectory != null)
        {
            if (File.Exists(Path.Combine(currentDirectory.FullName, "Reihitsu.sln")))
            {
                return Path.Combine(currentDirectory.FullName, "documentation", "rules");
            }

            currentDirectory = currentDirectory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the repository root.");
    }

    #endregion // Methods

    #region Tests

    /// <summary>
    /// Verifies that the general notes page exists and every rule page ends with the footer that links it
    /// </summary>
    [TestMethod]
    public void EveryRuleDocumentEndsWithGeneralNotesFooter()
    {
        Assert.IsTrue(File.Exists(Path.Combine(GetRuleDocumentationDirectory(), GeneralNotesFileName)), $"documentation/rules/{GeneralNotesFileName} is missing.");

        var failures = GetRuleDocumentPaths().Where(path => File.ReadLines(path).LastOrDefault(line => string.IsNullOrWhiteSpace(line) == false) != FooterLine)
                                             .Select(Path.GetFileName)
                                             .ToArray();

        Assert.IsEmpty(failures, $"The following rule documents do not end with the general-notes footer '{FooterLine}':{Environment.NewLine}{string.Join(Environment.NewLine, failures)}");
    }

    /// <summary>
    /// Verifies that no rule page mentions the formatter outside its code examples
    /// </summary>
    [TestMethod]
    public void RuleDocumentProseDoesNotMentionFormatter()
    {
        var failures = GetRuleDocumentPaths().SelectMany(path => GetProseLines(path).Where(line => _formatterMentionRegex.IsMatch(line.Text))
                                                                                    .Select(line => $"{Path.GetFileName(path)}:{line.LineNumber}: {line.Text}"))
                                             .ToArray();

        Assert.IsEmpty(failures, $"Rule documents describe the analyzer and its code fix only; the following lines mention the formatter:{Environment.NewLine}{string.Join(Environment.NewLine, failures)}");
    }

    #endregion // Tests
}