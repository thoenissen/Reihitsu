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
/// Test methods for <see cref="RH7309RegionsShouldFollowCategoryOrderCodeFixProvider"/> on directives, comments, documentation comments, and line breaks that a move would carry or cross
/// </summary>
[TestClass]
public class RH7309RegionsShouldFollowCategoryOrderTriviaTests : BatchCodeFixTestsBase<RH7309RegionsShouldFollowCategoryOrderAnalyzer, RH7309RegionsShouldFollowCategoryOrderCodeFixProvider>
{
    #region Constants

    /// <summary>
    /// Input of the RH7309-004 specification example, used for the line-ending examples of RH7309-007
    /// </summary>
    private const string LineEndingSource = """
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

    /// <summary>
    /// Result of the RH7309-004 specification example
    /// </summary>
    private const string LineEndingFixedSource = """
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

    #endregion // Constants

    #region Tests

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
    /// Verifies that regions are still reordered when the same conditional directive, whose branch the configuration
    /// compiles, wraps both of them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-002")]
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

        await Verify(testData, fixedData, test => test.SolutionTransforms.Add(DefineDebug), Diagnostics(RH7309RegionsShouldFollowCategoryOrderAnalyzer.DiagnosticId, CreateMessage("Methods")));
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
    /// Verifies the spec example: in a CRLF document whose line break behind a field of a moved block is LF, the
    /// diagnostic is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-007")]
    public async Task NoFixIsOfferedForALineFeedInAMovedBlockOfACarriageReturnLineFeedDocument()
    {
        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(LineEndingSource.Replace("_a;", "_a;LF", System.StringComparison.Ordinal)).Replace("LF\r\n", "\n", System.StringComparison.Ordinal));
    }

    /// <summary>
    /// Verifies that in a CRLF document an LF inside a comment of a moved block withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-007")]
    public async Task NoFixIsOfferedForALineFeedInsideACommentOfAMovedBlock()
    {
        var source = LineEndingSource.Replace("    public void Dispose()", "    /* oneLF\n    two */\n    public void Dispose()", System.StringComparison.Ordinal);

        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(source).Replace("LF\r\n", "\n", System.StringComparison.Ordinal));
    }

    /// <summary>
    /// Verifies the spec example: in a CRLF document whose line break behind the field between the blocks is LF, the fix is
    /// offered, the blocks change places, and every line break stays as it was, that LF included
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-004")]
    [Requirement("RH7309-007")]
    public async Task FixIsOfferedForALineFeedBetweenTheBlocksOfACarriageReturnLineFeedDocument()
    {
        var plainSource = LineEndingSource.Replace("{|RH7309:", string.Empty, System.StringComparison.Ordinal)
                                          .Replace("|}", string.Empty, System.StringComparison.Ordinal);

        var source = NormalizeToCarriageReturnLineFeed(plainSource.Replace("_between;", "_between;LF", System.StringComparison.Ordinal)).Replace("LF\r\n", "\n", System.StringComparison.Ordinal);
        var expected = NormalizeToCarriageReturnLineFeed(LineEndingFixedSource.Replace("_between;", "_between;LF", System.StringComparison.Ordinal)).Replace("LF\r\n", "\n", System.StringComparison.Ordinal);

        Assert.AreEqual(expected, await ApplyCodeFixAsync(source));
    }

    /// <summary>
    /// Verifies the spec example: in a document whose line breaks are all a lone carriage return, the diagnostic is
    /// reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-007")]
    public async Task NoFixIsOfferedInADocumentWhoseLineBreaksAreLoneCarriageReturns()
    {
        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(LineEndingSource, "\r"));
    }

    /// <summary>
    /// Verifies that in a document whose line breaks are all CRLF the fix of the spec example is offered and keeps every
    /// line break CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-004")]
    [Requirement("RH7309-007")]
    public async Task FixIsOfferedInADocumentWhoseLineBreaksAreAllCarriageReturnLineFeed()
    {
        await VerifyCodeFixWithEndOfLine(LineEndingSource, LineEndingFixedSource, "\r\n");
    }

    /// <summary>
    /// Verifies that in a CRLF document an LF in a region block that keeps its place among three blocks does not withhold
    /// the fix, the two other blocks change places, and the LF stays where it was
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-004")]
    [Requirement("RH7309-007")]
    public async Task FixIsOfferedForALineFeedInABlockThatKeepsItsPlaceOfACarriageReturnLineFeedDocument()
    {
        const string source = """
                              using System;

                              public class C : IDisposable
                              {
                                  #region Fields

                                  private int _a;LF

                                  #endregion // Fields

                                  #region IDisposable

                                  public void Dispose()
                                  {
                                  }

                                  #endregion // IDisposable

                                  #region Methods

                                  private void M()
                                  {
                                  }

                                  #endregion // Methods
                              }
                              """;
        const string fixedSource = """
                                   using System;

                                   public class C : IDisposable
                                   {
                                       #region Fields

                                       private int _a;LF

                                       #endregion // Fields

                                       #region Methods

                                       private void M()
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

        var input = NormalizeToCarriageReturnLineFeed(source).Replace("LF\r\n", "\n", System.StringComparison.Ordinal);
        var expected = NormalizeToCarriageReturnLineFeed(fixedSource).Replace("LF\r\n", "\n", System.StringComparison.Ordinal);

        Assert.AreEqual(expected, await ApplyCodeFixAsync(input));
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