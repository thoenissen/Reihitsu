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
    /// Verifies that an accessor list with a conditional directive around the misordered set accessor is judged as the
    /// analyzed build compiles it: without the symbol, the build compiles only the get accessor and nothing is reported;
    /// with the symbol, the get accessor is reported, and no code fix is offered, because the <c>#endif</c> in front of it
    /// would leave its <c>#if</c> behind
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7107-001")]
    [Requirement("RH7107-005")]
    [Requirement("SHARED-DIAG-008")]
    public async Task ConditionalDirectiveAroundTheSetAccessorIsJudgedAsTheBuildCompilesIt()
    {
        const string testData = """
                                public class TestClass
                                {
                                    public int X
                                    {
                                #if DEBUG
                                        set
                                        {
                                        }
                                #endif
                                        {|RH7107:get|}
                                        {
                                            return 0;
                                        }
                                    }
                                }
                                """;

        await Verify(RemoveMarkup(testData));
        await VerifyNoCodeFixIsOffered(testData, DocumentationMode.Parse, "DEBUG");
    }

    /// <summary>
    /// Verifies that an accessor list with a conditional directive between the attribute list and the keyword of the
    /// get accessor is reported, and no code fix is offered, because the <c>#if</c> would move without its <c>#endif</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7107-001")]
    [Requirement("RH7107-005")]
    [Requirement("SHARED-DIAG-008")]
    public async Task ConditionalDirectiveBehindTheAttributeListOfTheGetAccessorKeepsTheReportAndWithholdsTheFix()
    {
        const string testData = """
                                public class TestClass
                                {
                                    public int X
                                    {
                                        set
                                        {
                                        }

                                        [System.Obsolete]
                                #if DEBUG
                                        {|RH7107:get|}
                                        {
                                            return 0;
                                        }
                                #endif
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData, DocumentationMode.Parse, "DEBUG");
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

    /// <summary>
    /// Verifying the specification example: a comment between the accessors of a list on one line withholds the swap
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-009")]
    [Requirement("RH7107-005")]
    public async Task NoCodeFixWhenACommentStandsBetweenTheAccessorsOfAListOnOneLine()
    {
        const string source = """
                              class C
                              {
                                  public int A { set { } /* s */ {|RH7107:get|} => 1; }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that a comment behind the last accessor of a list on one line withholds the swap
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-009")]
    [Requirement("RH7107-005")]
    public async Task NoCodeFixWhenACommentStandsBehindTheLastAccessorOfAListOnOneLine()
    {
        const string source = """
                              class C
                              {
                                  public int A { set { } {|RH7107:get|} => 1; /* g */ }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that a comment in front of the first accessor of a list on one line withholds the swap
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-009")]
    [Requirement("RH7107-005")]
    public async Task NoCodeFixWhenACommentStandsInFrontOfTheFirstAccessorOfAListOnOneLine()
    {
        const string source = """
                              class C
                              {
                                  public int A { /* c */ set { } {|RH7107:get|} => 1; }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying the specification example: a comment inside an accessor of a list on one line travels with its accessor
    /// when the accessors swap places within the line
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-ORDER-009")]
    [Requirement("RH7107-004")]
    public async Task CommentInsideAnAccessorTravelsWithItWhenTheAccessorsSwapWithinTheLine(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  public int A { set { /* s */ } {|RH7107:get|} => 1; }
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       public int A { get => 1; set { /* s */ } }
                                   }
                                   """;

        await VerifyCodeFixWithEndOfLine(source, fixedSource, endOfLine);
    }

    /// <summary>
    /// Verifying that a comment inside the expression body of the moved accessor travels with it when the accessors swap
    /// places within the line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-009")]
    [Requirement("RH7107-004")]
    public async Task CommentInsideTheExpressionBodyOfTheMovedAccessorTravelsWithIt()
    {
        const string source = """
                              class C
                              {
                                  public int A { set { } {|RH7107:get|} => /* one */ 1; }
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       public int A { get => /* one */ 1; set { } }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying that a comment behind the closing brace of an accessor list on one line stands outside the list and does
    /// not withhold the swap
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-009")]
    [Requirement("RH7107-004")]
    public async Task CommentBehindTheClosingBraceOfAListOnOneLineKeepsTheSwap()
    {
        const string source = """
                              class C
                              {
                                  public int A { set { } {|RH7107:get|} => 1; } // value
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       public int A { get => 1; set { } } // value
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying that accessors on one line still swap places in a document whose line breaks are all a lone carriage
    /// return, because the swap moves no line break; every line break stays as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-008")]
    [Requirement("SHARED-ORDER-009")]
    public async Task AccessorsOnOneLineSwapInADocumentWhoseLineBreaksAreLoneCarriageReturns()
    {
        var source = """
                     class C
                     {
                         public int A { set { } get => 1; }
                     }
                     """.Replace("\r\n", "\n").Replace("\n", "\r");

        var fixedSource = """
                          class C
                          {
                              public int A { get => 1; set { } }
                          }
                          """.Replace("\r\n", "\n").Replace("\n", "\r");

        Assert.AreEqual(fixedSource, await ApplyCodeFixAsync(source));
    }

    /// <summary>
    /// Verifying that no code fix is offered for an accessor list over several lines when the crossed accessor holds LF
    /// line breaks in a document whose line breaks are otherwise CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-008")]
    public async Task NoCodeFixWhenTheCrossedAccessorHoldsALineBreakOtherThanTheDocumentsLineEnding()
    {
        var source = NormalizeToCarriageReturnLineFeed("""
                                                       class C
                                                       {
                                                           public int A
                                                           {
                                                               set{LF}
                                                               {{LF}
                                                               }
                                                               {|RH7107:get|} => 1;
                                                           }
                                                       }
                                                       """).Replace("{LF}\r\n", "\n");

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that no code fix is offered for an accessor list over several lines in a document whose line breaks are
    /// all a lone carriage return
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-008")]
    public async Task NoCodeFixForAnAccessorListOverSeveralLinesWhoseLineBreaksAreLoneCarriageReturns()
    {
        var source = """
                     class C
                     {
                         public int A
                         {
                             set
                             {
                             }
                             {|RH7107:get|} => 1;
                         }
                     }
                     """.Replace("\r\n", "\n").Replace("\n", "\r");

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that no code fix is offered when a <c>get</c> accessor indented less deeply than its slot holds an
    /// interpolated verbatim string whose second line starts at a hole
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ORDER-010")]
    [Requirement("RH7107-005")]
    public async Task NoCodeFixWhenARightShiftedAccessorHoldsAnInterpolatedVerbatimStringWhoseSecondLineStartsAtAHole()
    {
        const string source = """
                              class C
                              {
                                  public string A
                                  {
                                      set
                                      {
                                      }
                                    {|RH7107:get|} => $@"a
                              {nameof(C)}";
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies the specification example in which neither a conditional directive between the accessors nor one inside
    /// an accessor body keeps the get accessor from being reported; the fix is withheld for <c>A</c>, whose <c>#endif</c>
    /// in front of <c>get</c> would leave its <c>#if</c> behind, and offered for <c>B</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7107-001")]
    [Requirement("RH7107-005")]
    [Requirement("SHARED-DIAG-008")]
    public async Task ConditionalDirectiveBetweenTheAccessorsOrInsideABodyDoesNotKeepTheGetAccessorFromBeingReported()
    {
        const string testData = """
                                class C
                                {
                                    private int _a;

                                    public int A
                                    {
                                #if !NEVER
                                        set { _a = value; }
                                #endif
                                        {|RH7107:get|} { return _a; }
                                    }

                                    public int B
                                    {
                                        set
                                        {
                                #if DEBUG
                                            System.Console.WriteLine(value);
                                #endif
                                            _a = value;
                                        }

                                        {|RH7107:get|}
                                        {
                                            return _a;
                                        }
                                    }
                                }
                                """;

        const string fixedData = """
                                 class C
                                 {
                                     private int _a;

                                     public int A
                                     {
                                 #if !NEVER
                                         set { _a = value; }
                                 #endif
                                         {|RH7107:get|} { return _a; }
                                     }

                                     public int B
                                     {
                                         get
                                         {
                                             return _a;
                                         }

                                         set
                                         {
                                 #if DEBUG
                                             System.Console.WriteLine(value);
                                 #endif
                                             _a = value;
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
    /// Verifies that a balanced conditional directive pair in the leading trivia of the get accessor, empty or holding
    /// excluded code, does not keep it from being reported, and that the fix moves the pair with it
    /// </summary>
    /// <param name="directives">The balanced conditional directive pair, and the code it holds, in front of <c>get</c></param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("#if DEBUG\n#endif", DisplayName = "Empty block")]
    [DataRow("#if NEVER\n        init { }\n#endif", DisplayName = "Excluded accessor")]
    [DataRow("#if false\n        init { }\n#endif", DisplayName = "If false")]
    [Requirement("RH7107-001")]
    [Requirement("RH7107-005")]
    [Requirement("SHARED-DIAG-008")]
    [Requirement("SHARED-ORDER-006")]
    public async Task BalancedConditionalDirectivePairInFrontOfTheGetAccessorIsReportedAndMovesWithIt(string directives)
    {
        var testData = $$"""
                         class C
                         {
                             private int _a;

                             public int A
                             {
                                 set { _a = value; }
                         {{directives}}
                                 {|RH7107:get|} { return _a; }
                             }
                         }
                         """;

        var fixedData = $$"""
                          class C
                          {
                              private int _a;

                              public int A
                              {
                          {{directives}}
                                  get { return _a; }
                                  set { _a = value; }
                              }
                          }
                          """;

        await Verify(testData, fixedData);
    }

    /// <summary>
    /// Verifies that a conditional group whose <c>#if</c> stands in front of the crossed set accessor and whose
    /// <c>#endif</c> stands in front of the get accessor does not keep the get accessor from being reported, and that no
    /// code fix is offered, for every shape of the group
    /// </summary>
    /// <param name="directives">The conditional group, and the set accessor it holds, in front of <c>get</c></param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("#if !NEVER\n        set { _a = value; }\n#endif", DisplayName = "Active accessor")]
    [DataRow("#if NEVER\n        init { _a = value; }\n#else\n        set { _a = value; }\n#endif", DisplayName = "If and else")]
    [DataRow("#if NEVER\n        init { _a = value; }\n#elif !NEVER\n        set { _a = value; }\n#endif", DisplayName = "If and elif")]
    [Requirement("RH7107-001")]
    [Requirement("RH7107-005")]
    [Requirement("SHARED-DIAG-008")]
    [Requirement("SHARED-ORDER-003")]
    public async Task ConditionalGroupAroundTheCrossedSetAccessorKeepsTheReportAndWithholdsTheFix(string directives)
    {
        var testData = $$"""
                         class C
                         {
                             private int _a;

                             public int A
                             {
                         {{directives}}
                                 {|RH7107:get|} { return _a; }
                             }
                         }
                         """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that Fix All moves the get accessors whose move is safe and leaves the one whose <c>#endif</c> would be
    /// torn from its <c>#if</c> reported and in place
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7107-001")]
    [Requirement("RH7107-005")]
    [Requirement("SHARED-DIAG-008")]
    public async Task FixAllMovesOnlyTheGetAccessorsWhoseMoveKeepsTheirDirectivesPaired()
    {
        const string testData = """
                                class C
                                {
                                    private int _a;

                                    public int A
                                    {
                                #if !NEVER
                                        set { _a = value; }
                                #endif
                                        {|RH7107:get|} { return _a; }
                                    }

                                    public int B
                                    {
                                        set { _a = value; }
                                        {|RH7107:get|} { return _a; }
                                    }

                                    public int D
                                    {
                                        set { _a = value; }
                                        {|RH7107:get|} { return _a; }
                                    }
                                }
                                """;

        const string fixedData = """
                                 class C
                                 {
                                     private int _a;

                                     public int A
                                     {
                                 #if !NEVER
                                         set { _a = value; }
                                 #endif
                                         {|RH7107:get|} { return _a; }
                                     }

                                     public int B
                                     {
                                         get { return _a; }
                                         set { _a = value; }
                                     }

                                     public int D
                                     {
                                         get { return _a; }
                                         set { _a = value; }
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
    /// Verifies that a conditional directive pair with excluded code at the end of the crossed <c>set</c> accessor's
    /// expression body, in front of its semicolon, does not keep the <c>get</c> accessor from being reported, and that
    /// the fix moves <c>get</c> in front of it with the pair intact
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7107-001")]
    [Requirement("RH7107-004")]
    [Requirement("SHARED-ORDER-006")]
    public async Task ExcludedCodeAtTheEndOfTheExpressionBodyOfTheSetAccessorDoesNotKeepTheGetAccessorFromBeingReported(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  private int _a;

                                  public int A
                                  {
                                      set => _a = value
                              #if DEBUG
                                             + 1
                              #endif
                                             ;
                                      {|RH7107:get|} => _a;
                                  }
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _a;

                                       public int A
                                       {
                                           get => _a;
                                           set => _a = value
                                   #if DEBUG
                                                  + 1
                                   #endif
                                                  ;
                                       }
                                   }
                                   """;

        await VerifyCodeFixWithEndOfLine(source, fixedSource, endOfLine);
    }

    /// <summary>
    /// Verifies that an active conditional directive pair whose <c>#endif</c> stands at the end of the crossed <c>set</c>
    /// accessor's expression body, in front of its semicolon, does not keep the <c>get</c> accessor from being reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7107-001")]
    [Requirement("RH7107-004")]
    public async Task ConditionalDirectiveClosingAtTheEndOfTheExpressionBodyOfTheSetAccessorDoesNotKeepTheGetAccessorFromBeingReported()
    {
        const string source = """
                              class C
                              {
                                  private int _a;

                                  public int A
                                  {
                                      set => _a = value
                              #if !NEVER
                                             + 1
                              #endif
                                             ;
                                      {|RH7107:get|} => _a;
                                  }
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _a;

                                       public int A
                                       {
                                           get => _a;
                                           set => _a = value
                                   #if !NEVER
                                                  + 1
                                   #endif
                                                  ;
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies that a conditional directive pair with excluded code at the end of the moved <c>get</c> accessor's
    /// expression body, in front of its semicolon, does not keep it from being reported, and that the pair travels with it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7107-001")]
    [Requirement("RH7107-004")]
    [Requirement("SHARED-ORDER-006")]
    public async Task ExcludedCodeAtTheEndOfTheExpressionBodyOfTheGetAccessorDoesNotKeepItFromBeingReported()
    {
        const string source = """
                              class C
                              {
                                  private int _a;

                                  public int A
                                  {
                                      set => _a = value;
                                      {|RH7107:get|} => _a
                              #if DEBUG
                                             + 1
                              #endif
                                             ;
                                  }
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _a;

                                       public int A
                                       {
                                           get => _a
                                   #if DEBUG
                                                  + 1
                                   #endif
                                                  ;
                                           set => _a = value;
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