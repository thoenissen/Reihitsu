using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Structural;

/// <summary>
/// Tests verifying that expression-bodied indexers stay expression-bodied and that their arrow and the first
/// expression token are laid out on the line of the closing bracket, like expression-bodied properties
/// </summary>
[TestClass]
public class ExpressionBodiedIndexerTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that an expression-bodied indexer is kept
    /// </summary>
    [TestMethod]
    public void ExpressionBodiedIndexerRemainsUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private readonly int[] _items = [1, 2, 3];

                                 public int this[int index] => _items[index];
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an explicitly implemented expression-bodied indexer is kept
    /// </summary>
    [TestMethod]
    public void ExplicitInterfaceExpressionBodiedIndexerRemainsUnchanged()
    {
        // Arrange
        const string input = """
                             interface IValues
                             {
                                 int this[int index] { get; }
                             }

                             class C : IValues
                             {
                                 private readonly int[] _items = [1, 2, 3];

                                 int IValues.this[int index] => _items[index];
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an indexer with a throw-expression body is kept
    /// </summary>
    [TestMethod]
    public void ThrowExpressionIndexerRemainsUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public int this[int index] => throw new NotSupportedException();
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an arrow on its own line is joined onto the line of the closing bracket
    /// </summary>
    [TestMethod]
    public void ArrowOnOwnLineJoinsClosingBracketLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private readonly int[] _items = [1, 2, 3];

                                 public int this[int index]
                                     => _items[index];
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private readonly int[] _items = [1, 2, 3];

                                    public int this[int index] => _items[index];
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an expression on its own line after the arrow is joined onto the line of the closing bracket
    /// </summary>
    [TestMethod]
    public void ExpressionOnOwnLineJoinsClosingBracketLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private readonly int[] _items = [1, 2, 3];

                                 public int this[int index] =>
                                     _items[index];
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private readonly int[] _items = [1, 2, 3];

                                    public int this[int index] => _items[index];
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an inline comment between the arrow and the expression stays in place
    /// </summary>
    [TestMethod]
    public void InlineCommentAfterArrowRemainsInPlace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private readonly int[] _items = [1, 2, 3];

                                 public int this[int index] => /* inline */ _items[index];
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a line comment after the closing bracket keeps the arrow on its own line, because joining
    /// the arrow would move it into the comment
    /// </summary>
    [TestMethod]
    public void LineCommentAfterClosingBracketKeepsArrowOnOwnLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private readonly int[] _items = [1, 2, 3];

                                 public int this[int index] // trailing
                                     => _items[index]; // after
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that accessor-level expression bodies are left alone
    /// </summary>
    [TestMethod]
    public void AccessorLevelExpressionBodiesRemainUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private readonly int[] _items = [1, 2, 3];

                                 public int this[long index]
                                 {
                                     get => _items[(int)index];
                                     set => _items[(int)index] = value;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an expression-bodied property is still not converted to a block body
    /// </summary>
    [TestMethod]
    public void ExpressionBodiedPropertyRemainsUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private readonly int _value = 1;

                                 public int Value => _value;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an expression-bodied indexer inside an active conditional directive branch is kept while the
    /// directives stay in column zero
    /// </summary>
    [TestMethod]
    public void IndexerInsideActiveDirectiveBranchRemainsUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private readonly int[] _items = [1, 2, 3];

                             #if true
                                 public int this[int index] => _items[index];
                             #endif
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an indexer inside disabled text is left byte-identical
    /// </summary>
    [TestMethod]
    public void IndexerInsideDisabledTextRemainsUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                             #if false
                                 public int this[string key] {
                                     get { return 0; }
                                 }
                             #endif
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    #endregion // Methods
}