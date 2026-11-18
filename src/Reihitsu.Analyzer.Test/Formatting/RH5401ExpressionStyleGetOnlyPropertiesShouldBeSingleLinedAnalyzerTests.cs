using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzer"/>
/// </summary>
[TestClass]
public class RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzerTests : BatchCodeFixTestsBase<RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzer, RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying that a multi-line expression-bodied get-only property is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5401-001")]
    [Requirement("RH5401-002")]
    [Requirement("RH5401-003")]
    public async Task VerifyMultiLineExpressionBodiedPropertiesAreDetectedAndFixed()
    {
        const string testData = """
                                internal class RH5401
                                {
                                    {|#0:public int P2
                                            => 2;|}
                                }
                                """;
        const string fixedData = """
                                 internal class RH5401
                                 {
                                     public int P2 => 2;
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5401MessageFormat));
    }

    /// <summary>
    /// Verifying that multi-line expression-bodied get-only properties are detected
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5401-001")]
    [Requirement("RH5401-002")]
    public async Task VerifyMultiLineExpressionBodiedPropertiesAreDetected()
    {
        const string testData = """
                                internal class RH5401
                                {
                                    public int P1 => 0;

                                    {|#0:public int P2
                                            => 2;|}

                                    public int P3 => 0
                                                     + 3;

                                    public int P4 { get; set; }
                                    public int P5 { get; }
                                    public int P6 { get; } = 6;

                                    public int P7
                                    {
                                        get => 7;
                                    }

                                    public int P8
                                    {
                                        get => P4;
                                        set => P4 = value;
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5401MessageFormat));
    }

    /// <summary>
    /// Verifying that an expression body which starts on the signature line but wraps onto later lines is not
    /// flagged, because the formatter never collapses the wrapped continuation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5401-001")]
    public async Task VerifyWrappedExpressionStartingOnSignatureLineIsNotFlagged()
    {
        const string testData = """
                                internal class RH5401
                                {
                                    private int _value;

                                    public string Result => _value switch
                                    {
                                        1 => "a",
                                        _ => "b"
                                    };
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies no code fix is offered when the formatter cannot collapse the property to a single line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5401-004")]
    public async Task VerifyNoCodeFixWhenPropertyCannotBeCollapsed()
    {
        const string testData = """
                                internal class RH5401
                                {
                                    public int[] Values =>
                                    [
                                        1,
                                        2,
                                    ];
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<PropertyDeclarationSyntax>()
                                                               .Single()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that a property whose arrow, or whose first expression token, is below its name is reported on the
    /// whole property, while one whose arrow and first expression token are on the line of its name is not reported,
    /// also when the expression continues on later lines
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5401-001")]
    [Requirement("RH5401-002")]
    public async Task VerifyArrowOrExpressionBelowNameIsReportedOnProperty()
    {
        const string testCode = """
                                class C
                                {
                                    {|RH5401:int A
                                        => 1;|}

                                    {|RH5401:int B =>
                                        1;|}

                                    int D => 1
                                             + 2;
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the diagnostic covers the whole property declaration, its attribute lists and modifiers included
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5401-002")]
    public async Task VerifyDiagnosticCoversAttributeListsAndModifiers()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    {|RH5401:[Obsolete]
                                    int A
                                        => 1;|}
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the fix puts the arrow and the expression on the line of the property name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5401-003")]
    public async Task VerifyFixPutsPropertyOnOneLine()
    {
        const string testCode = """
                                class C
                                {
                                    {|RH5401:int A
                                        => 1;|}

                                    {|RH5401:int B =>
                                        1;|}
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     int A => 1;

                                     int B => 1;
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix puts the arrow and the expression on the line of the property name, with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5401-003")]
    public async Task VerifyFixPutsPropertyOnOneLineWithCarriageReturnLineFeed()
    {
        const string testCode = """
                                class C
                                {
                                    {|RH5401:int A
                                        => 1;|}

                                    {|RH5401:int B =>
                                        1;|}
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     int A => 1;

                                     int B => 1;
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testCode), NormalizeToCarriageReturnLineFeed(fixedCode));
    }

    /// <summary>
    /// Verifies that the fix puts an explicit interface implementation of a property on one line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5401-003")]
    public async Task VerifyFixPutsExplicitInterfaceImplementationOnOneLine()
    {
        const string testCode = """
                                interface I
                                {
                                    int P { get; }
                                }

                                class C : I
                                {
                                    {|RH5401:int I.P
                                        => 1;|}
                                }
                                """;
        const string fixedCode = """
                                 interface I
                                 {
                                     int P { get; }
                                 }

                                 class C : I
                                 {
                                     int I.P => 1;
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix puts an explicit interface implementation of a property on one line, with CRLF line
    /// endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5401-003")]
    public async Task VerifyFixPutsExplicitInterfaceImplementationOnOneLineWithCarriageReturnLineFeed()
    {
        const string testCode = """
                                interface I
                                {
                                    int P { get; }
                                }

                                class C : I
                                {
                                    {|RH5401:int I.P
                                        => 1;|}
                                }
                                """;
        const string fixedCode = """
                                 interface I
                                 {
                                     int P { get; }
                                 }

                                 class C : I
                                 {
                                     int I.P => 1;
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testCode), NormalizeToCarriageReturnLineFeed(fixedCode));
    }

    /// <summary>
    /// Verifies the specification example: the fix is withheld, while the diagnostic stays, when the expression stays wrapped,
    /// also behind an attribute list, when a comment in front of the arrow keeps it on a later line, and when a comment behind
    /// the arrow makes the formatter refuse the property
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5401-004")]
    public async Task VerifyFixIsWithheldWhenPropertyStaysOnSeveralLines()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    {|RH5401:int A =>
                                        1
                                        + 2;|}

                                    {|RH5401:int B // note
                                        => 1;|}

                                    {|RH5401:int D => // note
                                        1;|}

                                    {|RH5401:[Obsolete]
                                    int E =>
                                        1
                                        + 2;|}
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies that the fix spaces a tight arrow on both sides when it puts the property on one line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-SPACE-017")]
    public async Task VerifyFixSpacesATightArrow()
    {
        const string testCode = """
                                class C
                                {
                                    {|RH5401:int A
                                        =>1;|}

                                    {|RH5401:int B=>
                                        1;|}
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     int A => 1;

                                     int B => 1;
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix spaces a tight arrow on both sides when it puts the property on one line, with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-SPACE-017")]
    public async Task VerifyFixSpacesATightArrowWithCarriageReturnLineFeed()
    {
        const string testCode = """
                                class C
                                {
                                    {|RH5401:int A
                                        =>1;|}

                                    {|RH5401:int B=>
                                        1;|}
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     int A => 1;

                                     int B => 1;
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testCode), NormalizeToCarriageReturnLineFeed(fixedCode));
    }

    /// <summary>
    /// Verifies that the fix, which lays out its result through the formatter, is not offered in a file with a syntax error,
    /// while the diagnostic is still reported, and that it is offered once the syntax error is gone
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-011")]
    public async Task NoFixIsOfferedInAFileWithASyntaxError()
    {
        const string source = """
                              internal class RH5401
                              {
                                  public int P1
                                          => 1;
                                  public int P2
                                          => 2;
                              }
                              """;

        await VerifyNoCodeFixIsOfferedOnceASyntaxErrorIsAdded(source);
    }

    /// <summary>
    /// Verifies the spec example: the one-line documentation comment above the property the fix puts on one line stays
    /// as written
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5401-003")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixKeepsTheOneLineDocumentationCommentAboveTheProperty(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    /// <summary>A</summary>
                                    {|RH5401:int A
                                        => 1;|}
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     /// <summary>A</summary>
                                     int A => 1;
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that a property the fix puts on one line, which shares its line with a member behind it, keeps its own
    /// line, and that the member behind it starts on the next line
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyFixGivesThePropertyAndTheMemberBehindItLinesOfTheirOwn(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    {|RH5401:int A
                                        => 1;|} int _b;
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     int A => 1;
                                     int _b;
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that a comment or a directive in front of the arrow withholds the fix, because the formatter refuses the property,
    /// while the diagnostic stays: a line comment behind the name, a block comment in front of the arrow on its line, a comment line,
    /// and a pragma directive
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5401-004")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyFixIsWithheldForACommentOrDirectiveInFrontOfTheArrow(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  {|RH5401:int B // note
                                      => 1;|}

                                  {|RH5401:int D
                                  /* note */ => 1;|}

                                  {|RH5401:int E
                                      // note
                                      => 1;|}

                                  {|RH5401:int F
                              #pragma warning disable CS0618
                                      => 1;|}
                              #pragma warning restore CS0618
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies the specification example: the fix puts a property with attribute lists on one line, also when an
    /// attribute list shares the property's line, and lays the attribute lists out on lines of their own
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5401-003")]
    [Requirement("RH5401-004")]
    public async Task VerifyFixPutsAPropertyWithAttributeListsOnOneLine(string endOfLine)
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    {|RH5401:[Obsolete]
                                    [CLSCompliant(false)]
                                    public int A
                                        => 1;|}

                                    {|RH5401:[Obsolete] int B
                                        => 1;|}

                                    {|RH5401:[Obsolete, CLSCompliant(false)]
                                    int L
                                        => 1;|}
                                }
                                """;
        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     [Obsolete]
                                     [CLSCompliant(false)]
                                     public int A => 1;

                                     [Obsolete]
                                     int B => 1;

                                     [Obsolete]
                                     [CLSCompliant(false)]
                                     int L => 1;
                                 }
                                 """;

        await Verify(ConvertEndOfLine(testCode, endOfLine), ConvertEndOfLine(fixedCode, endOfLine));
    }

    /// <summary>
    /// Verifies that the fix puts a property whose attribute lists stand on lines of their own on one line and leaves
    /// the attribute lists as written
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5401-003")]
    [Requirement("RH5401-004")]
    public async Task VerifyFixPutsAPropertyBelowItsAttributeListsOnOneLine(string endOfLine)
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    {|RH5401:[Obsolete]
                                    [CLSCompliant(false)]
                                    public int A
                                        => 1;|}
                                }
                                """;
        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     [Obsolete]
                                     [CLSCompliant(false)]
                                     public int A => 1;
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix puts a property whose attribute list shares the property's line on one line, and moves the
    /// attribute list onto a line of its own
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5401-003")]
    [Requirement("RH5401-004")]
    public async Task VerifyFixMovesAnAttributeListOnThePropertysLineOntoALineOfItsOwn(string endOfLine)
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    {|RH5401:[Obsolete] int B
                                        => 1;|}
                                }
                                """;
        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     [Obsolete]
                                     int B => 1;
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix lays out an attribute list with several attributes as the formatter does, one attribute per
    /// list on lines of their own, and puts the property on one line
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5401-003")]
    public async Task VerifyFixSplitsAnAttributeListWithSeveralAttributesOnThePropertysLine(string endOfLine)
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    {|RH5401:[Obsolete, CLSCompliant(false)] public int E
                                        => 1;|}
                                }
                                """;
        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     [Obsolete]
                                     [CLSCompliant(false)]
                                     public int E => 1;
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that the line break the formatter writes between the lists it splits an attribute list with several
    /// attributes into is no blank line behind an attribute list and does not keep the fix from being offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5401-003")]
    [Requirement("RH5401-004")]
    public async Task VerifyFixSplitsAnAttributeListWithSeveralAttributesAboveTheProperty(string endOfLine)
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    {|RH5401:[Obsolete, CLSCompliant(false)]
                                    int L
                                        => 1;|}
                                }
                                """;
        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     [Obsolete]
                                     [CLSCompliant(false)]
                                     int L => 1;
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that an attribute list whose arguments span several lines does not keep the fix from being offered, and
    /// that the attribute list keeps its lines
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5401-004")]
    public async Task VerifyFixIsOfferedForAnAttributeListOnSeveralLines(string endOfLine)
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    {|RH5401:[Obsolete("a",
                                              false)]
                                    int E
                                        => 1;|}
                                }
                                """;
        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     [Obsolete("a",
                                               false)]
                                     int E => 1;
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that a property whose attribute list keeps the property's line, because it holds a comment between its
    /// brackets, gets the fix, and that the attribute list stays on the property's line
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5401-003")]
    [Requirement("RH5401-004")]
    public async Task VerifyFixKeepsAnAttributeListWithACommentBetweenItsBracketsOnThePropertysLine(string endOfLine)
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    {|RH5401:[Obsolete /* c */] int E
                                        => 1;|}
                                }
                                """;
        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     [Obsolete /* c */] int E => 1;
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that a line comment behind an attribute list on its line does not keep the fix from being offered, and
    /// that it stays behind the attribute list
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5401-003")]
    [Requirement("RH5401-004")]
    public async Task VerifyFixKeepsALineCommentBehindAnAttributeList(string endOfLine)
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    {|RH5401:[Obsolete] // why
                                    int E
                                        => 1;|}
                                }
                                """;
        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     [Obsolete] // why
                                     int E => 1;
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that a block comment between an attribute list and the property on one line does not keep the fix from
    /// being offered, and that it stays behind the attribute list when the property moves onto a line of its own
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5401-003")]
    [Requirement("RH5401-004")]
    public async Task VerifyFixKeepsABlockCommentBehindAnAttributeListOnThePropertysLine(string endOfLine)
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    {|RH5401:[Obsolete] /* c */ int E
                                        => 1;|}
                                }
                                """;
        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     [Obsolete] /* c */
                                     int E => 1;
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies the specification example: a blank line behind an attribute list that the formatter keeps, in front of a
    /// comment or of the next attribute list, does not keep the fix from being offered and stays as written
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5401-003")]
    [Requirement("RH5401-004")]
    public async Task VerifyFixKeepsABlankLineBehindAnAttributeListThatTheFormatterKeeps(string endOfLine)
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    {|RH5401:[Obsolete]

                                    // note
                                    int E
                                        => 1;|}

                                    {|RH5401:[Obsolete]

                                    [CLSCompliant(false)]
                                    int F
                                        => 1;|}
                                }
                                """;
        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     [Obsolete]

                                     // note
                                     int E => 1;

                                     [Obsolete]

                                     [CLSCompliant(false)]
                                     int F => 1;
                                 }
                                 """;

        await Verify(ConvertEndOfLine(testCode, endOfLine), ConvertEndOfLine(fixedCode, endOfLine));
    }

    /// <summary>
    /// Verifies that one blank line in front of a comment behind an attribute list stays as written
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5401-003")]
    [Requirement("RH5401-004")]
    public async Task VerifyFixKeepsOneBlankLineInFrontOfACommentBehindAnAttributeList(string endOfLine)
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    {|RH5401:[Obsolete]

                                    // note
                                    int E
                                        => 1;|}
                                }
                                """;
        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     [Obsolete]

                                     // note
                                     int E => 1;
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that one blank line between two attribute lists stays as written
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5401-003")]
    [Requirement("RH5401-004")]
    public async Task VerifyFixKeepsOneBlankLineBetweenTwoAttributeLists(string endOfLine)
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    {|RH5401:[Obsolete]

                                    [CLSCompliant(false)]
                                    int F
                                        => 1;|}
                                }
                                """;
        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     [Obsolete]

                                     [CLSCompliant(false)]
                                     int F => 1;
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that one blank line between an attribute list and the property's own code, without a comment, stays as
    /// written
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5401-003")]
    [Requirement("RH5401-004")]
    public async Task VerifyFixKeepsOneBlankLineBetweenAnAttributeListAndTheProperty(string endOfLine)
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    {|RH5401:[Obsolete]

                                    int E
                                        => 1;|}
                                }
                                """;
        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     [Obsolete]

                                     int E => 1;
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies the specification example: the fix is withheld, while the diagnostic stays, when the formatter would insert
    /// a blank line in front of a comment behind an attribute list or collapse several blank lines behind an attribute list
    /// into one
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5401-004")]
    public async Task VerifyFixIsWithheldWhenFormattingWouldChangeTheBlankLinesBehindAnAttributeList(string endOfLine)
    {
        const string source = """
                              using System;

                              class C
                              {
                                  {|RH5401:[Obsolete]
                                  // note
                                  int D
                                      => 1;|}

                                  {|RH5401:[Obsolete]
                                  // note
                                  [CLSCompliant(false)]
                                  int G
                                      => 1;|}

                                  {|RH5401:[Obsolete]


                                  // note
                                  int H
                                      => 1;|}

                                  {|RH5401:[Obsolete]


                                  [CLSCompliant(false)]
                                  int K
                                      => 1;|}

                                  {|RH5401:[Obsolete]


                                  int M
                                      => 1;|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that the fix is withheld, while the diagnostic stays, for a comment on its own line directly behind an
    /// attribute list, in front of which the formatter would insert a blank line
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5401-004")]
    public async Task VerifyFixIsWithheldForACommentDirectlyBehindAnAttributeList(string endOfLine)
    {
        const string source = """
                              using System;

                              class C
                              {
                                  {|RH5401:[Obsolete]
                                  // note
                                  int D
                                      => 1;|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that the fix is withheld, while the diagnostic stays, for a comment on its own line directly between two
    /// attribute lists, in front of which the formatter would insert a blank line
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5401-004")]
    public async Task VerifyFixIsWithheldForACommentDirectlyBetweenTwoAttributeLists(string endOfLine)
    {
        const string source = """
                              using System;

                              class C
                              {
                                  {|RH5401:[Obsolete]
                                  // note
                                  [CLSCompliant(false)]
                                  int G
                                      => 1;|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that the fix is withheld, while the diagnostic stays, for two blank lines in front of a comment behind an
    /// attribute list, which the formatter would collapse into one
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5401-004")]
    public async Task VerifyFixIsWithheldForTwoBlankLinesInFrontOfACommentBehindAnAttributeList(string endOfLine)
    {
        const string source = """
                              using System;

                              class C
                              {
                                  {|RH5401:[Obsolete]


                                  // note
                                  int H
                                      => 1;|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that the fix is withheld, while the diagnostic stays, for two blank lines between two attribute lists, which
    /// the formatter would collapse into one
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5401-004")]
    public async Task VerifyFixIsWithheldForTwoBlankLinesBetweenTwoAttributeLists(string endOfLine)
    {
        const string source = """
                              using System;

                              class C
                              {
                                  {|RH5401:[Obsolete]


                                  [CLSCompliant(false)]
                                  int K
                                      => 1;|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that the fix is withheld, while the diagnostic stays, for two blank lines between an attribute list and the
    /// property's own code, without a comment, which the formatter would collapse into one
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5401-004")]
    public async Task VerifyFixIsWithheldForTwoBlankLinesBetweenAnAttributeListAndTheProperty(string endOfLine)
    {
        const string source = """
                              using System;

                              class C
                              {
                                  {|RH5401:[Obsolete]


                                  int M
                                      => 1;|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that Fix All fixes a property whose blank lines behind its attribute list the formatter keeps, and leaves a
    /// property whose blank lines it would change as written and reported
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5401-003")]
    [Requirement("RH5401-004")]
    public async Task VerifyFixAllLeavesAPropertyWhoseBlankLinesTheFormatterWouldChange(string endOfLine)
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    {|RH5401:[Obsolete]

                                    // note
                                    int E
                                        => 1;|}

                                    {|RH5401:[Obsolete]
                                    // note
                                    int D
                                        => 1;|}

                                    {|RH5401:[Obsolete]
                                    int A
                                        => 1;|}
                                }
                                """;
        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     [Obsolete]

                                     // note
                                     int E => 1;

                                     {|RH5401:[Obsolete]
                                     // note
                                     int D
                                         => 1;|}

                                     [Obsolete]
                                     int A => 1;
                                 }
                                 """;

        await Verify(ConvertEndOfLine(testCode, endOfLine), ConvertEndOfLine(fixedCode, endOfLine));
    }

    /// <summary>
    /// Verifies that a conditional directive between two attribute lists does not keep the fix from being offered, and
    /// that the directives stay as written, without a blank line inserted next to them
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5401-003")]
    [Requirement("RH5401-004")]
    public async Task VerifyFixKeepsAConditionalDirectiveBetweenTwoAttributeLists(string endOfLine)
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    {|RH5401:[Obsolete]
                                #if DEBUG
                                    [CLSCompliant(false)]
                                #endif
                                    int E
                                        => 1;|}
                                }
                                """;
        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     [Obsolete]
                                 #if DEBUG
                                     [CLSCompliant(false)]
                                 #endif
                                     int E => 1;
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that a conditional directive around a comment between an attribute list and the property does not keep
    /// the fix from being offered, and that the directives and the comment stay as written, without a blank line inserted
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5401-003")]
    [Requirement("RH5401-004")]
    public async Task VerifyFixKeepsAConditionalCommentBetweenAnAttributeListAndTheProperty(string endOfLine)
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    {|RH5401:[Obsolete]
                                #if DEBUG
                                    // note
                                #endif
                                    int E
                                        => 1;|}
                                }
                                """;
        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     [Obsolete]
                                 #if DEBUG
                                     // note
                                 #endif
                                     int E => 1;
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that the one-line documentation comment above the attribute lists of the property the fix puts on one
    /// line stays as written
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5401-003")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixKeepsTheOneLineDocumentationCommentAboveTheAttributeLists(string endOfLine)
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    /// <summary>Doc</summary>
                                    {|RH5401:[Obsolete]
                                    int E
                                        => 1;|}
                                }
                                """;
        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     /// <summary>Doc</summary>
                                     [Obsolete]
                                     int E => 1;
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that an attributed property the fix puts on one line, which shares its line with a member in front of it,
    /// starts on the next line
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyFixGivesAnAttributedPropertyBehindAMemberALineOfItsOwn(string endOfLine)
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    int _f; {|RH5401:[Obsolete] int E
                                        => 1;|}
                                }
                                """;
        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     int _f;
                                     [Obsolete]
                                     int E => 1;
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that a comment behind the semicolon of the property does not keep the fix from being offered, with and
    /// without an attribute list, and that it stays behind the semicolon
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5401-004")]
    public async Task VerifyFixIsOfferedForACommentBehindTheSemicolon(string endOfLine)
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    {|RH5401:int A
                                        => 1;|} // a

                                    {|RH5401:[Obsolete]
                                    int B
                                        => 1;|} // b
                                }
                                """;
        const string fixedCode = """
                                 using System;

                                 class C
                                 {
                                     int A => 1; // a

                                     [Obsolete]
                                     int B => 1; // b
                                 }
                                 """;

        await Verify(ConvertEndOfLine(testCode, endOfLine), ConvertEndOfLine(fixedCode, endOfLine));
    }

    /// <summary>
    /// Verifies that the fix is withheld, while the diagnostic stays, for an attributed property whose expression the
    /// formatter keeps on several lines: a collection expression over several lines and a verbatim string over two lines
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5401-004")]
    public async Task VerifyFixIsWithheldForAnAttributedPropertyWhoseExpressionStaysOnSeveralLines(string endOfLine)
    {
        const string source = """
                              using System;

                              class C
                              {
                                  {|RH5401:[Obsolete]
                                  public int[] Values =>
                                  [
                                      1,
                                      2,
                                  ];|}

                                  {|RH5401:[Obsolete]
                                  string E
                                      => @"a
                              b";|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a comment in front of the arrow of an attributed property withholds the fix, while the diagnostic
    /// stays: the comment belongs to the property's own code, unlike a comment between its attribute lists and its first
    /// token
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5401-004")]
    public async Task VerifyFixIsWithheldForACommentInFrontOfTheArrowOfAnAttributedProperty(string endOfLine)
    {
        const string source = """
                              using System;

                              class C
                              {
                                  {|RH5401:[Obsolete]
                                  int B // note
                                      => 1;|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that the fix is withheld, while the diagnostic stays, for an attributed property whose attribute list holds a
    /// comment between an attribute argument and a comma on a later line, which the formatter refuses, although the
    /// property's own code would fit on one line
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-010")]
    public async Task VerifyFixIsWithheldForAnAttributeListTheFormatterRefuses(string endOfLine)
    {
        const string source = """
                              using System;

                              class C
                              {
                                  {|RH5401:[Obsolete("a" // c
                                            , false)]
                                  int E
                                      => 1;|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class RH5401
                                {
                                    {|#0:public int P1
                                            => 1;|}
                                    {|#1:public int P2
                                            => 2;|}
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5401
                                 {
                                     public int P1 => 1;
                                     public int P2 => 2;
                                 }
                                 """;

        // The two properties are adjacent with no blank line between them, so the first fix's replacement span
        // directly abuts the second occurrence's leading trivia
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5401MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}