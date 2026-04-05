using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Formatter.Test.Integration.Rules.Indentation;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Rules.Indentation.IndentationAndAlignmentRule"/> — method-chain alignment
/// </summary>
[TestClass]
public class MethodChainAlignmentIntegrationTests
{
    #region Constants

    private const string TestData = """
        internal class MethodChainAlignmentTestData
        {
            // --- Multi-line method chain with misaligned dots ---

            public void MultiLineChainMisaligned()
            {
                var result = new System.Collections.Generic.List<int> { 1, 2, 3 }
                        .Where(x => x > 0)
                            .Select(x => x * 2)
                        .ToList();
            }

            // --- Single-line chain (should stay unchanged) ---

            public void SingleLineChain()
            {
                var result = new System.Collections.Generic.List<int> { 1, 2, 3 }.Where(x => x > 0).ToList();
            }

            // --- Chain with conditional access (?.) ---

            public string ConditionalAccessChain(string input)
            {
                var result = input?
                        .Trim()
                            .ToUpper();

                return result;
            }

            // --- Short chain with only one link (should stay unchanged) ---

            public void ShortChain()
            {
                var result = "hello"
                    .ToUpper();
            }

            // --- Multi-line chain starting at various indentation levels ---

            public void ChainWithIndentation()
            {
                var result = System.Linq.Enumerable.Range(0, 10)
                                .Where(x => x > 2)
                        .Select(x => x.ToString())
                                    .ToList();
            }

            // --- Named arguments with method chain in anonymous object creation ---

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

    private const string ResultData = """
        internal class MethodChainAlignmentTestData
        {
            // --- Multi-line method chain with misaligned dots ---

            public void MultiLineChainMisaligned()
            {
                var result = new System.Collections.Generic.List<int> { 1, 2, 3 }.Where(x => x > 0)
                                                                                 .Select(x => x * 2)
                                                                                 .ToList();
            }

            // --- Single-line chain (should stay unchanged) ---

            public void SingleLineChain()
            {
                var result = new System.Collections.Generic.List<int> { 1, 2, 3 }.Where(x => x > 0).ToList();
            }

            // --- Chain with conditional access (?.) ---

            public string ConditionalAccessChain(string input)
            {
                var result = input?.Trim()
                                  .ToUpper();

                return result;
            }

            // --- Short chain with only one link (should stay unchanged) ---

            public void ShortChain()
            {
                var result = "hello".ToUpper();
            }

            // --- Multi-line chain starting at various indentation levels ---

            public void ChainWithIndentation()
            {
                var result = System.Linq.Enumerable.Range(0, 10)
                                                   .Where(x => x > 2)
                                                   .Select(x => x.ToString())
                                                   .ToList();
            }

            // --- Named arguments with method chain in anonymous object creation ---

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

    #endregion // Constants

    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test.
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that method chains are aligned correctly.
    /// </summary>
    [TestMethod]
    public void FormatsMethodChainAlignment()
    {
        // Arrange
        var input = TestData;
        var expected = ResultData;

        // Act
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var formattedTree = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationTokenSource.Token);
        var actual = formattedTree.GetRoot(TestContext.CancellationTokenSource.Token).ToFullString();

        // Assert
        Assert.AreEqual(expected, actual);
    }

    #endregion // Methods
}