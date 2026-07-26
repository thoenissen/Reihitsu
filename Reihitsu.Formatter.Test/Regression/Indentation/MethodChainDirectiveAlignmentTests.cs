using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// Regression tests for issue #489: a preprocessor directive above the first wrapped fluent call keeps
/// that call on its continuation line, exactly as a comment does. The method-chain alignment
/// contributor must therefore align the chain links under the chain root instead of letting the
/// generic block indentation flatten them to the statement column
/// </summary>
[TestClass]
public class MethodChainDirectiveAlignmentTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Asserts that formatting produces the expected output under both LF and CRLF line endings,
    /// honoring the requested ending byte-for-byte and staying idempotent
    /// </summary>
    /// <param name="input">The C# source text</param>
    /// <param name="expected">The expected formatted output</param>
    /// <param name="defineDebug">Whether <c>DEBUG</c> is defined, keeping the conditional branch active</param>
    private static void AssertFormatted(string input,
                                        string expected,
                                        bool defineDebug = true)
    {
        AssertRuleResult(input, expected, new CSharpParseOptions(preprocessorSymbols: defineDebug ? ["DEBUG"] : []));
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