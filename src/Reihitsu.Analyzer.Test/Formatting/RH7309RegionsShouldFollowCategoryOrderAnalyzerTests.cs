using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH7309RegionsShouldFollowCategoryOrderAnalyzer"/>
/// </summary>
[TestClass]
public class RH7309RegionsShouldFollowCategoryOrderAnalyzerTests : BatchCodeFixTestsBase<RH7309RegionsShouldFollowCategoryOrderAnalyzer, RH7309RegionsShouldFollowCategoryOrderCodeFixProvider>
{
    #region Constants

    /// <summary>
    /// Specification example of moved code, whose region blocks hold a layout the formatter changes
    /// </summary>
    private const string MovedCodeSource = """
                                           using System;

                                           public class C : IDisposable
                                           {
                                               #region IDisposable

                                               public void Dispose() { }

                                               #endregion // IDisposable

                                               {|RH7309:#region Fields|}

                                               private int _a;


                                               private int _b;

                                               #endregion // Fields
                                           }
                                           """;

    /// <summary>
    /// Expected result of fixing <see cref="MovedCodeSource"/>
    /// </summary>
    private const string MovedCodeFixedSource = """
                                                using System;

                                                public class C : IDisposable
                                                {
                                                    #region Fields

                                                    private int _a;


                                                    private int _b;

                                                    #endregion // Fields

                                                    #region IDisposable

                                                    public void Dispose() { }

                                                    #endregion // IDisposable
                                                }
                                                """;

    #endregion // Constants

    #region Tests

    /// <summary>
    /// Verifies that custom regions in any order do not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-002")]
    public async Task VerifyNoDiagnosticForCustomRegionsOnly()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Fields

                                    private int _value;

                                    #endregion // Fields

                                    #region Properties

                                    public int Value => _value;

                                    #endregion // Properties
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a single region does not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-002")]
    public async Task VerifyNoDiagnosticForSingleRegion()
    {
        const string testData = """
                                internal interface IWidget
                                {
                                    void Render();
                                }

                                internal class Widget : IWidget
                                {
                                    #region IWidget

                                    public void Render()
                                    {
                                    }

                                    #endregion // IWidget
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the canonical order of custom, base-type and interface regions does not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-001")]
    [Requirement("RH7309-002")]
    public async Task VerifyNoDiagnosticForCanonicalOrder()
    {
        const string testData = """
                                internal interface IWidget
                                {
                                    void Render();
                                }

                                internal class WidgetBase
                                {
                                    public virtual void Reset()
                                    {
                                    }
                                }

                                internal class Widget : WidgetBase, IWidget
                                {
                                    #region Methods

                                    public void Refresh()
                                    {
                                    }

                                    #endregion // Methods

                                    #region WidgetBase

                                    public override void Reset()
                                    {
                                    }

                                    #endregion // WidgetBase

                                    #region IWidget

                                    public void Render()
                                    {
                                    }

                                    #endregion // IWidget
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a custom region followed by an interface region does not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-002")]
    public async Task VerifyNoDiagnosticForCustomThenInterfaceRegion()
    {
        const string testData = """
                                internal interface IWidget
                                {
                                    void Render();
                                }

                                internal class Widget : IWidget
                                {
                                    #region Methods

                                    public void Refresh()
                                    {
                                    }

                                    #endregion // Methods

                                    #region IWidget

                                    public void Render()
                                    {
                                    }

                                    #endregion // IWidget
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an interface region placed before a custom region produces a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-002")]
    [Requirement("RH7309-003")]
    public async Task VerifyDiagnosticForInterfaceRegionBeforeCustomRegion()
    {
        const string testData = """
                                internal interface IWidget
                                {
                                    void Render();
                                }

                                internal class Widget : IWidget
                                {
                                    #region IWidget

                                    public void Render()
                                    {
                                    }

                                    #endregion // IWidget

                                    {|#0:#region Methods|}

                                    public void Refresh()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;
        const string fixedData = """
                                 internal interface IWidget
                                 {
                                     void Render();
                                 }

                                 internal class Widget : IWidget
                                 {
                                     #region Methods

                                     public void Refresh()
                                     {
                                     }

                                     #endregion // Methods

                                     #region IWidget

                                     public void Render()
                                     {
                                     }

                                     #endregion // IWidget
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH7309RegionsShouldFollowCategoryOrderAnalyzer.DiagnosticId, CreateMessage("Methods")));
    }

    /// <summary>
    /// Verifies that an interface region placed before a base-type override region produces a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-001")]
    [Requirement("RH7309-002")]
    [Requirement("RH7309-003")]
    public async Task VerifyDiagnosticForInterfaceRegionBeforeBaseTypeRegion()
    {
        const string testData = """
                                internal interface IWidget
                                {
                                    void Render();
                                }

                                internal class WidgetBase
                                {
                                    public virtual void Reset()
                                    {
                                    }
                                }

                                internal class Widget : WidgetBase, IWidget
                                {
                                    #region IWidget

                                    public void Render()
                                    {
                                    }

                                    #endregion // IWidget

                                    {|#0:#region WidgetBase|}

                                    public override void Reset()
                                    {
                                    }

                                    #endregion // WidgetBase
                                }
                                """;
        const string fixedData = """
                                 internal interface IWidget
                                 {
                                     void Render();
                                 }

                                 internal class WidgetBase
                                 {
                                     public virtual void Reset()
                                     {
                                     }
                                 }

                                 internal class Widget : WidgetBase, IWidget
                                 {
                                     #region WidgetBase

                                     public override void Reset()
                                     {
                                     }

                                     #endregion // WidgetBase

                                     #region IWidget

                                     public void Render()
                                     {
                                     }

                                     #endregion // IWidget
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH7309RegionsShouldFollowCategoryOrderAnalyzer.DiagnosticId, CreateMessage("WidgetBase")));
    }

    /// <summary>
    /// Verifies that a base-type override region placed before a custom region produces a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-001")]
    [Requirement("RH7309-002")]
    public async Task VerifyDiagnosticForBaseTypeRegionBeforeCustomRegion()
    {
        const string testData = """
                                internal class WidgetBase
                                {
                                    public virtual void Reset()
                                    {
                                    }
                                }

                                internal class Widget : WidgetBase
                                {
                                    #region WidgetBase

                                    public override void Reset()
                                    {
                                    }

                                    #endregion // WidgetBase

                                    {|#0:#region Methods|}

                                    public void Refresh()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;
        const string fixedData = """
                                 internal class WidgetBase
                                 {
                                     public virtual void Reset()
                                     {
                                     }
                                 }

                                 internal class Widget : WidgetBase
                                 {
                                     #region Methods

                                     public void Refresh()
                                     {
                                     }

                                     #endregion // Methods

                                     #region WidgetBase

                                     public override void Reset()
                                     {
                                     }

                                     #endregion // WidgetBase
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH7309RegionsShouldFollowCategoryOrderAnalyzer.DiagnosticId, CreateMessage("Methods")));
    }

    /// <summary>
    /// Verifies that both an out-of-order base-type region and an out-of-order custom region are reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-002")]
    public async Task VerifyDiagnosticsForInterfaceRegionBeforeBaseTypeAndCustomRegions()
    {
        const string testData = """
                                internal interface IWidget
                                {
                                    void Render();
                                }

                                internal class WidgetBase
                                {
                                    public virtual void Reset()
                                    {
                                    }
                                }

                                internal class Widget : WidgetBase, IWidget
                                {
                                    #region IWidget

                                    public void Render()
                                    {
                                    }

                                    #endregion // IWidget

                                    {|#0:#region WidgetBase|}

                                    public override void Reset()
                                    {
                                    }

                                    #endregion // WidgetBase

                                    {|#1:#region Methods|}

                                    public void Refresh()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        await Verify(testData,
                     Diagnostics(RH7309RegionsShouldFollowCategoryOrderAnalyzer.DiagnosticId,
                                 index => CreateMessage(index == 0 ? "WidgetBase" : "Methods"),
                                 2));
    }

    /// <summary>
    /// Verifies that all three region categories are reordered into the canonical order by the code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-004")]
    public async Task VerifyCodeFixReordersAllCategories()
    {
        const string testData = """
                                internal interface IWidget
                                {
                                    void Render();
                                }

                                internal class WidgetBase
                                {
                                    public virtual void Reset()
                                    {
                                    }
                                }

                                internal class Widget : WidgetBase, IWidget
                                {
                                    #region Methods

                                    public void Refresh()
                                    {
                                    }

                                    #endregion // Methods

                                    #region IWidget

                                    public void Render()
                                    {
                                    }

                                    #endregion // IWidget

                                    {|#0:#region WidgetBase|}

                                    public override void Reset()
                                    {
                                    }

                                    #endregion // WidgetBase
                                }
                                """;
        const string fixedData = """
                                 internal interface IWidget
                                 {
                                     void Render();
                                 }

                                 internal class WidgetBase
                                 {
                                     public virtual void Reset()
                                     {
                                     }
                                 }

                                 internal class Widget : WidgetBase, IWidget
                                 {
                                     #region Methods

                                     public void Refresh()
                                     {
                                     }

                                     #endregion // Methods

                                     #region WidgetBase

                                     public override void Reset()
                                     {
                                     }

                                     #endregion // WidgetBase

                                     #region IWidget

                                     public void Render()
                                     {
                                     }

                                     #endregion // IWidget
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH7309RegionsShouldFollowCategoryOrderAnalyzer.DiagnosticId, CreateMessage("WidgetBase")));
    }

    /// <summary>
    /// Verifies that no code fix is offered when a conditional directive is improperly nested with the region
    /// directives, opening inside one region and closing inside another
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-005")]
    public async Task VerifyNoCodeFixWhenConditionalDirectiveIsImproperlyNestedWithRegions()
    {
        const string testData = """
                                internal interface IWidget
                                {
                                    void Render();
                                }

                                internal class Widget : IWidget
                                {
                                    #region IWidget

                                    public void Render()
                                    {
                                    }
                                #if DEBUG

                                    #endregion // IWidget

                                    #region Methods

                                    public void Refresh()
                                    {
                                    }
                                #endif

                                    #endregion // Methods
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH7309RegionsShouldFollowCategoryOrderAnalyzer.DiagnosticId,
                                                   GetLastRegionDirectiveLocation,
                                                   "DEBUG");

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that no code fix is offered when a conditional directive wraps only the later of the two regions
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-005")]
    public async Task VerifyNoCodeFixWhenConditionalDirectiveWrapsOnlyTheLaterRegion()
    {
        const string testData = """
                                internal interface IWidget
                                {
                                    void Render();
                                }

                                internal class Widget : IWidget
                                {
                                    #region IWidget

                                    public void Render()
                                    {
                                    }

                                    #endregion // IWidget

                                #if DEBUG
                                    #region Methods

                                    public void Refresh()
                                    {
                                    }

                                    #endregion // Methods
                                #endif
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH7309RegionsShouldFollowCategoryOrderAnalyzer.DiagnosticId,
                                                   GetLastRegionDirectiveLocation,
                                                   "DEBUG");

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that no code fix is offered when a conditional directive wraps only the earlier of the two regions
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-005")]
    public async Task VerifyNoCodeFixWhenConditionalDirectiveWrapsOnlyTheEarlierRegion()
    {
        const string testData = """
                                internal interface IWidget
                                {
                                    void Render();
                                }

                                internal class Widget : IWidget
                                {
                                #if DEBUG
                                    #region IWidget

                                    public void Render()
                                    {
                                    }

                                    #endregion // IWidget
                                #endif

                                    #region Methods

                                    public void Refresh()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH7309RegionsShouldFollowCategoryOrderAnalyzer.DiagnosticId,
                                                   GetLastRegionDirectiveLocation,
                                                   "DEBUG");

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that regions are still reordered when the same conditional directive wraps both of them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-005")]
    public async Task VerifyCodeFixReordersRegionsWrappedByTheSameConditionalDirective()
    {
        const string testData = """
                                internal class WidgetBase
                                {
                                    public virtual void Reset()
                                    {
                                    }
                                }

                                internal class Widget : WidgetBase
                                {
                                #if DEBUG
                                    #region WidgetBase

                                    public override void Reset()
                                    {
                                    }

                                    #endregion // WidgetBase

                                    {|#0:#region Methods|}

                                    public void Refresh()
                                    {
                                    }

                                    #endregion // Methods
                                #endif
                                }
                                """;
        const string fixedData = """
                                 internal class WidgetBase
                                 {
                                     public virtual void Reset()
                                     {
                                     }
                                 }

                                 internal class Widget : WidgetBase
                                 {
                                 #if DEBUG
                                     #region Methods

                                     public void Refresh()
                                     {
                                     }

                                     #endregion // Methods

                                     #region WidgetBase

                                     public override void Reset()
                                     {
                                     }

                                     #endregion // WidgetBase
                                 #endif
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH7309RegionsShouldFollowCategoryOrderAnalyzer.DiagnosticId, CreateMessage("Methods")));
    }

    /// <summary>
    /// Verifies that regions are still reordered when a conditional directive pair sits completely inside one region
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-005")]
    public async Task VerifyCodeFixReordersRegionsContainingBalancedConditionalDirectives()
    {
        const string testData = """
                                internal interface IWidget
                                {
                                    void Render();
                                }

                                internal class Widget : IWidget
                                {
                                    #region IWidget

                                    public void Render()
                                    {
                                    }

                                    #endregion // IWidget

                                    {|#0:#region Methods|}

                                #if DEBUG
                                    public void Refresh()
                                    {
                                    }
                                #endif

                                    #endregion // Methods
                                }
                                """;
        const string fixedData = """
                                 internal interface IWidget
                                 {
                                     void Render();
                                 }

                                 internal class Widget : IWidget
                                 {
                                     #region Methods

                                 #if DEBUG
                                     public void Refresh()
                                     {
                                     }
                                 #endif

                                     #endregion // Methods

                                     #region IWidget

                                     public void Render()
                                     {
                                     }

                                     #endregion // IWidget
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH7309RegionsShouldFollowCategoryOrderAnalyzer.DiagnosticId, CreateMessage("Methods")));
    }

    /// <summary>
    /// Verifies that regions are still reordered when a conditional directive pair sits completely between two regions
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-005")]
    public async Task VerifyCodeFixReordersRegionsWithBalancedConditionalDirectivesBetweenThem()
    {
        const string testData = """
                                internal interface IWidget
                                {
                                    void Render();
                                }

                                internal class Widget : IWidget
                                {
                                    #region IWidget

                                    public void Render()
                                    {
                                    }

                                    #endregion // IWidget

                                #if DEBUG
                                    public void Trace()
                                    {
                                    }
                                #endif

                                    {|#0:#region Methods|}

                                    public void Refresh()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;
        const string fixedData = """
                                 internal interface IWidget
                                 {
                                     void Render();
                                 }

                                 internal class Widget : IWidget
                                 {
                                     #region Methods

                                     public void Refresh()
                                     {
                                     }

                                     #endregion // Methods

                                 #if DEBUG
                                     public void Trace()
                                     {
                                     }
                                 #endif

                                     #region IWidget

                                     public void Render()
                                     {
                                     }

                                     #endregion // IWidget
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH7309RegionsShouldFollowCategoryOrderAnalyzer.DiagnosticId, CreateMessage("Methods")));
    }

    /// <summary>
    /// Verifies that no code fix is offered when a region contains a pragma warning directive, because the swap
    /// would change which code the suppression covers
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-005")]
    public async Task VerifyNoCodeFixWhenRegionContainsPragmaWarningDirective()
    {
        const string testData = """
                                internal interface IWidget
                                {
                                    void Render();
                                }

                                internal class Widget : IWidget
                                {
                                    #region IWidget

                                #pragma warning disable CS0169

                                    public void Render()
                                    {
                                    }

                                    #endregion // IWidget

                                    #region Methods

                                    private int _unused;

                                    #endregion // Methods
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH7309RegionsShouldFollowCategoryOrderAnalyzer.DiagnosticId,
                                                   GetLastRegionDirectiveLocation);

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that no code fix is offered when a region contains a nullable directive, because the swap would
    /// change the annotation context of the moved members
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-005")]
    public async Task VerifyNoCodeFixWhenRegionContainsNullableDirective()
    {
        const string testData = """
                                internal interface IWidget
                                {
                                    void Render();
                                }

                                internal class Widget : IWidget
                                {
                                    #region IWidget

                                #nullable enable

                                    public void Render()
                                    {
                                    }

                                    #endregion // IWidget

                                    #region Methods

                                    public void Refresh()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH7309RegionsShouldFollowCategoryOrderAnalyzer.DiagnosticId,
                                                   GetLastRegionDirectiveLocation);

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that no code fix is offered when a pragma warning directive sits between the two regions
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-005")]
    public async Task VerifyNoCodeFixWhenPragmaWarningDirectiveSitsBetweenRegions()
    {
        const string testData = """
                                internal interface IWidget
                                {
                                    void Render();
                                }

                                internal class Widget : IWidget
                                {
                                    #region IWidget

                                    public void Render()
                                    {
                                    }

                                    #endregion // IWidget

                                #pragma warning disable CS0169

                                    #region Methods

                                    private int _unused;

                                    #endregion // Methods
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH7309RegionsShouldFollowCategoryOrderAnalyzer.DiagnosticId,
                                                   GetLastRegionDirectiveLocation);

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that regions are still reordered when a pragma warning directive sits ahead of every reordered
    /// region, because the swap does not change what the suppression covers
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-005")]
    public async Task VerifyCodeFixReordersRegionsFollowingAPragmaWarningDirective()
    {
        const string testData = """
                                internal interface IWidget
                                {
                                    void Render();
                                }

                                internal class Widget : IWidget
                                {
                                #pragma warning disable CS0169

                                    #region IWidget

                                    public void Render()
                                    {
                                    }

                                    #endregion // IWidget

                                    {|#0:#region Methods|}

                                    private int _unused;

                                    #endregion // Methods
                                }
                                """;
        const string fixedData = """
                                 internal interface IWidget
                                 {
                                     void Render();
                                 }

                                 internal class Widget : IWidget
                                 {
                                 #pragma warning disable CS0169

                                     #region Methods

                                     private int _unused;

                                     #endregion // Methods

                                     #region IWidget

                                     public void Render()
                                     {
                                     }

                                     #endregion // IWidget
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH7309RegionsShouldFollowCategoryOrderAnalyzer.DiagnosticId, CreateMessage("Methods")));
    }

    /// <summary>
    /// Verifies that a region named after an implemented interface is an interface region, one named after a base type a base-type region, and any other a custom region
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-001")]
    [Requirement("RH7309-002")]
    [Requirement("RH7309-003")]
    [Requirement("SHARED-TYPEREGION-006")]
    public async Task RegionCategoriesComeFromTheDescription()
    {
        const string testData = """
                                using System;

                                public class C : IComparable<C>
                                {
                                    #region IComparable

                                    public int CompareTo(C other) => 0;

                                    #endregion // IComparable

                                    {|RH7309:#region Object|}

                                    public override string ToString() => "";

                                    #endregion // Object

                                    {|RH7309:#region Methods|}

                                    public void M()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a description differing from the interface name in case, or no description, gives the custom category
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-001")]
    [Requirement("SHARED-REGDESC-001")]
    public async Task DescriptionDifferingInCaseOrMissingIsCustom()
    {
        const string testData = """
                                using System;

                                public class C : IDisposable
                                {
                                    #region IDisposable

                                    public void Dispose()
                                    {
                                    }

                                    #endregion // IDisposable

                                    {|RH7309:#region idisposable|}

                                    private int _a;

                                    #endregion // idisposable

                                    {|RH7309:#region|}

                                    private int _b;

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a custom region after an interface region of a struct is reported on its region directive
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-002")]
    [Requirement("RH7309-003")]
    public async Task CustomRegionAfterInterfaceRegionInStructIsReported()
    {
        const string testData = """
                                using System;

                                public struct S : IDisposable
                                {
                                    #region IDisposable

                                    public void Dispose()
                                    {
                                    }

                                    #endregion // IDisposable

                                    {|RH7309:#region Methods|}

                                    public void M()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the regions of an interface declaration are not checked
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-002")]
    public async Task RegionsOfAnInterfaceAreNotChecked()
    {
        const string testData = """
                                using System;

                                public interface I : IDisposable
                                {
                                    #region IDisposable

                                    void Close();

                                    #endregion // IDisposable

                                    #region Methods

                                    void M();

                                    #endregion // Methods
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the code fix orders the region blocks by category and keeps the code between them in place
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-004")]
    public async Task CodeFixOrdersRegionBlocksAndKeepsCodeBetweenThemInPlace()
    {
        const string testData = """
                                using System;

                                public class C : IDisposable
                                {
                                    #region IDisposable

                                    public void Dispose()
                                    {
                                    }

                                    #endregion // IDisposable

                                    private int _between;

                                    {|RH7309:#region Fields|}

                                    private int _a;

                                    #endregion // Fields
                                }
                                """;

        const string fixedData = """
                                 using System;

                                 public class C : IDisposable
                                 {
                                     #region Fields

                                     private int _a;

                                     #endregion // Fields

                                     private int _between;

                                     #region IDisposable

                                     public void Dispose()
                                     {
                                     }

                                     #endregion // IDisposable
                                 }
                                 """;

        await Verify(testData, fixedData);
    }

    /// <summary>
    /// Verifies that one application of the code fix corrects every diagnostic of the type, keeping the written order within a category
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-004")]
    public async Task OneApplicationCorrectsEveryDiagnosticOfTheType()
    {
        const string testData = """
                                using System;

                                public class C : IComparable<C>
                                {
                                    #region IComparable

                                    public int CompareTo(C other) => 0;

                                    #endregion // IComparable

                                    {|RH7309:#region Object|}

                                    public override string ToString() => "";

                                    #endregion // Object

                                    {|RH7309:#region Methods|}

                                    public void M()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        const string fixedData = """
                                 using System;

                                 public class C : IComparable<C>
                                 {
                                     #region Methods

                                     public void M()
                                     {
                                     }

                                     #endregion // Methods

                                     #region Object

                                     public override string ToString() => "";

                                     #endregion // Object

                                     #region IComparable

                                     public int CompareTo(C other) => 0;

                                     #endregion // IComparable
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     test =>
                     {
                         test.NumberOfIncrementalIterations = 1;
                         test.NumberOfFixAllIterations = 1;
                     });
    }

    /// <summary>
    /// Verifies that no code fix is offered when a pragma directive would be moved across, while the diagnostic is still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-005")]
    public async Task NoCodeFixWhenAPragmaDirectiveWouldBeMovedAcross()
    {
        const string testData = """
                                using System;

                                public class C : IDisposable
                                {
                                    #region IDisposable

                                    public void Dispose()
                                    {
                                    }

                                    #endregion // IDisposable

                                #pragma warning disable CS0169
                                    {|RH7309:#region Fields|}

                                    private int _a;

                                    #endregion // Fields
                                #pragma warning restore CS0169
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a conditional pair that lies wholly inside one block does not withhold the code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-005")]
    public async Task BalancedConditionalPairInsideABlockDoesNotWithholdTheFix()
    {
        const string testData = """
                                using System;

                                public class C : IDisposable
                                {
                                    #region IDisposable

                                    public void Dispose()
                                    {
                                #if DEBUG
                                        Console.WriteLine();
                                #endif
                                    }

                                    #endregion // IDisposable

                                    {|RH7309:#region Methods|}

                                    public void M()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        const string fixedData = """
                                 using System;

                                 public class C : IDisposable
                                 {
                                     #region Methods

                                     public void M()
                                     {
                                     }

                                     #endregion // Methods

                                     #region IDisposable

                                     public void Dispose()
                                     {
                                 #if DEBUG
                                         Console.WriteLine();
                                 #endif
                                     }

                                     #endregion // IDisposable
                                 }
                                 """;

        await Verify(testData, fixedData);
    }

    /// <summary>
    /// Verifies the specification example of moved code: the region blocks change places as they were written, so the two
    /// blank lines between the fields and the braces of the method on its line stay, although formatting the result
    /// would change both
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-007")]
    public async Task MovedRegionBlocksKeepTheirLayout()
    {
        Assert.AreNotEqual(MovedCodeFixedSource, Format(MovedCodeFixedSource), "The example must hold a layout that formatting changes.");

        await Verify(MovedCodeSource, MovedCodeFixedSource);
    }

    /// <summary>
    /// Verifies the specification example of moved code in a document whose line breaks are CRLF: the region blocks
    /// change places as they were written, and every line break stays CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-007")]
    public async Task MovedRegionBlocksKeepTheirLayoutInCarriageReturnLineFeedDocument()
    {
        await Verify(NormalizeToCarriageReturnLineFeed(MovedCodeSource), NormalizeToCarriageReturnLineFeed(MovedCodeFixedSource));
    }

    /// <summary>
    /// Verifies the specification example: moving the block would run a static initializer before the one that reads its
    /// field, so no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-006")]
    public async Task NoCodeFixWhenAnInitializerReadsAFieldOfABlockItWouldCross()
    {
        const string source = """
                              using System;

                              public class C : IComparable
                              {
                                  #region IComparable

                                  private static readonly int Y = X + 1;

                                  public int CompareTo(object obj) => Y;

                                  #endregion // IComparable

                                  {|RH7309:#region Fields|}

                                  private static readonly int X = 5;

                                  #endregion // Fields
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that the fix is withheld when the reorder would put a static initializer of a block in front of a static
    /// initializer between the blocks that runs before it today
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-006")]
    public async Task NoCodeFixWhenAnInitializerWouldChangeOrderWithAMemberBetweenTheBlocks()
    {
        const string source = """
                              using System;

                              public class C : IDisposable
                              {
                                  #region IDisposable

                                  private static readonly int Y = X + 1;

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  private static readonly int X = 5;

                                  {|RH7309:#region Fields|}

                                  private static int _z = 1;

                                  #endregion // Fields
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that the fix is withheld when the reorder would swap two instance field initializers
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-006")]
    public async Task NoCodeFixWhenTwoInstanceInitializersWouldChangeOrder()
    {
        const string source = """
                              using System;

                              public class C : IDisposable
                              {
                                  #region IDisposable

                                  private int _a = 1;

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Fields|}

                                  private int _b = 2;

                                  #endregion // Fields
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that the fix is withheld when the reorder would swap the initializer of an event field and the initializer
    /// of a property
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-006")]
    public async Task NoCodeFixWhenAnEventInitializerAndAPropertyInitializerWouldChangeOrder()
    {
        const string source = """
                              using System;

                              public class C : IDisposable
                              {
                                  #region IDisposable

                                  public event EventHandler Changed = delegate { };

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Fields|}

                                  public int Count { get; set; } = 1;

                                  #endregion // Fields
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that the fix is withheld when the reorder would swap an initializer that another build configuration
    /// compiles with an initializer of the other block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task NoCodeFixWhenAnInitializerInDisabledTextWouldChangeOrder()
    {
        const string source = """
                              using System;

                              public class C : IComparable
                              {
                                  #region IComparable

                              #if DEBUG
                                  private static readonly int Y = X + 1;
                              #endif

                                  public int CompareTo(object obj) => 0;

                                  #endregion // IComparable

                                  {|RH7309:#region Fields|}

                                  private static readonly int X = 5;

                                  #endregion // Fields
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a static and an instance initializer, which run at different times, do not withhold the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-006")]
    public async Task CodeFixReordersAStaticAndAnInstanceInitializer()
    {
        const string source = """
                              using System;

                              public class C : IDisposable
                              {
                                  #region IDisposable

                                  private static int _s = 1;

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Fields|}

                                  private int _i = 2;

                                  #endregion // Fields
                              }
                              """;

        const string fixedSource = """
                                   using System;

                                   public class C : IDisposable
                                   {
                                       #region Fields

                                       private int _i = 2;

                                       #endregion // Fields

                                       #region IDisposable

                                       private static int _s = 1;

                                       public void Dispose()
                                       {
                                       }

                                       #endregion // IDisposable
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies that constant fields, which have no runtime initializer, do not withhold the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-006")]
    public async Task CodeFixReordersConstantFields()
    {
        const string source = """
                              using System;

                              public class C : IDisposable
                              {
                                  #region IDisposable

                                  private const int A = 1;

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Fields|}

                                  private const int B = A;

                                  #endregion // Fields
                              }
                              """;

        const string fixedSource = """
                                   using System;

                                   public class C : IDisposable
                                   {
                                       #region Fields

                                       private const int B = A;

                                       #endregion // Fields

                                       #region IDisposable

                                       private const int A = 1;

                                       public void Dispose()
                                       {
                                       }

                                       #endregion // IDisposable
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies that initializers of nested types in the blocks, which belong to other contexts, do not withhold the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-006")]
    public async Task CodeFixReordersInitializersOfNestedTypes()
    {
        const string source = """
                              using System;

                              public class C : IDisposable
                              {
                                  #region IDisposable

                                  private class Inner
                                  {
                                      public int A = 1;
                                  }

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Fields|}

                                  public void M()
                                  {
                                  }

                                  private class Other
                                  {
                                      public int B = 2;
                                  }

                                  #endregion // Fields
                              }
                              """;

        const string fixedSource = """
                                   using System;

                                   public class C : IDisposable
                                   {
                                       #region Fields

                                       public void M()
                                       {
                                       }

                                       private class Other
                                       {
                                           public int B = 2;
                                       }

                                       #endregion // Fields

                                       #region IDisposable

                                       private class Inner
                                       {
                                           public int A = 1;
                                       }

                                       public void Dispose()
                                       {
                                       }

                                       #endregion // IDisposable
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies the specification example: moving the block would change the order of the struct's instance fields, so
    /// no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-006")]
    public async Task NoCodeFixForTheInstanceFieldsOfAStruct()
    {
        const string source = """
                              using System;

                              public struct S : IDisposable
                              {
                                  #region IDisposable

                                  private int _handle;

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Fields|}

                                  private int _a;

                                  #endregion // Fields
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies the specification example: the backing field of an auto-property counts like a declared field of the
    /// struct, so no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-006")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoCodeFixForAnAutoPropertyOfAStruct()
    {
        const string source = """
                              using System;

                              public struct S : IDisposable
                              {
                                  #region IDisposable

                                  public int Handle { get; set; }

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Fields|}

                                  private int _a;

                                  #endregion // Fields
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that the backing field of an instance field-like event counts like a declared field of the struct, so no
    /// code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-006")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoCodeFixForAnInstanceFieldLikeEventOfAStruct()
    {
        const string source = """
                              using System;

                              public struct S : IDisposable
                              {
                                  #region IDisposable

                                  public event EventHandler Changed;

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Fields|}

                                  private int _a;

                                  #endregion // Fields
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that the backing field of a property that uses the <c>field</c> keyword counts like a declared field of
    /// the struct, so no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-006")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoCodeFixForAPropertyThatUsesTheFieldKeywordOfAStruct()
    {
        const string source = """
                              using System;

                              public struct S : IDisposable
                              {
                                  #region IDisposable

                                  public int Handle { get => field; set => field = value; }

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Fields|}

                                  private int _a;

                                  #endregion // Fields
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that the fix is withheld when the reorder would change the order of the instance fields of a class
    /// declared with sequential layout
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-006")]
    public async Task NoCodeFixForTheInstanceFieldsOfAClassWithSequentialLayout()
    {
        const string source = """
                              using System;
                              using System.Runtime.InteropServices;

                              [StructLayout(LayoutKind.Sequential)]
                              public class C : IDisposable
                              {
                                  #region IDisposable

                                  private int _handle;

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Fields|}

                                  private int _a;

                                  #endregion // Fields
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies the specification example: a comment directly above the <c>#region</c> line of a block the fix would move
    /// withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-006")]
    public async Task NoCodeFixWhenACommentStandsDirectlyAboveABlockThatWouldMove()
    {
        const string source = """
                              using System;

                              public class C : IDisposable
                              {
                                  // The disposal logic
                                  #region IDisposable

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Fields|}

                                  private int _a;

                                  #endregion // Fields
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a comment directly above the <c>#region</c> line of a block the fix would move withholds the fix in a
    /// document with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-006")]
    public async Task NoCodeFixWhenACommentStandsDirectlyAboveABlockThatWouldMoveInCarriageReturnLineFeedDocument()
    {
        var source = NormalizeToCarriageReturnLineFeed("""
                                                       using System;

                                                       public class C : IDisposable
                                                       {
                                                           // The disposal logic
                                                           #region IDisposable

                                                           public void Dispose()
                                                           {
                                                           }

                                                           #endregion // IDisposable

                                                           {|RH7309:#region Fields|}

                                                           private int _a;

                                                           #endregion // Fields
                                                       }
                                                       """);

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies the specification example: a comment above the <c>#region</c> line of a block the fix would move, with a
    /// blank line between them, withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-006")]
    public async Task NoCodeFixWhenACommentAboveABlockThatWouldMoveIsSeparatedByABlankLine()
    {
        const string source = """
                              using System;

                              public class C : IDisposable
                              {
                                  // The disposal logic

                                  #region IDisposable

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Fields|}

                                  private int _a;

                                  #endregion // Fields
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies the specification example: a comment above the <c>#region</c> line of a block that keeps its place does
    /// not withhold the fix and stays above that block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-004")]
    [Requirement("RH7309-006")]
    public async Task CodeFixKeepsACommentAboveABlockThatKeepsItsPlace()
    {
        const string source = """
                              using System;

                              public class C : IDisposable
                              {
                                  // The fields
                                  #region Fields

                                  private int _a;

                                  #endregion // Fields

                                  #region IDisposable

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Methods|}

                                  public void M()
                                  {
                                  }

                                  #endregion // Methods
                              }
                              """;

        const string fixedSource = """
                                   using System;

                                   public class C : IDisposable
                                   {
                                       // The fields
                                       #region Fields

                                       private int _a;

                                       #endregion // Fields

                                       #region Methods

                                       public void M()
                                       {
                                       }

                                       #endregion // Methods

                                       #region IDisposable

                                       public void Dispose()
                                       {
                                       }

                                       #endregion // IDisposable
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies the specification example: a documentation comment behind the closing brace of the last member of a block
    /// documents the first member of the next block, so no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-006")]
    public async Task NoCodeFixWhenADocumentationCommentBehindTheLastCodeOfABlockDocumentsTheNextBlock()
    {
        const string source = """
                              using System;

                              public class C : IDisposable
                              {
                                  #region IDisposable

                                  public void Dispose()
                                  {
                                  } /// <summary>M</summary>

                                  #endregion // IDisposable

                                  {|RH7309:#region Methods|}

                                  public void M()
                                  {
                                  }

                                  #endregion // Methods
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies the specification example: a documentation comment above a <c>#region</c> line documents the first
    /// member of the block, so no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-006")]
    public async Task NoCodeFixWhenADocumentationCommentAboveTheRegionLineDocumentsTheBlock()
    {
        const string source = """
                              using System;

                              public class C : IDisposable
                              {
                                  #region IDisposable

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  /// <summary>M</summary>
                                  {|RH7309:#region Methods|}

                                  public void M()
                                  {
                                  }

                                  #endregion // Methods
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies the specification example: a documentation comment above an <c>#endregion</c> line documents the first
    /// member of the next block, so no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-006")]
    public async Task NoCodeFixWhenADocumentationCommentAboveTheEndRegionLineDocumentsTheNextBlock()
    {
        const string source = """
                              using System;

                              public class C : IDisposable
                              {
                                  #region IDisposable

                                  public void Dispose()
                                  {
                                  }

                                  /// <summary>M</summary>
                                  #endregion // IDisposable

                                  {|RH7309:#region Methods|}

                                  public void M()
                                  {
                                  }

                                  #endregion // Methods
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a documentation comment above a <c>#region</c> line withholds the fix in the documentation mode
    /// Diagnose as well
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-006")]
    public async Task NoCodeFixWhenADocumentationCommentAboveTheRegionLineDocumentsTheBlockInDocumentationModeDiagnose()
    {
        const string source = """
                              using System;

                              public class C : IDisposable
                              {
                                  #region IDisposable

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  /// <summary>M</summary>
                                  {|RH7309:#region Methods|}

                                  public void M()
                                  {
                                  }

                                  #endregion // Methods
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source, DocumentationMode.Diagnose);
    }

    /// <summary>
    /// Verifies the specification example in the documentation mode None: the comment behind the closing brace is an
    /// ordinary comment that moves with its block, and the fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-006")]
    public async Task CodeFixMovesAnOrdinaryCommentBehindTheLastCodeOfABlockWithTheBlockInDocumentationModeNone()
    {
        const string source = """
                              using System;

                              public class C : IDisposable
                              {
                                  #region IDisposable

                                  public void Dispose()
                                  {
                                  } /// <summary>M</summary>

                                  #endregion // IDisposable

                                  {|RH7309:#region Methods|}

                                  public void M()
                                  {
                                  }

                                  #endregion // Methods
                              }
                              """;

        const string fixedSource = """
                                   using System;

                                   public class C : IDisposable
                                   {
                                       #region Methods

                                       public void M()
                                       {
                                       }

                                       #endregion // Methods

                                       #region IDisposable

                                       public void Dispose()
                                       {
                                       } /// <summary>M</summary>

                                       #endregion // IDisposable
                                   }
                                   """;

        await Verify(source, fixedSource, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    /// <summary>
    /// Verifies that the fix is withheld for the instance fields of a partial class whose sequential layout another part
    /// declares
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-006")]
    public async Task NoCodeFixForTheInstanceFieldsOfAPartialClassWithSequentialLayoutOnAnotherPart()
    {
        const string source = """
                              using System;
                              using System.Runtime.InteropServices;

                              [StructLayout(LayoutKind.Sequential)]
                              public partial class P
                              {
                              }

                              public partial class P : IDisposable
                              {
                                  #region IDisposable

                                  private int _handle;

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Fields|}

                                  private int _a;

                                  #endregion // Fields
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that the fix reorders the instance fields of a partial struct whose automatic layout another part
    /// declares
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-006")]
    public async Task CodeFixReordersTheInstanceFieldsOfAPartialStructWithAutoLayoutOnAnotherPart()
    {
        const string source = """
                              using System;
                              using System.Runtime.InteropServices;

                              [StructLayout(LayoutKind.Auto)]
                              public partial struct S
                              {
                              }

                              public partial struct S : IDisposable
                              {
                                  #region IDisposable

                                  private int _handle;

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Fields|}

                                  private int _a;

                                  #endregion // Fields
                              }
                              """;

        const string fixedSource = """
                                   using System;
                                   using System.Runtime.InteropServices;

                                   [StructLayout(LayoutKind.Auto)]
                                   public partial struct S
                                   {
                                   }

                                   public partial struct S : IDisposable
                                   {
                                       #region Fields

                                       private int _a;

                                       #endregion // Fields

                                       #region IDisposable

                                       private int _handle;

                                       public void Dispose()
                                       {
                                       }

                                       #endregion // IDisposable
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies that a property that reads a member named <c>field</c> before C# 14 has no backing field, so the reorder
    /// keeps the order of the struct's instance fields and the fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-006")]
    [Requirement("SHARED-FIX-003")]
    public async Task CodeFixReordersAStructWhosePropertyReadsAMemberNamedFieldBeforeCSharp14()
    {
        const string source = """
                              using System;

                              public struct S : IDisposable
                              {
                                  #region IDisposable

                                  public int P
                                  {
                                      get
                                      {
                                          return field;
                                      }
                                  }

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Fields|}

                                  private static int field;

                                  private int _a;

                                  #endregion // Fields
                              }
                              """;

        const string fixedSource = """
                                   using System;

                                   public struct S : IDisposable
                                   {
                                       #region Fields

                                       private static int field;

                                       private int _a;

                                       #endregion // Fields

                                       #region IDisposable

                                       public int P
                                       {
                                           get
                                           {
                                               return field;
                                           }
                                       }

                                       public void Dispose()
                                       {
                                       }

                                       #endregion // IDisposable
                                   }
                                   """;

        await Verify(source,
                     fixedSource,
                     test => test.SolutionTransforms.Add((solution, projectId) => solution.GetProject(projectId)?.ParseOptions is CSharpParseOptions parseOptions
                                                                                      ? solution.WithProjectParseOptions(projectId, parseOptions.WithLanguageVersion(LanguageVersion.CSharp13))
                                                                                      : solution));
    }

    /// <summary>
    /// Verifies that a documentation comment above the <c>#endregion</c> line of the last block, which documents nothing,
    /// withholds the fix when the block moves: behind the moved block, it would document the first member of the block
    /// that follows
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-006")]
    public async Task NoCodeFixWhenADocumentationCommentAboveTheLastEndRegionLineWouldDocumentAMember()
    {
        const string source = """
                              using System;

                              public class C : IDisposable
                              {
                                  #region IDisposable

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Methods|}

                                  public void M()
                                  {
                                  }

                                  /// <summary>Stray</summary>
                                  #endregion // Methods
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a documentation comment behind the last code of the last block, which documents nothing, withholds
    /// the fix when the block moves: in front of the block that follows it, it would document that block's first member
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-006")]
    public async Task NoCodeFixWhenADocumentationCommentBehindTheLastCodeOfTheLastBlockWouldDocumentAMember()
    {
        const string source = """
                              using System;

                              public class C : IDisposable
                              {
                                  #region IDisposable

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Methods|}

                                  public void M()
                                  {
                                  } /// <summary>Stray</summary>

                                  #endregion // Methods
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a documentation comment above the <c>#endregion</c> line of a last block that keeps its place, which
    /// documents nothing before and after the reorder, does not withhold the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-006")]
    public async Task CodeFixKeepsADocumentationCommentThatDocumentsNothingInALastBlockThatKeepsItsPlace()
    {
        const string source = """
                              using System;

                              public class C : IDisposable, IComparable
                              {
                                  #region IDisposable

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Fields|}

                                  private int _a;

                                  #endregion // Fields

                                  #region IComparable

                                  public int CompareTo(object obj)
                                  {
                                      return 0;
                                  }

                                  /// <summary>Stray</summary>
                                  #endregion // IComparable
                              }
                              """;

        const string fixedSource = """
                                   using System;

                                   public class C : IDisposable, IComparable
                                   {
                                       #region Fields

                                       private int _a;

                                       #endregion // Fields

                                       #region IDisposable

                                       public void Dispose()
                                       {
                                       }

                                       #endregion // IDisposable

                                       #region IComparable

                                       public int CompareTo(object obj)
                                       {
                                           return 0;
                                       }

                                       /// <summary>Stray</summary>
                                       #endregion // IComparable
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies that a field of the moved block whose initializer only a build with <c>DEBUG</c> compiles counts as
    /// initialized: the reorder would run it before the initializer of the other block in that build, so no code fix is
    /// offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7309-006")]
    [Requirement("SHARED-ORDER-007")]
    public async Task NoCodeFixWhenAFieldOfTheMovedBlockHasAnInitializerOnlyInAnotherConfiguration(string endOfLine)
    {
        const string source = """
                              using System;

                              public class C : IDisposable
                              {
                                  #region IDisposable

                                  private static readonly int A = Log("a");

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Fields|}

                                  private static readonly int B
                              #if DEBUG
                                      = Log("b")
                              #endif
                                      ;

                                  #endregion // Fields

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a further member which excluded code inside a field of the moved block declares counts: a build with
    /// <c>DEBUG</c> declares the initialized field <c>Z</c> there, so the reorder would run its initializer before the
    /// initializer of the other block, and no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7309-006")]
    [Requirement("SHARED-ORDER-007")]
    public async Task NoCodeFixWhenExcludedCodeInsideAFieldOfTheMovedBlockDeclaresAnInitializedField(string endOfLine)
    {
        const string source = """
                              using System;

                              public class C : IDisposable
                              {
                                  #region IDisposable

                                  private static readonly int A = Log("a");

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Fields|}

                                  private static readonly int B
                              #if DEBUG
                                      ;
                                  private static readonly int Z = Log("z")
                              #endif
                                      ;

                                  #endregion // Fields

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a property initializer which only a build with <c>DEBUG</c> compiles behind the accessor list of a
    /// property of the moved block counts, although the excluded text does not form a member on its own
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7309-006")]
    [Requirement("SHARED-ORDER-007")]
    public async Task NoCodeFixWhenAPropertyOfTheMovedBlockHasAnInitializerBehindItsAccessorListOnlyInAnotherConfiguration(string endOfLine)
    {
        const string source = """
                              using System;

                              public class C : IDisposable
                              {
                                  #region IDisposable

                                  private static readonly int A = Log("a");

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Fields|}

                                  private static int B { get; }
                              #if DEBUG
                                      = Log("b");
                              #endif

                                  #endregion // Fields

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies the shared specification example: a field of a struct that only a build without <c>DEBUG</c> declares
    /// <c>static</c> is an instance field in a build with <c>DEBUG</c>, so the reorder would change the layout of that
    /// build, and no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7309-006")]
    [Requirement("SHARED-ORDER-007")]
    public async Task NoCodeFixWhenAFieldOfTheMovedBlockIsAnInstanceFieldOfAStructOnlyInAnotherConfiguration(string endOfLine)
    {
        const string source = """
                              using System;

                              public struct S : IDisposable
                              {
                                  #region IDisposable

                                  private int _a;

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Fields|}

                                  private
                              #if !DEBUG
                                      static
                              #endif
                                      int _b;

                                  #endregion // Fields
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies the other side of the split <c>static</c>: a field of a struct that only a build with <c>DEBUG</c>
    /// declares <c>static</c> is an instance field in the active code already, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7309-006")]
    public async Task NoCodeFixWhenAFieldOfTheMovedBlockIsAnInstanceFieldOfAStructInTheActiveCode(string endOfLine)
    {
        const string source = """
                              using System;

                              public struct S : IDisposable
                              {
                                  #region IDisposable

                                  private int _a;

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Fields|}

                                  private
                              #if DEBUG
                                      static
                              #endif
                                      int _b;

                                  #endregion // Fields
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a field between the region blocks, which the reorder crosses, counts with the initializer only a
    /// build with <c>DEBUG</c> compiles, so the moved block's initialized field would run before it in that build
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7309-006")]
    [Requirement("SHARED-ORDER-007")]
    public async Task NoCodeFixWhenACrossedFieldBetweenTheBlocksHasAnInitializerOnlyInAnotherConfiguration(string endOfLine)
    {
        const string source = """
                              using System;

                              public class C : IDisposable
                              {
                                  #region IDisposable

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  private static readonly int G
                              #if DEBUG
                                      = Log("g")
                              #endif
                                      ;

                                  {|RH7309:#region Fields|}

                                  private static readonly int A = Log("a");

                                  #endregion // Fields

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that excluded code at the start of the moved block that declares an initialized field counts, also when
    /// it ends in the head of the active field: the reorder would run <c>Log("x")</c> before <c>Log("a")</c> in a build
    /// with <c>DEBUG</c>, so no code fix is offered
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7309-006")]
    [Requirement("SHARED-ORDER-007")]
    public async Task NoCodeFixWhenExcludedCodeThatEndsInTheHeadOfAFieldDeclaresAnInitializedField(string endOfLine)
    {
        const string source = """
                              using System;

                              public class C : IDisposable
                              {
                                  #region IDisposable

                                  private static readonly int A = Log("a");

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Fields|}

                              #if DEBUG
                                  private static readonly int X = Log("x");
                                  private static readonly int Y
                              #else
                                  private static readonly int Y
                              #endif
                                      ;

                                  #endregion // Fields

                                  private static int Log(string name)
                                  {
                                      return 0;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(WithEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies the shared specification example: a member split across <c>#if DEBUG</c> and <c>#else</c> keeps the move
    /// when no configuration gives a moved member and a crossed member of one context an initializer each
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7309-006")]
    [Requirement("SHARED-ORDER-007")]
    public async Task CodeFixMovesABlockWithAMemberSplitAcrossConfigurationsWhenNothingInitializedIsCrossed(string endOfLine)
    {
        const string source = """
                              using System;

                              public class C : IDisposable
                              {
                                  #region IDisposable

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  {|RH7309:#region Fields|}

                              #if DEBUG
                                  private static readonly int X = Log("x");
                                  private static readonly int Y =
                              #else
                                  private static readonly int Y =
                              #endif
                                      Log("y");

                                  #endregion // Fields

                                  private static int Log(string name) => 0;
                              }
                              """;

        const string fixedSource = """
                                   using System;

                                   public class C : IDisposable
                                   {
                                       #region Fields

                                   #if DEBUG
                                       private static readonly int X = Log("x");
                                       private static readonly int Y =
                                   #else
                                       private static readonly int Y =
                                   #endif
                                           Log("y");

                                       #endregion // Fields

                                       #region IDisposable

                                       public void Dispose()
                                       {
                                       }

                                       #endregion // IDisposable

                                       private static int Log(string name) => 0;
                                   }
                                   """;

        await Verify(WithEndOfLine(source, endOfLine), WithEndOfLine(fixedSource, endOfLine));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Writes the provided text with the given line break
    /// </summary>
    /// <param name="text">Text</param>
    /// <param name="endOfLine">Line break</param>
    /// <returns>The text with every line break replaced by <paramref name="endOfLine"/></returns>
    private static string WithEndOfLine(string text, string endOfLine)
    {
        return text.Replace("\r\n", "\n").Replace("\n", endOfLine);
    }

    /// <summary>
    /// Gets the location of the last <c>#region</c> directive in the syntax tree
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <returns>Location of the last region directive</returns>
    private static Location GetLastRegionDirectiveLocation(SyntaxNode root)
    {
        return root.DescendantNodes(descendIntoTrivia: true)
                   .OfType<RegionDirectiveTriviaSyntax>()
                   .Last()
                   .GetLocation();
    }

    /// <summary>
    /// Creates the expected diagnostic message for the given region description
    /// </summary>
    /// <param name="regionDescription">Region description</param>
    /// <returns>Diagnostic message</returns>
    private static string CreateMessage(string regionDescription)
    {
        return string.Format(AnalyzerResources.RH7309MessageFormat, regionDescription);
    }

    #endregion // Methods

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                internal interface IWidget
                                {
                                    void Render();
                                }

                                internal class WidgetBase
                                {
                                    public virtual void Reset()
                                    {
                                    }
                                }

                                internal class Widget : WidgetBase, IWidget
                                {
                                    #region IWidget

                                    public void Render()
                                    {
                                    }

                                    #endregion // IWidget

                                    {|#0:#region WidgetBase|}

                                    public override void Reset()
                                    {
                                    }

                                    #endregion // WidgetBase

                                    {|#1:#region Methods|}

                                    public void Refresh()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;
        const string fixedData = """
                                 internal interface IWidget
                                 {
                                     void Render();
                                 }

                                 internal class WidgetBase
                                 {
                                     public virtual void Reset()
                                     {
                                     }
                                 }

                                 internal class Widget : WidgetBase, IWidget
                                 {
                                     #region Methods

                                     public void Refresh()
                                     {
                                     }

                                     #endregion // Methods

                                     #region WidgetBase

                                     public override void Reset()
                                     {
                                     }

                                     #endregion // WidgetBase

                                     #region IWidget

                                     public void Render()
                                     {
                                     }

                                     #endregion // IWidget
                                 }
                                 """;

        // Both diagnostics register the fix that reorders every top-level region of the same type into
        // canonical order, so the two computed fixes are byte-for-byte identical text changes: the strongest
        // form of interference in this batch, where the second fix is discarded as a duplicate of the first
        // and the already-canonical result still has to satisfy the scenario in one pass
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH7309RegionsShouldFollowCategoryOrderAnalyzer.DiagnosticId,
                                              index => CreateMessage(index == 0 ? "WidgetBase" : "Methods"),
                                              2));
    }

    #endregion // BatchCodeFixTestsBase
}