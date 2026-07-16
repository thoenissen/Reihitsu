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

                             class C;
                             """;
        const string expected = """
                                // Keep with Alpha
                                using Alpha.Alpha;
                                using Alpha.Zeta;

                                class C;
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

                             class C;
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

                             class C;
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

                             class C;
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

                             class C;
                             """;
        const string expected = """
                                using Alpha.Alpha; // Keep with Alpha
                                using Alpha.Zeta;

                                class C;
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

                             class C;
                             """;
        const string expected = """
                                // Keep alias attached
                                using AAlias = Alpha.Alpha;
                                using ZAlias = Alpha.Zeta;

                                class C;
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that aliases targeting different root namespaces are ordered by their target root namespace and
    /// separated into groups. This is the order the analyzers must converge on so the formatter output never
    /// carries an RH7204 (alias ordering) diagnostic
    /// </summary>
    [TestMethod]
    public void AliasesAreOrderedByTargetRootNamespace()
    {
        // Arrange
        const string input = """
                             using Z = A.B;
                             using A = Z.Q;

                             class C;
                             """;
        const string expected = """
                                using Z = A.B;

                                using A = Z.Q;

                                class C;
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

                             class C;
                             """;
        const string expected = """
                                // Keep static attached
                                using static Alpha.Alpha;
                                using static Alpha.Zeta;

                                class C;
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a file-header banner above the original first using directive stays at the top of
    /// the scope when reordering demotes that directive, instead of traveling into the middle of the
    /// using block (issue #432)
    /// </summary>
    [TestMethod]
    public void BannerStaysAtTopWhenOriginalFirstDirectiveIsDemoted()
    {
        // Arrange
        const string input = """
                             // Copyright (c) Example Corp. All rights reserved.

                             using System.Linq;
                             using System.Collections.Generic;

                             class C;
                             """;
        const string expected = """
                                // Copyright (c) Example Corp. All rights reserved.

                                using System.Collections.Generic;
                                using System.Linq;

                                class C;
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a multi-line banner stays intact at the top of the scope when the demoted
    /// directive moves into a different group and picks up its own blank-line separator (issue #432)
    /// </summary>
    [TestMethod]
    public void MultiLineBannerStaysAtTopWhenOriginalFirstDirectiveIsDemotedAcrossGroups()
    {
        // Arrange
        const string input = """
                             //
                             // Copyright (c) X. All rights reserved.
                             //

                             using Z.Lib;
                             using System;

                             class C;
                             """;
        const string expected = """
                                //
                                // Copyright (c) X. All rights reserved.
                                //

                                using System;

                                using Z.Lib;

                                class C;
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a banner stays at the top of the scope when reordering demotes the original first
    /// global using directive (issue #432)
    /// </summary>
    [TestMethod]
    public void BannerStaysAtTopWithGlobalUsingReordering()
    {
        // Arrange
        const string input = """
                             // Copyright (c) Example Corp. All rights reserved.

                             global using System.Linq;
                             global using System.Collections.Generic;

                             class C;
                             """;
        const string expected = """
                                // Copyright (c) Example Corp. All rights reserved.

                                global using System.Collections.Generic;
                                global using System.Linq;

                                class C;
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a banner stays at the top of the scope when reordering demotes the original first
    /// alias directive (issue #432)
    /// </summary>
    [TestMethod]
    public void BannerStaysAtTopWithAliasReordering()
    {
        // Arrange
        const string input = """
                             // Copyright (c) Example Corp. All rights reserved.

                             using ZAlias = System.Linq;
                             using AAlias = System.Collections;

                             class C;
                             """;
        const string expected = """
                                // Copyright (c) Example Corp. All rights reserved.

                                using AAlias = System.Collections;
                                using ZAlias = System.Linq;

                                class C;
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