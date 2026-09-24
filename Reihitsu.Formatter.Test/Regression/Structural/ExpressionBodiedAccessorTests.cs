using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Structural;

/// <summary>
/// Tests verifying that single-statement <c>get</c>, <c>set</c>, and <c>init</c> accessor blocks of properties and
/// indexers are converted to expression bodies, and that accessors whose statement shape or trivia cannot be
/// transferred safely keep their blocks
/// </summary>
[TestClass]
public class ExpressionBodiedAccessorTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a property's single-statement get and set accessor blocks are converted to expression bodies
    /// </summary>
    [TestMethod]
    public void PropertyGetAndSetAccessorsConvertToExpressionBodies()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private string _name;

                                 public string Name
                                 {
                                     get
                                     {
                                         return _name;
                                     }
                                     set
                                     {
                                         _name = value;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private string _name;

                                    public string Name
                                    {
                                        get => _name;
                                        set => _name = value;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an indexer's single-statement get and set accessor blocks are converted to expression bodies
    /// </summary>
    [TestMethod]
    public void IndexerGetAndSetAccessorsConvertToExpressionBodies()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private string[] _values;

                                 public string this[int index]
                                 {
                                     get
                                     {
                                         return _values[index];
                                     }
                                     set
                                     {
                                         _values[index] = value;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private string[] _values;

                                    public string this[int index]
                                    {
                                        get => _values[index];
                                        set => _values[index] = value;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a single-statement init accessor block is converted to an expression body
    /// </summary>
    [TestMethod]
    public void InitAccessorConvertsToExpressionBody()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x;

                                 public int X
                                 {
                                     get
                                     {
                                         return _x;
                                     }
                                     init
                                     {
                                         _x = value;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _x;

                                    public int X
                                    {
                                        get => _x;
                                        init => _x = value;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a getter whose only statement throws is converted to a throw expression body
    /// </summary>
    [TestMethod]
    public void GetterThrowStatementConvertsToThrowExpression()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public int X
                                 {
                                     get
                                     {
                                         throw new System.NotSupportedException();
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    public int X
                                    {
                                        get => throw new System.NotSupportedException();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that setter and init accessors whose only statement throws are converted to throw expression bodies
    /// </summary>
    [TestMethod]
    public void SetterAndInitThrowStatementsConvertToThrowExpressions()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public int X
                                 {
                                     get
                                     {
                                         return 0;
                                     }
                                     set
                                     {
                                         throw new System.NotSupportedException();
                                     }
                                 }

                                 public int Y
                                 {
                                     get
                                     {
                                         return 0;
                                     }
                                     init
                                     {
                                         throw new System.NotSupportedException();
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    public int X
                                    {
                                        get => 0;
                                        set => throw new System.NotSupportedException();
                                    }

                                    public int Y
                                    {
                                        get => 0;
                                        init => throw new System.NotSupportedException();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that each accessor is decided independently, so an eligible getter converts while a setter with several statements keeps its block
    /// </summary>
    [TestMethod]
    public void OnlyEligibleAccessorConvertsWhenSiblingHasSeveralStatements()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x;

                                 public int X
                                 {
                                     get
                                     {
                                         return _x;
                                     }
                                     set
                                     {
                                         _x = value;
                                         OnChanged();
                                     }
                                 }

                                 private void OnChanged()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _x;

                                    public int X
                                    {
                                        get => _x;
                                        set
                                        {
                                            _x = value;
                                            OnChanged();
                                        }
                                    }

                                    private void OnChanged()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a single-line accessor block converts and receives the layout of a hand-written expression-bodied accessor
    /// </summary>
    [TestMethod]
    public void SingleLineAccessorBlockConverts()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x;

                                 public int X { get { return _x; } }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _x;

                                    public int X
                                    {
                                        get => _x;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a single statement spanning several lines converts and its continuation lines are realigned to the expression
    /// </summary>
    [TestMethod]
    public void MultiLineStatementConvertsAndRealignsContinuation()
    {
        // Arrange
        const string input = """
                             using System.Linq;

                             class C
                             {
                                 private int[] _items;

                                 public int[] Positive
                                 {
                                     get
                                     {
                                         return _items.Where(x => x > 0)
                                                      .ToArray();
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                using System.Linq;

                                class C
                                {
                                    private int[] _items;

                                    public int[] Positive
                                    {
                                        get => _items.Where(x => x > 0)
                                                     .ToArray();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a getter returning by reference keeps the <c>ref</c> in its expression body
    /// </summary>
    [TestMethod]
    public void RefReturnGetterConvertsToRefExpressionBody()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x;

                                 public ref int X
                                 {
                                     get
                                     {
                                         return ref _x;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _x;

                                    public ref int X
                                    {
                                        get => ref _x;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that accessor attributes and accessibility modifiers survive the conversion unchanged
    /// </summary>
    [TestMethod]
    public void AccessorAttributesAndModifiersArePreserved()
    {
        // Arrange
        const string input = """
                             using System.Diagnostics;

                             class C
                             {
                                 private int _x;

                                 public int X
                                 {
                                     [DebuggerStepThrough]
                                     get
                                     {
                                         return _x;
                                     }
                                     private set
                                     {
                                         _x = value;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                using System.Diagnostics;

                                class C
                                {
                                    private int _x;

                                    public int X
                                    {
                                        [DebuggerStepThrough]
                                        get => _x;
                                        private set => _x = value;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that accessors of explicit interface implementations and interface default implementations convert
    /// </summary>
    [TestMethod]
    public void ExplicitInterfaceAndInterfaceDefaultAccessorsConvert()
    {
        // Arrange
        const string input = """
                             interface IValue
                             {
                                 int Value { get; }

                                 int Doubled
                                 {
                                     get
                                     {
                                         return Value * 2;
                                     }
                                 }
                             }

                             class C : IValue
                             {
                                 int IValue.Value
                                 {
                                     get
                                     {
                                         return 1;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                interface IValue
                                {
                                    int Value { get; }

                                    int Doubled
                                    {
                                        get => Value * 2;
                                    }
                                }

                                class C : IValue
                                {
                                    int IValue.Value
                                    {
                                        get => 1;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment trailing the statement's semicolon stays behind the semicolon of the converted accessor
    /// </summary>
    [TestMethod]
    public void TrailingCommentAfterStatementMovesToNewSemicolon()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x;

                                 public int X
                                 {
                                     get
                                     {
                                         return _x; // cached
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _x;

                                    public int X
                                    {
                                        get => _x; // cached
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a single-line comment trailing the statement's semicolon keeps the accessor block when the closing
    /// brace does not end its line, because the comment would otherwise swallow the code that follows the brace
    /// </summary>
    [TestMethod]
    public void TrailingLineCommentKeepsBlockWhenClosingBraceSharesLineWithNextAccessor()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x;

                                 public int X
                                 {
                                     get
                                     {
                                         return _x; // cached
                                     } set
                                     {
                                         _x = value;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _x;

                                    public int X
                                    {
                                        get
                                        {
                                            return _x; // cached
                                        } set => _x = value;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment trailing the accessor's closing brace follows the semicolon of the converted accessor
    /// </summary>
    [TestMethod]
    public void TrailingCommentAfterClosingBraceMovesToNewSemicolon()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x;

                                 public int X
                                 {
                                     set
                                     {
                                         _x = value;
                                     } /* note */
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _x;

                                    public int X
                                    {
                                        set => _x = value; /* note */
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a block comment between the expression and the semicolon stays between them
    /// </summary>
    [TestMethod]
    public void BlockCommentBeforeSemicolonTravelsWithExpression()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x;

                                 public int X
                                 {
                                     get
                                     {
                                         return _x /* why */;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _x;

                                    public int X
                                    {
                                        get => _x /* why */;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a balanced conditional group wholly inside the expression travels with it. The disabled branch keeps
    /// its column while the active one is realigned, exactly as the pipeline treats the equivalent hand-written
    /// expression-bodied accessor
    /// </summary>
    [TestMethod]
    public void BalancedConditionalInsideExpressionConverts()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public int X
                                 {
                                     get
                                     {
                                         return Compute(
                             #if DEBUG
                                                        1
                             #else
                                                        2
                             #endif
                                                       );
                                     }
                                 }

                                 private static int Compute(int value)
                                 {
                                     return value;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    public int X
                                    {
                                        get => Compute(
                                #if DEBUG
                                                           1
                                #else
                                                       2
                                #endif
                                                       );
                                    }

                                    private static int Compute(int value)
                                    {
                                        return value;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a balanced conditional group wholly inside the expression travels with it when the active branch is
    /// the first one, which is then the branch that is realigned
    /// </summary>
    [TestMethod]
    public void BalancedConditionalInsideExpressionConvertsWhenSymbolIsDefined()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public int X
                                 {
                                     get
                                     {
                                         return Compute(
                             #if DEBUG
                                                        1
                             #else
                                                        2
                             #endif
                                                       );
                                     }
                                 }

                                 private static int Compute(int value)
                                 {
                                     return value;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    public int X
                                    {
                                        get => Compute(
                                #if DEBUG
                                                       1
                                #else
                                                           2
                                #endif
                                                       );
                                    }

                                    private static int Compute(int value)
                                    {
                                        return value;
                                    }
                                }
                                """;
        var parseOptions = CSharpParseOptions.Default.WithPreprocessorSymbols("DEBUG");

        // Act & Assert
        AssertRuleResult(input, expected, parseOptions);
    }

    /// <summary>
    /// Verifies that an own-line comment above the statement keeps the accessor block, because converting would move it relative to the code
    /// </summary>
    [TestMethod]
    public void OwnLineCommentBeforeStatementKeepsBlock()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x;

                                 public int X
                                 {
                                     get
                                     {
                                         // cached value
                                         return _x;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a comment between the statement and the closing brace keeps the accessor block
    /// </summary>
    [TestMethod]
    public void CommentBeforeClosingBraceKeepsBlock()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x;

                                 public int X
                                 {
                                     set
                                     {
                                         _x = value;

                                         // raise nothing here
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a comment trailing the opening brace keeps the accessor block
    /// </summary>
    [TestMethod]
    public void CommentAfterOpeningBraceKeepsBlock()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x;

                                 public int X
                                 {
                                     get
                                     { // cached
                                         return _x;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a comment trailing the accessor keyword keeps the accessor block
    /// </summary>
    [TestMethod]
    public void CommentAfterAccessorKeywordKeepsBlock()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x;

                                 public int X
                                 {
                                     get // cached
                                     {
                                         return _x;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a comment between the <c>return</c> keyword and the returned expression keeps the accessor block,
    /// because the keyword it follows is removed by the conversion
    /// </summary>
    [TestMethod]
    public void CommentAfterReturnKeywordKeepsBlock()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x;

                                 public int X
                                 {
                                     get
                                     {
                                         return /* cached */ _x;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that comments trailing both the statement and the closing brace keep the accessor block, because they cannot share one semicolon
    /// </summary>
    [TestMethod]
    public void CommentsAfterStatementAndClosingBraceKeepBlock()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x;

                                 public int X
                                 {
                                     get
                                     {
                                         return _x; // first
                                     } // second
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a single-line comment between the expression and the semicolon keeps the accessor block
    /// </summary>
    [TestMethod]
    public void LineCommentBeforeSemicolonKeepsBlock()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x;

                                 public int X
                                 {
                                     get
                                     {
                                         return _x // cached
                                         ;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a documentation comment inside the accessor body keeps the accessor block
    /// </summary>
    [TestMethod]
    public void DocumentationCommentInsideBodyKeepsBlock()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x;

                                 public int X
                                 {
                                     get
                                     {
                                         /// <summary>
                                         /// Value
                                         /// </summary>
                                         return _x;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a conditional group around the statement keeps the accessor block
    /// </summary>
    [TestMethod]
    public void ConditionalAroundStatementKeepsBlock()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x;

                                 public int X
                                 {
                                     get
                                     {
                             #if DEBUG
                                         return _x;
                             #else
                                         return 0;
                             #endif
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a pragma directive before the statement keeps the accessor block
    /// </summary>
    [TestMethod]
    public void PragmaBeforeStatementKeepsBlock()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x;

                                 public int X
                                 {
                                     get
                                     {
                             #pragma warning disable CS0618
                                         return _x;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a region directive inside the accessor body keeps the accessor block
    /// </summary>
    [TestMethod]
    public void RegionInsideBodyKeepsBlock()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x;

                                 public int X
                                 {
                                     get
                                     {
                                         #region Value

                                         return _x;

                                         #endregion // Value
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a conditional group that opens inside the expression and closes outside the body keeps the accessor block
    /// </summary>
    [TestMethod]
    public void ConditionalStartingInsideExpressionAndEndingOutsideBodyKeepsBlock()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public int X
                                 {
                                     get
                                     {
                                         return Compute(
                             #if DEBUG
                                                        1);
                                     }
                                 }
                             #else
                                                        2);
                                     }
                                 }
                             #endif

                                 private static int Compute(int value)
                                 {
                                     return value;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an accessor body with two statements keeps its block
    /// </summary>
    [TestMethod]
    public void TwoStatementsKeepBlock()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x;

                                 public int X
                                 {
                                     get
                                     {
                                         var x = _x;

                                         return x;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an empty setter body keeps its block
    /// </summary>
    [TestMethod]
    public void EmptySetterKeepsBlock()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public int X
                                 {
                                     get => 0;
                                     set
                                     {
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a getter whose only statement is an expression statement keeps its block, because it has no value to return
    /// </summary>
    [TestMethod]
    public void GetterExpressionStatementKeepsBlock()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public int X
                                 {
                                     get
                                     {
                                         Compute();
                                     }
                                 }

                                 private static void Compute()
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a getter whose only statement is a rethrow without an operand keeps its block
    /// </summary>
    [TestMethod]
    public void GetterRethrowKeepsBlock()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public int X
                                 {
                                     get
                                     {
                                         throw;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an iterator getter whose only statement is a yield return keeps its block
    /// </summary>
    [TestMethod]
    public void GetterYieldReturnKeepsBlock()
    {
        // Arrange
        const string input = """
                             using System.Collections.Generic;

                             class C
                             {
                                 public IEnumerable<int> Values
                                 {
                                     get
                                     {
                                         yield return 1;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a setter whose only statement is a return keeps its block
    /// </summary>
    [TestMethod]
    public void SetterReturnKeepsBlock()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public int X
                                 {
                                     get => 0;
                                     set
                                     {
                                         return;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a setter whose only statement is a local declaration keeps its block
    /// </summary>
    [TestMethod]
    public void SetterDeclarationKeepsBlock()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public int X
                                 {
                                     get => 0;
                                     set
                                     {
                                         var ignored = value;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a setter whose only statement is a compound statement keeps its block
    /// </summary>
    [TestMethod]
    public void SetterIfStatementKeepsBlock()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x;

                                 public int X
                                 {
                                     get => _x;
                                     set
                                     {
                                         if (value > 0)
                                         {
                                             _x = value;
                                         }
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that event add and remove accessors keep their blocks
    /// </summary>
    [TestMethod]
    public void EventAccessorsKeepBlocks()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 private EventHandler _changed;

                                 public event EventHandler Changed
                                 {
                                     add
                                     {
                                         _changed += value;
                                     }
                                     remove
                                     {
                                         _changed -= value;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that accessors which already use expression bodies are left as they are
    /// </summary>
    [TestMethod]
    public void ExpressionBodiedAccessorsStayUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x;

                                 public int X
                                 {
                                     get => _x;
                                     set => _x = value;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    #endregion // Methods
}