using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Regions;

/// <summary>
/// Regression tests for a region pair that straddles an element-body boundary: the opening
/// directive sits inside a body while the matching closing directive does not. Removing only the
/// half that sits inside the body left the other half orphaned, so formatting turned source that
/// parsed cleanly into source that no longer compiles (CS1028)
/// </summary>
[TestClass]
public class StraddlingRegionPairTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Asserts that formatting the given source keeps the region directives balanced and does not
    /// introduce a preprocessor diagnostic
    /// </summary>
    /// <param name="input">The source text to format</param>
    private static void AssertFormattedSourceKeepsDirectivesBalanced(string input)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var normalizedInput = NormalizeLineEndings(input, endOfLine);
            var endingName = DescribeLineEnding(endOfLine);

            Assert.IsEmpty(CSharpSyntaxTree.ParseText(normalizedInput).GetDiagnostics(),
                           $"The fixture itself must parse cleanly under {endingName} line endings.");

            var actual = ApplyRule(normalizedInput, endOfLine);

            var regionCount = actual.Split("#region").Length - 1;
            var endRegionCount = actual.Split("#endregion").Length - 1;

            Assert.AreEqual(regionCount,
                            endRegionCount,
                            $"Formatted output has unbalanced region directives under {endingName} line endings.");

            Assert.IsEmpty(CSharpSyntaxTree.ParseText(actual).GetDiagnostics(),
                           $"Formatted output no longer parses cleanly under {endingName} line endings.");
        }
    }

    /// <summary>
    /// Verifies that a region opened between an indexer declaration and its accessor list keeps its
    /// matching closing directive, so the formatted source still compiles
    /// </summary>
    [TestMethod]
    public void RegionStraddlingAnAccessorListKeepsBothDirectives()
    {
        // Arrange
        const string input = """
                             class C
                             {
                             #region Members

                                 public int this[int index]
                             #region Inner
                                 {
                                     get { return 0; }
                                 }
                             #endregion

                             #endregion
                             }
                             """;

        // Act & Assert
        AssertFormattedSourceKeepsDirectivesBalanced(input);
    }

    /// <summary>
    /// Verifies that a region opened between a method declaration and its body keeps its matching
    /// closing directive, so the formatted source still compiles
    /// </summary>
    [TestMethod]
    public void RegionStraddlingAMethodBodyKeepsBothDirectives()
    {
        // Arrange
        const string input = """
                             class C
                             {
                             #region Members

                                 public int M()
                             #region Inner
                                 {
                                     return 0;
                                 }
                             #endregion

                             #endregion
                             }
                             """;

        // Act & Assert
        AssertFormattedSourceKeepsDirectivesBalanced(input);
    }

    /// <summary>
    /// Verifies that a region fully contained in a method body is still removed
    /// </summary>
    [TestMethod]
    public void RegionFullyInsideAMethodBodyIsStillRemoved()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public void M()
                                 {
                             #region Inner
                                     var value = 1;
                             #endregion
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    public void M()
                                    {
                                        var value = 1;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}