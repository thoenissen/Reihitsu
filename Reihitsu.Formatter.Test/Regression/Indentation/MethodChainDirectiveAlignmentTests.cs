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
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The formatted source text</returns>
    private static string Format(string input,
                                 bool defineDebug,
                                 CancellationToken cancellationToken)
    {
        var parseOptions = new CSharpParseOptions(preprocessorSymbols: defineDebug ? ["DEBUG"] : []);
        var tree = CSharpSyntaxTree.ParseText(input, parseOptions, cancellationToken: cancellationToken);
        var context = new FormattingContext("\n");
        var result = FormattingPipeline.Execute(tree.GetRoot(cancellationToken), context, cancellationToken);

        return result.ToFullString();
    }

    /// <summary>
    /// Normalizes raw string literals to the line ending used by <see cref="Format"/>
    /// </summary>
    /// <param name="text">The text to normalize</param>
    /// <returns>The normalized text</returns>
    private static string NormalizeLineEndings(string text)
    {
        return text.Replace("\r\n", "\n");
    }

    /// <summary>
    /// Asserts that formatting produces the expected output and is idempotent
    /// </summary>
    /// <param name="input">The C# source text</param>
    /// <param name="expected">The expected formatted output</param>
    /// <param name="defineDebug">Whether <c>DEBUG</c> is defined, keeping the conditional branch active</param>
    private void AssertFormatted(string input,
                                 string expected,
                                 bool defineDebug = true)
    {
        var actual = Format(input, defineDebug, TestContext.CancellationToken);

        Assert.AreEqual(NormalizeLineEndings(expected), actual, "The chain must stay aligned under its root.");

        var secondPass = Format(actual, defineDebug, TestContext.CancellationToken);

        Assert.AreEqual(NormalizeLineEndings(expected), secondPass, "Formatting must be idempotent.");
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
    /// out, so the remaining active links are not flattened to the statement column
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

    #endregion // Tests
}