using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.UsingDirectives;

/// <summary>
/// Unit tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/>
/// </summary>
[TestClass]
public class UsingDirectiveOrderingTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a line comment moves together with its using directive during reordering
    /// </summary>
    [TestMethod]
    public void LineCommentMovesWithUsingDirective()
    {
        // Arrange
        const string input = """
                             using Alpha.Zeta;
                             // Keep with Alpha
                             using Alpha.Alpha;

                             class C
                             {
                             }
                             """;
        const string expected = """
                                // Keep with Alpha
                                using Alpha.Alpha;
                                using Alpha.Zeta;

                                class C
                                {
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a using block with a nullable directive on a later directive is not reordered
    /// </summary>
    [TestMethod]
    public void BlockWithNullableDirectiveIsNotReordered()
    {
        // Arrange
        const string input = """
                             using System;

                             #nullable enable
                             using System.Linq;

                             class C
                             {
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a using block with conditional directives is not reordered
    /// </summary>
    [TestMethod]
    public void BlockWithConditionalDirectiveIsNotReordered()
    {
        // Arrange
        const string input = """
                             using System;
                             #if DEBUG
                             using System.Linq;
                             #endif

                             class C
                             {
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a using block with a pragma warning directive on a later directive is not reordered
    /// </summary>
    [TestMethod]
    public void BlockWithPragmaWarningDirectiveIsNotReordered()
    {
        // Arrange
        const string input = """
                             using System;

                             #pragma warning disable CS8019
                             using System.Linq;

                             class C
                             {
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an inline trailing comment moves together with its using directive during reordering
    /// </summary>
    [TestMethod]
    public void InlineTrailingCommentMovesWithUsingDirective()
    {
        // Arrange
        const string input = """
                             using Alpha.Zeta;
                             using Alpha.Alpha; // Keep with Alpha

                             class C
                             {
                             }
                             """;
        const string expected = """
                                using Alpha.Alpha; // Keep with Alpha
                                using Alpha.Zeta;

                                class C
                                {
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment attached to an alias directive is preserved during reordering
    /// </summary>
    [TestMethod]
    public void AliasDirectiveCommentMovesWithUsingDirective()
    {
        // Arrange
        const string input = """
                             using ZAlias = Alpha.Zeta;
                             // Keep alias attached
                             using AAlias = Alpha.Alpha;

                             class C
                             {
                             }
                             """;
        const string expected = """
                                // Keep alias attached
                                using AAlias = Alpha.Alpha;
                                using ZAlias = Alpha.Zeta;

                                class C
                                {
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment attached to a static using is preserved during reordering
    /// </summary>
    [TestMethod]
    public void StaticUsingCommentMovesWithUsingDirective()
    {
        // Arrange
        const string input = """
                             using static Alpha.Zeta;
                             // Keep static attached
                             using static Alpha.Alpha;

                             class C
                             {
                             }
                             """;
        const string expected = """
                                // Keep static attached
                                using static Alpha.Alpha;
                                using static Alpha.Zeta;

                                class C
                                {
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that attached comments remain with non-first namespace usings after reordering
    /// </summary>
    [TestMethod]
    public void NamespaceUsingsWithCommentsKeepAttachedTrivia()
    {
        // Arrange
        const string input = """
                             namespace Example
                             {
                                 using Zeta;
                                 using System.Collections;
                                 // Keep with Alpha
                                 using Alpha;
                             }
                             """;
        const string expected = """
                                namespace Example
                                {
                                    using System.Collections;

                                    // Keep with Alpha
                                    using Alpha;

                                    using Zeta;
                                }
                                """;

        // Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}