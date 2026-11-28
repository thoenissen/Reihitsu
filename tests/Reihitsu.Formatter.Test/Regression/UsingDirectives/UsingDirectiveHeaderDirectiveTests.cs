using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.UsingDirectives;

/// <summary>
/// Full-pipeline tests for a using block with preprocessor directives in front of its first directive: a block whose directives in
/// front are not conditional is reordered with its whole leading trivia kept at the top, while a conditional directive in front of
/// the block, or a directive between its directives or between the tokens of one, keeps the block as written
/// </summary>
[TestClass]
public class UsingDirectiveHeaderDirectiveTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies the change's main case: a block led by a <c>#nullable</c> directive is reordered, and the directive stays at the top
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [Requirement("FMT-USING-012")]
    public void NullableHeaderStaysAtTheTopOfTheReorderedBlock()
    {
        // Arrange
        const string input = """
                             #nullable enable
                             using System.Linq;
                             using System;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                #nullable enable
                                using System;
                                using System.Linq;

                                class C
                                {
                                    int x;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: a block led by a <c>#nullable</c> directive whose order is canonical gets the blank line between its groups
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [Requirement("FMT-USING-002")]
    public void HeaderDirectiveBlockInCanonicalOrderGetsItsGroupBlankLine()
    {
        // Arrange
        const string input = """
                             #nullable enable
                             using System;
                             using Microsoft.Win32;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                #nullable enable
                                using System;

                                using Microsoft.Win32;

                                class C
                                {
                                    int x;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an organized block led by a <c>#nullable</c> directive, whose first directive keeps its position, is left as written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-012")]
    public void OrganizedBlockLedByAHeaderDirectiveIsUnchanged()
    {
        // Arrange
        const string input = """
                             #nullable enable

                             // About System
                             using System;
                             using System.Linq;

                             using Microsoft.Win32;

                             class C
                             {
                                 int x;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the specification example: a directive behind a comment header's blank line stays at the top, where the header split alone would move it with the original first directive
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-012")]
    public void DirectiveBehindAHeaderBlankLineStaysAtTheTop()
    {
        // Arrange
        const string input = """
                             // Header

                             #nullable enable
                             using System.Linq;
                             using System;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                // Header

                                #nullable enable
                                using System;
                                using System.Linq;

                                class C
                                {
                                    int x;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: a blank line behind a header directive stays behind it at the top of the block
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-012")]
    public void BlankLineBehindAHeaderDirectiveStaysBehindIt()
    {
        // Arrange
        const string input = """
                             #nullable enable

                             using System.Linq;
                             using System;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                #nullable enable

                                using System;
                                using System.Linq;

                                class C
                                {
                                    int x;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: a comment directly below a header directive stays at the top with it, and no blank line is added
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-012")]
    public void CommentDirectlyBelowAHeaderDirectiveStaysAtTheTop()
    {
        // Arrange
        const string input = """
                             #nullable enable
                             // About Linq
                             using System.Linq;
                             using System;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                #nullable enable
                                // About Linq
                                using System;
                                using System.Linq;

                                class C
                                {
                                    int x;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: a comment behind a blank line below a header directive stays at the top with the directive and the blank line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-012")]
    public void CommentBehindABlankLineBelowAHeaderDirectiveStaysAtTheTop()
    {
        // Arrange
        const string input = """
                             #nullable enable

                             // About Linq
                             using System.Linq;
                             using System;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                #nullable enable

                                // About Linq
                                using System;
                                using System.Linq;

                                class C
                                {
                                    int x;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: a <c>#region</c> in front of the block stays at the top of the reordered block and is framed as any region, while the <c>#endregion</c> behind the block stays where it is
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-012")]
    public void RegionOpeningTheBlockIsFramedAndStaysAtTheTop()
    {
        // Arrange
        const string input = """
                             #region Usings
                             using System.Linq;
                             using System;
                             #endregion

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                #region Usings

                                using System;
                                using System.Linq;

                                #endregion // Usings

                                class C
                                {
                                    int x;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that two header directives separated by a blank line stay at the top together with the blank line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-012")]
    public void TwoHeaderDirectivesSeparatedByABlankLineStayAtTheTop()
    {
        // Arrange
        const string input = """
                             #nullable enable

                             #pragma warning disable CS1591
                             using System.Linq;
                             using System;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                #nullable enable

                                #pragma warning disable CS1591
                                using System;
                                using System.Linq;

                                class C
                                {
                                    int x;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment header, a blank line, and a comment directly above a header directive all stay at the top
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-012")]
    public void CommentsAndBlankLineInFrontOfAHeaderDirectiveStayAtTheTop()
    {
        // Arrange
        const string input = """
                             // Header

                             // About nullable
                             #nullable enable
                             using System.Linq;
                             using System;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                // Header

                                // About nullable
                                #nullable enable
                                using System;
                                using System.Linq;

                                class C
                                {
                                    int x;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a copyright header, a blank line, and two directives stay at the top of the reordered block, and that a directive behind the block stays behind it
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [Requirement("FMT-USING-012")]
    public void CopyrightHeaderAndTwoDirectivesStayAtTheTopAndADirectiveBehindTheBlockStaysBehindIt()
    {
        // Arrange
        const string input = """
                             // <copyright file="C.cs">
                             // Header
                             // </copyright>

                             #nullable enable
                             #pragma warning disable CS0105
                             using System.Linq;
                             using System;
                             #pragma warning restore CS0105

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                // <copyright file="C.cs">
                                // Header
                                // </copyright>

                                #nullable enable
                                #pragma warning disable CS0105
                                using System;
                                using System.Linq;
                                #pragma warning restore CS0105

                                class C
                                {
                                    int x;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a <c>#define</c> directive in front of the block stays at the top, in front of the file's first token
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [Requirement("FMT-USING-012")]
    public void DefineHeaderStaysInFrontOfTheFirstToken()
    {
        // Arrange
        const string input = """
                             #define TRACE_X
                             using System.Linq;
                             using System;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                #define TRACE_X
                                using System;
                                using System.Linq;

                                class C
                                {
                                    int x;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a header directive in front of a block of global directives stays at the top of the reordered block
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [Requirement("FMT-USING-012")]
    public void HeaderDirectiveInFrontOfGlobalDirectivesStaysAtTheTop()
    {
        // Arrange
        const string input = """
                             #nullable enable
                             global using System.Linq;
                             global using System;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                #nullable enable
                                global using System;
                                global using System.Linq;

                                class C
                                {
                                    int x;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a header directive in front of the block of a namespace with a body stays at the top of the reordered block, in column 0, and the directives keep the namespace's indentation
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [Requirement("FMT-USING-012")]
    public void HeaderDirectiveOfANamespaceWithABodyStaysAtTheTop()
    {
        // Arrange
        const string input = """
                             namespace N
                             {
                                 #nullable enable
                                 using System.Linq;
                                 using System;

                                 class C
                                 {
                                     int x;
                                 }
                             }
                             """;
        const string expected = """
                                namespace N
                                {
                                #nullable enable
                                    using System;
                                    using System.Linq;

                                    class C
                                    {
                                        int x;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a header directive in front of the block of a file-scoped namespace stays at the top of the reordered block
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [Requirement("FMT-USING-012")]
    public void HeaderDirectiveOfAFileScopedNamespaceStaysAtTheTop()
    {
        // Arrange
        const string input = """
                             namespace N;

                             #nullable enable
                             using System.Linq;
                             using System;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                namespace N;

                                #nullable enable
                                using System;
                                using System.Linq;

                                class C
                                {
                                    int x;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a block led by a header directive whose directives share a line is reordered and split, as a block without the directive
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [Requirement("FMT-USING-003")]
    public void HeaderDirectiveBlockWithSharedLineIsReorderedAndSplit()
    {
        // Arrange
        const string input = """
                             #nullable enable
                             using System.Linq; using System;
                             using Microsoft.CodeAnalysis;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                #nullable enable
                                using System;
                                using System.Linq;

                                using Microsoft.CodeAnalysis;

                                class C
                                {
                                    int x;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a block led by a header directive that also holds a directive in front of a later directive is not reordered
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [Requirement("FMT-USING-004")]
    public void HeaderDirectiveDoesNotMakeABlockWithALaterDirectiveSafe()
    {
        // Arrange
        const string input = """
                             #nullable enable
                             using System.Linq;
                             #pragma warning disable CS0105
                             using System;

                             class C
                             {
                                 int x;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the change doc's unchanged case: a directive between the tokens of the block's first directive keeps the block as written
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [Requirement("FMT-USING-004")]
    public void DirectiveBetweenTokensOfTheFirstDirectiveKeepsTheBlock()
    {
        // Arrange
        const string input = """
                             using
                             #pragma warning disable CS0105
                                 System.Linq;
                             using System;

                             class C
                             {
                                 int x;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a block led by a header directive that also holds a directive between the tokens of its first directive is not reordered
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [Requirement("FMT-USING-004")]
    public void HeaderDirectiveDoesNotMakeABlockWithADirectiveBetweenTokensOfItsFirstDirectiveSafe()
    {
        // Arrange
        const string input = """
                             #nullable enable
                             using
                             #pragma warning disable CS0105
                                 System.Linq;
                             using System;

                             class C
                             {
                                 int x;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the specification example: a block behind a conditional group whose code the build excludes is not reordered
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [Requirement("FMT-USING-004")]
    public void ConditionalGroupInFrontOfTheBlockKeepsIt()
    {
        // Arrange
        const string input = """
                             #if DEBUG
                             using System.Diagnostics;
                             #endif
                             using System.Linq;
                             using System;

                             class C
                             {
                                 int x;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a block in canonical order behind a conditional group does not get the blank line between its groups
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [Requirement("FMT-USING-004")]
    public void ConditionalGroupInFrontOfACanonicalBlockAddsNoGroupBlankLine()
    {
        // Arrange
        const string input = """
                             #if DEBUG
                             using System.Diagnostics;
                             #endif
                             using System;
                             using Microsoft.Win32;

                             class C
                             {
                                 int x;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the specification example: in a block behind a conditional group, only the line two directives share is split
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [Requirement("FMT-USING-004")]
    public void ConditionalGroupInFrontOfTheBlockOnlySplitsASharedLine()
    {
        // Arrange
        const string input = """
                             #if DEBUG
                             using System.Diagnostics;
                             #endif
                             using System.Linq; using System;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                #if DEBUG
                                using System.Diagnostics;
                                #endif
                                using System.Linq;
                                using System;

                                class C
                                {
                                    int x;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: a block that a conditional group encloses is not reordered
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [Requirement("FMT-USING-004")]
    public void ConditionalGroupEnclosingTheBlockKeepsIt()
    {
        // Arrange
        const string input = """
                             #if !LEGACY
                             using System.Linq;
                             using System;
                             #endif

                             class C
                             {
                                 int x;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a block of a namespace with a body behind a conditional group is not reordered
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [Requirement("FMT-USING-004")]
    public void ConditionalGroupInFrontOfANamespaceBlockKeepsIt()
    {
        // Arrange
        const string input = """
                             namespace N
                             {
                             #if DEBUG
                                 using System.Diagnostics;
                             #endif
                                 using System.Linq;
                                 using System;

                                 class C
                                 {
                                     int x;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a conditional group keeps the block as written also when a non-conditional directive stands in front of it
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [Requirement("FMT-USING-004")]
    public void HeaderDirectiveAndConditionalGroupInFrontOfTheBlockKeepIt()
    {
        // Arrange
        const string input = """
                             #nullable enable
                             #if DEBUG
                             using System.Diagnostics;
                             #endif
                             using System.Linq;
                             using System;

                             class C
                             {
                                 int x;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    #endregion // Methods
}