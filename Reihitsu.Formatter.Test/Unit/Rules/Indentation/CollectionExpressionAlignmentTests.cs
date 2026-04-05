using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Rules;
using Reihitsu.Formatter.Rules.Indentation;

namespace Reihitsu.Formatter.Test.Unit.Rules.Indentation;

/// <summary>
/// Tests for <see cref="IndentationAndAlignmentRule"/> — collection-expression alignment
/// </summary>
[TestClass]
public class CollectionExpressionAlignmentTests
{
    #region Methods

    /// <summary>
    /// Verifies that a multi-line collection expression with elements aligned after the
    /// opening bracket remains unchanged.
    /// </summary>
    [TestMethod]
    public void AlignedCollectionExpressionRemainsUnchanged()
    {
        // Arrange
        const string input = """
        class C
        {
            private static readonly string[] _items = [
                                                          "Alpha",
                                                          "Bravo",
                                                          "Charlie"
                                                      ];
        }
        """;

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(Normalize(input), actual);
    }

    /// <summary>
    /// Verifies that a single-line collection expression remains unchanged.
    /// </summary>
    [TestMethod]
    public void SingleLineCollectionExpressionRemainsUnchanged()
    {
        // Arrange
        const string input = """
        class C
        {
            private static readonly string[] _items = ["Alpha", "Bravo"];
        }
        """;

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(Normalize(input), actual);
    }

    /// <summary>
    /// Verifies the current behavior for collection expressions assigned in a local declaration.
    /// Elements are aligned relative to the opening bracket column.
    /// </summary>
    [TestMethod]
    public void CollectionExpressionInLocalDeclarationAlignsToOpeningBracketColumnCurrentBehavior()
    {
        // Arrange
        const string input = """
        class C
        {
            void M()
            {
                string[] a = [
                    "a",
                    "b"
                ];
            }
        }
        """;

        const string expected = """
        class C
        {
            void M()
            {
                string[] a = [
                                 "a",
                                 "b"
                             ];
            }
        }
        """;

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(Normalize(expected), actual);
    }

    /// <summary>
    /// Verifies the current behavior for collection expressions assigned in a field initializer.
    /// Elements are aligned relative to the opening bracket column.
    /// </summary>
    [TestMethod]
    public void CollectionExpressionInFieldInitializerAlignsToOpeningBracketColumnCurrentBehavior()
    {
        // Arrange
        const string input = """
        class C
        {
            private static readonly string[] _a = [
                "a",
                "b"
            ];
        }
        """;

        const string expected = """
        class C
        {
            private static readonly string[] _a = [
                                                      "a",
                                                      "b"
                                                  ];
        }
        """;

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(Normalize(expected), actual);
    }

    /// <summary>
    /// Verifies the current behavior for collection expressions used with null-coalescing assignment.
    /// </summary>
    [TestMethod]
    public void CollectionExpressionInNullCoalescingAssignmentAlignsToOpeningBracketColumnCurrentBehavior()
    {
        // Arrange
        const string input = """
        class C
        {
            void M()
            {
                string[] a = null;
                a ??= [
                    "a",
                    "b"
                ];
            }
        }
        """;

        const string expected = """
        class C
        {
            void M()
            {
                string[] a = null;
                a ??= [
                          "a",
                          "b"
                      ];
            }
        }
        """;

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(Normalize(expected), actual);
    }

    #endregion // Methods

    #region Helper

    /// <summary>
    /// Normalizes line endings.
    /// </summary>
    /// <param name="text">The text to normalize.</param>
    /// <returns>The text with normalized line endings.</returns>
    private static string Normalize(string text)
    {
        return text.Replace("\r\n", "\n");
    }

    /// <summary>
    /// Parses and applies the <see cref="IndentationAndAlignmentRule"/>.
    /// </summary>
    /// <param name="input">The source code to format.</param>
    /// <returns>The formatted source code.</returns>
    private static string ApplyRule(string input)
    {
        input = Normalize(input);

        var tree = CSharpSyntaxTree.ParseText(input);
        var context = new FormattingContext("\n");
        var rule = new IndentationAndAlignmentRule(context, CancellationToken.None);
        var result = rule.Apply(tree.GetRoot());

        return result.ToFullString();
    }

    #endregion // Helper
}