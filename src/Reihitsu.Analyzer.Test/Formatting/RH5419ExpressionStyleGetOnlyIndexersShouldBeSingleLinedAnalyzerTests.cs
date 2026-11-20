using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5419ExpressionStyleGetOnlyIndexersShouldBeSingleLinedAnalyzer"/> and <see cref="RH5419ExpressionStyleGetOnlyIndexersShouldBeSingleLinedCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5419ExpressionStyleGetOnlyIndexersShouldBeSingleLinedAnalyzerTests : BatchCodeFixTestsBase<RH5419ExpressionStyleGetOnlyIndexersShouldBeSingleLinedAnalyzer, RH5419ExpressionStyleGetOnlyIndexersShouldBeSingleLinedCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that the diagnostic carries the rule's message and that the fix joins the arrow onto the line of the
    /// closing bracket
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5419-001")]
    [Requirement("RH5419-003")]
    public async Task VerifyWrappedArrowIsReportedWithTheRuleMessageAndFixed()
    {
        const string testCode = """
                                class C
                                {
                                    {|#0:int this[int i]
                                        => i;|}
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     int this[int i] => i;
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     Diagnostics(RH5419ExpressionStyleGetOnlyIndexersShouldBeSingleLinedAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example: an indexer whose arrow, first expression token, or closing bracket is on a
    /// later line than the start of its parameter list is reported, while one on one line, one whose expression continues
    /// on later lines, and one with a line break only between <c>this</c> and the <c>[</c> are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5419-001")]
    public async Task VerifySpecificationExampleOfReportedAndUnreportedIndexers()
    {
        const string testCode = """
                                class C
                                {
                                    {|RH5419:int this[int i]
                                        => i;|}

                                    {|RH5419:int this[long i] =>
                                        1;|}

                                    {|RH5419:int this[short a,
                                             short b]
                                        => a;|}

                                    {|RH5419:int this[byte a,
                                             byte b] => a;|}

                                    int this[uint a, uint b] => 1;

                                    int this[char a, char b] => a
                                                                + b;

                                    int this
                                        [ulong a, ulong b] => 1;
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that an indexer whose parameter list, arrow, and first expression token stand on one line is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5419-001")]
    public async Task VerifyIndexerOnOneLineIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    public int this[int i] => i;

                                    public int this[long a, long b] => 1;
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that an indexer whose arrow stands on the line below its closing bracket is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5419-001")]
    public async Task VerifyArrowBelowTheClosingBracketIsReported()
    {
        const string testCode = """
                                class C
                                {
                                    {|RH5419:int this[int i]
                                        => i;|}
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that an indexer whose arrow stands on the line of its closing bracket, but whose expression starts on the
    /// next line, is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5419-001")]
    public async Task VerifyExpressionBelowTheArrowIsReported()
    {
        const string testCode = """
                                class C
                                {
                                    {|RH5419:int this[int i] =>
                                        i;|}
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that an indexer whose parameter list spans several lines is reported, also when its arrow and the start
    /// of its expression stand on the line of its closing bracket
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5419-001")]
    public async Task VerifyParameterListOnSeveralLinesIsReportedWithTheArrowOnTheClosingBracketLine()
    {
        const string testCode = """
                                class C
                                {
                                    {|RH5419:int this[byte a,
                                             byte b] => a;|}
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that an indexer whose parameter list spans several lines only because its closing bracket stands on a
    /// line of its own is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5419-001")]
    public async Task VerifyClosingBracketOnALineOfItsOwnIsReported()
    {
        const string testCode = """
                                class C
                                {
                                    {|RH5419:int this[uint a, uint b
                                        ] => 1;|}
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that an indexer whose expression starts on the line of its closing bracket and continues on later lines
    /// is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5419-001")]
    public async Task VerifyExpressionContinuingOnLaterLinesIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    int this[char a, char b] => a
                                                                + b;

                                    int this[int i] => Foo(i,
                                                           i);

                                    int this[long i] => i switch
                                                        {
                                                            _ => 1,
                                                        };

                                    int Foo(int a, int b) => a;
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that a line break between <c>this</c> and the <c>[</c> of the parameter list is not reported when the
    /// parameter list, the arrow, and the start of the expression stand on one line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5419-001")]
    public async Task VerifyLineBreakBetweenThisAndTheOpeningBracketIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    int this
                                        [short i] => i;
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that a line break between <c>this</c> and the <c>[</c> of the parameter list does not keep an arrow below
    /// the closing bracket from being reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5419-001")]
    public async Task VerifyArrowBelowTheClosingBracketIsReportedBehindALineBreakBetweenThisAndTheOpeningBracket()
    {
        const string testCode = """
                                class C
                                {
                                    {|RH5419:int this
                                        [short i]
                                        => i;|}
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that an indexer with an accessor list is not reported, also when its accessor has an expression body that
    /// wraps
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5419-001")]
    public async Task VerifyIndexerWithAnAccessorListIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    int this[int i]
                                    {
                                        get
                                            => i;
                                    }

                                    int this[long a,
                                             long b]
                                    {
                                        get
                                        {
                                            return 1;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that a wrapped expression-bodied property and a wrapped expression-bodied method are not reported by the
    /// indexer rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5419-001")]
    public async Task VerifyPropertiesAndMethodsAreNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    int P
                                        => 1;

                                    int M(int a,
                                          int b)
                                        => a;
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the analyzer reports the same indexers under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5419-001")]
    public async Task VerifyReportedIndexersDoNotDependOnTheLineEnding(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    {|RH5419:int this[int i]
                                        => i;|}

                                    {|RH5419:int this[byte a,
                                             byte b] => a;|}

                                    int this[uint a, uint b] => 1;
                                }
                                """;

        await Verify(ConvertEndOfLine(testCode, endOfLine));
    }

    /// <summary>
    /// Verifies the specification example: the diagnostic covers the whole indexer declaration, its attribute lists and
    /// modifiers included
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5419-002")]
    public async Task VerifyDiagnosticCoversAttributeListsAndModifiers()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    {|RH5419:[Obsolete]
                                    public int this[int i]
                                        => i;|}
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that an indexer that is wrapped in its parameter list, in front of its arrow, and behind its arrow is
    /// reported once
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5419-002")]
    public async Task VerifyIndexerWrappedInSeveralPlacesIsReportedOnce()
    {
        const string testCode = """
                                class C
                                {
                                    {|#0:int this[int a,
                                             int b]
                                        =>
                                        a;|}
                                }
                                """;

        await Verify(testCode, Diagnostics(RH5419ExpressionStyleGetOnlyIndexersShouldBeSingleLinedAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example: the fix puts each indexer on one line, joining its parameter list, its arrow,
    /// and the start of its expression, also behind a line break or a block comment between <c>this</c> and the <c>[</c>
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5419-003")]
    public async Task VerifyFixPutsIndexersOnOneLine(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    {|RH5419:int this[int i]
                                        => i;|}

                                    {|RH5419:int this[long i] =>
                                        1;|}

                                    {|RH5419:int this[
                                        uint a,
                                        uint b]
                                        => 1;|}

                                    {|RH5419:int this[short a,
                                             short b] => a;|}

                                    {|RH5419:int this
                                        [ulong a,
                                         ulong b] => 1;|}

                                    {|RH5419:int this /* c */ [sbyte a,
                                                      sbyte b] => a;|}
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     int this[int i] => i;

                                     int this[long i] => 1;

                                     int this[uint a, uint b] => 1;

                                     int this[short a, short b] => a;

                                     int this[ulong a, ulong b] => 1;

                                     int this /* c */ [sbyte a, sbyte b] => a;
                                 }
                                 """;

        await Verify(ConvertEndOfLine(testCode, endOfLine), ConvertEndOfLine(fixedCode, endOfLine));
    }

    /// <summary>
    /// Verifies the specification example: the one-line documentation comment above the indexer the fix puts on one line
    /// stays as written
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5419-003")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixKeepsTheDocumentationCommentAboveTheIndexer(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    /// <summary>A</summary>
                                    {|RH5419:int this[int i]
                                        => i;|}
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     /// <summary>A</summary>
                                     int this[int i] => i;
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies the specification example: the fix puts an indexer with attribute lists on one line and lays its attribute
    /// lists out on lines of their own, one attribute per list
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5419-003")]
    public async Task VerifyFixPutsAnIndexerWithAttributeListsOnOneLine(string endOfLine)
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    {|RH5419:[Obsolete] int this[int i]
                                        => i;|}

                                    {|RH5419:[Obsolete, CLSCompliant(false)]
                                    int this[long i]
                                        => 1;|}
                                }
                                """;
        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     [Obsolete]
                                     int this[int i] => i;

                                     [Obsolete]
                                     [CLSCompliant(false)]
                                     int this[long i] => 1;
                                 }
                                 """;

        await Verify(ConvertEndOfLine(testCode, endOfLine), ConvertEndOfLine(fixedCode, endOfLine));
    }

    /// <summary>
    /// Verifies the change-document example: the fix joins the arrow, or the expression behind an arrow on the closing
    /// bracket's line, onto that line, with modifiers in front of the indexer
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5419-003")]
    public async Task VerifyFixJoinsAPublicIndexerOverAField(string endOfLine)
    {
        const string testCode = """
                                public class Sample
                                {
                                    private readonly int[] _values = new int[3];

                                    {|RH5419:public int this[int index]
                                        => _values[index];|}

                                    {|RH5419:public int this[long index] =>
                                        _values[index];|}
                                }
                                """;
        const string fixedCode = """
                                 public class Sample
                                 {
                                     private readonly int[] _values = new int[3];

                                     public int this[int index] => _values[index];

                                     public int this[long index] => _values[index];
                                 }
                                 """;

        await Verify(ConvertEndOfLine(testCode, endOfLine), ConvertEndOfLine(fixedCode, endOfLine));
    }

    /// <summary>
    /// Verifies the change-document example: the fix joins a parameter list on several lines, also when the arrow
    /// stands on the closing bracket's line, and leaves an indexer on one line alone
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5419-001")]
    [Requirement("RH5419-003")]
    public async Task VerifyFixJoinsParameterListsOnSeveralLines(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    {|RH5419:int this[short a,
                                             short b]
                                        => a;|}

                                    {|RH5419:int this[
                                        uint a,
                                        uint b]
                                        => 1;|}

                                    {|RH5419:int this[byte a,
                                             byte b] => a;|}

                                    int this[char a, char b] => a;
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     int this[short a, short b] => a;

                                     int this[uint a, uint b] => 1;

                                     int this[byte a, byte b] => a;

                                     int this[char a, char b] => a;
                                 }
                                 """;

        await Verify(ConvertEndOfLine(testCode, endOfLine), ConvertEndOfLine(fixedCode, endOfLine));
    }

    /// <summary>
    /// Verifies that the fix removes the whitespace around every line break inside the parameter list, a line break in
    /// front of a comma and in front of the closing bracket included, and spaces the joined tokens as on any line
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5419-003")]
    public async Task VerifyFixRemovesTheWhitespaceAroundLineBreaksInsideTheParameterList(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    {|RH5419:int this[ uint a
                                        , uint b
                                        ] => 1;|}

                                    {|RH5419:int this[int a,

                                             int b] => a;|}
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     int this[uint a, uint b] => 1;

                                     int this[int a, int b] => a;
                                 }
                                 """;

        await Verify(ConvertEndOfLine(testCode, endOfLine), ConvertEndOfLine(fixedCode, endOfLine));
    }

    /// <summary>
    /// Verifies that the fix joins a parameter list whose parameters carry an attribute, a modifier, and a default value
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5419-003")]
    public async Task VerifyFixJoinsParametersWithAttributesModifiersAndDefaultValues(string endOfLine)
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    {|RH5419:int this[[CLSCompliant(false)] sbyte a,
                                             string s = "a, b",
                                             params int[] rest]
                                        => a;|}
                                }
                                """;
        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     int this[[CLSCompliant(false)] sbyte a, string s = "a, b", params int[] rest] => a;
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix spaces a tight arrow on both sides when it puts the indexer on one line
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5419-003")]
    public async Task VerifyFixSpacesATightArrow(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    {|RH5419:int this[ulong i]
                                =>1;|}
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     int this[ulong i] => 1;
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix puts an explicit interface implementation of an indexer on one line
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5419-003")]
    public async Task VerifyFixPutsAnExplicitInterfaceImplementationOnOneLine(string endOfLine)
    {
        const string testCode = """
                                interface I
                                {
                                    int this[int i] { get; }
                                }

                                class C : I
                                {
                                    {|RH5419:int I.this[int i]
                                        => i;|}
                                }
                                """;
        const string fixedCode = """
                                 interface I
                                 {
                                     int this[int i] { get; }
                                 }

                                 class C : I
                                 {
                                     int I.this[int i] => i;
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix puts a <see langword="ref"/>-returning indexer on one line, whose expression starts with
    /// <see langword="ref"/>
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5419-003")]
    public async Task VerifyFixPutsARefReturningIndexerOnOneLine(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    private int[] _a = new int[1];

                                    {|RH5419:public ref int this[long i]
                                        => ref _a[i];|}
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     private int[] _a = new int[1];

                                     public ref int this[long i] => ref _a[i];
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix puts the indexer of a struct and the default implementation of an interface indexer on one line
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5419-003")]
    public async Task VerifyFixPutsIndexersOfAStructAndAnInterfaceOnOneLine(string endOfLine)
    {
        const string testCode = """
                                struct S
                                {
                                    {|RH5419:int this[int i]
                                        => i;|}
                                }

                                interface J
                                {
                                    {|RH5419:int this[int a,
                                             int b]
                                        => a;|}
                                }
                                """;
        const string fixedCode = """
                                 struct S
                                 {
                                     int this[int i] => i;
                                 }

                                 interface J
                                 {
                                     int this[int a, int b] => a;
                                 }
                                 """;

        await Verify(ConvertEndOfLine(testCode, endOfLine), ConvertEndOfLine(fixedCode, endOfLine));
    }

    /// <summary>
    /// Verifies that the fix puts an indexer of a nested type on one line in the column of the nested type's members
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5419-003")]
    public async Task VerifyFixPutsAnIndexerOfANestedTypeOnOneLine(string endOfLine)
    {
        const string testCode = """
                                class D
                                {
                                    class E
                                    {
                                        {|RH5419:int this[
                                            uint a,
                                            uint b]
                                            => 1;|}
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class D
                                 {
                                     class E
                                     {
                                         int this[uint a, uint b] => 1;
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix collapses an expression whose continuation the formatter joins, so the indexer ends up on
    /// one line
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5419-003")]
    [Requirement("RH5419-004")]
    public async Task VerifyFixIsOfferedWhenTheFormatterJoinsTheExpression(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    {|RH5419:int this[byte i]
                                        => Foo(
                                            i);|}

                                    int Foo(byte i) => i;
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     int this[byte i] => Foo(i);

                                     int Foo(byte i) => i;
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that a line comment behind <c>this</c> withholds the fix, while the diagnostic stays: it keeps the <c>[</c>
    /// of the parameter list on several lines on another line than <c>this</c>, so the indexer cannot be put on one line
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5419-004")]
    public async Task VerifyFixIsWithheldForALineCommentBehindThis(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  {|RH5419:int this // c
                                      [int a,
                                       int b] => a;|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a block comment on a line of its own in front of the <c>[</c> of a parameter list on several lines,
    /// in the leading trivia of the <c>[</c>, withholds the fix, while the diagnostic stays: the line break in front of it
    /// stays, so the indexer cannot be put on one line
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5419-004")]
    public async Task VerifyFixIsWithheldForACommentOnALineOfItsOwnInFrontOfTheOpeningBracket(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  {|RH5419:int this
                                      /* c */ [int a,
                                               int b] => a;|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a directive between <c>this</c> and the <c>[</c> of a parameter list on several lines withholds the
    /// fix, while the diagnostic stays: the directive keeps the <c>[</c> on a line of its own
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5419-004")]
    public async Task VerifyFixIsWithheldForADirectiveBetweenThisAndTheOpeningBracket(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  {|RH5419:int this
                              #if DEBUG
                              #endif
                                      [int a,
                                       int b] => a;|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a line comment behind <c>this</c> withholds the fix also when the parameter list stands on one line
    /// and only the arrow is wrapped: the arrow could be joined, but the indexer would still span two lines
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5419-004")]
    public async Task VerifyFixIsWithheldForALineCommentBehindThisInFrontOfAParameterListOnOneLine(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  {|RH5419:int this // c
                                      [int i]
                                      => i;|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a line comment behind a modifier withholds the fix, while the diagnostic stays: the measured code
    /// starts at the first token behind the attribute lists, so the line break behind the modifier counts
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5419-004")]
    public async Task VerifyFixIsWithheldForALineCommentBehindAModifier(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  {|RH5419:public // c
                                  int this[int i]
                                      => i;|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that an indexer the fix puts on one line, which shares its line with a member behind it, keeps its own
    /// line, and that the member behind it starts on the next line
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyFixGivesTheIndexerAndTheMemberBehindItLinesOfTheirOwn(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    {|RH5419:int this[int a,
                                             int b]
                                        => a;|} int _b;
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     int this[int a, int b] => a;
                                     int _b;
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that an attributed indexer the fix puts on one line, which shares its line with a member in front of it,
    /// starts on the next line
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyFixGivesAnAttributedIndexerBehindAMemberALineOfItsOwn(string endOfLine)
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    int _f; {|RH5419:[Obsolete] int this[int i]
                                        => i;|}
                                }
                                """;
        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     int _f;
                                     [Obsolete]
                                     int this[int i] => i;
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that a comment behind the semicolon of the indexer does not keep the fix from being offered, and that it
    /// stays behind the semicolon
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5419-004")]
    public async Task VerifyFixIsOfferedForACommentBehindTheSemicolon(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    {|RH5419:int this[int a,
                                             int b]
                                        => a;|} // a
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     int this[int a, int b] => a; // a
                                 }
                                 """;

        await Verify(ConvertEndOfLine(testCode, endOfLine), ConvertEndOfLine(fixedCode, endOfLine));
    }

    /// <summary>
    /// Verifies that one blank line between an attribute list and the indexer stays as written
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5419-003")]
    [Requirement("RH5419-004")]
    public async Task VerifyFixKeepsOneBlankLineBetweenAnAttributeListAndTheIndexer(string endOfLine)
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    {|RH5419:[Obsolete]

                                    int this[int a,
                                             int b]
                                        => 1;|}
                                }
                                """;
        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     [Obsolete]

                                     int this[int a, int b] => 1;
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix is not offered in a file with a syntax error, while the diagnostic is still reported, and that
    /// it is offered once the syntax error is gone
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-011")]
    public async Task NoFixIsOfferedInAFileWithASyntaxError()
    {
        const string source = """
                              class C
                              {
                                  int this[int a,
                                           int b]
                                      => a;
                              }
                              """;

        await VerifyNoCodeFixIsOfferedOnceASyntaxErrorIsAdded(source);
    }

    /// <summary>
    /// Verifies the specification example: the fix is withheld, while the diagnostic stays, when the expression stays
    /// wrapped, when a comment or a directive stands inside a parameter list on several lines, when a literal spans lines
    /// inside the parameter list, when a comment or a directive in front of the <c>[</c> keeps a line break, when a comment
    /// stands in front of or behind the arrow, and in the attribute-list cases of a property
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5419-001")]
    [Requirement("RH5419-004")]
    public async Task VerifySpecificationExampleOfWithheldFixes(string endOfLine)
    {
        const string source = """"
                              using System;

                              class C
                              {
                                  {|RH5419:int this[int i] =>
                                      i
                                      + 1;|}

                                  {|RH5419:int this[char a,
                                           char b] => Foo(a,
                                                          b);|}

                                  {|RH5419:int this[byte a, // note
                                           byte b] => a;|}

                                  {|RH5419:int this[
                              #if DEBUG
                                      uint a
                              #else
                                      ulong a
                              #endif
                                      ] => 1;|}

                                  {|RH5419:int this[string s = """
                                                      a
                                                      """] => 1;|}

                                  {|RH5419:int this // note
                                      [sbyte a,
                                       sbyte b] => a;|}

                                  {|RH5419:int this
                                      /* note */ [ushort a,
                                                  ushort b] => a;|}

                                  {|RH5419:int this
                              #if DEBUG
                              #endif
                                      [float a,
                                       float b] => 1;|}

                                  {|RH5419:int this // note
                                      [double d]
                                      => 1;|}

                                  {|RH5419:int this[long i] // note
                                      => 1;|}

                                  {|RH5419:int this[short i] => // note
                                      1;|}

                                  {|RH5419:[Obsolete]
                                  // note
                                  int this[byte i]
                                      => 1;|}

                                  {|RH5419:[Obsolete,

                                   CLSCompliant(false)]
                                  int this[char c]
                                      => 1;|}

                                  int Foo(char a, char b) => a;
                              }
                              """";

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies the specification example: a block comment inside a parameter list on one line, one on the line of
    /// <c>this</c> in front of the <c>[</c> of a parameter list on several lines, and a documentation comment in front of
    /// the indexer do not withhold the fix, and the comments stay where they are
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5419-003")]
    [Requirement("RH5419-004")]
    public async Task VerifySpecificationExampleOfBlockCommentsThatDoNotWithholdTheFix(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    {|RH5419:int this[byte a /* c */]
                                        => a;|}

                                    {|RH5419:int this /* c */ [sbyte a,
                                                      sbyte b] => a;|}

                                    /// <summary>A</summary>
                                    {|RH5419:int this[short a,
                                             short b] => a;|}
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     int this[byte a /* c */] => a;

                                     int this /* c */ [sbyte a, sbyte b] => a;

                                     /// <summary>A</summary>
                                     int this[short a, short b] => a;
                                 }
                                 """;

        await Verify(ConvertEndOfLine(testCode, endOfLine), ConvertEndOfLine(fixedCode, endOfLine));
    }

    /// <summary>
    /// Verifies that a block comment inside a parameter list on several lines withholds the fix, next to the same comment
    /// inside a parameter list on one line, which does not
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5419-004")]
    public async Task VerifyFixIsWithheldForABlockCommentInsideAParameterListOnSeveralLines(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  {|RH5419:int this[byte a, /* c */
                                           byte b] => a;|}

                                  {|RH5419:int this[short a /* c */,
                                           short b] => a;|}

                                  {|RH5419:int this[int a,
                                           /* c */ int b] => a;|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a comment inside a parameter list on several lines withholds the fix wherever it stands between the
    /// <c>[</c> and the <c>]</c>: behind the <c>[</c>, on a line of its own, and in front of the <c>]</c>
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5419-004")]
    public async Task VerifyFixIsWithheldForACommentAtEitherEndOfAParameterListOnSeveralLines(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  {|RH5419:int this[ // c
                                      int a] => a;|}

                                  {|RH5419:int this[
                                      // lead
                                      uint a,
                                      uint b] => 1;|}

                                  {|RH5419:int this[long a
                                      // c
                                      ] => 1;|}

                                  {|RH5419:int this[short a
                                      /* c */ ] => a;|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a block comment spanning several lines inside a parameter list makes the list span several lines,
    /// which is reported, and withholds the fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5419-001")]
    [Requirement("RH5419-004")]
    public async Task VerifyMultiLineBlockCommentInsideTheParameterListIsReportedWithoutAFix(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  {|RH5419:int this[byte a /* x
                                      y */] => a;|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a directive or disabled text inside a parameter list withholds the fix, while the diagnostic stays:
    /// a pragma, a region, and an inactive conditional group
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5419-001")]
    [Requirement("RH5419-004")]
    public async Task VerifyFixIsWithheldForADirectiveOrDisabledTextInsideTheParameterList(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  {|RH5419:int this[int a,
                              #pragma warning disable CS0618
                                           int b] => a;|}
                              #pragma warning restore CS0618

                                  {|RH5419:int this[long a,
                              #region R
                                           long b
                              #endregion
                                           ] => 1;|}

                                  {|RH5419:int this[short a
                              #if false
                                           , short b
                              #endif
                                           ] => a;|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that the fix is withheld, while the diagnostic stays, when the formatter keeps the expression on several
    /// lines: a switch expression and a raw string literal over several lines
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5419-004")]
    public async Task VerifyFixIsWithheldWhenTheExpressionStaysOnSeveralLines(string endOfLine)
    {
        const string source = """"
                              class C
                              {
                                  {|RH5419:int this[char i] =>
                                      i switch
                                      {
                                          _ => 1,
                                      };|}

                                  {|RH5419:int this[string s]
                                      => """
                                         a
                                         """.Length;|}

                                  {|RH5419:int this[long a,
                                           long b] => a switch
                                                      {
                                                          _ => 1,
                                                      };|}
                              }
                              """";

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a comment or a directive in front of the arrow, which makes the formatter refuse the indexer,
    /// withholds the fix while the diagnostic stays: a comment line, a block comment, and a pragma directive
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5419-004")]
    [Requirement("SHARED-FIX-010")]
    public async Task VerifyFixIsWithheldForACommentOrDirectiveInFrontOfTheArrow(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  {|RH5419:int this[int i]
                                      // note
                                      => 1;|}

                                  {|RH5419:int this[long i]
                                  /* note */ => 1;|}

                                  {|RH5419:int this[short i]
                              #pragma warning disable CS0618
                                      => 1;|}
                              #pragma warning restore CS0618

                                  {|RH5419:int this[byte a,
                                           byte b] /* note */ => a;|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a comment or a directive behind the arrow, which makes the formatter refuse the indexer, withholds the
    /// fix while the diagnostic stays
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5419-004")]
    [Requirement("SHARED-FIX-010")]
    public async Task VerifyFixIsWithheldForACommentOrDirectiveBehindTheArrow(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  {|RH5419:int this[int i] =>
                                      // note
                                      1;|}

                                  {|RH5419:int this[long i]
                                      => /* note */
                                      1;|}

                                  {|RH5419:int this[short i] =>
                              #pragma warning disable CS0618
                                      1;|}
                              #pragma warning restore CS0618
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that the fix is withheld, while the diagnostic stays, when formatting would change the blank lines behind
    /// an attribute list: a blank line inserted in front of a comment behind it, and several blank lines collapsed into one
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5419-004")]
    public async Task VerifyFixIsWithheldWhenFormattingWouldChangeTheBlankLinesBehindAnAttributeList(string endOfLine)
    {
        const string source = """
                              using System;

                              class C
                              {
                                  {|RH5419:[Obsolete]
                                  // note
                                  int this[int i]
                                      => 1;|}

                                  {|RH5419:[Obsolete]
                                  // note
                                  [CLSCompliant(false)]
                                  int this[long i]
                                      => 1;|}

                                  {|RH5419:[Obsolete]


                                  int this[short a,
                                           short b] => 1;|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that the fix is withheld, while the diagnostic stays, for an attribute list that holds a blank line, a
    /// directive, disabled text, or a comment on its own line between its brackets
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5419-004")]
    public async Task VerifyFixIsWithheldForABlankLineADirectiveDisabledTextOrACommentOnItsOwnLineBetweenTheBracketsOfAnAttributeList(string endOfLine)
    {
        const string source = """
                              using System;

                              class C
                              {
                                  {|RH5419:[Obsolete,

                                   CLSCompliant(false)]
                                  int this[int i]
                                      => 1;|}

                                  {|RH5419:[Obsolete("a",
                                            // why
                                            false)]
                                  int this[long i]
                                      => 1;|}

                                  {|RH5419:[Obsolete("a",
                              #if DEBUG
                                            false
                              #else
                                            true
                              #endif
                                            )]
                                  int this[short i]
                                      => 1;|}

                                  {|RH5419:[Obsolete,
                              #region R
                                   CLSCompliant(false)
                              #endregion
                                  ]
                                  int this[byte i]
                                      => 1;|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies the change-document example: Fix All puts the indexers it can put on one line on one line, and leaves the
    /// ones whose fix is withheld as written and reported
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5419-003")]
    [Requirement("RH5419-004")]
    public async Task VerifyFixAllLeavesTheIndexersWhoseFixIsWithheld(string endOfLine)
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    {|RH5419:int this[byte a, // note
                                             byte b] => a;|}

                                    {|RH5419:int this[
                                #if DEBUG
                                        uint a
                                #else
                                        ulong a
                                #endif
                                        ] => 1;|}

                                    {|RH5419:int this[short a /* c */]
                                        => a;|}

                                    {|RH5419:[Obsolete] int this[int i]
                                        => i;|}

                                    {|RH5419:[Obsolete]
                                    // note
                                    int this[sbyte i]
                                        => 1;|}

                                    {|RH5419:int this[long a,
                                             long b]
                                        => 1;|}
                                }
                                """;
        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     {|RH5419:int this[byte a, // note
                                              byte b] => a;|}

                                     {|RH5419:int this[
                                 #if DEBUG
                                         uint a
                                 #else
                                         ulong a
                                 #endif
                                         ] => 1;|}

                                     int this[short a /* c */] => a;

                                     [Obsolete]
                                     int this[int i] => i;

                                     {|RH5419:[Obsolete]
                                     // note
                                     int this[sbyte i]
                                         => 1;|}

                                     int this[long a, long b] => 1;
                                 }
                                 """;

        await Verify(ConvertEndOfLine(testCode, endOfLine),
                     ConvertEndOfLine(fixedCode, endOfLine),
                     test =>
                     {
                         // The diagnostics whose fix is withheld stay and are marked in the expected source
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                     });
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class C
                                {
                                    {|#0:public int this[int i]
                                            => 1;|}
                                    {|#1:public int this[long a,
                                                    long b]
                                            => 2;|}
                                }
                                """;

        const string fixedCode = """
                                 internal class C
                                 {
                                     public int this[int i] => 1;
                                     public int this[long a, long b] => 2;
                                 }
                                 """;

        // The two indexers are adjacent with no blank line between them, so the first fix's replacement span directly
        // abuts the second occurrence's leading trivia
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5419ExpressionStyleGetOnlyIndexersShouldBeSingleLinedAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}