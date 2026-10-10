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
/// Test methods for <see cref="RH6012ClosingGenericBracketsMustBeSpacedCorrectlyAnalyzer"/> and <see cref="RH6012ClosingGenericBracketsMustBeSpacedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH6012ClosingGenericBracketsMustBeSpacedCorrectlyAnalyzerTests : BatchCodeFixTestsBase<RH6012ClosingGenericBracketsMustBeSpacedCorrectlyAnalyzer, RH6012ClosingGenericBracketsMustBeSpacedCorrectlyCodeFixProvider>
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
                                using System.Collections.Generic;
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        _ = new List<int>();
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
    public async Task VerifyIssueIsDetectedAndFixed()
    {
        const string testData = """
                                using System.Collections.Generic;
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        _ = new List<int{|#0: |}>();
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         _ = new List<int>();
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6012ClosingGenericBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6012MessageFormat));
    }

    /// <summary>
    /// Verifies that a space before the closing generic bracket of a type parameter list is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTypeParameterListIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method<T{|#0: |}>(T value)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method<T>(T value)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6012ClosingGenericBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6012MessageFormat));
    }

    /// <summary>
    /// Verifies that a tab before a closing generic bracket is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTabBeforeClosingGenericBracketIsDetectedAndFixed()
    {
        const string testData = """
                                using System.Collections.Generic;
                                internal class TestClass
                                {
                                    List<int{|#0:	|}> Method() => new();
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;
                                 internal class TestClass
                                 {
                                     List<int> Method() => new();
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6012ClosingGenericBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6012MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment before a same-line closing generic bracket still reports the diagnostic but
    /// withholds the fix, because deleting the whitespace run would glue the comment to the bracket
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixIsNotOfferedForSameLineClosingGenericBracketWithPrecedingComment()
    {
        const string testData = """
                                using System.Collections.Generic;
                                internal class TestClass
                                {
                                    List<int /* Keep. */{|#0: |}> Method() => new();
                                }
                                """;

        await Verify(testData, Diagnostics(RH6012ClosingGenericBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6012MessageFormat));

        var actions = await GetCodeFixActionsAsync(testData.Replace("{|#0: |}", " "),
                                                   RH6012ClosingGenericBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId,
                                                   root =>
                                                   {
                                                       var typeArgumentList = root.DescendantNodes().OfType<TypeArgumentListSyntax>().First();
                                                       var lastTrailingTrivia = typeArgumentList.GreaterThanToken.GetPreviousToken().TrailingTrivia.Last();

                                                       Assert.IsTrue(lastTrailingTrivia.IsKind(SyntaxKind.WhitespaceTrivia),
                                                                     "The preceding token's trailing trivia must end with the whitespace run immediately before the bracket.");

                                                       return Location.Create(root.SyntaxTree, lastTrailingTrivia.Span);
                                                   });

        Assert.IsEmpty(actions,
                       "The comment sits in the same token gap as the reported whitespace run; the fix must be withheld.");
    }

    /// <summary>
    /// Repeats <see cref="VerifyFixIsNotOfferedForSameLineClosingGenericBracketWithPrecedingComment"/> with the
    /// source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixIsNotOfferedForSameLineClosingGenericBracketWithPrecedingCommentCarriageReturnLineFeed()
    {
        const string testData = """
                                using System.Collections.Generic;
                                internal class TestClass
                                {
                                    List<int /* Keep. */{|#0: |}> Method() => new();
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData), Diagnostics(RH6012ClosingGenericBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6012MessageFormat));

        var actions = await GetCodeFixActionsAsync(NormalizeToCarriageReturnLineFeed(testData.Replace("{|#0: |}", " ")),
                                                   RH6012ClosingGenericBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId,
                                                   root =>
                                                   {
                                                       var typeArgumentList = root.DescendantNodes().OfType<TypeArgumentListSyntax>().First();
                                                       var lastTrailingTrivia = typeArgumentList.GreaterThanToken.GetPreviousToken().TrailingTrivia.Last();

                                                       Assert.IsTrue(lastTrailingTrivia.IsKind(SyntaxKind.WhitespaceTrivia),
                                                                     "The preceding token's trailing trivia must end with the whitespace run immediately before the bracket.");

                                                       return Location.Create(root.SyntaxTree, lastTrailingTrivia.Span);
                                                   });

        Assert.IsEmpty(actions,
                       "The comment sits in the same token gap as the reported whitespace run; the fix must be withheld.");
    }

    /// <summary>
    /// Verifies that a continuation-line closing generic bracket preceded by a block comment still reports the
    /// diagnostic on the same-line whitespace run between the comment and the bracket, and that the fix is
    /// withheld rather than offered, because the guard must inspect the gap the whitespace run actually sits in
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixIsNotOfferedForContinuationLineClosingGenericBracketWithPrecedingComment()
    {
        const string testData = """
                                using System.Collections.Generic;
                                internal class TestClass
                                {
                                    List<int
                                        /* keep */{|#0: |}> Method() => new();
                                }
                                """;

        await Verify(testData, Diagnostics(RH6012ClosingGenericBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6012MessageFormat));

        var actions = await GetCodeFixActionsAsync(testData.Replace("{|#0: |}", " "),
                                                   RH6012ClosingGenericBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId,
                                                   root =>
                                                   {
                                                       var typeArgumentList = root.DescendantNodes().OfType<TypeArgumentListSyntax>().First();
                                                       var lastLeadingTrivia = typeArgumentList.GreaterThanToken.LeadingTrivia.Last();

                                                       Assert.IsTrue(lastLeadingTrivia.IsKind(SyntaxKind.WhitespaceTrivia),
                                                                     "The bracket's leading trivia must end with the whitespace run immediately preceding it.");

                                                       return Location.Create(root.SyntaxTree, lastLeadingTrivia.Span);
                                                   });

        Assert.IsEmpty(actions,
                       "The comment sits between the previous token and the reported whitespace run; the fix must be withheld.");
    }

    /// <summary>
    /// Repeats <see cref="VerifyFixIsNotOfferedForContinuationLineClosingGenericBracketWithPrecedingComment"/>
    /// with the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixIsNotOfferedForContinuationLineClosingGenericBracketWithPrecedingCommentCarriageReturnLineFeed()
    {
        const string testData = """
                                using System.Collections.Generic;
                                internal class TestClass
                                {
                                    List<int
                                        /* keep */{|#0: |}> Method() => new();
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData), Diagnostics(RH6012ClosingGenericBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6012MessageFormat));

        var actions = await GetCodeFixActionsAsync(NormalizeToCarriageReturnLineFeed(testData.Replace("{|#0: |}", " ")),
                                                   RH6012ClosingGenericBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId,
                                                   root =>
                                                   {
                                                       var typeArgumentList = root.DescendantNodes().OfType<TypeArgumentListSyntax>().First();
                                                       var lastLeadingTrivia = typeArgumentList.GreaterThanToken.LeadingTrivia.Last();

                                                       Assert.IsTrue(lastLeadingTrivia.IsKind(SyntaxKind.WhitespaceTrivia),
                                                                     "The bracket's leading trivia must end with the whitespace run immediately preceding it.");

                                                       return Location.Create(root.SyntaxTree, lastLeadingTrivia.Span);
                                                   });

        Assert.IsEmpty(actions,
                       "The comment sits between the previous token and the reported whitespace run; the fix must be withheld.");
    }

    /// <summary>
    /// Verifies that a closing generic bracket on a continuation line does not produce a diagnostic with LF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyContinuationLineClosingGenericBracketDoesNotProduceDiagnostic()
    {
        const string testData = """
                                using System.Collections.Generic;
                                internal class TestClass
                                {
                                    List<int
                                        > Method() => new();
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a closing generic bracket on a continuation line does not produce a diagnostic with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyContinuationLineClosingGenericBracketDoesNotProduceDiagnosticWithCarriageReturnLineFeed()
    {
        const string testData = """
                                using System.Collections.Generic;
                                internal class TestClass
                                {
                                    List<int
                                        > Method() => new();
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData));
    }

    /// <summary>
    /// Verifies that disabled text and a directive inside the gap before a continuation-line closing generic bracket do not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDirectiveAndDisabledTextBeforeContinuationLineClosingGenericBracketDoNotProduceDiagnostic()
    {
        const string testData = """
                                using System.Collections.Generic;
                                internal class TestClass
                                {
                                    List<int
                                #if false
                                    disabled text
                                #endif
                                        > Method() => new();
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that disabled text and a directive inside the gap before a continuation-line closing generic bracket do not produce a diagnostic with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDirectiveAndDisabledTextBeforeContinuationLineClosingGenericBracketDoNotProduceDiagnosticWithCarriageReturnLineFeed()
    {
        const string testData = """
                                using System.Collections.Generic;
                                internal class TestClass
                                {
                                    List<int
                                #if false
                                    disabled text
                                #endif
                                        > Method() => new();
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData));
    }

    /// <summary>
    /// Verifies that a space before a closing generic bracket inside a documentation-comment cref is not
    /// flagged, because the formatter never rewrites inside a cref and node-kind dispatch reaches structured
    /// trivia that the previous tree walk never saw
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySpaceBeforeClosingGenericBracketInsideCrefIsIgnored()
    {
        const string testData = """
                                using System.Collections.Generic;

                                /// <summary>
                                /// See <see cref="List{T }"/>.
                                /// </summary>
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        _ = new List<int>();
                                    }
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                using System.Collections.Generic;
                                internal class TestClass
                                {
                                    List<int{|#0: |}> First() => new();
                                    List<string{|#1:	|}> Second() => new();
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;
                                 internal class TestClass
                                 {
                                     List<int> First() => new();
                                     List<string> Second() => new();
                                 }
                                 """;

        // Verifies that Fix All removes multiple same-line whitespace runs in one iteration
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH6012ClosingGenericBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6012MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}