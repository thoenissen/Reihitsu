using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.FullPipeline;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/> — method-chain alignment
/// </summary>
[TestClass]
public class MethodChainAlignmentFullPipelineTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// Input source used for method-chain-alignment formatting scenarios
    /// </summary>
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

    /// <summary>
    /// Expected formatter output for method-chain-alignment scenarios
    /// </summary>
    private const string ResultData = """
                                      internal class MethodChainAlignmentTestData
                                      {
                                          // --- Multi-line method chain with misaligned dots ---

                                          public void MultiLineChainMisaligned()
                                          {
                                              var result = new System.Collections.Generic.List<int>
                                                           {
                                                               1,
                                                               2,
                                                               3
                                                           }.Where(x => x > 0)
                                                            .Select(x => x * 2)
                                                            .ToList();
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

    #region Tests

    /// <summary>
    /// Verifies that method chains are aligned correctly under both LF and CRLF line endings
    /// </summary>
    [TestMethod]
    public void FormatsMethodChainAlignment()
    {
        AssertRuleResult(TestData, ResultData);
    }

    /// <summary>
    /// Verifies that a comment above the first wrapped fluent call keeps the chain unchanged
    /// </summary>
    [TestMethod]
    public void CommentAboveFirstWrappedCallKeepsChainUnchanged()
    {
        // Arrange
        const string input = """
                             internal sealed class Example
                             {
                                 private static object Create()
                                 {
                                     return new Builder()

                                         // Keep this step separate.
                                         .UseLogging()
                                         .UseValidation()
                                         .Build();
                                 }

                                 private sealed class Builder
                                 {
                                     public Builder UseLogging()
                                     {
                                         return this;
                                     }

                                     public Builder UseValidation()
                                     {
                                         return this;
                                     }

                                     public object Build()
                                     {
                                         return new object();
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                internal sealed class Example
                                {
                                    private static object Create()
                                    {
                                        return new Builder()

                                            // Keep this step separate.
                                            .UseLogging()
                                            .UseValidation()
                                            .Build();
                                    }

                                    private sealed class Builder
                                    {
                                        public Builder UseLogging()
                                        {
                                            return this;
                                        }

                                        public Builder UseValidation()
                                        {
                                            return this;
                                        }

                                        public object Build()
                                        {
                                            return new object();
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Tests
}