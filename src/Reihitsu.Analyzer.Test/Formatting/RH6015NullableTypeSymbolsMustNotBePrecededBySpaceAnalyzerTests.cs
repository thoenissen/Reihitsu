using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Spacing;
using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH6015NullableTypeSymbolsMustNotBePrecededBySpaceAnalyzer"/> and <see cref="RH6015NullableTypeSymbolsMustNotBePrecededBySpaceCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH6015NullableTypeSymbolsMustNotBePrecededBySpaceAnalyzerTests : BatchCodeFixTestsBase<RH6015NullableTypeSymbolsMustNotBePrecededBySpaceAnalyzer, RH6015NullableTypeSymbolsMustNotBePrecededBySpaceCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that clean code does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenCodeIsClean()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        if (true)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the issue is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6015-001")]
    [Requirement("RH6015-003")]
    [Requirement("RH6015-004")]
    [Requirement("SHARED-WS-001")]
    public async Task VerifyIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int{|#0: |}? value)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int? value)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6015NullableTypeSymbolsMustNotBePrecededBySpaceAnalyzer.DiagnosticId, AnalyzerResources.RH6015MessageFormat));
    }

    /// <summary>
    /// Verifies that a tab before a nullable type symbol is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6015-001")]
    [Requirement("RH6015-004")]
    [Requirement("SHARED-WS-001")]
    public async Task VerifyTabBeforeNullableTypeSymbolIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int{|#0:	|}? value)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int? value)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6015NullableTypeSymbolsMustNotBePrecededBySpaceAnalyzer.DiagnosticId, AnalyzerResources.RH6015MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment before a same-line nullable type symbol still reports the diagnostic but
    /// withholds the fix, because deleting the whitespace run would glue the comment to the <c>?</c> operator
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixIsNotOfferedForSameLineNullableTypeSymbolWithPrecedingComment()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int /* Keep. */{|#0: |}? value)
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH6015NullableTypeSymbolsMustNotBePrecededBySpaceAnalyzer.DiagnosticId, AnalyzerResources.RH6015MessageFormat));

        var actions = await GetCodeFixActionsAsync(testData.Replace("{|#0: |}", " "),
                                                   RH6015NullableTypeSymbolsMustNotBePrecededBySpaceAnalyzer.DiagnosticId,
                                                   root =>
                                                   {
                                                       var nullableType = root.DescendantNodes().OfType<NullableTypeSyntax>().First();
                                                       var lastTrailingTrivia = nullableType.QuestionToken.GetPreviousToken().TrailingTrivia.Last();

                                                       Assert.IsTrue(lastTrailingTrivia.IsKind(SyntaxKind.WhitespaceTrivia),
                                                                     "The preceding token's trailing trivia must end with the whitespace run immediately before the operator.");

                                                       return Location.Create(root.SyntaxTree, lastTrailingTrivia.Span);
                                                   });

        Assert.IsEmpty(actions,
                       "The comment sits in the same token gap as the reported whitespace run; the fix must be withheld.");
    }

    /// <summary>
    /// Repeats <see cref="VerifyFixIsNotOfferedForSameLineNullableTypeSymbolWithPrecedingComment"/> with the
    /// source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixIsNotOfferedForSameLineNullableTypeSymbolWithPrecedingCommentCarriageReturnLineFeed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int /* Keep. */{|#0: |}? value)
                                    {
                                    }
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData), Diagnostics(RH6015NullableTypeSymbolsMustNotBePrecededBySpaceAnalyzer.DiagnosticId, AnalyzerResources.RH6015MessageFormat));

        var actions = await GetCodeFixActionsAsync(NormalizeToCarriageReturnLineFeed(testData.Replace("{|#0: |}", " ")),
                                                   RH6015NullableTypeSymbolsMustNotBePrecededBySpaceAnalyzer.DiagnosticId,
                                                   root =>
                                                   {
                                                       var nullableType = root.DescendantNodes().OfType<NullableTypeSyntax>().First();
                                                       var lastTrailingTrivia = nullableType.QuestionToken.GetPreviousToken().TrailingTrivia.Last();

                                                       Assert.IsTrue(lastTrailingTrivia.IsKind(SyntaxKind.WhitespaceTrivia),
                                                                     "The preceding token's trailing trivia must end with the whitespace run immediately before the operator.");

                                                       return Location.Create(root.SyntaxTree, lastTrailingTrivia.Span);
                                                   });

        Assert.IsEmpty(actions,
                       "The comment sits in the same token gap as the reported whitespace run; the fix must be withheld.");
    }

    /// <summary>
    /// Verifies that a continuation-line nullable type symbol preceded by a block comment still reports the
    /// diagnostic on the same-line whitespace run between the comment and the <c>?</c> operator, and that the
    /// fix is withheld rather than offered, because the guard must inspect the gap the whitespace run actually
    /// sits in
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixIsNotOfferedForContinuationLineNullableTypeSymbolWithPrecedingComment()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int
                                        /* keep */{|#0: |}? value)
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH6015NullableTypeSymbolsMustNotBePrecededBySpaceAnalyzer.DiagnosticId, AnalyzerResources.RH6015MessageFormat));

        var actions = await GetCodeFixActionsAsync(testData.Replace("{|#0: |}", " "),
                                                   RH6015NullableTypeSymbolsMustNotBePrecededBySpaceAnalyzer.DiagnosticId,
                                                   root =>
                                                   {
                                                       var nullableType = root.DescendantNodes().OfType<NullableTypeSyntax>().First();
                                                       var lastLeadingTrivia = nullableType.QuestionToken.LeadingTrivia.Last();

                                                       Assert.IsTrue(lastLeadingTrivia.IsKind(SyntaxKind.WhitespaceTrivia),
                                                                     "The operator's leading trivia must end with the whitespace run immediately preceding it.");

                                                       return Location.Create(root.SyntaxTree, lastLeadingTrivia.Span);
                                                   });

        Assert.IsEmpty(actions,
                       "The comment sits between the previous token and the reported whitespace run; the fix must be withheld.");
    }

    /// <summary>
    /// Repeats
    /// <see cref="VerifyFixIsNotOfferedForContinuationLineNullableTypeSymbolWithPrecedingComment"/> with the
    /// source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixIsNotOfferedForContinuationLineNullableTypeSymbolWithPrecedingCommentCarriageReturnLineFeed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int
                                        /* keep */{|#0: |}? value)
                                    {
                                    }
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData), Diagnostics(RH6015NullableTypeSymbolsMustNotBePrecededBySpaceAnalyzer.DiagnosticId, AnalyzerResources.RH6015MessageFormat));

        var actions = await GetCodeFixActionsAsync(NormalizeToCarriageReturnLineFeed(testData.Replace("{|#0: |}", " ")),
                                                   RH6015NullableTypeSymbolsMustNotBePrecededBySpaceAnalyzer.DiagnosticId,
                                                   root =>
                                                   {
                                                       var nullableType = root.DescendantNodes().OfType<NullableTypeSyntax>().First();
                                                       var lastLeadingTrivia = nullableType.QuestionToken.LeadingTrivia.Last();

                                                       Assert.IsTrue(lastLeadingTrivia.IsKind(SyntaxKind.WhitespaceTrivia),
                                                                     "The operator's leading trivia must end with the whitespace run immediately preceding it.");

                                                       return Location.Create(root.SyntaxTree, lastLeadingTrivia.Span);
                                                   });

        Assert.IsEmpty(actions,
                       "The comment sits between the previous token and the reported whitespace run; the fix must be withheld.");
    }

    /// <summary>
    /// Verifies that a nullable type symbol on a continuation line does not produce a diagnostic with LF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6015-001")]
    public async Task VerifyContinuationLineNullableTypeSymbolDoesNotProduceDiagnostic()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int
                                        ? value)
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a nullable type symbol on a continuation line does not produce a diagnostic with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6015-001")]
    public async Task VerifyContinuationLineNullableTypeSymbolDoesNotProduceDiagnosticWithCarriageReturnLineFeed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int
                                        ? value)
                                    {
                                    }
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData));
    }

    /// <summary>
    /// Verifies that disabled text and a directive inside the gap before a continuation-line nullable type symbol do not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6015-001")]
    public async Task VerifyDirectiveAndDisabledTextBeforeContinuationLineNullableTypeSymbolDoNotProduceDiagnostic()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int
                                #if false
                                    disabled text
                                #endif
                                        ? value)
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that disabled text and a directive inside the gap before a continuation-line nullable type symbol do not produce a diagnostic with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6015-001")]
    public async Task VerifyDirectiveAndDisabledTextBeforeContinuationLineNullableTypeSymbolDoNotProduceDiagnosticWithCarriageReturnLineFeed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int
                                #if false
                                    disabled text
                                #endif
                                        ? value)
                                    {
                                    }
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData));
    }

    /// <summary>
    /// Verifies that a space before a nullable type's question mark inside a documentation-comment cref
    /// parameter is not flagged, because the formatter never rewrites inside a cref and node-kind dispatch
    /// reaches structured trivia that the previous tree walk never saw
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6015-002")]
    public async Task VerifySpaceBeforeNullableQuestionMarkInsideCrefIsIgnored()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    /// See <see cref="Method(int ?)"/>.
                                    /// </summary>
                                    void Method(int? value)
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that whitespace before the <c>?</c> of a nullable value or reference type on the same line is reported
    /// once per <c>?</c> in fields, arrays, parameters, <c>typeof</c>, casts, type arguments, and <c>as</c>, and that a
    /// <c>?</c> that starts its line is not reported. The verifier reports nullable warnings as errors, so the
    /// uninitialized array field also raises CS8618
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6015-001")]
    [Requirement("RH6015-003")]
    public async Task VerifyWhitespaceBeforeNullableQuestionMarkIsReportedWhereverTypeIsWritten()
    {
        const string testData = """
                                #nullable enable

                                using System.Collections.Generic;

                                class C
                                {
                                    int{|RH6015: |}? _a;
                                    string{|RH6015:→|}? _b;
                                    int{|RH6015: |}?[] {|CS8618:_c|};
                                    int
                                        ? _e;

                                    void M(int{|RH6015: |}? a, object o)
                                    {
                                        var t = typeof(int{|RH6015: |}?);
                                        var x = (int{|RH6015: |}?)o;
                                        List<int{|RH6015: |}?> l = new();
                                        var y = o as int{|RH6015: |}? ?? 0;
                                    }
                                }
                                """;

        await Verify(ExpandWhitespaceMarkers(testData));
    }

    /// <summary>
    /// Repeats <see cref="VerifyWhitespaceBeforeNullableQuestionMarkIsReportedWhereverTypeIsWritten"/> with the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6015-001")]
    [Requirement("RH6015-003")]
    public async Task VerifyWhitespaceBeforeNullableQuestionMarkIsReportedWhereverTypeIsWrittenCarriageReturnLineFeed()
    {
        const string testData = """
                                #nullable enable

                                using System.Collections.Generic;

                                class C
                                {
                                    int{|RH6015: |}? _a;
                                    string{|RH6015:→|}? _b;
                                    int{|RH6015: |}?[] {|CS8618:_c|};
                                    int
                                        ? _e;

                                    void M(int{|RH6015: |}? a, object o)
                                    {
                                        var t = typeof(int{|RH6015: |}?);
                                        var x = (int{|RH6015: |}?)o;
                                        List<int{|RH6015: |}?> l = new();
                                        var y = o as int{|RH6015: |}? ?? 0;
                                    }
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(ExpandWhitespaceMarkers(testData)));
    }

    /// <summary>
    /// Verifies that the <c>?</c> of a conditional expression and a nullable type inside a <c>cref</c> are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6015-002")]
    public async Task VerifyConditionalQuestionMarkAndCrefAreNotReported()
    {
        const string testData = """
                                class C
                                {
                                    /// <see cref="M(int ?)"/>
                                    int M(int? a, object o)
                                    {
                                        var x = o is int ? 1 : 2;

                                        return a == null ? x : 0;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the fix deletes spaces and a tab before the <c>?</c> of a nullable type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6015-004")]
    [Requirement("SHARED-WS-001")]
    public async Task VerifyFixDeletesWhitespaceBeforeNullableQuestionMark()
    {
        const string testData = """
                                #nullable enable

                                class C
                                {
                                    int{|RH6015: |}? _a;
                                    string{|RH6015:→|}? _b;

                                    void M(int{|RH6015: |}? a, object o)
                                    {
                                        var x = (int{|RH6015: |}?)o;
                                    }
                                }
                                """;
        const string fixedData = """
                                 #nullable enable

                                 class C
                                 {
                                     int? _a;
                                     string? _b;

                                     void M(int? a, object o)
                                     {
                                         var x = (int?)o;
                                     }
                                 }
                                 """;

        await Verify(ExpandWhitespaceMarkers(testData), fixedData);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int{|#0: |}? first, long{|#1:	|}? second)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int? first, long? second)
                                     {
                                     }
                                 }
                                 """;

        // Verifies that Fix All removes multiple same-line whitespace runs in one iteration
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH6015NullableTypeSymbolsMustNotBePrecededBySpaceAnalyzer.DiagnosticId, AnalyzerResources.RH6015MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}