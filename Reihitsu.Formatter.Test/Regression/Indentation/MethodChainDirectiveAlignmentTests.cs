using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// Regression tests for issue #489: a preprocessor directive above the first wrapped fluent call keeps
/// that call on its continuation line, exactly as a comment does. The method-chain alignment
/// contributor must therefore align the chain links under the chain root instead of letting the
/// generic block indentation flatten them to the statement column
/// </summary>
[TestClass]
public class MethodChainDirectiveAlignmentTests
{
    #region Fields

    /// <summary>
    /// The line endings every fixture is exercised against, so the directive handling is pinned under
    /// CRLF as well as LF (issue #330)
    /// </summary>
    private static readonly string[] _lineEndings = ["\n", "\r\n"];

    #endregion // Fields

    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Formats the source through the full pipeline
    /// </summary>
    /// <param name="input">The C# source text</param>
    /// <param name="defineDebug">Whether <c>DEBUG</c> is defined, keeping the conditional branch active</param>
    /// <param name="endOfLine">The end-of-line sequence to format with</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The formatted source text</returns>
    private static string Format(string input,
                                 bool defineDebug,
                                 string endOfLine,
                                 CancellationToken cancellationToken)
    {
        var parseOptions = new CSharpParseOptions(preprocessorSymbols: defineDebug ? ["DEBUG"] : []);
        var tree = CSharpSyntaxTree.ParseText(input, parseOptions, cancellationToken: cancellationToken);
        var context = new FormattingContext(endOfLine);
        var result = FormattingPipeline.Execute(tree.GetRoot(cancellationToken), context, cancellationToken);

        return result.ToFullString();
    }

    /// <summary>
    /// Rewrites every line break in the given text to the requested end-of-line sequence
    /// </summary>
    /// <param name="text">The text to normalize</param>
    /// <param name="endOfLine">The target end-of-line sequence</param>
    /// <returns>The text using the requested line endings</returns>
    private static string NormalizeLineEndings(string text, string endOfLine)
    {
        var lineFeedOnly = text.Replace("\r\n", "\n");

        return endOfLine == "\n" ? lineFeedOnly : lineFeedOnly.Replace("\n", endOfLine);
    }

    /// <summary>
    /// Verifies that the given text uses the requested end-of-line sequence for every line break
    /// </summary>
    /// <param name="text">The formatted text to inspect</param>
    /// <param name="endOfLine">The end-of-line sequence every line break must use</param>
    private static void AssertUsesLineEnding(string text, string endOfLine)
    {
        if (endOfLine == "\n")
        {
            Assert.IsFalse(text.Contains('\r'), "Formatted output must not contain a carriage return when LF line endings are requested.");
        }
        else
        {
            var withoutCrlf = text.Replace("\r\n", string.Empty);

            Assert.IsFalse(withoutCrlf.Contains('\n') || withoutCrlf.Contains('\r'), "Formatted output must use CRLF for every line break.");
        }
    }

    /// <summary>
    /// Asserts that formatting produces the expected output, honors the requested end-of-line sequence,
    /// and is idempotent, under both LF and CRLF line endings
    /// </summary>
    /// <param name="input">The C# source text</param>
    /// <param name="expected">The expected formatted output</param>
    /// <param name="defineDebug">Whether <c>DEBUG</c> is defined, keeping the conditional branch active</param>
    private void AssertFormatted(string input,
                                 string expected,
                                 bool defineDebug = true)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var endingName = endOfLine == "\n" ? "LF" : "CRLF";
            var normalizedInput = NormalizeLineEndings(input, endOfLine);
            var normalizedExpected = NormalizeLineEndings(expected, endOfLine);
            var actual = Format(normalizedInput, defineDebug, endOfLine, TestContext.CancellationToken);

            Assert.AreEqual(normalizedExpected, actual, $"The chain must stay aligned under its root under {endingName} line endings.");
            AssertUsesLineEnding(actual, endOfLine);

            var secondPass = Format(actual, defineDebug, endOfLine, TestContext.CancellationToken);

            Assert.AreEqual(normalizedExpected, secondPass, $"Formatting must be idempotent under {endingName} line endings.");
        }
    }

    #endregion // Methods

    #region Tests

    /// <summary>
    /// Verifies that a directive above the first wrapped fluent call aligns the chain links under the
    /// chain root token, mirroring the comment behavior instead of flattening the chain to the
    /// statement column
    /// </summary>
    [TestMethod]
    public void DirectiveAboveFirstWrappedCallAlignsChainUnderRoot()
    {
        // Arrange
        const string input = """
                             internal sealed class Example
                             {
                                 private static object Create()
                                 {
                                     return new Builder()
                             #if DEBUG
                                         .UseLogging()
                             #endif
                                         .UseValidation()
                                         .Build();
                                 }
                             }
                             """;

        const string expected = """
                                internal sealed class Example
                                {
                                    private static object Create()
                                    {
                                        return new Builder()
                                #if DEBUG
                                               .UseLogging()
                                #endif
                                               .UseValidation()
                                               .Build();
                                    }
                                }
                                """;

        // Act & Assert
        AssertFormatted(input, expected);
    }

    /// <summary>
    /// Verifies that the chain stays aligned under its root when the conditional branch is compiled
    /// out, so the remaining active links are not flattened to the statement column. The compiled-out
    /// link keeps its original column because the formatter never rewrites disabled text
    /// </summary>
    [TestMethod]
    public void DisabledBranchAboveFirstWrappedCallAlignsChainUnderRoot()
    {
        // Arrange
        const string input = """
                             internal sealed class Example
                             {
                                 private static object Create()
                                 {
                                     return new Builder()
                             #if DEBUG
                                         .UseLogging()
                             #endif
                                         .UseValidation()
                                         .Build();
                                 }
                             }
                             """;

        const string expected = """
                                internal sealed class Example
                                {
                                    private static object Create()
                                    {
                                        return new Builder()
                                #if DEBUG
                                            .UseLogging()
                                #endif
                                               .UseValidation()
                                               .Build();
                                    }
                                }
                                """;

        // Act & Assert
        AssertFormatted(input, expected, false);
    }

    /// <summary>
    /// Verifies that a directive around a middle chain link still collapses the first link onto the
    /// root line and aligns the remaining links to the first dot, so the fix does not widen the
    /// exemption beyond the first wrapped call
    /// </summary>
    [TestMethod]
    public void DirectiveAroundMiddleLinkKeepsFirstLinkOnRootLine()
    {
        // Arrange
        const string input = """
                             internal sealed class Example
                             {
                                 private static void Run(Builder builder)
                                 {
                                     builder.UseLogging()
                                            .UseTracing()
                             #if DEBUG
                                            .UseValidation()
                             #endif
                                            .Build();
                                 }
                             }
                             """;

        const string expected = """
                                internal sealed class Example
                                {
                                    private static void Run(Builder builder)
                                    {
                                        builder.UseLogging()
                                               .UseTracing()
                                #if DEBUG
                                               .UseValidation()
                                #endif
                                               .Build();
                                    }
                                }
                                """;

        // Act & Assert
        AssertFormatted(input, expected);
    }

    /// <summary>
    /// Verifies that a directive above the first wrapped call of a chain nested in an argument keeps
    /// the chain aligned under the chain root rather than under the enclosing statement
    /// </summary>
    [TestMethod]
    public void DirectiveAboveFirstWrappedCallInArgumentAlignsChainUnderRoot()
    {
        // Arrange
        const string input = """
                             internal sealed class Example
                             {
                                 private static void Run(Builder builder)
                                 {
                                     Register(builder
                             #if DEBUG
                                         .UseLogging()
                             #endif
                                         .Build());
                                 }
                             }
                             """;

        const string expected = """
                                internal sealed class Example
                                {
                                    private static void Run(Builder builder)
                                    {
                                        Register(builder
                                #if DEBUG
                                                 .UseLogging()
                                #endif
                                                 .Build());
                                    }
                                }
                                """;

        // Act & Assert
        AssertFormatted(input, expected);
    }

    /// <summary>
    /// Verifies the headline shape from issue #489: an <c>#endif</c> sitting after the chain's
    /// terminating semicolon is never joined onto the last chain link, which would re-emit the
    /// directive mid-line and fail to compile
    /// </summary>
    [TestMethod]
    public void DirectiveAfterTerminatingSemicolonKeepsItsOwnLine()
    {
        // Arrange
        const string input = """
                             internal sealed class Example
                             {
                                 private static void Run(Builder builder)
                                 {
                                     builder.UseLogging()
                                            .UseTracing()
                             #if DEBUG
                                            .UseValidation();
                             #endif
                                 }
                             }
                             """;

        const string expected = """
                                internal sealed class Example
                                {
                                    private static void Run(Builder builder)
                                    {
                                        builder.UseLogging()
                                               .UseTracing()
                                #if DEBUG
                                               .UseValidation();
                                #endif
                                    }
                                }
                                """;

        // Act & Assert
        AssertFormatted(input, expected);
    }

    #endregion // Tests
}