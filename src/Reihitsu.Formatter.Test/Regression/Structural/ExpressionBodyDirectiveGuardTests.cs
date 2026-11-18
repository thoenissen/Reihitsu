using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Structural;

/// <summary>
/// Regression tests for expression-bodied members carrying a preprocessor directive. A directive between the arrow and the
/// expression refuses the input as unsupported, because a converted member would host it directly after the generated
/// <c>return</c>, where a directive cannot sit. A directive that follows the expression travels inside the generated statement
/// and is still converted, and a region around the arrow or behind the expression keeps the expression body
/// </summary>
[TestClass]
public class ExpressionBodyDirectiveGuardTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a directive between the arrow and the expression of each kind of member the formatter converts, and of an
    /// indexer, refuses the input as unsupported in every build, instead of keeping the expression body: the input is returned
    /// unchanged, and the directive is reported
    /// </summary>
    /// <param name="members">The members of the class</param>
    /// <param name="directive">The directive that is reported</param>
    [TestMethod]
    [Requirement("FMT-UNSUP-012")]
    [DataRow("    public int M() =>\n#if DEBUG\n        1;\n#else\n    0;\n#endif", "#if DEBUG", DisplayName = "Method")]
    [DataRow("    private readonly int[] _items = [1, 2, 3];\n\n    public int this[int index] =>\n#if DEBUG\n        1;\n#else\n    0;\n#endif", "#if DEBUG", DisplayName = "Indexer")]
    [DataRow("    private int _value;\n\n    public C() =>\n#if DEBUG\n        _value = 1;\n#else\n    _value = 0;\n#endif", "#if DEBUG", DisplayName = "Constructor")]
    [DataRow("    public static C operator +(C left, C right) =>\n#if DEBUG\n        left;\n#else\n    right;\n#endif", "#if DEBUG", DisplayName = "Operator")]
    [DataRow("    public static implicit operator int(C value) =>\n#if DEBUG\n        1;\n#else\n    0;\n#endif", "#if DEBUG", DisplayName = "Conversion operator")]
    [DataRow("    private int _value;\n\n    ~C() =>\n#if DEBUG\n        _value = 1;\n#else\n    _value = 0;\n#endif", "#if DEBUG", DisplayName = "Finalizer")]
    [DataRow("    public void M()\n    {\n        int Inner() =>\n#if DEBUG\n            1;\n#else\n        0;\n#endif\n\n        Inner();\n    }", "#if DEBUG", DisplayName = "Local function")]
    [DataRow("    public int M() =>\n#pragma warning disable CS0618\n    1;\n#pragma warning restore CS0618", "#pragma warning disable CS0618", DisplayName = "Pragma directive")]
    [DataRow("    public string M() =>\n#nullable disable\n    null;\n#nullable restore", "#nullable disable", DisplayName = "Nullable directive")]
    [DataRow("    public int M() =>\n#line 200\n    1;\n#line default", "#line 200", DisplayName = "Line directive")]
    [DataRow("    public int M() =>\n\n    #region Body\n\n    1;\n\n    #endregion // Body", "#region Body", DisplayName = "Region directive")]
    public void DirectiveBetweenTheArrowAndTheExpressionIsUnsupported(string members, string directive)
    {
        var input = $"class C\n{{\n{members}\n}}";

        // Act & Assert
        AssertUnsupportedFor(ExpressionBodyArrowCommentReason, input, directive);
        AssertUnsupportedFor(ExpressionBodyArrowCommentReason, input, directive, parseOptions: CSharpParseOptions.Default.WithPreprocessorSymbols("DEBUG"));
    }

    /// <summary>
    /// Verifies that a block comment between the arrow and the expression refuses the input as unsupported as well, so the
    /// member is not converted with the comment on its new opening brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-012")]
    public void MethodWithInlineCommentBehindTheArrowIsUnsupported()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public int M() => /* inline */ 1;
                             }
                             """;

        // Act & Assert
        AssertUnsupportedFor(ExpressionBodyArrowCommentReason, input, "/* inline */");
    }

    /// <summary>
    /// Verifies that a directive following the expression is still converted, because it travels
    /// inside the generated statement and keeps its own line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-007")]
    public void MethodWithDirectiveAfterExpressionIsStillConverted()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int Foo() => 42
                             #pragma warning disable CS0168
                                     ;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int Foo()
                                    {
                                        return 42
                                #pragma warning disable CS0168
                                        ;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a conditional group lying wholly between the expression and the semicolon is
    /// still converted, because the whole group travels into the generated statement
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-007")]
    public void MethodWithBalancedConditionalAfterExpressionIsStillConverted()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public int M() => 1
                             #if DEBUG
                                                   + 2
                             #endif
                                                   ;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    public int M()
                                    {
                                        return 1
                                #if DEBUG
                                                      + 2
                                #endif
                                        ;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a region pair following the expression is refused. The rewrite cannot safely
    /// re-host a directive pair between the expression and its terminating semicolon without changing
    /// directive placement and token boundaries
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-007")]
    public void MethodWithRegionAfterExpressionIsNotConverted()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public int A() => 1

                                 #region R

                                 #endregion // R

                                 ;

                                 public int B()
                                 {
                                     return 2;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a region opened before the arrow refuses the input as unsupported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-013")]
    public void MethodWithRegionBeforeArrowIsUnsupported()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public int M()

                                 #region Body

                                 => 1;

                                 #endregion // Body
                             }
                             """;

        // Act & Assert
        AssertUnsupportedFor(ExpressionBodyArrowLeadingCommentReason, input, "#region Body");
    }

    #endregion // Methods
}