using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5514MethodAttributeListsMustFollowShapeRulesAnalyzer"/> and <see cref="RH5514MethodAttributeListsMustFollowShapeRulesCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5514MethodAttributeListsMustFollowShapeRulesAnalyzerTests : BatchCodeFixTestsBase<RH5514MethodAttributeListsMustFollowShapeRulesAnalyzer, RH5514MethodAttributeListsMustFollowShapeRulesCodeFixProvider>
{
    #region Constants

    /// <summary>
    /// Declarations of the attributes the split examples use, appended to each example
    /// </summary>
    private const string AttributeDeclarations = """

                                                 sealed class A : System.Attribute
                                                 {
                                                 }
                                                 sealed class B : System.Attribute
                                                 {
                                                 }
                                                 sealed class D2 : System.Attribute
                                                 {
                                                 }
                                                 """;

    #endregion // Constants

    #region Tests

    /// <summary>
    /// Verifies that policy violations are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5514-001")]
    [Requirement("RH5514-003")]
    [Requirement("RH5514-004")]
    [Requirement("SHARED-ATTR-013")]
    [Requirement("SHARED-ATTR-016")]
    [Requirement("SHARED-ATTR-017")]
    public async Task VerifyDiagnosticAndCodeFixForPolicyViolation()
    {
        const string testData = """
                                internal class Example
                                {
                                    {|#0:[First, Second]|}
                                    internal void M() { }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     [First]
                                     [Second]
                                     internal void M() { }
                                 }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 sealed class SecondAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5514MethodAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5514MessageFormat));
    }

    /// <summary>
    /// Verifies that compliant code is not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5514-001")]
    [Requirement("SHARED-ATTR-013")]
    public async Task VerifyNoDiagnosticForCompliantCode()
    {
        const string testData = """
                                internal class Example
                                {
                                    [First]
                                    [Second]
                                    internal void M() { }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that commented violations are still reported without offering an unsafe code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5514-005")]
    [Requirement("SHARED-ATTR-019")]
    public async Task VerifyDiagnosticWithoutCodeFixWhenCommentsArePresent()
    {
        const string testData = """
                                internal class Example
                                {
                                    {|#0:[First, /* keep */ Second]|}
                                    internal void M() { }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;
        const string codeFixData = """
                                   internal class Example
                                   {
                                       [First, /* keep */ Second]
                                       internal void M() { }
                                   }
                                   sealed class FirstAttribute : System.Attribute
                                   {
                                   }
                                   sealed class SecondAttribute : System.Attribute
                                   {
                                   }
                                   """;

        await Verify(testData,
                     Diagnostics(RH5514MethodAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5514MessageFormat));

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5514MethodAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<AttributeListSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that the multi-attribute lists of a method, a local function, a lambda, an operator, a destructor, and a <c>method:</c> list are reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5514-001")]
    [Requirement("RH5514-003")]
    [Requirement("SHARED-ATTR-013")]
    [Requirement("SHARED-ATTR-016")]
    public async Task VerifyDiagnosticForMultiAttributeListsOfEveryMethodKind()
    {
        const string testData = """
                                class C
                                {
                                    {|#0:[A, B]|}
                                    void M()
                                    {
                                        {|#1:[A, B]|}
                                        void L()
                                        {
                                        }

                                        System.Action a = {|#2:[A, B]|} () => { };
                                    }

                                    {|#3:[A, B]|}
                                    public static C operator +(C a, C b) => a;

                                    {|#4:[A, B]|}
                                    ~C()
                                    {
                                    }

                                    {|#5:[method: A, B]|}
                                    void N()
                                    {
                                    }
                                }
                                sealed class A : System.Attribute
                                {
                                }
                                sealed class B : System.Attribute
                                {
                                }
                                """;

        await Verify(testData,
                     Diagnostics(RH5514MethodAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5514MessageFormat, 6));
    }

    /// <summary>
    /// Verifies that a multi-attribute list of an accessor is left to the accessor rule, although it resolves to the method target
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5514-002")]
    public async Task VerifyNoDiagnosticForMultiAttributeAccessorList()
    {
        const string testData = """
                                class C
                                {
                                    int P
                                    {
                                        [A, B]
                                        get;
                                        set;
                                    }
                                }
                                sealed class A : System.Attribute
                                {
                                }
                                sealed class B : System.Attribute
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a multi-attribute list is reported both on its own line and on the line of its method, while single-attribute lists are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5514-001")]
    [Requirement("SHARED-ATTR-013")]
    public async Task VerifyDiagnosticForMultiAttributeListWhetherOnItsOwnLineOrSharingIt()
    {
        const string testData = """
                                class C
                                {
                                    {|#0:[A, B]|}
                                    void M()
                                    {
                                    }

                                    {|#1:[A, B]|} void N()
                                    {
                                    }

                                    [A]
                                    [B]
                                    void O()
                                    {
                                    }
                                }
                                sealed class A : System.Attribute
                                {
                                }
                                sealed class B : System.Attribute
                                {
                                }
                                """;

        await Verify(testData,
                     Diagnostics(RH5514MethodAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5514MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that the code fix replaces the method attribute list by one list per attribute, each on its own line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5514-004")]
    [Requirement("SHARED-ATTR-017")]
    public async Task VerifyCodeFixGivesEachMethodAttributeAListOfItsOwn()
    {
        const string testData = """
                                class C
                                {
                                    {|#0:[A, B]|}
                                    void M()
                                    {
                                    }
                                }
                                sealed class A : System.Attribute
                                {
                                }
                                sealed class B : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     [A]
                                     [B]
                                     void M()
                                     {
                                     }
                                 }
                                 sealed class A : System.Attribute
                                 {
                                 }
                                 sealed class B : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5514MethodAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5514MessageFormat));
    }

    /// <summary>
    /// Verifies that the code fix keeps a documentation comment in front of the first new list and a trailing comment after the last one, inside a namespace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5514-004")]
    [Requirement("SHARED-ATTR-017")]
    public async Task VerifyCodeFixKeepsDocumentationCommentAndTrailingCommentAroundTheSplitLists()
    {
        const string testData = """
                                namespace N
                                {
                                    class C
                                    {
                                        /// <summary>M</summary>
                                        {|#0:[A, B]|}
                                        void M()
                                        {
                                        }

                                        {|#1:[method: A, B]|} // c
                                        void O()
                                        {
                                        }
                                    }
                                }
                                sealed class A : System.Attribute
                                {
                                }
                                sealed class B : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 namespace N
                                 {
                                     class C
                                     {
                                         /// <summary>M</summary>
                                         [A]
                                         [B]
                                         void M()
                                         {
                                         }

                                         [method: A]
                                         [method: B] // c
                                         void O()
                                         {
                                         }
                                     }
                                 }
                                 sealed class A : System.Attribute
                                 {
                                 }
                                 sealed class B : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5514MethodAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5514MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that the code fix splits the list of a local function in a switch section at the indentation of the section's statements
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5514-004")]
    [Requirement("SHARED-ATTR-017")]
    public async Task VerifyCodeFixSplitsLocalFunctionListInsideSwitchSection()
    {
        const string testData = """
                                class C
                                {
                                    void M(int i)
                                    {
                                        switch (i)
                                        {
                                            case 1:
                                                {|#0:[A, B]|}
                                                void L()
                                                {
                                                }
                                                break;
                                        }
                                    }
                                }
                                sealed class A : System.Attribute
                                {
                                }
                                sealed class B : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M(int i)
                                     {
                                         switch (i)
                                         {
                                             case 1:
                                                 [A]
                                                 [B]
                                                 void L()
                                                 {
                                                 }
                                                 break;
                                         }
                                     }
                                 }
                                 sealed class A : System.Attribute
                                 {
                                 }
                                 sealed class B : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5514MethodAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5514MessageFormat));
    }

    /// <summary>
    /// Verifies that the code fix replaces the method attribute list by one list per attribute, each on its own line, under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5514-004")]
    [Requirement("SHARED-ATTR-017")]
    [Requirement("SHARED-FIX-008")]
    public async Task VerifyCodeFixGivesEachMethodAttributeAListOfItsOwnWithCarriageReturnLineFeed()
    {
        const string testData = """
                                class C
                                {
                                    {|#0:[A, B]|}
                                    void M()
                                    {
                                    }
                                }
                                sealed class A : System.Attribute
                                {
                                }
                                sealed class B : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     [A]
                                     [B]
                                     void M()
                                     {
                                     }
                                 }
                                 sealed class A : System.Attribute
                                 {
                                 }
                                 sealed class B : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData),
                     NormalizeToCarriageReturnLineFeed(fixedData),
                     Diagnostics(RH5514MethodAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5514MessageFormat));

        Assert.AreEqual(NormalizeToCarriageReturnLineFeed(fixedData), await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(RemoveMarkup(testData))), "Every line break the fix inserts must be CRLF.");
    }

    /// <summary>
    /// Verifies that the code fix keeps a documentation comment in front of the first new list and a trailing comment after the last one, inside a namespace, under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5514-004")]
    [Requirement("SHARED-ATTR-017")]
    public async Task VerifyCodeFixKeepsDocumentationCommentAndTrailingCommentAroundTheSplitListsWithCarriageReturnLineFeed()
    {
        const string testData = """
                                namespace N
                                {
                                    class C
                                    {
                                        /// <summary>M</summary>
                                        {|#0:[A, B]|}
                                        void M()
                                        {
                                        }

                                        {|#1:[method: A, B]|} // c
                                        void O()
                                        {
                                        }
                                    }
                                }
                                sealed class A : System.Attribute
                                {
                                }
                                sealed class B : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 namespace N
                                 {
                                     class C
                                     {
                                         /// <summary>M</summary>
                                         [A]
                                         [B]
                                         void M()
                                         {
                                         }

                                         [method: A]
                                         [method: B] // c
                                         void O()
                                         {
                                         }
                                     }
                                 }
                                 sealed class A : System.Attribute
                                 {
                                 }
                                 sealed class B : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData),
                     NormalizeToCarriageReturnLineFeed(fixedData),
                     Diagnostics(RH5514MethodAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5514MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that the code fix splits the list of a local function in a switch section at the indentation of the section's statements, under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5514-004")]
    [Requirement("SHARED-ATTR-017")]
    public async Task VerifyCodeFixSplitsLocalFunctionListInsideSwitchSectionWithCarriageReturnLineFeed()
    {
        const string testData = """
                                class C
                                {
                                    void M(int i)
                                    {
                                        switch (i)
                                        {
                                            case 1:
                                                {|#0:[A, B]|}
                                                void L()
                                                {
                                                }
                                                break;
                                        }
                                    }
                                }
                                sealed class A : System.Attribute
                                {
                                }
                                sealed class B : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M(int i)
                                     {
                                         switch (i)
                                         {
                                             case 1:
                                                 [A]
                                                 [B]
                                                 void L()
                                                 {
                                                 }
                                                 break;
                                         }
                                     }
                                 }
                                 sealed class A : System.Attribute
                                 {
                                 }
                                 sealed class B : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData),
                     NormalizeToCarriageReturnLineFeed(fixedData),
                     Diagnostics(RH5514MethodAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5514MessageFormat));
    }

    /// <summary>
    /// Verifies the spec example: each further list starts a new line in the column of the reported list, behind a
    /// documentation comment and in front of a trailing comment, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5514-004")]
    [Requirement("SHARED-ATTR-017")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task SplitFixStartsEachFurtherListOnANewLine(string endOfLine)
    {
        const string testData = """
                                namespace N
                                {
                                    class C
                                    {
                                        /// <summary>M</summary>
                                        {|RH5514:[A, B]|}
                                        void M()
                                        {
                                        }

                                        {|RH5514:[method: A, B]|} // c
                                        void O()
                                        {
                                        }
                                    }
                                }
                                """;
        const string fixedData = """
                                 namespace N
                                 {
                                     class C
                                     {
                                         /// <summary>M</summary>
                                         [A]
                                         [B]
                                         void M()
                                         {
                                         }

                                         [method: A]
                                         [method: B] // c
                                         void O()
                                         {
                                         }
                                     }
                                 }
                                 """;

        await VerifySplitFix(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies the spec example: braces indented differently from the list, a directive and comments above and below the
    /// list keep their columns, and the new list starts in the column of the reported one, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5514-004")]
    [Requirement("SHARED-ATTR-017")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task SplitFixKeepsTheCodeAroundTheListAsWritten(string endOfLine)
    {
        const string testData = """
                                namespace N
                                {
                                class C
                                {
                                #pragma warning disable CS0001
                                // c
                                    {|RH5514:[A, B]|}
                                        // d
                                    void M()
                                    {
                                    }
                                }
                                }
                                """;
        const string fixedData = """
                                 namespace N
                                 {
                                 class C
                                 {
                                 #pragma warning disable CS0001
                                 // c
                                     [A]
                                     [B]
                                         // d
                                     void M()
                                     {
                                     }
                                 }
                                 }
                                 """;

        await VerifySplitFix(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies the spec example: whitespace at the end of the line above the list, on a line that holds only whitespace,
    /// and behind the reported list stays, the last behind the last new list, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5514-004")]
    [Requirement("SHARED-ATTR-017")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task SplitFixKeepsTheWhitespaceAtTheEndOfTheLinesAroundTheList(string endOfLine)
    {
        var testData = ExpandWhitespaceMarkers("""
                                               class C
                                               {
                                                   int _f;···
                                               ····
                                                   {|RH5514:[A, B]|}···
                                                   void M()
                                                   {
                                                   }
                                               }
                                               """);
        var fixedData = ExpandWhitespaceMarkers("""
                                                class C
                                                {
                                                    int _f;···
                                                ····
                                                    [A]
                                                    [B]···
                                                    void M()
                                                    {
                                                    }
                                                }
                                                """);

        await VerifySplitFix(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies the spec example: only the reported list behind another list on its line is split, and the declaration
    /// behind it stays on the line of the last new list, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5514-004")]
    [Requirement("SHARED-ATTR-017")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task SplitFixKeepsAListInFrontOfTheReportedListOnItsLine(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    [A] {|RH5514:[B, D2]|} void M()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     [A] [B]
                                     [D2] void M()
                                     {
                                     }
                                 }
                                 """;

        await VerifySplitFix(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies the spec example: a list behind the reported list on its line stays behind the last new list, under LF and
    /// CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5514-004")]
    [Requirement("SHARED-ATTR-017")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task SplitFixKeepsAListBehindTheReportedListOnItsLine(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    {|RH5514:[A, B]|} [D2] void N()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     [A]
                                     [B] [D2] void N()
                                     {
                                     }
                                 }
                                 """;

        await VerifySplitFix(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies the spec example: a comment in front of the reported list stays in front of the first new list, under LF
    /// and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5514-004")]
    [Requirement("SHARED-ATTR-017")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task SplitFixKeepsACommentInFrontOfTheReportedListOnItsLine(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    /* c */ {|RH5514:[A, B]|}
                                    void O()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     /* c */ [A]
                                     [B]
                                     void O()
                                     {
                                     }
                                 }
                                 """;

        await VerifySplitFix(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies the spec example: a return-value list in front of the reported list stays where it is, and the expression
    /// body stays behind the last new list, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5514-004")]
    [Requirement("SHARED-ATTR-017")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task SplitFixKeepsAReturnValueListInFrontOfTheReportedListOnItsLine(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    [return: A] {|RH5514:[A, B]|} int R() => 0;
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     [return: A] [A]
                                     [B] int R() => 0;
                                 }
                                 """;

        await VerifySplitFix(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that the list of a lambda is split behind the whitespace at the start of its line, and the lambda stays
    /// behind the last new list, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5514-004")]
    [Requirement("SHARED-ATTR-017")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task SplitFixStartsTheFurtherListOfALambdaBehindTheWhitespaceOfItsLine(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void M()
                                    {
                                        System.Action a = {|RH5514:[A, B]|} () => { };
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M()
                                     {
                                         System.Action a = [A]
                                         [B] () => { };
                                     }
                                 }
                                 """;

        await VerifySplitFix(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies the spec example: the further list of a tab-indented list starts behind a tab, and the tabs in front of
    /// the declaration and the method braces stay, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5514-004")]
    [Requirement("SHARED-ATTR-017")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task SplitFixCopiesTheTabInFrontOfTheReportedList(string endOfLine)
    {
        var testData = ExpandWhitespaceMarkers("""
                                               class C
                                               {
                                               →{|RH5514:[A, B]|}
                                               →void M()
                                               →{
                                               →}
                                               }
                                               """);
        var fixedData = ExpandWhitespaceMarkers("""
                                                class C
                                                {
                                                →[A]
                                                →[B]
                                                →void M()
                                                →{
                                                →}
                                                }
                                                """);

        await VerifySplitFix(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies the spec example: the further list of a list behind a tab and four spaces starts behind a tab and four
    /// spaces
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5514-004")]
    [Requirement("SHARED-ATTR-017")]
    public async Task SplitFixCopiesATabAndFourSpacesInFrontOfTheReportedList()
    {
        var testData = ExpandWhitespaceMarkers("""
                                               class C
                                               {
                                               →····{|RH5514:[A, B]|}
                                               →····void M()
                                               →····{
                                               →····}
                                               }
                                               """);
        var fixedData = ExpandWhitespaceMarkers("""
                                                class C
                                                {
                                                →····[A]
                                                →····[B]
                                                →····void M()
                                                →····{
                                                →····}
                                                }
                                                """);

        await VerifySplitFix(testData, fixedData, "\n");
    }

    /// <summary>
    /// Verifies the spec example: in a document whose line breaks are all a lone carriage return, the line break the fix
    /// inserts is LF, and every other line break stays a lone carriage return
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ATTR-017")]
    [Requirement("SHARED-FIX-008")]
    public async Task SplitFixInsertsLineFeedInADocumentWhoseLineBreaksAreLoneCarriageReturns()
    {
        const string testData = """
                                class C
                                {
                                    [A, B]
                                    void M()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     [A]LF
                                     [B]
                                     void M()
                                     {
                                     }
                                 }
                                 """;

        var expected = ConvertEndOfLine(fixedData + AttributeDeclarations, "\r").Replace("LF\r", "\n", System.StringComparison.Ordinal);

        Assert.AreEqual(expected, await ApplyCodeFixAsync(ConvertEndOfLine(testData + AttributeDeclarations, "\r")));
    }

    /// <summary>
    /// Verifies that in a document whose line breaks are CRLF, except the line break of the list's line, which is LF, the
    /// line break the fix inserts is CRLF, the detected line ending, and the LF stays behind the last new list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ATTR-017")]
    [Requirement("SHARED-FIX-008")]
    public async Task SplitFixInsertsTheDetectedLineEndingWhenTheListLineEndsInAnotherOne()
    {
        const string testData = """
                                class C
                                {
                                    [A, B]LF
                                    void M()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     [A]
                                     [B]LF
                                     void M()
                                     {
                                     }
                                 }
                                 """;

        var expected = NormalizeToCarriageReturnLineFeed(fixedData + AttributeDeclarations).Replace("LF\r\n", "\n", System.StringComparison.Ordinal);

        Assert.AreEqual(expected, await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(testData + AttributeDeclarations).Replace("LF\r\n", "\n", System.StringComparison.Ordinal)));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Verifies the diagnostics marked in the source and the result of applying the split fix to each of them, with the
    /// attribute declarations appended and every line break written as <paramref name="endOfLine"/>, both through the
    /// verifier and on the raw text, line endings included
    /// </summary>
    /// <param name="testData">Source with markup, without the attribute declarations</param>
    /// <param name="fixedData">Expected result, without the attribute declarations</param>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task VerifySplitFix(string testData, string fixedData, string endOfLine)
    {
        var source = ConvertEndOfLine(testData + AttributeDeclarations, endOfLine);
        var fixedSource = ConvertEndOfLine(fixedData + AttributeDeclarations, endOfLine);

        await Verify(source, fixedSource);

        var actual = RemoveMarkup(source);
        var diagnosticCount = source.Split("{|RH5514:").Length - 1;

        for (var index = 0; index < diagnosticCount; index++)
        {
            actual = await ApplyCodeFixAsync(actual);
        }

        Assert.AreEqual(fixedSource, actual);
    }

    #endregion // Methods

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class Example
                                {
                                    {|#0:[First, Second]|} {|#1:[Third, Fourth]|}
                                    internal void M() { }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                sealed class ThirdAttribute : System.Attribute
                                {
                                }
                                sealed class FourthAttribute : System.Attribute
                                {
                                }
                                """;

        const string fixedCode = """
                                 internal class Example
                                 {
                                     [First]
                                     [Second] [Third]
                                     [Fourth]
                                     internal void M() { }
                                 }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 sealed class SecondAttribute : System.Attribute
                                 {
                                 }
                                 sealed class ThirdAttribute : System.Attribute
                                 {
                                 }
                                 sealed class FourthAttribute : System.Attribute
                                 {
                                 }
                                 """;

        // Two multi-attribute method lists sit back to back on one line of the same declaration: each split changes
        // only its own list, so the second list's first attribute stays on the line on which the first list's last
        // attribute now stands. Whether the batch fixer needs one pass or two for the two changes is not what this
        // scenario asserts
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5514MethodAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5514MessageFormat, 2),
                                  Configure: config => config.NumberOfFixAllIterations = -2);
    }

    #endregion // BatchCodeFixTestsBase
}