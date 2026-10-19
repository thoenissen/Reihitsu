using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Spacing;
using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH6024BinaryOperatorsMustBeSpacedCorrectlyAnalyzer"/> and <see cref="RH6024BinaryOperatorsMustBeSpacedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH6024BinaryOperatorsMustBeSpacedCorrectlyAnalyzerTests : BatchCodeFixTestsBase<RH6024BinaryOperatorsMustBeSpacedCorrectlyAnalyzer, RH6024BinaryOperatorsMustBeSpacedCorrectlyCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that clean code does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6024-001")]
    [Requirement("SHARED-OPSPACE-002")]
    public async Task VerifyNoDiagnosticsWhenCodeIsClean()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    int Method(int a, int b)
                                    {
                                        return a + b;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that padding around a binary operator is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6024-001")]
    [Requirement("RH6024-003")]
    [Requirement("SHARED-OPSPACE-001")]
    [Requirement("SHARED-OPSPACE-003")]
    [Requirement("SHARED-OPSPACE-004")]
    public async Task VerifyPaddingAroundBinaryOperatorIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    int Method(int a, int b)
                                    {
                                        return a  {|#0:+|}  b;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     int Method(int a, int b)
                                     {
                                         return a + b;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6024BinaryOperatorsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6024MessageFormat));
    }

    /// <summary>
    /// Verifies that a missing space around a binary operator is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6024-001")]
    [Requirement("RH6024-003")]
    [Requirement("SHARED-OPSPACE-001")]
    [Requirement("SHARED-OPSPACE-003")]
    [Requirement("SHARED-OPSPACE-004")]
    public async Task VerifyMissingSpaceAroundBinaryOperatorIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    int Method(int a, int b)
                                    {
                                        return a{|#0:+|}b;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     int Method(int a, int b)
                                     {
                                         return a + b;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6024BinaryOperatorsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6024MessageFormat));
    }

    /// <summary>
    /// Verifies that a comparison operator is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6024-001")]
    [Requirement("RH6024-003")]
    [Requirement("SHARED-OPSPACE-001")]
    [Requirement("SHARED-OPSPACE-004")]
    public async Task VerifyComparisonOperatorIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    bool Method(int a, int b)
                                    {
                                        return a{|#0:==|}b;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     bool Method(int a, int b)
                                     {
                                         return a == b;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6024BinaryOperatorsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6024MessageFormat));
    }

    /// <summary>
    /// Verifies that a logical operator is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6024-001")]
    [Requirement("RH6024-003")]
    [Requirement("SHARED-OPSPACE-001")]
    [Requirement("SHARED-OPSPACE-004")]
    public async Task VerifyLogicalOperatorIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    bool Method(bool a, bool b)
                                    {
                                        return a  {|#0:&&|}  b;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     bool Method(bool a, bool b)
                                     {
                                         return a && b;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6024BinaryOperatorsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6024MessageFormat));
    }

    /// <summary>
    /// Verifies that a binary operator that wraps to a new line does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6024-001")]
    [Requirement("SHARED-OPSPACE-002")]
    public async Task VerifyWrappedBinaryOperatorIsIgnored()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    int Method(int a, int b)
                                    {
                                        return a
                                               + b;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that extra padding around the <c>is</c> keyword operator is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6024-001")]
    [Requirement("RH6024-003")]
    [Requirement("SHARED-OPSPACE-001")]
    [Requirement("SHARED-OPSPACE-004")]
    public async Task VerifyIsKeywordOperatorIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    bool Method(object value)
                                    {
                                        return value  {|#0:is|}  string;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     bool Method(object value)
                                     {
                                         return value is string;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6024BinaryOperatorsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6024MessageFormat));
    }

    /// <summary>
    /// Verifies that extra padding around the <c>as</c> keyword operator is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6024-001")]
    [Requirement("RH6024-003")]
    [Requirement("SHARED-OPSPACE-001")]
    [Requirement("SHARED-OPSPACE-004")]
    public async Task VerifyAsKeywordOperatorIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    string Method(object value)
                                    {
                                        return value  {|#0:as|}  string;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     string Method(object value)
                                     {
                                         return value as string;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6024BinaryOperatorsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6024MessageFormat));
    }

    /// <summary>
    /// Verifies that a keyword operator on a continuation line does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6024-001")]
    [Requirement("SHARED-OPSPACE-002")]
    public async Task VerifyWrappedKeywordOperatorIsIgnored()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    string Method(object value)
                                    {
                                        return value
                                               as string;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that generic type arguments do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6024-001")]
    public async Task VerifyGenericTypeArgumentsAreIgnored()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class TestClass
                                {
                                    List<int> Method()
                                    {
                                        return new List<int>();
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a binary operator inside a preprocessor directive condition is not flagged, because the
    /// formatter never rewrites a directive condition and node-kind dispatch reaches structured trivia that the
    /// previous tree walk never saw
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6024-002")]
    public async Task VerifyDirectiveConditionOperatorIsIgnored()
    {
        const string testData = """
                                #if DEBUG  &&  TRACE
                                internal class TestClass
                                {
                                }
                                #endif

                                internal class Fallback
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the analyzer registers every <see cref="SyntaxKind"/> Roslyn classifies as a binary
    /// expression, so a hand-maintained kind list can never silently drop an operator family from this rule's
    /// coverage the way the type-hierarchy check it replaced could not
    /// </summary>
    [TestMethod]
    [Requirement("RH6024-001")]
    public void VerifyRegisteredKindsCoverEveryBinaryExpressionKind()
    {
        // SyntaxFacts.IsBinaryExpression/GetBinaryExpression classify the operator TOKEN kind, not the
        // expression kind, so every recognized operator token is mapped to the expression kind it produces.
        var expectedKinds = Enum.GetValues<SyntaxKind>()
                                .Where(SyntaxFacts.IsBinaryExpression)
                                .Select(SyntaxFacts.GetBinaryExpression)
                                .OrderBy(static kind => (int)kind)
                                .ToImmutableArray();
        var registeredKinds = RH6024BinaryOperatorsMustBeSpacedCorrectlyAnalyzer.BinaryExpressionKinds.OrderBy(static kind => (int)kind).ToImmutableArray();

        Assert.AreSequenceEqual(expectedKinds, registeredKinds);
    }

    /// <summary>
    /// Verifies that arithmetic, shift, comparison, logical, null-coalescing, and type-test operators are reported once
    /// when a side is not exactly one space, also inside an interpolation hole, and that a wrapped side is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6024-001")]
    [Requirement("SHARED-OPSPACE-001")]
    [Requirement("SHARED-OPSPACE-003")]
    public async Task VerifyEveryBinaryOperatorIsChecked()
    {
        const string testData = """
                                class C
                                {
                                    bool M(int a, int b, object o, string s)
                                    {
                                        _ = a{|RH6024:+|}b;
                                        _ = a  {|RH6024:+|}  b;
                                        _ = a→{|RH6024:*|} b;
                                        _ = a{|RH6024:==|}b && a{|RH6024:!=|}b;
                                        _ = a{|RH6024:<|}b || a{|RH6024:>=|}b;
                                        _ = a{|RH6024:<<|}1;
                                        _ = a{|RH6024:>>>|}1;
                                        _ = (o){|RH6024:is|} string;
                                        _ = o {|RH6024:as|}  string;
                                        _ = s{|RH6024:??|}"x";
                                        _ = a{|RH6024:-|} -b;
                                        _ = $"{a{|RH6024:+|}b}";
                                        _ = a +
                                            b;

                                        return a{|RH6024:>|}b;
                                    }
                                }
                                """;

        await Verify(ExpandWhitespaceMarkers(testData));
    }

    /// <summary>
    /// Repeats <see cref="VerifyEveryBinaryOperatorIsChecked"/> with the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6024-001")]
    [Requirement("SHARED-OPSPACE-001")]
    [Requirement("SHARED-OPSPACE-003")]
    public async Task VerifyEveryBinaryOperatorIsCheckedCarriageReturnLineFeed()
    {
        const string testData = """
                                class C
                                {
                                    bool M(int a, int b, object o, string s)
                                    {
                                        _ = a{|RH6024:+|}b;
                                        _ = a  {|RH6024:+|}  b;
                                        _ = a→{|RH6024:*|} b;
                                        _ = a{|RH6024:==|}b && a{|RH6024:!=|}b;
                                        _ = a{|RH6024:<|}b || a{|RH6024:>=|}b;
                                        _ = a{|RH6024:<<|}1;
                                        _ = a{|RH6024:>>>|}1;
                                        _ = (o){|RH6024:is|} string;
                                        _ = o {|RH6024:as|}  string;
                                        _ = s{|RH6024:??|}"x";
                                        _ = a{|RH6024:-|} -b;
                                        _ = $"{a{|RH6024:+|}b}";
                                        _ = a +
                                            b;

                                        return a{|RH6024:>|}b;
                                    }
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(ExpandWhitespaceMarkers(testData)));
    }

    /// <summary>
    /// Verifies that an operator in a directive condition and a mis-spaced <c>is</c> followed by a pattern are not
    /// reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6024-002")]
    public async Task VerifyDirectiveConditionsAndPatternsAreNotReported()
    {
        const string testData = """
                                class C
                                {
                                    bool M(int a, object o)
                                    {
                                #if DEBUG&&TRACE
                                #endif
                                        _ = o is  null;
                                        _ = o  is string s;

                                        return a is  > 0 and < 5;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the fix puts one space on both sides of an arithmetic, type-test, null-coalescing, and comparison
    /// operator
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6024-003")]
    [Requirement("SHARED-OPSPACE-004")]
    public async Task VerifyFixPutsOneSpaceOnBothSidesOfBinaryOperators()
    {
        const string testData = """
                                class C
                                {
                                    bool M(int a, int b, object o, string s)
                                    {
                                        _ = a{|RH6024:+|}b;
                                        _ = (o){|RH6024:is|} string;
                                        _ = s{|RH6024:??|}"x";
                                        _ = a{|RH6024:+|}-b;

                                        return a  {|RH6024:>|}  b;
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     bool M(int a, int b, object o, string s)
                                     {
                                         _ = a + b;
                                         _ = (o) is string;
                                         _ = s ?? "x";
                                         _ = a + -b;

                                         return a > b;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData);
    }

    /// <summary>
    /// Verifies that a gap without whitespace, with more than one space, or with a tab is reported, also when the other
    /// side holds a comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-OPSPACE-001")]
    [Requirement("SHARED-OPSPACE-003")]
    public async Task VerifyGapThatIsNotExactlyOneSpaceIsReported()
    {
        const string testData = """
                                class C
                                {
                                    int M(int a, int b)
                                    {
                                        var x = a{|RH6024:+|}b;
                                        var y = a {|RH6024:+|}b;
                                        var z = a  {|RH6024:+|}  b;
                                        var w = a→{|RH6024:*|} b;
                                        var v = a /* note */{|RH6024:+|}b;

                                        return a +
                                            b;
                                    }
                                }
                                """;

        await Verify(ExpandWhitespaceMarkers(testData));
    }

    /// <summary>
    /// Repeats <see cref="VerifyGapThatIsNotExactlyOneSpaceIsReported"/> with the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-OPSPACE-001")]
    [Requirement("SHARED-OPSPACE-003")]
    public async Task VerifyGapThatIsNotExactlyOneSpaceIsReportedCarriageReturnLineFeed()
    {
        const string testData = """
                                class C
                                {
                                    int M(int a, int b)
                                    {
                                        var x = a{|RH6024:+|}b;
                                        var y = a {|RH6024:+|}b;
                                        var z = a  {|RH6024:+|}  b;
                                        var w = a→{|RH6024:*|} b;
                                        var v = a /* note */{|RH6024:+|}b;

                                        return a +
                                            b;
                                    }
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(ExpandWhitespaceMarkers(testData)));
    }

    /// <summary>
    /// Verifies that an operator whose gaps each hold exactly one space, a line break, or a comment is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-OPSPACE-002")]
    public async Task VerifyGapsWithOneSpaceLineBreakOrCommentAreNotReported()
    {
        const string testData = """
                                class C
                                {
                                    int M(int a, int b)
                                    {
                                        var x = a + b;
                                        var y = a /* note */+ b;
                                        var z = a + /* note */b;
                                        var w = a
                                            + b;

                                        return a +
                                            b;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Repeats <see cref="VerifyGapsWithOneSpaceLineBreakOrCommentAreNotReported"/> with the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-OPSPACE-002")]
    public async Task VerifyGapsWithOneSpaceLineBreakOrCommentAreNotReportedCarriageReturnLineFeed()
    {
        const string testData = """
                                class C
                                {
                                    int M(int a, int b)
                                    {
                                        var x = a + b;
                                        var y = a /* note */+ b;
                                        var z = a + /* note */b;
                                        var w = a
                                            + b;

                                        return a +
                                            b;
                                    }
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData));
    }

    /// <summary>
    /// Verifies that an operator whose gap after it holds a line break and a directive is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-OPSPACE-002")]
    public async Task VerifyGapWithDirectiveIsNotReported()
    {
        const string testData = """
                                class C
                                {
                                    int M(int a, int b)
                                    {
                                        return a +
                                #if DEBUG
                                #endif
                                            b;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Repeats <see cref="VerifyGapWithDirectiveIsNotReported"/> with the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-OPSPACE-002")]
    public async Task VerifyGapWithDirectiveIsNotReportedCarriageReturnLineFeed()
    {
        const string testData = """
                                class C
                                {
                                    int M(int a, int b)
                                    {
                                        return a +
                                #if DEBUG
                                #endif
                                            b;
                                    }
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData));
    }

    /// <summary>
    /// Verifies that the fix leaves exactly one space in each gap next to the operator that holds no line break and no
    /// comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-OPSPACE-004")]
    public async Task VerifyFixLeavesOneSpaceInEachGap()
    {
        const string testData = """
                                class C
                                {
                                    int M(int a, int b)
                                    {
                                        var x = a{|RH6024:+|}b;
                                        var z = a  {|RH6024:+|}  b;
                                        var w = a→{|RH6024:*|} b;

                                        return a{|RH6024:-|} -b;
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     int M(int a, int b)
                                     {
                                         var x = a + b;
                                         var z = a + b;
                                         var w = a * b;

                                         return a - -b;
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
                                    bool Method(object value)
                                    {
                                        return value  {|#0:is|}  string && value  {|#1:as|}  string != null;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     bool Method(object value)
                                     {
                                         return value is string && value as string != null;
                                     }
                                 }
                                 """;

        // Verifies that Fix All normalizes multiple keyword-operator diagnostics in one iteration
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH6024BinaryOperatorsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6024MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}