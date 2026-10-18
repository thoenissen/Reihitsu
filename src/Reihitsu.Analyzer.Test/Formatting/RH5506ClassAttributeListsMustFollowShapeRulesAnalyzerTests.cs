using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5506ClassAttributeListsMustFollowShapeRulesAnalyzer"/> and <see cref="RH5506ClassAttributeListsMustFollowShapeRulesCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5506ClassAttributeListsMustFollowShapeRulesAnalyzerTests : BatchCodeFixTestsBase<RH5506ClassAttributeListsMustFollowShapeRulesAnalyzer, RH5506ClassAttributeListsMustFollowShapeRulesCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that policy violations are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5506-001")]
    [Requirement("RH5506-002")]
    [Requirement("RH5506-003")]
    [Requirement("SHARED-ATTR-013")]
    [Requirement("SHARED-ATTR-016")]
    [Requirement("SHARED-ATTR-017")]
    public async Task VerifyDiagnosticAndCodeFixForPolicyViolation()
    {
        const string testData = """
                                {|#0:[First, Second]|}
                                internal class Example { }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 [First]
                                 [Second]
                                 internal class Example { }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 sealed class SecondAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5506ClassAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5506MessageFormat));
    }

    /// <summary>
    /// Verifies that compliant code is not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5506-001")]
    [Requirement("SHARED-ATTR-013")]
    public async Task VerifyNoDiagnosticForCompliantCode()
    {
        const string testData = """
                                [First]
                                [Second]
                                internal class Example { }
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
    [Requirement("RH5506-004")]
    [Requirement("SHARED-ATTR-019")]
    public async Task VerifyDiagnosticWithoutCodeFixWhenCommentsArePresent()
    {
        const string testData = """
                                {|#0:[First, /* keep */ Second]|}
                                internal class Example { }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;
        const string codeFixData = """
                                   [First, /* keep */ Second]
                                   internal class Example { }
                                   sealed class FirstAttribute : System.Attribute
                                   {
                                   }
                                   sealed class SecondAttribute : System.Attribute
                                   {
                                   }
                                   """;

        await Verify(testData,
                     Diagnostics(RH5506ClassAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5506MessageFormat));

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5506ClassAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<AttributeListSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that the inserted line break matches the document's detected CRLF end-of-line sequence instead of
    /// <see cref="System.Environment.NewLine"/>, so the fix does not introduce mixed line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5506-003")]
    [Requirement("SHARED-ATTR-017")]
    public async Task VerifyInsertedLineBreakUsesDetectedCarriageReturnLineFeedEndOfLine()
    {
        const string testData = """
                                [First, Second]
                                internal class Example
                                {
                                }

                                sealed class FirstAttribute : System.Attribute
                                {
                                }

                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;

        var fixedSource = await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(testData));

        Assert.DoesNotContain("\n", fixedSource.Replace("\r\n", string.Empty));
    }

    /// <summary>
    /// Verifies that the member's own documentation comment does not withhold the code fix. It sits in the
    /// leading trivia of the attribute list, so a full-span guard would treat a documented class differently
    /// from an identical undocumented one
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5506-004")]
    [Requirement("SHARED-ATTR-019")]
    public async Task VerifyDocumentedClassAttributeListIsOfferedACodeFix()
    {
        const string codeFixData = """
                                   using System;

                                   /// <summary>
                                   /// A documented class.
                                   /// </summary>
                                   [Serializable, Obsolete]
                                   public class TestClass
                                   {
                                   }
                                   """;

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5506ClassAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<AttributeListSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsNotEmpty(actions);
    }

    /// <summary>
    /// Verifies that the multi-attribute lists of a class and of a record, with or without the <c>type:</c> specifier, are reported, while single-attribute lists are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5506-001")]
    [Requirement("RH5506-002")]
    [Requirement("SHARED-ATTR-013")]
    [Requirement("SHARED-ATTR-016")]
    public async Task VerifyDiagnosticForMultiAttributeListsOfClassesAndRecords()
    {
        const string testData = """
                                {|#0:[A, B]|}
                                class C
                                {
                                }

                                {|#1:[type: A, B]|}
                                record R;

                                [A]
                                [B]
                                class D
                                {
                                }
                                sealed class A : System.Attribute
                                {
                                }
                                sealed class B : System.Attribute
                                {
                                }
                                """;

        await Verify(testData,
                     Diagnostics(RH5506ClassAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5506MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that the code fix replaces the class attribute list by one list per attribute, each on its own line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5506-003")]
    [Requirement("SHARED-ATTR-017")]
    public async Task VerifyCodeFixGivesEachClassAttributeAListOfItsOwn()
    {
        const string testData = """
                                {|#0:[A, B]|}
                                class C
                                {
                                }
                                sealed class A : System.Attribute
                                {
                                }
                                sealed class B : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 [A]
                                 [B]
                                 class C
                                 {
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
                     Diagnostics(RH5506ClassAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5506MessageFormat));
    }

    /// <summary>
    /// Verifies that the code fix replaces the class attribute list by one list per attribute, each on its own line, under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5506-003")]
    [Requirement("SHARED-ATTR-017")]
    public async Task VerifyCodeFixGivesEachClassAttributeAListOfItsOwnWithCarriageReturnLineFeed()
    {
        const string testData = """
                                {|#0:[A, B]|}
                                class C
                                {
                                }
                                sealed class A : System.Attribute
                                {
                                }
                                sealed class B : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 [A]
                                 [B]
                                 class C
                                 {
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
                     Diagnostics(RH5506ClassAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5506MessageFormat));
    }

    /// <summary>
    /// Verifies that the code fix splits two multi-attribute class lists that sit on separate lines, and that Fix All corrects both in one application
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5506-003")]
    [Requirement("SHARED-ATTR-017")]
    public async Task VerifyCodeFixSplitsEveryClassAttributeListOnItsOwnLine()
    {
        const string testData = """
                                {|#0:[A, B]|}
                                {|#1:[D, E]|}
                                class C
                                {
                                }
                                sealed class A : System.Attribute
                                {
                                }
                                sealed class B : System.Attribute
                                {
                                }
                                sealed class D : System.Attribute
                                {
                                }
                                sealed class E : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 [A]
                                 [B]
                                 [D]
                                 [E]
                                 class C
                                 {
                                 }
                                 sealed class A : System.Attribute
                                 {
                                 }
                                 sealed class B : System.Attribute
                                 {
                                 }
                                 sealed class D : System.Attribute
                                 {
                                 }
                                 sealed class E : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5506ClassAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5506MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that a list with a comment or a preprocessor directive between its brackets is still reported without offering a code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5506-004")]
    [Requirement("SHARED-ATTR-019")]
    public async Task VerifyDiagnosticWithoutCodeFixWhenCommentOrDirectiveIsInsideTheList()
    {
        const string testData = """
                                {|#0:[A, /* c */ B]|}
                                class C
                                {
                                }

                                {|#1:[A,
                                #if DEBUG
                                #endif
                                 B]|}
                                class E
                                {
                                }
                                sealed class A : System.Attribute
                                {
                                }
                                sealed class B : System.Attribute
                                {
                                }
                                """;
        const string codeFixData = """
                                   [A, /* c */ B]
                                   class C
                                   {
                                   }

                                   [A,
                                   #if DEBUG
                                   #endif
                                    B]
                                   class E
                                   {
                                   }
                                   sealed class A : System.Attribute
                                   {
                                   }
                                   sealed class B : System.Attribute
                                   {
                                   }
                                   """;

        await Verify(testData,
                     Diagnostics(RH5506ClassAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5506MessageFormat, 2));

        foreach (var index in new[] { 0, 1 })
        {
            var actions = await GetCodeFixActionsAsync(codeFixData,
                                                       RH5506ClassAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId,
                                                       root => root.DescendantNodes()
                                                                   .OfType<AttributeListSyntax>()
                                                                   .ElementAt(index)
                                                                   .GetLocation());

            Assert.IsEmpty(actions, $"A code fix was offered for attribute list {index}.");
        }
    }

    /// <summary>
    /// Verifies that two identical multi-attribute lists on one line are reported once each, on their own spans
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5506-001")]
    [Requirement("RH5506-002")]
    [Requirement("SHARED-ATTR-013")]
    [Requirement("SHARED-ATTR-016")]
    public async Task VerifyDiagnosticForEachOfTwoIdenticalListsOnOneLine()
    {
        const string testData = """
                                {|#0:[A, B]|} {|#1:[A, B]|}
                                class C
                                {
                                }
                                [System.AttributeUsage(System.AttributeTargets.All, AllowMultiple = true)]
                                sealed class A : System.Attribute
                                {
                                }
                                [System.AttributeUsage(System.AttributeTargets.All, AllowMultiple = true)]
                                sealed class B : System.Attribute
                                {
                                }
                                """;

        await Verify(testData,
                     Diagnostics(RH5506ClassAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5506MessageFormat, 2));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                {|#0:[First, Second]|} {|#1:[Third, Fourth]|}
                                internal class Example { }
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
                                 [First]
                                 [Second]
                                 [Third]
                                 [Fourth]
                                 internal class Example { }
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

        // Two multi-attribute class lists sit back to back on the same declaration: each fix computes its split
        // against the same original document and both target the same class attribute-list run, so the batch
        // fixer discards one of the two conflicting changes in its first pass and needs a second pass to
        // correct the diagnostic that survives
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5506ClassAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5506MessageFormat, 2),
                                  Configure: config => config.NumberOfFixAllIterations = 2);
    }

    #endregion // BatchCodeFixTestsBase
}