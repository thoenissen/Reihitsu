using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH7309RegionsShouldFollowCategoryOrderAnalyzer"/>
/// </summary>
[TestClass]
public class RH7309RegionsShouldFollowCategoryOrderAnalyzerTests : AnalyzerTestsBase<RH7309RegionsShouldFollowCategoryOrderAnalyzer, RH7309RegionsShouldFollowCategoryOrderCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that custom regions in any order do not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

    #endregion // Tests

    #region Methods

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
}