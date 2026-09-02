using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5201MethodChainsShouldBeAlignedAnalyzer"/> and <see cref="RH5201MethodChainsShouldBeAlignedCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5201MethodChainsShouldBeAlignedAnalyzerTests : BatchCodeFixTestsBase<RH5201MethodChainsShouldBeAlignedAnalyzer, RH5201MethodChainsShouldBeAlignedCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying that misaligned method chains are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMisalignedMethodChainsAreDetectedAndFixed()
    {
        const string testData = """
                                using System;
                                using System.Globalization;
                                using System.Linq;
                                using System.Threading;
                                using System.Threading.Tasks;

                                internal class RH5201
                                {
                                    // Valid: entire chain on a single line
                                    void ValidSingleLine()
                                    {
                                        var a = new[] { 1, 2, 3 }.Where(x => x > 0).Select(x => x * 2).ToList();
                                        var b = "hello".Trim().ToUpper();
                                    }

                                    // Valid: first call inline, subsequent wrapped and aligned
                                    void ValidFirstInlineRestWrapped()
                                    {
                                        var a = new[] { 1, 2, 3 }.Where(x => x > 0)
                                                                 .Select(x => x * 2)
                                                                 .ToList();

                                        var b = "hello".Trim()
                                                       .ToUpper();
                                    }

                                    // Valid: all wrapped and aligned (first call also on own line)
                                    void ValidAllWrapped()
                                    {
                                        var a = new[] { 1, 2, 3 }
                                                .Where(x => x > 0)
                                                .Select(x => x * 2)
                                                .ToList();
                                    }

                                    // Valid: single member access (no chain)
                                    void ValidSingleAccess()
                                    {
                                        var a = "hello".Length;
                                    }

                                    // Valid: indexer transparent
                                    void ValidIndexer()
                                    {
                                        var a = new[] { new[] { 1 } }
                                                .First()[0]
                                                .ToString();
                                    }

                                    // Valid: nested chain in lambda (inner chain single-line)
                                    void ValidNestedLambda()
                                    {
                                        var a = new[] { "hello", "world" }.Where(x => x.Trim().Length > 0)
                                                                          .ToList();
                                    }

                                    // Valid: namespace-qualified method chain (namespace dots are not chain links)
                                    void ValidNamespaceQualified()
                                    {
                                        System.Linq.Enumerable.Range(0, 10)
                                                              .Where(x => x > 0)
                                                              .ToList();
                                    }

                                    // Valid: namespace-qualified method chain with continuation
                                    void ValidNamespaceQualifiedContinuation(CancellationTokenSource cancellationTokenSource)
                                    {
                                        System.Threading.Tasks.Task.Delay(60_000, cancellationTokenSource.Token)
                                                                   .ContinueWith(obj => { }, cancellationTokenSource.Token);
                                    }

                                    // Valid: property access before method chain (property dots are not chain links)
                                    void ValidPropertyAccess()
                                    {
                                        var a = DateTime.Now.Date.ToString("d", CultureInfo.InvariantCulture)
                                                                 .Trim();
                                    }

                                    // Valid: multiple property accesses before method chain
                                    void ValidMultiplePropertyAccess()
                                    {
                                        var a = DateTime.Now.Date.TimeOfDay.ToString()
                                                                           .Trim();
                                    }

                                    // Invalid: dots not aligned (misalignment)
                                    void InvalidMisaligned()
                                    {
                                        var a = new[] { 1, 2, 3 }.Where(x => x > 0)
                                                                 .Select(x => x * 2)
                                                                     {|#0:.|}ToList();
                                    }

                                    // Invalid: wrapped calls not aligned with first dot
                                    void InvalidNotAlignedWithFirst()
                                    {
                                        var b = new[] { 1, 2, 3 }.Where(x => x > 0)
                                            {|#1:.|}OrderBy(x => x)
                                            {|#2:.|}ToList();
                                    }

                                    // Invalid: one outlier dot
                                    void InvalidOutlier()
                                    {
                                        var d = "hello".Trim()
                                                       .ToUpper()
                                                         {|#3:.|}ToString();
                                    }

                                    // Invalid: middle call not wrapped when subsequent wraps
                                    void InvalidMiddleNotWrapped()
                                    {
                                        var e = new[] { 1, 2, 3 }.Where(x => x > 0){|#4:.|}Select(x => x)
                                                                 .ToList();
                                    }

                                    // Valid: null-forgiving operator in chain (aligned)
                                    void ValidNullForgiving()
                                    {
                                        var a = default(string[])!.Where(x => x.Length > 0)
                                                                 .Select(x => x.Trim())
                                                                 .ToList();
                                    }

                                    // Invalid: null-forgiving operator in chain (misaligned)
                                    void InvalidNullForgiving()
                                    {
                                        var a = default(string[])!.Where(x => x.Length > 0)
                                                                  {|#5:.|}Select(x => x.Trim())
                                                                  {|#6:.|}ToList();
                                    }
                                }
                                """;

        const string resultData = """
                                  using System;
                                  using System.Globalization;
                                  using System.Linq;
                                  using System.Threading;
                                  using System.Threading.Tasks;

                                  internal class RH5201
                                  {
                                      // Valid: entire chain on a single line
                                      void ValidSingleLine()
                                      {
                                          var a = new[] { 1, 2, 3 }.Where(x => x > 0).Select(x => x * 2).ToList();
                                          var b = "hello".Trim().ToUpper();
                                      }

                                      // Valid: first call inline, subsequent wrapped and aligned
                                      void ValidFirstInlineRestWrapped()
                                      {
                                          var a = new[] { 1, 2, 3 }.Where(x => x > 0)
                                                                   .Select(x => x * 2)
                                                                   .ToList();

                                          var b = "hello".Trim()
                                                         .ToUpper();
                                      }

                                      // Valid: all wrapped and aligned (first call also on own line)
                                      void ValidAllWrapped()
                                      {
                                          var a = new[] { 1, 2, 3 }
                                                  .Where(x => x > 0)
                                                  .Select(x => x * 2)
                                                  .ToList();
                                      }

                                      // Valid: single member access (no chain)
                                      void ValidSingleAccess()
                                      {
                                          var a = "hello".Length;
                                      }

                                      // Valid: indexer transparent
                                      void ValidIndexer()
                                      {
                                          var a = new[] { new[] { 1 } }
                                                  .First()[0]
                                                  .ToString();
                                      }

                                      // Valid: nested chain in lambda (inner chain single-line)
                                      void ValidNestedLambda()
                                      {
                                          var a = new[] { "hello", "world" }.Where(x => x.Trim().Length > 0)
                                                                            .ToList();
                                      }

                                      // Valid: namespace-qualified method chain (namespace dots are not chain links)
                                      void ValidNamespaceQualified()
                                      {
                                          System.Linq.Enumerable.Range(0, 10)
                                                                .Where(x => x > 0)
                                                                .ToList();
                                      }

                                      // Valid: namespace-qualified method chain with continuation
                                      void ValidNamespaceQualifiedContinuation(CancellationTokenSource cancellationTokenSource)
                                      {
                                          System.Threading.Tasks.Task.Delay(60_000, cancellationTokenSource.Token)
                                                                     .ContinueWith(obj => { }, cancellationTokenSource.Token);
                                      }

                                      // Valid: property access before method chain (property dots are not chain links)
                                      void ValidPropertyAccess()
                                      {
                                          var a = DateTime.Now.Date.ToString("d", CultureInfo.InvariantCulture)
                                                                   .Trim();
                                      }

                                      // Valid: multiple property accesses before method chain
                                      void ValidMultiplePropertyAccess()
                                      {
                                          var a = DateTime.Now.Date.TimeOfDay.ToString()
                                                                             .Trim();
                                      }

                                      // Invalid: dots not aligned (misalignment)
                                      void InvalidMisaligned()
                                      {
                                          var a = new[] { 1, 2, 3 }.Where(x => x > 0)
                                                                   .Select(x => x * 2)
                                                                   .ToList();
                                      }

                                      // Invalid: wrapped calls not aligned with first dot
                                      void InvalidNotAlignedWithFirst()
                                      {
                                          var b = new[] { 1, 2, 3 }.Where(x => x > 0)
                                                                   .OrderBy(x => x)
                                                                   .ToList();
                                      }

                                      // Invalid: one outlier dot
                                      void InvalidOutlier()
                                      {
                                          var d = "hello".Trim()
                                                         .ToUpper()
                                                         .ToString();
                                      }

                                      // Invalid: middle call not wrapped when subsequent wraps
                                      void InvalidMiddleNotWrapped()
                                      {
                                          var e = new[] { 1, 2, 3 }.Where(x => x > 0)
                                                                   .Select(x => x)
                                                                   .ToList();
                                      }

                                      // Valid: null-forgiving operator in chain (aligned)
                                      void ValidNullForgiving()
                                      {
                                          var a = default(string[])!.Where(x => x.Length > 0)
                                                                   .Select(x => x.Trim())
                                                                   .ToList();
                                      }

                                      // Invalid: null-forgiving operator in chain (misaligned)
                                      void InvalidNullForgiving()
                                      {
                                          var a = default(string[])!.Where(x => x.Length > 0)
                                                                   .Select(x => x.Trim())
                                                                   .ToList();
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH5201MethodChainsShouldBeAlignedAnalyzer.DiagnosticId, AnalyzerResources.RH5201MessageFormat, 7));
    }

    /// <summary>
    /// Verifies that nested conditional-access calls are collected, fixed, and clean after one code-fix application
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNestedConditionalAccessChainFixConvergesInOneApplication()
    {
        const string testData = """
                                using System.Linq;

                                internal sealed class Example
                                {
                                    private static int[] Convert(int[] values)
                                    {
                                        return values?.Where(value => value > 0)
                                                ?.ToArray();
                                    }
                                }
                                """;
        const string resultData = """
                                  using System.Linq;

                                  internal sealed class Example
                                  {
                                      private static int[] Convert(int[] values)
                                      {
                                          return values?.Where(value => value > 0)
                                                       ?.ToArray();
                                      }
                                  }
                                  """;

        var fixedSource = await ApplyCodeFixAsync(testData);

        Assert.AreEqual(resultData, fixedSource);
        await Verify(fixedSource);
    }

    /// <summary>
    /// Verifies that invoked member accesses within a conditional-access arm participate in chain alignment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMemberAccessInsideConditionalAccessArmIsDetectedAndFixed()
    {
        const string testData = """
                                using System.Linq;

                                internal sealed class Example
                                {
                                    private static int[] Convert(int[] values)
                                    {
                                        return values?.Where(value => value > 0)
                                                     .Select(value => value * 2)
                                                          {|#0:.|}ToArray();
                                    }
                                }
                                """;
        const string resultData = """
                                  using System.Linq;

                                  internal sealed class Example
                                  {
                                      private static int[] Convert(int[] values)
                                      {
                                          return values?.Where(value => value > 0)
                                                       .Select(value => value * 2)
                                                       .ToArray();
                                      }
                                  }
                                  """;

        await Verify(testData,
                     resultData,
                     Diagnostics(RH5201MethodChainsShouldBeAlignedAnalyzer.DiagnosticId, AnalyzerResources.RH5201MessageFormat));
    }

    /// <summary>
    /// Verifies that property accesses do not change the reference column used by the code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMixedPropertyAndInvocationChainFixConvergesInOneApplication()
    {
        const string testData = """
                                internal sealed class Example
                                {
                                    private static string Convert(int[] values)
                                    {
                                        return values.Length.ToString()
                                                .Trim();
                                    }
                                }
                                """;
        const string resultData = """
                                  internal sealed class Example
                                  {
                                      private static string Convert(int[] values)
                                      {
                                          return values.Length.ToString()
                                                              .Trim();
                                      }
                                  }
                                  """;

        var fixedSource = await ApplyCodeFixAsync(testData);

        Assert.AreEqual(resultData, fixedSource);
        await Verify(fixedSource);
    }

    /// <summary>
    /// Verifies that moving a same-line link does not leave the previous token with trailing whitespace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySameLineFixRemovesPreviousTokenTrailingWhitespace()
    {
        const string testData = """
                                using System.Linq;

                                internal sealed class Example
                                {
                                    private static int[] Convert(int[] values)
                                    {
                                        return values.Where(value => value > 0)  .Select(value => value)
                                                     .ToArray();
                                    }
                                }
                                """;
        const string resultData = """
                                  using System.Linq;

                                  internal sealed class Example
                                  {
                                      private static int[] Convert(int[] values)
                                      {
                                          return values.Where(value => value > 0)
                                                       .Select(value => value)
                                                       .ToArray();
                                      }
                                  }
                                  """;

        var fixedSource = await ApplyCodeFixAsync(testData);

        Assert.AreEqual(resultData, fixedSource);
        await Verify(fixedSource);
    }

    /// <summary>
    /// Verifies that moving a same-line link preserves a block comment in the token gap
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySameLineFixPreservesBlockCommentInTokenGap()
    {
        const string testData = """
                                using System.Linq;

                                internal sealed class Example
                                {
                                    private static int[] Convert(int[] values)
                                    {
                                        return values.Where(value => value > 0) /* Keep the next call. */ .Select(value => value)
                                                     .ToArray();
                                    }
                                }
                                """;
        const string resultData = """
                                  using System.Linq;

                                  internal sealed class Example
                                  {
                                      private static int[] Convert(int[] values)
                                      {
                                          return values.Where(value => value > 0) /* Keep the next call. */
                                                       .Select(value => value)
                                                       .ToArray();
                                      }
                                  }
                                  """;

        var fixedSource = await ApplyCodeFixAsync(testData);

        Assert.AreEqual(resultData, fixedSource);
        await Verify(fixedSource);
    }

    /// <summary>
    /// Verifies that reindenting a conditional link preserves a directive in the token gap
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixPreservesDirectiveInTokenGap()
    {
        const string testData = """
                                #define FEATURE

                                using System.Linq;

                                internal sealed class Example
                                {
                                    private static int[] Convert(int[] values)
                                    {
                                        return values
                                #if FEATURE
                                            ?.Where(value => value > 0)
                                #endif
                                                ?.ToArray();
                                    }
                                }
                                """;
        const string resultData = """
                                  #define FEATURE

                                  using System.Linq;

                                  internal sealed class Example
                                  {
                                      private static int[] Convert(int[] values)
                                      {
                                          return values
                                  #if FEATURE
                                              ?.Where(value => value > 0)
                                  #endif
                                              ?.ToArray();
                                      }
                                  }
                                  """;

        var fixedSource = await ApplyCodeFixAsync(testData);

        Assert.AreEqual(resultData, fixedSource);
        await Verify(fixedSource);
    }

    /// <summary>
    /// Verifies that a wrapped member-access dot remains part of the preceding null-forgiving chain link
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyWrappedMemberAccessAfterNullForgivingOperatorMatchesConditionalAccessPolicy()
    {
        const string testData = """
                                internal sealed class Example
                                {
                                    private static string Convert(string value)
                                    {
                                        return value?.Trim()!
                                .ToString();
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that conditional-access and null-forgiving operators use the same chain-link alignment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyConditionalAccessAndNullForgivingOperatorsAlignAsChainLinks()
    {
        const string testData = """
                                internal sealed class Example
                                {
                                    private static string Convert(string value)
                                    {
                                        return value.Trim()
                                                    ?.ToString()
                                                    !.Trim();
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the inserted line break matches the document's detected CRLF end-of-line sequence instead of
    /// <see cref="System.Environment.NewLine"/>, so the fix does not introduce mixed line endings (issue #257)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyInsertedLineBreakUsesDetectedCarriageReturnLineFeedEndOfLine()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        var e = new[] { 1, 2, 3 }.Where(x => x > 0).Select(x => x)
                                                                 .ToList();
                                    }
                                }
                                """;

        var fixedSource = await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(testData));

        Assert.DoesNotContain("\n", fixedSource.Replace("\r\n", string.Empty));
    }

    /// <summary>
    /// Verifies that a compiled-out link is not a chain link: parsed without <c>DEBUG</c>, the
    /// <c>#if</c> branch is disabled text, so only the two active links are compared and their shared
    /// column reports nothing. This pins the parse-time boundary only — it does not cover the case
    /// where <c>DEBUG</c> is defined, in which the same text is still flagged because
    /// <c>reihitsu-format</c> parses with no preprocessor symbols while the analyzer uses the
    /// project's (issue #489)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyChainWithDisabledLinkReportsNoDiagnostic()
    {
        const string testData = """
                                internal sealed class Example
                                {
                                    private static object Create(Builder builder)
                                    {
                                        return builder
                                #if DEBUG
                                            .UseLogging()
                                #endif
                                               .UseValidation()
                                               .Build();
                                    }
                                }

                                internal sealed class Builder
                                {
                                    public Builder UseLogging() => this;

                                    public Builder UseValidation() => this;

                                    public object Build() => this;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the formatter's own output for a method chain rooted in a parenthesized
    /// <c>with</c> expression is RH5201-clean. The output is produced directly through
    /// <see cref="FormattingPipeline"/> — the same engine <c>reihitsu-format</c> uses — and fed to the
    /// analyzer as-is, so the two surfaces are checked against each other rather than against a
    /// hand-written expectation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterOutputForChainRootedInParenthesizedWithExpressionIsClean()
    {
        const string rawInput = """
                                class C
                                {
                                    void M(Record r)
                                    {
                                        var x = (r with
                                        {
                                            Value = 1
                                        }).Select(item => item)
                                           .ToList();
                                    }
                                }
                                """;

        var formatted = FormatWithLineFeed(rawInput);

        await Verify(formatted, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Formats the given source text through the shared formatting pipeline with LF line endings,
    /// the same engine <c>reihitsu-format</c> uses, without going through the CLI
    /// </summary>
    /// <param name="rawInput">The source text to format</param>
    /// <returns>The formatted source text</returns>
    private static string FormatWithLineFeed(string rawInput)
    {
        var tree = CSharpSyntaxTree.ParseText(rawInput);
        var context = new FormattingContext("\n");

        return FormattingPipeline.Execute(tree.GetRoot(), context, CancellationToken.None).ToFullString();
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                using System.Linq;

                                internal sealed class Example
                                {
                                    private static int[] Convert(int[] values)
                                    {
                                        return values.Where(value => value > 0)  {|#0:.|}Select(value => value)  {|#1:.|}OrderBy(value => value)
                                                     .ToArray();
                                    }
                                }
                                """;
        const string resultData = """
                                  using System.Linq;

                                  internal sealed class Example
                                  {
                                      private static int[] Convert(int[] values)
                                      {
                                          return values.Where(value => value > 0)
                                                       .Select(value => value)
                                                       .OrderBy(value => value)
                                                       .ToArray();
                                      }
                                  }
                                  """;

        // Verifies that Fix All moves multiple same-line links without leaving whitespace or diagnostics behind
        return new FixAllScenario(testData,
                                  resultData,
                                  Diagnostics(RH5201MethodChainsShouldBeAlignedAnalyzer.DiagnosticId, AnalyzerResources.RH5201MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}