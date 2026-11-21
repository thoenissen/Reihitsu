using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Design;
using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider"/> on comments and directives in and around the property, and the modifiers the fix reorders
/// </summary>
[TestClass]
public class RH2001PrivateAutoPropertiesShouldNotBeUsedTriviaTests : BatchCodeFixTestsBase<RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer, RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider>
{
    #region Constants

    /// <summary>
    /// Source of a property whose multi-line accessor list holds a comment line
    /// </summary>
    private const string MultiLineAccessorListWithCommentSource = """
                                                                  class C
                                                                  {
                                                                      private int {|RH2001:Value|}
                                                                      {
                                                                          // the getter
                                                                          get;
                                                                          set;
                                                                      }
                                                                  }
                                                                  """;

    /// <summary>
    /// Source of a property whose accessor list holds a conditional directive around an accessor
    /// </summary>
    private const string AccessorListWithDirectiveSource = """
                                                           class C
                                                           {
                                                               private int {|RH2001:Value|}
                                                               {
                                                                   get;
                                                           #if DEBUG
                                                                   set;
                                                           #endif
                                                               }
                                                           }
                                                           """;

    /// <summary>
    /// Source of a property whose initializer is followed by a comment line in front of the semicolon
    /// </summary>
    private const string CommentLineInFrontOfTheSemicolonSource = """
                                                                  class C
                                                                  {
                                                                      private int {|RH2001:Value|} { get; set; } = 5
                                                                          // note
                                                                          ;
                                                                  }
                                                                  """;

    /// <summary>
    /// Source of a property whose initializer is followed by a directive line in front of the semicolon
    /// </summary>
    private const string DirectiveLineInFrontOfTheSemicolonSource = """
                                                                    class C
                                                                    {
                                                                        private int {|RH2001:Value|} { get; set; } = 5
                                                                    #pragma warning disable CS0414
                                                                            ;
                                                                    }
                                                                    """;

    /// <summary>
    /// Source of a property with a comment line between its type and its name
    /// </summary>
    private const string CommentLineInFrontOfTheNameSource = """
                                                             class C
                                                             {
                                                                 private int
                                                                     // note
                                                                     {|RH2001:Value|} { get; set; }
                                                             }
                                                             """;

    /// <summary>
    /// Source of a property with a directive line between its type and its name
    /// </summary>
    private const string DirectiveLineInFrontOfTheNameSource = """
                                                               class C
                                                               {
                                                                   private int
                                                               #pragma warning disable CS0169
                                                                       {|RH2001:Value|} { get; set; }
                                                               }
                                                               """;

    /// <summary>
    /// Source of a property whose semicolon stands on a line below its initializer, with no comment or directive in
    /// front of it
    /// </summary>
    private const string SemicolonOnItsOwnLineSource = """
                                                       class C
                                                       {
                                                           private int {|RH2001:Value|} { get; set; } = 5
                                                               ;
                                                       }
                                                       """;

    /// <summary>
    /// Expected result of the fix applied to <see cref="SemicolonOnItsOwnLineSource"/>
    /// </summary>
    private const string SemicolonOnItsOwnLineFixedSource = """
                                                            class C
                                                            {
                                                                private int _value = 5;
                                                            }
                                                            """;

    /// <summary>
    /// Source of a property whose name stands on a line below its type, with no comment or directive in front of it
    /// </summary>
    private const string NameOnItsOwnLineSource = """
                                                  class C
                                                  {
                                                      private int
                                                          {|RH2001:Value|} { get; set; }
                                                  }
                                                  """;

    /// <summary>
    /// Expected result of the fix applied to <see cref="NameOnItsOwnLineSource"/>: the field laid out as the formatter
    /// lays it out
    /// </summary>
    private const string NameOnItsOwnLineFixedSource = """
                                                       class C
                                                       {
                                                           private int
                                                           _value;
                                                       }
                                                       """;

    #endregion // Constants

    #region Tests

    /// <summary>
    /// Verifies the specification example of comments behind the name and inside the accessor list: the diagnostic is
    /// reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyNoFixIsOfferedForCommentsBehindTheNameAndInsideTheAccessorList()
    {
        const string testData = """
                                class C
                                {
                                    private int {|RH2001:Value|} /* name */ { get; /* inner */ set; } // trailing
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a comment behind the name alone withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyNoFixIsOfferedForACommentBehindTheName()
    {
        const string testData = """
                                class C
                                {
                                    private int {|RH2001:Value|} /* name */ { get; set; }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a comment inside a one-line accessor list alone withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyNoFixIsOfferedForACommentInsideAOneLineAccessorList()
    {
        const string testData = """
                                class C
                                {
                                    private int {|RH2001:Value|} { get; /* inner */ set; }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies the specification example of a comment between the accessor list and the initializer: the diagnostic
    /// is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyNoFixIsOfferedForACommentBetweenTheAccessorListAndTheInitializer()
    {
        const string testData = """
                                class C
                                {
                                    private int {|RH2001:Value|} { get; set; } /* init */ = 5;
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies the specification example of a comment line in a multi-line accessor list: the diagnostic is reported
    /// and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyNoFixIsOfferedForACommentLineInAMultiLineAccessorList()
    {
        await VerifyNoCodeFixIsOffered(MultiLineAccessorListWithCommentSource);
    }

    /// <summary>
    /// Repeats <see cref="VerifyNoFixIsOfferedForACommentLineInAMultiLineAccessorList"/> with the source normalized to
    /// CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyNoFixIsOfferedForACommentLineInAMultiLineAccessorListCarriageReturnLineFeed()
    {
        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(MultiLineAccessorListWithCommentSource));
    }

    /// <summary>
    /// Verifies the specification example of a directive inside the accessor list: the diagnostic is reported and no
    /// code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyNoFixIsOfferedForADirectiveInsideTheAccessorList()
    {
        await VerifyNoCodeFixIsOffered(AccessorListWithDirectiveSource);
    }

    /// <summary>
    /// Repeats <see cref="VerifyNoFixIsOfferedForADirectiveInsideTheAccessorList"/> with the source normalized to CRLF
    /// line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyNoFixIsOfferedForADirectiveInsideTheAccessorListCarriageReturnLineFeed()
    {
        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(AccessorListWithDirectiveSource));
    }

    /// <summary>
    /// Verifies that a <c>#pragma</c> directive line between the name and the accessor list withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyNoFixIsOfferedForADirectiveBetweenTheNameAndTheAccessorList()
    {
        const string testData = """
                                class C
                                {
                                    private int {|RH2001:Value|}
                                #pragma warning disable CS0414
                                    {
                                        get;
                                        set;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a comment line between the initializer and the semicolon withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyNoFixIsOfferedForACommentLineInFrontOfTheSemicolon()
    {
        await VerifyNoCodeFixIsOffered(CommentLineInFrontOfTheSemicolonSource);
    }

    /// <summary>
    /// Repeats <see cref="VerifyNoFixIsOfferedForACommentLineInFrontOfTheSemicolon"/> with the source normalized to CRLF
    /// line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyNoFixIsOfferedForACommentLineInFrontOfTheSemicolonCarriageReturnLineFeed()
    {
        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(CommentLineInFrontOfTheSemicolonSource));
    }

    /// <summary>
    /// Verifies that a <c>#pragma</c> directive line between the initializer and the semicolon withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyNoFixIsOfferedForADirectiveLineInFrontOfTheSemicolon()
    {
        await VerifyNoCodeFixIsOffered(DirectiveLineInFrontOfTheSemicolonSource);
    }

    /// <summary>
    /// Repeats <see cref="VerifyNoFixIsOfferedForADirectiveLineInFrontOfTheSemicolon"/> with the source normalized to
    /// CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyNoFixIsOfferedForADirectiveLineInFrontOfTheSemicolonCarriageReturnLineFeed()
    {
        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(DirectiveLineInFrontOfTheSemicolonSource));
    }

    /// <summary>
    /// Verifies that an empty <c>#if</c> block between the initializer and the semicolon withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyNoFixIsOfferedForAConditionalBlockInFrontOfTheSemicolon()
    {
        const string testData = """
                                class C
                                {
                                    private int {|RH2001:Value|} { get; set; } = 5
                                #if DEBUG
                                #endif
                                        ;
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies the specification example of the other side of
    /// <see cref="VerifyNoFixIsOfferedForACommentLineInFrontOfTheSemicolon"/>: a semicolon on its own line without a
    /// comment or directive in front of it keeps the fix, and the field ends on the initializer's line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyFixIsOfferedForASemicolonOnItsOwnLineWithoutCommentOrDirective()
    {
        await Verify(SemicolonOnItsOwnLineSource, SemicolonOnItsOwnLineFixedSource);
    }

    /// <summary>
    /// Repeats <see cref="VerifyFixIsOfferedForASemicolonOnItsOwnLineWithoutCommentOrDirective"/> with the source
    /// normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyFixIsOfferedForASemicolonOnItsOwnLineWithoutCommentOrDirectiveCarriageReturnLineFeed()
    {
        await Verify(NormalizeToCarriageReturnLineFeed(SemicolonOnItsOwnLineSource), NormalizeToCarriageReturnLineFeed(SemicolonOnItsOwnLineFixedSource));
    }

    /// <summary>
    /// Verifies the specification example of a comment line between the type and the name: the diagnostic is
    /// reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyNoFixIsOfferedForACommentLineInFrontOfTheName()
    {
        await VerifyNoCodeFixIsOffered(CommentLineInFrontOfTheNameSource);
    }

    /// <summary>
    /// Repeats <see cref="VerifyNoFixIsOfferedForACommentLineInFrontOfTheName"/> with the source normalized to CRLF
    /// line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyNoFixIsOfferedForACommentLineInFrontOfTheNameCarriageReturnLineFeed()
    {
        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(CommentLineInFrontOfTheNameSource));
    }

    /// <summary>
    /// Verifies the specification example of a directive line between the type and the name: the diagnostic is
    /// reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyNoFixIsOfferedForADirectiveLineInFrontOfTheName()
    {
        await VerifyNoCodeFixIsOffered(DirectiveLineInFrontOfTheNameSource);
    }

    /// <summary>
    /// Repeats <see cref="VerifyNoFixIsOfferedForADirectiveLineInFrontOfTheName"/> with the source normalized to CRLF
    /// line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyNoFixIsOfferedForADirectiveLineInFrontOfTheNameCarriageReturnLineFeed()
    {
        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(DirectiveLineInFrontOfTheNameSource));
    }

    /// <summary>
    /// Verifies the specification example of an <c>#if</c> block with disabled text between the type and the name:
    /// the diagnostic is reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyNoFixIsOfferedForAConditionalBlockInFrontOfTheName()
    {
        const string testData = """
                                class C
                                {
                                    private int
                                #if NEVER
                                        unused
                                #endif
                                        {|RH2001:Third|} { get; set; }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies the other side of <see cref="VerifyNoFixIsOfferedForACommentLineInFrontOfTheName"/>: a name on its own
    /// line without a comment or directive in front of it keeps the fix, and the fix lays out the field as the
    /// formatter does
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-004")]
    [Requirement("RH2001-007")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyFixIsOfferedForANameOnItsOwnLineWithoutCommentOrDirective()
    {
        await Verify(NameOnItsOwnLineSource, NameOnItsOwnLineFixedSource);
    }

    /// <summary>
    /// Repeats <see cref="VerifyFixIsOfferedForANameOnItsOwnLineWithoutCommentOrDirective"/> with the source
    /// normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-004")]
    [Requirement("RH2001-007")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyFixIsOfferedForANameOnItsOwnLineWithoutCommentOrDirectiveCarriageReturnLineFeed()
    {
        await Verify(NormalizeToCarriageReturnLineFeed(NameOnItsOwnLineSource), NormalizeToCarriageReturnLineFeed(NameOnItsOwnLineFixedSource));
    }

    /// <summary>
    /// Verifies that a comment line between the modifier and the type withholds the fix: the field the fix builds would
    /// hold the comment between its own tokens, which the formatter refuses
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    [Requirement("RH2001-009")]
    [Requirement("SHARED-FIX-010")]
    public async Task VerifyNoFixIsOfferedForACommentLineInFrontOfTheType()
    {
        const string testData = """
                                class C
                                {
                                    private
                                        // note
                                        int {|RH2001:Value|} { get; set; }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a comment behind the type on the property's line withholds the fix: the field the fix builds would
    /// hold the comment between its type and its name, which the formatter refuses
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    [Requirement("SHARED-FIX-010")]
    public async Task VerifyNoFixIsOfferedForACommentBehindTheTypeOnTheSameLine()
    {
        const string testData = """
                                class C
                                {
                                    private int /* note */ {|RH2001:Value|} { get; set; }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies the specification example of fields the fix would build with a comment the formatter refuses: both
    /// properties are reported, and no code fix is offered for either
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-010")]
    public async Task VerifyNoFixIsOfferedWhenTheBuiltFieldHoldsACommentTheFormatterRefuses()
    {
        const string testData = """
                                class C
                                {
                                    private int /* note */ {|RH2001:Value|} { get; set; }

                                    private int {|RH2001:Count|} { get; set; } = /* five */ 5;
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a comment behind the initializer's <c>=</c> withholds the fix: the field the fix builds would hold
    /// it in front of its value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyNoFixIsOfferedForACommentInFrontOfTheInitializerValue()
    {
        const string testData = """
                                class C
                                {
                                    private int {|RH2001:Value|} { get; set; } = /* five */ 5;
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a comment on a line of its own in front of the initializer's value withholds the fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-007")]
    public async Task VerifyNoFixIsOfferedForACommentLineInFrontOfTheInitializerValue(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    private int {|RH2001:Value|} { get; set; } =
                                        // five
                                        5;
                                }
                                """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies that a comment behind the initializer value's last token on that token's line withholds the fix: the
    /// field the fix builds would hold it between its value and its <c>;</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyNoFixIsOfferedForACommentBehindTheInitializerValue()
    {
        const string testData = """
                                class C
                                {
                                    private int {|RH2001:Value|} { get; set; } = 5 /* five */;
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies the specification example: a comment inside the initializer's value that stands behind no binary operator does not
    /// withhold the fix, and stays inside the value of the field
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyFixIsOfferedForACommentInsideTheInitializerValue(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    private int {|RH2001:Value|} { get; set; } = 2 /* two */ + Offset(/* three */ 3);

                                    private static int Offset(int value)
                                    {
                                        return value;
                                    }
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      private int _value = 2 /* two */ + Offset(/* three */3);

                                      private static int Offset(int value)
                                      {
                                          return value;
                                      }
                                  }
                                  """;

        await VerifyCodeFixWithEndOfLine(testData, resultData, endOfLine);
    }

    /// <summary>
    /// Verifies the specification example: a comment behind a binary operator of the initializer's value withholds the fix, because
    /// the formatter refuses the field the fix builds, and the diagnostic stays
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyNoFixIsOfferedForACommentBehindAnOperatorOfTheInitializerValue(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    private int {|RH2001:Value|} { get; set; } = 2 + /* three */ 3;
                                }
                                """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies the specification example: a comment between the tokens of a <c>?.</c> in the initializer's value withholds the
    /// fix, because the formatter refuses the field the fix builds, and the diagnostic stays
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyNoFixIsOfferedForACommentInsideALinkOperatorOfTheInitializerValue(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    private static string Source = "";

                                    private int? {|RH2001:Value|} { get; set; } = Source? /* three */ .Length;
                                }
                                """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies the change example: a line comment between the <c>?</c> and the <c>.</c> of the initializer's value withholds the
    /// fix, and the diagnostic stays
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyNoFixIsOfferedForALineCommentInsideALinkOperatorOfTheInitializerValue(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    private static string Source = "";

                                    private int? {|RH2001:Value|} { get; set; } = Source? // three
                                                                             .Length;
                                }
                                """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies the specification example: a comment between the <c>.</c> of a member access and its name in the initializer's
    /// value withholds the fix, because the formatter refuses the field the fix builds, and the diagnostic stays
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyNoFixIsOfferedForACommentBetweenTheDotAndTheMemberNameOfTheInitializerValue(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    private static string Source = "";

                                    private int {|RH2001:Value|} { get; set; } = Source. /* three */ Length;
                                }
                                """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies the change example: a line comment between the <c>.</c> of a member access and its name in the initializer's value
    /// withholds the fix, and the diagnostic stays
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyNoFixIsOfferedForALineCommentBetweenTheDotAndTheMemberNameOfTheInitializerValue(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    private static string Source = "";

                                    private int {|RH2001:Value|} { get; set; } = Source. // three
                                                                      Length;
                                }
                                """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies that a comment in front of a <c>?.</c> in the initializer's value, which stands inside no link operator, does not
    /// withhold the fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyFixIsOfferedForACommentInFrontOfALinkOperatorOfTheInitializerValue(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    private static string Source = "";

                                    private int? {|RH2001:Value|} { get; set; } = Source /* three */ ?.Length;
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      private static string Source = "";

                                      private int? _value = Source /* three */?.Length;
                                  }
                                  """;

        await VerifyCodeFixWithEndOfLine(testData, resultData, endOfLine);
    }

    /// <summary>
    /// Verifies that a line comment behind a binary operator at the end of a line of the initializer's value withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyNoFixIsOfferedForALineCommentBehindAnOperatorOfTheInitializerValue()
    {
        const string testData = """
                                class C
                                {
                                    private int {|RH2001:Value|} { get; set; } = 2 + // three
                                                                                     3;
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies the specification example of get-only properties whose modifiers are not in the order of RH7105: the fix
    /// writes the modifiers in that order on one line, keeps the blank line, the comment, and the documentation comment
    /// once in front of the field, and keeps the written order of a property with a <c>set</c> accessor
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-004")]
    public async Task VerifyFixWritesReorderedModifiersOnOneLineBehindTheLeadingTrivia(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    private int _x;

                                    // the shared value
                                    static private int {|RH2001:Value|} { get; }

                                    /// <summary>
                                    /// The name.
                                    /// </summary>
                                    static private string {|RH2001:Name|} { get; } = "v"; // name

                                    static private int {|RH2001:Count|} { get; set; }

                                    int Read() => _x + Value + Name.Length + Count;
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      private int _x;

                                      // the shared value
                                      private static readonly int _value;

                                      /// <summary>
                                      /// The name.
                                      /// </summary>
                                      private static readonly string _name = "v"; // name

                                      static private int _count;

                                      int Read() => _x + _value + _name.Length + _count;
                                  }
                                  """;

        await Verify(WithEndOfLine(testData, endOfLine), WithEndOfLine(resultData, endOfLine));
    }

    /// <summary>
    /// Verifies the specification example of a comment in front of the first modifier on its line: it stays once in
    /// front of the reordered modifiers
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-004")]
    public async Task VerifyFixKeepsACommentInFrontOfTheFirstModifierOnceInFrontOfTheReorderedModifiers(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    /* lead */ static private int {|RH2001:Value|} { get; }
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      /* lead */ private static readonly int _value;
                                  }
                                  """;

        await Verify(WithEndOfLine(testData, endOfLine), WithEndOfLine(resultData, endOfLine));
    }

    /// <summary>
    /// Verifies that the fix writes the reordered modifiers of a get-only property with one space between them
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-004")]
    public async Task VerifyFixWritesReorderedModifiersWithOneSpaceBetweenThem(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    static private int {|RH2001:Value|} { get; }
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      private static readonly int _value;
                                  }
                                  """;

        await Verify(WithEndOfLine(testData, endOfLine), WithEndOfLine(resultData, endOfLine));
    }

    /// <summary>
    /// Verifies that the fix writes three reordered modifiers and the added <c>readonly</c> in the order of RH7105 with
    /// one space between them
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-004")]
    public async Task VerifyFixWritesThreeReorderedModifiersInTheOrderOfRh7105(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    static new private int {|RH2001:Value|} { get; }
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      private static new readonly int _value;
                                  }
                                  """;

        await Verify(WithEndOfLine(testData, endOfLine), WithEndOfLine(resultData, endOfLine));
    }

    /// <summary>
    /// Verifies that a comment behind the property stays behind the field when the fix reorders the modifiers
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-004")]
    public async Task VerifyFixKeepsATrailingCommentWhenItReordersTheModifiers(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    static private int {|RH2001:Value|} { get; } // t
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      private static readonly int _value; // t
                                  }
                                  """;

        await Verify(WithEndOfLine(testData, endOfLine), WithEndOfLine(resultData, endOfLine));
    }

    /// <summary>
    /// Verifies the change-document example of a blank line above a property whose modifiers the fix reorders: the
    /// blank line stays above the field, and the modifiers stay on one line
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-004")]
    public async Task VerifyFixKeepsTheBlankLineAboveAndTheReorderedModifiersOnOneLine(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    private int _x;

                                    static private int {|RH2001:Value|} { get; }

                                    int Read() => _x + Value;
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      private int _x;

                                      private static readonly int _value;

                                      int Read() => _x + _value;
                                  }
                                  """;

        await Verify(WithEndOfLine(testData, endOfLine), WithEndOfLine(resultData, endOfLine));
    }

    /// <summary>
    /// Verifies that a <c>#pragma</c> directive on the line above a property whose modifiers the fix reorders stays once
    /// in front of the field, and the fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-004")]
    public async Task VerifyFixKeepsAPragmaDirectiveAboveOnceInFrontOfTheReorderedModifiers(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                #pragma warning disable CS0414
                                    static private int {|RH2001:Value|} { get; }
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                  #pragma warning disable CS0414
                                      private static readonly int _value;
                                  }
                                  """;

        await Verify(WithEndOfLine(testData, endOfLine), WithEndOfLine(resultData, endOfLine));
    }

    /// <summary>
    /// Verifies that a <c>#region</c> directive on the line above a property whose modifiers the fix reorders stays once
    /// in front of the field, and the fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-004")]
    public async Task VerifyFixKeepsARegionDirectiveAboveOnceInFrontOfTheReorderedModifiers(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    #region Properties

                                    static private int {|RH2001:Value|} { get; }

                                    #endregion // Properties
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      #region Properties

                                      private static readonly int _value;

                                      #endregion // Properties
                                  }
                                  """;

        await Verify(WithEndOfLine(testData, endOfLine), WithEndOfLine(resultData, endOfLine));
    }

    /// <summary>
    /// Verifies that the fix writes <c>readonly</c> in front of a modifier that RH7105 puts behind it, also when the
    /// property's modifiers already are in order
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-004")]
    public async Task VerifyFixWritesReadonlyInFrontOfUnsafe()
    {
        const string testData = """
                                class C
                                {
                                    private unsafe string {|RH2001:First|} { get; }

                                    unsafe private string {|RH2001:Second|} { get; }
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      private readonly unsafe string _first;

                                      private readonly unsafe string _second;
                                  }
                                  """;

        await Verify(testData, resultData, test => test.SolutionTransforms.Add(ApplyAllowUnsafeToTestProject));
    }

    /// <summary>
    /// Verifies the specification example of comments among the modifiers or in front of the type, whether a reordered
    /// modifier would cross them or not: all four properties are reported, and no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-009")]
    public async Task VerifyNoFixIsOfferedForACommentAmongTheModifiersOrInFrontOfTheType(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    static /* a */ private int {|RH2001:First|} { get; }

                                    static // b
                                    private int {|RH2001:Second|} { get; }

                                    private /* c */ static int {|RH2001:Third|} { get; }

                                    private static /* d */ int {|RH2001:Fourth|} { get; }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies the specification example of a directive that a reordered modifier would cross: the property is reported,
    /// and no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-009")]
    public async Task VerifyNoFixIsOfferedForADirectiveThatAReorderedModifierWouldCross(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    static
                                #pragma warning disable CS0169
                                    private int {|RH2001:Value|} { get; }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies that a comment among the modifiers that no modifier crosses withholds the fix as well: the field the fix
    /// builds would hold the comment, which the formatter refuses
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-009")]
    public async Task VerifyNoFixIsOfferedForACommentThatNoModifierCrosses(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    private /* a */ static int {|RH2001:First|} { get; }

                                    private /* b */ new static int {|RH2001:Second|} { get; }

                                    static private /* c */ int {|RH2001:Third|} { get; }

                                    private static /* d */ int {|RH2001:Fourth|} { get; }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies that a comment in front of the place of <c>readonly</c>, with a modifier that RH7105 puts behind
    /// <c>readonly</c>, withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-009")]
    public async Task VerifyNoFixIsOfferedForACommentInFrontOfUnsafe()
    {
        const string testData = """
                                class C
                                {
                                    private /* c */ unsafe string {|RH2001:Value|} { get; }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData, test => test.SolutionTransforms.Add(ApplyAllowUnsafeToTestProject));
    }

    /// <summary>
    /// Verifies that an end-of-line comment between two modifiers that keep their order withholds the fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-009")]
    public async Task VerifyNoFixIsOfferedForAnEndOfLineCommentBetweenModifiersThatKeepTheirOrder(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    private // a
                                    static int {|RH2001:Value|} { get; }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies that a directive between two modifiers that keep their order keeps its place, and the fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-009")]
    public async Task VerifyFixKeepsADirectiveBetweenModifiersThatKeepTheirOrder(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    private
                                #pragma warning disable CS0169
                                    static int {|RH2001:Value|} { get; }
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      private
                                  #pragma warning disable CS0169
                                      static readonly int _value;
                                  }
                                  """;

        await Verify(WithEndOfLine(testData, endOfLine), WithEndOfLine(resultData, endOfLine));
    }

    /// <summary>
    /// Verifies that Fix All replaces the property whose modifiers the fix reorders and leaves the property with a
    /// comment that a reordered modifier would cross reported without a fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-004")]
    [Requirement("RH2001-009")]
    [Requirement("SHARED-FIX-002")]
    public async Task VerifyFixAllSkipsThePropertyWithACommentThatAReorderedModifierWouldCross()
    {
        const string testData = """
                                class C
                                {
                                    static /* a */ private int {|RH2001:First|} { get; }

                                    static private int {|RH2001:Second|} { get; }

                                    new static private int {|RH2001:Third|} { get; }

                                    int Read() => First + Second + Third;
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      static /* a */ private int {|RH2001:First|} { get; }

                                      private static readonly int _second;

                                      private static new readonly int _third;

                                      int Read() => First + _second + _third;
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     test =>
                     {
                         // The diagnostic whose fix is withheld stays and is marked in the expected source
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                     });
    }

    /// <summary>
    /// Verifies that a conditional group between the last modifier and the type withholds the fix, also when its branch
    /// holds only <c>static</c>: the property is reported, and no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-009")]
    public async Task VerifyNoCodeFixIsOfferedForAConditionalModifierBetweenTheModifiersAndTheType(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private
                              #if DEBUG
                                      static
                              #endif
                                      int {|RH2001:Value|} { get; }

                                  public int Get()
                                  {
                                      return Value;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a conditional group between the modifiers and the type withholds the fix when it holds <c>extern</c>,
    /// which is not valid on a field: written as a field, a build with <c>DEBUG</c> would declare an extern field
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-009")]
    public async Task VerifyNoCodeFixIsOfferedForAConditionalExternBetweenTheModifiersAndTheType(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private
                              #if DEBUG
                                      extern
                              #endif
                                      int {|RH2001:Value|} { get; }

                                  public int Get()
                                  {
                                      return Value;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a conditional group between the modifiers and the type withholds the fix when it holds
    /// <c>readonly</c>: with the <c>readonly</c> the fix adds, a build with <c>DEBUG</c> would declare the modifier twice
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-009")]
    public async Task VerifyNoCodeFixIsOfferedForAConditionalReadonlyBetweenTheModifiersAndTheType(string endOfLine)
    {
        const string source = """
                              public struct C
                              {
                                  private
                              #if DEBUG
                                      readonly
                              #endif
                                      int {|RH2001:Value|} { get; }

                                  public int Get()
                                  {
                                      return Value;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a conditional group between the modifiers and the type withholds the fix when it holds a modifier
    /// RH7105 puts behind <c>readonly</c>, such as <c>unsafe</c>
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-009")]
    public async Task VerifyNoCodeFixIsOfferedForAConditionalUnsafeBetweenTheModifiersAndTheType(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private
                              #if DEBUG
                                      unsafe
                              #endif
                                      int {|RH2001:Value|} { get; }

                                  public int Get()
                                  {
                                      return Value;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a conditional group whose <c>#else</c> and <c>#endif</c> stand between two modifiers withholds the fix,
    /// although every branch holds an accessibility modifier: the property is reported, and no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-009")]
    public async Task VerifyNoCodeFixIsOfferedForAConditionalGroupWithADirectiveBetweenTwoModifiers(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                              #if !DEBUG
                                  private
                              #else
                                  internal
                              #endif
                                  static int {|RH2001:Value|} { get; }

                                  public int Get()
                                  {
                                      return Value;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies the counterpart of a conditional group between the modifiers: a whole conditional group in front of the
    /// first modifier keeps the fix, and the group stays in front of the field
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-004")]
    [Requirement("RH2001-009")]
    public async Task VerifyFixKeepsAConditionalGroupInFrontOfTheFirstModifier(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                              #if DEBUG
                                  private int _debug;
                              #endif
                                  private static int {|RH2001:Value|} { get; }

                                  public int Get()
                                  {
                                      return Value;
                                  }
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                   #if DEBUG
                                       private int _debug;
                                   #endif
                                       private static readonly int _value;

                                       public int Get()
                                       {
                                           return _value;
                                       }
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifies that a conditional group holding <c>extern</c> in front of the type of a get-only property without other
    /// modifiers withholds the fix: written as a field, a build with <c>DEBUG</c> would declare an extern field
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-009")]
    public async Task VerifyNoCodeFixIsOfferedForAConditionalModifierInFrontOfTheTypeOfAGetOnlyPropertyWithoutModifiers(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                              #if DEBUG
                                  extern
                              #endif
                                  int {|RH2001:Value|} { get; }

                                  public int Get()
                                  {
                                      return Value;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a conditional group holding <c>extern</c> in front of the type of a property with a setter and
    /// without other modifiers withholds the fix: written as a field, a build with <c>DEBUG</c> would declare an extern field
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-009")]
    public async Task VerifyNoCodeFixIsOfferedForAConditionalModifierInFrontOfTheTypeOfAReadWritePropertyWithoutModifiers(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                              #if DEBUG
                                  extern
                              #endif
                                  int {|RH2001:Value|} { get; set; }

                                  public int Get()
                                  {
                                      return Value;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a conditional group holding only a whole member in front of the type of a property with a setter
    /// and without modifiers withholds the fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-009")]
    public async Task VerifyNoCodeFixIsOfferedForAConditionalWholeMemberGroupInFrontOfTheTypeOfAReadWritePropertyWithoutModifiers(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                              #if DEBUG
                                  private int _trace;
                              #endif
                                  int {|RH2001:Value|} { get; set; }

                                  public int Get()
                                  {
                                      return Value;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies the specification example of <c>extern</c> in an excluded branch in front of the first modifier of a
    /// property of a class: written as a field, a build with <c>DEBUG</c> would declare an extern field, so the property is
    /// reported and no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-010")]
    public async Task VerifyNoCodeFixIsOfferedForAConditionalExternInFrontOfTheFirstModifier(string endOfLine)
    {
        const string source = """
                              class C
                              {
                              #if DEBUG
                                  extern
                              #endif
                                  private int {|RH2001:Value|} { get; }

                                  int Read() => Value;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies the specification example of <c>readonly</c> in an excluded branch in front of the first modifier of a
    /// property of a struct: with the <c>readonly</c> the fix adds, a build with <c>DEBUG</c> would declare the modifier
    /// twice, so the property is reported and no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-010")]
    public async Task VerifyNoCodeFixIsOfferedForAConditionalReadonlyInFrontOfTheFirstModifierInAStruct(string endOfLine)
    {
        const string source = """
                              struct S
                              {
                              #if DEBUG
                                  readonly
                              #endif
                                  private int {|RH2001:Value|} { get; }

                                  int Read() => Value;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that <c>static</c> in an excluded branch in front of the first modifier withholds the fix: the branch is
    /// no whole member
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-010")]
    public async Task VerifyNoCodeFixIsOfferedForAConditionalStaticInFrontOfTheFirstModifier(string endOfLine)
    {
        const string source = """
                              class C
                              {
                              #if DEBUG
                                  static
                              #endif
                                  private int {|RH2001:Value|} { get; }

                                  int Read() => Value;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies the specification example of an excluded <c>#else</c> branch behind a group of a whole member: without
    /// <c>DEBUG</c> defined, the <c>#else</c> branch holds <c>extern</c>, so the property is reported and no code fix is
    /// offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-010")]
    public async Task VerifyNoCodeFixIsOfferedForAnExcludedElseBranchHoldingExternBehindAWholeMember(string endOfLine)
    {
        const string source = """
                              class C
                              {
                              #if !DEBUG
                                  private int _trace;
                              #else
                                  extern
                              #endif
                                  private int {|RH2001:Value|} { get; }

                                  int Read() => Value;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies the specification example of an excluded branch in front of the member of the active <c>#else</c> branch,
    /// where only the <c>#endif</c> stands in the leading trivia of the property's first modifier: in a build with
    /// <c>DEBUG</c>, <c>extern</c> stands in front of the property, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-010")]
    public async Task VerifyNoCodeFixIsOfferedForAnExcludedBranchInFrontOfTheMemberOfTheActiveElseBranch(string endOfLine)
    {
        const string source = """
                              class C
                              {
                              #if DEBUG
                                  extern
                              #else
                                  private int _trace;
                              #endif
                                  private int {|RH2001:Value|} { get; }

                                  int Read() => Value;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that an excluded <c>#else</c> branch holding <c>extern</c> withholds the fix when the symbol of the active
    /// <c>#if</c> branch is defined in the document
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-010")]
    public async Task VerifyNoCodeFixIsOfferedForAnExcludedElseBranchHoldingExternWhenTheSymbolIsDefined(string endOfLine)
    {
        const string source = """
                              #define DEBUG
                              class C
                              {
                              #if DEBUG
                                  private int _trace;
                              #else
                                  extern
                              #endif
                                  private int {|RH2001:Value|} { get; }

                                  int Read() => Value;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that an excluded <c>#elif</c> branch holding <c>extern</c> withholds the fix, although the excluded
    /// <c>#if</c> branch holds a whole member: every excluded branch of the group is read
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-010")]
    public async Task VerifyNoCodeFixIsOfferedForAnExcludedElifBranchHoldingExternBehindAnExcludedWholeMember(string endOfLine)
    {
        const string source = """
                              class C
                              {
                              #if DEBUG
                                  private int _trace;
                              #elif TRACE
                                  extern
                              #endif
                                  private int {|RH2001:Value|} { get; }

                                  int Read() => Value;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies the specification example of an attribute list in an excluded branch in front of the first modifier: the
    /// branch is no whole member, so the property is reported and no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-010")]
    public async Task VerifyNoCodeFixIsOfferedForAConditionalAttributeListInFrontOfTheFirstModifier(string endOfLine)
    {
        const string source = """
                              class C
                              {
                              #if DEBUG
                                  [System.Obsolete]
                              #endif
                                  private int {|RH2001:Value|} { get; }

                                  int Read() => Value;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that an excluded branch in front of the first modifier that holds text that is not code withholds the fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-010")]
    public async Task VerifyNoCodeFixIsOfferedForAnExcludedBranchInFrontOfTheFirstModifierHoldingTextThatIsNotCode(string endOfLine)
    {
        const string source = """
                              class C
                              {
                              #if DEBUG
                                  this is not code
                              #endif
                                  private int {|RH2001:Value|} { get; }

                                  int Read() => Value;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a group in front of the first modifier that holds another conditional group withholds the fix, although
    /// the inner group holds a whole member: the group is nested
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-010")]
    public async Task VerifyNoCodeFixIsOfferedForANestedGroupOfAWholeMemberInFrontOfTheFirstModifier(string endOfLine)
    {
        const string source = """
                              class C
                              {
                              #if DEBUG
                              #if TRACE
                                  private int _trace;
                              #endif
                              #endif
                                  private int {|RH2001:Value|} { get; }

                                  int Read() => Value;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies the specification example of a group whose <c>#endif</c> stands behind a property without an initializer
    /// and whose branch holds another group: the group behind the property does not wrap whole members, so no code fix is
    /// offered, although the group in front of the first modifier holds a whole member
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-010")]
    public async Task VerifyNoCodeFixIsOfferedForAGroupBehindThePropertyThatHoldsAnotherGroupInItsBranch(string endOfLine)
    {
        const string source = """
                              class C
                              {
                              #if !RELEASE
                                  private int _first;
                              #if DEBUG
                                  private int _trace;
                              #endif
                                  private int {|RH2001:Value|} { get; }
                              #endif

                                  int Read() => Value;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies the specification example of a group of a whole member in front of the first modifier: the fix is offered
    /// and keeps the group in front of the field
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-004")]
    [Requirement("RH2001-010")]
    public async Task VerifyFixKeepsAGroupOfAWholeMemberInFrontOfTheFirstModifier(string endOfLine)
    {
        const string source = """
                              class C
                              {
                              #if DEBUG
                                  private int _trace;
                              #endif
                                  private int {|RH2001:Value|} { get; }

                                  int Read() => Value;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                   #if DEBUG
                                       private int _trace;
                                   #endif
                                       private readonly int _value;

                                       int Read() => _value;
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifies that a group whose excluded branches all hold whole members keeps the fix, also when the branches are an
    /// excluded <c>#if</c> branch in front of an active <c>#else</c> member and only the <c>#endif</c> stands in front of
    /// the first modifier
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-004")]
    [Requirement("RH2001-010")]
    public async Task VerifyFixKeepsAGroupWhoseExcludedBranchInFrontOfTheActiveElseMemberHoldsAWholeMember(string endOfLine)
    {
        const string source = """
                              class C
                              {
                              #if DEBUG
                                  private long _trace;
                              #else
                                  private int _trace;
                              #endif
                                  private int {|RH2001:Value|} { get; }

                                  int Read() => Value;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                   #if DEBUG
                                       private long _trace;
                                   #else
                                       private int _trace;
                                   #endif
                                       private readonly int _value;

                                       int Read() => _value;
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifies that a group in front of the first modifier whose excluded branch holds only a comment keeps the fix: the
    /// branch is a sequence of no members
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-004")]
    [Requirement("RH2001-010")]
    public async Task VerifyFixKeepsAGroupInFrontOfTheFirstModifierWhoseExcludedBranchHoldsOnlyAComment(string endOfLine)
    {
        const string source = """
                              class C
                              {
                              #if DEBUG
                                  // Traced
                              #endif
                                  private int {|RH2001:Value|} { get; }

                                  int Read() => Value;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                   #if DEBUG
                                       // Traced
                                   #endif
                                       private readonly int _value;

                                       int Read() => _value;
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifies the specification example of an initializer in an excluded branch behind a property without one: in a
    /// build with <c>X</c>, the field the fix would write would be followed by <c>= 5;</c>, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-010")]
    public async Task VerifyNoCodeFixIsOfferedForAnExcludedInitializerBehindAPropertyWithoutOne(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  private int {|RH2001:Value|} { get; }
                              #if X
                                  = 5;
                              #endif

                                  int Read() => Value;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that an initializer in an excluded branch behind the last property of a type body withholds the fix: the
    /// token behind the property is the closing brace of the type body
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-010")]
    public async Task VerifyNoCodeFixIsOfferedForAnExcludedInitializerBehindTheLastPropertyInFrontOfTheClosingBrace(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  int Read() => Value;

                                  private int {|RH2001:Value|} { get; }
                              #if X
                                  = 5;
                              #endif
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that an initializer in an excluded branch behind a property without a modifier withholds the fix, as behind
    /// a property with a modifier
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-010")]
    public async Task VerifyNoCodeFixIsOfferedForAnExcludedInitializerBehindAPropertyWithoutAModifier(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  int {|RH2001:Value|} { get; }
                              #if X
                                  = 5;
                              #endif

                                  int Read() => Value;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a group behind a property without an initializer whose excluded branch holds a modifier of the next
    /// member withholds the fix: the branch does not read as whole members
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-010")]
    public async Task VerifyNoCodeFixIsOfferedForAConditionalModifierBehindAPropertyWithoutAnInitializer(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  private int {|RH2001:Value|} { get; }
                              #if DEBUG
                                  static
                              #endif
                                  private int _other;

                                  int Read() => Value;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a group behind a property without an initializer that holds another conditional group withholds the
    /// fix, although the inner group holds a whole member
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-010")]
    public async Task VerifyNoCodeFixIsOfferedForANestedGroupOfAWholeMemberBehindAPropertyWithoutAnInitializer(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  private int {|RH2001:Value|} { get; }
                              #if DEBUG
                              #if TRACE
                                  private int _trace;
                              #endif
                              #endif

                                  int Read() => Value;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies the specification example of a group of a whole member behind a property without an initializer: the fix
    /// is offered and keeps the group behind the field
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-004")]
    [Requirement("RH2001-010")]
    public async Task VerifyFixKeepsAGroupOfAWholeMemberBehindAPropertyWithoutAnInitializer(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  private int {|RH2001:Value|} { get; }
                              #if DEBUG
                                  private int _trace;
                              #endif

                                  int Read() => Value;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private readonly int _value;
                                   #if DEBUG
                                       private int _trace;
                                   #endif

                                       int Read() => _value;
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifies the specification example of a group behind a property with an initializer: the property ends in
    /// <c>;</c>, so the excluded <c>static</c> belongs to the next member in every build, and the fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-004")]
    [Requirement("RH2001-010")]
    public async Task VerifyFixKeepsAConditionalModifierBehindAPropertyWithAnInitializer(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  private int {|RH2001:Value|} { get; } = 5;
                              #if DEBUG
                                  static
                              #endif
                                  private int _other;

                                  int Read() => Value;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private readonly int _value = 5;
                                   #if DEBUG
                                       static
                                   #endif
                                       private int _other;

                                       int Read() => _value;
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifies the specification example of a group of a whole member in front of the first modifier inside the active
    /// branch of a group that wraps the file: the wrapping group has no directive in front of the first modifier or behind
    /// the property, so it is not read, and the fix is offered as in multi-targeting code
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH2001-004")]
    [Requirement("RH2001-010")]
    public async Task VerifyFixKeepsAGroupOfAWholeMemberInsideTheActiveBranchOfAGroupThatWrapsTheFile(string endOfLine)
    {
        const string source = """
                              #define NET
                              #if NET
                              class C
                              {
                              #if DEBUG
                                  private int _trace;
                              #endif
                                  private int {|RH2001:Value|} { get; }

                                  int Read() => Value;
                              }
                              #endif
                              """;

        const string fixedSource = """
                                   #define NET
                                   #if NET
                                   class C
                                   {
                                   #if DEBUG
                                       private int _trace;
                                   #endif
                                       private readonly int _value;

                                       int Read() => _value;
                                   }
                                   #endif
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifies that Fix All replaces the property behind which a group of a whole member stands and leaves the property
    /// behind which an excluded initializer stands reported without a fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-010")]
    [Requirement("SHARED-FIX-002")]
    public async Task VerifyFixAllSkipsThePropertyBehindWhichAnExcludedInitializerStands()
    {
        const string testData = """
                                class C
                                {
                                    private int {|RH2001:Second|} { get; }
                                #if DEBUG
                                    private int _trace;
                                #endif

                                    private int {|RH2001:Third|} { get; }

                                    private int {|RH2001:First|} { get; }
                                #if X
                                    = 5;
                                #endif

                                    int Read() => First + Second + Third;
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                      private readonly int _second;
                                  #if DEBUG
                                      private int _trace;
                                  #endif

                                      private readonly int _third;

                                      private int {|RH2001:First|} { get; }
                                  #if X
                                      = 5;
                                  #endif

                                      int Read() => First + _second + _third;
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     test =>
                     {
                         // The diagnostic whose fix is withheld stays and is marked in the expected source
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                     });
    }

    /// <summary>
    /// Verifies that Fix All replaces the property behind a group of a whole member and leaves the property behind a group
    /// holding <c>extern</c> reported without a fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-010")]
    [Requirement("SHARED-FIX-002")]
    public async Task VerifyFixAllSkipsThePropertyBehindAConditionalExternInFrontOfTheFirstModifier()
    {
        const string testData = """
                                class C
                                {
                                #if DEBUG
                                    extern
                                #endif
                                    private int {|RH2001:First|} { get; }

                                #if DEBUG
                                    private int _trace;
                                #endif
                                    private int {|RH2001:Second|} { get; }

                                    private int {|RH2001:Third|} { get; }

                                    int Read() => First + Second + Third;
                                }
                                """;

        const string resultData = """
                                  class C
                                  {
                                  #if DEBUG
                                      extern
                                  #endif
                                      private int {|RH2001:First|} { get; }

                                  #if DEBUG
                                      private int _trace;
                                  #endif
                                      private readonly int _second;

                                      private readonly int _third;

                                      int Read() => First + _second + _third;
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     test =>
                     {
                         // The diagnostic whose fix is withheld stays and is marked in the expected source
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                     });
    }

    /// <summary>
    /// Verifies the specification example: a comment in front of an argument list of the initializer's value withholds the fix,
    /// because the formatter refuses the field the fix builds, and the diagnostic stays
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyNoFixIsOfferedForACommentInFrontOfAnArgumentListOfTheInitializerValue(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    private int {|RH2001:Value|} { get; set; } = Offset /* three */ (3);

                                    private static int Offset(int value)
                                    {
                                        return value;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies the change example: a line comment that keeps the argument list of the initializer's value on the line below
    /// withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    public async Task VerifyNoFixIsOfferedForALineCommentInFrontOfAnArgumentListOfTheInitializerValue()
    {
        const string testData = """
                                class C
                                {
                                    private int {|RH2001:Value|} { get; set; } = Offset // three
                                                                                   (3);

                                    private static int Offset(int value)
                                    {
                                        return value;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a comment in front of an element access, in front of a null-forgiving operator, or between the <c>?</c> and the
    /// <c>[</c> of a conditional element access in the initializer's value withholds the fix
    /// </summary>
    /// <param name="value">The initializer's value</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    [DataRow("Values /* c */ [0]", DisplayName = "In front of an element access")]
    [DataRow("(Text /* c */ !).Length", DisplayName = "In front of a null-forgiving operator")]
    [DataRow("Values? /* c */ [0] ?? 0", DisplayName = "Between the question mark and the bracket")]
    public async Task VerifyNoFixIsOfferedForACommentInFrontOfAnAttachedPartOfTheInitializerValue(string value)
    {
        var testData = $"#nullable enable\n\nclass C\n{{\n    private int {{|RH2001:Value|}} {{ get; set; }} = {value};\n\n    private static int[] Values = [];\n\n    private static string? Text = \"\";\n}}";

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies the specification example: a comment in a gap the line-break phase joins in the initializer's value withholds the
    /// fix, because the formatter refuses the field the fix builds, and the diagnostic stays
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyNoFixIsOfferedForACommentBehindNewInTheInitializerValue(string endOfLine)
    {
        const string testData = """
                                class Counter;

                                class C
                                {
                                    private Counter {|RH2001:Value|} { get; set; } = new /* three */ Counter();
                                }
                                """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies the specification example: a comment behind an assignment operator inside an anonymous-object member of the
    /// initializer's value withholds the fix, because the formatter refuses the field the fix builds, and the diagnostic stays
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2001-007")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyNoFixIsOfferedForACommentBehindAnAssignmentInsideAnAnonymousObjectMember(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    private static int s_count;

                                    private object {|RH2001:Value|} { get; set; } = new { A = s_count += /* three */ 3 };
                                }
                                """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(testData, endOfLine));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Writes the provided text with the given line break
    /// </summary>
    /// <param name="text">Text</param>
    /// <param name="endOfLine">Line break</param>
    /// <returns>The text with every line break replaced by <paramref name="endOfLine"/></returns>
    private static string WithEndOfLine(string text, string endOfLine)
    {
        return text.Replace("\r\n", "\n").Replace("\n", endOfLine);
    }

    #endregion // Methods

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources
                                {
                                    internal class RH2001
                                    {
                                        public int Sum()
                                        {
                                            return Alpha + Beta;
                                        }

                                        private int {|#0:Alpha|} { get; set; }

                                        private int {|#1:Beta|} { get; set; }
                                    }
                                }
                                """;

        const string resultData = """
                                  namespace Reihitsu.Analyzer.Test.Design.Resources
                                  {
                                      internal class RH2001
                                      {
                                          public int Sum()
                                          {
                                              return _alpha + _beta;
                                          }

                                          private int _alpha;

                                          private int _beta;
                                      }
                                  }
                                  """;

        return new FixAllScenario(testData, resultData, Diagnostics(RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}