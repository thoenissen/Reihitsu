using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Regions;

/// <summary>
/// Tests covering region directives that sit inside an inactive conditional compilation branch.
/// The naming rewrite stays, because RH7301 and RH7302 report those directives too, but the layout
/// rewrites do not: the blank lines they insert land inside the surrounding disabled text, where the
/// "is there already a blank line" query cannot see them, so every run inserted another one
/// </summary>
[TestClass]
public class DisabledRegionLayoutTests : FormatterTestsBase
{
    #region Fields

    /// <summary>
    /// Parse options that activate the <c>DEBUG</c> branch of a fixture
    /// </summary>
    private static readonly CSharpParseOptions _withDebug = new CSharpParseOptions().WithPreprocessorSymbols("DEBUG");

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Verifies that a canonically named region inside disabled code survives formatting byte for byte
    /// </summary>
    [TestMethod]
    public void CanonicalRegionInsideDisabledCodeIsUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                             #if false
                                 #region Test

                                 void M()
                                 {
                                 }

                                 #endregion // Test
                             #endif
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a region inside disabled code is renamed without being padded with blank lines
    /// </summary>
    [TestMethod]
    public void RegionInsideDisabledCodeIsRenamedWithoutBlankLinePadding()
    {
        // Arrange
        const string input = """
                             class C
                             {
                             #if false
                                 #region test

                                 void M()
                                 {
                                 }

                                 #endregion
                             #endif
                             }
                             """;
        const string expected = """
                                class C
                                {
                                #if false
                                    #region Test

                                    void M()
                                    {
                                    }

                                    #endregion // Test
                                #endif
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a region inside disabled code with no blank lines at all stays that way
    /// </summary>
    [TestMethod]
    public void EmptyRegionInsideDisabledCodeKeepsItsLineCount()
    {
        // Arrange
        const string input = """
                             class C
                             {
                             #if false
                                 #region Test
                                 #endregion // Test
                             #endif
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a region inside a method body in disabled code is not removed, although the
    /// same region in active code would be
    /// </summary>
    [TestMethod]
    public void RegionInsideDisabledMethodBodyIsNotRemoved()
    {
        // Arrange
        const string input = """
                             class C
                             {
                             #if false
                                 void M()
                                 {
                                     #region Test

                                     var x = 1;

                                     #endregion // Test
                                 }
                             #endif
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a region inside an active conditional branch is still padded with blank lines —
    /// the other side of the boundary the <c>IsActive</c> guard is taken on
    /// </summary>
    [TestMethod]
    public void RegionInsideActiveConditionalIsStillFormatted()
    {
        // Arrange
        const string input = """
                             class C
                             {
                             #if DEBUG
                                 #region test
                                 void M()
                                 {
                                 }
                                 #endregion
                             #endif
                             }
                             """;
        const string expected = """
                                class C
                                {
                                #if DEBUG
                                    #region Test

                                    void M()
                                    {
                                    }

                                    #endregion // Test

                                #endif
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected, _withDebug);
    }

    /// <summary>
    /// Verifies that a region in unconditional code is still padded with blank lines
    /// </summary>
    [TestMethod]
    public void RegionInUnconditionalCodeIsStillFormatted()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 #region test
                                 void M()
                                 {
                                 }
                                 #endregion
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    #region Test

                                    void M()
                                    {
                                    }

                                    #endregion // Test
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}