using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5111AssignmentsMustHaveProperLineBreaksCodeFixProvider"/> on assignments behind a conditional access, whose wrapped members the fix lays out under the anchor
/// </summary>
[TestClass]
public class RH5111AssignmentsMustHaveProperLineBreaksConditionalAccessTests : BatchCodeFixTestsBase<RH5111AssignmentsMustHaveProperLineBreaksAnalyzer, RH5111AssignmentsMustHaveProperLineBreaksCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that the fix of an assignment behind a conditional access whose assigned member stands on its own line behind a
    /// member binding puts the member under the <c>?</c>, where formatting the document puts it, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-006")]
    public async Task VerifyFixAlignsAWrappedMemberBehindAMemberBindingUnderTheAnchor()
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
                                     }
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the fix keeps a wrapped assigned member behind a member binding that already stands in the anchor's column,
    /// under the <c>?</c>, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-006")]
    public async Task VerifyFixKeepsAWrappedMemberThatStandsInTheAnchorColumn()
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
                                     }
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the fix moves a wrapped assigned member written left of the anchor's column into that column, under LF and
    /// CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-006")]
    public async Task VerifyFixMovesAWrappedMemberLeftOfTheAnchorIntoTheAnchorColumn()
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
                                     }
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the fix moves a wrapped assigned member written right of the assignment's first token into the anchor's
    /// column, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-006")]
    public async Task VerifyFixMovesAWrappedMemberRightOfTheAssignmentStartIntoTheAnchorColumn()
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
                                     }
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the fix of an assignment behind a conditional access whose assigned member stands on its own line behind an
    /// element binding joins the member to the line of the binding, as formatting the document does, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-006")]
    public async Task VerifyFixJoinsAMemberBehindAnElementBinding()
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
                                         a?[0].C = 5;
                                     }
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the fix of an assignment behind a nested conditional access whose assigned member stands on its own line puts
    /// the member under the second <c>?</c>, the last link in front of it, where formatting the document puts it, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-006")]
    public async Task VerifyFixAlignsAWrappedMemberBehindANestedConditionalAccessUnderTheAnchor()
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
                                        a?.B?{|#0:.B
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
                                         a?.B?.B
                                             .C = 5;
                                     }
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the fix moves a wrapped assigned member written under the dot of the last binding of a nested conditional
    /// access into the anchor's column, under the <c>?</c> of that binding, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-006")]
    public async Task VerifyFixMovesAWrappedMemberUnderTheLastBindingOfANestedConditionalAccessIntoTheAnchorColumn()
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
                                        a?.B?{|#0:.B
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
                                         a?.B?.B
                                             .C = 5;
                                     }
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the fix of an assignment behind a conditional access whose assigned member stands on its own line behind an
    /// invoked binding puts the member under the <c>?</c>, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-006")]
    public async Task VerifyFixAlignsAWrappedMemberBehindAnInvokedBindingUnderTheAnchor()
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
                                        a?{|#0:.N()
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
                                         a?.N()
                                          .C = 5;
                                     }
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the fix of an assignment behind a conditional access whose assigned member and whose operator each stand on
    /// their own line puts the member under the <c>?</c> and joins the operator and the value to it, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-006")]
    public async Task VerifyFixAlignsAWrappedMemberWhoseOperatorIsWrappedUnderTheAnchor()
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
                                          .C
                                            = 5|};
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
                                     }
                                 }
                                 """;

        await VerifyUnderBothLineEndings(testData, fixedData);
    }

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
    /// <c>?</c> puts the wrapped member under that <c>?</c>, the last link in front of it, under LF and CRLF
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
    /// Verifies that the fix of an assignment behind a conditional access with four links in front of its wrapped member,
    /// two of them in front of the <c>?</c>, puts the member under the last of them, not under the second, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5111-006")]
    public async Task VerifyFixAlignsAWrappedMemberBehindFourLinksAroundAConditionalAccessUnderTheLastOfThem()
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
                                        a.B.B?{|#0:.B.B
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
                                         a.B.B?.B.B
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

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                namespace TestNamespace
                                {
                                    class TestClass
                                    {
                                        public void TestMethod()
                                        {
                                            int {|#0:first
                                                = 1|}, {|#1:second
                                                = 2|};
                                        }
                                    }
                                }
                                """;

        const string fixedData = """
                                 namespace TestNamespace
                                 {
                                     class TestClass
                                     {
                                         public void TestMethod()
                                         {
                                             int first = 1, second = 2;
                                         }
                                     }
                                 }
                                 """;

        // Verifies that Fix All converges for multiple declarators in the same declaration
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId, MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}