using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.UsingDirectives;

/// <summary>
/// Full-pipeline tests for the using-block layout no narrower document-level test pins: the blank lines between
/// and inside groups, same-line directives, block comments that trail a directive, the block's ending, and the
/// comments above a directive that moves
/// </summary>
[TestClass]
public class UsingDirectiveRequirementTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that several blank lines between two groups collapse to exactly one
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-002")]
    public void SeveralBlankLinesBetweenGroupsBecomeOne()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Linq;



                             using Microsoft.CodeAnalysis;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
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
    /// Verifies that a blank line inside a group is removed while a missing one between groups is added
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-002")]
    public void BlankLineInsideGroupIsRemovedAndOneBetweenGroupsIsAdded()
    {
        // Arrange
        const string input = """
                             using System;

                             using System.Linq;
                             using Microsoft.CodeAnalysis;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
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
    /// Verifies that two same-group directives sharing one line are put on separate lines
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-003")]
    public void SameGroupDirectivesSharingOneLineAreSplit()
    {
        // Arrange
        const string input = """
                             using System; using System.Linq;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
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
    /// Verifies that in a block a directive forbids to reorder, same-line directives are split while their order and
    /// the missing group separator are left as they are
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-004")]
    public void UnsafeBlockOnlyGetsOneDirectivePerLine()
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
                                using System.Linq;
                                using System;
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
    /// Verifies that a block comment trailing a directive that shares its line with the next directive stays on its
    /// line, with the line break placed after it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-006")]
    public void BlockCommentStaysOnTheLineItTrails()
    {
        // Arrange
        const string input = """
                             using System; /* note */ using Microsoft.CodeAnalysis;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                using System; /* note */

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
    /// Verifies that a directive moved up keeps its line comment, which gets a line break of its own, while the
    /// block's ending stays after the new last directive
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-007")]
    public void BlockEndingStaysWhenCommentedLastDirectiveMovesUp()
    {
        // Arrange
        const string input = """
                             using System.Text;
                             using System; // core

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                using System; // core
                                using System.Text;

                                class C
                                {
                                    int x;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the block's ending is kept when nothing separates it from the next declaration
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-007")]
    public void BlockEndingWithoutBlankLineIsKept()
    {
        // Arrange
        const string input = """
                             using System.Text;
                             using System;
                             using System.Linq;
                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                using System;
                                using System.Linq;
                                using System.Text;
                                class C
                                {
                                    int x;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment above a directive that starts a new group moves with it and gets the group's blank line
    /// above the comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-008")]
    public void LeadingCommentMovesWithDirectiveIntoNewGroup()
    {
        // Arrange
        const string input = """
                             using Reihitsu.Core;
                             // About Analysis
                             using Microsoft.CodeAnalysis;
                             using System;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                using System;

                                // About Analysis
                                using Microsoft.CodeAnalysis;

                                using Reihitsu.Core;

                                class C
                                {
                                    int x;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment after the blank line that ends the header moves with the original first directive,
    /// while the header stays at the top
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-009")]
    public void CommentAfterHeaderBlankLineMovesWithItsDirective()
    {
        // Arrange
        const string input = """
                             // Header

                             // About Analysis
                             using Microsoft.CodeAnalysis;
                             using System;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
                                // Header

                                using System;

                                // About Analysis
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
    /// Verifies that a comment directly above the original first directive, with no blank line, stays at the top
    /// </summary>
    [TestMethod]
    [Requirement("FMT-USING-009")]
    public void CommentDirectlyAboveFirstDirectiveStaysAtTheTop()
    {
        // Arrange
        const string input = """
                             // About Linq
                             using System.Linq;
                             using System;

                             class C
                             {
                                 int x;
                             }
                             """;
        const string expected = """
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

    #endregion // Methods
}