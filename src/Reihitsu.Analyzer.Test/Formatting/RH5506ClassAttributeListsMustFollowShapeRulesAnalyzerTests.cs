using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Testing;
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
                     Diagnostics(RH5506ClassAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, MessageFormat));
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
                     Diagnostics(RH5506ClassAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, MessageFormat));

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
                     Diagnostics(RH5506ClassAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, MessageFormat, 2));
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
                     Diagnostics(RH5506ClassAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the code fix replaces the class attribute list by one list per attribute, each on its own line, under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5506-003")]
    [Requirement("SHARED-ATTR-017")]
    [Requirement("SHARED-FIX-008")]
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
                     Diagnostics(RH5506ClassAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, MessageFormat));

        Assert.AreEqual(NormalizeToCarriageReturnLineFeed(fixedData), await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(RemoveMarkup(testData))), "Every line break the fix inserts must be CRLF.");
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
                     Diagnostics(RH5506ClassAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    /// <summary>
    /// Verifies the spec example: a list with a comment, a <c>#pragma</c> directive between its attributes, or a conditional directive inside an attribute's argument list is still reported without offering a code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5506-004")]
    [Requirement("SHARED-ATTR-019")]
    public async Task VerifyDiagnosticWithoutCodeFixWhenCommentOrDirectiveIsInsideTheList()
    {
        const string testData = """
                                {|RH5506:[A, /* c */ B]|}
                                class C
                                {
                                }

                                {|RH5506:[A,
                                #pragma warning disable CS0618
                                 B]|}
                                class E
                                {
                                }

                                {|RH5506:[A(1
                                #if DEBUG
                                 + 1
                                #endif
                                 ), B]|}
                                class F
                                {
                                }
                                sealed class A : System.Attribute
                                {
                                    public A()
                                    {
                                    }

                                    public A(int value)
                                    {
                                    }
                                }
                                sealed class B : System.Attribute
                                {
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
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
                     Diagnostics(RH5506ClassAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    /// <summary>
    /// Verifies the spec example: a list on the line of its class is split, and the class declaration stays behind the
    /// last new list, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5506-003")]
    [Requirement("SHARED-ATTR-017")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task SplitFixKeepsTheClassDeclarationBehindTheLastNewList(string endOfLine)
    {
        const string testData = """
                                {|RH5506:[A, B]|} class C
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
                                 [B] class C
                                 {
                                 }
                                 sealed class A : System.Attribute
                                 {
                                 }
                                 sealed class B : System.Attribute
                                 {
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that the list of a nested class on its line is split behind the whitespace at the start of that line, and
    /// the declaration stays behind the last new list, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5506-003")]
    [Requirement("SHARED-ATTR-017")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task SplitFixKeepsANestedClassDeclarationBehindTheLastNewList(string endOfLine)
    {
        const string testData = """
                                class O
                                {
                                    {|RH5506:[A, B]|} class C { }
                                }
                                sealed class A : System.Attribute
                                {
                                }
                                sealed class B : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 class O
                                 {
                                     [A]
                                     [B] class C { }
                                 }
                                 sealed class A : System.Attribute
                                 {
                                 }
                                 sealed class B : System.Attribute
                                 {
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies the spec example: a list on the line of the <c>{</c> of the namespace that holds the class is reported, and
    /// no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5506-004")]
    [Requirement("SHARED-ATTR-022")]
    public async Task VerifyDiagnosticWithoutCodeFixOnTheLineOfTheBraceOfTheNamespaceThatHoldsTheClass()
    {
        const string testData = """
                                namespace N { {|RH5506:[A, B]|} class C { } }

                                sealed class A : System.Attribute
                                {
                                }
                                sealed class B : System.Attribute
                                {
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a list on the line of the <c>{</c> of the class that holds a nested class is reported, and no code fix
    /// is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ATTR-022")]
    public async Task VerifyDiagnosticWithoutCodeFixOnTheLineOfTheBraceOfTheClassThatHoldsTheNestedClass()
    {
        const string testData = """
                                class O { {|RH5506:[A, B]|} class C { } }

                                sealed class A : System.Attribute
                                {
                                }
                                sealed class B : System.Attribute
                                {
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a list of a class in a file-scoped namespace, which no <c>{</c> holds, gets the fix, under LF and CRLF
    /// line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ATTR-022")]
    [Requirement("SHARED-ATTR-017")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyCodeFixSplitsAListOfAClassInAFileScopedNamespace(string endOfLine)
    {
        const string testData = """
                                namespace N;

                                {|RH5506:[A, B]|} class C { }

                                sealed class A : System.Attribute
                                {
                                }
                                sealed class B : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 namespace N;

                                 [A]
                                 [B] class C { }

                                 sealed class A : System.Attribute
                                 {
                                 }
                                 sealed class B : System.Attribute
                                 {
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies the spec example: a list with a conditional directive between its attributes is reported without a code fix, with and without the symbol defined, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5506-004")]
    [Requirement("SHARED-ATTR-019")]
    [Requirement("SHARED-DIAG-008")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyDiagnosticWithoutCodeFixWhenConditionalDirectiveIsBetweenTheAttributes(string endOfLine)
    {
        const string testData = """
                                {|RH5506:[A,
                                #if DEBUG
                                 B,
                                #endif
                                 C1]|}
                                class C
                                {
                                }

                                sealed class A : System.Attribute
                                {
                                }
                                sealed class B : System.Attribute
                                {
                                }
                                sealed class C1 : System.Attribute
                                {
                                }
                                sealed class D : System.Attribute
                                {
                                }
                                """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(testData, endOfLine));
        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(testData, endOfLine), DocumentationMode.Parse, "DEBUG");
    }

    /// <summary>
    /// Verifies that a list whose attribute a false conditional excludes as disabled text is reported without a code fix, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5506-004")]
    [Requirement("SHARED-ATTR-019")]
    [Requirement("SHARED-DIAG-008")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyDiagnosticWithoutCodeFixWhenDisabledTextHoldsAnAttributeOfTheList(string endOfLine)
    {
        const string testData = """
                                {|RH5506:[A,
                                #if false
                                 B,
                                #else
                                 D,
                                #endif
                                 C1]|}
                                class C
                                {
                                }

                                sealed class A : System.Attribute
                                {
                                }
                                sealed class B : System.Attribute
                                {
                                }
                                sealed class C1 : System.Attribute
                                {
                                }
                                sealed class D : System.Attribute
                                {
                                }
                                """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies that a list is reported without a code fix for every shape of conditional directive between its
    /// attributes: the rule judges the attributes the analyzed build compiles
    /// </summary>
    /// <param name="directives">The conditional directives, and the attributes they hold, between <c>A,</c> and <c>C1</c></param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("#if FEATURE\n#endif", DisplayName = "Empty block")]
    [DataRow("#if NEVER\n B,\n#endif", DisplayName = "Excluded attribute")]
    [DataRow("#if !NEVER\n B,\n#endif", DisplayName = "Active attribute")]
    [DataRow("#if NEVER\n B,\n#else\n D,\n#endif", DisplayName = "If and else")]
    [DataRow("#if NEVER\n B,\n#elif !NEVER\n D,\n#endif", DisplayName = "If and elif")]
    [Requirement("RH5506-004")]
    [Requirement("SHARED-ATTR-019")]
    [Requirement("SHARED-DIAG-008")]
    public async Task VerifyDiagnosticWithoutCodeFixForEveryShapeOfConditionalDirectiveBetweenTheAttributes(string directives)
    {
        var testData = $$"""
                         {|RH5506:[A,
                         {{directives}}
                          C1]|}
                         class C
                         {
                         }

                         sealed class A : System.Attribute
                         {
                         }
                         sealed class B : System.Attribute
                         {
                         }
                         sealed class C1 : System.Attribute
                         {
                         }
                         sealed class D : System.Attribute
                         {
                         }
                         """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that Fix All splits the lists without a conditional directive between their attributes and leaves the
    /// list with such a directive reported and unchanged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5506-004")]
    [Requirement("SHARED-ATTR-019")]
    [Requirement("SHARED-DIAG-008")]
    public async Task VerifyFixAllSplitsOnlyTheListsWithoutAConditionalDirective()
    {
        const string testData = """
                                {|RH5506:[A,
                                #if FEATURE
                                #endif
                                 C1]|}
                                class C
                                {
                                }

                                {|RH5506:[A, B]|}
                                class E
                                {
                                }

                                {|RH5506:[C1, D]|}
                                class F
                                {
                                }

                                sealed class A : System.Attribute
                                {
                                }
                                sealed class B : System.Attribute
                                {
                                }
                                sealed class C1 : System.Attribute
                                {
                                }
                                sealed class D : System.Attribute
                                {
                                }
                                """;

        const string fixedData = """
                                 {|RH5506:[A,
                                 #if FEATURE
                                 #endif
                                  C1]|}
                                 class C
                                 {
                                 }

                                 [A]
                                 [B]
                                 class E
                                 {
                                 }

                                 [C1]
                                 [D]
                                 class F
                                 {
                                 }

                                 sealed class A : System.Attribute
                                 {
                                 }
                                 sealed class B : System.Attribute
                                 {
                                 }
                                 sealed class C1 : System.Attribute
                                 {
                                 }
                                 sealed class D : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     test =>
                     {
                         // The diagnostic whose fix is withheld stays and is marked in the expected source
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                     });
    }

    /// <summary>
    /// Verifies that a list with a region directive pair between its attributes is still reported without offering a code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5506-004")]
    [Requirement("SHARED-ATTR-019")]
    public async Task VerifyDiagnosticWithoutCodeFixWhenRegionDirectiveIsBetweenTheAttributes()
    {
        const string testData = """
                                {|RH5506:[A,
                                #region Attributes
                                 B
                                #endregion
                                 ]|}
                                class C
                                {
                                }

                                sealed class A : System.Attribute
                                {
                                }
                                sealed class B : System.Attribute
                                {
                                }
                                sealed class C1 : System.Attribute
                                {
                                }
                                sealed class D : System.Attribute
                                {
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a conditional directive in front of the list lies outside the span the rule judges, so a list with two attributes inside it is still reported, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5506-004")]
    [Requirement("SHARED-ATTR-019")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyDiagnosticWhenConditionalDirectiveIsInFrontOfTheList(string endOfLine)
    {
        const string testData = """
                                #if DEBUG
                                #endif
                                {|RH5506:[A, B]|}
                                class C
                                {
                                }

                                sealed class A : System.Attribute
                                {
                                }
                                sealed class B : System.Attribute
                                {
                                }
                                sealed class C1 : System.Attribute
                                {
                                }
                                sealed class D : System.Attribute
                                {
                                }
                                """;

        await Verify(ConvertEndOfLine(testData, endOfLine));
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
                                 [Second] [Third]
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

        // Two multi-attribute class lists sit back to back on one line of the same declaration: each split changes only
        // its own list, so the second list's first attribute stays on the line on which the first list's last attribute
        // now stands.
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5506ClassAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, MessageFormat, 2),
                                  Configure: config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}