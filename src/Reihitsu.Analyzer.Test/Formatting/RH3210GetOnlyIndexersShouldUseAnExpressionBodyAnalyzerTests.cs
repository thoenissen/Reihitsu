using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Clarity;
using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH3210GetOnlyIndexersShouldUseAnExpressionBodyAnalyzer"/>
/// </summary>
[TestClass]
public class RH3210GetOnlyIndexersShouldUseAnExpressionBodyAnalyzerTests : BatchCodeFixTestsBase<RH3210GetOnlyIndexersShouldUseAnExpressionBodyAnalyzer, RH3210GetOnlyIndexersShouldUseAnExpressionBodyCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying that the diagnostic carries the rule's message and lies on the accessor list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-004")]
    public async Task DiagnosticCarriesTheRuleMessageOnTheAccessorList()
    {
        const string source = """
                              class C
                              {
                                  public int this[int index] {|#0:{ get => index; }|}
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       public int this[int index] => index;
                                   }
                                   """;

        await Verify(source, fixedSource, Diagnostics(RH3210GetOnlyIndexersShouldUseAnExpressionBodyAnalyzer.DiagnosticId, AnalyzerResources.RH3210MessageFormat));
    }

    /// <summary>
    /// Verifying the example of RH3210-001: get-only indexers are reported and converted, and an indexer with a setter and
    /// an expression-bodied indexer are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3210-001")]
    [Requirement("SHARED-GETONLY-001")]
    [Requirement("SHARED-GETONLY-002")]
    [Requirement("SHARED-GETONLY-004")]
    [Requirement("SHARED-GETONLY-005")]
    public async Task GetOnlyIndexersAreReportedAndConverted()
    {
        const string source = """
                              using System;

                              class C
                              {
                                  private readonly int[] _values = new int[3];

                                  public int this[int index] {|RH3210:{ get => _values[index]; }|}

                                  public int this[long index]
                                  {|RH3210:{
                                      get
                                      {
                                          return _values[index];
                                      }
                                  }|}

                                  public int this[string key] {|RH3210:{ get { throw new NotSupportedException(key); } }|}

                                  public int this[short index]
                                  {
                                      get => _values[index];
                                      set => _values[index] = value;
                                  }

                                  public int this[byte index] => _values[index];
                              }
                              """;

        const string fixedSource = """
                                   using System;

                                   class C
                                   {
                                       private readonly int[] _values = new int[3];

                                       public int this[int index] => _values[index];

                                       public int this[long index] => _values[index];

                                       public int this[string key] => throw new NotSupportedException(key);

                                       public int this[short index]
                                       {
                                           get => _values[index];
                                           set => _values[index] = value;
                                       }

                                       public int this[byte index] => _values[index];
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying that an interface indexer with a body and an explicit interface implementation are reported and converted,
    /// and an interface indexer without a body is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3210-001")]
    [Requirement("SHARED-GETONLY-002")]
    public async Task InterfaceAndExplicitImplementationIndexersAreReportedAndConverted()
    {
        const string source = """
                              interface I
                              {
                                  int this[int index] { get; }

                                  int this[long index] {|RH3210:{ get => (int)index; }|}
                              }

                              class C : I
                              {
                                  int I.this[int index] {|RH3210:{ get { return index; } }|}
                              }
                              """;

        const string fixedSource = """
                                   interface I
                                   {
                                       int this[int index] { get; }

                                       int this[long index] => (int)index;
                                   }

                                   class C : I
                                   {
                                       int I.this[int index] => index;
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying that indexers with a <c>ref</c> return and with a <c>params</c> parameter, in a record and a nested type,
    /// are reported and converted
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3210-001")]
    public async Task RefAndParamsIndexersAreReportedAndConverted()
    {
        const string source = """
                              record R
                              {
                                  private readonly int[] _values = new int[3];

                                  public ref int this[long index] {|RH3210:{ get { return ref _values[index]; } }|}

                                  public class Nested
                                  {
                                      public int this[params int[] items] {|RH3210:{ get => items.Length; }|}
                                  }
                              }
                              """;

        const string fixedSource = """
                                   record R
                                   {
                                       private readonly int[] _values = new int[3];

                                       public ref int this[long index] => ref _values[index];

                                       public class Nested
                                       {
                                           public int this[params int[] items] => items.Length;
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying that the fix converts only the accessor list and keeps the spacing of the signature as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-005")]
    public async Task FixKeepsTheSignatureAsWritten()
    {
        const string source = """
                              class C
                              {
                                  public  int  this[ int index ] {|RH3210:{ get { return index; } }|}
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       public  int  this[ int index ] => index;
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying that a comment behind only the closing brace of the accessor list follows the new semicolon
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-003")]
    [Requirement("SHARED-GETONLY-005")]
    public async Task CommentBehindTheClosingBraceFollowsTheNewSemicolon()
    {
        const string source = """
                              class C
                              {
                                  public int this[int index]
                                  {|RH3210:{
                                      get => index;
                                  }|} // a
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       public int this[int index] => index; // a
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying the fix under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-005")]
    public async Task FixKeepsCarriageReturnLineFeedLineEndings()
    {
        const string source = """
                              class C
                              {
                                  public int this[int index]
                                  {|RH3210:{
                                      get
                                      {
                                          return index; // a
                                      }
                                  }|}
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       public int this[int index] => index; // a
                                   }
                                   """;

        await VerifyCodeFixWithEndOfLine(source, fixedSource, "\r\n");
    }

    /// <summary>
    /// Verifying the fix under LF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-005")]
    public async Task FixKeepsLineFeedLineEndings()
    {
        const string source = """
                              class C
                              {
                                  public int this[int index]
                                  {|RH3210:{
                                      get
                                      {
                                          return index;
                                      }
                                  }|}
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       public int this[int index] => index;
                                   }
                                   """;

        await VerifyCodeFixWithEndOfLine(source, fixedSource, "\n");
    }

    /// <summary>
    /// Verifying that properties are not reported by the indexer rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH3210-001")]
    public async Task PropertiesAreNotReported()
    {
        const string source = """
                              class C
                              {
                                  private int _value;

                                  public int A { get => _value; }

                                  public int B
                                  {
                                      get
                                      {
                                          return _value;
                                      }
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying that under C# 6 a block getter that returns is reported and converted, and a getter that throws is not
    /// reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-001")]
    public async Task UnderCSharp6OnlyAReturningGetterIsReported()
    {
        const string source = """
                              using System;

                              class C
                              {
                                  public int this[int index] {|RH3210:{ get { return index; } }|}

                                  public int this[string key] { get { throw new NotSupportedException(key); } }
                              }
                              """;

        const string fixedSource = """
                                   using System;

                                   class C
                                   {
                                       public int this[int index] => index;

                                       public int this[string key] { get { throw new NotSupportedException(key); } }
                                   }
                                   """;

        await Verify(source, fixedSource, test => test.SolutionTransforms.Add((solution, projectId) => ApplyLanguageVersion(solution, projectId, LanguageVersion.CSharp6)));
    }

    /// <summary>
    /// Verifying that under C# 5 a get-only indexer is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-001")]
    public async Task UnderCSharp5NothingIsReported()
    {
        const string source = """
                              class C
                              {
                                  public int this[int index] { get { return index; } }
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add((solution, projectId) => ApplyLanguageVersion(solution, projectId, LanguageVersion.CSharp5)));
    }

    /// <summary>
    /// Verifying that indexers whose shape the formatter keeps are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-002")]
    public async Task IndexersWhoseShapeTheFormatterKeepsAreNotReported()
    {
        const string source = """
                              using System;

                              class C
                              {
                                  private readonly int[] _values = new int[3];

                                  public int this[int index]
                                  {
                                      get => _values[index];
                                      init => _values[index] = value;
                                  }

                                  public int this[long index]
                                  {
                                      [Obsolete]
                                      get => _values[index];
                                  }

                                  public int this[string key]
                                  {
                                      get
                                      {
                                          var index = key.Length;

                                          return _values[index];
                                      }
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifying that comments the formatter keeps the accessor list or the getter block for are reported, and no fix is
    /// offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-003")]
    public async Task IndexersWithCommentsTheFormatterKeepsAreReportedWithoutFix()
    {
        const string source = """
                              class C
                              {
                                  public int this[int index] // note
                                  {|RH3210:{
                                      get => index;
                                  }|}

                                  public int this[long index]
                                  {|RH3210:{
                                      get => (int)index; // a
                                  }|} // b

                                  public int this[string key]
                                  {|RH3210:{
                                      get
                                      {
                                          // length
                                          return key.Length;
                                      }
                                  }|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that a getter the formatter refuses is reported, and no fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-006")]
    public async Task IndexerGetterTheFormatterRefusesIsReportedWithoutFix()
    {
        const string source = """
                              class C
                              {
                                  public int this[int index] {|RH3210:{ get /* c */ => index; }|}

                                  public int this[long index] {|RH3210:{ get => /* c */ (int)index; }|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that directives the formatter keeps the accessor list for are reported, and no fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-007")]
    public async Task IndexersWithDirectivesTheFormatterKeepsAreReportedWithoutFix()
    {
        const string source = """
                              class C
                              {
                                  private readonly int[] _values = new int[3];

                                  public int this[int index]
                                  {|RH3210:{
                                      get => _values[index];
                              #if WRITABLE
                                      set => _values[index] = value;
                              #endif
                                  }|}

                                  public int this[long index]
                                  {|RH3210:{
                              #if DEBUG
                                      get => 1;
                              #else
                                      get => 2;
                              #endif
                                  }|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that an indexer whose <c>set</c> accessor stands in an active branch is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-007")]
    public async Task IndexerWithActiveSetterInConditionalGroupIsNotReported()
    {
        const string source = """
                              class C
                              {
                                  private readonly int[] _values = new int[3];

                                  public int this[int index]
                                  {
                                      get => _values[index];
                              #if WRITABLE
                                      set => _values[index] = value;
                              #endif
                                  }
                              }
                              """;

        await VerifyWithPreprocessorSymbols(source, "WRITABLE");
    }

    /// <summary>
    /// Verifying that a conditional group inside the getter's expression is reported, and the fix writes what the formatter
    /// writes
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-007")]
    [Requirement("SHARED-GETONLY-005")]
    public async Task ConditionalGroupInsideTheExpressionIsReportedAndFixedAsTheFormatterFormats()
    {
        const string source = """
                              class C
                              {
                                  public int this[int index]
                                  {|RH3210:{
                                      get
                                      {
                                          return M(
                              #if DEBUG
                                              index
                              #else
                                              0
                              #endif
                                              );
                                      }
                                  }|}

                                  private static int M(int value)
                                  {
                                      return value;
                                  }
                              }
                              """;

        await VerifyCodeFixMatchesFormatter(source);
    }

    /// <summary>
    /// Verifying that no fix is offered in a file with a syntax error, while the diagnostic is still reported, and that it
    /// is offered once the syntax error is gone
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-011")]
    public async Task NoFixIsOfferedInAFileWithASyntaxError()
    {
        const string source = """
                              class C
                              {
                                  public int this[int index] { get => index; }

                                  public int this[long index] { get { return (int)index; } }
                              }
                              """;

        await VerifyNoCodeFixIsOfferedOnceASyntaxErrorIsAdded(source);
    }

    /// <summary>
    /// Verifying the negative example <c>B</c> of SHARED-GETONLY-003 on an indexer: a delimited comment on a line of its own
    /// in front of the getter's semicolon withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-003")]
    public async Task DelimitedCommentOnALineOfItsOwnInFrontOfTheGettersSemicolonWithholdsFix()
    {
        const string source = """
                              class C
                              {
                                  public int this[int index]
                                  {|RH3210:{
                                      get => index
                                          /* d */ ;
                                  }|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying the block variant of the negative example <c>B</c> of SHARED-GETONLY-003 on an indexer: a delimited comment
    /// on a line of its own in front of the semicolon of the getter block's <c>return</c> statement withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-GETONLY-003")]
    public async Task DelimitedCommentOnALineOfItsOwnInFrontOfTheReturnSemicolonWithholdsFix()
    {
        const string source = """
                              class C
                              {
                                  public int this[int index]
                                  {|RH3210:{
                                      get
                                      {
                                          return index
                                              /* e */ ;
                                      }
                                  }|}
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that a member behind the closing brace of a multi-line accessor list, on the same line, starts on the line
    /// after the converted indexer, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-007")]
    public async Task FixPutsAMemberBehindTheClosingBraceOnTheNextLine(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  public int this[int i]
                                  {|RH3210:{
                                      get => i;
                                  }|} public void M() { }
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       public int this[int i] => i;
                                       public void M() { }
                                   }
                                   """;

        await VerifyCodeFixWithEndOfLine(source, fixedSource, endOfLine);
    }

    /// <summary>
    /// Verifying that the converted indexer starts on the line after a member in front of it on the same line, under LF and
    /// CRLF
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-007")]
    public async Task FixPutsTheConvertedIndexerOnTheLineAfterAMemberInFrontOfIt(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  public int B => 1; public int this[int i] {|RH3210:{ get { return i; } }|}
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       public int B => 1;
                                       public int this[int i] => i;
                                   }
                                   """;

        await VerifyCodeFixWithEndOfLine(source, fixedSource, endOfLine);
    }

    /// <summary>
    /// Verifying that fixing two reported indexers that share one line, one at a time and with Fix All, gives each of them
    /// a line of its own, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-007")]
    public async Task FixAllPutsTwoConvertedIndexersOfOneLineOnLinesOfTheirOwn(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  public int this[int i] {|RH3210:{ get => i; }|} public int this[long l] {|RH3210:{ get => (int)l; }|}
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       public int this[int i] => i;
                                       public int this[long l] => (int)l;
                                   }
                                   """;

        await Verify(ConvertEndOfLine(source, endOfLine), ConvertEndOfLine(fixedSource, endOfLine));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Sets the language version of the test project
    /// </summary>
    /// <param name="solution">Solution</param>
    /// <param name="projectId">Project ID</param>
    /// <param name="languageVersion">Language version</param>
    /// <returns>The solution with the language version applied</returns>
    private static Solution ApplyLanguageVersion(Solution solution, ProjectId projectId, LanguageVersion languageVersion)
    {
        return solution.GetProject(projectId)?.ParseOptions is CSharpParseOptions parseOptions
                   ? solution.WithProjectParseOptions(projectId, parseOptions.WithLanguageVersion(languageVersion))
                   : solution;
    }

    #endregion // Methods

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string source = """
                              using System;

                              class C
                              {
                                  private readonly int[] _values = new int[3];

                                  public int Count { get => _values.Length; }

                                  public int this[int index] {|#0:{ get => _values[index]; }|}

                                  public int this[long index]
                                  {|#1:{
                                      get
                                      {
                                          return _values[index];
                                      }
                                  }|}

                                  public class Nested
                                  {
                                      public int this[string key] {|#2:{ get { throw new NotSupportedException(key); } }|}
                                  }
                              }
                              """;

        const string fixedSource = """
                                   using System;

                                   class C
                                   {
                                       private readonly int[] _values = new int[3];

                                       public int Count { get => _values.Length; }

                                       public int this[int index] => _values[index];

                                       public int this[long index] => _values[index];

                                       public class Nested
                                       {
                                           public int this[string key] => throw new NotSupportedException(key);
                                       }
                                   }
                                   """;

        return new FixAllScenario(source,
                                  fixedSource,
                                  Diagnostics(RH3210GetOnlyIndexersShouldUseAnExpressionBodyAnalyzer.DiagnosticId, AnalyzerResources.RH3210MessageFormat, 3));
    }

    #endregion // BatchCodeFixTestsBase
}