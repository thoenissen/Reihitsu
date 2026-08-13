using System;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.ArchitectureTests.General;

/// <summary>
/// Guards that the two production paths which rewrite <c>///</c> exteriors stay free of regular expressions.
/// Both once normalized documentation prefixes through <see cref="System.Text.RegularExpressions.Regex"/> calls
/// carrying a wall-clock match timeout, so whether they succeeded depended on process scheduling rather than on
/// their input. An output assertion cannot catch a reintroduced regular expression — a generous match timeout
/// produces byte-identical text right up to the moment CI load trips it — which is why this guard reads the
/// sources instead
/// </summary>
[TestClass]
public sealed class DocumentationCommentNormalizationDeterminismTests
{
    #region Constants

    /// <summary>
    /// Production files that must normalize documentation prefixes without a regular expression
    /// </summary>
    private static readonly string[] _guardedPaths = [
                                                         Path.Combine("Reihitsu.Core", "DocumentationCommentUtilities.cs"),
                                                         Path.Combine("Reihitsu.Formatter", "Pipeline", "DocumentationComments", "DocumentationCommentFormattingPhase.cs"),
                                                         Path.Combine("Reihitsu.Analyzer.CodeFixes", "Rules", "Documentation", "RH8401SingleLineCommentsMustNotUseDocumentationStyleSlashesCodeFixProvider.cs")
                                                     ];

    #endregion // Constants

    #region Tests

    /// <summary>
    /// Verifies that no documentation-prefix normalization path references the regular-expression namespace
    /// </summary>
    [TestMethod]
    public void DocumentationPrefixNormalizationPathsUseNoRegularExpressions()
    {
        // Arrange
        var repositoryRoot = FindRepositoryRoot();

        // Act and assert
        foreach (var relativePath in _guardedPaths)
        {
            var fullPath = Path.Combine(repositoryRoot, relativePath);

            Assert.IsTrue(File.Exists(fullPath), $"Guarded file '{relativePath}' was not found. Update the guard when a path moves.");

            var source = File.ReadAllText(fullPath);

            Assert.IsFalse(source.Contains("System.Text.RegularExpressions", StringComparison.Ordinal),
                           $"'{relativePath}' references System.Text.RegularExpressions. Documentation-prefix normalization must stay a deterministic scan whose termination depends only on input length.");

            Assert.IsFalse(source.Contains("Regex.", StringComparison.Ordinal),
                           $"'{relativePath}' calls into Regex. Documentation-prefix normalization must stay a deterministic scan whose termination depends only on input length.");
        }
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Locates the repository root by walking up from the test assembly location
    /// </summary>
    /// <returns>The repository root directory</returns>
    /// <exception cref="DirectoryNotFoundException">The repository root could not be located</exception>
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