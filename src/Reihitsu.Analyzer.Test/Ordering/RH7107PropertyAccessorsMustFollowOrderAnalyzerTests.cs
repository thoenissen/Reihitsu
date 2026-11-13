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
/// Test methods for <see cref="RH7107PropertyAccessorsMustFollowOrderAnalyzer"/> and <see cref="RH7107PropertyAccessorsMustFollowOrderCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH7107PropertyAccessorsMustFollowOrderAnalyzerTests : BatchCodeFixTestsBase<RH7107PropertyAccessorsMustFollowOrderAnalyzer, RH7107PropertyAccessorsMustFollowOrderCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying property accessors are reported and fixed when get appears after set
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7107-001")]
    [Requirement("RH7107-003")]
    [Requirement("RH7107-004")]
    public async Task PropertyAccessorsAreReportedAndFixedWhenGetAppearsAfterSet()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    public string Name
                                    {
                                        set
                                        {
                                        }

                                        {|#0:get|}
                                        {
                                            return string.Empty;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class TestClass
                                 {
                                     public string Name
                                     {
                                         get
                                         {
                                             return string.Empty;
                                         }

                                         set
                                         {
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7107PropertyAccessorsMustFollowOrderAnalyzer.DiagnosticId, AnalyzerResources.RH7107MessageFormat));
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
                                public class TestClass
                                {
                                    public int X
                                    {
                                #if DEBUG
                                        set
                                        {
                                        }
                                #endif
                                        get
                                        {
                                            return 0;
                                        }
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testCode,
                                                   RH7107PropertyAccessorsMustFollowOrderAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<AccessorDeclarationSyntax>()
                                                               .Single(accessor => accessor.Kind() == SyntaxKind.GetAccessorDeclaration)
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
                                public class TestClass
                                {
                                    public int X
                                    {
                                        set
                                        {
                                        }

                                        [System.Obsolete]
                                #if DEBUG
                                        get
                                        {
                                            return 0;
                                        }
                                #endif
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testCode,
                                                   RH7107PropertyAccessorsMustFollowOrderAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<AccessorDeclarationSyntax>()
                                                               .Single(accessor => accessor.Kind() == SyntaxKind.GetAccessorDeclaration)
                                                               .Keyword
                                                               .GetLocation(),
                                                   "DEBUG");

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying that expression-bodied accessors written on one line swap places within the line, and the accessor list
    /// stays on one line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7107-004")]
    [Requirement("SHARED-ORDER-009")]
    public async Task SingleLineExpressionBodiedAccessorsSwapPlacesWithinTheLine()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    private int _value;

                                    public int Value { set => _value = value; {|#0:get|} => _value; }
                                }
                                """;

        const string fixedCode = """
                                 public class TestClass
                                 {
                                     private int _value;

                                     public int Value { get => _value; set => _value = value; }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7107PropertyAccessorsMustFollowOrderAnalyzer.DiagnosticId, AnalyzerResources.RH7107MessageFormat));
    }

    /// <summary>
    /// Verifying that the accessors of an auto indexer written on separate lines move as whole lines, and the accessor list
    /// keeps its lines
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-007")]
    [Requirement("SHARED-ORDER-009")]
    public async Task MultiLineAutoIndexerAccessorsKeepTheirLines()
    {
        const string testCode = """
                                public interface ITestInterface
                                {
                                    int this[int index]
                                    {
                                        set;
                                        {|#0:get|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public interface ITestInterface
                                 {
                                     int this[int index]
                                     {
                                         get;
                                         set;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7107PropertyAccessorsMustFollowOrderAnalyzer.DiagnosticId, AnalyzerResources.RH7107MessageFormat));
    }

    /// <summary>
    /// Verifying the code fix keeps the blank line above the reordered property, and the <c>set</c> accessor keeps the
    /// layout it was written with: the embedded statement of its <c>if</c> statement gets no braces
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-006")]
    [Requirement("SHARED-FIX-007")]
    public async Task BlankLineAboveReorderedPropertyIsKeptWhenAccessorBodyIsBraced()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    private string _d;

                                    public string Description
                                    {
                                        set
                                        {
                                            if (value != null)
                                                _d = value;
                                        }

                                        {|#0:get|}
                                        {
                                            return _d;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class TestClass
                                 {
                                     private string _d;

                                     public string Description
                                     {
                                         get
                                         {
                                             return _d;
                                         }

                                         set
                                         {
                                             if (value != null)
                                                 _d = value;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7107PropertyAccessorsMustFollowOrderAnalyzer.DiagnosticId, AnalyzerResources.RH7107MessageFormat));
    }

    /// <summary>
    /// Verifying the specification example: a <c>get</c> accessor after <c>init</c> or <c>set</c> is reported on its
    /// keyword, for auto accessors, accessors with bodies, an indexer, and an interface property
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7107-001")]
    [Requirement("RH7107-003")]
    public async Task GetAccessorAfterSetOrInitIsReportedOnItsKeyword()
    {
        const string source = """
                              class C
                              {
                                  public int A { init; {|RH7107:get|}; }

                                  public int this[int index] { set { } {|RH7107:get|} { return index; } }
                              }

                              interface I
                              {
                                  int B { set; {|RH7107:get|}; }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying that a <c>get</c> accessor after a <c>set</c> accessor of a record property is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7107-001")]
    public async Task GetAccessorAfterSetInARecordIsReported()
    {
        const string source = """
                              record R
                              {
                                  public int A { set; {|RH7107:get|}; }
                              }

                              struct S
                              {
                                  public int B { set; {|RH7107:get|}; }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying the specification example: accessor lists that start with <c>get</c> or have no <c>get</c> are not
    /// reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7107-002")]
    public async Task AccessorListsInOrderAreNotReported()
    {
        const string source = """
                              class C
                              {
                                  public int A { get; set; }

                                  public int B { get; init; }

                                  public int D { set { } }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying the specification example: the fix moves <c>get</c> in front of <c>init</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7107-004")]
    public async Task FixMovesGetInFrontOfInit()
    {
        const string source = """
                              class C
                              {
                                  public int A { init; {|RH7107:get|}; }
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       public int A { get; init; }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying the specification example: the comment above the <c>get</c> accessor travels with it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7107-004")]
    [Requirement("SHARED-ORDER-002")]
    public async Task FixMovesTheCommentAboveTheGetAccessorWithIt()
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public int A
                                  {
                                      set
                                      {
                                          _value = value;
                                      }

                                      // Reads the value
                                      {|RH7107:get|}
                                      {
                                          return _value;
                                      }
                                  }
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _value;

                                       public int A
                                       {
                                           // Reads the value
                                           get
                                           {
                                               return _value;
                                           }

                                           set
                                           {
                                               _value = value;
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
    [Requirement("RH7107-005")]
    public async Task PragmaDirectiveInFrontOfTheGetAccessorWithholdsTheFix()
    {
        const string source = """
                              class C
                              {
                                  public int A
                                  {
                                      set
                                      {
                                      }

                              #pragma warning disable CS0162
                                      {|RH7107:get|}
                                      {
                                          return 0;
                                      }
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Characterizes current behavior that no requirement states: a balanced pragma pair inside the body of the
    /// crossed <c>set</c> accessor withholds the fix, and the diagnostic is still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task BalancedPragmaPairInTheCrossedAccessorBodyWithholdsTheFix()
    {
        const string source = """
                              class C
                              {
                                  public int A
                                  {
                                      set
                                      {
                              #pragma warning disable CS0168
                                          int unused;
                              #pragma warning restore CS0168
                                      }

                                      {|RH7107:get|}
                                      {
                                          return 0;
                                      }
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a comment the formatter refuses in the setter no longer withholds the fix for the reported getter,
    /// which moves the getter without formatting the property
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-006")]
    [Requirement("SHARED-FIX-010")]
    public async Task FixIsOfferedWhenTheOtherAccessorHoldsACommentTheFormatterRefuses()
    {
        const string source = """
                              internal class C
                              {
                                  private int _value;

                                  public int Value
                                  {
                                      set
                                      {
                                          int /* c */ y = value;
                                          _value = y;
                                      }
                                      {|RH7107:get|}
                                      {
                                          return _value;
                                      }
                                  }
                              }
                              """;

        const string fixedSource = """
                                   internal class C
                                   {
                                       private int _value;

                                       public int Value
                                       {
                                           get
                                           {
                                               return _value;
                                           }
                                           set
                                           {
                                               int /* c */ y = value;
                                               _value = y;
                                           }
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying the specification example: accessors with bodies written on one line swap places within the line, and
    /// the accessor list stays on one line
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7107-004")]
    [Requirement("SHARED-ORDER-009")]
    public async Task AccessorsWithBodiesOnOneLineSwapPlacesWithinTheLine(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  private int _b;

                                  public int B { set => _b = value; {|RH7107:get|} => _b; }
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _b;

                                       public int B { get => _b; set => _b = value; }
                                   }
                                   """;

        await VerifyCodeFixWithEndOfLine(source, fixedSource, endOfLine);
    }

    /// <summary>
    /// Verifying the specification example: the moved accessor shares its line with another accessor in a list that spans
    /// lines, so no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-009")]
    [Requirement("RH7107-005")]
    public async Task NoCodeFixWhenTheMovedAccessorSharesItsLineInAListThatSpansLines()
    {
        const string source = """
                              class C
                              {
                                  public int A
                                  {
                                      set; {|RH7107:get|};
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that no code fix is offered when the target accessor shares its line with the opening brace of an accessor
    /// list that spans lines
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-009")]
    public async Task NoCodeFixWhenTheTargetAccessorSharesItsLineWithTheOpeningBraceOfTheList()
    {
        const string source = """
                              class C
                              {
                                  public int A
                                  {   set;
                                      {|RH7107:get|};
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying the reported example: the <c>set</c> accessor written over two lines keeps its layout when <c>get</c> moves
    /// in front of it, and its embedded statement gets no braces
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7107-004")]
    [Requirement("SHARED-FIX-007")]
    public async Task CrossedSetAccessorOverTwoLinesKeepsItsLayout(string endOfLine)
    {
        const string source = """
                              public class C
                              {
                                  private string _d;

                                  public string D
                                  {
                                      set { if (value != null)
                                          _d = value; }
                                      {|RH7107:get|} { return _d; }
                                  }
                              }
                              """;

        const string fixedSource = """
                                   public class C
                                   {
                                       private string _d;

                                       public string D
                                       {
                                           get { return _d; }
                                           set { if (value != null)
                                               _d = value; }
                                       }
                                   }
                                   """;

        await VerifyCodeFixWithEndOfLine(source, fixedSource, endOfLine);
    }

    /// <summary>
    /// Verifying that the continuation line of a moved accessor keeps its column when the accessor moves into a slot
    /// indented as it is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-007")]
    public async Task ContinuationLineOfAMovedAccessorKeepsItsColumn()
    {
        const string source = """
                              class C
                              {
                                  public int A
                                  {
                                      set
                                      {
                                      }
                                      {|RH7107:get|} => 1
                                                + 2;
                                  }
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       public int A
                                       {
                                           get => 1
                                                     + 2;
                                           set
                                           {
                                           }
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying that no code fix is offered when a <c>get</c> accessor indented deeper than its slot holds a verbatim
    /// string over two lines
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-010")]
    [Requirement("RH7107-005")]
    public async Task NoCodeFixWhenAShiftedAccessorHoldsAVerbatimStringOverTwoLines()
    {
        const string source = """
                              class C
                              {
                                  public string A
                                  {
                                      set
                                      {
                                      }
                                          {|RH7107:get|} => @"x
                              y";
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
                                public class TestClass
                                {
                                    public string Name
                                    {
                                        set
                                        {
                                        }

                                        {|#0:get|}
                                        {
                                            return string.Empty;
                                        }
                                    }

                                    public string Description
                                    {
                                        set
                                        {
                                        }

                                        {|#1:get|}
                                        {
                                            return string.Empty;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class TestClass
                                 {
                                     public string Name
                                     {
                                         get
                                         {
                                             return string.Empty;
                                         }

                                         set
                                         {
                                         }
                                     }

                                     public string Description
                                     {
                                         get
                                         {
                                             return string.Empty;
                                         }

                                         set
                                         {
                                         }
                                     }
                                 }
                                 """;

        // Two misordered accessor lists inside one type, so both rewrites are merged into the same type
        // declaration and each accessor move has to stay confined to its own property
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH7107PropertyAccessorsMustFollowOrderAnalyzer.DiagnosticId, AnalyzerResources.RH7107MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}