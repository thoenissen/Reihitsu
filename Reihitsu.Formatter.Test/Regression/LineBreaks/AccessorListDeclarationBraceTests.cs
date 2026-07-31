using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Regression tests for issue #581: the accessor-list opening brace of an indexer or event
/// declaration must be moved to its own line by the first formatting pass, even when the accessor
/// bodies are re-flowed during the same pass. No expression body is involved in these scenarios,
/// so they pin the accessor-list brace placement itself rather than the expression-body transform
/// </summary>
[TestClass]
public class AccessorListDeclarationBraceTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a hand-written indexer whose accessor body is re-flowed in the same pass still
    /// gets its accessor-list opening brace moved to its own line
    /// </summary>
    [TestMethod]
    public void IndexerBraceMovesWhenAccessorBodyIsReflowed()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private readonly int[] _items = [1, 2, 3];

                                 public int this[int index] {
                                     get { return _items[index]; }
                                 }
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
    /// Verifies that an indexer whose accessor body already needs no change still gets exactly one
    /// line break before the accessor-list opening brace
    /// </summary>
    [TestMethod]
    public void IndexerBraceMovesWhenAccessorBodyNeedsNoChange()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private readonly int[] _items = [1, 2, 3];

                                 public int this[int index] {
                                     get
                                     {
                                         return _items[index];
                                     }
                                 }
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
    /// Verifies that an event declaration whose accessor bodies are re-flowed in the same pass gets
    /// its accessor-list opening brace moved to its own line
    /// </summary>
    [TestMethod]
    public void EventBraceMovesWhenAccessorBodiesAreReflowed()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private EventHandler _handler;

                                 public event EventHandler Changed {
                                     add { _handler += value; }
                                     remove { _handler -= value; }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private EventHandler _handler;

                                    public event EventHandler Changed
                                    {
                                        add
                                        {
                                            _handler += value;
                                        }
                                        remove
                                        {
                                            _handler -= value;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an event declaration that is already laid out correctly stays untouched
    /// </summary>
    [TestMethod]
    public void FormattedEventRemainsUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private EventHandler _handler;

                                 public event EventHandler Changed
                                 {
                                     add
                                     {
                                         _handler += value;
                                     }

                                     remove
                                     {
                                         _handler -= value;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a single-line auto-accessor indexer stays on the declaration line
    /// </summary>
    [TestMethod]
    public void SingleLineAutoAccessorIndexerRemainsUnchanged()
    {
        // Arrange
        const string input = """
                             interface IValues
                             {
                                 int this[string key] { get; set; }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a multi-line auto-accessor indexer keeps its existing layout, including the
    /// opening brace on the declaration line
    /// </summary>
    [TestMethod]
    public void MultiLineAutoAccessorIndexerRemainsUnchanged()
    {
        // Arrange
        const string input = """
                             interface IValues
                             {
                                 int this[int index] {
                                     get;
                                     set;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    #endregion // Methods
}