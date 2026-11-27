using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer"/> and <see cref="RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzerTests : BatchCodeFixTestsBase<RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer, RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that correctly formatted generic constraints do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5110-001")]
    public async Task VerifyNoDiagnosticsWhenGenericConstraintsAreFormattedCorrectly()
    {
        const string testData = """
                                internal class Example<T>
                                    where T : class
                                {
                                }

                                internal struct ExampleStruct<T>
                                    where T : struct
                                {
                                }

                                internal interface IExample<T>
                                    where T : class
                                {
                                }

                                internal delegate void ExampleDelegate<T>()
                                    where T : class;

                                internal record ExampleRecord<T>(T Value)
                                    where T : class;

                                internal class Container
                                {
                                    internal void Method<TKey, TValue>()
                                        where TKey : notnull
                                        where TValue : class
                                    {
                                        void Local<TLocal>()
                                            where TLocal : class
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that class generic constraint on the wrong line is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5110-001")]
    [Requirement("RH5110-003")]
    public async Task VerifyClassConstraintIsDetectedAndFixed()
    {
        const string testData = """
                                internal class Example<T> {|#0:where|} T : class
                                {
                                    public int Bar { get; set; }
                                }
                                """;
        const string fixedData = """
                                 internal class Example<T>
                                     where T : class
                                 {
                                     public int Bar { get; set; }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that struct generic constraint on the wrong line is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5110-001")]
    [Requirement("RH5110-003")]
    public async Task VerifyStructConstraintIsDetectedAndFixed()
    {
        const string testData = """
                                internal struct ExampleStruct<T>
                                {|#0:where|} T : struct
                                {
                                    public int Bar { get; set; }
                                }
                                """;
        const string fixedData = """
                                 internal struct ExampleStruct<T>
                                     where T : struct
                                 {
                                     public int Bar { get; set; }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that interface generic constraint on the wrong line is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5110-001")]
    [Requirement("RH5110-003")]
    public async Task VerifyInterfaceConstraintIsDetectedAndFixed()
    {
        const string testData = """
                                internal interface IExample<T> {|#0:where|} T : class
                                {
                                    public int Bar { get; set; }
                                }
                                """;
        const string fixedData = """
                                 internal interface IExample<T>
                                     where T : class
                                 {
                                     public int Bar { get; set; }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that delegate generic constraint on the wrong line is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5110-001")]
    [Requirement("RH5110-003")]
    public async Task VerifyDelegateConstraintIsDetectedAndFixed()
    {
        const string testData = """
                                internal delegate void ExampleDelegate<T>()
                                {|#0:where|} T : class;
                                """;
        const string fixedData = """
                                 internal delegate void ExampleDelegate<T>()
                                     where T : class;
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that record generic constraint on the wrong line is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5110-001")]
    [Requirement("RH5110-003")]
    public async Task VerifyRecordConstraintIsDetectedAndFixed()
    {
        const string testData = """
                                internal record ExampleRecord<T>(T Value) {|#0:where|} T : class;
                                """;
        const string fixedData = """
                                 internal record ExampleRecord<T>(T Value)
                                     where T : class;
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that method generic constraint on the wrong line is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5110-001")]
    [Requirement("RH5110-002")]
    [Requirement("RH5110-003")]
    public async Task VerifyMethodConstraintIsDetectedAndFixed()
    {
        const string testData = """
                                internal class Container
                                {
                                    internal void Method<TKey, TValue>() {|#0:where|} TKey : notnull
                                    where TValue : class
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class Container
                                 {
                                     internal void Method<TKey, TValue>()
                                         where TKey : notnull
                                         where TValue : class
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that local function generic constraint on the wrong line is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5110-001")]
    [Requirement("RH5110-003")]
    public async Task VerifyLocalFunctionConstraintIsDetectedAndFixed()
    {
        const string testData = """
                                internal class Container
                                {
                                    internal void Method()
                                    {
                                        void Local<TLocal>()
                                        {|#0:where|} TLocal : class
                                        {
                                        }
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class Container
                                 {
                                     internal void Method()
                                     {
                                         void Local<TLocal>()
                                             where TLocal : class
                                         {
                                         }
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a constraint clause that starts its own line in another column than one level deeper than the
    /// declaration is detected and moved to that column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5110-001")]
    [Requirement("RH5110-003")]
    public async Task VerifyConstraintInWrongColumnIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method<TKey>()
                                            {|#0:where|} TKey : notnull
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method<TKey>()
                                         where TKey : notnull
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a constraint clause at the declaration's own column is reported, although the next clause is
    /// placed correctly
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5110-001")]
    [Requirement("RH5110-002")]
    public async Task VerifyConstraintAtDeclarationColumnIsDetected()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method<TKey, TValue>()
                                    {|#0:where|} TKey : notnull
                                        where TValue : class
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a second constraint clause sharing the line of the first one is reported, although the first
    /// clause is placed correctly
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5110-001")]
    [Requirement("RH5110-002")]
    public async Task VerifySecondConstraintSharingLineIsDetected()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method<TKey, TValue>()
                                        where TKey : notnull {|#0:where|} TValue : class
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that two misplaced clauses of one declaration produce one diagnostic on the first, and that the fix
    /// puts each clause on its own line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5110-002")]
    [Requirement("RH5110-003")]
    public async Task VerifyOneDiagnosticIsReportedPerDeclaration()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method<TKey, TValue>() {|#0:where|} TKey : notnull where TValue : class
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method<TKey, TValue>()
                                         where TKey : notnull
                                         where TValue : class
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix puts the constraint clause on its own line under CRLF line endings and keeps them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5110-003")]
    public async Task VerifyMethodConstraintIsFixedWithCrLf()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method<TKey>() {|#0:where|} TKey : notnull
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method<TKey>()
                                         where TKey : notnull
                                     {
                                     }
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(fixedData), Diagnostics(RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix lays out the constraints of the clauses it moves: a clause wrapped behind a comma becomes one
    /// constraint per line, aligned under the first constraint, and a constraint wrapped behind the colon is joined onto
    /// the colon's line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5110-003")]
    public async Task VerifyFixLaysOutWrappedConstraints()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    void M<T>() {|#0:where|} T : class,
                                        new()
                                    {
                                    }

                                    void N<T>() {|#1:where|} T :
                                        IDisposable
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System;

                                 internal class TestClass
                                 {
                                     void M<T>()
                                         where T : class,
                                                   new()
                                     {
                                     }

                                     void N<T>()
                                         where T : IDisposable
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that the fix lays out a partly wrapped clause one constraint per line under CRLF line endings and keeps them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5110-003")]
    public async Task VerifyFixLaysOutWrappedConstraintsWithCrLf()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    void M<T>() {|#0:where|} T : class, IDisposable,
                                        new()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System;

                                 internal class TestClass
                                 {
                                     void M<T>()
                                         where T : class,
                                                   IDisposable,
                                                   new()
                                     {
                                     }
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(fixedData), Diagnostics(RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies the spec example: the fix is offered where a comment behind the colon keeps its line break, and it lays out the
    /// lines around it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5110-003")]
    public async Task VerifyFixIsOfferedWhereCommentBlocksJoin()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void M<T>() {|#0:where|} T : // c
                                        class, new()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void M<T>()
                                         where T : // c
                                                   class,
                                                   new()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies the spec example: a comment in front of a comma on a later line is a shape the formatter refuses, so the clause
    /// is reported and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5110-003")]
    public async Task VerifyNoCodeFixWhereCommentStandsInFrontOfACommaOnALaterLine()
    {
        const string testData = """
                                class C
                                {
                                    void N<T>() {|RH5110:where|} T : class // c
                                        , new()
                                    {
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a directive in front of a comma on a later line withholds the fix as the comment does
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5110-003")]
    public async Task VerifyNoCodeFixWhereDirectiveStandsInFrontOfACommaOnALaterLine()
    {
        const string testData = """
                                using System;

                                class C
                                {
                                    void N<T>() {|RH5110:where|} T : IDisposable
                                #if !X
                                        , new()
                                #endif
                                    {
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that the fix is offered where a comment behind the type parameter keeps the colon on its own line, and
    /// that it indents the colon one level deeper than <c>where</c> with the constraints aligned behind it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5110-003")]
    public async Task VerifyFixIsOfferedWhereCommentKeepsColonOnItsOwnLine()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void M<T>() {|#0:where|} T // c
                                        : class, new()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void M<T>()
                                         where T // c
                                             : class,
                                               new()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix is offered where a directive behind the colon keeps the first constraint on the next line,
    /// and that it aligns the constraints to the column behind the colon
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5110-003")]
    public async Task VerifyFixIsOfferedWhereDirectiveKeepsFirstConstraintOnNextLine()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void M<T>() {|#0:where|} T :
                                #if !X
                                        class,
                                #endif
                                        new()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void M<T>()
                                         where T :
                                 #if !X
                                                   class,
                                 #endif
                                                   new()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix gives a tight constraint colon the formatter's spacing: one space on each side
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5110-003")]
    public async Task VerifyFixSpacesTightConstraintColon()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void M<T>() {|#0:where|} T:class
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void M<T>()
                                         where T : class
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a refused comment in a member of the constrained type withholds the fix: the fix formats the
    /// declaration that owns the constraint clause, which the formatter refuses, although the clause holds no refused
    /// shape
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-010")]
    public async Task NoFixIsOfferedWhenTheConstrainedDeclarationHoldsACommentTheFormatterRefuses()
    {
        const string source = """
                              internal class Example<T> {|RH5110:where|} T : class
                              {
                                  private void M()
                                  {
                                      int /* c */ y = 1;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that the fix, which lays out its result through the formatter, is not offered in a file with a syntax error,
    /// while the diagnostic is still reported, and that it is offered once the syntax error is gone
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-011")]
    public async Task NoFixIsOfferedInAFileWithASyntaxError()
    {
        const string source = """
                              internal class Container
                              {
                                  internal void Outer<TOuter>() where TOuter : class
                                  {
                                      void Local<TLocal>()
                                      where TLocal : class
                                      {
                                      }
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOfferedOnceASyntaxErrorIsAdded(source);
    }

    /// <summary>
    /// Verifies that a constraint clause behind a comment that starts its line outside the constraint column is reported, although the <c>where</c> keyword behind the comment reaches the constraint column, and that the code fix is withheld or lays the line out in one pass
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5110-001")]
    [Requirement("SHARED-COMMENT-011")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyConstraintBehindMisplacedLeadingCommentIsReported(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void N<T>()
                                /* c */ {|RH5110:where|} T : class
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void N<T>()
                                         /* c */ where T : class
                                     {
                                     }
                                 }
                                 """;

        await VerifyCodeFixIsWithheldOrProducesOneOf(ConvertEndOfLine(testData, endOfLine), true, ConvertEndOfLine(fixedData, endOfLine));
    }

    /// <summary>
    /// Verifies the spec example: a comment that leads the method whose constraint clause the fix lays out stays as
    /// written, with no blank line inserted above it
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5110-003")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixInsertsNoBlankLineAboveCommentThatLeadsTheDeclaration(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    int _a;
                                    // c
                                    void M<T>() {|RH5110:where|} T : class
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     int _a;
                                     // c
                                     void M<T>()
                                         where T : class
                                     {
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that a method whose constraint clause the fix moves, and which shares its line with a field in front of it,
    /// starts on the next line in the column of that line, and that its constraint clause and its block are laid out from there
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyFixGivesAMethodThatSharesItsLineWithAFieldInFrontALineOfItsOwn(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    private int _a; public void M<T>() {|RH5110:where|} T : class
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     private int _a;
                                     public void M<T>()
                                         where T : class
                                     {
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that a method whose constraint clause the fix moves, and which shares its line with a field behind it,
    /// keeps its own line, and that the field behind it starts on the next line
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyFixGivesAMethodThatSharesItsLineWithAFieldBehindItALineOfItsOwn(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    public void N<T>() {|RH5110:where|} T : class
                                    {
                                    } private int _c;
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     public void N<T>()
                                         where T : class
                                     {
                                     }
                                     private int _c;
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies the union example of RH5110-001: a constraint clause of a bodiless union on the line of the case-type
    /// list is reported and moved onto its own line in front of the semicolon, and a union whose clause stands on its
    /// own line is not reported
    /// </summary>
    /// <param name="endOfLine">Line break of the source and the expected result</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5110-001")]
    [Requirement("RH5110-003")]
    [DataRow("\n", DisplayName = "LF")]
    [DataRow("\r\n", DisplayName = "CRLF")]
    public async Task UnionConstraintClauseOnDeclarationLineIsReportedAndMoved(string endOfLine)
    {
        const string testCode = """
                                union U<T>(T, string) {|RH5110:where|} T : class;

                                union V<T>(T, string)
                                    where T : class
                                {
                                    public int X => 1;
                                }
                                """;

        const string fixedCode = """
                                 union U<T>(T, string)
                                     where T : class;

                                 union V<T>(T, string)
                                     where T : class
                                 {
                                     public int X => 1;
                                 }
                                 """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(ConvertEndOfLine(testCode, endOfLine), ConvertEndOfLine(fixedCode, endOfLine), test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    /// <summary>
    /// Verifies that a union with several constraint clauses is reported once, on the first misplaced clause, and that
    /// the fix puts each clause on its own line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5110-002")]
    [Requirement("RH5110-003")]
    public async Task UnionWithSeveralConstraintClausesIsReportedOnceAndEachClauseMoved()
    {
        const string testCode = """
                                union W<T1, T2>(T1, T2) {|RH5110:where|} T1 : class where T2 : struct;
                                """;

        const string fixedCode = """
                                 union W<T1, T2>(T1, T2)
                                     where T1 : class
                                     where T2 : struct;
                                 """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(testCode, fixedCode, test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    /// <summary>
    /// Verifies that the constraint clause of a union nested in a class is reported and moved onto its own line, four
    /// spaces right of the union's first token
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5110-001")]
    [Requirement("RH5110-003")]
    public async Task NestedUnionConstraintClauseIsReportedAndMoved()
    {
        const string testCode = """
                                public class C
                                {
                                    union N<T>(T, string) {|RH5110:where|} T : class
                                    {
                                        public int X => 1;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class C
                                 {
                                     union N<T>(T, string)
                                         where T : class
                                     {
                                         public int X => 1;
                                     }
                                 }
                                 """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(testCode, fixedCode, test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    /// <summary>
    /// Verifies that a union constraint clause on its own line in the right column is not reported behind an end-of-
    /// line comment, and is reported in another column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5110-001")]
    public async Task UnionConstraintClauseColumnIsJudgedBehindComment()
    {
        const string testCode = """
                                union U<T>(T, string) // note
                                    where T : class;

                                union V<T>(T, string)
                                  {|RH5110:where|} T : class;
                                """;

        const string fixedCode = """
                                 union U<T>(T, string) // note
                                     where T : class;

                                 union V<T>(T, string)
                                     where T : class;
                                 """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(testCode, fixedCode, test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    /// <summary>
    /// Verifies that a union constraint clause on its own line in the right column is not reported behind a directive
    /// line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5110-001")]
    public async Task UnionConstraintClauseBehindDirectiveIsNotReported()
    {
        const string testCode = """
                                union U<T>(T, string)
                                #pragma warning disable CS0168
                                    where T : class;
                                """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(testCode, testCode, test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    /// <summary>
    /// Verifies that a field declaration with several declarators and a directive between them in the constrained type withholds
    /// the fix: the fix formats the declaration that owns the constraint clause, which the formatter refuses
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-010")]
    [Requirement("FMT-UNSUP-010")]
    public async Task NoFixIsOfferedWhenTheConstrainedTypeHoldsACombinedFieldWithADirectiveInside()
    {
        const string source = """
                              internal class Example<T> {|RH5110:where|} T : class
                              {
                                  private int _a,
                              #pragma warning disable CS0169
                                      _b;
                              #pragma warning restore CS0169
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a region in front of a combined field of the constrained type does not withhold the fix, which formats the
    /// type and splits the field behind the region as the formatter splits it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-STRUCT-022")]
    public async Task FixSplitsACombinedFieldBehindARegionOfTheConstrainedType()
    {
        const string source = """
                              internal class Example<T> {|RH5110:where|} T : class
                              {
                                  #region Fields

                                  private int _a, _b;

                                  #endregion // Fields
                              }
                              """;

        const string fixedSource = """
                                   internal class Example<T>
                                       where T : class
                                   {
                                       #region Fields

                                       private int _a;
                                       private int _b;

                                       #endregion // Fields
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies the change example: a comment behind the constraint clause in front of the arrow withholds the fix, because the
    /// formatter refuses the method the fix formats, while the diagnostic stays
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-010")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task VerifyNoFixIsOfferedForACommentInFrontOfTheArrow(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  T Make<T>() {|RH5110:where|} T : new() // c
                                      => new T();
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies a constraint clause of a local function inside an interpolation of a non-verbatim interpolated string, also behind a verbatim one, is not
    /// reported when the code is compiled with C# 10, while a constraint clause of a local function that only contains an interpolated string is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5110-004")]
    public async Task ConstraintClauseInsideANonVerbatimInterpolationIsNotReportedBeforeCSharp11()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    static int F(Func<int> f) => f();

                                    string M() => $"a {F(() => { int L<T>() where T : class => 1; return L<string>(); })} b";

                                    string Q() => $@"x {$"a {F(() => { int L<T>() where T : class => 1; return L<string>(); })} b"} y";

                                    void N()
                                    {
                                        int L<T>() {|RH5110:where|} T : class => $"{1}".Length;
                                    }
                                }
                                """;

        await Verify(testCode, test => test.SolutionTransforms.Add(CreateLanguageVersionTransform(LanguageVersion.CSharp10)));
    }

    /// <summary>
    /// Verifies a constraint clause of a local function inside an interpolation of a non-verbatim interpolated string, also behind a verbatim one, is
    /// reported when the code is compiled with C# 11
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5110-004")]
    public async Task ConstraintClauseInsideANonVerbatimInterpolationIsReportedFromCSharp11()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    static int F(Func<int> f) => f();

                                    string M() => $"a {F(() => { int L<T>() {|RH5110:where|} T : class => 1; return L<string>(); })} b";

                                    string Q() => $@"x {$"a {F(() => { int L<T>() {|RH5110:where|} T : class => 1; return L<string>(); })} b"} y";
                                }
                                """;

        await Verify(testCode, test => test.SolutionTransforms.Add(CreateLanguageVersionTransform(LanguageVersion.CSharp11)));
    }

    /// <summary>
    /// Verifies a constraint clause of a local function inside an interpolation of a verbatim interpolated string, written <c>$@</c> or <c>@$</c>, is
    /// reported when the code is compiled with C# 10
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5110-004")]
    public async Task ConstraintClauseInsideAVerbatimInterpolationIsReportedBeforeCSharp11()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    static int F(Func<int> f) => f();

                                    string M() => $@"a {F(() => { int L<T>() {|RH5110:where|} T : class => 1; return L<string>(); })} b";

                                    string P() => @$"a {F(() => { int L<T>() {|RH5110:where|} T : class => 1; return L<string>(); })} b";
                                }
                                """;

        await Verify(testCode, test => test.SolutionTransforms.Add(CreateLanguageVersionTransform(LanguageVersion.CSharp10)));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class Container
                                {
                                    internal void Outer<TOuter>() {|#0:where|} TOuter : class
                                    {
                                        void Local<TLocal>()
                                        {|#1:where|} TLocal : class
                                        {
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class Container
                                 {
                                     internal void Outer<TOuter>()
                                         where TOuter : class
                                     {
                                         void Local<TLocal>()
                                             where TLocal : class
                                         {
                                         }
                                     }
                                 }
                                 """;

        // The outer diagnostic's fix reformats the whole outer method declaration, whose span fully contains the
        // local function rewritten by the inner diagnostic's fix, so the batch fixer discards the overlapping
        // inner change in its first pass; the outer rewrite already carries the correctly formatted nested
        // constraint clause, but the batch fixer still needs a second pass to converge because its first pass
        // re-analyzes and re-applies the (now redundant) inner fix before recognizing it is a no-op
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer.DiagnosticId, MessageFormat, 2),
                                  test => test.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}