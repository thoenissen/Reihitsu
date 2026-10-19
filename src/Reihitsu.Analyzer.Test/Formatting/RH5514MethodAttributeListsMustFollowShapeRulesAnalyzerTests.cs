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

    #endregion // Tests

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
                                     [Second]
                                     [Third]
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

        // Two multi-attribute method lists sit back to back on the same declaration: each fix computes its
        // split against the same original document and both target the same method attribute-list run, so the
        // batch fixer discards one of the two conflicting changes in its first pass and needs a second pass to
        // correct the diagnostic that survives
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5514MethodAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5514MessageFormat, 2),
                                  Configure: config => config.NumberOfFixAllIterations = 2);
    }

    #endregion // BatchCodeFixTestsBase
}