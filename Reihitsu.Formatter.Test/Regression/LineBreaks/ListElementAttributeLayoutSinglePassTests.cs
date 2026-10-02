using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Indexer, argument, and parameter lists whose elements change their line span through attribute formatting settle
/// in a single formatter pass: the list layout decision sees the elements as attribute formatting emits them
/// </summary>
[TestClass]
public class ListElementAttributeLayoutSinglePassTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that an indexer argument whose lambda parameter carries split attribute lists is merged and collapsed in a single pass
    /// </summary>
    [TestMethod]
    public void IndexerLambdaParameterWithSplitAttributeListsSettlesInOnePass()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var v = this[([A]
                                                   [B] int x) => x,
                                                  1];
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var v = this[([A, B] int x) => x, 1];
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an indexer argument whose lambda parameter attribute list only needs to move onto the parameter's line is collapsed in a single pass
    /// </summary>
    [TestMethod]
    public void IndexerLambdaParameterAttributePlacementSettlesInOnePass()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var v = this[([A]
                                                   int x) => x,
                                                  1];
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var v = this[([A] int x) => x, 1];
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an indexer list is still collapsed when a lambda attribute list only opens its own line afterwards
    /// </summary>
    [TestMethod]
    public void IndexerLambdaAttributeOpeningLineKeepsCollapsedList()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var v = this[[A] x => x,
                                                  1];
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var v = this[[A]
                                                     x => x, 1];
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an argument list is split in a single pass when a lambda attribute list moves onto its own line
    /// </summary>
    [TestMethod]
    public void ArgumentLambdaAttributeSplitsArgumentsInOnePass()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     N([A] x => x, 1);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        N([A]
                                          x => x,
                                          1);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an argument list is split in a single pass when a lambda's merged attribute list is split into one list per line
    /// </summary>
    [TestMethod]
    public void ArgumentLambdaMergedAttributeListSplitsArgumentsInOnePass()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     N([A, B] x => x, 1);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        N([A]
                                          [B]
                                          x => x,
                                          1);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a minimal-API style call with an attributed lambda settles in a single pass
    /// </summary>
    [TestMethod]
    public void MinimalApiAttributedLambdaSettlesInOnePass()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     app.MapGet("/", [Authorize] () => "x");
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        app.MapGet("/",
                                                   [Authorize]
                                                   () => "x");
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an argument whose attribute lists stay on one line after attribute formatting does not split the argument list
    /// </summary>
    [TestMethod]
    public void ArgumentLambdaParameterAttributeOnOneLineKeepsArgumentsOnOneLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     N(([A] int x) => x, 1);
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a single attributed lambda argument keeps its list unsplit, because there is no separator to break
    /// </summary>
    [TestMethod]
    public void SingleArgumentLambdaAttributeKeepsSingleArgumentLayout()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     N([A] x => x);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        N([A]
                                          x => x);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a parameter list is split in a single pass when a lambda default value's attribute list moves onto its own line
    /// </summary>
    [TestMethod]
    public void ParameterDefaultLambdaAttributeSplitsParametersInOnePass()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(Func<int, int> f = [A] x => x, int y = 0)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(Func<int, int> f = [A]
                                           x => x,
                                           int y = 0)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a parameter list split because of split attribute lists stays split after the lists are merged
    /// </summary>
    [TestMethod]
    public void ParameterListSplitBySplitAttributeListsStaysSplit()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M([A]
                                        [B] int x, int y)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M([A, B] int x,
                                           int y)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that record positional parameters with split attribute lists keep one parameter per line
    /// </summary>
    [TestMethod]
    public void RecordParametersWithSplitAttributeListsStayOnePerLine()
    {
        // Arrange
        const string input = """
                             record Example([A]
                                            [B]
                                            int T,
                                            [A]
                                            [B]
                                            int U);
                             """;
        const string expected = """
                                record Example([A, B] int T,
                                               [A, B] int U);
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}