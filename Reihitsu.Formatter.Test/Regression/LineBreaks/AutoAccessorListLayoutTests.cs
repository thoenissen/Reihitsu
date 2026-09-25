using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// An auto-accessor list of a property or indexer either stays on a single line, or is laid out over several lines
/// with every accessor starting its own line. Indexers collapse under the same conditions as auto-properties
/// </summary>
[TestClass]
public class AutoAccessorListLayoutTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that an auto-property whose interior comment prevents the collapse places each accessor on its own
    /// line, and that the accessor modifier starts the following accessor's line
    /// </summary>
    [TestMethod]
    public void InteriorCommentBeforeAccessorModifierLaysOutAccessors()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 public int Value { get; /* inner */ private set; }
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    public int Value
                                    {
                                        get; /* inner */
                                        private set;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an attributed auto-property whose interior comment prevents the single-line layout places each
    /// accessor on its own line
    /// </summary>
    [TestMethod]
    public void AttributedAutoAccessorsWithInteriorCommentStartOwnLines()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 public int Value { [System.Obsolete] get; /* inner */ set; }
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    public int Value
                                    {
                                        [System.Obsolete]
                                        get; /* inner */
                                        set;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that attributed auto accessors sharing a line in a multi-line accessor list each start their own line
    /// </summary>
    [TestMethod]
    public void AttributedAutoAccessorsSharingLineInMultiLineListStartOwnLines()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 public int Value
                                 {
                                     [System.Obsolete] get; set;
                                 }
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    public int Value
                                    {
                                        [System.Obsolete]
                                        get;
                                        set;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the attribute list of the second auto accessor in a multi-line accessor list starts that accessor's
    /// line instead of staying behind on the first accessor's line
    /// </summary>
    [TestMethod]
    public void AttributeOnSecondAutoAccessorInMultiLineListStartsTheAccessorLine()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 public int Value
                                 {
                                     get; [System.Obsolete] set;
                                 }
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    public int Value
                                    {
                                        get;
                                        [System.Obsolete]
                                        set;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment trailing a multi-line attributed accessor list stays after the closing brace while the
    /// accessors each start their own line
    /// </summary>
    [TestMethod]
    public void AttributedAutoAccessorsWithTrailingCommentInMultiLineListStartOwnLines()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 public int Value
                                 {
                                     [System.Obsolete] get; set;
                                 } // trailing
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    public int Value
                                    {
                                        [System.Obsolete]
                                        get;
                                        set;
                                    } // trailing
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an attributed auto-property spanning several lines with its opening brace on the signature line is
    /// laid out with each accessor on its own line instead of keeping a mixed layout
    /// </summary>
    [TestMethod]
    public void AttributedAutoAccessorsWithBraceOnSignatureLineAreLaidOut()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 public int Value { [System.Obsolete] get;
                                     set; }
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    public int Value
                                    {
                                        [System.Obsolete]
                                        get;
                                        set;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an attributed auto-property whose signature comment prevents a single line is laid out with each
    /// accessor on its own line, and that the comment is not crossed
    /// </summary>
    [TestMethod]
    public void AttributedAutoAccessorsWithSignatureCommentAreLaidOut()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 public int Value // note
                                 { [System.Obsolete] get; set; }
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    public int Value // note
                                    {
                                        [System.Obsolete]
                                        get;
                                        set;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a single-line auto-property with an attributed accessor stays on one line
    /// </summary>
    [TestMethod]
    public void SingleLineAttributedAutoPropertyRemainsUnchanged()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 public int Value { [System.Obsolete] get; set; }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a single-line auto-property whose second accessor carries an attribute stays on one line
    /// </summary>
    [TestMethod]
    public void SingleLineAutoPropertyWithAttributedSecondAccessorRemainsUnchanged()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 public int Value { get; [System.Obsolete] set; }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a comment trailing a single-line attributed auto-property does not force its accessors apart
    /// </summary>
    [TestMethod]
    public void SingleLineAttributedAutoPropertyWithTrailingCommentRemainsUnchanged()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 public int Value { [System.Obsolete] get; set; } // trailing
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a single-line attributed auto-property with an initializer stays on one line
    /// </summary>
    [TestMethod]
    public void SingleLineAttributedAutoPropertyWithInitializerRemainsUnchanged()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 public int Value { [System.Obsolete] get; set; } = 1;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a property-level attribute on its own line does not make a single-line attributed auto-property
    /// count as multi-line
    /// </summary>
    [TestMethod]
    public void SingleLineAttributedAutoPropertyBelowPropertyAttributeRemainsUnchanged()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 [System.Obsolete]
                                 public int Value { [System.Obsolete] get; set; }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a single-line auto indexer with an attributed accessor stays on one line
    /// </summary>
    [TestMethod]
    public void SingleLineAttributedAutoIndexerRemainsUnchanged()
    {
        // Arrange
        const string input = """
                             interface I
                             {
                                 int this[int index] { [System.Obsolete] get; set; }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a multi-line auto indexer whose accessors share a line collapses to a single line
    /// </summary>
    [TestMethod]
    public void MultiLineAutoIndexerCollapses()
    {
        // Arrange
        const string input = """
                             interface I
                             {
                                 int this[int index]
                                 {
                                     get; set;
                                 }
                             }
                             """;
        const string expected = """
                                interface I
                                {
                                    int this[int index] { get; set; }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an auto indexer whose accessor list spans two lines collapses to a single line
    /// </summary>
    [TestMethod]
    public void AutoIndexerWithClosingBraceOnAccessorLineCollapses()
    {
        // Arrange
        const string input = """
                             interface I
                             {
                                 int this[int index] { get;
                                     set; }
                             }
                             """;
        const string expected = """
                                interface I
                                {
                                    int this[int index] { get; set; }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a multi-line abstract auto indexer collapses to a single line and keeps its accessor modifier
    /// </summary>
    [TestMethod]
    public void MultiLineAbstractAutoIndexerWithAccessorModifierCollapses()
    {
        // Arrange
        const string input = """
                             abstract class C
                             {
                                 public abstract int this[int index]
                                 {
                                     get;
                                     protected set;
                                 }
                             }
                             """;
        const string expected = """
                                abstract class C
                                {
                                    public abstract int this[int index] { get; protected set; }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an auto indexer whose signature comment prevents the collapse places each accessor on its own line,
    /// and that the comment is not crossed
    /// </summary>
    [TestMethod]
    public void AutoIndexerWithSignatureCommentIsLaidOut()
    {
        // Arrange
        const string input = """
                             interface I
                             {
                                 int this[int index] // note
                                 { get; set; }
                             }
                             """;
        const string expected = """
                                interface I
                                {
                                    int this[int index] // note
                                    {
                                        get;
                                        set;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an auto indexer whose parameter list spans several lines is laid out with each accessor on its own
    /// line, because its signature cannot share a single line with the accessor list
    /// </summary>
    [TestMethod]
    public void AutoIndexerWithMultiLineParameterListIsLaidOut()
    {
        // Arrange
        const string input = """
                             interface I
                             {
                                 int this[int row,
                                          int column] { get; set; }
                             }
                             """;
        const string expected = """
                                interface I
                                {
                                    int this[int row,
                                             int column]
                                    {
                                        get;
                                        set;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an auto indexer whose interior comment prevents the collapse places each accessor on its own line
    /// </summary>
    [TestMethod]
    public void AutoIndexerWithInteriorCommentIsLaidOut()
    {
        // Arrange
        const string input = """
                             interface I
                             {
                                 int this[int index] { get; /* inner */ set; }
                             }
                             """;
        const string expected = """
                                interface I
                                {
                                    int this[int index]
                                    {
                                        get; /* inner */
                                        set;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an event accessor list without bodies keeps its existing layout
    /// </summary>
    [TestMethod]
    public void AutoEventAccessorListRemainsUnchanged()
    {
        // Arrange
        const string input = """
                             interface I
                             {
                                 event System.EventHandler Changed
                                 {
                                     add; remove;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    #endregion // Methods
}