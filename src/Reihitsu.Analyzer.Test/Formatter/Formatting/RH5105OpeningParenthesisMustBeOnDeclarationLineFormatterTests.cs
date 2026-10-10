using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5105OpeningParenthesisMustBeOnDeclarationLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH5105OpeningParenthesisMustBeOnDeclarationLineFormatterTests : FormatterTestsBase<RH5105OpeningParenthesisMustBeOnDeclarationLineAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter moves the opening parenthesis onto the declaration line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 void Method
                                 {|#0:(|}int value)
                                 {
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     void Method(int value)
                                     {
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5105OpeningParenthesisMustBeOnDeclarationLineAnalyzer.DiagnosticId, AnalyzerResources.RH5105MessageFormat));
    }

    /// <summary>
    /// Verifies that a type primary-constructor opener carried by the preceding token's trailing end-of-line is joined
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterJoinsPrimaryConstructorOpeningGapAndIsIdempotent()
    {
        const string input = """
                             internal class Example
                             {|#0:(|}int value)
                             {
                             }
                             """;
        const string fixedData = """
                                 internal class Example(int value);
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5105OpeningParenthesisMustBeOnDeclarationLineAnalyzer.DiagnosticId, AnalyzerResources.RH5105MessageFormat));
    }

    /// <summary>
    /// Verifies that formatter-owned callback wrapping remains stable and does not create an RH5105 diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterWrappedParenthesizedLambdaIsStable()
    {
        const string input = """
                             using System;

                             internal class Example
                             {
                                 void Method()
                                 {
                                     Accept("callback",
                                            (int item) => item);
                                 }

                                 void Accept(string name, Func<int, int> callback)
                                 {
                                 }
                             }
                             """;

        await VerifyFormatter(input);
    }

    /// <summary>
    /// Verifies that the formatter collapses the opening parenthesis of a generic method's parameter list, whose
    /// break the type-parameter list's closing angle bracket carries, matching RH5105's own policy
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesGenericMethodViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 void Method<TValue>
                                 {|#0:(|}TValue value)
                                 {
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     void Method<TValue>(TValue value)
                                     {
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5105OpeningParenthesisMustBeOnDeclarationLineAnalyzer.DiagnosticId, AnalyzerResources.RH5105MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter collapses the opening parenthesis of a generic local function's parameter list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesGenericLocalFunctionViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 void Outer()
                                 {
                                     void Local<TValue>
                                     {|#0:(|}TValue value)
                                     {
                                     }
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     void Outer()
                                     {
                                         void Local<TValue>(TValue value)
                                         {
                                         }
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5105OpeningParenthesisMustBeOnDeclarationLineAnalyzer.DiagnosticId, AnalyzerResources.RH5105MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter collapses the opening parenthesis of an operator declaration's parameter list —
    /// a shape neither formatter collapse rewriter covered before the LineBreaks/StructuralTransforms cleanup
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesOperatorViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 public static Example operator +
                                 {|#0:(|}Example left, Example right)
                                 {
                                     return left;
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     public static Example operator +(Example left, Example right)
                                     {
                                         return left;
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5105OpeningParenthesisMustBeOnDeclarationLineAnalyzer.DiagnosticId, AnalyzerResources.RH5105MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter collapses the opening parenthesis of a generic delegate declaration's parameter list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesGenericDelegateViolation()
    {
        const string input = """
                             internal delegate void TestDelegate<TValue>
                             {|#0:(|}TValue value);
                             """;
        const string fixedData = """
                                 internal delegate void TestDelegate<TValue>(TValue value);
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5105OpeningParenthesisMustBeOnDeclarationLineAnalyzer.DiagnosticId, AnalyzerResources.RH5105MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter collapses the opening parenthesis of a record declaration's primary-constructor
    /// parameter list — a shape neither formatter collapse rewriter covered before the LineBreaks/StructuralTransforms cleanup
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesRecordViolation()
    {
        const string input = """
                             internal record TestRecord
                             {|#0:(|}int Value);
                             """;
        const string fixedData = """
                                 internal record TestRecord(int Value);
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5105OpeningParenthesisMustBeOnDeclarationLineAnalyzer.DiagnosticId, AnalyzerResources.RH5105MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter collapses the opening parenthesis of a generic record declaration's
    /// primary-constructor parameter list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesGenericRecordViolation()
    {
        const string input = """
                             internal record TestRecord<TValue>
                             {|#0:(|}TValue Value);
                             """;
        const string fixedData = """
                                 internal record TestRecord<TValue>(TValue Value);
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5105OpeningParenthesisMustBeOnDeclarationLineAnalyzer.DiagnosticId, AnalyzerResources.RH5105MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment between a generic method's type-parameter list and its parameter list still refuses
    /// the collapse — the widened owner-scoped policy must keep respecting the same unjoinable-trivia guard as the
    /// plain (non-generic) shape
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterGenericMethodCommentInGapIsStable()
    {
        const string input = """
                             internal class Example
                             {
                                 void Method<TValue>

                                 // why
                                 (TValue value)
                                 {
                                 }
                             }
                             """;

        await VerifyFormatter(input);
    }

    #endregion // Tests
}