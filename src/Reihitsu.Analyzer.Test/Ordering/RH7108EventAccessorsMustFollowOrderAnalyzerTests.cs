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
    /// Characterizes the code fix for block event accessors written on one line: it puts the accessor list and each
    /// reordered accessor on lines of their own, and spreads the braces of the moved <c>add</c> accessor over separate
    /// lines. This test pins today's behavior; the decided behavior is stated by SHARED-FIX-007, under which the moved
    /// accessor keeps its braces and line breaks as written and the separators around it follow SHARED-ORDER-002, and by
    /// SHARED-FIX-006, under which the fix does not format the event that holds the accessors, and its change belongs to
    /// an existing follow-up
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task SingleLineEventAccessorsAreFixedOntoSeparateLines()
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