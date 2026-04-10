using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reihitsu.Formatter.Test.Unit.Rules.Base;

namespace Reihitsu.Formatter.Test.Unit.Rules.Indentation;

/// <summary>
/// Unit tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/>
/// </summary>
[TestClass]
public class IndentationAndAlignmentRuleTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that top-level code at column 0 stays at column 0.
    /// </summary>
    [TestMethod]
    public void TopLevelCodeNoIndentation()
    {
        // Arrange
        const string input = """
            class C
            {
            }
            """;

        const string expected = """
            class C
            {
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a conditional expression moves the <c>?</c> token to the next line
    /// when the true branch begins on a new line.
    /// </summary>
    [TestMethod]
    public void ConditionalExpressionMovesQuestionTokenToNextLine()
    {
        // Arrange
        const string input = """
            class C
            {
                void M(Entry entry, Item item, IEnumerable<Upgrade> upgrades)
                {
                    var itemName = entry.ItemId == null ?
                                       upgrades.FirstOrDefault(obj => obj.Id == entry.UpgradeId)?.Name
                                       : item?.Name;
                }
            }
            """;

        const string expected = """
            class C
            {
                void M(Entry entry, Item item, IEnumerable<Upgrade> upgrades)
                {
                    var itemName = entry.ItemId == null
                                       ? upgrades.FirstOrDefault(obj => obj.Id == entry.UpgradeId)?.Name
                                       : item?.Name;
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a multiline switch expression in a regular method is broken and aligned correctly.
    /// </summary>
    [TestMethod]
    public void SwitchExpressionWithoutLambdaBreaksAndAligns()
    {
        // Arrange
        const string input = """
            class C
            {
                int Map(string value)
                {
                    return value switch
                                       {
                                           "a" => 1,
                                       _ => 0
                          };
                }
            }
            """;

        const string expected = """
            class C
            {
                int Map(string value)
                {
                    return value switch
                           {
                               "a" => 1,
                               _ => 0
                           };
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a multiline switch expression in a lambda block body is broken and aligned correctly.
    /// </summary>
    [TestMethod]
    public void SwitchExpressionInsideLambdaBreaksAndAligns()
    {
        // Arrange
        const string input = """
            class C
            {
                int M(IEnumerable<string> values)
                {
                    return values.Aggregate(0,
                               (total, current) =>
                                 {
                                     var mapped = current switch
                                                       {
                                                           "a" => 1,
                                                       _ => 0
                                         };

                                     return total + mapped;
                                 });
                }
            }
            """;

        const string expected = """
            class C
            {
                int M(IEnumerable<string> values)
                {
                    return values.Aggregate(0,
                                            (total, current) =>
                                            {
                                                var mapped = current switch
                                                             {
                                                                 "a" => 1,
                                                                 _ => 0
                                                             };

                                                return total + mapped;
                                            });
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that members of a block-scoped namespace get one level of indentation.
    /// </summary>
    [TestMethod]
    public void BlockScopedNamespaceMembersOneLevel()
    {
        // Arrange
        const string input = """
            namespace X
            {
            class C
            {
            }
            }
            """;

        const string expected = """
            namespace X
            {
                class C
                {
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that file-scoped namespaces do not add extra indentation because they have no braces.
    /// </summary>
    [TestMethod]
    public void FileScopedNamespaceNoExtraIndent()
    {
        // Arrange
        const string input = """
            namespace X;
            class C
            {
            }
            """;

        const string expected = """
            namespace X;
            class C
            {
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that methods inside a class get correct indentation.
    /// </summary>
    [TestMethod]
    public void ClassMembersCorrectIndentation()
    {
        // Arrange
        const string input = """
            class C
            {
            void M()
            {
            }
            void N()
            {
            }
            }
            """;

        const string expected = """
            class C
            {
                void M()
                {
                }
                void N()
                {
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that statements inside a method body get correct indentation.
    /// </summary>
    [TestMethod]
    public void MethodBodyCorrectIndentation()
    {
        // Arrange
        const string input = """
            class C
            {
            void M()
            {
            int x = 1;
            return;
            }
            }
            """;

        const string expected = """
            class C
            {
                void M()
                {
                    int x = 1;

                    return;
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that nested blocks accumulate indentation levels.
    /// </summary>
    [TestMethod]
    public void NestedBlocksCumulativeIndentation()
    {
        // Arrange
        const string input = """
            class C
            {
            void M()
            {
            if (true)
            {
            if (false)
            {
            int x = 1;
            }
            }
            }
            }
            """;

        const string expected = """
            class C
            {
                void M()
                {
                    if (true)
                    {
                        if (false)
                        {
                            int x = 1;
                        }
                    }
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that switch statements have correct indentation for the switch body, case labels, and statements.
    /// </summary>
    [TestMethod]
    public void SwitchStatementCorrectLevels()
    {
        // Arrange
        const string input = """
            class C
            {
            void M(int x)
            {
            switch (x)
            {
            case 1:
            break;
            default:
            break;
            }
            }
            }
            """;

        const string expected = """
            class C
            {
                void M(int x)
                {
                    switch (x)
                    {
                        case 1:
                            break;

                        default:
                            break;
                    }
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that statements in a switch section get an extra indent level beyond the case labels.
    /// </summary>
    [TestMethod]
    public void SwitchSectionStatementsIndented()
    {
        // Arrange
        const string input = """
            class C
            {
            void M(int x)
            {
            switch (x)
            {
            case 1:
            int y = 1;
            break;
            }
            }
            }
            """;

        const string expected = """
            class C
            {
                void M(int x)
                {
                    switch (x)
                    {
                        case 1:
                            int y = 1;
                            break;
                    }
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that property accessor bodies get correct indentation.
    /// </summary>
    [TestMethod]
    public void PropertyAccessorCorrectIndentation()
    {
        // Arrange
        const string input = """
            class C
            {
            int X
            {
            get
            {
            return 1;
            }
            }
            }
            """;

        const string expected = """
            class C
            {
                int X
                {
                    get
                    {
                        return 1;
                    }
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that enum members get correct indentation.
    /// </summary>
    [TestMethod]
    public void EnumMembersCorrectIndentation()
    {
        // Arrange
        const string input = """
            enum E
            {
            A,
            B,
            C
            }
            """;

        const string expected = """
            enum E
            {
                A,
                B,
                C
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that initializer expression contents get correct indentation.
    /// </summary>
    [TestMethod]
    public void InitializerExpressionIndentsCorrectly()
    {
        // Arrange
        const string input = """
            class C
            {
            void M()
            {
            var x = new Foo
            {
            X = 1
            };
            }
            }
            """;

        const string expected = """
            class C
            {
                void M()
                {
                    var x = new Foo
                            {
                                X = 1
                            };
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that anonymous object creation contents get correct indentation.
    /// </summary>
    [TestMethod]
    public void AnonymousObjectIndentsCorrectly()
    {
        // Arrange
        const string input = """
            class C
            {
            void M()
            {
            var x = new
            {
            A = 1,
            B = 2
            };
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
                                A = 1,
                                B = 2
                            };
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that inline anonymous object members are broken to separate lines with proper indentation.
    /// </summary>
    [TestMethod]
    public void InlineAnonymousObjectBreaksToMultipleLines()
    {
        // Arrange - anonymous object members on a single line should be broken to separate lines
        const string input = """
            class C
            {
                void M()
                {
                    var x = items.Select(g => new { Date = g.Key, Total = g.Sum() })
                                 .ToList();
                }
            }
            """;

        const string expected = """
            class C
            {
                void M()
                {
                    var x = items.Select(g => new
                                              {
                                                  Date = g.Key,
                                                  Total = g.Sum()
                                              })
                                 .ToList();
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that wrong indentation (e.g. 2 spaces) is corrected to the expected 4 spaces per level.
    /// </summary>
    [TestMethod]
    public void WrongIndentationCorrected()
    {
        // Arrange
        const string input = """
            class C
            {
              void M()
              {
                int x = 1;
              }
            }
            """;

        const string expected = """
            class C
            {
                void M()
                {
                    int x = 1;
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that tab-based indentation is replaced with the correct number of spaces.
    /// </summary>
    [TestMethod]
    public void TabsReplacedWithSpaces()
    {
        // Arrange
        var nl = Environment.NewLine;
        var input = $"class C{nl}{{{nl}\tvoid M(){nl}\t{{{nl}\t\tint x = 1;{nl}\t}}{nl}}}";
        var expected = $"class C{nl}{{{nl}    void M(){nl}    {{{nl}        int x = 1;{nl}    }}{nl}}}";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that single-line comments in leading trivia are preserved and the following token gets correct indentation.
    /// </summary>
    [TestMethod]
    public void CommentsPreservedWithCorrectIndentation()
    {
        // Arrange
        const string input = """
            class C
            {
                // A comment
            void M()
            {
            }
            }
            """;

        const string expected = """
            class C
            {
                // A comment
                void M()
                {
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an array creation expression with a multi-line initializer
    /// in a return statement gets correct indentation aligned to the <c>new</c> keyword.
    /// </summary>
    [TestMethod]
    public void ArrayInitializerInReturnStatementIndentsCorrectly()
    {
        // Arrange
        const string input = """
            class C
            {
                IReadOnlyList<int> M()
                {
                    return new int[]
                           {
                               1,
                               2,
                               3
                           };
                }
            }
            """;

        const string expected = """
            class C
            {
                IReadOnlyList<int> M()
                {
                    return new int[]
                           {
                               1,
                               2,
                               3
                           };
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an array creation expression nested inside an argument-aligned
    /// argument list gets correct indentation aligned to the <c>new</c> keyword.
    /// </summary>
    [TestMethod]
    public void ArrayInitializerInArgumentAlignedCallIndentsCorrectly()
    {
        // Arrange
        const string input = """
            class C
            {
                void M()
                {
                    var x = Outer(Inner(new int[]
                                        {
                                            1,
                                            2
                                        }));
                }
            }
            """;

        const string expected = """
            class C
            {
                void M()
                {
                    var x = Outer(Inner(new int[]
                                        {
                                            1,
                                            2
                                        }));
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a method chain on an argument-aligned line preserves
    /// the correct chain alignment relative to the argument column.
    /// </summary>
    [TestMethod]
    public void ChainOnArgumentAlignedLineAlignsCorrectly()
    {
        // Arrange
        const string input = """
            class C
            {
                void M()
                {
                    var x = Call(SyntaxFactory.Token(SyntaxKind.OpenBraceToken)
                                             .WithLeadingTrivia(SyntaxFactory.Whitespace(" "))
                                             .WithTrailingTrivia(SyntaxFactory.EndOfLine("\n")));
                }
            }
            """;

        const string expected = """
            class C
            {
                void M()
                {
                    var x = Call(SyntaxFactory.Token(SyntaxKind.OpenBraceToken)
                                              .WithLeadingTrivia(SyntaxFactory.Whitespace(" "))
                                              .WithTrailingTrivia(SyntaxFactory.EndOfLine("\n")));
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a lambda with a block body
    /// gets correct indentation for the block and its contents.
    /// </summary>
    [TestMethod]
    public void LambdaBlockBodyInArgumentListIndentsCorrectly()
    {
        // Arrange
        const string input = """
            class C
            {
                void M()
                {
                    var result = items.Select(x => x,
                                              (a, b) =>
                                              {
                                                  if (a > b)
                                                  {
                                                      return a;
                                                  }

                                                  return b;
                                              });
                }
            }
            """;

        const string expected = """
            class C
            {
                void M()
                {
                    var result = items.Select(x => x,
                                              (a, b) =>
                                              {
                                                  if (a > b)
                                                  {
                                                      return a;
                                                  }

                                                  return b;
                                              });
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that switch expression arms remain aligned with the switch expression block style.
    /// </summary>
    [TestMethod]
    public void SwitchExpressionArmsRemainAlignedWithSwitchKeyword()
    {
        // Arrange
        const string input = """
            class C
            {
                private static bool IsKeywordRequiringSpace(SyntaxToken token)
                {
                    return token.Kind() switch
                           {
                               SyntaxKind.IfKeyword
                               or SyntaxKind.ForKeyword
                               or SyntaxKind.ForEachKeyword
                               or SyntaxKind.WhileKeyword
                               or SyntaxKind.SwitchKeyword
                               or SyntaxKind.CatchKeyword
                               or SyntaxKind.UsingKeyword
                               or SyntaxKind.LockKeyword
                               or SyntaxKind.ReturnKeyword
                               or SyntaxKind.ThrowKeyword
                               or SyntaxKind.NewKeyword => true,
                               _ => false
                           };
                }
            }
            """;

        const string expected = """
            class C
            {
                private static bool IsKeywordRequiringSpace(SyntaxToken token)
                {
                    return token.Kind() switch
                           {
                               SyntaxKind.IfKeyword
                               or SyntaxKind.ForKeyword
                               or SyntaxKind.ForEachKeyword
                               or SyntaxKind.WhileKeyword
                               or SyntaxKind.SwitchKeyword
                               or SyntaxKind.CatchKeyword
                               or SyntaxKind.UsingKeyword
                               or SyntaxKind.LockKeyword
                               or SyntaxKind.ReturnKeyword
                               or SyntaxKind.ThrowKeyword
                               or SyntaxKind.NewKeyword => true,
                               _ => false
                           };
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that misaligned multi-line <c>or</c> pattern lines in a switch expression arm
    /// are corrected to the arm indentation.
    /// </summary>
    [TestMethod]
    public void SwitchExpressionOrPatternWithWrongIndentationIsFormattedCorrectly()
    {
        // Arrange
        const string input = """
            class C
            {
                private static bool IsKeywordRequiringSpace(SyntaxToken token)
                {
                    return token.Kind() switch
                                       {
                               SyntaxKind.IfKeyword
                    or SyntaxKind.ForKeyword
                          or SyntaxKind.ForEachKeyword
                                       or SyntaxKind.WhileKeyword
                               or SyntaxKind.SwitchKeyword => true,
                               _ => false
                        };
                }
            }
            """;

        const string expected = """
            class C
            {
                private static bool IsKeywordRequiringSpace(SyntaxToken token)
                {
                    return token.Kind() switch
                           {
                               SyntaxKind.IfKeyword
                               or SyntaxKind.ForKeyword
                               or SyntaxKind.ForEachKeyword
                               or SyntaxKind.WhileKeyword
                               or SyntaxKind.SwitchKeyword => true,
                               _ => false
                           };
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that combined flags in a local variable stay on separate lines with aligned pipe operators.
    /// </summary>
    [TestMethod]
    public void LocalVariableCombinedFlagsAlignPipes()
    {
        // Arrange
        const string input = """
            using System;

            [Flags]
            enum MyFlags
            {
                None = 0,
                First = 1,
                Second = 2,
                Third = 4
            }

            class C
            {
                void M()
                {
                    var flags = MyFlags.First
                              | MyFlags.Second
                        | MyFlags.Third;
                }
            }
            """;

        const string expected = """
            using System;

            [Flags]
            enum MyFlags
            {
                None = 0,
                First = 1,
                Second = 2,
                Third = 4
            }

            class C
            {
                void M()
                {
                    var flags = MyFlags.First
                                | MyFlags.Second
                                | MyFlags.Third;
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a method call keeps the first argument on the same line as the method name,
    /// including calls that use named arguments.
    /// </summary>
    [TestMethod]
    public void MethodCallFirstArgumentIsPlacedOnMethodLine()
    {
        // Arrange
        const string input = """
            class C
            {
                void M()
                {
                    Compute(
                        1,
                            2);

                    Configure(
                        timeout: 100,
                              retries: 3);
                }

                void Compute(int x, int y)
                {
                }

                void Configure(int timeout, int retries)
                {
                }
            }
            """;

        const string expected = """
            class C
            {
                void M()
                {
                    Compute(1,
                            2);

                    Configure(timeout: 100,
                              retries: 3);
                }

                void Compute(int x, int y)
                {
                }

                void Configure(int timeout, int retries)
                {
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that switch case block braces remain unchanged.
    /// </summary>
    [TestMethod]
    public void SwitchCaseBlockBracesRemainUnchanged()
    {
        // Arrange
        const string input = """
            class C
            {
                int M(string value)
                {
                    var result = 0;

                    switch (value)
                    {
                        case "A":
                            {
                                result = 1;
                            }
                            break;

                        case "B":
                            {
                                result = 2;
                            }
                            break;
                    }

                    return result;
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that multiline <c>or</c> pattern alignment in <c>is</c>-style expressions remains unchanged.
    /// </summary>
    [TestMethod]
    public void MultilineOrPatternExpressionRemainsUnchanged()
    {
        // Arrange
        const string input = """
            class C
            {
                bool M(object token)
                {
                    return token is string
                                    or int
                                    or long;
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a nested async lambda in an object initializer is indented and aligned correctly.
    /// </summary>
    [TestMethod]
    public void NestedAsyncLambdaInObjectInitializerIndentsCorrectly()
    {
        // Arrange
        const string input = """
            class C
            {
                void M()
                {
                    options.Add(new OptionItem<bool>
                                {
                                    Label = TextCatalog.GetFormattedText("DeleteProfile", "Delete profile '{0}'", profileKey),
                                    Action = async () =>
                                             {
                                                 if (await ExecuteStep<ProfileDeleteConfirmationDialog, bool>(new ProfileDeleteConfirmationDialog(_textService, profileKey)).ConfigureAwait(false))
                                                 {
                                                     var actor = await _actorService.GetActorByContextId(RuntimeContext.User)
                                                                                    .ConfigureAwait(false);

                                                     if (_storeFactory.GetStore<ProfileActivityStore>()
                                                                      .RemoveRange(item => item.Profile.OwnerId == actor.Id
                                                                                           && item.Key == profileKey)
                                                         && _storeFactory.GetStore<ProfileStore>()
                                                                         .Remove(item => item.OwnerId == actor.Id
                                                                                         && item.Key == profileKey))
                                                     {
                                                         await RuntimeContext.Channel
                                                                             .SendMessageAsync(TextCatalog.GetText("ProfileDeleted", "The profile was deleted successfully."))
                                                                             .ConfigureAwait(false);
                                                     }
                                                     else
                                                     {
                                                         throw _storeFactory.LastException;
                                                     }
                                                 }

                                                 return true;
                                             }
                                });
                }
            }
            """;

        const string expected = input;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that switch option parsing case blocks remain unchanged.
    /// </summary>
    [TestMethod]
    public void SwitchOptionParsingCaseBlocksRemainUnchanged()
    {
        // Arrange
        const string input = """
            class C
            {
                void M(string[] args)
                {
                    var checkOnly = false;

                    foreach (var arg in args)
                    {
                        switch (arg)
                        {
                        case "--check":
                        {
                            checkOnly = true;
                        }
                        break;

                        default:
                        {
                        }
                        break;
                        }
                    }
                }
            }
            """;

        const string expected = """
            class C
            {
                void M(string[] args)
                {
                    var checkOnly = false;

                    foreach (var arg in args)
                    {
                        switch (arg)
                        {
                            case "--check":
                                {
                                    checkOnly = true;
                                }
                                break;

                            default:
                                {
                                }
                                break;
                        }
                    }
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that switch expression arms with or patterns and object initializer results are formatted correctly.
    /// </summary>
    [TestMethod]
    public void SwitchExpressionOrPatternWithObjectInitializerFormatsCorrectly()
    {
        // Arrange
        const string input = """
            class C
            {
                private Data Process(int type)
                {
                    return type switch
                                  {
                                      1
                                          or 2 => new Data
                                                    {
                                                        Category = "Primary",
                                                        Group = "Main"
                                                    },
                                  _ => null
                                  };
                }
            }

            class Data
            {
                public string Category { get; set; }
                public string Group { get; set; }
            }
            """;

        const string expected = """
            class C
            {
                private Data Process(int type)
                {
                    return type switch
                           {
                               1
                               or 2 => new Data
                                       {
                                           Category = "Primary",
                                           Group = "Main"
                                       },
                               _ => null
                           };
                }
            }

            class Data
            {
                public string Category { get; set; }
                public string Group { get; set; }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that multiline predicate and action arguments keep their alignment.
    /// </summary>
    [TestMethod]
    public void RepositoryRefreshCallWithLambdaArgumentsRemainsAligned()
    {
        // Arrange
        const string input = """
            class AccountProcessor
            {
                void UpdateAccount()
                {
                    if (_store.GetRepository<AccountRecordRepository>()
                              .Upsert(item => item.UserId == user.Id
                                              && item.Name == payload.Name,
                                      item =>
                                      {
                                          item.UserId = user.Id;
                                          item.Name = payload.Name;
                                      }))
                    {
                    }
                }

                dynamic _store;
                dynamic user;
                dynamic payload;
            }

            class AccountRecordRepository
            {
            }
            """;

        const string expected = """
            class AccountProcessor
            {
                void UpdateAccount()
                {
                    if (_store.GetRepository<AccountRecordRepository>()
                              .Upsert(item => item.UserId == user.Id
                                              && item.Name == payload.Name,
                                      item =>
                                      {
                                          item.UserId = user.Id;
                                          item.Name = payload.Name;
                                      }))
                    {
                    }
                }

                dynamic _store;
                dynamic user;
                dynamic payload;
            }

            class AccountRecordRepository
            {
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that multiline calls with expression lambdas keep their alignment.
    /// </summary>
    [TestMethod]
    public void RepositoryRefreshWithExpressionLambdaRemainsAligned()
    {
        // Arrange
        const string input = """
            class TokenProcessor
            {
                void UpdateToken(string token)
                {
                    if (_store.GetRepository<UserRecordRepository>()
                              .Refresh(item => item.Id == current.Id,
                                       item => item.Token = string.IsNullOrWhiteSpace(token)
                            ? null
                                                    : token))
                    {
                    }
                }

                dynamic _store;
                dynamic current;
            }

            class UserRecordRepository
            {
            }
            """;

        const string expected = """
            class TokenProcessor
            {
                void UpdateToken(string token)
                {
                    if (_store.GetRepository<UserRecordRepository>()
                              .Refresh(item => item.Id == current.Id,
                                       item => item.Token = string.IsNullOrWhiteSpace(token)
                                                   ? null
                                                   : token))
                    {
                    }
                }

                dynamic _store;
                dynamic current;
            }

            class UserRecordRepository
            {
            }
            """;

        // Act & Assert
        AssertRuleResult(input, ApplyRule(expected));
    }

    /// <summary>
    /// Verifies that multiline calls with block lambdas keep their alignment.
    /// </summary>
    [TestMethod]
    public void RepositoryRefreshWithBlockLambdaRemainsAligned()
    {
        // Arrange
        const string input = """
            class ProfileProcessor
            {
                void UpdateProfile(string name)
                {
                    if (_store.GetRepository<ProfileRecordRepository>()
                              .Refresh(item => item.Id == context.Id,
                                       item =>
                                       {
                                           item.Name = string.IsNullOrWhiteSpace(name) ? null
                                                           : name;
                                       }))
                    {
                    }
                }

                dynamic _store;
                dynamic context;
            }

            class ProfileRecordRepository
            {
            }
            """;

        const string expected = """
            class ProfileProcessor
            {
                void UpdateProfile(string name)
                {
                    if (_store.GetRepository<ProfileRecordRepository>()
                              .Refresh(item => item.Id == context.Id,
                                       item =>
                                       {
                                           item.Name = string.IsNullOrWhiteSpace(name)
                                                           ? null
                                                           : name;
                                       }))
                    {
                    }
                }

                dynamic _store;
                dynamic context;
            }

            class ProfileRecordRepository
            {
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that base type continuation remains aligned.
    /// </summary>
    [TestMethod]
    public void BaseTypeContinuationRemainsAligned()
    {
        // Arrange
        const string input = """
            public sealed class WorkerCoordinator : ServiceBase,
                                    IAsyncDisposable,
                                                    IJobFactory
            {
            }

            public interface ServiceBase
            {
            }

            public interface IAsyncDisposable
            {
            }

            public interface IJobFactory
            {
            }
            """;

        const string expected = """
            public sealed class WorkerCoordinator : ServiceBase,
                                                    IAsyncDisposable,
                                                    IJobFactory
            {
            }

            public interface ServiceBase
            {
            }

            public interface IAsyncDisposable
            {
            }

            public interface IJobFactory
            {
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that binary pattern continuation with <c>and</c> remains aligned.
    /// </summary>
    [TestMethod]
    public void BinaryPatternAndContinuationRemainsAligned()
    {
        // Arrange
        const string input = """
            using System;

            public static class DateHelper
            {
                public static bool IsBusinessDay(DateTime date)
                {
                    var day = date.DayOfWeek;

                    return day is >= DayOfWeek.Monday
                              and <= DayOfWeek.Friday;
                }
            }
            """;

        const string expected = """
            using System;

            public static class DateHelper
            {
                public static bool IsBusinessDay(DateTime date)
                {
                    var day = date.DayOfWeek;

                    return day is >= DayOfWeek.Monday
                                  and <= DayOfWeek.Friday;
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that <c>is</c> pattern after multiline await-chain remains aligned.
    /// </summary>
    [TestMethod]
    public void IsPatternAfterAwaitChainRemainsAligned()
    {
        // Arrange
        const string input = """
            using System.Collections.Generic;
            using System.Threading.Tasks;

            class ChannelReader
            {
                async Task<List<object>> Read(ulong id)
                {
                    var result = new List<object>();

                    if (await Context.Provider
                                     .GetChannelAsync(id)
                                     .ConfigureAwait(false)
                    is IVoiceChannel channel)
                    {
                        result.Add(channel);
                    }

                    return result;
                }
            }

            interface IVoiceChannel
            {
            }

            static class Context
            {
                public static dynamic Provider { get; set; }
            }
            """;

        const string expected = """
            using System.Collections.Generic;
            using System.Threading.Tasks;

            class ChannelReader
            {
                async Task<List<object>> Read(ulong id)
                {
                    var result = new List<object>();

                    if (await Context.Provider
                                     .GetChannelAsync(id)
                                     .ConfigureAwait(false)
                        is IVoiceChannel channel)
                    {
                        result.Add(channel);
                    }

                    return result;
                }
            }

            interface IVoiceChannel
            {
            }

            static class Context
            {
                public static dynamic Provider { get; set; }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that nested ternary continuation remains aligned.
    /// </summary>
    [TestMethod]
    public void NestedTernaryContinuationRemainsAligned()
    {
        // Arrange
        const string input = """
            class HeaderBuilder
            {
                void Build()
                {
                    var outer = 0;
                    var inner = 1;

                    var title = outer == 0 ? "A" : inner == 1 ? "B" : "C";
                }
            }
            """;

        const string expected = """
            class HeaderBuilder
            {
                void Build()
                {
                    var outer = 0;
                    var inner = 1;

                    var title = outer == 0
                                    ? "A"
                                    : inner == 1
                                        ? "B"
                                        : "C";
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that invocation lambdas with block bodies remain aligned.
    /// </summary>
    [TestMethod]
    public void InvokeLambdaBodyRemainsAligned()
    {
        // Arrange
        const string input = """
            using System.Threading.Tasks;

            class ApiConnector
            {
                Task<string> FetchAsync()
                {
                    return Invoke(async () =>
                    {
                        using (var response = await CreateRequest("https://api.example.test/resource").ConfigureAwait(false))
                        {
                            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        }
                    });
                }

                Task<T> Invoke<T>(System.Func<Task<T>> func)
                {
                    return func();
                }

                Task<dynamic> CreateRequest(string url)
                {
                    return Task.FromResult<dynamic>(null);
                }
            }
            """;

        const string expected = """
            using System.Threading.Tasks;

            class ApiConnector
            {
                Task<string> FetchAsync()
                {
                    return Invoke(async () =>
                                  {
                                      using (var response = await CreateRequest("https://api.example.test/resource").ConfigureAwait(false))
                                      {
                                          return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                                      }
                                  });
                }

                Task<T> Invoke<T>(System.Func<Task<T>> func)
                {
                    return func();
                }

                Task<dynamic> CreateRequest(string url)
                {
                    return Task.FromResult<dynamic>(null);
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that null-coalescing continuation remains aligned.
    /// </summary>
    [TestMethod]
    public void NullCoalescingContinuationRemainsAligned()
    {
        // Arrange
        const string input = """
            using System.Threading.Tasks;

            class LogProvider
            {
                async Task<LogEntry> GetAsync(string id)
                {
                    return await TryReadCache(id).ConfigureAwait(false)
                      ?? await ReadRemote(id).ConfigureAwait(false);
                }

                Task<LogEntry> TryReadCache(string id)
                {
                    return Task.FromResult<LogEntry>(null);
                }

                Task<LogEntry> ReadRemote(string id)
                {
                    return Task.FromResult<LogEntry>(null);
                }
            }

            class LogEntry
            {
            }
            """;

        const string expected = """
            using System.Threading.Tasks;

            class LogProvider
            {
                async Task<LogEntry> GetAsync(string id)
                {
                    return await TryReadCache(id).ConfigureAwait(false)
                           ?? await ReadRemote(id).ConfigureAwait(false);
                }

                Task<LogEntry> TryReadCache(string id)
                {
                    return Task.FromResult<LogEntry>(null);
                }

                Task<LogEntry> ReadRemote(string id)
                {
                    return Task.FromResult<LogEntry>(null);
                }
            }

            class LogEntry
            {
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that addition continuation remains aligned.
    /// </summary>
    [TestMethod]
    public void AdditionContinuationRemainsAligned()
    {
        // Arrange
        const string input = """
            class SortValueProvider
            {
                int GetValue(int id)
                {
                    return Helper.GetPrimary(id)
                            + Helper.GetSecondary(id);
                }
            }

            static class Helper
            {
                public static int GetPrimary(int id)
                {
                    return id;
                }

                public static int GetSecondary(int id)
                {
                    return id;
                }
            }
            """;

        const string expected = """
            class SortValueProvider
            {
                int GetValue(int id)
                {
                    return Helper.GetPrimary(id)
                           + Helper.GetSecondary(id);
                }
            }

            static class Helper
            {
                public static int GetPrimary(int id)
                {
                    return id;
                }

                public static int GetSecondary(int id)
                {
                    return id;
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that deeply nested refresh-range lambdas remain aligned.
    /// </summary>
    [TestMethod]
    public void DeeplyNestedRefreshRangeLambdasRemainAligned()
    {
        // Arrange
        const string input = """
            using System;
            using System.Collections.Generic;

            class AppointmentService
            {
                void Assign(List<long> selectedIds)
                {
                    var now = DateTime.Now;

                    store.GetRepository<AppointmentRepository>()
                         .RefreshRange(item => item.TimeStamp > now
                                              && selectedIds.Contains(item.Id),
                                       item =>
                                       {
                                           item.OwnerId = 1;
                                       });
                }

                dynamic store;
            }

            class AppointmentRepository;
            """;

        const string expected = """
            using System;
            using System.Collections.Generic;

            class AppointmentService
            {
                void Assign(List<long> selectedIds)
                {
                    var now = DateTime.Now;

                    store.GetRepository<AppointmentRepository>()
                         .RefreshRange(item => item.TimeStamp > now
                                               && selectedIds.Contains(item.Id),
                                       item =>
                                       {
                                           item.OwnerId = 1;
                                       });
                }

                dynamic store;
            }

            class AppointmentRepository;
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that nested upsert calls inside property lambdas remain aligned.
    /// </summary>
    [TestMethod]
    public void UpsertInsidePropertyLambdaRemainsAligned()
    {
        // Arrange
        const string input = """
            class MessageSetup
            {
                void Configure()
                {
                    if (store.GetRepository<MessageRuleRepository>()
                             .Upsert(item => item.ChannelId == context.Channel.Id,
                                     item =>
                                     {
                                         item.ChannelId = context.Channel.Id;
                                     })
                        == false)
                    {
                    }
                }

                dynamic store;
                dynamic context;
            }

            class MessageRuleRepository;
            """;

        const string expected = """
            class MessageSetup
            {
                void Configure()
                {
                    if (store.GetRepository<MessageRuleRepository>()
                             .Upsert(item => item.ChannelId == context.Channel.Id,
                                     item =>
                                     {
                                         item.ChannelId = context.Channel.Id;
                                     })
                        == false)
                    {
                    }
                }

                dynamic store;
                dynamic context;
            }

            class MessageRuleRepository;
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that configuration upsert calls remain aligned.
    /// </summary>
    [TestMethod]
    public void ConfigurationUpsertRemainsAligned()
    {
        // Arrange
        const string input = """
            class ChannelConfigurationService
            {
                void SetChannel(long tenantId, int type, ulong messageId)
                {
                    store.GetRepository<ChannelConfigurationRepository>()
                         .Upsert(item => item.TenantId == tenantId
                                      && item.Type == type,
                                 item =>
                                 {
                                     item.TenantId = tenantId;
                                     item.Type = type;
                                     item.MessageId = messageId;
                                 });
                }

                dynamic store;
            }

            class ChannelConfigurationRepository;
            """;

        const string expected = """
            class ChannelConfigurationService
            {
                void SetChannel(long tenantId, int type, ulong messageId)
                {
                    store.GetRepository<ChannelConfigurationRepository>()
                         .Upsert(item => item.TenantId == tenantId
                                         && item.Type == type,
                                 item =>
                                 {
                                     item.TenantId = tenantId;
                                     item.Type = type;
                                     item.MessageId = messageId;
                                 });
                }

                dynamic store;
            }

            class ChannelConfigurationRepository;
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that simple second-lambda arguments remain aligned.
    /// </summary>
    [TestMethod]
    public void RefreshWithSimpleSecondLambdaRemainsAligned()
    {
        // Arrange
        const string input = """
            class RankEditor
            {
                void Update(long rankId, string name)
                {
                    store.GetRepository<RankRepository>()
                         .Refresh(item => item.Id == rankId,
                                  item => item.Name = name);
                }

                dynamic store;
            }

            class RankRepository
            {
            }
            """;

        const string expected = """
            class RankEditor
            {
                void Update(long rankId, string name)
                {
                    store.GetRepository<RankRepository>()
                         .Refresh(item => item.Id == rankId,
                                  item => item.Name = name);
                }

                dynamic store;
            }

            class RankRepository
            {
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that role-assignment upsert lambdas remain aligned.
    /// </summary>
    [TestMethod]
    public void RoleAssignmentUpsertRemainsAligned()
    {
        // Arrange
        const string input = """
            class RoleAssignmentEditor
            {
                void Update(long configId, AssignmentData data)
                {
                    store.GetRepository<RoleAssignmentRepository>()
                         .Upsert(item => item.ConfigurationId == configId
                                      && item.RoleId == data.RoleId,
                                 item =>
                                 {
                                     item.ConfigurationId = configId;
                                     item.RoleId = data.RoleId;
                                     item.Points = data.Points;
                                 });
                }

                dynamic store;
            }

            class RoleAssignmentRepository;

            class AssignmentData
            {
                public ulong RoleId { get; set; }
                public int Points { get; set; }
            }
            """;

        const string expected = """
            class RoleAssignmentEditor
            {
                void Update(long configId, AssignmentData data)
                {
                    store.GetRepository<RoleAssignmentRepository>()
                         .Upsert(item => item.ConfigurationId == configId
                                         && item.RoleId == data.RoleId,
                                 item =>
                                 {
                                     item.ConfigurationId = configId;
                                     item.RoleId = data.RoleId;
                                     item.Points = data.Points;
                                 });
                }

                dynamic store;
            }

            class RoleAssignmentRepository;

            class AssignmentData
            {
                public ulong RoleId { get; set; }
                public int Points { get; set; }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that complex upsert lambdas with nested conditions remain aligned.
    /// </summary>
    [TestMethod]
    public void ComplexUpsertLambdaRemainsAligned()
    {
        // Arrange
        const string input = """
            using System;
            using System.Collections.Generic;

            class AssignmentJob
            {
                void Run(long tenantId, List<RankInfo> ranks)
                {
                    foreach (var account in GetAccounts())
                    {
                        if (_store.GetRepository<AssignmentRepository>()
                                 .Upsert(item => item.UserId == account.Id
                                               && item.TenantId == tenantId,
                                          item =>
                                          {
                                              item.TenantId = tenantId;
                                              item.UserId = account.Id;

                                              if (ranks.Count > 0)
                                              {
                                                  item.RankId = ranks[0].Id;
                                                  item.TimeStamp = DateTime.Now;
                                              }
                                          }) == false)
                        {
                        }
                    }
                }

                List<AccountInfo> GetAccounts()
                {
                    return new List<AccountInfo>();
                }

                dynamic _store;
            }

            class AssignmentRepository
            {
            }

            class RankInfo
            {
                public long Id { get; set; }
            }

            class AccountInfo
            {
                public long Id { get; set; }
            }
            """;

        const string expected = """
            using System;
            using System.Collections.Generic;

            class AssignmentJob
            {
                void Run(long tenantId, List<RankInfo> ranks)
                {
                    foreach (var account in GetAccounts())
                    {
                        if (_store.GetRepository<AssignmentRepository>()
                                  .Upsert(item => item.UserId == account.Id
                                                  && item.TenantId == tenantId,
                                          item =>
                                          {
                                              item.TenantId = tenantId;
                                              item.UserId = account.Id;

                                              if (ranks.Count > 0)
                                              {
                                                  item.RankId = ranks[0].Id;
                                                  item.TimeStamp = DateTime.Now;
                                              }
                                          }) == false)
                        {
                        }
                    }
                }

                List<AccountInfo> GetAccounts()
                {
                    return new List<AccountInfo>();
                }

                dynamic _store;
            }

            class AssignmentRepository
            {
            }

            class RankInfo
            {
                public long Id { get; set; }
            }

            class AccountInfo
            {
                public long Id { get; set; }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that refresh calls with multiline block lambdas remain aligned.
    /// </summary>
    [TestMethod]
    public void CommitRefreshLambdaRemainsAligned()
    {
        // Arrange
        const string input = """
            using System;

            class CommitService
            {
                void Commit(long id)
                {
                    var next = new AppointmentRecord();

                    store.GetRepository<AppointmentRepository>()
                         .Refresh(item => item.Id == id,
                                  item =>
                                  {
                                      item.IsCommitted = true;
                                        next.TimeStamp =    item.TimeStamp.AddDays(7);
                                  });
                }

                dynamic store;
            }

            class AppointmentRepository
            {
            }

            class AppointmentRecord
            {
                public bool IsCommitted { get; set; }
                public DateTime TimeStamp { get; set; }
            }
            """;

        const string expected = """
            using System;

            class CommitService
            {
                void Commit(long id)
                {
                    var next = new AppointmentRecord();

                    store.GetRepository<AppointmentRepository>()
                         .Refresh(item => item.Id == id,
                                  item =>
                                  {
                                      item.IsCommitted = true;
                                      next.TimeStamp = item.TimeStamp.AddDays(7);
                                  });
                }

                dynamic store;
            }

            class AppointmentRepository
            {
            }

            class AppointmentRecord
            {
                public bool IsCommitted { get; set; }
                public DateTime TimeStamp { get; set; }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that refresh-range with nested lambda blocks remains aligned.
    /// </summary>
    [TestMethod]
    public void PointRefreshRangeLambdaRemainsAligned()
    {
        // Arrange
        const string input = """
            using System;
            using System.Collections.Generic;
            using System.Linq;

            class PointsService
            {
                void Recalculate(DateTime pointInTime)
                {
                    var users = GetUsers();

                    store.GetRepository<UserPointsRepository>()
                         .RefreshRange(item => true,
                                       item =>
                                       {
                                           var user = users.FirstOrDefault(u => u.UserId == item.UserId);

                                           if (user != null)
                                           {
                                               item.Points = user.Entries.Sum(entry =>
                                                                  {
                                                                      return entry.Value;
                                                                  });
                                           }
                                       });
                }

                List<UserData> GetUsers()
                {
                    return new List<UserData>();
                }

                dynamic store;
            }

            class UserPointsRepository
            {
            }

            class UserData
            {
                public long UserId { get; set; }
                public List<PointEntry> Entries { get; set; }
            }

            class PointEntry
            {
                public double Value { get; set; }
            }
            """;

        const string expected = """
            using System;
            using System.Collections.Generic;
            using System.Linq;

            class PointsService
            {
                void Recalculate(DateTime pointInTime)
                {
                    var users = GetUsers();

                    store.GetRepository<UserPointsRepository>()
                         .RefreshRange(item => true,
                                       item =>
                                       {
                                           var user = users.FirstOrDefault(u => u.UserId == item.UserId);

                                           if (user != null)
                                           {
                                               item.Points = user.Entries.Sum(entry =>
                                                                              {
                                                                                  return entry.Value;
                                                                              });
                                           }
                                       });
                }

                List<UserData> GetUsers()
                {
                    return new List<UserData>();
                }

                dynamic store;
            }

            class UserPointsRepository
            {
            }

            class UserData
            {
                public long UserId { get; set; }
                public List<PointEntry> Entries { get; set; }
            }

            class PointEntry
            {
                public double Value { get; set; }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, ApplyRule(expected));
    }

    /// <summary>
    /// Verifies that upsert calls with three lambda arguments remain aligned.
    /// </summary>
    [TestMethod]
    public void RegistrationUpsertWithIfElseRemainsAligned()
    {
        // Arrange
        const string input = """
            using System;

            class RegistrationService
            {
                void Register(long appointmentId)
                {
                    var user = GetUser();
                    long? registrationId = null;

                    if   (store.GetRepository<RegistrationRepository>()
                               .Upsert(item => item.AppointmentId == appointmentId
                                            && item.UserId == user.Id,
                                       item =>
                                       {
                                           if (item.Id == 0)
                                           {
                                               item.AppointmentId = appointmentId;
                                               item.UserId = user.Id;
                                               item.RegisteredAt = DateTime.Now;
                                           }
                                       },
                                       item => registrationId = item.Id))
                    {
                    }
                }

                UserInfo GetUser()
                {
                    return new UserInfo();
                }

                dynamic store;
            }

            class RegistrationRepository
            {
            }

            class UserInfo
            {
                public long Id { get; set; }
            }
            """;

        const string expected = """
            using System;

            class RegistrationService
            {
                void Register(long appointmentId)
                {
                    var user = GetUser();
                    long? registrationId = null;

                    if (store.GetRepository<RegistrationRepository>()
                             .Upsert(item => item.AppointmentId == appointmentId
                                             && item.UserId == user.Id,
                                     item =>
                                     {
                                         if (item.Id == 0)
                                         {
                                             item.AppointmentId = appointmentId;
                                             item.UserId = user.Id;
                                             item.RegisteredAt = DateTime.Now;
                                         }
                                     },
                                     item => registrationId = item.Id))
                    {
                    }
                }

                UserInfo GetUser()
                {
                    return new UserInfo();
                }

                dynamic store;
            }

            class RegistrationRepository
            {
            }

            class UserInfo
            {
                public long Id { get; set; }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that refresh calls in foreach loops remain aligned.
    /// </summary>
    [TestMethod]
    public void RefreshInForeachLoopRemainsAligned()
    {
        // Arrange
        const string input = """
            using System.Collections.Generic;

            class ImportJob
            {
                void Process(List<LogEntry> entries)
                {
                    foreach (var entry in entries)
                    {
                        _store.GetRepository<LogRepository>()
                        .Refresh(item => item.GroupId == entry.GroupId
                                            && item.Id == entry.Id,
                                       item => item.IsProcessed = true);
                    }
                }

                dynamic _store;
            }

            class LogRepository
            {
            }

            class LogEntry
            {
                public long GroupId { get; set; }
                public long Id { get; set; }
            }
            """;

        const string expected = """
            using System.Collections.Generic;

            class ImportJob
            {
                void Process(List<LogEntry> entries)
                {
                    foreach (var entry in entries)
                    {
                        _store.GetRepository<LogRepository>()
                              .Refresh(item => item.GroupId == entry.GroupId
                                               && item.Id == entry.Id,
                                       item => item.IsProcessed = true);
                    }
                }

                dynamic _store;
            }

            class LogRepository
            {
            }

            class LogEntry
            {
                public long GroupId { get; set; }
                public long Id { get; set; }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that refresh calls with wrapped condition and block action remain aligned.
    /// </summary>
    [TestMethod]
    public void RefreshWithFalseComparisonContinuationRemainsAligned()
    {
        // Arrange
        const string input = """
            using System;

            class SegmentService
            {
               void FinalizeSegment(ulong serverId, ulong channelId, ulong accountId, DateTime start, DateTime end)
                {
                    if (store.GetRepository<VoiceSegmentRepository>()
                     .Refresh(item => item.ServerId == serverId
                                         && item.ChannelId == channelId
                                         && item.AccountId == accountId
                                         && item.Start == start,
                           item =>
                           {
                               item.End = end;
                               item.IsCompleted = true;
                           })
                        == false)
                    {
                    }
                }

                dynamic store;
            }

            class VoiceSegmentRepository;
            """;

        const string expected = """
            using System;

            class SegmentService
            {
                void FinalizeSegment(ulong serverId, ulong channelId, ulong accountId, DateTime start, DateTime end)
                {
                    if (store.GetRepository<VoiceSegmentRepository>()
                             .Refresh(item => item.ServerId == serverId
                                              && item.ChannelId == channelId
                                              && item.AccountId == accountId
                                              && item.Start == start,
                                      item =>
                                      {
                                          item.End = end;
                                          item.IsCompleted = true;
                                      })
                        == false)
                    {
                    }
                }

                dynamic store;
            }

            class VoiceSegmentRepository;
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that trailing <c>== false</c> after multiline invocation remains aligned.
    /// </summary>
    [TestMethod]
    public void TrailingFalseComparisonAfterMultilineCallRemainsAligned()
    {
        // Arrange
        const string input = """
            class StorageDialog
            {
                void UpdateFlags(bool accepted, bool extendedAccepted)
                {
                    if (_store.GetRepository<UserRecordRepository>()
                              .Refresh(item => query.Any(match => match.UserId == item.Id
                                                               && match.ActorId == Session.Actor.Id),
                          item =>
                          {
                              item.IsAccepted = accepted;
                                    item.IsExtendedAccepted = extendedAccepted;
                          }) == false)
                    {
                    throw _store.LastError;
                    }
                }

                dynamic query;
                dynamic _store;

                static class Session
                {
                    public static dynamic Actor { get; set; }
                }
            }

            class UserRecordRepository
            {
            }
            """;

        const string expected = """
            class StorageDialog
            {
                void UpdateFlags(bool accepted, bool extendedAccepted)
                {
                    if (_store.GetRepository<UserRecordRepository>()
                              .Refresh(item => query.Any(match => match.UserId == item.Id
                                                                  && match.ActorId == Session.Actor.Id),
                                       item =>
                                       {
                                           item.IsAccepted = accepted;
                                           item.IsExtendedAccepted = extendedAccepted;
                                       }) == false)
                    {
                        throw _store.LastError;
                    }
                }

                dynamic query;
                dynamic _store;

                static class Session
                {
                    public static dynamic Actor { get; set; }
                }
            }

            class UserRecordRepository
            {
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}