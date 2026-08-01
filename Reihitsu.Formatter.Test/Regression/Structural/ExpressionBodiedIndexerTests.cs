using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Structural;

/// <summary>
/// Regression tests for issue #581: converting an expression-bodied indexer must produce the final
/// layout in a single formatting pass, including the accessor-list opening brace
/// </summary>
[TestClass]
public class ExpressionBodiedIndexerTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that an expression-bodied indexer is converted to a get accessor and that the
    /// generated accessor-list opening brace is placed on its own line by the first pass
    /// </summary>
    [TestMethod]
    public void IndexerConvertsToGetAccessorWithBraceOnOwnLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private readonly int[] _items = [1, 2, 3];

                                 public int this[int index] => _items[index];
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private readonly int[] _items = [1, 2, 3];

                                    public int this[int index]
                                    {
                                        get
                                        {
                                            return _items[index];
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an explicitly implemented expression-bodied indexer is converted with the
    /// accessor-list opening brace on its own line
    /// </summary>
    [TestMethod]
    public void ExplicitInterfaceIndexerConvertsWithBraceOnOwnLine()
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
        const string expected = """
                                interface IValues
                                {
                                    int this[int index] { get; }
                                }

                                class C : IValues
                                {
                                    private readonly int[] _items = [1, 2, 3];

                                    int IValues.this[int index]
                                    {
                                        get
                                        {
                                            return _items[index];
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an expression-bodied indexer throwing an exception is converted to a throw
    /// statement with the accessor-list opening brace on its own line
    /// </summary>
    [TestMethod]
    public void ThrowExpressionIndexerConvertsToThrowStatement()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public int this[int index] => throw new NotSupportedException();
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    public int this[int index]
                                    {
                                        get
                                        {
                                            throw new NotSupportedException();
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment between the arrow and the expression stays attached to the moved
    /// opening brace, matching the expression-bodied method counterpart
    /// </summary>
    [TestMethod]
    public void InlineCommentAfterArrowTrailsMovedOpenBrace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private readonly int[] _items = [1, 2, 3];

                                 public int this[int index] => /* inline */ _items[index];
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private readonly int[] _items = [1, 2, 3];

                                    public int this[int index]
                                    {/* inline */
                                        get
                                        {
                                            return _items[index];
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an arrow already placed on its own line produces exactly one line break before
    /// the opening brace and keeps the semicolon's trailing comment on the closing brace
    /// </summary>
    [TestMethod]
    public void ArrowOnOwnLineDoesNotDoubleTheLineBreak()
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
        const string expected = """
                                class C
                                {
                                    private readonly int[] _items = [1, 2, 3];

                                    public int this[int index] // trailing
                                    {
                                        get
                                        {
                                            return _items[index];
                                        }
                                    } // after
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
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
    /// Verifies that an indexer inside an active conditional directive branch is converted with the
    /// opening brace on its own line while the directives stay in column zero
    /// </summary>
    [TestMethod]
    public void IndexerInsideActiveDirectiveBranchConvertsWithBraceOnOwnLine()
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
        const string expected = """
                                class C
                                {
                                    private readonly int[] _items = [1, 2, 3];

                                #if true
                                    public int this[int index]
                                    {
                                        get
                                        {
                                            return _items[index];
                                        }
                                    }
                                #endif
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
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