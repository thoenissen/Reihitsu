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
    /// Verifies that a conditional directive pair that sits completely inside a region whose place the fix changes
    /// withholds the fix, while the diagnostic is still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-005")]
    [Requirement("SHARED-ORDER-008")]
    public async Task VerifyNoCodeFixWhenAMovedRegionContainsBalancedConditionalDirectives()
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

                                    {|RH7309:#region Methods|}

                                #if DEBUG
                                    public void Refresh()
                                    {
                                    }
                                #endif

                                    #endregion // Methods
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a conditional directive pair that sits completely between two regions, in code that a moved region
    /// crosses, withholds the fix, while the diagnostic is still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-005")]
    [Requirement("SHARED-ORDER-008")]
    public async Task VerifyNoCodeFixWhenAMovedRegionCrossesBalancedConditionalDirectivesBetweenRegions()
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

                                    {|RH7309:#region Methods|}

                                    public void Refresh()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
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
    /// Verifies the RH7309-005 example: a conditional pair that lies wholly inside a block whose place the fix changes
    /// withholds the fix, while the diagnostic is still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-005")]
    [Requirement("SHARED-ORDER-008")]
    public async Task ConditionalPairInsideABlockWhosePlaceChangesWithholdsTheFix()
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

        await VerifyNoCodeFixIsOffered(testData);
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

    /// <summary>
    /// Verifies the RH7309-005 example of a conditional pair inside a block that keeps its place: the pair in
    /// <c>IComparable</c> does not withhold the fix, which moves <c>Methods</c> in front of <c>IDisposable</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-005")]
    [Requirement("SHARED-ORDER-008")]
    public async Task ConditionalPairInsideABlockThatKeepsItsPlaceDoesNotWithholdTheFix()
    {
        const string testData = """
                                using System;

                                public class C : IDisposable, IComparable
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

                                    #region IComparable

                                    public int CompareTo(object obj)
                                    {
                                #if DEBUG
                                        Console.WriteLine();
                                #endif
                                        return 0;
                                    }

                                    #endregion // IComparable
                                }
                                """;

        const string fixedData = """
                                 using System;

                                 public class C : IDisposable, IComparable
                                 {
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

                                     #region IComparable

                                     public int CompareTo(object obj)
                                     {
                                 #if DEBUG
                                         Console.WriteLine();
                                 #endif
                                         return 0;
                                     }

                                     #endregion // IComparable
                                 }
                                 """;

        await Verify(testData, fixedData);
    }

    /// <summary>
    /// Verifies that a conditional group in code between two blocks that no moved block crosses does not withhold the
    /// fix: the group stays between the second and the third block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-005")]
    [Requirement("SHARED-ORDER-008")]
    public async Task ConditionalGroupBetweenBlocksThatNoMovedBlockCrossesDoesNotWithholdTheFix()
    {
        const string testData = """
                                using System;

                                public class C : IDisposable, IComparable
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

                                #if DEBUG
                                    public void Trace()
                                    {
                                    }
                                #endif

                                    #region IComparable

                                    public int CompareTo(object obj)
                                    {
                                        return 0;
                                    }

                                    #endregion // IComparable
                                }
                                """;

        const string fixedData = """
                                 using System;

                                 public class C : IDisposable, IComparable
                                 {
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

                                 #if DEBUG
                                     public void Trace()
                                     {
                                     }
                                 #endif

                                     #region IComparable

                                     public int CompareTo(object obj)
                                     {
                                         return 0;
                                     }

                                     #endregion // IComparable
                                 }
                                 """;

        await Verify(testData, fixedData);
    }

    /// <summary>
    /// Verifies that a conditional group inside a block that keeps its place withholds the fix when the reorder moves
    /// blocks across that block, while every diagnostic is still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-005")]
    [Requirement("SHARED-ORDER-008")]
    public async Task ConditionalPairInsideAKeptBlockThatMovedBlocksCrossWithholdsTheFix()
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

                                    {|RH7309:#region Object|}

                                    public override string ToString()
                                    {
                                #if DEBUG
                                        Console.WriteLine();
                                #endif
                                        return string.Empty;
                                    }

                                    #endregion // Object

                                    {|RH7309:#region Methods|}

                                    public void M()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a conditional group in the code between two pairs of blocks that are each reordered on their own
    /// side does not withhold the fix: no block is moved across that code, so the group stays where it is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-005")]
    [Requirement("SHARED-ORDER-008")]
    public async Task ConditionalGroupBetweenTwoPairsReorderedOnTheirOwnSideDoesNotWithholdTheFix()
    {
        const string testData = """
                                using System;

                                public class Base
                                {
                                }

                                public class C : Base, IDisposable
                                {
                                    #region Object

                                    public override string ToString()
                                    {
                                        return string.Empty;
                                    }

                                    #endregion // Object

                                    {|RH7309:#region Methods|}

                                    public void M()
                                    {
                                    }

                                    #endregion // Methods

                                #if DEBUG
                                    public void Trace()
                                    {
                                    }
                                #endif

                                    #region IDisposable

                                    public void Dispose()
                                    {
                                    }

                                    #endregion // IDisposable

                                    {|RH7309:#region Base|}

                                    public void Reset()
                                    {
                                    }

                                    #endregion // Base
                                }
                                """;

        const string fixedData = """
                                 using System;

                                 public class Base
                                 {
                                 }

                                 public class C : Base, IDisposable
                                 {
                                     #region Methods

                                     public void M()
                                     {
                                     }

                                     #endregion // Methods

                                     #region Object

                                     public override string ToString()
                                     {
                                         return string.Empty;
                                     }

                                     #endregion // Object

                                 #if DEBUG
                                     public void Trace()
                                     {
                                     }
                                 #endif

                                     #region Base

                                     public void Reset()
                                     {
                                     }

                                     #endregion // Base

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
    /// Verifies that the same conditional group, in the code between the first two blocks of a type whose two pairs of
    /// blocks are each reordered on their own side, withholds the fix: the reorder moves a block across that code
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-005")]
    [Requirement("SHARED-ORDER-008")]
    public async Task ConditionalGroupInCodeThatAMovedBlockCrossesBetweenTwoReorderedPairsWithholdsTheFix()
    {
        const string testData = """
                                using System;

                                public class Base
                                {
                                }

                                public class C : Base, IDisposable
                                {
                                    #region Object

                                    public override string ToString()
                                    {
                                        return string.Empty;
                                    }

                                    #endregion // Object

                                #if DEBUG
                                    public void Trace()
                                    {
                                    }
                                #endif

                                    {|RH7309:#region Methods|}

                                    public void M()
                                    {
                                    }

                                    #endregion // Methods

                                    #region IDisposable

                                    public void Dispose()
                                    {
                                    }

                                    #endregion // IDisposable

                                    {|RH7309:#region Base|}

                                    public void Reset()
                                    {
                                    }

                                    #endregion // Base
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies the SHARED-ORDER-008 place in the reordered body in front of the moved fields for RH7309: a group of whole
    /// members in front of the first region block makes the class count as one whose field order sets its layout, so no
    /// code fix is offered for a reorder that puts <c>_a</c> in front of <c>_handle</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-005")]
    [Requirement("SHARED-ORDER-008")]
    public async Task NoCodeFixWhenAGroupInFrontOfTheFirstBlockPrecedesAReorderOfInstanceFields()
    {
        const string testData = """
                                using System;

                                public class C : IDisposable
                                {
                                #if DEBUG
                                    private int _trace;
                                #endif

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

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies the SHARED-ORDER-008 place in the reordered body in front of the moved fields for RH7309 when the fix reorders
    /// two separate runs of blocks: a group between the runs, which closes the class and opens a struct in another
    /// build, stands in front of the instance fields the second run reorders, so no code fix is offered for a reorder
    /// that puts <c>_b</c> in front of <c>_d</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-005")]
    [Requirement("SHARED-ORDER-008")]
    public async Task NoCodeFixWhenAGroupBetweenTwoReorderedRunsPrecedesAReorderOfInstanceFieldsThatOpensAStruct()
    {
        const string testData = """
                                using System;

                                public class Base
                                {
                                }

                                public class C : Base, IDisposable
                                {
                                    #region Object

                                    public override string ToString()
                                    {
                                        return string.Empty;
                                    }

                                    #endregion // Object

                                    {|RH7309:#region Methods|}

                                    public void M()
                                    {
                                    }

                                    #endregion // Methods

                                #if X
                                }

                                public struct S
                                {
                                #endif

                                    #region IDisposable

                                    private int _d;

                                    public void Dispose()
                                    {
                                    }

                                    #endregion // IDisposable

                                    {|RH7309:#region Base|}

                                    private int _b;

                                    #endregion // Base
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies the SHARED-ORDER-008 place in the reordered body in front of the moved fields for RH7309 when the fix reorders
    /// two separate runs of blocks, the RH7309-005 example: a group of a whole method between the runs stands in front of
    /// the instance fields the second run reorders, so no code fix is offered for a reorder that puts <c>_b</c> in front
    /// of <c>_d</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-005")]
    [Requirement("SHARED-ORDER-008")]
    public async Task NoCodeFixWhenAGroupOfMembersBetweenTwoReorderedRunsPrecedesAReorderOfInstanceFields()
    {
        const string testData = """
                                using System;

                                public class Base
                                {
                                }

                                public class C : Base, IDisposable
                                {
                                    #region Object

                                    public override string ToString()
                                    {
                                        return string.Empty;
                                    }

                                    #endregion // Object

                                    {|RH7309:#region Methods|}

                                    public void M()
                                    {
                                    }

                                    #endregion // Methods

                                #if DEBUG
                                    public void Trace()
                                    {
                                    }
                                #endif

                                    #region IDisposable

                                    private int _d;

                                    public void Dispose()
                                    {
                                    }

                                    #endregion // IDisposable

                                    {|RH7309:#region Base|}

                                    private int _b;

                                    #endregion // Base
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies the opposite side of the RH7309-005 example: the instance fields <c>_o</c> and <c>_m</c> lie in the first
    /// run of reordered blocks, so the group of a whole method between the runs stands behind every instance field of the
    /// spans, and the fix reorders the blocks with <c>_m</c> now in front of <c>_o</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-005")]
    [Requirement("SHARED-ORDER-008")]
    public async Task FixReordersInstanceFieldsOfTheFirstRunInFrontOfAGroupBetweenTwoReorderedRuns()
    {
        const string testData = """
                                using System;

                                public class Base
                                {
                                }

                                public class C : Base, IDisposable
                                {
                                    #region Object

                                    private int _o;

                                    public override string ToString()
                                    {
                                        return string.Empty;
                                    }

                                    #endregion // Object

                                    {|RH7309:#region Methods|}

                                    private int _m;

                                    public void M()
                                    {
                                    }

                                    #endregion // Methods

                                #if DEBUG
                                    public void Trace()
                                    {
                                    }
                                #endif

                                    #region IDisposable

                                    public void Dispose()
                                    {
                                    }

                                    #endregion // IDisposable

                                    {|RH7309:#region Base|}

                                    public void Reset()
                                    {
                                    }

                                    #endregion // Base
                                }
                                """;

        const string fixedData = """
                                 using System;

                                 public class Base
                                 {
                                 }

                                 public class C : Base, IDisposable
                                 {
                                     #region Methods

                                     private int _m;

                                     public void M()
                                     {
                                     }

                                     #endregion // Methods

                                     #region Object

                                     private int _o;

                                     public override string ToString()
                                     {
                                         return string.Empty;
                                     }

                                     #endregion // Object

                                 #if DEBUG
                                     public void Trace()
                                     {
                                     }
                                 #endif

                                     #region Base

                                     public void Reset()
                                     {
                                     }

                                     #endregion // Base

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
    /// Verifies that a group between two reordered runs of blocks, in front of an instance field of the spans, does not
    /// withhold the fix when the reorder keeps the order of the instance fields: <c>_o</c> stays in front of <c>_d</c>,
    /// and SHARED-ORDER-008 refuses for the type's layout only a move that reorders instance fields
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-005")]
    [Requirement("SHARED-ORDER-008")]
    public async Task FixReordersTheBlocksWhenAGroupBetweenTwoReorderedRunsPrecedesAnInstanceFieldWhoseOrderStays()
    {
        const string testData = """
                                using System;

                                public class Base
                                {
                                }

                                public class C : Base, IDisposable
                                {
                                    #region Object

                                    private int _o;

                                    public override string ToString()
                                    {
                                        return string.Empty;
                                    }

                                    #endregion // Object

                                    {|RH7309:#region Methods|}

                                    public void M()
                                    {
                                    }

                                    #endregion // Methods

                                #if DEBUG
                                    public void Trace()
                                    {
                                    }
                                #endif

                                    #region IDisposable

                                    private int _d;

                                    public void Dispose()
                                    {
                                    }

                                    #endregion // IDisposable

                                    {|RH7309:#region Base|}

                                    public void Reset()
                                    {
                                    }

                                    #endregion // Base
                                }
                                """;

        const string fixedData = """
                                 using System;

                                 public class Base
                                 {
                                 }

                                 public class C : Base, IDisposable
                                 {
                                     #region Methods

                                     public void M()
                                     {
                                     }

                                     #endregion // Methods

                                     #region Object

                                     private int _o;

                                     public override string ToString()
                                     {
                                         return string.Empty;
                                     }

                                     #endregion // Object

                                 #if DEBUG
                                     public void Trace()
                                     {
                                     }
                                 #endif

                                     #region Base

                                     public void Reset()
                                     {
                                     }

                                     #endregion // Base

                                     #region IDisposable

                                     private int _d;

                                     public void Dispose()
                                     {
                                     }

                                     #endregion // IDisposable
                                 }
                                 """;

        await Verify(testData, fixedData);
    }

    /// <summary>
    /// Verifies the exception of SHARED-ORDER-008 for RH7309: a group of an <c>#if</c> and its <c>#endif</c> that wraps
    /// the whole type keeps a reorder of instance fields, and the fix moves <c>Fields</c> in front of
    /// <c>IDisposable</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-008")]
    public async Task GroupThatWrapsTheWholeTypeKeepsAReorderOfInstanceFields()
    {
        const string testData = """
                                #if !DEBUG
                                public class C : System.IDisposable
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
                                #endif
                                """;

        const string fixedData = """
                                 #if !DEBUG
                                 public class C : System.IDisposable
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
                                 #endif
                                 """;

        await Verify(testData, fixedData);
    }

    /// <summary>
    /// Verifies the RH7309-005 example of a <c>#pragma warning</c> pair inside a member of a moved block: the pair does
    /// not withhold the fix and moves with its block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-005")]
    [Requirement("SHARED-ORDER-011")]
    public async Task WarningPairInsideAMemberOfAMovedBlockDoesNotWithholdTheFix()
    {
        const string testData = """
                                using System;

                                public class C : IDisposable
                                {
                                    #region IDisposable

                                    public void Dispose()
                                    {
                                #pragma warning disable CS0168
                                        int unused;
                                #pragma warning restore CS0168
                                    }

                                    #endregion // IDisposable

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

                                     #region IDisposable

                                     public void Dispose()
                                     {
                                 #pragma warning disable CS0168
                                         int unused;
                                 #pragma warning restore CS0168
                                     }

                                     #endregion // IDisposable
                                 }
                                 """;

        await Verify(testData, fixedData);
    }

    /// <summary>
    /// Verifies the RH7309-005 example of a <c>#pragma warning</c> pair inside a member of a block that keeps its place:
    /// the pair does not withhold the fix, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7309-005")]
    [Requirement("SHARED-ORDER-011")]
    public async Task WarningPairInsideAMemberOfABlockThatKeepsItsPlaceDoesNotWithholdTheFix(string endOfLine)
    {
        const string testData = """
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
                                #pragma warning disable CS0168
                                        int unused;
                                #pragma warning restore CS0168
                                        return 0;
                                    }

                                    #endregion // IComparable
                                }
                                """;

        const string fixedData = """
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
                                 #pragma warning disable CS0168
                                         int unused;
                                 #pragma warning restore CS0168
                                         return 0;
                                     }

                                     #endregion // IComparable
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that a <c>#pragma warning</c> pair inside a member in the code between two blocks, which a moved block
    /// crosses, does not withhold the fix: the pair ends its own effect inside the member
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-005")]
    [Requirement("SHARED-ORDER-011")]
    public async Task WarningPairInsideAMemberBetweenTheBlocksDoesNotWithholdTheFix()
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

                                    public void Reset()
                                    {
                                #pragma warning disable CS0168
                                        int unused;
                                #pragma warning restore CS0168
                                    }

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

                                     public void Reset()
                                     {
                                 #pragma warning disable CS0168
                                         int unused;
                                 #pragma warning restore CS0168
                                     }

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
    /// Verifies that a <c>#nullable disable</c> closed by <c>#nullable restore</c> inside a member of a moved block does
    /// not withhold the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-011")]
    public async Task NullablePairInsideAMemberOfAMovedBlockDoesNotWithholdTheFix()
    {
        const string testData = """
                                using System;

                                public class C : IDisposable
                                {
                                    #region IDisposable

                                    public void Dispose()
                                    {
                                #nullable disable
                                        string text = null;
                                #nullable restore
                                    }

                                    #endregion // IDisposable

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

                                     #region IDisposable

                                     public void Dispose()
                                     {
                                 #nullable disable
                                         string text = null;
                                 #nullable restore
                                     }

                                     #endregion // IDisposable
                                 }
                                 """;

        await Verify(testData, fixedData);
    }

    /// <summary>
    /// Verifies that a lone <c>#pragma warning disable</c> inside a member of a block withholds the fix: no
    /// <c>restore</c> closes it inside the member
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-005")]
    [Requirement("SHARED-ORDER-011")]
    public async Task NoCodeFixWhenAMemberOfABlockHoldsALoneWarningDisable()
    {
        const string testData = """
                                using System;

                                public class C : IDisposable
                                {
                                    #region IDisposable

                                    public void Dispose()
                                    {
                                #pragma warning disable CS0168
                                        int unused;
                                    }

                                    #endregion // IDisposable

                                    {|RH7309:#region Fields|}

                                    private int _a;

                                    #endregion // Fields
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that for RH7309 the text in front of the spans is the text in front of the type's first region block: an
    /// unclosed <c>#pragma warning disable</c> there withholds the fix across a pair inside a member of a block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-005")]
    [Requirement("SHARED-ORDER-011")]
    public async Task NoCodeFixWhenAnUnclosedWarningDirectiveStandsInFrontOfTheFirstBlock()
    {
        const string testData = """
                                using System;

                                public class C : IDisposable
                                {
                                #pragma warning disable CS1591

                                    #region IDisposable

                                    public void Dispose()
                                    {
                                #pragma warning disable CS0168
                                        int unused;
                                #pragma warning restore CS0168
                                    }

                                    #endregion // IDisposable

                                    {|RH7309:#region Fields|}

                                    private int _a;

                                    #endregion // Fields
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a closed pair in front of the type's first region block keeps the fix across a pair inside a member
    /// of a moved block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7309-005")]
    [Requirement("SHARED-ORDER-011")]
    public async Task ClosedWarningPairInFrontOfTheFirstBlockDoesNotWithholdTheFix()
    {
        const string testData = """
                                using System;

                                public class C : IDisposable
                                {
                                    public void Log()
                                    {
                                #pragma warning disable CS0168
                                        int unused;
                                #pragma warning restore CS0168
                                    }

                                    #region IDisposable

                                    public void Dispose()
                                    {
                                #pragma warning disable CS0168
                                        int unused;
                                #pragma warning restore CS0168
                                    }

                                    #endregion // IDisposable

                                    {|RH7309:#region Fields|}

                                    private int _a;

                                    #endregion // Fields
                                }
                                """;

        const string fixedData = """
                                 using System;

                                 public class C : IDisposable
                                 {
                                     public void Log()
                                     {
                                 #pragma warning disable CS0168
                                         int unused;
                                 #pragma warning restore CS0168
                                     }

                                     #region Fields

                                     private int _a;

                                     #endregion // Fields

                                     #region IDisposable

                                     public void Dispose()
                                     {
                                 #pragma warning disable CS0168
                                         int unused;
                                 #pragma warning restore CS0168
                                     }

                                     #endregion // IDisposable
                                 }
                                 """;

        await Verify(testData, fixedData);
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