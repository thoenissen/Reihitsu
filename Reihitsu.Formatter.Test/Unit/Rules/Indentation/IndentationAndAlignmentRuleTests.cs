using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reihitsu.Formatter.Rules;
using Reihitsu.Formatter.Rules.Indentation;

namespace Reihitsu.Formatter.Test.Unit.Rules.Indentation;

/// <summary>
/// Unit tests for <see cref="IndentationAndAlignmentRule"/>
/// </summary>
[TestClass]
public class IndentationAndAlignmentRuleTests
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

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(expected, actual);
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

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(expected, actual);
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

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(expected, actual);
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

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(expected, actual);
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

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(expected, actual);
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

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(expected, actual);
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

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(expected, actual);
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

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(expected, actual);
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

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(expected, actual);
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

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(expected, actual);
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

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(expected, actual);
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

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(expected, actual);
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

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(expected, actual);
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

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(expected, actual);
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

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(expected, actual);
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

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(expected, actual);
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

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(expected, actual);
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

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that the Phase property returns <see cref="FormattingPhase.Indentation"/>.
    /// </summary>
    [TestMethod]
    public void PhaseReturnsIndentation()
    {
        // Arrange
        var context = new FormattingContext(Environment.NewLine);
        var rule = new IndentationAndAlignmentRule(context, CancellationToken.None);

        // Act & Assert
        Assert.AreEqual(FormattingPhase.Indentation, rule.Phase);
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

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(expected, actual);
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

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(expected, actual);
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

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(expected, actual);
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

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(expected, actual);
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

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(expected, actual);
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

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(expected, actual);
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

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(expected, actual);
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

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(expected, actual);
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

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(input, actual);
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

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(input, actual);
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

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.IsTrue(System.Text.RegularExpressions.Regex.IsMatch(actual,
                                                                   @"var actor = await _actorService\.GetActorByContextId\(RuntimeContext\.User\)\r?\n\s+\.ConfigureAwait\(false\);"));

        Assert.IsTrue(System.Text.RegularExpressions.Regex.IsMatch(actual,
                                                                   @"await RuntimeContext\.Channel\r?\n\s+\.SendMessageAsync\(TextCatalog\.GetText\(""ProfileDeleted"", ""The profile was deleted successfully\.""\)\)"));

        Assert.IsFalse(System.Text.RegularExpressions.Regex.IsMatch(actual,
                                                                    @"GetActorByContextId\(RuntimeContext\.User\)\r?\n {120,}\.ConfigureAwait"));
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

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(input, actual);
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

        // Act
        var actual = ApplyRule(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    #endregion // Methods

    #region Helper Methods

    /// <summary>
    /// Applies the <see cref="IndentationAndAlignmentRule"/> to the given source code and returns the formatted result.
    /// </summary>
    /// <param name="input">The source code to format.</param>
    /// <returns>The formatted source code.</returns>
    private static string ApplyRule(string input)
    {
        var tree = CSharpSyntaxTree.ParseText(input);
        var context = new FormattingContext(Environment.NewLine);
        var rule = new IndentationAndAlignmentRule(context, CancellationToken.None);
        var result = rule.Apply(tree.GetRoot());

        return result.ToFullString();
    }

    #endregion // Helper Methods
}