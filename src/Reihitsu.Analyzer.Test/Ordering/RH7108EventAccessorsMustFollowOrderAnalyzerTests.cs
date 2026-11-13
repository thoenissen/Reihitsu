using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Ordering;

/// <summary>
/// Test methods for <see cref="RH7108EventAccessorsMustFollowOrderAnalyzer"/> and <see cref="RH7108EventAccessorsMustFollowOrderCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH7108EventAccessorsMustFollowOrderAnalyzerTests : BatchCodeFixTestsBase<RH7108EventAccessorsMustFollowOrderAnalyzer, RH7108EventAccessorsMustFollowOrderCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying event accessors are reported and fixed when add appears after remove
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7108-001")]
    [Requirement("RH7108-003")]
    [Requirement("RH7108-004")]
    public async Task EventAccessorsAreReportedAndFixedWhenAddAppearsAfterRemove()
    {
        const string testCode = """
                                using System;

                                public class TestClass
                                {
                                    private EventHandler _changed;

                                    public event EventHandler Changed
                                    {
                                        remove
                                        {
                                            _changed -= value;
                                        }

                                        {|#0:add|}
                                        {
                                            _changed += value;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 public class TestClass
                                 {
                                     private EventHandler _changed;

                                     public event EventHandler Changed
                                     {
                                         add
                                         {
                                             _changed += value;
                                         }

                                         remove
                                         {
                                             _changed -= value;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7108EventAccessorsMustFollowOrderAnalyzer.DiagnosticId, AnalyzerResources.RH7108MessageFormat));
    }

    /// <summary>
    /// Verifying no code fix is offered when the move would separate a preprocessor directive from its partner,
    /// with the conditional opened around the target accessor and closed before the moved accessor
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NoCodeFixWhenDirectivesAreInAccessorLeadingTrivia()
    {
        const string testCode = """
                                using System;

                                public class TestClass
                                {
                                    private EventHandler _changed;

                                    public event EventHandler Changed
                                    {
                                #if DEBUG
                                        remove
                                        {
                                            _changed -= value;
                                        }
                                #endif
                                        add
                                        {
                                            _changed += value;
                                        }
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testCode,
                                                   RH7108EventAccessorsMustFollowOrderAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<AccessorDeclarationSyntax>()
                                                               .Single(accessor => accessor.Kind() == SyntaxKind.AddAccessorDeclaration)
                                                               .Keyword
                                                               .GetLocation(),
                                                   "DEBUG");

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying no code fix is offered when a preprocessor directive sits between the accessor attribute list and the
    /// accessor keyword, since the directive attaches to a later token and moving the accessor would split the pair
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NoCodeFixWhenDirectiveFollowsAccessorAttributeList()
    {
        const string testCode = """
                                using System;

                                public class TestClass
                                {
                                    private EventHandler _changed;

                                    public event EventHandler Changed
                                    {
                                        remove
                                        {
                                            _changed -= value;
                                        }

                                        [Obsolete]
                                #if DEBUG
                                        add
                                        {
                                            _changed += value;
                                        }
                                #endif
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testCode,
                                                   RH7108EventAccessorsMustFollowOrderAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<AccessorDeclarationSyntax>()
                                                               .Single(accessor => accessor.Kind() == SyntaxKind.AddAccessorDeclaration)
                                                               .Keyword
                                                               .GetLocation(),
                                                   "DEBUG");

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying that block event accessors written on one line swap places within the line, keep their braces as
    /// written, and the accessor list stays on one line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7108-004")]
    [Requirement("SHARED-ORDER-009")]
    public async Task SingleLineEventAccessorsSwapPlacesWithinTheLine()
    {
        const string testCode = """
                                using System;

                                public class TestClass
                                {
                                    private EventHandler _changed;

                                    public event EventHandler Changed { remove { _changed -= value; } {|#0:add|} { _changed += value; } }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 public class TestClass
                                 {
                                     private EventHandler _changed;

                                     public event EventHandler Changed { add { _changed += value; } remove { _changed -= value; } }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7108EventAccessorsMustFollowOrderAnalyzer.DiagnosticId, AnalyzerResources.RH7108MessageFormat));
    }

    /// <summary>
    /// Verifying the specification example: an <c>add</c> accessor after <c>remove</c> is reported on its keyword
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7108-001")]
    [Requirement("RH7108-003")]
    public async Task AddAccessorAfterRemoveIsReportedOnItsKeyword()
    {
        const string source = """
                              using System;

                              class C
                              {
                                  private Action _handlers;

                                  public event Action Changed
                                  {
                                      remove
                                      {
                                          _handlers -= value;
                                      }

                                      {|RH7108:add|}
                                      {
                                          _handlers += value;
                                      }
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying the specification example: an event whose <c>add</c> accessor comes first is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7108-002")]
    public async Task AccessorListInOrderIsNotReported()
    {
        const string source = """
                              using System;

                              class C
                              {
                                  private Action _handlers;

                                  public event Action Changed
                                  {
                                      add
                                      {
                                          _handlers += value;
                                      }

                                      remove
                                      {
                                          _handlers -= value;
                                      }
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying the specification example: the fix moves <c>add</c> in front of <c>remove</c>, and the comment above
    /// the <c>add</c> accessor travels with it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7108-004")]
    [Requirement("SHARED-ORDER-002")]
    public async Task FixMovesAddAndItsCommentInFrontOfRemove()
    {
        const string source = """
                              using System;

                              class C
                              {
                                  private Action _handlers;

                                  public event Action Changed
                                  {
                                      remove
                                      {
                                          _handlers -= value;
                                      }

                                      // Subscribes
                                      {|RH7108:add|}
                                      {
                                          _handlers += value;
                                      }
                                  }
                              }
                              """;

        const string fixedSource = """
                                   using System;

                                   class C
                                   {
                                       private Action _handlers;

                                       public event Action Changed
                                       {
                                           // Subscribes
                                           add
                                           {
                                               _handlers += value;
                                           }

                                           remove
                                           {
                                               _handlers -= value;
                                           }
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying the specification example: a pragma directive in front of the moved accessor withholds the fix, and
    /// the diagnostic is still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7108-005")]
    public async Task PragmaDirectiveInFrontOfTheAddAccessorWithholdsTheFix()
    {
        const string source = """
                              using System;

                              class C
                              {
                                  private Action _handlers;

                                  public event Action Changed
                                  {
                                      remove
                                      {
                                          _handlers -= value;
                                      }

                              #pragma warning disable CS0162
                                      {|RH7108:add|}
                                      {
                                          _handlers += value;
                                      }
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a comment the formatter refuses in the remove accessor no longer withholds the fix for the reported add
    /// accessor, which moves the accessor without formatting the event
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-006")]
    [Requirement("SHARED-FIX-010")]
    public async Task FixIsOfferedWhenTheOtherAccessorHoldsACommentTheFormatterRefuses()
    {
        const string source = """
                              using System;

                              internal class C
                              {
                                  private EventHandler _changed;

                                  public event EventHandler Changed
                                  {
                                      remove
                                      {
                                          EventHandler /* c */ handler = value;
                                          _changed -= handler;
                                      }
                                      {|RH7108:add|}
                                      {
                                          _changed += value;
                                      }
                                  }
                              }
                              """;

        const string fixedSource = """
                                   using System;

                                   internal class C
                                   {
                                       private EventHandler _changed;

                                       public event EventHandler Changed
                                       {
                                           add
                                           {
                                               _changed += value;
                                           }
                                           remove
                                           {
                                               EventHandler /* c */ handler = value;
                                               _changed -= handler;
                                           }
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying the specification example: an event accessor list on one line stays on one line, and the accessors swap
    /// places within it
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7108-004")]
    [Requirement("SHARED-ORDER-009")]
    public async Task ExpressionBodiedEventAccessorsOnOneLineSwapPlacesWithinTheLine(string endOfLine)
    {
        const string source = """
                              using System;

                              class C
                              {
                                  private Action _handlers;

                                  public event Action Changed { remove => _handlers -= value; {|RH7108:add|} => _handlers += value; }
                              }
                              """;

        const string fixedSource = """
                                   using System;

                                   class C
                                   {
                                       private Action _handlers;

                                       public event Action Changed { add => _handlers += value; remove => _handlers -= value; }
                                   }
                                   """;

        await VerifyCodeFixWithEndOfLine(source, fixedSource, endOfLine);
    }

    /// <summary>
    /// Verifying the reported example: the crossed <c>remove</c> accessor keeps its layout when <c>add</c> moves in front of
    /// it, and the embedded statement of its <c>if</c> statement gets no braces
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7108-004")]
    [Requirement("SHARED-FIX-006")]
    public async Task CrossedRemoveAccessorKeepsItsLayout(string endOfLine)
    {
        const string source = """
                              using System;

                              public class C
                              {
                                  private EventHandler _e;

                                  public event EventHandler E
                                  {
                                      remove { if (_e != null) _e -= value; }
                                      {|RH7108:add|} { _e += value; }
                                  }
                              }
                              """;

        const string fixedSource = """
                                   using System;

                                   public class C
                                   {
                                       private EventHandler _e;

                                       public event EventHandler E
                                       {
                                           add { _e += value; }
                                           remove { if (_e != null) _e -= value; }
                                       }
                                   }
                                   """;

        await VerifyCodeFixWithEndOfLine(source, fixedSource, endOfLine);
    }

    /// <summary>
    /// Verifying that no code fix is offered when the moved <c>add</c> accessor shares its line with <c>remove</c> in an
    /// accessor list that spans lines
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-009")]
    [Requirement("RH7108-005")]
    public async Task NoCodeFixWhenTheMovedAccessorSharesItsLineInAListThatSpansLines()
    {
        const string source = """
                              using System;

                              public class C
                              {
                                  private EventHandler _e;

                                  public event EventHandler E
                                  {
                                      remove { _e -= value; } {|RH7108:add|} { _e += value; }
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                using System;

                                public class TestClass
                                {
                                    private EventHandler _changed;

                                    private EventHandler _closed;

                                    public event EventHandler Changed
                                    {
                                        remove
                                        {
                                            _changed -= value;
                                        }

                                        {|#0:add|}
                                        {
                                            _changed += value;
                                        }
                                    }

                                    public event EventHandler Closed
                                    {
                                        remove
                                        {
                                            _closed -= value;
                                        }

                                        {|#1:add|}
                                        {
                                            _closed += value;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 public class TestClass
                                 {
                                     private EventHandler _changed;

                                     private EventHandler _closed;

                                     public event EventHandler Changed
                                     {
                                         add
                                         {
                                             _changed += value;
                                         }

                                         remove
                                         {
                                             _changed -= value;
                                         }
                                     }

                                     public event EventHandler Closed
                                     {
                                         add
                                         {
                                             _closed += value;
                                         }

                                         remove
                                         {
                                             _closed -= value;
                                         }
                                     }
                                 }
                                 """;

        // Two misordered accessor lists inside one type, so both rewrites are merged into the same type
        // declaration and each accessor move has to stay confined to its own event
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH7108EventAccessorsMustFollowOrderAnalyzer.DiagnosticId, AnalyzerResources.RH7108MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}