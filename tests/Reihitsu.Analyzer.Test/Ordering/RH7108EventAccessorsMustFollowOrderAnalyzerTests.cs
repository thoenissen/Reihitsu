using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Testing;
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

        await Verify(testCode, fixedCode, Diagnostics(RH7108EventAccessorsMustFollowOrderAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that an accessor list with a conditional directive around the misordered remove accessor is reported
    /// where the symbol is defined, and no code fix is offered, because the <c>#endif</c> in front of the add accessor
    /// would leave its <c>#if</c> behind
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7108-001")]
    [Requirement("RH7108-005")]
    [Requirement("SHARED-DIAG-008")]
    public async Task ConditionalDirectiveAroundTheRemoveAccessorKeepsTheReportAndWithholdsTheFix()
    {
        const string testData = """
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
                                        {|RH7108:add|}
                                        {
                                            _changed += value;
                                        }
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData, DocumentationMode.Parse, "DEBUG");
    }

    /// <summary>
    /// Verifies that an accessor list with a conditional directive between the attribute list and the keyword of the
    /// add accessor is reported, and no code fix is offered, because the <c>#if</c> would move without its <c>#endif</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7108-001")]
    [Requirement("RH7108-005")]
    [Requirement("SHARED-DIAG-008")]
    public async Task ConditionalDirectiveBehindTheAttributeListOfTheAddAccessorKeepsTheReportAndWithholdsTheFix()
    {
        const string testData = """
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

                                        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
                                #if DEBUG
                                        {|RH7108:add|}
                                        {
                                            _changed += value;
                                        }
                                #endif
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData, DocumentationMode.Parse, "DEBUG");
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

        await Verify(testCode, fixedCode, Diagnostics(RH7108EventAccessorsMustFollowOrderAnalyzer.DiagnosticId, MessageFormat));
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
    /// Verifying that a <c>#pragma warning</c> pair inside the body of the crossed <c>remove</c> accessor keeps the fix:
    /// the pair ends its own effect inside the accessor and stays with it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7108-005")]
    [Requirement("SHARED-ORDER-011")]
    public async Task WarningPairInTheCrossedRemoveAccessorBodyKeepsTheFix()
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
                              #pragma warning disable CS0168
                                          int unused;
                              #pragma warning restore CS0168
                                          _handlers -= value;
                                      }

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
                                           add
                                           {
                                               _handlers += value;
                                           }

                                           remove
                                           {
                                   #pragma warning disable CS0168
                                               int unused;
                                   #pragma warning restore CS0168
                                               _handlers -= value;
                                           }
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying that a pair split across a conditional branch inside the body of the crossed <c>remove</c> accessor
    /// withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7108-005")]
    [Requirement("SHARED-ORDER-011")]
    public async Task NoCodeFixWhenAWarningPairInTheCrossedRemoveAccessorBodyIsSplitAcrossAConditionalBranch()
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
                              #pragma warning disable CS0168
                                          int unused;
                              #if DEBUG
                              #pragma warning restore CS0168
                              #endif
                                          _handlers -= value;
                                      }

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

    /// <summary>
    /// Verifying that a comment between the event accessors of a list on one line withholds the swap
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-009")]
    [Requirement("RH7108-005")]
    public async Task NoCodeFixWhenACommentStandsBetweenTheEventAccessorsOfAListOnOneLine()
    {
        const string source = """
                              using System;

                              class C
                              {
                                  private Action _handlers;

                                  public event Action Changed { remove => _handlers -= value; /* r */ {|RH7108:add|} => _handlers += value; }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that a comment behind the last event accessor of a list on one line withholds the swap
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-009")]
    [Requirement("RH7108-005")]
    public async Task NoCodeFixWhenACommentStandsBehindTheLastEventAccessorOfAListOnOneLine()
    {
        const string source = """
                              using System;

                              class C
                              {
                                  private Action _handlers;

                                  public event Action Changed { remove => _handlers -= value; {|RH7108:add|} => _handlers += value; /* a */ }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that a comment in front of the first event accessor of a list on one line withholds the swap
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-009")]
    [Requirement("RH7108-005")]
    public async Task NoCodeFixWhenACommentStandsInFrontOfTheFirstEventAccessorOfAListOnOneLine()
    {
        const string source = """
                              using System;

                              class C
                              {
                                  private Action _handlers;

                                  public event Action Changed { /* c */ remove => _handlers -= value; {|RH7108:add|} => _handlers += value; }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that a comment inside an event accessor of a list on one line travels with its accessor when the
    /// accessors swap places within the line
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-009")]
    [Requirement("RH7108-004")]
    public async Task CommentInsideAnEventAccessorTravelsWithItWhenTheAccessorsSwapWithinTheLine(string endOfLine)
    {
        const string source = """
                              using System;

                              class C
                              {
                                  private Action _handlers;

                                  public event Action Changed { remove { _handlers -= value; /* r */ } {|RH7108:add|} { _handlers += value; } }
                              }
                              """;

        const string fixedSource = """
                                   using System;

                                   class C
                                   {
                                       private Action _handlers;

                                       public event Action Changed { add { _handlers += value; } remove { _handlers -= value; /* r */ } }
                                   }
                                   """;

        await VerifyCodeFixWithEndOfLine(source, fixedSource, endOfLine);
    }

    /// <summary>
    /// Verifies the specification example in which neither a conditional directive between the accessors nor one inside
    /// an accessor body keeps the add accessor from being reported; the fix is withheld for <c>A</c>, whose <c>#endif</c>
    /// in front of <c>add</c> would leave its <c>#if</c> behind, and offered for <c>B</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7108-001")]
    [Requirement("RH7108-005")]
    [Requirement("SHARED-DIAG-008")]
    public async Task ConditionalDirectiveBetweenTheAccessorsOrInsideABodyDoesNotKeepTheAddAccessorFromBeingReported()
    {
        const string testData = """
                                using System;

                                class C
                                {
                                    private Action _handlers;

                                    public event Action A
                                    {
                                #if !NEVER
                                        remove { _handlers -= value; }
                                #endif
                                        {|RH7108:add|} { _handlers += value; }
                                    }

                                    public event Action B
                                    {
                                        remove
                                        {
                                #if DEBUG
                                            Console.WriteLine();
                                #endif
                                            _handlers -= value;
                                        }

                                        {|RH7108:add|}
                                        {
                                            _handlers += value;
                                        }
                                    }
                                }
                                """;

        const string fixedData = """
                                 using System;

                                 class C
                                 {
                                     private Action _handlers;

                                     public event Action A
                                     {
                                 #if !NEVER
                                         remove { _handlers -= value; }
                                 #endif
                                         {|RH7108:add|} { _handlers += value; }
                                     }

                                     public event Action B
                                     {
                                         add
                                         {
                                             _handlers += value;
                                         }

                                         remove
                                         {
                                 #if DEBUG
                                             Console.WriteLine();
                                 #endif
                                             _handlers -= value;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     test =>
                     {
                         // The diagnostic whose fix is withheld stays and is marked in the expected source
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                     });
    }

    /// <summary>
    /// Verifies that a balanced conditional directive pair in the leading trivia of the add accessor, empty or holding
    /// excluded code, does not keep it from being reported, and that the fix moves the pair with it
    /// </summary>
    /// <param name="directives">The balanced conditional directive pair, and the code it holds, in front of <c>add</c></param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("#if DEBUG\n#endif", DisplayName = "Empty block")]
    [DataRow("#if NEVER\n        add { }\n#endif", DisplayName = "Excluded accessor")]
    [DataRow("#if false\n        add { }\n#endif", DisplayName = "If false")]
    [Requirement("RH7108-001")]
    [Requirement("RH7108-005")]
    [Requirement("SHARED-DIAG-008")]
    [Requirement("SHARED-ORDER-006")]
    public async Task BalancedConditionalDirectivePairInFrontOfTheAddAccessorIsReportedAndMovesWithIt(string directives)
    {
        var testData = $$"""
                         using System;

                         class C
                         {
                             private Action _handlers;

                             public event Action A
                             {
                                 remove { _handlers -= value; }
                         {{directives}}
                                 {|RH7108:add|} { _handlers += value; }
                             }
                         }
                         """;

        var fixedData = $$"""
                          using System;

                          class C
                          {
                              private Action _handlers;

                              public event Action A
                              {
                          {{directives}}
                                  add { _handlers += value; }
                                  remove { _handlers -= value; }
                              }
                          }
                          """;

        await Verify(testData, fixedData);
    }

    /// <summary>
    /// Verifies that a conditional group whose <c>#if</c> stands in front of the crossed remove accessor and whose
    /// <c>#endif</c> stands in front of the add accessor does not keep the add accessor from being reported, and that no
    /// code fix is offered, for every shape of the group
    /// </summary>
    /// <param name="directives">The conditional group, and the remove accessor it holds, in front of <c>add</c></param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("#if !NEVER\n        remove { _handlers -= value; }\n#endif", DisplayName = "Active accessor")]
    [DataRow("#if NEVER\n        remove { }\n#else\n        remove { _handlers -= value; }\n#endif", DisplayName = "If and else")]
    [DataRow("#if NEVER\n        remove { }\n#elif !NEVER\n        remove { _handlers -= value; }\n#endif", DisplayName = "If and elif")]
    [Requirement("RH7108-001")]
    [Requirement("RH7108-005")]
    [Requirement("SHARED-DIAG-008")]
    [Requirement("SHARED-ORDER-003")]
    public async Task ConditionalGroupAroundTheCrossedRemoveAccessorKeepsTheReportAndWithholdsTheFix(string directives)
    {
        var testData = $$"""
                         using System;

                         class C
                         {
                             private Action _handlers;

                             public event Action A
                             {
                         {{directives}}
                                 {|RH7108:add|} { _handlers += value; }
                             }
                         }
                         """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that Fix All moves the add accessors whose move is safe and leaves the one whose <c>#endif</c> would be
    /// torn from its <c>#if</c> reported and in place
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7108-001")]
    [Requirement("RH7108-005")]
    [Requirement("SHARED-DIAG-008")]
    public async Task FixAllMovesOnlyTheAddAccessorsWhoseMoveKeepsTheirDirectivesPaired()
    {
        const string testData = """
                                using System;

                                class C
                                {
                                    private Action _handlers;

                                    public event Action A
                                    {
                                #if !NEVER
                                        remove { _handlers -= value; }
                                #endif
                                        {|RH7108:add|} { _handlers += value; }
                                    }

                                    public event Action B
                                    {
                                        remove { _handlers -= value; }
                                        {|RH7108:add|} { _handlers += value; }
                                    }

                                    public event Action D
                                    {
                                        remove { _handlers -= value; }
                                        {|RH7108:add|} { _handlers += value; }
                                    }
                                }
                                """;

        const string fixedData = """
                                 using System;

                                 class C
                                 {
                                     private Action _handlers;

                                     public event Action A
                                     {
                                 #if !NEVER
                                         remove { _handlers -= value; }
                                 #endif
                                         {|RH7108:add|} { _handlers += value; }
                                     }

                                     public event Action B
                                     {
                                         add { _handlers += value; }
                                         remove { _handlers -= value; }
                                     }

                                     public event Action D
                                     {
                                         add { _handlers += value; }
                                         remove { _handlers -= value; }
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     test =>
                     {
                         // The diagnostic whose fix is withheld stays and is marked in the expected source
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                     });
    }

    /// <summary>
    /// Verifies that a conditional directive pair with excluded code at the end of the crossed <c>remove</c> accessor's
    /// expression body, in front of its semicolon, does not keep the <c>add</c> accessor from being reported, and that
    /// the fix moves <c>add</c> in front of it with the pair intact
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7108-001")]
    [Requirement("RH7108-004")]
    [Requirement("SHARED-ORDER-006")]
    public async Task ExcludedCodeAtTheEndOfTheExpressionBodyOfTheRemoveAccessorDoesNotKeepTheAddAccessorFromBeingReported(string endOfLine)
    {
        const string source = """
                              using System;

                              class C
                              {
                                  private Action _handlers;

                                  public event Action A
                                  {
                                      remove => _handlers -= value
                              #if DEBUG
                                                  ?? throw new InvalidOperationException()
                              #endif
                                                  ;
                                      {|RH7108:add|} => _handlers += value;
                                  }
                              }
                              """;

        const string fixedSource = """
                                   using System;

                                   class C
                                   {
                                       private Action _handlers;

                                       public event Action A
                                       {
                                           add => _handlers += value;
                                           remove => _handlers -= value
                                   #if DEBUG
                                                       ?? throw new InvalidOperationException()
                                   #endif
                                                       ;
                                       }
                                   }
                                   """;

        await VerifyCodeFixWithEndOfLine(source, fixedSource, endOfLine);
    }

    /// <summary>
    /// Verifies that a conditional directive pair with excluded code at the end of the moved <c>add</c> accessor's
    /// expression body, in front of its semicolon, does not keep it from being reported, and that the pair travels with it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7108-001")]
    [Requirement("RH7108-004")]
    [Requirement("SHARED-ORDER-006")]
    public async Task ExcludedCodeAtTheEndOfTheExpressionBodyOfTheAddAccessorDoesNotKeepItFromBeingReported()
    {
        const string source = """
                              using System;

                              class C
                              {
                                  private Action _handlers;

                                  public event Action A
                                  {
                                      remove => _handlers -= value;
                                      {|RH7108:add|} => _handlers += value
                              #if DEBUG
                                                  ?? throw new InvalidOperationException()
                              #endif
                                                  ;
                                  }
                              }
                              """;

        const string fixedSource = """
                                   using System;

                                   class C
                                   {
                                       private Action _handlers;

                                       public event Action A
                                       {
                                           add => _handlers += value
                                   #if DEBUG
                                                       ?? throw new InvalidOperationException()
                                   #endif
                                                       ;
                                           remove => _handlers -= value;
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
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
                                  Diagnostics(RH7108EventAccessorsMustFollowOrderAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}