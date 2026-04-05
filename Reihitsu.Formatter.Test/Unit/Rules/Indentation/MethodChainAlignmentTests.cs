using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Rules;
using Reihitsu.Formatter.Rules.Indentation;

namespace Reihitsu.Formatter.Test.Unit.Rules.Indentation;

/// <summary>
/// Tests for <see cref="IndentationAndAlignmentRule"/> — method-chain alignment
/// </summary>
[TestClass]
public class MethodChainAlignmentTests
{
    #region Methods

    /// <summary>
    /// Verifies that a single-line method chain remains unchanged.
    /// </summary>
    [TestMethod]
    public void SingleLineChainRemainsUnchanged()
    {
        // Arrange
        const string input = """
        var x = a.Foo().Bar().Baz();
        """;

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(Normalize(input), actual);
    }

    /// <summary>
    /// Verifies that a chain with only a single link on a different line is collapsed to the same line.
    /// </summary>
    [TestMethod]
    public void SingleLinkCollapsesToSameLine()
    {
        // Arrange
        const string input = """
        var x = a
                    .Foo();
        """;

        const string expected = """
        var x = a.Foo();
        """;

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(Normalize(expected), actual);
    }

    /// <summary>
    /// Verifies that a multi-line method chain collapses the first link to the root line
    /// and aligns subsequent dots to the first dot's column.
    /// </summary>
    [TestMethod]
    public void MultiLineChainCollapsesFirstLinkAndAligns()
    {
        // Arrange — first dot on different line, others misaligned
        const string input = """
        var x = a
            .Foo()
                  .Bar()
            .Baz();
        """;

        const string expected = """
        var x = a.Foo()
                 .Bar()
                 .Baz();
        """;

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(Normalize(expected), actual);
    }

    /// <summary>
    /// Verifies that conditional access tokens (<c>?.</c>) in a mixed chain are collapsed and aligned correctly.
    /// </summary>
    [TestMethod]
    public void ChainWithConditionalAccessCollapsesAndAligns()
    {
        // Arrange — obj.Foo().Bar()?.Baz() with dots/? at different columns
        const string input = """
        var x = obj
            .Foo()
                  .Bar()
                      ?.Baz();
        """;

        const string expected = """
        var x = obj.Foo()
                   .Bar()
                   ?.Baz();
        """;

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(Normalize(expected), actual);
    }

    /// <summary>
    /// Verifies that an inner chain member is not double-processed.
    /// </summary>
    [TestMethod]
    public void InnerChainMemberIsNotProcessed()
    {
        // Arrange — b.Bar() inside argument is not a separate chain
        const string input = """
        var x = a
            .Foo(b.Bar())
            .Baz();
        """;

        const string expected = """
        var x = a.Foo(b.Bar())
                 .Baz();
        """;

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(Normalize(expected), actual);
    }

    /// <summary>
    /// Verifies that a property-only access without invocation is skipped (not counted as a chain link).
    /// </summary>
    [TestMethod]
    public void PropertyOnlyAccessIsSkipped()
    {
        // Arrange
        const string input = """
        var x = a.Prop;
        """;

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(Normalize(input), actual);
    }

    /// <summary>
    /// Verifies that a chain with the first link on a different line is collapsed and aligned.
    /// </summary>
    [TestMethod]
    public void ChainWithFirstLinkOnDifferentLineCollapsesAndAligns()
    {
        // Arrange
        const string input = """
        var x = a
            .Foo()
            .Bar()
            .Baz();
        """;

        const string expected = """
        var x = a.Foo()
                 .Bar()
                 .Baz();
        """;

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(Normalize(expected), actual);
    }

    /// <summary>
    /// Verifies that a chain not starting at column 0 collapses the first link and aligns the rest.
    /// </summary>
    [TestMethod]
    public void ChainStartingOnDifferentColumnCollapsesAndAligns()
    {
        // Arrange — chain indented, first dot on different line
        const string input = """
                var x = a
                    .Foo()
                              .Bar()
                    .Baz();
        """;

        const string expected = """
        var x = a.Foo()
                 .Bar()
                 .Baz();
        """;

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(Normalize(expected), actual);
    }

    /// <summary>
    /// Verifies that same-line links that precede different-line links are moved to new lines.
    /// </summary>
    [TestMethod]
    public void SameLineLinksBeforeDifferentLineAreMovedToNewLine()
    {
        // Arrange — .Bar() is on same line as .Foo() but .Baz() is on next line
        const string input = """
        var x = a.Foo().Bar()
            .Baz();
        """;

        const string expected = """
        var x = a.Foo()
                 .Bar()
                 .Baz();
        """;

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(Normalize(expected), actual);
    }

    /// <summary>
    /// Verifies that a chain with a null-forgiving operator (<c>!</c>) keeps the
    /// <c>!</c> aligned with the other chain dots.
    /// </summary>
    [TestMethod]
    public void ChainWithNullForgivingOperatorAlignsCorrectly()
    {
        // Arrange — expr.Initializer!.OpenBraceToken.GetLocation() with ! on continuation line
        const string input = """
        class C
        {
            void M()
            {
                var pos = objectCreationExpression.Initializer
                                                  !.OpenBraceToken
                                                  .GetLocation()
                                                  .GetLineSpan()
                                                  .StartLinePosition;
            }
        }
        """;

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(Normalize(input), actual);
    }

    /// <summary>
    /// Verifies that a chain continuation (<c>.Reverse()</c>) on a line that begins
    /// with a logical operator (<c>||</c>) is aligned correctly.
    /// </summary>
    [TestMethod]
    public void ChainOnLogicalOperatorContinuationLineRemainsAligned()
    {
        // Arrange — .Reverse() is aligned with .GetLeadingTrivia(), || is the first token on its line
        const string input = """
        namespace N
        {
            class C
            {
                void M(object syntaxNode)
                {
                    if (true)
                    {
                        var found = SearchTrivia(syntaxNode.GetTrailingTrivia()
                                                           .Reverse())
                                    || SearchTrivia(syntaxNode.GetLeadingTrivia()
                                                              .Reverse());
                    }
                }
                bool SearchTrivia(object t) => true;
            }
        }
        """;

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(Normalize(input), actual);
    }

    /// <summary>
    /// Verifies that a method chain with a statement lambda argument remains aligned
    /// and is not collapsed to block-indentation style.
    /// </summary>
    [TestMethod]
    public void ChainWithStatementLambdaArgumentRemainsAligned()
    {
        // Arrange
        const string input = """
        var result = source.Select(item =>
                                   {
                                       if (item > 0)
                                       {
                                           return item;
                                       }

                                       return 0;
                                   });
        """;

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(Normalize(input), actual);
    }

    /// <summary>
    /// Verifies current formatter behavior for method chains inside a migration-style initializer.
    /// </summary>
    [TestMethod]
    public void MethodChainInExampleMigrationIsFormattedAsExpected()
    {
        // Arrange
        const string input = """
        class AddEntityLevelsMigration
        {
            protected void Up(MigrationBuilder migrationBuilder)
            {
                migrationBuilder.CreateTable("EntityLevels",
                                             table => new
                                                      {
                                                          Id = table.Column<long>("bigint", nullable: false)
                              .Annotation("SqlServer:Identity", "1, 1"),
                                                          ParentEntityLevelId = table.Column<long>("bigint", nullable: false),
                                                          OptionalRoleId = table.Column<decimal>("decimal(20,0)", nullable: true)
                                                      },
                                             constraints: table =>
                                             {
                                                 table.PrimaryKey("PK_EntityLevels", x => x.Id);

                                                 table.ForeignKey("FK_EntityLevels_EntityLevels_ParentEntityLevelId",
                                                                  x => x.ParentEntityLevelId,
                                                                  "EntityLevels",
                                                                  "Id",
                                                                  onDelete: ReferentialAction.Restrict);
                                             });
            }
        }
        """;

        const string expected = """
        class AddEntityLevelsMigration
        {
            protected void Up(MigrationBuilder migrationBuilder)
            {
                migrationBuilder.CreateTable("EntityLevels",
                                             table => new
                                                      {
                                                          Id = table.Column<long>("bigint", nullable: false)
                                                                    .Annotation("SqlServer:Identity", "1, 1"),
                                                          ParentEntityLevelId = table.Column<long>("bigint", nullable: false),
                                                          OptionalRoleId = table.Column<decimal>("decimal(20,0)", nullable: true)
                                                      },
                                             constraints: table =>
                                                          {
                                                              table.PrimaryKey("PK_EntityLevels", x => x.Id);

                                                              table.ForeignKey("FK_EntityLevels_EntityLevels_ParentEntityLevelId",
                                                                               x => x.ParentEntityLevelId,
                                                                               "EntityLevels",
                                                                               "Id",
                                                                               onDelete: ReferentialAction.Restrict);
                                                          });
            }
        }
        """;

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(Normalize(expected), actual);
    }

    /// <summary>
    /// Verifies that the rule reports <see cref="FormattingPhase.Indentation"/>.
    /// </summary>
    [TestMethod]
    public void PhaseReturnsIndentation()
    {
        // Arrange
        var context = new FormattingContext("\n");
        var rule = new IndentationAndAlignmentRule(context, CancellationToken.None);

        // Act
        var phase = rule.Phase;

        // Assert
        Assert.AreEqual(FormattingPhase.Indentation, phase);
    }

    #endregion // Methods

    #region Helper

    /// <summary>
    /// Normalizes line endings in a string to LF.
    /// </summary>
    /// <param name="text">The text to normalize.</param>
    /// <returns>The text with LF line endings.</returns>
    private static string Normalize(string text)
    {
        return text.Replace("\r\n", "\n");
    }

    /// <summary>
    /// Applies the <see cref="IndentationAndAlignmentRule"/> to the given input.
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