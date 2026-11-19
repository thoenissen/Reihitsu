using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5111AssignmentsMustHaveProperLineBreaksAnalyzer"/>
/// </summary>
public partial class RH5111AssignmentsMustHaveProperLineBreaksAnalyzerTests
{
    #region Tests

    /// <summary>
    /// Verifies that the fix of an assignment behind a conditional access with two wrapped links puts both links under the
    /// <c>?</c>, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-006")]
    public async Task VerifyFixAlignsTwoWrappedLinksUnderTheAnchor()
    {
        const string testData = """
                                class A
                                {
                                    public A B;
                                    public A this[int i] => null;
                                    public int C;
                                    public A N()
                                    {
                                        return null;
                                    }
                                }

                                class D
                                {
                                    void M(A a)
                                    {
                                        a?{|#0:.B
                                          .B
                                          .C =
                                            5|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 class A
                                 {
                                     public A B;
                                     public A this[int i] => null;
                                     public int C;
                                     public A N()
                                     {
                                         return null;
                                     }
                                 }

                                 class D
                                 {
                                     void M(A a)
                                     {
                                         a?.B
                                          .B
                                          .C = 5;
                                     }
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the fix of an assignment behind a conditional access whose receiver holds a member access in front of its
    /// <c>?</c> puts the wrapped member under the dot of that member access, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-006")]
    public async Task VerifyFixAlignsAWrappedMemberBehindAReceiverWithAMemberAccessUnderTheAnchor()
    {
        const string testData = """
                                class A
                                {
                                    public A B;
                                    public A this[int i] => null;
                                    public int C;
                                    public A N()
                                    {
                                        return null;
                                    }
                                }

                                class D
                                {
                                    void M(A a)
                                    {
                                        a.B?{|#0:.B
                                           .C =
                                            5|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 class A
                                 {
                                     public A B;
                                     public A this[int i] => null;
                                     public int C;
                                     public A N()
                                     {
                                         return null;
                                     }
                                 }

                                 class D
                                 {
                                     void M(A a)
                                     {
                                         a.B?.B
                                          .C = 5;
                                     }
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the fix of an assignment behind a nested conditional access whose last binding is an element binding puts the
    /// wrapped member under the first <c>?</c>, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-006")]
    public async Task VerifyFixAlignsAWrappedMemberBehindAnElementBindingOfANestedConditionalAccessUnderTheAnchor()
    {
        const string testData = """
                                class A
                                {
                                    public A B;
                                    public A this[int i] => null;
                                    public int C;
                                    public A N()
                                    {
                                        return null;
                                    }
                                }

                                class D
                                {
                                    void M(A a)
                                    {
                                        a?.B?{|#0:[0]
                                          .C =
                                            5|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 class A
                                 {
                                     public A B;
                                     public A this[int i] => null;
                                     public int C;
                                     public A N()
                                     {
                                         return null;
                                     }
                                 }

                                 class D
                                 {
                                     void M(A a)
                                     {
                                         a?.B?[0]
                                          .C = 5;
                                     }
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the fix of an assignment behind two element bindings joins the wrapped member to the line of the bindings,
    /// under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-006")]
    public async Task VerifyFixJoinsAMemberBehindTwoElementBindings()
    {
        const string testData = """
                                class A
                                {
                                    public A B;
                                    public A this[int i] => null;
                                    public int C;
                                    public A N()
                                    {
                                        return null;
                                    }
                                }

                                class D
                                {
                                    void M(A a)
                                    {
                                        a?[0]?{|#0:[1]
                                          .C =
                                            5|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 class A
                                 {
                                     public A B;
                                     public A this[int i] => null;
                                     public int C;
                                     public A N()
                                     {
                                         return null;
                                     }
                                 }

                                 class D
                                 {
                                     void M(A a)
                                     {
                                         a?[0]?[1].C = 5;
                                     }
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the fix of an assignment behind an element binding with two wrapped links joins the first link to the line
    /// of the binding and puts the second under it, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-006")]
    public async Task VerifyFixJoinsTheFirstLinkBehindAnElementBindingAndAlignsTheNextLinkUnderIt()
    {
        const string testData = """
                                class A
                                {
                                    public A B;
                                    public A this[int i] => null;
                                    public int C;
                                    public A N()
                                    {
                                        return null;
                                    }
                                }

                                class D
                                {
                                    void M(A a)
                                    {
                                        a?{|#0:[0]
                                          .B
                                          .C =
                                            5|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 class A
                                 {
                                     public A B;
                                     public A this[int i] => null;
                                     public int C;
                                     public A N()
                                     {
                                         return null;
                                     }
                                 }

                                 class D
                                 {
                                     void M(A a)
                                     {
                                         a?[0].B
                                              .C = 5;
                                     }
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the fix of an assignment behind an element binding whose member access stands on its own line joins it to the
    /// line of the binding, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-006")]
    public async Task VerifyFixJoinsAMemberAccessBehindAnElementBinding()
    {
        const string testData = """
                                class A
                                {
                                    public A B;
                                    public A this[int i] => null;
                                    public int C;
                                    public A N()
                                    {
                                        return null;
                                    }
                                }

                                class D
                                {
                                    void M(A a)
                                    {
                                        a?{|#0:[0]
                                          .B.C =
                                            5|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 class A
                                 {
                                     public A B;
                                     public A this[int i] => null;
                                     public int C;
                                     public A N()
                                     {
                                         return null;
                                     }
                                 }

                                 class D
                                 {
                                     void M(A a)
                                     {
                                         a?[0].B.C = 5;
                                     }
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the fix of an assignment behind a conditional access that is the body of a lambda puts the wrapped member
    /// under the <c>?</c>, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-006")]
    public async Task VerifyFixAlignsAWrappedMemberInALambdaBodyUnderTheAnchor()
    {
        const string testData = """
                                class A
                                {
                                    public A B;
                                    public A this[int i] => null;
                                    public int C;
                                    public A N()
                                    {
                                        return null;
                                    }
                                }

                                class D
                                {
                                    void M(A a)
                                    {
                                        System.Action f = () => a?{|#0:.B
                                                                  .C =
                                                                    5|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 class A
                                 {
                                     public A B;
                                     public A this[int i] => null;
                                     public int C;
                                     public A N()
                                     {
                                         return null;
                                     }
                                 }

                                 class D
                                 {
                                     void M(A a)
                                     {
                                         System.Action f = () => a?.B
                                                                  .C = 5;
                                     }
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the fix of an assignment behind a conditional access keeps the comment above the statement and puts the
    /// wrapped member under the <c>?</c>, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-006")]
    public async Task VerifyFixKeepsTheCommentAboveTheConditionalAccess()
    {
        const string testData = """
                                class A
                                {
                                    public A B;
                                    public A this[int i] => null;
                                    public int C;
                                    public A N()
                                    {
                                        return null;
                                    }
                                }

                                class D
                                {
                                    void M(A a)
                                    {
                                        // x
                                        a?{|#0:.B
                                          .C =
                                            5|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 class A
                                 {
                                     public A B;
                                     public A this[int i] => null;
                                     public int C;
                                     public A N()
                                     {
                                         return null;
                                     }
                                 }

                                 class D
                                 {
                                     void M(A a)
                                     {
                                         // x
                                         a?.B
                                          .C = 5;
                                     }
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the fix of an assignment behind a conditional access whose links all stand on the line of the <c>?</c> joins
    /// the value to the operator and changes nothing else, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-006")]
    public async Task VerifyFixJoinsTheValueOfAnAssignmentBehindAConditionalAccessWhoseLinksShareALine()
    {
        const string testData = """
                                class A
                                {
                                    public A B;
                                    public A this[int i] => null;
                                    public int C;
                                    public A N()
                                    {
                                        return null;
                                    }
                                }

                                class D
                                {
                                    void M(A a)
                                    {
                                        a?{|#0:.B.C =
                                            5|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 class A
                                 {
                                     public A B;
                                     public A this[int i] => null;
                                     public int C;
                                     public A N()
                                     {
                                         return null;
                                     }
                                 }

                                 class D
                                 {
                                     void M(A a)
                                     {
                                         a?.B.C = 5;
                                     }
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the fix of an assignment to a bare member binding behind a conditional access joins the value to the
    /// operator, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-006")]
    public async Task VerifyFixJoinsTheValueOfAnAssignmentToABareBinding()
    {
        const string testData = """
                                class A
                                {
                                    public A B;
                                    public A this[int i] => null;
                                    public int C;
                                    public A N()
                                    {
                                        return null;
                                    }
                                }

                                class D
                                {
                                    void M(A a)
                                    {
                                        a?{|#0:.B =
                                            a|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 class A
                                 {
                                     public A B;
                                     public A this[int i] => null;
                                     public int C;
                                     public A N()
                                     {
                                         return null;
                                     }
                                 }

                                 class D
                                 {
                                     void M(A a)
                                     {
                                         a?.B = a;
                                     }
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the fix of an assignment that is not behind a conditional access and whose assigned member stands on its
    /// own line puts the member under the first dot, as today, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-006")]
    public async Task VerifyFixOfAnAssignmentNotBehindAConditionalAccessAlignsItsWrappedMemberAsInPlace()
    {
        const string testData = """
                                class A
                                {
                                    public A B;
                                    public A this[int i] => null;
                                    public int C;
                                    public A N()
                                    {
                                        return null;
                                    }
                                }

                                class D
                                {
                                    void M(A a)
                                    {
                                        {|#0:a.B
                                          .C =
                                            5|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 class A
                                 {
                                     public A B;
                                     public A this[int i] => null;
                                     public int C;
                                     public A N()
                                     {
                                         return null;
                                     }
                                 }

                                 class D
                                 {
                                     void M(A a)
                                     {
                                         a.B
                                          .C = 5;
                                     }
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testData, fixedData);
    }

    /// <summary>
    /// Verifies the first example of the requirement: Fix All lays out six assignments behind conditional accesses with wrapped
    /// links in one document as formatting the document lays them out, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-006")]
    public async Task VerifyFixAllLaysOutEveryAssignmentBehindAConditionalAccess()
    {
        const string testData = """
                                class A
                                {
                                    public A B;
                                    public A this[int i] => null;
                                    public int C;
                                    public A N()
                                    {
                                        return null;
                                    }
                                }

                                class D
                                {
                                    void M(A a)
                                    {
                                        a?{|#0:.B
                                          .C =
                                            5|};
                                        a?{|#1:.B
                                         .C =
                                            5|};
                                        a?{|#2:[0]
                                          .C =
                                            5|};
                                        a?.B?{|#3:.B
                                            .C =
                                            5|};
                                        a?{|#4:.B
                                          .B
                                          .C =
                                            5|};
                                        a.B?{|#5:.B
                                           .C =
                                            5|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 class A
                                 {
                                     public A B;
                                     public A this[int i] => null;
                                     public int C;
                                     public A N()
                                     {
                                         return null;
                                     }
                                 }

                                 class D
                                 {
                                     void M(A a)
                                     {
                                         a?.B
                                          .C = 5;
                                         a?.B
                                          .C = 5;
                                         a?[0].C = 5;
                                         a?.B?.B
                                          .C = 5;
                                         a?.B
                                          .B
                                          .C = 5;
                                         a.B?.B
                                          .C = 5;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat, 6));
        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(fixedData), Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat, 6));
    }

    /// <summary>
    /// Verifies the second example of the requirement: an assignment behind a conditional access whose links share the line of the
    /// <c>?</c>, and an assignment that is not behind a conditional access, are laid out in place, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-006")]
    public async Task VerifyFixOfTheRequirementBoundariesLaysOutTheAssignmentsInPlace()
    {
        const string testData = """
                                class A
                                {
                                    public A B;
                                    public A this[int i] => null;
                                    public int C;
                                    public A N()
                                    {
                                        return null;
                                    }
                                }

                                class D
                                {
                                    void M(A a)
                                    {
                                        a?{|#0:.B.C =
                                            5|};
                                        {|#1:a.B
                                          .C =
                                            5|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 class A
                                 {
                                     public A B;
                                     public A this[int i] => null;
                                     public int C;
                                     public A N()
                                     {
                                         return null;
                                     }
                                 }

                                 class D
                                 {
                                     void M(A a)
                                     {
                                         a?.B.C = 5;
                                         a.B
                                          .C = 5;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat, 2));
        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(fixedData), Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that the fix is withheld for an assignment behind a conditional access with a comment line in front of its
    /// wrapped member, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-005")]
    public async Task VerifyNoFixIsOfferedForAnAssignmentBehindAConditionalAccessWithACommentLineInFrontOfTheWrappedMember()
    {
        const string testData = """
                                class A
                                {
                                    public A B;
                                    public A this[int i] => null;
                                    public int C;
                                    public A N()
                                    {
                                        return null;
                                    }
                                }

                                class D
                                {
                                    void M(A a)
                                    {
                                        a?{|RH5111:.B
                                          // c
                                          .C =
                                            5|};
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(testData));
    }

    /// <summary>
    /// Verifies that the fix is withheld for an assignment behind a conditional access with a comment behind the binding in front
    /// of its wrapped member, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-005")]
    public async Task VerifyNoFixIsOfferedForAnAssignmentBehindAConditionalAccessWithACommentBehindTheBinding()
    {
        const string testData = """
                                class A
                                {
                                    public A B;
                                    public A this[int i] => null;
                                    public int C;
                                    public A N()
                                    {
                                        return null;
                                    }
                                }

                                class D
                                {
                                    void M(A a)
                                    {
                                        a?{|RH5111:.B // c
                                          .C =
                                            5|};
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(testData));
    }

    /// <summary>
    /// Verifies that the fix is withheld for an assignment behind a conditional access with a directive in front of its wrapped
    /// member, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-005")]
    public async Task VerifyNoFixIsOfferedForAnAssignmentBehindAConditionalAccessWithADirectiveInFrontOfTheWrappedMember()
    {
        const string testData = """
                                class A
                                {
                                    public A B;
                                    public A this[int i] => null;
                                    public int C;
                                    public A N()
                                    {
                                        return null;
                                    }
                                }

                                class D
                                {
                                    void M(A a)
                                    {
                                        a?{|RH5111:.B
                                #pragma warning disable CS0168
                                          .C =
                                            5|};
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(testData));
    }

    /// <summary>
    /// Verifies that the fix of an assignment behind a member binding with a wrapped member gives the text formatting the document gives,
    /// and that the analyzer reports nothing on it, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-006")]
    public async Task VerifyFixBehindMemberBindingMatchesFormattingTheDocument()
    {
        const string testData = """
                                class A
                                {
                                    public A B;
                                    public A this[int i] => null;
                                    public int C;
                                    public A N()
                                    {
                                        return null;
                                    }
                                }

                                class D
                                {
                                    void M(A a)
                                    {
                                        a?{|RH5111:.B
                                          .C =
                                            5|};
                                    }
                                }
                                """;

        await VerifyCodeFixMatchesFormatter(testData);
        await VerifyCodeFixMatchesFormatter(NormalizeToCarriageReturnLineFeed(testData));
    }

    /// <summary>
    /// Verifies that the fix of an assignment behind an element binding with a wrapped member gives the text formatting the document gives,
    /// and that the analyzer reports nothing on it, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-006")]
    public async Task VerifyFixBehindElementBindingMatchesFormattingTheDocument()
    {
        const string testData = """
                                class A
                                {
                                    public A B;
                                    public A this[int i] => null;
                                    public int C;
                                    public A N()
                                    {
                                        return null;
                                    }
                                }

                                class D
                                {
                                    void M(A a)
                                    {
                                        a?{|RH5111:[0]
                                          .C =
                                            5|};
                                    }
                                }
                                """;

        await VerifyCodeFixMatchesFormatter(testData);
        await VerifyCodeFixMatchesFormatter(NormalizeToCarriageReturnLineFeed(testData));
    }

    /// <summary>
    /// Verifies that the fix of an assignment behind a nested conditional access with a wrapped member gives the text formatting the document gives,
    /// and that the analyzer reports nothing on it, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-006")]
    public async Task VerifyFixBehindNestedConditionalAccessMatchesFormattingTheDocument()
    {
        const string testData = """
                                class A
                                {
                                    public A B;
                                    public A this[int i] => null;
                                    public int C;
                                    public A N()
                                    {
                                        return null;
                                    }
                                }

                                class D
                                {
                                    void M(A a)
                                    {
                                        a?.B?{|RH5111:.B
                                            .C =
                                            5|};
                                    }
                                }
                                """;

        await VerifyCodeFixMatchesFormatter(testData);
        await VerifyCodeFixMatchesFormatter(NormalizeToCarriageReturnLineFeed(testData));
    }

    /// <summary>
    /// Verifies that the fix of an assignment behind an element binding with two wrapped links with a wrapped member gives the text formatting the document gives,
    /// and that the analyzer reports nothing on it, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-006")]
    public async Task VerifyFixBehindElementBindingWithTwoWrappedLinksMatchesFormattingTheDocument()
    {
        const string testData = """
                                class A
                                {
                                    public A B;
                                    public A this[int i] => null;
                                    public int C;
                                    public A N()
                                    {
                                        return null;
                                    }
                                }

                                class D
                                {
                                    void M(A a)
                                    {
                                        a?{|RH5111:[0]
                                          .B
                                          .C =
                                            5|};
                                    }
                                }
                                """;

        await VerifyCodeFixMatchesFormatter(testData);
        await VerifyCodeFixMatchesFormatter(NormalizeToCarriageReturnLineFeed(testData));
    }

    /// <summary>
    /// Verifies that the fix of an assignment behind a receiver with a member access with a wrapped member gives the text formatting the document gives,
    /// and that the analyzer reports nothing on it, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-006")]
    public async Task VerifyFixBehindReceiverWithAMemberAccessMatchesFormattingTheDocument()
    {
        const string testData = """
                                class A
                                {
                                    public A B;
                                    public A this[int i] => null;
                                    public int C;
                                    public A N()
                                    {
                                        return null;
                                    }
                                }

                                class D
                                {
                                    void M(A a)
                                    {
                                        a.B?{|RH5111:.B
                                           .C =
                                            5|};
                                    }
                                }
                                """;

        await VerifyCodeFixMatchesFormatter(testData);
        await VerifyCodeFixMatchesFormatter(NormalizeToCarriageReturnLineFeed(testData));
    }

    /// <summary>
    /// Verifies that the fix of a declarator that starts a later line of its declaration keeps the declarator in its column
    /// in the document, where formatting the document would put it in another column, and keeps the wrapped argument under
    /// the first argument
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyDeclaratorOnALaterLineKeepsItsColumnAndKeepsTheWrappedArgumentUnderTheFirstArgument()
    {
        const string testData = """
                                class C
                                {
                                    static int Other(int x, int y)
                                    {
                                        return x + y;
                                    }

                                    void M()
                                    {
                                        int a = 1,
                                              {|#0:b =
                                                  Other(1,
                                                        2)|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     static int Other(int x, int y)
                                     {
                                         return x + y;
                                     }

                                     void M()
                                     {
                                         int a = 1,
                                               b = Other(1,
                                                         2);
                                     }
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the fix of a declarator that starts a later line of its declaration, whose value is an object
    /// creation, puts the braces under new and the member one level deeper
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    public async Task VerifyDeclaratorOnALaterLineKeepsTheBracesOfAnObjectCreationUnderNew()
    {
        const string testData = """
                                class C
                                {
                                    int P { get; set; }

                                    void M()
                                    {
                                        C a = null,
                                              {|#0:b =
                                                  new C
                                                  {
                                                      P = 1
                                                  }|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     int P { get; set; }

                                     void M()
                                     {
                                         C a = null,
                                               b = new C
                                                   {
                                                       P = 1
                                                   };
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of a declarator that starts a later line of its declaration, whose value is a lambda, puts the
    /// lambda's block under its parameter list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    public async Task VerifyDeclaratorOnALaterLineKeepsTheBlockOfALambdaUnderItsParameterList()
    {
        const string testData = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action c = null,
                                              {|#0:d =
                                                  () =>
                                                  {
                                                      M();
                                                  }|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Action c = null,
                                               d = () =>
                                                   {
                                                       M();
                                                   };
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of an enum member that starts a later line of its enum keeps the member in its column and puts
    /// the wrapped binary operator under the value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyEnumMemberOnALaterLineKeepsItsColumnAndKeepsTheWrappedOperatorUnderTheValue()
    {
        const string testData = """
                                enum E
                                {
                                    A = 1,
                                      {|#0:B =
                                      1 +
                                      2|}
                                }
                                """;
        const string fixedData = """
                                 enum E
                                 {
                                     A = 1,
                                       B = 1
                                           + 2
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of a parameter that starts a later line of its parameter list in another column than
    /// formatting the document gives it keeps the parameter in its column and puts the wrapped binary operator under the
    /// value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyParameterOnALaterLineKeepsItsColumnAndKeepsTheWrappedOperatorUnderTheValue()
    {
        const string testData = """
                                class C
                                {
                                    void M(int a,
                                                {|#0:string c
                                               = "x" +
                                                 "y"|})
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M(int a,
                                                 string c = "x"
                                                            + "y")
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of a parameter that shares its line with the parameter in front of it keeps it on that line,
    /// where formatting the document would put it on a line of its own, and puts the wrapped binary operator under the
    /// value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-FIX-007")]
    [Requirement("FMT-PIPE-024")]
    public async Task VerifyParameterBehindAnotherParameterOnItsLineStaysOnThatLineAndKeepsTheWrappedOperatorUnderTheValue()
    {
        const string testData = """
                                class C
                                {
                                    void M(int a,
                                           int b, {|#0:string c
                                               = "x" +
                                                 "y"|})
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M(int a,
                                            int b, string c = "x"
                                                              + "y")
                                     {
                                     }
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the fix of a declarator behind the closing line of a raw string literal that formatting moves keeps
    /// the literal's lines and the declarator as in the document and puts the wrapped binary operator under the value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyDeclaratorBehindTheClosingLineOfARawStringKeepsTheWrappedOperatorUnderTheValue()
    {
        const string testData = """"
                                class C
                                {
                                    void M()
                                    {
                                        string a = """
                                            x
                                            """, {|#0:b =
                                            "y" +
                                            "z"|};
                                    }
                                }
                                """";
        const string fixedData = """"
                                 class C
                                 {
                                     void M()
                                     {
                                         string a = """
                                             x
                                             """, b = "y"
                                                      + "z";
                                     }
                                 }
                                 """";

        await VerifyUnderBothLineEndings(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the fix of the first declarator on the line after its type keeps the declarator in its column and the
    /// wrapped argument under the first argument
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyFirstDeclaratorOnTheLineAfterItsTypeKeepsTheWrappedArgumentUnderTheFirstArgument()
    {
        const string testData = """
                                class C
                                {
                                    static int Other(int x, int y)
                                    {
                                        return x + y;
                                    }

                                    void M()
                                    {
                                        int
                                            {|#0:a =
                                                Other(1,
                                                      2)|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     static int Other(int x, int y)
                                     {
                                         return x + y;
                                     }

                                     void M()
                                     {
                                         int
                                             a = Other(1,
                                                       2);
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of a field declarator that starts a later line of its declaration keeps the declarator in its
    /// column and the wrapped argument under the first argument
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyFieldDeclaratorOnALaterLineKeepsTheWrappedArgumentUnderTheFirstArgument()
    {
        const string testData = """
                                class C
                                {
                                    static int Other(int x, int y)
                                    {
                                        return x + y;
                                    }

                                    private static int _a = 1,
                                                       {|#0:_b =
                                                           Other(1,
                                                                 2)|};
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     static int Other(int x, int y)
                                     {
                                         return x + y;
                                     }

                                     private static int _a = 1,
                                                        _b = Other(1,
                                                                   2);
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of a declarator that starts a later line of a for initializer keeps the declarator in its
    /// column and the wrapped argument under the first argument
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyForInitializerDeclaratorOnALaterLineKeepsTheWrappedArgumentUnderTheFirstArgument()
    {
        const string testData = """
                                class C
                                {
                                    static int Other(int x, int y)
                                    {
                                        return x + y;
                                    }

                                    void M()
                                    {
                                        for (int i = 0,
                                                 {|#0:j =
                                                     Other(1,
                                                           2)|}; i < j; i++)
                                        {
                                        }
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     static int Other(int x, int y)
                                     {
                                         return x + y;
                                     }

                                     void M()
                                     {
                                         for (int i = 0,
                                                  j = Other(1,
                                                            2); i < j; i++)
                                         {
                                         }
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of a declarator that starts a later line of a tab-indented declaration keeps the tabs in front
    /// of it and puts the wrapped argument under the first argument, with each tab in front of the declarator counted as
    /// one column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-FIX-007")]
    [Requirement("FMT-PIPE-018")]
    public async Task VerifyDeclaratorOnALaterLineOfATabIndentedDeclarationKeepsTheWrappedArgumentUnderTheFirstArgument()
    {
        const string testData = "class C\n{\n\tstatic int Other(int x, int y)\n\t{\n\t\treturn x + y;\n\t}\n\n\tvoid M()\n\t{\n\t\tint a = 1,\n\t\t    {|#0:b =\n\t\t        Other(1,\n\t\t              2)|};\n\t}\n}\n";
        const string fixedData = "class C\n{\n\tstatic int Other(int x, int y)\n\t{\n\t\treturn x + y;\n\t}\n\n\tvoid M()\n\t{\n\t\tint a = 1,\n\t\t    b = Other(1,\n                2);\n\t}\n}\n";

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of the first parameter on the line after the opening parenthesis keeps it on its line in its
    /// column, where formatting the document would join it to the parenthesis, and puts the wrapped binary operator under
    /// the value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-FIX-007")]
    [Requirement("FMT-PIPE-024")]
    public async Task VerifyFirstParameterOnTheLineAfterTheOpeningParenthesisStaysOnItsLineAndKeepsTheWrappedOperatorUnderTheValue()
    {
        const string testData = """
                                class C
                                {
                                    void M(
                                        {|#0:int a
                                            = 1 +
                                              2|})
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M(
                                         int a = 1
                                                 + 2)
                                     {
                                     }
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the fix of an enum member behind the opening brace keeps it on that line, where formatting the
    /// document would put it on a line of its own, and puts the wrapped binary operator under the value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-FIX-007")]
    [Requirement("FMT-PIPE-024")]
    public async Task VerifyEnumMemberBehindTheOpeningBraceStaysOnItsLineAndKeepsTheWrappedOperatorUnderTheValue()
    {
        const string testData = """
                                enum E { {|#0:A =
                                           1 +
                                           2|} }
                                """;
        const string fixedData = """
                                 enum E { A = 1
                                              + 2 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of the first declarator behind its type and several spaces keeps those spaces and puts the
    /// wrapped argument under the first argument
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyFirstDeclaratorBehindItsTypeAndSeveralSpacesKeepsTheWrappedArgumentUnderTheFirstArgument()
    {
        const string testData = """
                                class C
                                {
                                    static int Other(int x, int y)
                                    {
                                        return x + y;
                                    }

                                    void M()
                                    {
                                        int    {|#0:a =
                                            Other(1,
                                                  2)|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     static int Other(int x, int y)
                                     {
                                         return x + y;
                                     }

                                     void M()
                                     {
                                         int    a = Other(1,
                                                          2);
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of the first parameter behind the opening parenthesis and several spaces keeps those spaces
    /// and puts the wrapped binary operator under the value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyFirstParameterBehindTheOpeningParenthesisAndSeveralSpacesKeepsTheWrappedOperatorUnderTheValue()
    {
        const string testData = """
                                class C
                                {
                                    void M(   {|#0:int a
                                                  = 1 +
                                                    2|})
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M(   int a = 1
                                                       + 2)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of a parameter behind the parameter in front of it and several spaces keeps those spaces and
    /// puts the wrapped binary operator under the value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyParameterBehindAnotherParameterAndSeveralSpacesKeepsTheWrappedOperatorUnderTheValue()
    {
        const string testData = """
                                class C
                                {
                                    void M(int a,    {|#0:string c
                                                         = "x" +
                                                           "y"|})
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M(int a,    string c = "x"
                                                                 + "y")
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of a declarator behind the declarator in front of it and several spaces keeps those spaces and
    /// puts the wrapped argument under the first argument
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyDeclaratorBehindAnotherDeclaratorAndSeveralSpacesKeepsTheWrappedArgumentUnderTheFirstArgument()
    {
        const string testData = """
                                class C
                                {
                                    static int Other(int x, int y)
                                    {
                                        return x + y;
                                    }

                                    void M()
                                    {
                                        int a = 1,    {|#0:b =
                                            Other(1,
                                                  2)|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     static int Other(int x, int y)
                                     {
                                         return x + y;
                                     }

                                     void M()
                                     {
                                         int a = 1,    b = Other(1,
                                                                 2);
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of an enum member behind the enum member in front of it and several spaces keeps those spaces
    /// and puts the wrapped binary operator under the value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyEnumMemberBehindAnotherEnumMemberAndSeveralSpacesKeepsTheWrappedOperatorUnderTheValue()
    {
        const string testData = """
                                enum E
                                {
                                    A = 1,    {|#0:B =
                                        1 +
                                        2|}
                                }
                                """;
        const string fixedData = """
                                 enum E
                                 {
                                     A = 1,    B = 1
                                                   + 2
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of a parameter behind the closing line of a raw string literal that formatting moves keeps the
    /// literal's lines and the parameter as in the document and puts the wrapped binary operator under the value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyParameterBehindTheClosingLineOfARawStringKeepsTheWrappedOperatorUnderTheValue()
    {
        const string testData = """"
                                class C
                                {
                                    void M(string a = """
                                        x
                                        """, {|#0:string c =
                                        "y" +
                                        "z"|})
                                    {
                                    }
                                }
                                """";
        const string fixedData = """"
                                 class C
                                 {
                                     void M(string a = """
                                         x
                                         """, string c = "y"
                                                         + "z")
                                     {
                                     }
                                 }
                                 """";

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of the first parameter on the line after the opening parenthesis, in the column joining it to
    /// the parenthesis would give it, keeps it on its line and puts the wrapped binary operator under the value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    public async Task VerifyFirstParameterOnTheLineAfterTheOpeningParenthesisInTheJoinedColumnKeepsTheWrappedOperatorUnderTheValue()
    {
        const string testData = """
                                class C
                                {
                                    void M(
                                           {|#0:int a
                                               = 1 +
                                                 2|})
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M(
                                            int a = 1
                                                    + 2)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of a declarator one space behind the declarator in front of it puts the wrapped argument under
    /// the first argument
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    public async Task VerifyDeclaratorOneSpaceBehindAnotherDeclaratorKeepsTheWrappedArgumentUnderTheFirstArgument()
    {
        const string testData = """
                                class C
                                {
                                    static int Other(int x, int y)
                                    {
                                        return x + y;
                                    }

                                    void M()
                                    {
                                        int a = 1, {|#0:b =
                                            Other(1,
                                                  2)|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     static int Other(int x, int y)
                                     {
                                         return x + y;
                                     }

                                     void M()
                                     {
                                         int a = 1, b = Other(1,
                                                              2);
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of an enum member one space behind the enum member in front of it puts the wrapped binary
    /// operator under the value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    public async Task VerifyEnumMemberOneSpaceBehindAnotherEnumMemberKeepsTheWrappedOperatorUnderTheValue()
    {
        const string testData = """
                                enum E
                                {
                                    A = 1, {|#0:B =
                                        1 +
                                        2|}
                                }
                                """;
        const string fixedData = """
                                 enum E
                                 {
                                     A = 1, B = 1
                                                + 2
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of a parameter that starts a later line of its parameter list in the column formatting the
    /// document gives it puts the wrapped binary operator under the value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    public async Task VerifyParameterOnALaterLineInTheColumnOfTheFormatterKeepsTheWrappedOperatorUnderTheValue()
    {
        const string testData = """
                                class C
                                {
                                    void M(int a,
                                           {|#0:string c
                                               = "x" +
                                                 "y"|})
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M(int a,
                                            string c = "x"
                                                       + "y")
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of a declarator behind the last line of a multi-line comment that stands behind the
    /// separating comma is withheld: the fix formats the declarator in its declaration statement, and the formatter
    /// refuses that statement for the comment between the declaration's own tokens
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-010")]
    public async Task VerifyNoFixIsOfferedForADeclaratorBehindAMultiLineCommentBehindTheSeparatingComma()
    {
        const string testData = """
                                class C
                                {
                                    static int Other(int x, int y)
                                    {
                                        return x + y;
                                    }

                                    void M()
                                    {
                                        int a = 1, /* x
                                                      y */ {|RH5111:b =
                                            Other(1,
                                                  2)|};
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix of a parameter that starts a later line behind a comment line keeps the comment and the
    /// parameter in their columns and puts the wrapped binary operator under the value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-FIX-007")]
    [Requirement("FMT-PIPE-018")]
    public async Task VerifyParameterOnALaterLineBehindACommentLineKeepsTheCommentAndKeepsTheWrappedOperatorUnderTheValue()
    {
        const string testData = """
                                class C
                                {
                                    void M(int a,
                                                // c
                                                {|#0:string c
                                               = "x" +
                                                 "y"|})
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M(int a,
                                                 // c
                                                 string c = "x"
                                                            + "y")
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of an enum member whose documentation comment is written behind the member in front of it
    /// keeps that line as written, keeps the member in its column, and puts the wrapped binary operator under the value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-FIX-007")]
    [Requirement("FMT-PIPE-018")]
    public async Task VerifyEnumMemberWithDocumentationCommentBehindThePreviousMemberKeepsThatLineAndKeepsTheWrappedOperatorUnderTheValue()
    {
        const string testData = """
                                enum E
                                {
                                    A, /// Doc
                                      {|#0:B =
                                      1 +
                                      2|},
                                    C,
                                }
                                """;
        const string fixedData = """
                                 enum E
                                 {
                                     A, /// Doc
                                       B = 1
                                           + 2,
                                     C,
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the fix of an enum member behind a conditional directive and a blank line keeps both and the member's
    /// column as in the document and puts the wrapped binary operator under the value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-FIX-007")]
    [Requirement("FMT-PIPE-018")]
    public async Task VerifyEnumMemberBehindConditionalDirectiveKeepsTheDirectiveAndKeepsTheWrappedOperatorUnderTheValue()
    {
        const string testData = """
                                enum E
                                {
                                    A,
                                  #if !DEBUG

                                      {|#0:B =
                                      1 +
                                      2|},
                                #endif
                                    C,
                                }
                                """;
        const string fixedData = """
                                 enum E
                                 {
                                     A,
                                   #if !DEBUG

                                       B = 1
                                           + 2,
                                 #endif
                                     C,
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that fixing two enum members that start later lines of their enum, also through Fix All, keeps both members
    /// in their columns and puts each wrapped binary operator under its value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    [Requirement("FMT-PIPE-017")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyTwoEnumMembersOnLaterLinesAreFixedTogetherAndKeepTheWrappedOperatorsUnderTheirValues()
    {
        const string testData = """
                                enum E
                                {
                                    A = 1,
                                      {|#0:B =
                                      1 +
                                      2|},
                                      {|#1:C =
                                      3 +
                                      4|}
                                }
                                """;
        const string fixedData = """
                                 enum E
                                 {
                                     A = 1,
                                       B = 1
                                           + 2,
                                       C = 3
                                           + 4
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    /// <summary>
    /// Verifies the specification example: an enum member's <c>=</c> below its name is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-001")]
    public async Task VerifyEnumMemberEqualsBelowNameIsReported()
    {
        const string testData = """
                                enum E
                                {
                                    {|#0:A
                                        = 1|}
                                }
                                """;

        await Verify(testData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of an enum member whose <c>=</c> stands below its name joins the member in one application and
    /// keeps the trailing comma, which belongs to the enum, not to the member
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-001")]
    [Requirement("RH5111-004")]
    public async Task VerifyEnumMemberEqualsBelowNameIsFixedInOneApplication()
    {
        const string testData = """
                                enum E
                                {
                                    {|#0:A
                                        = 1|},
                                }
                                """;
        const string fixedData = """
                                 enum E
                                 {
                                     A = 1,
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     static config =>
                            {
                                config.NumberOfIncrementalIterations = 1;
                                config.NumberOfFixAllIterations = 1;
                            },
                     Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
        await Verify(NormalizeToCarriageReturnLineFeed(testData),
                     NormalizeToCarriageReturnLineFeed(fixedData),
                     static config =>
                            {
                                config.NumberOfIncrementalIterations = 1;
                                config.NumberOfFixAllIterations = 1;
                            },
                     Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of an enum member whose <c>=</c> stands alone on its line joins name, <c>=</c>, and value in one
    /// application
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    public async Task VerifyLoneEnumMemberEqualsIsFixedInOneApplication()
    {
        const string testData = """
                                enum E
                                {
                                    {|#0:A
                                        =
                                        1|},
                                }
                                """;
        const string fixedData = """
                                 enum E
                                 {
                                     A = 1,
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     static config =>
                            {
                                config.NumberOfIncrementalIterations = 1;
                                config.NumberOfFixAllIterations = 1;
                            },
                     Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that Fix All joins two enum members whose <c>=</c> stands below the name in one iteration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-004")]
    public async Task VerifyFixAllJoinsSeveralEnumMembers()
    {
        const string testData = """
                                enum E
                                {
                                    {|#0:A
                                        = 1|},
                                    {|#1:B
                                        = 2|}
                                }
                                """;
        const string fixedData = """
                                 enum E
                                 {
                                     A = 1,
                                     B = 2
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     static config => config.NumberOfFixAllIterations = 1,
                     Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that in a CRLF document the fix writes the LF behind the last member of an enum, which it joins, as CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-010")]
    [Requirement("SHARED-FIX-008")]
    public async Task FixWritesTheDetectedLineEndingBehindTheLastEnumMemberInAMixedDocument()
    {
        const string testData = "enum E\r\n{\r\n    A = 1,\r\n    B =\r\n    2\n}\r\n";
        const string resultData = "enum E\r\n{\r\n    A = 1,\r\n    B = 2\r\n}\r\n";

        Assert.AreEqual(resultData, await ApplyCodeFixAsync(testData));
    }

    /// <summary>
    /// Verifies that the fix removes the whitespace behind the last member of an enum, which it joins, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">The line ending of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("FMT-PIPE-010")]
    public async Task FixRemovesWhitespaceBehindTheLastEnumMember(string endOfLine)
    {
        const string testData = "enum E\n{\n    A = 1,\n    {|RH5111:B =\n    2|}   \n}\n";
        const string resultData = "enum E\n{\n    A = 1,\n    B = 2\n}\n";

        await VerifyCodeFixWithEndOfLine(testData, resultData, endOfLine);
    }

    /// <summary>
    /// Verifies the spec example: a comment or a <c>#pragma</c> directive in each wrapped gap around the equals sign of an enum member, a property, a declarator, or an assignment keeps the report, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-005")]
    [Requirement("SHARED-DIAG-010")]
    public async Task VerifyCommentOrPragmaInEveryWrappedGapKeepsTheReportWithoutCodeFix()
    {
        const string testData = """
                                enum E
                                {
                                    {|RH5111:A // c
                                        = 1|},
                                }

                                class C
                                {
                                    {|RH5111:int Q { get; set; } // c
                                        = 5;|}

                                    void M()
                                    {
                                        var {|RH5111:y // c
                                            = 1|};
                                        int z;
                                        {|RH5111:z = /* c */
                                            2|};
                                        {|RH5111:z
                                #pragma warning disable CS0168
                                            = 3|};
                                    }

                                    void N()
                                    {
                                        int /* t */ {|RH5111:x
                                #pragma warning disable CS0168
                                            = 1|};
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a region directive pair in the wrapped gap in front of the equals sign of an assignment, a comment on a line of its own behind the equals sign, and a comment behind a parameter in front of its wrapped default value keep the report, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-005")]
    [Requirement("SHARED-DIAG-010")]
    public async Task VerifyRegionDirectiveOrCommentInTheWrappedGapKeepsTheReportWithoutCodeFix()
    {
        const string testData = """
                                class C
                                {
                                    void M()
                                    {
                                        int z;
                                        {|RH5111:z
                                #region Value
                                #endregion
                                            = 3|};
                                        {|RH5111:z =
                                            // c
                                            4|};
                                    }

                                    void N({|RH5111:int a // c
                                               = 1|})
                                    {
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies the spec example: the fix of a declarator in the body of an assigned lambda joins only the declarator,
    /// while the assignment and the other statement of the lambda stay as written
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5111-004")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixOfDeclaratorInAssignedLambdaLeavesTheAssignmentAsWritten(string endOfLine)
    {
        const string testData = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action a;
                                        a = () => { var {|RH5111:x
                                            = 1|}; int  y = x; };
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Action a;
                                         a = () => { var x = 1; int  y = x; };
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies the spec example: the one-line documentation comment above the property whose initializer the fix joins
    /// stays as written
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5111-004")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixKeepsTheOneLineDocumentationCommentAboveTheProperty(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    /// <summary>P</summary>
                                    {|RH5111:int P { get; set; }
                                        = 5;|}
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     /// <summary>P</summary>
                                     int P { get; set; } = 5;
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix of a default value of a local function parameter in the body of an assigned lambda joins
    /// only the parameter, while the assignment, the local function, and the other statement of the lambda stay as
    /// written
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5111-004")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixOfParameterDefaultInAssignedLambdaLeavesTheAssignmentAsWritten(string endOfLine)
    {
        const string testData = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action a;
                                        a = () => { int L({|RH5111:int p
                                            = 1|}) => p; int  y = L(); };
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Action a;
                                         a = () => { int L(int p = 1) => p; int  y = L(); };
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix of a declarator in the body of an assigned anonymous method joins only the declarator,
    /// while the assignment and the other statement stay as written
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5111-004")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixOfDeclaratorInAssignedAnonymousMethodLeavesTheAssignmentAsWritten(string endOfLine)
    {
        const string testData = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Action a;
                                        a = delegate { var {|RH5111:x
                                            = 1|}; int  y = x; };
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System;

                                 class C
                                 {
                                     void M()
                                     {
                                         Action a;
                                         a = delegate { var x = 1; int  y = x; };
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix of a declarator in the body of a lambda assigned to a member in an object initializer
    /// joins only the declarator, while the member assignment and the other statement stay as written
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5111-004")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixOfDeclaratorInLambdaOfObjectInitializerLeavesTheMemberAssignmentAsWritten(string endOfLine)
    {
        const string testData = """
                                using System;

                                class C
                                {
                                    Action X { get; set; }

                                    void M()
                                    {
                                        var c = new C { X = () => { var {|RH5111:x
                                            = 1|}; int  y = x; } };
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System;

                                 class C
                                 {
                                     Action X { get; set; }

                                     void M()
                                     {
                                         var c = new C { X = () => { var x = 1; int  y = x; } };
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix of an assignment in the body of an assigned lambda joins only that assignment, while the
    /// outer assignment and the other statement stay as written
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5111-004")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixOfAssignmentInAssignedLambdaJoinsOnlyTheInnerAssignment(string endOfLine)
    {
        const string testData = """
                                using System;

                                class C
                                {
                                    void M(int b)
                                    {
                                        Action a;
                                        a = () => { {|RH5111:b
                                            = 1|}; int  y = 2; };
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System;

                                 class C
                                 {
                                     void M(int b)
                                     {
                                         Action a;
                                         a = () => { b = 1; int  y = 2; };
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix of a declarator in the body of a lambda that initializes a field joins only the
    /// declarator, while the other statement of the lambda stays as written
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5111-004")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixOfDeclaratorInLambdaOfFieldInitializerLeavesTheLambdaAsWritten(string endOfLine)
    {
        const string testData = """
                                using System;

                                class C
                                {
                                    Action _a = () => { var {|RH5111:x
                                        = 1|}; int  y = x; };
                                }
                                """;
        const string fixedData = """
                                 using System;

                                 class C
                                 {
                                     Action _a = () => { var x = 1; int  y = x; };
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that no blank line is inserted above a comment that leads the assignment statement whose value the fix
    /// joins, where formatting the document would insert one
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixInsertsNoBlankLineAboveCommentThatLeadsTheAssignment(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void M()
                                    {
                                        int b = 0;
                                        // c
                                        {|RH5111:b
                                            = 1|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M()
                                     {
                                         int b = 0;
                                         // c
                                         b = 1;
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that a property whose initializer the fix joins, and which shares its line with a field in front of it,
    /// starts on the next line in the column of that line
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyFixGivesAPropertyThatSharesItsLineWithAFieldInFrontALineOfItsOwn(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    private int _a; {|RH5111:public int P { get; } =
                                        1;|}
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     private int _a;
                                     public int P { get; } = 1;
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that a property whose initializer the fix joins, and which shares its line with a field behind it, keeps
    /// its own line, and that the field behind it starts on the next line
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyFixGivesAPropertyThatSharesItsLineWithAFieldBehindItALineOfItsOwn(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    {|RH5111:public int P { get; } =
                                        1;|} private int _b;
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     public int P { get; } = 1;
                                     private int _b;
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix, which formats the property through the formatter, joins the initializer of a property whose
    /// setter carries two <c>param:</c> lists, and that the lists come back merged with one space behind the comma, under LF
    /// and CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixForPropertyWithMergedSetterParamListsKeepsTheCommaSpace()
    {
        const string testData = """
                                class TestClass
                                {
                                    {|#0:int Property { get; [param: A] [param: B] set; }
                                        = 1;|}
                                }
                                sealed class A : System.Attribute
                                {
                                }
                                sealed class B : System.Attribute
                                {
                                }
                                """;

        const string fixedData = """
                                 class TestClass
                                 {
                                     int Property { get; [param: A, B] set; } = 1;
                                 }
                                 sealed class A : System.Attribute
                                 {
                                 }
                                 sealed class B : System.Attribute
                                 {
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testData, fixedData);
    }

    /// <summary>
    /// Verifies the one diagnostic marked in the source and the fix result under LF and under CRLF
    /// </summary>
    /// <param name="testData">The source with LF line endings</param>
    /// <param name="fixedData">The fixed source with LF line endings</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task VerifyUnderBothLineEndings(string testData, string fixedData)
    {
        await Verify(testData, fixedData, Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(fixedData), Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat));
    }

    #endregion // Tests
}