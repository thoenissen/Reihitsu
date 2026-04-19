using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/> — object-initializer alignment
/// </summary>
[TestClass]
public class ObjectInitializerAlignmentTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that an object creation without an initializer remains unchanged.
    /// </summary>
    [TestMethod]
    public void NoInitializerRemainsUnchanged()
    {
        // Arrange
        const string input = """
                             var x = new Foo();
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a multi-line object initializer aligns braces to the <c>new</c> keyword
    /// column and assignments are indented by +4.
    /// </summary>
    [TestMethod]
    public void MultiLineInitializerAlignsToNewKeyword()
    {
        // Arrange
        const string input = """
                             var x = new Foo
                                       {
                                                 A = 1
                                       };
                             """;

        const string expected = """
                                var x = new Foo
                                        {
                                            A = 1
                                        };
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that multiple assignments in an object initializer remain aligned,
    /// including fluent-chain continuation lines.
    /// </summary>
    [TestMethod]
    public void MultipleAssignmentsWithFluentChainAlignCorrectly()
    {
        // Arrange
        const string input = """
                             var list = new List<string>();
                             var x = new Foo
                             {
                                 A = "123",
                                    B = "123",
                                    C = list.Where(s => s == "123")
                                      .FirstOrDefault(),
                                    D = "123"
                             };
                             """;

        const string expected = """
                                var list = new List<string>();
                                var x = new Foo
                                        {
                                            A = "123",
                                            B = "123",
                                            C = list.Where(s => s == "123")
                                                    .FirstOrDefault(),
                                            D = "123"
                                        };
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an empty object initializer has its braces realigned to the <c>new</c> keyword column.
    /// </summary>
    [TestMethod]
    public void EmptyInitializerBracesAlignToNewKeyword()
    {
        // Arrange — multi-line empty initializer with wrong brace positions
        const string input = """
                             var x = new Foo
                                          {
                                          };
                             """;

        const string expected = """
                                var x = new Foo
                                        {
                                        };
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a collection initializer is reformatted with proper indentation.
    /// </summary>
    [TestMethod]
    public void CollectionInitializerIsReformatted()
    {
        // Arrange
        const string input = """
                             var x = new List<int> { 1, 2, 3 };
                             """;

        const string expected = """
                                var x = new List<int>
                                        {
                                            1,
                                            2,
                                            3
                                        };
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an indented object creation (column > 0) aligns correctly.
    /// The combined rule normalizes block indentation, so the global statement
    /// moves to column 0 and the initializer aligns relative to the <c>new</c> keyword.
    /// </summary>
    [TestMethod]
    public void IndentedObjectCreationAlignsCorrectly()
    {
        // Arrange — new at column 12
        const string input = """
                                 var x = new Foo
                                              {
                                                        A = 1
                                              };
                             """;

        const string expected = """
                                var x = new Foo
                                        {
                                            A = 1
                                        };
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a multi-line assignment with a method chain inside an
    /// object initializer keeps chain continuation alignment relative to the assignment.
    /// </summary>
    [TestMethod]
    public void MultilineAssignmentAlignsCorrectly()
    {
        // Arrange
        const string input = """
                             var l = new List<string>();
                             var x = new Foo
                             {
                             A = 1,
                             B = l.Where(s => s.Length > 3)
                             .FirstOrDefault()
                             };
                             """;

        const string expected = """
                                var l = new List<string>();
                                var x = new Foo
                                        {
                                            A = 1,
                                            B = l.Where(s => s.Length > 3)
                                                 .FirstOrDefault()
                                        };
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a nested initializer inside another initializer aligns correctly.
    /// </summary>
    [TestMethod]
    public void NestedInitializerAlignsCorrectly()
    {
        // Arrange — outer new at col 8, inner new at col 18 after formatting
        const string input = """
                             var x = new Foo
                                       {
                                            Bar = new Baz
                                                      {
                                                           C = 3
                                                      }
                                       };
                             """;

        const string expected = """
                                var x = new Foo
                                        {
                                            Bar = new Baz
                                                  {
                                                      C = 3
                                                  }
                                        };
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an already correctly aligned initializer is not modified.
    /// </summary>
    [TestMethod]
    public void AlreadyCorrectLayoutNoChange()
    {
        // Arrange — new at col 8, brace at col 8, assignment at col 12
        const string input = """
                             var x = new Foo
                                     {
                                         A = 1
                                     };
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that object initializers inside switch expressions are preserved.
    /// </summary>
    [TestMethod]
    public void SwitchExpressionObjectInitializerPreservesLayout()
    {
        // Arrange
        const string input = """
                             internal enum EncounterCode
                             {
                                 SkyForge,
                                 MistVault,
                             }

                             internal enum EncounterCategory
                             {
                                 Challenge,
                                 Training,
                             }

                             internal enum EncounterSegment
                             {
                                 UpperSpire,
                                 LowerWing,
                             }

                             internal enum EncounterTarget
                             {
                                 PrimeConstruct,
                                 ArchiveSentinel,
                                 Unknown,
                             }

                             internal sealed class EncounterDescriptor
                             {
                                 public EncounterCategory Category { get; set; }
                                 public EncounterSegment Segment { get; set; }
                                 public EncounterTarget Target { get; set; }
                             }

                             internal static class EncounterMapper
                             {
                                 public static EncounterDescriptor MapFromCode(long encounterCode)
                                 {
                                     return ((EncounterCode)encounterCode) switch
                                            {
                                                EncounterCode.SkyForge => new EncounterDescriptor
                                                                          {
                                                                              Category = EncounterCategory.Challenge,
                                                                              Segment = EncounterSegment.UpperSpire,
                                                                              Target = EncounterTarget.PrimeConstruct,
                                                                          },
                                                EncounterCode.MistVault => new EncounterDescriptor
                                                                           {
                                                                               Category = EncounterCategory.Training,
                                                                               Segment = EncounterSegment.LowerWing,
                                                                               Target = EncounterTarget.ArchiveSentinel,
                                                                           },
                                                _ => new EncounterDescriptor
                                                     {
                                                         Category = EncounterCategory.Training,
                                                         Segment = EncounterSegment.LowerWing,
                                                         Target = EncounterTarget.Unknown,
                                                     },
                                            };
                                 }
                             }
                             """;

        const string expected = input;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that misaligned object initializers inside switch expressions are corrected.
    /// </summary>
    [TestMethod]
    public void SwitchExpressionObjectInitializerMisalignedGetsFormatted()
    {
        // Arrange
        const string input = """
                             internal enum DispatchCode
                             {
                                 EmberHall,
                                 TideGallery,
                             }

                             internal enum DispatchGroup
                             {
                                 Raid,
                                 Practice,
                             }

                             internal enum DispatchZone
                             {
                                 EastDeck,
                                 WestDeck,
                             }

                             internal enum DispatchTarget
                             {
                                 IronKeeper,
                                 CrystalWatcher,
                                 Unknown,
                             }

                             internal sealed class DispatchDescriptor
                             {
                                 public DispatchGroup Group { get; set; }
                                 public DispatchZone Zone { get; set; }
                                 public DispatchTarget Target { get; set; }
                             }

                             internal static class DispatchMapper
                             {
                                 public static DispatchDescriptor Map(long dispatchCode)
                                 {
                                     return ((DispatchCode)dispatchCode) switch
                                            {
                                                DispatchCode.EmberHall => new DispatchDescriptor
                                                                             {
                                                                      Group = DispatchGroup.Raid,
                                                                                Zone = DispatchZone.EastDeck,
                                                                             Target = DispatchTarget.IronKeeper,
                                                                             },
                                                DispatchCode.TideGallery => new DispatchDescriptor
                                                                            {
                                                                    Group = DispatchGroup.Practice,
                                                                             Zone = DispatchZone.WestDeck,
                                                                                  Target = DispatchTarget.CrystalWatcher,
                                                                            },
                                                _ => new DispatchDescriptor
                                                              {
                                                                    Group = DispatchGroup.Practice,
                                                                    Zone = DispatchZone.WestDeck,
                                                                         Target = DispatchTarget.Unknown,
                                                              },
                                            };
                                 }
                             }
                             """;

        const string expected = """
                                internal enum DispatchCode
                                {
                                    EmberHall,
                                    TideGallery,
                                }

                                internal enum DispatchGroup
                                {
                                    Raid,
                                    Practice,
                                }

                                internal enum DispatchZone
                                {
                                    EastDeck,
                                    WestDeck,
                                }

                                internal enum DispatchTarget
                                {
                                    IronKeeper,
                                    CrystalWatcher,
                                    Unknown,
                                }

                                internal sealed class DispatchDescriptor
                                {
                                    public DispatchGroup Group { get; set; }
                                    public DispatchZone Zone { get; set; }
                                    public DispatchTarget Target { get; set; }
                                }

                                internal static class DispatchMapper
                                {
                                    public static DispatchDescriptor Map(long dispatchCode)
                                    {
                                        return ((DispatchCode)dispatchCode) switch
                                               {
                                                   DispatchCode.EmberHall => new DispatchDescriptor
                                                                             {
                                                                                 Group = DispatchGroup.Raid,
                                                                                 Zone = DispatchZone.EastDeck,
                                                                                 Target = DispatchTarget.IronKeeper,
                                                                             },
                                                   DispatchCode.TideGallery => new DispatchDescriptor
                                                                               {
                                                                                   Group = DispatchGroup.Practice,
                                                                                   Zone = DispatchZone.WestDeck,
                                                                                   Target = DispatchTarget.CrystalWatcher,
                                                                               },
                                                   _ => new DispatchDescriptor
                                                        {
                                                            Group = DispatchGroup.Practice,
                                                            Zone = DispatchZone.WestDeck,
                                                            Target = DispatchTarget.Unknown,
                                                        },
                                               };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that combined flags in an object initializer stay on separate lines with aligned pipe operators.
    /// </summary>
    [TestMethod]
    public void ObjectInitializerCombinedFlagsAlignPipes()
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

                             class Foo
                             {
                                 public MyFlags Flags { get; set; }
                             }

                             var value = new Foo
                                         {
                                             Flags = MyFlags.First
                                                 | MyFlags.Second
                                                         | MyFlags.Third
                                         };
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

                                class Foo
                                {
                                    public MyFlags Flags { get; set; }
                                }

                                var value = new Foo
                                            {
                                                Flags = MyFlags.First
                                                        | MyFlags.Second
                                                        | MyFlags.Third
                                            };
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a collection initializer entry with an inline object initializer and lambda block body
    /// is aligned correctly.
    /// </summary>
    [TestMethod]
    public void CollectionInitializerEntryWithLambdaBlockBodyAlignsCorrectly()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var list = new List<SelectMenuEntryData<bool>>
                                               {
                                                  new()
                                                  {
                                                     CommandText = LocalizationGroup.GetText("Key", "Fallback"),
                                                   InteractionResponse = async obj =>
                                                                                   {
                                                                                await obj.RespondWithModalAsync().ConfigureAwait(false);
                             
                                                                                return false;
                                                                         }
                                               }
                                               };
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var list = new List<SelectMenuEntryData<bool>>
                                                   {
                                                       new()
                                                       {
                                                           CommandText = LocalizationGroup.GetText("Key", "Fallback"),
                                                           InteractionResponse = async obj =>
                                                                                 {
                                                                                     await obj.RespondWithModalAsync().ConfigureAwait(false);

                                                                                     return false;
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
    /// Verifies that switch-expression arms with object initializers remain unchanged when already aligned.
    /// </summary>
    [TestMethod]
    public void SwitchExpressionObjectInitializerArmsRemainAligned()
    {
        // Arrange
        const string input = """
                             using System;

                             class CommandFactory
                             {
                                 object Build(int commandType)
                                 {
                                     return commandType switch
                                            {
                                                1 => new MessageCommandBuilder
                                                     {
                                                         Name = "test",
                                                         IsEnabled = true,
                                                         ContextTypes = true
                                                                            ? new[] { 1, 2, 3 }
                                                                            : new[] { 1, 2 }
                                                     }.Build(),
                                                2 => new UserCommandBuilder
                                                     {
                                                         Name = "test",
                                                         IsEnabled = true,
                                                         ContextTypes = true
                                                                            ? new[] { 1, 2, 3 }
                                                                            : new[] { 1, 2 }
                                                     }.Build(),
                                                _ => throw new InvalidOperationException("Unsupported type.")
                                            };
                                 }
                             }

                             class MessageCommandBuilder
                             {
                                 public string Name { get; set; }
                                 public bool IsEnabled { get; set; }
                                 public int[] ContextTypes { get; set; }

                                 public object Build()
                                 {
                                     return this;
                                 }
                             }

                             class UserCommandBuilder
                             {
                                 public string Name { get; set; }
                                 public bool IsEnabled { get; set; }
                                 public int[] ContextTypes { get; set; }

                                 public object Build()
                                 {
                                     return this;
                                 }
                             }
                             """;

        const string expected = input;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a collection initializer inside an object initializer is indented correctly.
    /// </summary>
    [TestMethod]
    public void CollectionInitializerInsideObjectInitializerIndentsCorrectly()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Collections.Generic;

                             class C
                             {
                                 void M(string content)
                                 {
                                     var result = Deserialize(content, new Settings
                                                                          {
                                                                              Items = {
                                                                                          new Converter()
                                                                                      },
                                                                              Handler = (sender, args) =>
                                                                                        {
                                                                                            if (args.Path == "test")
                                                                                            {
                                                                                                args.Handled = true;
                                                                                            }
                                                                                        }
                                                                          });
                                 }

                                 static T Deserialize<T>(string content, Settings settings)
                                 {
                                     return default;
                                 }
                             }

                             class Settings
                             {
                                 public List<Converter> Items { get; set; }

                                 public EventHandler<ErrorArgs> Handler { get; set; }
                             }

                             class Converter
                             {
                             }

                             class ErrorArgs : EventArgs
                             {
                                 public string Path { get; set; }

                                 public bool Handled { get; set; }
                             }
                             """;

        const string expected = """
                                using System;
                                using System.Collections.Generic;

                                class C
                                {
                                    void M(string content)
                                    {
                                        var result = Deserialize(content, new Settings
                                                                          {
                                                                              Items = {
                                                                                          new Converter()
                                                                                      },
                                                                              Handler = (sender, args) =>
                                                                                        {
                                                                                            if (args.Path == "test")
                                                                                            {
                                                                                                args.Handled = true;
                                                                                            }
                                                                                        }
                                                                          });
                                    }

                                    static T Deserialize<T>(string content, Settings settings)
                                    {
                                        return default;
                                    }
                                }

                                class Settings
                                {
                                    public List<Converter> Items { get; set; }

                                    public EventHandler<ErrorArgs> Handler { get; set; }
                                }

                                class Converter
                                {
                                }

                                class ErrorArgs : EventArgs
                                {
                                    public string Path { get; set; }

                                    public bool Handled { get; set; }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a nested object initializer with collection initializer and lambda block body
    /// indents correctly relative to the <c>new</c> keyword in a generic method argument.
    /// </summary>
    [TestMethod]
    public void GenericMethodCallWithCollectionInitializerAndLambdaAlignsCorrectly()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Collections.Generic;

                             class C
                             {
                                 void M(string payload)
                                 {
                                     var config = DataParser.Parse<ImportResult>(payload, new ParserOptions
                                                                                                           {
                                                                                                               Validators = { new SchemaValidator() },
                                                                                                               OnError = (_, context) =>
                                                                                                                         {
                                                                                                                             if (context.FieldName == "identifier")
                                                                                                                             {
                                                                                                                                 context.IsHandled = true;
                                                                                                                             }
                                                                                                                         }
                                                                                                           });
                                 }

                                 static T Parse<T>(string payload, ParserOptions options)
                                 {
                                     return default;
                                 }
                             }

                             static class DataParser
                             {
                                 public static T Parse<T>(string payload, ParserOptions options)
                                 {
                                     return default;
                                 }
                             }

                             class ParserOptions
                             {
                                 public List<SchemaValidator> Validators { get; set; }

                                 public EventHandler<ErrorContext> OnError { get; set; }
                             }

                             class SchemaValidator
                             {
                             }

                             class ErrorContext : EventArgs
                             {
                                 public string FieldName { get; set; }

                                 public bool IsHandled { get; set; }
                             }
                             """;

        const string expected = """
                                using System;
                                using System.Collections.Generic;

                                class C
                                {
                                    void M(string payload)
                                    {
                                        var config = DataParser.Parse<ImportResult>(payload, new ParserOptions
                                                                                             {
                                                                                                 Validators = {
                                                                                                                  new SchemaValidator()
                                                                                                              },
                                                                                                 OnError = (_, context) =>
                                                                                                           {
                                                                                                               if (context.FieldName == "identifier")
                                                                                                               {
                                                                                                                   context.IsHandled = true;
                                                                                                               }
                                                                                                           }
                                                                                             });
                                    }

                                    static T Parse<T>(string payload, ParserOptions options)
                                    {
                                        return default;
                                    }
                                }

                                static class DataParser
                                {
                                    public static T Parse<T>(string payload, ParserOptions options)
                                    {
                                        return default;
                                    }
                                }

                                class ParserOptions
                                {
                                    public List<SchemaValidator> Validators { get; set; }

                                    public EventHandler<ErrorContext> OnError { get; set; }
                                }

                                class SchemaValidator
                                {
                                }

                                class ErrorContext : EventArgs
                                {
                                    public string FieldName { get; set; }

                                    public bool IsHandled { get; set; }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}