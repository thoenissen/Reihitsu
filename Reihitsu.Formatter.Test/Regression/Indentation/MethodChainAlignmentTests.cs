using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/> — method-chain alignment
/// </summary>
[TestClass]
public class MethodChainAlignmentTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a single-line method chain remains unchanged
    /// </summary>
    [TestMethod]
    public void SingleLineChainRemainsUnchanged()
    {
        // Arrange
        const string input = """
                             var x = a.Foo().Bar().Baz();
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a chain with only a single link on a different line is collapsed to the same line
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

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a multi-line method chain collapses the first link to the root line
    /// and aligns subsequent dots to the first dot's column
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

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that conditional access tokens (<c>?.</c>) in a mixed chain are collapsed and aligned correctly
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

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that in a wrapped chain a property between calls is a link of its own: <c>.Parent</c> after the first
    /// call starts its own line, and the conditional invocation behind it stays a continuation
    /// </summary>
    [TestMethod]
    public void PropertyAfterFirstCallStartsItsOwnLineBeforeConditionalContinuation()
    {
        // Arrange
        const string input = """
                             var x = root.FindToken(0).Parent
                                         ?.AncestorsAndSelf()
                                         .OfType<object>()
                                         .FirstOrDefault();
                             """;

        const string expected = """
                                var x = root.FindToken(0)
                                            .Parent
                                            ?.AncestorsAndSelf()
                                            .OfType<object>()
                                            .FirstOrDefault();
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a line break between a null-forgiving operator and the following dot moves in front of the
    /// <c>!</c>, so <c>!.</c> stays together and starts the continuation line under the first link
    /// </summary>
    [TestMethod]
    public void WrappedMemberAccessAfterNullForgivingOperatorMovesBreakInFrontOfIt()
    {
        // Arrange
        const string input = """
                             var result = value?.B()!
                             .C();
                             """;
        const string expected = """
                                var result = value?.B()
                                                  !.C();
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a null-forgiving link is wrapped together when a later chain link continues on another line
    /// </summary>
    [TestMethod]
    public void NullForgivingOperatorAndMemberAccessWrapTogether()
    {
        // Arrange
        const string input = """
                             var result = value.B()!
                             .C()
                             .D();
                             """;
        const string expected = """
                                var result = value.B()
                                                  !.C()
                                                  .D();
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that conditional-access and null-forgiving operators use the same chain-link alignment
    /// </summary>
    [TestMethod]
    public void ConditionalAccessAndNullForgivingOperatorsAlignAsChainLinks()
    {
        // Arrange
        const string input = """
                             var result = value.Trim()
                                                   ?.ToString()
                                                     !.Trim();
                             """;
        const string expected = """
                                var result = value.Trim()
                                                  ?.ToString()
                                                  !.Trim();
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an inner chain member is not double-processed
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

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a property-only access without invocation is skipped (not counted as a chain link)
    /// </summary>
    [TestMethod]
    public void PropertyOnlyAccessIsSkipped()
    {
        // Arrange
        const string input = """
                             var x = a.Prop;
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a property access split across lines is rejoined onto a single line
    /// when the chain has no invocation to keep it broken (formatter-level coverage of this case in
    /// isolation, apart from the full-pipeline test)
    /// </summary>
    [TestMethod]
    public void SplitPropertyAccessIsRejoined()
    {
        // Arrange
        const string input = """
                             var x = a
                                 .Prop;
                             """;

        const string expected = """
                                var x = a.Prop;
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a split null-conditional chain ending in a property access is rejoined
    /// onto a single line, including the dangling <c>?.</c>
    /// </summary>
    [TestMethod]
    public void ConditionalAccessChainWithTrailingPropertyIsRejoined()
    {
        // Arrange
        const string input = """
                             var x = a?.
                                 Foo(0, 0).
                                 Length;
                             """;

        const string expected = """
                                var x = a?.Foo(0, 0).Length;
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an oddly spaced null-conditional access (<c>? .Length</c>) is collapsed
    /// to <c>?.Length</c>
    /// </summary>
    [TestMethod]
    public void OddlySpacedConditionalAccessIsCollapsed()
    {
        // Arrange
        const string input = """
                             var x = a? .Length;
                             """;

        const string expected = """
                                var x = a?.Length;
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a chain with the first link on a different line is collapsed and aligned
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

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a chain with a comment directly above the first wrapped call keeps its links
    /// aligned under the chain root token
    /// </summary>
    [TestMethod]
    public void ChainWithCommentAboveFirstWrappedCallAlignsUnderChainRoot()
    {
        // Arrange
        const string input = """
                             var x = a

                             // Keep this step separate.
                             .Foo()
                             .Bar();
                             """;
        const string expected = """
                                var x = a

                                        // Keep this step separate.
                                        .Foo()
                                        .Bar();
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a chain not starting at column 0 collapses the first link and aligns the rest
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

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that same-line links that precede different-line links are moved to new lines
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

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a chain with a null-forgiving operator (<c>!</c>) keeps the
    /// <c>!</c> aligned with the other chain dots
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

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a chain continuation (<c>.Reverse()</c>) on a line that begins
    /// with a logical operator (<c>||</c>) is aligned correctly
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

                                     bool SearchTrivia(object t)
                                     {
                                         return true;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a method chain with a statement lambda argument remains aligned
    /// and is not collapsed to block-indentation style
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

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a statement lambda used as the second invocation argument
    /// aligns its block with the lambda argument start when the block starts on
    /// the line after <c>=&gt;</c>
    /// </summary>
    [TestMethod]
    public void StatementLambdaAsWrappedSecondArgumentAlignsBlockToLambdaStart()
    {
        // Arrange
        const string input = """
                             var response = manager.Apply(entry => entry.Key == currentKey, entry =>
                                                                                           {
                                                                                               entry.State = nextState;
                                                                                               entry.Payload = nextPayload;
                                                                                           });
                             """;

        const string expected = """
                                var response = manager.Apply(entry => entry.Key == currentKey,
                                                             entry =>
                                                             {
                                                                 entry.State = nextState;
                                                                 entry.Payload = nextPayload;
                                                             });
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a call with a multiline predicate first argument and a wrapped
    /// statement-lambda second argument preserves indentation for both the logical
    /// continuation line and the lambda block
    /// </summary>
    [TestMethod]
    public void RefreshCallWithMultilinePredicateKeepsAndSecondLambdaIndentation()
    {
        // Arrange
        const string input = """
                             class Handler
                             {
                                 void Process(bool allowArchive, bool allowAudit)
                                 {
                                     if (_nodeFactory.GetNodeStore<RecordStore>()
                                                           .Update(item => snapshots.Any(match => match.OwnerId == item.Id
                                                                                                  && match.EntryId == SessionState.Actor.Id),
                                                                   item =>
                                                                   {
                                                                       item.IsArchiveAllowed = allowArchive;
                                                                       item.IsAuditAllowed = allowAudit;
                                                                   }) == false)
                                     {
                                         throw _nodeFactory.LastIssue;
                                     }
                                 }
                             }
                             """;

        const string expected = """
                                class Handler
                                {
                                    void Process(bool allowArchive, bool allowAudit)
                                    {
                                        if (_nodeFactory.GetNodeStore<RecordStore>()
                                                        .Update(item => snapshots.Any(match => match.OwnerId == item.Id
                                                                                               && match.EntryId == SessionState.Actor.Id),
                                                                item =>
                                                                {
                                                                    item.IsArchiveAllowed = allowArchive;
                                                                    item.IsAuditAllowed = allowAudit;
                                                                }) == false)
                                        {
                                            throw _nodeFactory.LastIssue;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that deeply nested invocation/lambda structures keep multiline predicate
    /// continuation and wrapped second-lambda indentation stable
    /// </summary>
    [TestMethod]
    public void DeeplyNestedResponseWithMultilinePredicateAndWrappedLambdaKeepsIndentation()
    {
        // Arrange
        const string input = """
                             class Scenario
                             {
                                 object Build()
                                 {
                                     return new[]
                                     {
                                         new ActionNode
                                         {
                                             Execute = async value =>
                                                       {
                                                           if (_serviceProvider.GetService<UserLedger>()
                                                                               .Refresh(current => records.Any(item => item.SessionId == current.Id
                                                                                                                        && item.ActorId == Context.User.Id),
                                                                                        current =>
                                                                                        {
                                                                                            current.IsEnabled = true;

                                                                                            if (value > 0)
                                                                                            {
                                                                                                current.IsPrimary = false;
                                                                                            }
                                                                                        }) == false)
                                                                   {
                                                                       throw _serviceProvider.LastProblem;
                                                                   }

                                                                   return true;
                                                               }
                                         }
                                     };
                                 }
                             }
                             """;

        const string expected = """
                                class Scenario
                                {
                                    object Build()
                                    {
                                        return new[]
                                               {
                                                   new ActionNode
                                                   {
                                                       Execute = async value =>
                                                                 {
                                                                     if (_serviceProvider.GetService<UserLedger>()
                                                                                         .Refresh(current => records.Any(item => item.SessionId == current.Id
                                                                                                                                 && item.ActorId == Context.User.Id),
                                                                                                  current =>
                                                                                                  {
                                                                                                      current.IsEnabled = true;

                                                                                                      if (value > 0)
                                                                                                      {
                                                                                                          current.IsPrimary = false;
                                                                                                      }
                                                                                                  }) == false)
                                                                     {
                                                                         throw _serviceProvider.LastProblem;
                                                                     }

                                                                     return true;
                                                                 }
                                                   }
                                               };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a method chain in the true branch of a ternary expression is collapsed and aligned
    /// </summary>
    [TestMethod]
    public void ChainInTernaryTrueBranchCollapsesAndAligns()
    {
        // Arrange
        const string input = """
                             var result = condition
                                 ? inputValue
                                     .Trim()
                                             .ToUpperInvariant()
                                 : fallback;
                             """;

        const string expected = """
                                var result = condition
                                                 ? inputValue.Trim()
                                                             .ToUpperInvariant()
                                                 : fallback;
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a method chain in the false branch of a ternary expression is collapsed and aligned
    /// </summary>
    [TestMethod]
    public void ChainInTernaryFalseBranchCollapsesAndAligns()
    {
        // Arrange
        const string input = """
                             var result = condition
                                 ? fallback
                                 : inputValue
                                     .Trim()
                                             .ToUpperInvariant();
                             """;

        const string expected = """
                                var result = condition
                                                 ? fallback
                                                 : inputValue.Trim()
                                                             .ToUpperInvariant();
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies current formatter behavior for method chains inside a migration-style initializer
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

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies formatter behavior for migration-style named arguments with an anonymous-object method chain
    /// </summary>
    [TestMethod]
    public void MethodChainInNamedMigrationArgumentsIsFormattedAsExpected()
    {
        // Arrange
        const string input = """
                             class AddEntityLevelsMigration
                             {
                                 protected override void Up(MigrationBuilder migrationBuilder)
                                 {
                                     migrationBuilder.CreateTable(
                                         name: "EntityLevels",
                                         columns: table => new
                                                           {
                                                               Id = table.Column<int>(type: "int", nullable: false)
                                                                         .Annotation("SqlServer:Identity", "1, 1"),
                                                               Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                                                               Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                                                               CreationUserId = table.Column<long>(type: "bigint", nullable: false),
                                                               ChannelId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                                                               MessageId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                                                               ThreadId = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                                                           },
                                         constraints: table =>
                                         {
                                             table.PrimaryKey("PK_EntityLevels", x => x.Id);
                                             table.ForeignKey(name: "FK_EntityLevels_Users_CreationUserId",
                                                              column: x => x.CreationUserId,
                                                              principalTable: "Users",
                                                              principalColumn: "Id",
                                                              onDelete: ReferentialAction.Restrict);
                                         });
                                 }
                             }
                             """;

        const string expected = """
                                class AddEntityLevelsMigration
                                {
                                    protected override void Up(MigrationBuilder migrationBuilder)
                                    {
                                        migrationBuilder.CreateTable(name: "EntityLevels",
                                                                     columns: table => new
                                                                                       {
                                                                                           Id = table.Column<int>(type: "int", nullable: false)
                                                                                                     .Annotation("SqlServer:Identity", "1, 1"),
                                                                                           Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                                                                                           Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                                                                                           CreationUserId = table.Column<long>(type: "bigint", nullable: false),
                                                                                           ChannelId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                                                                                           MessageId = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                                                                                           ThreadId = table.Column<decimal>(type: "decimal(20,0)", nullable: false)
                                                                                       },
                                                                     constraints: table =>
                                                                                  {
                                                                                      table.PrimaryKey("PK_EntityLevels", x => x.Id);
                                                                                      table.ForeignKey(name: "FK_EntityLevels_Users_CreationUserId",
                                                                                                       column: x => x.CreationUserId,
                                                                                                       principalTable: "Users",
                                                                                                       principalColumn: "Id",
                                                                                                       onDelete: ReferentialAction.Restrict);
                                                                                  });
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a complex LINQ chain in a <c>foreach</c> declaration remains unchanged
    /// </summary>
    [TestMethod]
    public void MethodChainInForeachDeclarationRemainsUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(object provider, object endpoints, object plans, object tenant, object nowTicks)
                                 {
                                     foreach (var entry in provider.Resolve<BucketStore>()
                                                                   .Items()
                                                                   .Where(obj => tenant == null
                                                                                 || obj.ScopeId == tenant)
                                                                   .Select(obj => new
                                                                                  {
                                                                                      Primary = endpoints.Where(obj2 => obj2.BucketId == obj.Id
                                                                                                                        && obj2.Kind == EndpointKind.Notifier)
                                                                                                         .Select(obj2 => new
                                                                                                                         {
                                                                                                                             TargetId = obj2.ExternalId,
                                                                                                                             Token = obj2.Reference
                                                                                                                         })
                                                                                                         .FirstOrDefault(),
                                                                                      Timeline = plans.Where(obj2 => obj2.ScopeId == obj.ScopeId)
                                                                                                      .SelectMany(obj2 => obj2.Segments
                                                                                                                              .Where(obj3 => obj3.StartsAt > nowTicks
                                                                                                                                             && obj2.Segments.Any(obj4 => obj4.StartsAt > nowTicks
                                                                                                                                                                          && obj4.StartsAt < obj3.StartsAt) == false)
                                                                                                                              .Select(obj3 => new
                                                                                                                                              {
                                                                                                                                                  obj3.StartsAt,
                                                                                                                                                  obj2.Label
                                                                                                                                              }))
                                                                                                      .OrderBy(obj2 => obj2.StartsAt)
                                                                                                      .ToList()
                                                                                  })
                                                                   .Where(obj => obj.Primary.TargetId > 0)
                                                                   .ToList())
                                     {
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a single named argument in a method invocation is collapsed
    /// to the same line as the method call
    /// </summary>
    [TestMethod]
    public void SingleNamedArgumentInInvocationCollapsesToSameLine()
    {
        // Arrange
        const string input = """
                             class Migration
                             {
                                 protected void Down(object builder)
                                 {
                                     builder.Drop(
                                         name: "Name");
                                 }
                             }
                             """;

        const string expected = """
                                class Migration
                                {
                                    protected void Down(object builder)
                                    {
                                        builder.Drop(name: "Name");
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that method chains inside switch expression arms maintain consistent alignment
    /// relative to the first invoked method on the same line
    /// </summary>
    [TestMethod]
    public void MethodChainInSwitchExpressionArmAlignsCorrectly()
    {
        // Arrange
        const string input = """
                             class Sample
                             {
                                 public object Process(IQueryable<Item> items, Category category, DateTime cutoff)
                                 {
                                     return category switch
                                     {
                                     Category.Recent => items.Where(x => x.Date > cutoff)
                                     .OrderBy(x => x.Date)
                                     .Select(x => x.Name)
                                     .ToList(),
                                     _ => items.ToList()
                                     };
                                 }
                             }
                             """;

        const string expected = """
                                class Sample
                                {
                                    public object Process(IQueryable<Item> items, Category category, DateTime cutoff)
                                    {
                                        return category switch
                                               {
                                                   Category.Recent => items.Where(x => x.Date > cutoff)
                                                                           .OrderBy(x => x.Date)
                                                                           .Select(x => x.Name)
                                                                           .ToList(),
                                                   _ => items.ToList()
                                               };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that nested statement-lambda content in an object initializer preserves
    /// method-chain and logical-chain alignment
    /// </summary>
    [TestMethod]
    public void NestedInitializerStatementLambdaPreservesMethodAndLogicalChainAlignment()
    {
        // Arrange
        const string input = """
                             class WorkflowMenuBuilder
                             {
                                 void Build()
                                 {
                                     entries.Add(new WorkflowEntry<bool>
                                                 {
                                                     Operation = async () =>
                                                                 {
                                                                     if (_storageFactory.GetRepository<AuditEventRepository>()
                                                                                        .DeleteRange(record => record.UserId == 1
                                                                                                               && record.Name == "alpha")
                                                                         && _storageFactory.GetRepository<AccountRepository>()
                                                                                           .Delete(record => record.UserId == 1
                                                                                                             && record.Name == "alpha"))
                                                                     {
                                                                     }

                                                                     return true;
                                                                 }
                                                 });
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a comment-exempt chain nested in a lambda argument of a conditional access
    /// chain aligns its links under the chain root instead of the enclosing block
    /// </summary>
    [TestMethod]
    public void CommentExemptChainInsideLambdaArgumentAlignsUnderChainRoot()
    {
        // Arrange
        const string input = """
                             public class Class
                             {
                                 public void M()
                                 {
                                     var value = a.List.Find(e => d

                                                                  // Keep this call separate.
                                                                  .Equals(e.Number))
                                                       ?.Value;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a misaligned chain continuation inside a lambda argument is aligned to the
    /// chain anchor when the outer chain is a conditional access expression
    /// </summary>
    [TestMethod]
    public void MisalignedChainInsideLambdaArgumentOfConditionalAccessIsAligned()
    {
        // Arrange
        const string input = """
                             public class Class
                             {
                                 public void M()
                                 {
                                     var value = a.List.Find(e => d.Equals(e.Number.ToString()
                                         .Trim(),
                                                                           StringComparison.OrdinalIgnoreCase))
                                                       ?.Value;
                                 }
                             }
                             """;

        const string expected = """
                                public class Class
                                {
                                    public void M()
                                    {
                                        var value = a.List.Find(e => d.Equals(e.Number.ToString()
                                                                                      .Trim(),
                                                                              StringComparison.OrdinalIgnoreCase))
                                                          ?.Value;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped chain continuation inside a plain argument keeps its alignment
    /// when the outer chain is a conditional access expression
    /// </summary>
    [TestMethod]
    public void ChainInsideArgumentOfConditionalAccessKeepsAlignment()
    {
        // Arrange
        const string input = """
                             public class Class
                             {
                                 public void M()
                                 {
                                     var value = a.List.Find(d.Equals(e.Number.ToString()
                                                                              .Trim()))
                                                       ?.Value;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a wrapped chain continuation inside a lambda argument keeps its alignment
    /// when the outer chain is a conditional access expression
    /// </summary>
    [TestMethod]
    public void ChainInsideLambdaArgumentOfConditionalAccessKeepsAlignment()
    {
        // Arrange
        const string input = """
                             public class Class
                             {
                                 public void M()
                                 {
                                     var a = new A();
                                     var d = string.Empty;
                                     var value = a.List.Find(e => d.Equals(e.Number.ToString()
                                                                                   .Trim(),
                                                                           StringComparison.OrdinalIgnoreCase))
                                                       ?.Value;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a wrapped chain whose first invoked link is introduced by <c>?.</c> and is
    /// preceded by a plain (non-invoked) property access aligns continuation dots to that
    /// <c>?.</c>-invoked link
    /// </summary>
    [TestMethod]
    public void ConditionalAccessAfterPlainPropertyAccessAlignsToInvokedLink()
    {
        // Arrange
        const string input = """
                             using System.Collections.Generic;
                             using System.Linq;

                             internal sealed class Example
                             {
                                 internal sealed class Choice
                                 {
                                     public string Name { get; set; }
                                 }

                                 internal sealed class Option
                                 {
                                     public string Name { get; set; }
                                 }

                                 internal sealed class Request
                                 {
                                     public IEnumerable Choices { get; set; }
                                 }

                                 internal static List Convert(Request request)
                                 {
                                     var options = request.Choices?.Select(choice => new Option
                                     {
                                         Name = choice.Name
                                     })
                                                           .ToList();

                                     return options;
                                 }
                             }
                             """;

        const string expected = """
                                using System.Collections.Generic;
                                using System.Linq;

                                internal sealed class Example
                                {
                                    internal sealed class Choice
                                    {
                                        public string Name { get; set; }
                                    }

                                    internal sealed class Option
                                    {
                                        public string Name { get; set; }
                                    }

                                    internal sealed class Request
                                    {
                                        public IEnumerable Choices { get; set; }
                                    }

                                    internal static List Convert(Request request)
                                    {
                                        var options = request.Choices?.Select(choice => new Option
                                                                                        {
                                                                                            Name = choice.Name
                                                                                        })
                                                                     .ToList();

                                        return options;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped chain whose first invoked link is introduced by <c>!.</c> and is
    /// preceded by a plain (non-invoked) property access aligns continuation dots to that
    /// <c>!.</c>-invoked link
    /// </summary>
    [TestMethod]
    public void NullForgivingAfterPlainPropertyAccessAlignsToInvokedLink()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var options = request.Choices!.Select(choice => new Option
                                     {
                                         Name = choice.Name
                                     })
                                                           .ToList();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var options = request.Choices!.Select(choice => new Option
                                                                                        {
                                                                                            Name = choice.Name
                                                                                        })
                                                                     .ToList();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a null-forgiving operator on a non-invoked member access is not treated as a
    /// chain link: the anchor stays the first plain invoked dot
    /// </summary>
    [TestMethod]
    public void NonInvokedNullForgivingIsNotTheChainAnchor()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var options = request!.Choices.Select(choice => new Option
                                                                                     {
                                                                                         Name = choice.Name
                                                                                     })
                                                                   .ToList();
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a wrapped chain whose first invoked link is introduced by <c>?.</c> aligns to
    /// that link even when two plain (non-invoked) property accesses precede it
    /// </summary>
    [TestMethod]
    public void DeepPlainPrefixBeforeConditionalAccessAlignsToInvokedLink()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = a.B.C?.Select(item => new Wrapper
                                     {
                                         Value = item
                                     })
                                               .ToList();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = a.B.C?.Select(item => new Wrapper
                                                                      {
                                                                          Value = item
                                                                      })
                                                     .ToList();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that when a wrapped chain has two <c>?.</c>-invoked links, continuation dots align
    /// to the first one rather than to a plain (non-invoked) property access before it
    /// </summary>
    [TestMethod]
    public void MultipleConditionalAccessLinksAlignToFirstLink()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = a.Prop?.Select(item => new Wrapper
                                     {
                                         Value = item
                                     })
                                              ?.Where(item => item != null)
                                               .ToList();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = a.Prop?.Select(item => new Wrapper
                                                                       {
                                                                           Value = item
                                                                       })
                                                      ?.Where(item => item != null)
                                                      .ToList();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a non-invoked trailing property access at the end of a wrapped chain aligns to
    /// the same <c>?.</c>-invoked link as the rest of the chain
    /// </summary>
    [TestMethod]
    public void TrailingPropertyAfterConditionalAccessChainAlignsToInvokedLink()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var count = a.Choices?.Select(item => new Wrapper
                                     {
                                         Value = item
                                     })
                                                   .ToList()
                                                   .Count;
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var count = a.Choices?.Select(item => new Wrapper
                                                                              {
                                                                                  Value = item
                                                                              })
                                                             .ToList()
                                                             .Count;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a <c>?.</c> link directly after a collection initializer's closing brace keeps
    /// anchoring correctly even with a non-invoked prefix dot between them
    /// </summary>
    [TestMethod]
    public void ConditionalAccessDirectlyAfterInitializerCloseBraceKeepsAnchor()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = new List<string>
                                             {
                                                 "a",
                                                 "b"
                                             }?.Select(item => new Wrapper
                                                               {
                                                                   Value = item
                                                               })
                                              .ToList();
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a <c>?.</c> link aligns correctly when it is separated from an initializer's
    /// closing brace by a plain (non-invoked) property access, so the anchor's column is still
    /// derived from the creation expression's <c>new</c> keyword rather than left unadjusted
    /// </summary>
    [TestMethod]
    public void ConditionalAccessAfterInitializerCloseBraceAndPropertyAlignsToInvokedLink()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = new Request { Choices = data }.Choices?.Select(item => new Wrapper
                                     {
                                         Value = item
                                     })
                                                                            .ToList();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = new Request
                                                {
                                                    Choices = data
                                                }.Choices?.Select(item => new Wrapper
                                                                          {
                                                                              Value = item
                                                                          })
                                                         .ToList();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a chain whose own first dot is wrapped onto its own line collapses that dot back
    /// onto the chain root, so the remaining continuation dots align to the first invoked link's
    /// column — the column RH5201 computes — instead of to block indentation. The dot is a plain,
    /// non-invoked property access, which the line-break phase's invoked-link-only view never
    /// considered
    /// </summary>
    [TestMethod]
    public void WrappedFirstChainDotCollapsesOntoChainRoot()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = a
                                     .Prop?.Call()
                                     .Then();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = a.Prop?.Call()
                                                      .Then();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a block comment between a plain (non-invoked) property access and its following
    /// <c>?.</c>-invoked link does not shift the anchor: continuation dots still align to the link's
    /// own column, which sits to the right of the comment
    /// </summary>
    [TestMethod]
    public void BlockCommentBeforeConditionalAccessLinkDoesNotShiftAnchor()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = a.Prop /* keep */ ?.Select(item => new Wrapper
                                     {
                                         Value = item
                                     })
                                               .ToList();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = a.Prop /* keep */?.Select(item => new Wrapper
                                                                                  {
                                                                                      Value = item
                                                                                  })
                                                                 .ToList();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a member-access dot immediately followed by a line break rejoins its own member
    /// name, so the name is no longer left orphaned on a continuation line at block indentation. The
    /// break lives in the dot's trailing trivia, which no chain predicate inspected. The rejoined
    /// result is byte-identical to the expected output of
    /// <see cref="ConditionalAccessAfterInitializerCloseBraceAndPropertyAlignsToInvokedLink"/>, whose
    /// input writes the same chain with its initializer on one line
    /// </summary>
    [TestMethod]
    public void TrailingDotAfterInitializerCloseBraceRejoinsItsMemberName()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = new Request
                                             {
                                                 Choices = data
                                             }.
                                                 Choices?.Select(item => new Wrapper
                                                                         {
                                                                             Value = item
                                                                         })
                                                        .ToList();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = new Request
                                                {
                                                    Choices = data
                                                }.Choices?.Select(item => new Wrapper
                                                                          {
                                                                              Value = item
                                                                          })
                                                         .ToList();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the initializer-adjacent anchor correction does not mix columns from two
    /// different source lines when the chain's first collected dot sits on an initializer's closing
    /// brace line but the anchor link itself wraps onto a later line; the correction must only apply
    /// when the anchor shares the first collected dot's own line, or a second formatter pass changes
    /// the result.
    /// <para>
    /// A comment between the dot and its member name is what keeps the two on separate lines here:
    /// it blocks the rejoin that <see cref="TrailingDotAfterInitializerCloseBraceRejoinsItsMemberName"/>
    /// performs, which is the only remaining way to reach the correction's unequal-line branch. The
    /// blank line and the comment's own indentation in the expected output are pre-existing behavior
    /// of the surrounding phases and are not what this test guards
    /// </para>
    /// </summary>
    [TestMethod]
    public void ConditionalAccessAnchorOnLaterLineThanInitializerCloseBraceStaysIdempotent()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = new Request
                                             {
                                                 Choices = data
                                             }.
                                                 // keep
                                                 Choices?.Select(item => new Wrapper
                                                                         {
                                                                             Value = item
                                                                         })
                                                        .ToList();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = new Request
                                                {
                                                    Choices = data
                                                }.

                                        // keep
                                        Choices?.Select(item => new Wrapper
                                                                {
                                                                    Value = item
                                                                })
                                               .ToList();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the wrapped-first-dot collapse does not depend on a conditional-access or
    /// null-forgiving operator: a chain of plain dots diverges and converges identically
    /// </summary>
    [TestMethod]
    public void WrappedFirstChainDotWithPlainDotsCollapsesOntoChainRoot()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = a
                                         .Prop.ToString()
                                         .Trim();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = a.Prop.ToString()
                                                      .Trim();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a null-forgiving operator introducing the first invoked link does not exempt the
    /// chain from the wrapped-first-dot collapse
    /// </summary>
    [TestMethod]
    public void WrappedFirstChainDotBeforeNullForgivingLinkCollapsesOntoChainRoot()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = a
                                         .Prop!.ToString()
                                         .Trim();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = a.Prop!.ToString()
                                                      .Trim();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a chain whose only user wrap is in front of its first link counts as wrapped: the first link joins
    /// the root, the first call stays on the root line, and the later call starts its own line under the first call
    /// </summary>
    [TestMethod]
    public void WrappedFirstChainDotCountsAsWrapForLaterCall()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = a
                                         .Prop?.ToString().Trim();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = a.Prop?.ToString()
                                                      .Trim();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the wrapped-first-dot collapse applies inside an argument, so the enclosing
    /// formatting scope is not a discriminator
    /// </summary>
    [TestMethod]
    public void WrappedFirstChainDotInsideArgumentCollapsesOntoChainRoot()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(object a)
                                 {
                                     Handle(a
                                         .Prop?.ToString()
                                         .Trim());
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M(object a)
                                    {
                                        Handle(a.Prop?.ToString()
                                                     .Trim());
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the other side of the collapse boundary: when the chain's own first dot already sits
    /// on the root line, the first-link collapse never reaches past it to the wrapped link behind it.
    /// That link follows a member access, so it keeps its own line and aligns under the first dot,
    /// exactly like the same chain written with a plain <c>.</c>
    /// </summary>
    [TestMethod]
    public void UnwrappedFirstChainDotKeepsTheWrappedConditionalLinkBehindItLikePlainChain()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = a.Prop
                                         ?.ToString()
                                         .Trim();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = a.Prop
                                                 ?.ToString()
                                                 .Trim();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a chain whose first link already sits on the root line keeps the user's wrap in front of its first
    /// call: the root ends in a property access, so only the first link is joined and the wrapped call stays on its own line
    /// </summary>
    [TestMethod]
    public void FluentChainWithUnwrappedFirstDotStaysWrapped()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = a.Prop
                                              .Foo()
                                              .Bar();
                                     var y = a.Prop
                                              .Select(item => item);
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a chain with exactly one invoked link collapses its own wrapped first dot too,
    /// so the same chain formats identically however the author placed the break. The wrapped fluent
    /// link keeps its line and aligns to the chain's reference column
    /// </summary>
    [TestMethod]
    public void WrappedFirstChainDotWithSingleInvokedLinkCollapsesOntoChainRoot()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = a
                                         .Prop
                                         .Select(item => item);
                                     var a1 = a
                                         .Prop.Call();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = a.Prop
                                                 .Select(item => item);
                                        var a1 = a.Prop.Call();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a null-forgiving operator in front of the wrapped first dot belongs to that link (<c>!.Prop</c>), so
    /// the link is joined onto the root line as a whole and the later links align under its <c>!</c>
    /// </summary>
    [TestMethod]
    public void WrappedFirstChainDotAfterNullForgivingRootCollapsesOntoChainRoot()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = a!
                                         .Prop
                                         .Foo()
                                         .Bar();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = a!.Prop
                                                 .Foo()
                                                 .Bar();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment above the chain's first link refuses only that join: the chain stays wrapped at the
    /// commented link, and every later link still starts its own line in the root's column, so the output remains
    /// RH5201-clean
    /// </summary>
    [TestMethod]
    public void CommentAboveFirstLinkStillBreaksContinuationLinks()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = a
                                             // keep this chain wrapped
                                             .Prop
                                             .Foo()
                                             .Bar().Baz();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = a

                                                // keep this chain wrapped
                                                .Prop
                                                .Foo()
                                                .Bar()
                                                .Baz();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a chain rooted in a single-line implicit array initializer aligns its
    /// continuation dot to the first invoked link. The closing brace shares its line with the
    /// initializer's elements, so rebasing the anchor onto the <c>new</c> keyword would shift it left
    /// by the initializer's printed width
    /// </summary>
    [TestMethod]
    public void ChainRootedInSingleLineImplicitArrayInitializerAlignsToInvokedLink()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = new[] { 1, 2, 3 }.Select(item => item)
                                        .ToList();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = new[] { 1, 2, 3 }.Select(item => item)
                                                                 .ToList();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the same chain rooted in an explicit single-line array initializer aligns
    /// identically, so the defect is not specific to the implicit-array arm
    /// </summary>
    [TestMethod]
    public void ChainRootedInSingleLineExplicitArrayInitializerAlignsToInvokedLink()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = new int[] { 1, 2, 3 }.Select(item => item)
                                        .ToList();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = new int[] { 1, 2, 3 }.Select(item => item)
                                                                     .ToList();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the other side of the initializer-correction boundary: an initializer whose closing
    /// brace starts its own line still has its chain anchor rebased onto the creation expression's
    /// <c>new</c> keyword, which is the arm the correction exists for
    /// </summary>
    [TestMethod]
    public void ChainRootedInInitializerWithCloseBraceFirstOnLineKeepsNewKeywordRebase()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = new Wrapper
                                     {
                                         Value = 1
                                     }.Select(item => item)
                                        .ToList();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = new Wrapper
                                                {
                                                    Value = 1
                                                }.Select(item => item)
                                                 .ToList();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a chain whose first dot wraps after a single-line array initializer both
    /// collapses that dot and aligns the remaining continuation dot — the shape where the collapse and
    /// the alignment corrections meet on one input
    /// </summary>
    [TestMethod]
    public void WrappedFirstChainDotAfterSingleLineArrayInitializerCollapsesAndAligns()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = new[] { 1, 2, 3 }
                                         .Select(item => item)
                                         .ToList();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = new[] { 1, 2, 3 }.Select(item => item)
                                                                 .ToList();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that every continuation dot of a three-link chain rooted in a single-line array
    /// initializer lands on the same reference column
    /// </summary>
    [TestMethod]
    public void ThreeLinkChainRootedInSingleLineArrayInitializerAlignsEveryContinuationDot()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = new[] { 1, 2, 3 }.Where(item => item > 0)
                                        .Select(item => item)
                                        .ToList();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = new[] { 1, 2, 3 }.Where(item => item > 0)
                                                                 .Select(item => item)
                                                                 .ToList();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a chain whose root ends in a token other than an initializer's closing brace is
    /// untouched by the initializer correction
    /// </summary>
    [TestMethod]
    public void ChainRootedInElementAccessIsUnaffectedByInitializerCorrection()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = arr[0].Foo()
                                        .Bar();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = arr[0].Foo()
                                                      .Bar();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verify that an argument list nested under a chain continuation line is aligned against the
    /// chain's final column rather than the column the chain held before its anchor was resolved
    /// </summary>
    [TestMethod]
    public void ArgumentListUnderChainContinuationLineFollowsTheResolvedChainColumn()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = new int[] { 1,
                                         2 }.Select(item => item)
                                            .Where(alpha,
                                                beta)
                                            .ToList();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = new int[] { 1,
                                                    2 }.Select(item => item)
                                                       .Where(alpha,
                                                              beta)
                                                       .ToList();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verify that an argument list nested under the continuation line of a chain rooted in a
    /// parenthesized <c>with</c> expression follows the chain's resolved column
    /// </summary>
    [TestMethod]
    public void ArgumentListUnderWithExpressionChainContinuationFollowsTheResolvedChainColumn()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(Record r)
                                 {
                                     var x = (r with
                                     {
                                         Value = 1
                                     }).Select(item => item)
                                        .Where(alpha,
                                            beta)
                                        .ToList();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M(Record r)
                                    {
                                        var x = (r with
                                                   {
                                                       Value = 1
                                                   }).Select(item => item)
                                                     .Where(alpha,
                                                            beta)
                                                     .ToList();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verify that a statement lambda nested under a chain continuation line follows the chain's
    /// resolved column
    /// </summary>
    [TestMethod]
    public void StatementLambdaUnderChainContinuationLineFollowsTheResolvedChainColumn()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = new int[] { 1,
                                         2 }.Select(item =>
                                             {
                                                 return item;
                                             })
                                            .ToList();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = new int[] { 1,
                                                    2 }.Select(item =>
                                                               {
                                                                   return item;
                                                               })
                                                       .ToList();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verify that a ternary nested under a chain continuation line follows the chain's resolved
    /// column
    /// </summary>
    [TestMethod]
    public void TernaryUnderChainContinuationLineFollowsTheResolvedChainColumn()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = new int[] { 1,
                                         2 }.Select(flag
                                                 ? one
                                                 : two)
                                            .ToList();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = new int[] { 1,
                                                    2 }.Select(flag
                                                                   ? one
                                                                   : two)
                                                       .ToList();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verify that a directive above the first invoked link leaves the collapse of a wrapped prefix
    /// dot enabled, unlike the comment arm. The line-break phase's exemption is deliberately
    /// comment-only; this test pins the directive side of that decision so it cannot drift silently
    /// </summary>
    [TestMethod]
    public void DirectiveAboveFirstInvokedLinkStillCollapsesWrappedPrefixDot()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = a
                                         .Prop
                             #pragma warning disable 1234
                                         .Foo()
                                         .Bar();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = a.Prop
                                #pragma warning disable 1234
                                                 .Foo()
                                                 .Bar();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verify that a chain rooted in a multi-line array initializer whose closing brace shares its
    /// line with an element aligns its continuation dot to the first invoked link. The initializer
    /// correction does not apply here (the brace is not first on its line), so the anchor falls back
    /// to the adjusted-column lookup — which is resolved after the initializer has claimed its own
    /// line, rather than against that line's block indentation.
    /// <para>
    /// The initializer's own layout is deliberately left untouched: only the continuation dot moves
    /// </para>
    /// </summary>
    [TestMethod]
    public void MultiLineArrayInitializerWithTrailingCloseBraceAlignsToInvokedLink()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = new int[] { 1,
                                         2 }.Select(item => item)
                                        .ToList();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = new int[] { 1,
                                                    2 }.Select(item => item)
                                                       .ToList();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a null-conditional chain whose continuation dots are already aligned to the
    /// first invoked link (<c>?.FirstOrDefault()</c>) stays unchanged, rather than having its
    /// continuation indentation reduced
    /// </summary>
    [TestMethod]
    public void NullConditionalChainAlreadyAlignedToFirstInvokedLinkStaysUnchanged()
    {
        // Arrange
        const string input = """
                             var filePath = metadata.Media?.FirstOrDefault()
                                                          ?.Part
                                                          ?.FirstOrDefault()
                                                          ?.File;
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a comment above the chain's own first dot keeps the chain wrapped, and every remaining invoked link
    /// still starts its own line — the arrangement <c>RH5201MethodChainsShouldBeAlignedAnalyzer</c> requires
    /// </summary>
    [TestMethod]
    public void CommentAboveChainRootKeepsEveryInvokedLinkOnItsOwnLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = a
                                         // keep wrapped
                                         .Foo()
                                         .Bar().Baz();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = a

                                                // keep wrapped
                                                .Foo()
                                                .Bar()
                                                .Baz();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a chain whose first link a comment keeps wrapped puts every link on its own line at the root column,
    /// also the first invoked link that shared its line with the prefix
    /// </summary>
    [TestMethod]
    public void CommentExemptChainWithPrefixSharingFirstInvokedLinkLineStartsEveryLinkAtRootColumn()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = a
                                         // keep wrapped
                                         .Prop.Foo()
                                         .Bar().Baz();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = a

                                                // keep wrapped
                                                .Prop
                                                .Foo()
                                                .Bar()
                                                .Baz();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a chain whose first link a comment keeps wrapped puts every link on its own line at the root column,
    /// including a trailing non-invoked property
    /// </summary>
    [TestMethod]
    public void CommentExemptChainWithTrailingNonInvokedPropertyStartsEveryLinkAtRootColumn()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = a
                                         // keep wrapped
                                         .Prop.Foo()
                                         .Result;
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = a

                                                // keep wrapped
                                                .Prop
                                                .Foo()
                                                .Result;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a <c>#region</c> directive in front of the first link has the same effect as a comment: every link
    /// starts its own line at the root column
    /// </summary>
    [TestMethod]
    public void RegionAboveCommentExemptChainWithSharedInvokedLinkLineStartsEveryLinkAtRootColumn()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = a
                                         #region Chain
                                         .Prop.Foo()
                                         .Bar().Baz();
                                         #endregion
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = a

                                        #region Chain

                                                .Prop
                                                .Foo()
                                                .Bar()
                                                .Baz();

                                        #endregion // Chain
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that disabled text (an <c>#if false</c> body) in front of the first link has the same effect as a comment:
    /// every link starts its own line at the root column
    /// </summary>
    [TestMethod]
    public void DisabledTextAboveCommentExemptChainWithSharedInvokedLinkLineStartsEveryLinkAtRootColumn()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = a
                             #if false
                                         .Disabled()
                             #endif
                                         .Prop.Foo()
                                         .Bar().Baz();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = a
                                #if false
                                            .Disabled()
                                #endif
                                                .Prop
                                                .Foo()
                                                .Bar()
                                                .Baz();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment-kept chain puts a conditional first invoked link on its own line with <c>?.</c> at the
    /// root column
    /// </summary>
    [TestMethod]
    public void CommentExemptChainWithConditionalAccessSharingFirstInvokedLinkLineStartsEveryLinkAtRootColumn()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = a
                                         // keep wrapped
                                         .Prop?.Foo()
                                         .Bar().Baz();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = a

                                                // keep wrapped
                                                .Prop
                                                ?.Foo()
                                                .Bar()
                                                .Baz();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment-kept chain puts a null-forgiving first invoked link on its own line with <c>!.</c> at the
    /// root column
    /// </summary>
    [TestMethod]
    public void CommentExemptChainWithNullForgivingLinkSharingFirstInvokedLinkLineStartsEveryLinkAtRootColumn()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = a
                                         // keep wrapped
                                         .Prop!.Foo()
                                         .Bar().Baz();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = a

                                                // keep wrapped
                                                .Prop
                                                !.Foo()
                                                .Bar()
                                                .Baz();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verify that a chain rooted in a multi-line implicit array initializer whose closing brace
    /// shares its line with an element aligns its continuation dot to the first invoked link
    /// </summary>
    [TestMethod]
    public void MultiLineImplicitArrayInitializerWithTrailingCloseBraceAlignsToInvokedLink()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = new[] { 1,
                                         2 }.Select(item => item)
                                        .ToList();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = new[] { 1,
                                                    2 }.Select(item => item)
                                                       .ToList();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verify that a chain rooted in a parenthesized <c>with</c> expression aligns its continuation
    /// dot to the first invoked link while the initializer braces stay on the <c>with</c> keyword
    /// </summary>
    [TestMethod]
    public void ChainRootedInParenthesizedWithExpressionAlignsToInvokedLink()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(Record r)
                                 {
                                     var x = (r with
                                     {
                                         Value = 1
                                     }).Select(item => item)
                                        .ToList();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M(Record r)
                                    {
                                        var x = (r with
                                                   {
                                                       Value = 1
                                                   }).Select(item => item)
                                                     .ToList();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verify that a chain rooted in a parenthesized object creation aligns its continuation dot to
    /// the first invoked link
    /// </summary>
    [TestMethod]
    public void ChainRootedInParenthesizedObjectCreationAlignsToInvokedLink()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = (new Wrapper
                                     {
                                         Value = 1
                                     }).Select(item => item)
                                        .ToList();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = (new Wrapper
                                                 {
                                                     Value = 1
                                                 }).Select(item => item)
                                                   .ToList();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verify that a chain rooted in a parenthesized array creation whose closing brace shares its
    /// line with an element aligns its continuation dot to the first invoked link
    /// </summary>
    [TestMethod]
    public void ChainRootedInParenthesizedArrayCreationAlignsToInvokedLink()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = (new int[] { 1,
                                         2 }).Select(item => item)
                                        .ToList();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = (new int[] { 1,
                                                     2 }).Select(item => item)
                                                         .ToList();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verify that a chain rooted in a collection expression aligns its continuation dot to the first
    /// invoked link
    /// </summary>
    [TestMethod]
    public void ChainRootedInCollectionExpressionAlignsToInvokedLink()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = [1,
                                         2 ].Select(item => item)
                                        .ToList();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = [
                                                    1,
                                                    2
                                                ].Select(item => item)
                                                 .ToList();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verify that a chain rooted in an anonymous object creation aligns its continuation dot to the
    /// first invoked link
    /// </summary>
    [TestMethod]
    public void ChainRootedInAnonymousObjectAlignsToInvokedLink()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = new
                                     {
                                         Value = 1
                                     }.ToString()
                                      .Trim();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = new
                                                {
                                                    Value = 1
                                                }.ToString()
                                                 .Trim();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verify that a chain rooted in a parenthesized switch expression aligns its continuation dot to
    /// the first invoked link
    /// </summary>
    [TestMethod]
    public void ChainRootedInParenthesizedSwitchExpressionAlignsToInvokedLink()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int i)
                                 {
                                     var x = (i switch
                                     {
                                         1 => "a",
                                         _ => "b"
                                     }).Select(item => item)
                                        .ToList();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M(int i)
                                    {
                                        var x = (i switch
                                                 {
                                                     1 => "a",
                                                     _ => "b"
                                                 }).Select(item => item)
                                                   .ToList();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verify that a comment above a conditional-access first invoked link keeps the chain wrapped
    /// while every invoked link still starts its own line
    /// </summary>
    [TestMethod]
    public void CommentAboveConditionalAccessChainRootKeepsEveryInvokedLinkOnItsOwnLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = a
                                         // keep wrapped
                                         ?.Foo()
                                         .Bar().Baz();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = a

                                                // keep wrapped
                                                ?.Foo()
                                                .Bar()
                                                .Baz();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verify that a non-invoked prefix dot on the chain root line keeps the anchor column while the
    /// invoked links are broken onto their own lines
    /// </summary>
    [TestMethod]
    public void CommentAboveChainWithPrefixDotOnRootLineBreaksLinksAtAnchorColumn()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = a.Prop
                                         // keep wrapped
                                         .Foo()
                                         .Bar().Baz();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = a.Prop

                                                 // keep wrapped
                                                 .Foo()
                                                 .Bar()
                                                 .Baz();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verify that a conditional-access prefix whose binding is not invoked still breaks every
    /// following invoked link onto its own line
    /// </summary>
    [TestMethod]
    public void CommentAboveChainWithNonInvokedConditionalPrefixBreaksEveryInvokedLink()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = a
                                         // keep wrapped
                                         ?.Prop.Foo()
                                         .Bar().Baz();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = a

                                                // keep wrapped
                                                ?.Prop
                                                .Foo()
                                                .Bar()
                                                .Baz();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verify that a comment above the first invoked link does not stop the join of an uncommented, separately wrapped
    /// first link in front of it: only the trivia in front of the chain's first link decides that join, matching the
    /// directive arm below
    /// </summary>
    [TestMethod]
    public void CommentAboveFirstInvokedLinkStillCollapsesWrappedPrefixDot()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = a
                                         .Prop
                                         // keep wrapped
                                         .Foo()
                                         .Bar();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = a.Prop

                                                 // keep wrapped
                                                 .Foo()
                                                 .Bar();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verify that only the chain's own first wrapped dot collapses onto the root line when a comment
    /// sits above the first invoked link and more than one non-invoked prefix dot is wrapped; the
    /// remaining prefix stays on its own line but aligns to the anchor column
    /// </summary>
    [TestMethod]
    public void CommentAboveFirstInvokedLinkCollapsesOnlyFirstOfTwoWrappedPrefixDots()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = a
                                         .Prop
                                         .Other
                                         // keep wrapped
                                         .Foo()
                                         .Bar();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = a.Prop
                                                 .Other

                                                 // keep wrapped
                                                 .Foo()
                                                 .Bar();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verify that a comment above a wrapped null-forgiving operator in front of the chain's first dot refuses the join
    /// of that link and keeps <c>!.</c> together, and that every later link starts its own line in the root's column
    /// </summary>
    [TestMethod]
    public void CommentAboveWrappedNullForgivingRootRefusesCollapseWithTwoLinks()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = a
                                         // keep wrapped
                                         !
                                         .Foo()
                                         .Bar();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = a

                                                // keep wrapped
                                                !.Foo()
                                                .Bar();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Same as <see cref="CommentAboveWrappedNullForgivingRootRefusesCollapseWithTwoLinks"/> with a third invoked link
    /// </summary>
    [TestMethod]
    public void CommentAboveWrappedNullForgivingRootRefusesCollapseWithThreeLinks()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = a
                                         // keep wrapped
                                         !
                                         .Foo()
                                         .Bar()
                                         .Baz();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = a

                                                // keep wrapped
                                                !.Foo()
                                                .Bar()
                                                .Baz();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verify that an uncommented, wrapped null-forgiving operator on the chain root still collapses
    /// onto the root line, and the remaining invoked links align to its own column — the other side
    /// of the boundary from the two tests above, proving the fix refuses only the commented case
    /// rather than every null-forgiving-root chain (repair)
    /// </summary>
    [TestMethod]
    public void WrappedNullForgivingRootWithoutCommentStillCollapsesOntoChainRoot()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = a
                                         !
                                         .Foo()
                                         .Bar()
                                         .Baz();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = a!.Foo()
                                                 .Bar()
                                                 .Baz();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verify that a pragma directive above the first invoked link leaves every invoked link on its
    /// own line
    /// </summary>
    [TestMethod]
    public void PragmaAboveFirstInvokedLinkKeepsEveryInvokedLinkOnItsOwnLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = a
                             #pragma warning disable 1234
                                         .Foo()
                                         .Bar().Baz();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = a
                                #pragma warning disable 1234
                                                .Foo()
                                                .Bar()
                                                .Baz();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verify that disabled text above the first invoked link leaves every invoked link on its own
    /// line
    /// </summary>
    [TestMethod]
    public void DisabledTextAboveFirstInvokedLinkKeepsEveryInvokedLinkOnItsOwnLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = a
                             #if false
                                     .Nope()
                             #endif
                                         .Foo()
                                         .Bar().Baz();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = a
                                #if false
                                        .Nope()
                                #endif
                                                .Foo()
                                                .Bar()
                                                .Baz();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verify that a wrapped null-conditional chain whose continuations sit on the chain-root dot is
    /// realigned to the first invoked link
    /// </summary>
    [TestMethod]
    public void NullConditionalChainMisalignedToChainRootDotIsRealignedToInvokedLink()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(object metadata)
                                 {
                                     var filePath = metadata.Media?.FirstOrDefault()
                                                    ?.Part
                                                    ?.FirstOrDefault()
                                                    ?.File;
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M(object metadata)
                                    {
                                        var filePath = metadata.Media?.FirstOrDefault()
                                                                     ?.Part
                                                                     ?.FirstOrDefault()
                                                                     ?.File;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verify that a top-level null-conditional chain misaligned to the chain-root dot is realigned to
    /// the first invoked link
    /// </summary>
    [TestMethod]
    public void TopLevelNullConditionalChainMisalignedToChainRootDotIsRealignedToInvokedLink()
    {
        // Arrange
        const string input = """
                             var filePath = metadata.Media?.FirstOrDefault()
                                            ?.Part
                                            ?.FirstOrDefault()
                                            ?.File;
                             """;

        const string expected = """
                                var filePath = metadata.Media?.FirstOrDefault()
                                                             ?.Part
                                                             ?.FirstOrDefault()
                                                             ?.File;
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verify that a null-conditional chain whose root is split across lines rejoins the non-invoked
    /// first dot onto its root and aligns the continuations to the first invoked link
    /// </summary>
    [TestMethod]
    public void NullConditionalChainWithSplitRootRejoinsAndAlignsToInvokedLink()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(object metadata)
                                 {
                                     var filePath = metadata
                                                    .Media?.FirstOrDefault()
                                                          ?.Part
                                                          ?.FirstOrDefault()
                                                          ?.File;
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M(object metadata)
                                    {
                                        var filePath = metadata.Media?.FirstOrDefault()
                                                                     ?.Part
                                                                     ?.FirstOrDefault()
                                                                     ?.File;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped null-forgiving operator introducing a non-invoked prefix
    /// (<c>!.Prop</c>) collapses onto the chain root line the same way a plain non-invoked prefix dot
    /// does, and the following invoked links align to the resulting column
    /// </summary>
    [TestMethod]
    public void WrappedNullForgivingNonInvokedPrefixCollapsesOntoChainRoot()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = a
                                         !.Prop
                                         .Foo()
                                         .Bar();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = a!.Prop
                                                 .Foo()
                                                 .Bar();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment above the chain's first invoked link does not stop the join of an earlier, uncommented,
    /// wrapped null-forgiving first link (<c>!.Prop</c>) — mirroring
    /// <see cref="CommentAboveFirstInvokedLinkStillCollapsesWrappedPrefixDot"/> for a plain prefix dot
    /// </summary>
    [TestMethod]
    public void CommentAboveFirstInvokedLinkStillCollapsesWrappedNullForgivingPrefixDot()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = a
                                         !.Prop
                                         // keep wrapped
                                         .Foo()
                                         .Bar();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = a!.Prop

                                                 // keep wrapped
                                                 .Foo()
                                                 .Bar();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that only the chain's own first wrapped token collapses onto the root line when a
    /// null-forgiving prefix is followed by a second, plain non-invoked prefix dot before a comment
    /// above the first invoked link; the remaining prefix stays on its own line but aligns to the
    /// anchor column, mirroring <see cref="CommentAboveFirstInvokedLinkCollapsesOnlyFirstOfTwoWrappedPrefixDots"/>
    /// </summary>
    [TestMethod]
    public void CommentAboveFirstInvokedLinkCollapsesOnlyFirstOfTwoWrappedPrefixDotsWithNullForgivingPrefix()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = a
                                         !.Prop
                                         .Other
                                         // keep wrapped
                                         .Foo()
                                         .Bar();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = a!.Prop
                                                 .Other

                                                 // keep wrapped
                                                 .Foo()
                                                 .Bar();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped null-forgiving link in front of the first call stays wrapped when a property access comes
    /// before it: <c>a.Prop1!.Prop2.Foo()</c> keeps the user's wrap at <c>!.Prop2</c> the same way
    /// <c>a.Prop1.Prop2.Foo()</c> keeps it at <c>.Prop2</c>, because only the chain's first link is joined; see
    /// <see cref="WrappedNullForgivingInvokedLinkWithIntermediateMemberAccessStaysWrapped"/> for the same shape with the
    /// first call written as <c>!.Foo()</c>
    /// </summary>
    [TestMethod]
    public void NullForgivingPrefixWithIntermediateMemberAccessStaysWrapped()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = a.Prop1
                                         !.Prop2
                                         .Foo()
                                         .Bar();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = a.Prop1
                                                 !.Prop2
                                                 .Foo()
                                                 .Bar();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped first call written as <c>!.Foo()</c> stays wrapped when the root ends in a property access:
    /// the call follows a prefix, so it may keep the user's wrap, and <c>!.</c> stays together under the prefix link
    /// </summary>
    [TestMethod]
    public void WrappedNullForgivingInvokedLinkWithIntermediateMemberAccessStaysWrapped()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = a.Prop1
                                         !.Foo()
                                         .Bar();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = a.Prop1
                                                 !.Foo()
                                                 .Bar();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a break between a prefix's null-forgiving operator and the following dot moves in front of the
    /// <c>!</c>, so the user's wrap is kept as <c>!.Prop2</c>, and that the layout is stable on a second pass
    /// </summary>
    [TestMethod]
    public void WrappedDotBehindNullForgivingPrefixKeepsOperatorTogetherAcrossPasses()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = a
                                         .Prop1!
                                         .Prop2
                                         .Foo()
                                         .Bar();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = a.Prop1
                                                 !.Prop2
                                                 .Foo()
                                                 .Bar();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verify that a <c>#pragma</c> directive directly above a wrapped null-forgiving prefix leaves
    /// every invoked link on its own line, mirroring
    /// <see cref="PragmaAboveFirstInvokedLinkKeepsEveryInvokedLinkOnItsOwnLine"/> for a
    /// null-forgiving prefix link
    /// </summary>
    [TestMethod]
    public void PragmaAboveWrappedNullForgivingPrefixKeepsEveryInvokedLinkOnItsOwnLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = a
                             #pragma warning disable 1234
                                         !.Foo()
                                         .Bar().Baz();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = a
                                #pragma warning disable 1234
                                                !.Foo()
                                                .Bar()
                                                .Baz();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verify that disabled text directly above a wrapped null-forgiving prefix leaves every invoked
    /// link on its own line, mirroring
    /// <see cref="DisabledTextAboveFirstInvokedLinkKeepsEveryInvokedLinkOnItsOwnLine"/> for a
    /// null-forgiving prefix link
    /// </summary>
    [TestMethod]
    public void DisabledTextAboveWrappedNullForgivingPrefixKeepsEveryInvokedLinkOnItsOwnLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = a
                             #if false
                                     .Nope()
                             #endif
                                         !.Foo()
                                         .Bar().Baz();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = a
                                #if false
                                        .Nope()
                                #endif
                                                !.Foo()
                                                .Bar()
                                                .Baz();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}