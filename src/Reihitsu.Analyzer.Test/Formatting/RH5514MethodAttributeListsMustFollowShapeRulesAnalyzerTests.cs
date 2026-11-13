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

    /// <summary>
    /// Declarations of the attributes the examples with attribute arguments use, appended to each example
    /// </summary>
    private const string ArgumentAttributeDeclarations = """

                                                         sealed class A : System.Attribute
                                                         {
                                                             public A(int x, int y)
                                                             {
                                                             }
                                                         }
                                                         sealed class B : System.Attribute
                                                         {
                                                         }
                                                         sealed class S : System.Attribute
                                                         {
                                                             public S(string s)
                                                             {
                                                             }
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
    [Requirement("SHARED-ATTR-023")]
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

    /// <summary>
    /// Verifies the spec example: a list on the line of the <c>{</c> of the class that holds the method is reported, and no
    /// code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5514-005")]
    [Requirement("SHARED-ATTR-022")]
    public async Task SplitFixIsWithheldOnTheLineOfTheBraceOfTheClassThatHoldsTheMethod()
    {
        const string testData = """
                                class C { {|RH5514:[A, B]|} void M() { } }
                                """;

        await VerifyNoCodeFixIsOffered(testData + AttributeDeclarations);
    }

    /// <summary>
    /// Verifies that a list on the line of the <c>{</c> of the interface that holds the method is reported, and no code fix
    /// is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ATTR-022")]
    public async Task SplitFixIsWithheldOnTheLineOfTheBraceOfTheInterfaceThatHoldsTheMethod()
    {
        const string testData = """
                                interface I { {|RH5514:[A, B]|} void M(); }
                                """;

        await VerifyNoCodeFixIsOffered(testData + AttributeDeclarations);
    }

    /// <summary>
    /// Verifies the spec example: a list on the line of the <c>{</c> of the block that holds the local function is reported,
    /// and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5514-005")]
    [Requirement("SHARED-ATTR-022")]
    public async Task SplitFixIsWithheldOnTheLineOfTheBraceOfTheBlockThatHoldsTheLocalFunction()
    {
        const string testData = """
                                class D
                                {
                                    void N() { {|RH5514:[A, B]|} void L() { } }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData + AttributeDeclarations);
    }

    /// <summary>
    /// Verifies that a list on the line of the <c>{</c> of the block that holds the local function is reported, and no
    /// code fix is offered, also where a statement stands between that <c>{</c> and the list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ATTR-022")]
    public async Task SplitFixIsWithheldOnTheLineOfTheBraceOfTheBlockThatHoldsTheLocalFunctionBehindAStatement()
    {
        const string testData = """
                                class D
                                {
                                    void N() { int x = 0; {|RH5514:[A, B]|} void L() { } }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData + AttributeDeclarations);
    }

    /// <summary>
    /// Verifies the spec example: a <c>{</c> on the line of the reported list that opens the body of another method does
    /// not withhold the fix, and the further list starts a new line, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ATTR-022")]
    [Requirement("SHARED-ATTR-017")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task SplitFixIsOfferedBehindTheBraceOfAnotherBodyOnTheLineOfTheList(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void M() { } {|RH5514:[A, B]|} void N() { }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M() { } [A]
                                     [B] void N() { }
                                 }
                                 """;

        await VerifySplitFix(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies the spec example: a list of a lambda in a field initializer, on the line of the <c>{</c> of the class that
    /// holds the field, is reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ATTR-022")]
    public async Task SplitFixIsWithheldForALambdaInAFieldInitializerOnTheLineOfTheBraceOfTheClass()
    {
        const string testData = """
                                class C2 { System.Func<int> f = {|RH5514:[A, B]|} () => 1; }
                                """;

        await VerifyNoCodeFixIsOffered(testData + AttributeDeclarations);
    }

    /// <summary>
    /// Verifies the spec example: a <c>{</c> that is closed in front of the list on its line does not cancel a <c>{</c> that
    /// is still open there, so the list is reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ATTR-022")]
    public async Task SplitFixIsWithheldBehindAClosedBodyWhileTheBraceOfTheClassIsStillOpenOnTheLineOfTheList()
    {
        const string testData = """
                                class C3 { void M() { } {|RH5514:[A, B]|} void N() { } }
                                """;

        await VerifyNoCodeFixIsOffered(testData + AttributeDeclarations);
    }

    /// <summary>
    /// Verifies the spec example: a list of a lambda in a statement, on the line of the <c>{</c> of the block that holds the
    /// statement, is reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ATTR-022")]
    public async Task SplitFixIsWithheldForALambdaInAStatementOnTheLineOfTheBraceOfTheBlock()
    {
        const string testData = """
                                class D
                                {
                                    void O() { System.Func<int> f = {|RH5514:[A, B]|} () => 1; }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData + AttributeDeclarations);
    }

    /// <summary>
    /// Verifies the spec example: a list of a local function in a switch section, on the line of the <c>{</c> of the switch
    /// statement, is reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ATTR-022")]
    public async Task SplitFixIsWithheldForALocalFunctionInASwitchSectionOnTheLineOfTheBraceOfTheSwitchStatement()
    {
        const string testData = """
                                class D
                                {
                                    void P(int x)
                                    {
                                        switch (x) { case 1: {|RH5514:[A, B]|} void L() { } break; }
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData + AttributeDeclarations);
    }

    /// <summary>
    /// Verifies the spec example: a list of a lambda in a statement of a block whose <c>{</c> stands on a line above the list
    /// is split, and the further list starts a new line, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ATTR-022")]
    [Requirement("SHARED-ATTR-017")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task SplitFixIsOfferedForALambdaInAStatementBelowTheLineOfTheBraceOfTheBlock(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void M()
                                    {
                                        System.Func<int> f = {|RH5514:[A, B]|} () => 1;
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M()
                                     {
                                         System.Func<int> f = [A]
                                         [B] () => 1;
                                     }
                                 }
                                 """;

        await VerifySplitFix(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies the spec example: a <c>{</c> inside a string literal in front of the list on its line is not a token and does
    /// not withhold the fix, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ATTR-022")]
    [Requirement("SHARED-ATTR-017")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task SplitFixIsOfferedBehindABraceInsideAStringLiteralOnTheLineOfTheList(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    string s = "{"; {|RH5514:[A, B]|} void N() { }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     string s = "{"; [A]
                                     [B] void N() { }
                                 }
                                 """;

        await VerifySplitFix(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that a <c>{</c> inside a comment in front of the list on its line is not a token and does not withhold the
    /// fix, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ATTR-022")]
    [Requirement("SHARED-ATTR-017")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task SplitFixIsOfferedBehindABraceInsideACommentOnTheLineOfTheList(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    /* { */ {|RH5514:[A, B]|} void N() { }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     /* { */ [A]
                                     [B] void N() { }
                                 }
                                 """;

        await VerifySplitFix(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that the <c>{</c> of an interpolation in front of the list on its line is closed there and does not
    /// withhold the fix, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ATTR-022")]
    [Requirement("SHARED-ATTR-017")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task SplitFixIsOfferedBehindAClosedInterpolationOnTheLineOfTheList(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    string s = $"{1}"; {|RH5514:[A, B]|} void N() { }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     string s = $"{1}"; [A]
                                     [B] void N() { }
                                 }
                                 """;

        await VerifySplitFix(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies the spec example: in a CRLF document, an LF inside the argument list of an attribute withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5514-005")]
    [Requirement("SHARED-ATTR-023")]
    public async Task SplitFixIsWithheldForALineFeedInsideAnAttributeInACarriageReturnLineFeedDocument()
    {
        const string testData = """
                                class C
                                {
                                    {|RH5514:[A(1,LF
                                      2), B]|}
                                    void M()
                                    {
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(WithLineFeedMarkers(testData + ArgumentAttributeDeclarations));
    }

    /// <summary>
    /// Verifies that in a CRLF document an LF inside a verbatim string argument of an attribute withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ATTR-023")]
    public async Task SplitFixIsWithheldForALineFeedInsideAStringArgumentInACarriageReturnLineFeedDocument()
    {
        const string testData = """
                                class C
                                {
                                    {|RH5514:[S(@"oneLF
                                two"), B]|}
                                    void M()
                                    {
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(WithLineFeedMarkers(testData + ArgumentAttributeDeclarations));
    }

    /// <summary>
    /// Verifies that in a CRLF document an LF between two attributes withholds the fix, although the fix would not keep it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ATTR-023")]
    public async Task SplitFixIsWithheldForALineFeedBetweenTwoAttributesInACarriageReturnLineFeedDocument()
    {
        const string testData = """
                                class C
                                {
                                    {|RH5514:[A,LF
                                        B]|}
                                    void M()
                                    {
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(WithLineFeedMarkers(testData + AttributeDeclarations));
    }

    /// <summary>
    /// Verifies the spec example: in a document whose line breaks are all a lone carriage return, a list that holds a line
    /// break is reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ATTR-023")]
    [Requirement("SHARED-FIX-008")]
    public async Task SplitFixIsWithheldForALineBreakInsideTheListInADocumentWhoseLineBreaksAreLoneCarriageReturns()
    {
        const string testData = """
                                class C
                                {
                                    {|RH5514:[A(1,
                                      2), B]|}
                                    void M()
                                    {
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(testData + ArgumentAttributeDeclarations, "\r"));
    }

    /// <summary>
    /// Verifies the spec example: in a document whose line breaks are all one line ending, a list with a line break inside
    /// the argument list of an attribute gets the fix, and that line break stays as written, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ATTR-023")]
    [Requirement("SHARED-ATTR-017")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task SplitFixKeepsALineBreakInsideAnAttributeWhenEveryLineBreakIsTheDetectedOne(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    {|RH5514:[A(1,
                                      2), B]|}
                                    void M()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     [A(1,
                                       2)]
                                     [B]
                                     void M()
                                     {
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData + ArgumentAttributeDeclarations, fixedData + ArgumentAttributeDeclarations, endOfLine);
    }

    /// <summary>
    /// Verifies that in a document whose line breaks are all one line ending, a list with a line break between two
    /// attributes gets the fix, and the further list starts a new line in the column of the reported list, under LF and
    /// CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ATTR-023")]
    [Requirement("SHARED-ATTR-017")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task SplitFixIsOfferedForALineBreakBetweenTwoAttributesWhenEveryLineBreakIsTheDetectedOne(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    {|RH5514:[A,
                                        B]|}
                                    void M()
                                    {
                                    }
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
                                 """;

        await VerifySplitFix(testData, fixedData, endOfLine);
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

    /// <summary>
    /// Writes every line break of the provided text as CRLF, except a line break marked with <c>LF</c> in front of it,
    /// which is written as LF
    /// </summary>
    /// <param name="text">Text with <c>LF</c> markers</param>
    /// <returns>The text with CRLF line breaks and an LF at each marker</returns>
    private static string WithLineFeedMarkers(string text)
    {
        return NormalizeToCarriageReturnLineFeed(text).Replace("LF\r\n", "\n", System.StringComparison.Ordinal);
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