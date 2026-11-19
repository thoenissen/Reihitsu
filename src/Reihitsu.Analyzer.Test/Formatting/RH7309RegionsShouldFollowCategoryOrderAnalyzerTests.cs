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
/// Test methods for <see cref="RH7309RegionsShouldFollowCategoryOrderAnalyzer"/> and <see cref="RH7309RegionsShouldFollowCategoryOrderCodeFixProvider"/>: the categories of the regions, which regions are compared, and the order the fix writes
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

    /// <summary>
    /// Specification example of regions in the <c>#if</c> and the <c>#else</c> branch of one conditional directive
    /// </summary>
    private const string RegionsInIfAndElseBranchSource = """
                                                          using System;

                                                          public class C : IDisposable
                                                          {
                                                              public void Dispose()
                                                              {
                                                              }

                                                          #if DEBUG
                                                              #region IDisposable

                                                              private int _a;

                                                              #endregion
                                                          #else
                                                              #region Fields

                                                              private int _b;

                                                              #endregion
                                                          #endif
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
    /// Verifies the specification example in which the regions of the <c>#if</c> and the <c>#else</c> branch of one
    /// conditional directive are not compared, where the <c>#else</c> branch is compiled
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-002")]
    [Requirement("SHARED-TYPEREGION-006")]
    public async Task RegionsInTheIfAndElseBranchOfOneConditionalDirectiveAreNotCompared()
    {
        await Verify(RegionsInIfAndElseBranchSource);
    }

    /// <summary>
    /// Verifies the specification example in which the regions of the <c>#if</c> and the <c>#else</c> branch of one
    /// conditional directive are not compared, where the <c>#if</c> branch is compiled
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-002")]
    [Requirement("SHARED-TYPEREGION-006")]
    public async Task RegionsInTheIfAndElseBranchOfOneConditionalDirectiveAreNotComparedWhereTheSymbolIsDefined()
    {
        await Verify(RegionsInIfAndElseBranchSource, test => test.SolutionTransforms.Add(DefineDebug));
    }

    /// <summary>
    /// Verifies that the regions of the <c>#if</c> and the <c>#elif</c> branch of one conditional directive are not
    /// compared, neither where the first branch is compiled nor where no branch is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-002")]
    public async Task RegionsInTheIfAndElifBranchOfOneConditionalDirectiveAreNotCompared()
    {
        const string testData = """
                                using System;

                                public class C : IDisposable
                                {
                                    public void Dispose()
                                    {
                                    }

                                #if DEBUG
                                    #region IDisposable

                                    private int _a;

                                    #endregion
                                #elif TRACE
                                    #region Fields

                                    private int _b;

                                    #endregion
                                #endif
                                }
                                """;

        await Verify(testData);
        await Verify(testData, test => test.SolutionTransforms.Add(DefineDebug));
    }

    /// <summary>
    /// Verifies the specification example in which a region in the compiled branch after a region outside every branch
    /// is reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-002")]
    [Requirement("RH7309-005")]
    public async Task RegionInTheCompiledBranchAfterARegionOutsideEveryBranchIsReportedWithoutACodeFix()
    {
        const string testData = """
                                using System;

                                public class C : IDisposable
                                {
                                    #region IDisposable

                                    public void Dispose()
                                    {
                                    }

                                    #endregion

                                #if DEBUG
                                    {|RH7309:#region Diagnostics|}

                                    private int _calls;

                                    #endregion
                                #endif
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData, DocumentationMode.Parse, "DEBUG");
    }

    /// <summary>
    /// Verifies the specification example in which a region in an excluded branch after a region outside every branch is
    /// not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-002")]
    public async Task RegionInAnExcludedBranchAfterARegionOutsideEveryBranchIsNotReported()
    {
        const string testData = """
                                using System;

                                public class C : IDisposable
                                {
                                    #region IDisposable

                                    public void Dispose()
                                    {
                                    }

                                    #endregion

                                #if DEBUG
                                    #region Diagnostics

                                    private int _calls;

                                    #endregion
                                #endif
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a region in an excluded branch does not count as an earlier region of a region outside every branch
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-002")]
    public async Task RegionInAnExcludedBranchDoesNotCountAsAnEarlierRegion()
    {
        const string testData = """
                                using System;

                                public class C : IDisposable
                                {
                                #if DEBUG
                                    #region IDisposable

                                    public void Dispose()
                                    {
                                    }

                                    #endregion
                                #else
                                    public void Dispose()
                                    {
                                    }
                                #endif

                                    #region Fields

                                    private int _a;

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a region in the compiled branch counts as an earlier region of a region outside every branch
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-002")]
    public async Task RegionInTheCompiledBranchCountsAsAnEarlierRegion()
    {
        const string testData = """
                                using System;

                                public class C : IDisposable
                                {
                                #if DEBUG
                                    #region IDisposable

                                    public void Dispose()
                                    {
                                    }

                                    #endregion
                                #else
                                    public void Dispose()
                                    {
                                    }
                                #endif

                                    {|RH7309:#region Fields|}

                                    private int _a;

                                    #endregion
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData, DocumentationMode.Parse, "DEBUG");
    }

    /// <summary>
    /// Verifies the specification example in which a region in an excluded branch is a block whose conditional directive
    /// lies outside it, so a later region is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-004")]
    [Requirement("RH7309-005")]
    public async Task NoCodeFixWhenARegionInAnExcludedBranchStandsInFrontOfTheReportedRegions()
    {
        const string testData = """
                                using System;

                                public class C : IDisposable
                                {
                                #if DEBUG
                                    #region Diagnostics

                                    private int _calls;

                                    #endregion
                                #endif

                                    #region IDisposable

                                    public void Dispose()
                                    {
                                    }

                                    #endregion

                                    {|RH7309:#region Fields|}

                                    private int _a;

                                    #endregion
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the region of a nested enum belongs to the enum and is not compared with the regions of the type
    /// that holds it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-002")]
    [Requirement("SHARED-TYPEREGION-006")]
    public async Task RegionOfANestedEnumIsNotComparedWithTheRegionsOfTheType()
    {
        const string testData = """
                                using System;

                                public class C : IDisposable
                                {
                                    #region IDisposable

                                    public void Dispose()
                                    {
                                    }

                                    #endregion

                                    private enum E
                                    {
                                        #region Fields
                                        A,
                                        #endregion
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that two regions inside a nested enum are not top-level regions of the type that holds the enum, so they
    /// are neither reported nor reordered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-002")]
    [Requirement("SHARED-TYPEREGION-006")]
    public async Task RegionsInsideANestedEnumAreNotReported()
    {
        const string testData = """
                                using System;

                                public class C : IDisposable
                                {
                                    public void Dispose()
                                    {
                                    }

                                    private enum E
                                    {
                                        #region IDisposable
                                        A,
                                        #endregion

                                        #region Values
                                        B,
                                        #endregion
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the code fix reorders the top-level region blocks of a type around a nested enum with a region of
    /// its own, and leaves the enum and its region in place
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-002")]
    [Requirement("RH7309-004")]
    [Requirement("SHARED-TYPEREGION-006")]
    public async Task CodeFixReordersTheBlocksOfTheTypeAroundANestedEnumWithARegion()
    {
        const string testData = """
                                using System;

                                public class C : IDisposable
                                {
                                    #region IDisposable

                                    public void Dispose()
                                    {
                                    }

                                    #endregion

                                    private enum E
                                    {
                                        #region Values
                                        A,
                                        #endregion
                                    }

                                    {|RH7309:#region Fields|}

                                    private int _a;

                                    #endregion
                                }
                                """;
        const string fixedData = """
                                 using System;

                                 public class C : IDisposable
                                 {
                                     #region Fields

                                     private int _a;

                                     #endregion

                                     private enum E
                                     {
                                         #region Values
                                         A,
                                         #endregion
                                     }

                                     #region IDisposable

                                     public void Dispose()
                                     {
                                     }

                                     #endregion
                                 }
                                 """;

        await Verify(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the code fix reorders the blocks of the type when the reported block starts with a nested class,
    /// and leaves the nested class untouched
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-004")]
    [Requirement("SHARED-TYPEREGION-006")]
    public async Task CodeFixReordersTheTypeWhenTheReportedBlockStartsWithANestedClass()
    {
        const string testData = """
                                using System;

                                public class C : IDisposable
                                {
                                    #region IDisposable

                                    public void Dispose()
                                    {
                                    }

                                    #endregion

                                    {|RH7309:#region Fields|}

                                    private class Other
                                    {
                                    }

                                    private int _a;

                                    #endregion
                                }
                                """;
        const string fixedData = """
                                 using System;

                                 public class C : IDisposable
                                 {
                                     #region Fields

                                     private class Other
                                     {
                                     }

                                     private int _a;

                                     #endregion

                                     #region IDisposable

                                     public void Dispose()
                                     {
                                     }

                                     #endregion
                                 }
                                 """;

        await Verify(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the code fix reorders the blocks of the type when the reported block starts with a nested record
    /// that carries an attribute list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-004")]
    public async Task CodeFixReordersTheTypeWhenTheReportedBlockStartsWithAnAttributedNestedRecord()
    {
        const string testData = """
                                using System;

                                public class C : IDisposable
                                {
                                    #region IDisposable

                                    public void Dispose()
                                    {
                                    }

                                    #endregion

                                    {|RH7309:#region Fields|}

                                    [Serializable]
                                    private record R(int A);

                                    private int _a;

                                    #endregion
                                }
                                """;
        const string fixedData = """
                                 using System;

                                 public class C : IDisposable
                                 {
                                     #region Fields

                                     [Serializable]
                                     private record R(int A);

                                     private int _a;

                                     #endregion

                                     #region IDisposable

                                     public void Dispose()
                                     {
                                     }

                                     #endregion
                                 }
                                 """;

        await Verify(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the code fix reorders the blocks of the type when the reported block starts with a nested class
    /// whose own regions are in order, and leaves the regions of the nested class in place
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-004")]
    [Requirement("SHARED-FIX-005")]
    [Requirement("SHARED-TYPEREGION-006")]
    public async Task CodeFixReordersTheTypeWhenTheReportedBlockStartsWithANestedClassWhoseRegionsAreInOrder()
    {
        const string testData = """
                                using System;

                                public class C : IDisposable
                                {
                                    #region IDisposable

                                    public void Dispose()
                                    {
                                    }

                                    #endregion

                                    {|RH7309:#region Fields|}

                                    private class Other : IDisposable
                                    {
                                        #region Fields

                                        private int _b;

                                        #endregion

                                        #region IDisposable

                                        public void Dispose()
                                        {
                                        }

                                        #endregion
                                    }

                                    private int _a;

                                    #endregion
                                }
                                """;
        const string fixedData = """
                                 using System;

                                 public class C : IDisposable
                                 {
                                     #region Fields

                                     private class Other : IDisposable
                                     {
                                         #region Fields

                                         private int _b;

                                         #endregion

                                         #region IDisposable

                                         public void Dispose()
                                         {
                                         }

                                         #endregion
                                     }

                                     private int _a;

                                     #endregion

                                     #region IDisposable

                                     public void Dispose()
                                     {
                                     }

                                     #endregion
                                 }
                                 """;

        await Verify(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the code fix for the diagnostic of the outer type reorders the blocks of the outer type only, when
    /// the reported block starts with a nested class whose own regions are out of order
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-004")]
    [Requirement("SHARED-TYPEREGION-006")]
    public async Task CodeFixForTheOuterTypeLeavesTheRegionsOfANestedClassAtTheStartOfTheReportedBlockInPlace()
    {
        const string testData = """
                                using System;

                                public class C : IDisposable
                                {
                                    #region IDisposable

                                    public void Dispose()
                                    {
                                    }

                                    #endregion

                                    {|RH7309:#region Fields|}

                                    private class Other : IDisposable
                                    {
                                        #region IDisposable

                                        public void Dispose()
                                        {
                                        }

                                        #endregion

                                        {|RH7309:#region Fields|}

                                        private int _b;

                                        #endregion
                                    }

                                    private int _a;

                                    #endregion
                                }
                                """;
        const string expected = """
                                using System;

                                public class C : IDisposable
                                {
                                    #region Fields

                                    private class Other : IDisposable
                                    {
                                        #region IDisposable

                                        public void Dispose()
                                        {
                                        }

                                        #endregion

                                        #region Fields

                                        private int _b;

                                        #endregion
                                    }

                                    private int _a;

                                    #endregion

                                    #region IDisposable

                                    public void Dispose()
                                    {
                                    }

                                    #endregion
                                }
                                """;

        await Verify(testData);

        var source = RemoveMarkup(testData);
        var fixedSource = await TryApplyCodeFixAtAsync(source,
                                                       RH7309RegionsShouldFollowCategoryOrderAnalyzer.DiagnosticId,
                                                       root => root.DescendantNodes(descendIntoTrivia: true)
                                                                   .OfType<RegionDirectiveTriviaSyntax>()
                                                                   .ElementAt(1)
                                                                   .GetLocation());

        Assert.AreEqual(expected, fixedSource);
    }

    /// <summary>
    /// Verifies the union example of RH7309-002: a custom region after an interface region of a union is reported on
    /// its region directive, and the fix orders the regions as in a struct
    /// </summary>
    /// <param name="endOfLine">Line break of the source and the expected result</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-002")]
    [Requirement("RH7309-003")]
    [Requirement("RH7309-004")]
    [DataRow("\n", DisplayName = "LF")]
    [DataRow("\r\n", DisplayName = "CRLF")]
    public async Task UnionCustomRegionAfterInterfaceRegionIsReportedAndReordered(string endOfLine)
    {
        const string testCode = """
                                using System;

                                public union U(int, string) : IComparable
                                {
                                    #region IComparable

                                    public int CompareTo(object o)
                                    {
                                        return 0;
                                    }

                                    #endregion // IComparable

                                    {|RH7309:#region Methods|}

                                    public void M()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 public union U(int, string) : IComparable
                                 {
                                     #region Methods

                                     public void M()
                                     {
                                     }

                                     #endregion // Methods

                                     #region IComparable

                                     public int CompareTo(object o)
                                     {
                                         return 0;
                                     }

                                     #endregion // IComparable
                                 }
                                 """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(ConvertEndOfLine(testCode, endOfLine), ConvertEndOfLine(fixedCode, endOfLine), test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Defines the preprocessor symbol <c>DEBUG</c> for the test project
    /// </summary>
    /// <param name="solution">Solution</param>
    /// <param name="projectId">Project ID</param>
    /// <returns>Solution</returns>
    private static Solution DefineDebug(Solution solution, ProjectId projectId)
    {
        return solution.GetProject(projectId)?.ParseOptions is CSharpParseOptions parseOptions
                   ? solution.WithProjectParseOptions(projectId, parseOptions.WithPreprocessorSymbols("DEBUG"))
                   : solution;
    }

    /// <summary>
    /// Creates the expected diagnostic message for the given region description
    /// </summary>
    /// <param name="regionDescription">Region description</param>
    /// <returns>Diagnostic message</returns>
    private static string CreateMessage(string regionDescription)
    {
        return string.Format(MessageFormat, regionDescription);
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