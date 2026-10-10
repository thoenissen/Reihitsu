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