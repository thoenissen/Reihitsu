using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/> — collection-expression alignment
/// </summary>
[TestClass]
public class CollectionExpressionAlignmentTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a multi-line collection expression with elements aligned after the
    /// opening bracket remains unchanged
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

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a single-line collection expression remains unchanged
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

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the current behavior for collection expressions assigned in a local declaration.
    /// Elements are aligned relative to the opening bracket column
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

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that object initializers in collection expressions align to the opening bracket column
    /// </summary>
    [TestMethod]
    public void CollectionExpressionObjectInitializerAlignsToOpeningBracketColumn()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var items = [
                                         new Item
                                         {
                                     Name = "One"
                                         }
                                     ];
                                 }

                                 private sealed class Item
                                 {
                                     public string Name { get; set; }
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var items = [
                                                        new Item
                                                        {
                                                            Name = "One"
                                                        }
                                                    ];
                                    }

                                    private sealed class Item
                                    {
                                        public string Name { get; set; }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the current behavior for collection expressions assigned in a field initializer.
    /// Elements are aligned relative to the opening bracket column
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

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the current behavior for collection expressions used with null-coalescing assignment
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

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the current behavior for a collection expression with object initializers
    /// in a null-coalescing assignment
    /// </summary>
    [TestMethod]
    public void CollectionExpressionWithObjectInitializersInNullCoalescingAssignmentCurrentBehavior()
    {
        // Arrange
        const string input = """
                             using System.Collections.Generic;
                             using System.Threading.Tasks;

                             abstract class PromptBuilder
                             {
                                 private IReadOnlyList<ActionOption<bool>> _options;

                                 public virtual IReadOnlyList<ActionOption<bool>> BuildOptions()
                                 {
                                     return _options ??= [
                                             new ActionOption<bool>
                                 {
                                     Caption = PhraseBook.Lookup("Approve", "Approve"),
                                     Symbol = IconSet.GetAcceptSymbol(RuntimeScope.Engine),
                                     Resolver = () => Task.FromResult(true)
                                 },
                                             new ActionOption<bool>
                                 {
                                     Caption = PhraseBook.Lookup("Reject", "Reject"),
                                     Symbol = IconSet.GetDeclineSymbol(RuntimeScope.Engine),
                                     Resolver = () => Task.FromResult(false)
                                 }
                                         ];
                                 }
                             }

                             sealed class ActionOption<T>
                             {
                                 public string Caption { get; set; }
                                 public object Symbol { get; set; }
                                 public System.Func<Task<T>> Resolver { get; set; }
                             }

                             static class PhraseBook
                             {
                                 public static string Lookup(string key, string fallback) => fallback;
                             }

                             static class IconSet
                             {
                                 public static object GetAcceptSymbol(object engine) => null;
                                 public static object GetDeclineSymbol(object engine) => null;
                             }

                             static class RuntimeScope
                             {
                                 public static object Engine => null;
                             }
                             """;

        const string expected = """
                                using System.Collections.Generic;
                                using System.Threading.Tasks;

                                abstract class PromptBuilder
                                {
                                    private IReadOnlyList<ActionOption<bool>> _options;

                                    public virtual IReadOnlyList<ActionOption<bool>> BuildOptions()
                                    {
                                        return _options ??= [
                                                                new ActionOption<bool>
                                                                {
                                                                    Caption = PhraseBook.Lookup("Approve", "Approve"),
                                                                    Symbol = IconSet.GetAcceptSymbol(RuntimeScope.Engine),
                                                                    Resolver = () => Task.FromResult(true)
                                                                },
                                                                new ActionOption<bool>
                                                                {
                                                                    Caption = PhraseBook.Lookup("Reject", "Reject"),
                                                                    Symbol = IconSet.GetDeclineSymbol(RuntimeScope.Engine),
                                                                    Resolver = () => Task.FromResult(false)
                                                                }
                                                            ];
                                    }
                                }

                                sealed class ActionOption<T>
                                {
                                    public string Caption { get; set; }
                                    public object Symbol { get; set; }
                                    public System.Func<Task<T>> Resolver { get; set; }
                                }

                                static class PhraseBook
                                {
                                    public static string Lookup(string key, string fallback)
                                    {
                                        return fallback;
                                    }
                                }

                                static class IconSet
                                {
                                    public static object GetAcceptSymbol(object engine)
                                    {
                                        return null;
                                    }
                                    public static object GetDeclineSymbol(object engine)
                                    {
                                        return null;
                                    }
                                }

                                static class RuntimeScope
                                {
                                    public static object Engine => null;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a collection expression with nested block lambda, method chains, and object initializers
    /// in a null-coalescing assignment is aligned consistently
    /// </summary>
    [TestMethod]
    public void CollectionExpressionWithNestedBlockLambdaInNullCoalescingAssignmentAlignsConsistently()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Collections.Generic;
                             using System.Threading.Tasks;

                             class WorkflowComposerBase
                             {
                                 private IReadOnlyList<DecisionPacket<bool>> _decisionNodes;

                                 public virtual IReadOnlyList<DecisionPacket<bool>> ComposeNodes()
                                 {
                                     return _decisionNodes ??= [
                                                           new DecisionPacket<bool>
                                                           {
                                                               Marker = GlyphHub.ResolveOkMarker(SessionContext.Channel),
                                                               Handler = () =>
                                                                         {
                                                                             using (var scope = VaultFactory.Open())
                                                                             {
                                                                                 var planKey = FlowState.Read<long>("PlanKey");

                                                                                 if (scope.GetStore<TimelineBlueprintStore>()
                                                                                          .Touch(item => item.Key == planKey, item => item.IsRetired = true))
                                                                                 {
                                                                                     var checkpoint = DateTime.Now;

                                                                                     scope.GetStore<TimelineEntryStore>()
                                                                                          .EraseMany(item => item.BlueprintKey == planKey && item.RecordedAt > checkpoint);
                                                                                 }
                                                                             }

                                                                             return Task.FromResult(true);
                                                                         }
                                                           },
                                                           new DecisionPacket<bool>
                                                           {
                                                               Marker = GlyphHub.ResolveCancelMarker(SessionContext.Channel),
                                                               Handler = () => Task.FromResult(true)
                                                           }
                                                       ];
                                 }
                             }
                             """;

        const string expected = """
                                using System;
                                using System.Collections.Generic;
                                using System.Threading.Tasks;

                                class WorkflowComposerBase
                                {
                                    private IReadOnlyList<DecisionPacket<bool>> _decisionNodes;

                                    public virtual IReadOnlyList<DecisionPacket<bool>> ComposeNodes()
                                    {
                                        return _decisionNodes ??= [
                                                                      new DecisionPacket<bool>
                                                                      {
                                                                          Marker = GlyphHub.ResolveOkMarker(SessionContext.Channel),
                                                                          Handler = () =>
                                                                                    {
                                                                                        using (var scope = VaultFactory.Open())
                                                                                        {
                                                                                            var planKey = FlowState.Read<long>("PlanKey");

                                                                                            if (scope.GetStore<TimelineBlueprintStore>()
                                                                                                     .Touch(item => item.Key == planKey, item => item.IsRetired = true))
                                                                                            {
                                                                                                var checkpoint = DateTime.Now;

                                                                                                scope.GetStore<TimelineEntryStore>()
                                                                                                     .EraseMany(item => item.BlueprintKey == planKey && item.RecordedAt > checkpoint);
                                                                                            }
                                                                                        }

                                                                                        return Task.FromResult(true);
                                                                                    }
                                                                      },
                                                                      new DecisionPacket<bool>
                                                                      {
                                                                          Marker = GlyphHub.ResolveCancelMarker(SessionContext.Channel),
                                                                          Handler = () => Task.FromResult(true)
                                                                      }
                                                                  ];
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a nested statement-lambda argument in a collection-expression/object-initializer/async-lambda
    /// scenario keeps the opening brace aligned to the lambda parameter
    /// </summary>
    [TestMethod]
    public void NestedCollectionInitializerAsyncLambdaKeepsStatementLambdaBraceAtParameter()
    {
        // Arrange
        const string input = """
                             using System.Collections.Generic;
                             using System.Threading.Tasks;

                             class DialogBuilderBase
                             {
                                 private IReadOnlyList<ReactionData<bool>> _reactions;

                                 public IReadOnlyList<ReactionData<bool>> Build()
                                 {
                                     return _reactions ??= [
                                                               new ReactionData<bool>
                                                               {
                                                                   Func = async () =>
                                                                          {
                                                                              var data = await RunSubElement<ScheduleDialogElement, ScheduleData>()
                                                                                         .ConfigureAwait(false);
                                                           
                                                                              using (var dbFactory = RepositoryFactory.CreateInstance())
                                                                              {
                                                                                  dbFactory.GetRepository<ScheduleRepository>()
                                                                                           .Refresh(obj => obj.Id == data.Id, obj =>
                                                                                                                              {
                                                                                                                                  obj.Type = data.Type;
                                                                                                                                  obj.AdditionalData = data.AdditionalData;
                                                                                                                              });
                                                                              }
                                                           
                                                                              return true;
                                                                          }
                                                               }
                                                           ];
                                 }
                             }
                             """;

        const string expected = """
                                using System.Collections.Generic;
                                using System.Threading.Tasks;

                                class DialogBuilderBase
                                {
                                    private IReadOnlyList<ReactionData<bool>> _reactions;

                                    public IReadOnlyList<ReactionData<bool>> Build()
                                    {
                                        return _reactions ??= [
                                                                  new ReactionData<bool>
                                                                  {
                                                                      Func = async () =>
                                                                             {
                                                                                 var data = await RunSubElement<ScheduleDialogElement, ScheduleData>().ConfigureAwait(false);

                                                                                 using (var dbFactory = RepositoryFactory.CreateInstance())
                                                                                 {
                                                                                     dbFactory.GetRepository<ScheduleRepository>()
                                                                                              .Refresh(obj => obj.Id == data.Id, obj =>
                                                                                                                                 {
                                                                                                                                     obj.Type = data.Type;
                                                                                                                                     obj.AdditionalData = data.AdditionalData;
                                                                                                                                 });
                                                                                 }

                                                                                 return true;
                                                                             }
                                                                  }
                                                              ];
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that collection expressions used in constructor base calls remain aligned
    /// </summary>
    [TestMethod]
    public void CollectionExpressionInConstructorBaseCallRemainsAligned()
    {
        // Arrange
        const string input = """
                             using System;

                             class ScheduledBatch : BatchBase
                             {
                                 public ScheduledBatch()
                                     : base([
                                                typeof(HealthCheckJob),
                                                typeof(IndexSyncJob),
                                                typeof(ReportAggregationJob)
                                            ])
                                 {
                                 }
                             }

                             class BatchBase
                             {
                                 public BatchBase(Type[] jobs)
                                 {
                                 }
                             }

                             class HealthCheckJob;
                             class IndexSyncJob;
                             class ReportAggregationJob;
                             """;

        const string expected = input;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that nested collection expressions inside an object initializer
    /// align elements correctly when combined with property assignments and method chains
    /// </summary>
    [TestMethod]
    public void NestedCollectionExpressionsInObjectInitializerAlignCorrectly()
    {
        // Arrange
        const string input = """
                             using System.Linq;

                             class C
                             {
                                 void M(string[] labels, double[] values, double[] trendValues)
                                 {
                                     _dashboardConfig = new DashboardConfig
                                                        {
                                                            Labels = labels,
                                                            Panels = [
                                                                new PanelConfig
                                                                {
                                                                    Title = Localizer.GetText("PrimaryLabel", "Primary"),
                                                                    Values = values,
                                                                    Colors = values.Select(_ => Theme.AccentColor)
                                                                                   .ToArray()
                                                                },
                                                                new PanelConfig
                                                                {
                                                                    Title = Localizer.GetText("TrendLabel", "Trend"),
                                                                    Type = "line",
                                                                    Values = trendValues,
                                                                    BorderColors = ["#ff6384"],
                                                                    BorderWidth = 2,
                                                                    PointRadius = 0,
                                                                    Colors = ["transparent"]
                                                                }
                                                            ]
                                                        };
                                 }
                             }

                             class DashboardConfig
                             {
                                 public string[] Labels { get; set; }

                                 public PanelConfig[] Panels { get; set; }
                             }

                             class PanelConfig
                             {
                                 public string Title { get; set; }

                                 public string Type { get; set; }

                                 public double[] Values { get; set; }

                                 public string[] Colors { get; set; }

                                 public string[] BorderColors { get; set; }

                                 public int BorderWidth { get; set; }

                                 public int PointRadius { get; set; }
                             }

                             static class Localizer
                             {
                                 public static string GetText(string key, string fallback)
                                 {
                                     return fallback;
                                 }
                             }

                             static class Theme
                             {
                                 public static string AccentColor { get; set; }
                             }
                             """;

        const string expected = """
                                using System.Linq;

                                class C
                                {
                                    void M(string[] labels, double[] values, double[] trendValues)
                                    {
                                        _dashboardConfig = new DashboardConfig
                                                           {
                                                               Labels = labels,
                                                               Panels = [
                                                                            new PanelConfig
                                                                            {
                                                                                Title = Localizer.GetText("PrimaryLabel", "Primary"),
                                                                                Values = values,
                                                                                Colors = values.Select(_ => Theme.AccentColor)
                                                                                               .ToArray()
                                                                            },
                                                                            new PanelConfig
                                                                            {
                                                                                Title = Localizer.GetText("TrendLabel", "Trend"),
                                                                                Type = "line",
                                                                                Values = trendValues,
                                                                                BorderColors = ["#ff6384"],
                                                                                BorderWidth = 2,
                                                                                PointRadius = 0,
                                                                                Colors = ["transparent"]
                                                                            }
                                                                        ]
                                                           };
                                    }
                                }

                                class DashboardConfig
                                {
                                    public string[] Labels { get; set; }

                                    public PanelConfig[] Panels { get; set; }
                                }

                                class PanelConfig
                                {
                                    public string Title { get; set; }

                                    public string Type { get; set; }

                                    public double[] Values { get; set; }

                                    public string[] Colors { get; set; }

                                    public string[] BorderColors { get; set; }

                                    public int BorderWidth { get; set; }

                                    public int PointRadius { get; set; }
                                }

                                static class Localizer
                                {
                                    public static string GetText(string key, string fallback)
                                    {
                                        return fallback;
                                    }
                                }

                                static class Theme
                                {
                                    public static string AccentColor { get; set; }
                                }
                                """;

        // Act & Assert — already correctly formatted
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a collection expression start is moved to the line of the assignment operator, and elements are aligned to the opening bracket column
    /// </summary>
    [TestMethod]
    public void CollectionExpressionStartMovesToAssignmentOperatorLineAndElementsAlignToOpeningBracketColumn()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private static readonly string[] _a =
                                 [
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

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a collection expression start with object initializer is moved to the line of the assignment operator, and elements are aligned to the opening bracket column
    /// </summary>
    [TestMethod]
    public void CollectionExpressionWithObjectInitializerStartMovesToAssignmentOperatorLineAndElementsAlignToOpeningBracketColumn()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public List<string> A { get; set; }

                                 private void Test()
                                 {
                                     var a = new C
                                             {
                                                 A = 
                                                 [
                                                 "a",
                                                 "b"
                                                 ]
                                             }
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    public List<string> A { get; set; }

                                    private void Test()
                                    {
                                        var a = new C
                                                {
                                                    A = [
                                                            "a",
                                                            "b"
                                                        ]
                                                }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}