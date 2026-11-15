using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5403StatementMustNotBeOnSingleLineAnalyzer"/> and <see cref="RH5403StatementMustNotBeOnSingleLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5403StatementMustNotBeOnSingleLineAnalyzerTests : BatchCodeFixTestsBase<RH5403StatementMustNotBeOnSingleLineAnalyzer, RH5403StatementMustNotBeOnSingleLineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that clean code does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenCodeIsClean()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        if (true)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the issue is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5403-001")]
    [Requirement("RH5403-002")]
    [Requirement("RH5403-003")]
    public async Task VerifyIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        if (true) {|#0:{|} return; }
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         if (true)
                                         {
                                             return;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5403StatementMustNotBeOnSingleLineAnalyzer.DiagnosticId, AnalyzerResources.RH5403MessageFormat));
    }

    /// <summary>
    /// Verifies that the inserted line breaks match the document's detected CRLF end-of-line sequence instead of
    /// <see cref="System.Environment.NewLine"/>, so the fix does not introduce mixed line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyInsertedLineBreaksUseDetectedCarriageReturnLineFeedEndOfLine()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        if (true) { return; }
                                    }
                                }
                                """;

        var fixedSource = await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(testData));

        Assert.DoesNotContain("\n", fixedSource.Replace("\r\n", string.Empty));
    }

    /// <summary>
    /// Verifies that the complete shared statement-parent family reports populated single-line bodies
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BLOCK-001")]
    [Requirement("RH5403-001")]
    [Requirement("RH5403-002")]
    public async Task VerifyAdditionalStatementParentsAreDetected()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    unsafe void Method((int, int)[] values, IDisposable disposable, int[] buffer)
                                    {
                                        foreach (var (first, second) in values) {|#0:{|} return; }
                                        using (disposable) {|#1:{|} return; }
                                        lock (this) {|#2:{|} return; }
                                        fixed (int* item = buffer) {|#3:{|} return; }
                                        do {|#4:{|} return; } while (false);
                                    }
                                }
                                """;

        await Verify(testData,
                     test => test.SolutionTransforms.Add(ApplyAllowUnsafeToTestProject),
                     Diagnostics(RH5403StatementMustNotBeOnSingleLineAnalyzer.DiagnosticId, AnalyzerResources.RH5403MessageFormat, 5));
    }

    /// <summary>
    /// Verifies that empty covered bodies and populated catch bodies remain outside RH5403
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BLOCK-001")]
    [Requirement("RH5403-001")]
    public async Task VerifyEmptyCoveredBodyAndCatchAreExcluded()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        do { } while (false);

                                        try
                                        {
                                        }
                                        catch { return; }
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a single-line block nested in a lambda body is detected, so the rule reaches blocks that are
    /// not direct descendants of a member declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BLOCK-001")]
    public async Task VerifySingleLineBlockInsideLambdaBodyIsDetected()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        Action action = () =>
                                                        {
                                                            if (true) {|#0:{|} return; }
                                                        };
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5403StatementMustNotBeOnSingleLineAnalyzer.DiagnosticId, AnalyzerResources.RH5403MessageFormat));
    }

    /// <summary>
    /// Verifies that a single-line block nested in a local function is detected
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BLOCK-001")]
    public async Task VerifySingleLineBlockInsideLocalFunctionIsDetected()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        void Local()
                                        {
                                            if (true) {|#0:{|} return; }
                                        }

                                        Local();
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5403StatementMustNotBeOnSingleLineAnalyzer.DiagnosticId, AnalyzerResources.RH5403MessageFormat));
    }

    /// <summary>
    /// Verifies that a block of an if or else statement holding statements on one line is reported on its opening
    /// brace, while an empty block on one line and a lambda body are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5403-001")]
    [Requirement("RH5403-002")]
    public async Task VerifyBlockWithStatementsOnOneLineIsReportedOnBrace()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a)
                                    {
                                        if (a) {|RH5403:{|} M(a); }
                                        else {|RH5403:{|} M(a); M(a); }
                                        while (a) { }
                                        System.Action f = () => { M(a); };
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the blocks of while and for statements holding a statement on one line are reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BLOCK-001")]
    [Requirement("RH5403-001")]
    public async Task VerifyWhileAndForBlocksOnOneLineAreReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a)
                                    {
                                        while (a) {|RH5403:{|} M(a); }
                                        for (var i = 0; i < 1; i++) {|RH5403:{|} M(a); }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that a nested block, checked and unchecked blocks, a block in a switch section, and a method body on
    /// one line are not checked
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BLOCK-001")]
    public async Task VerifyBlocksOutsideControlFlowStatementsAreNotChecked()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        { M(a); }
                                        checked { M(a); }
                                        unchecked { M(a); }
                                        switch (a)
                                        {
                                            case 1: { M(a); break; }
                                        }
                                    }

                                    void N(int a) { M(a); }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the fix lays out the if statement that owns the reported block with each brace and each statement
    /// on its own line, including the block of its else clause
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5403-003")]
    public async Task VerifyFixLaysOutIfStatementWithItsElse()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a)
                                    {
                                        if (a) {|RH5403:{|} M(a); }
                                        else {|RH5403:{|} M(a); M(a); }
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(bool a)
                                     {
                                         if (a)
                                         {
                                             M(a);
                                         }
                                         else
                                         {
                                             M(a);
                                             M(a);
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix lays out the if statement that owns the reported block with each brace and each statement
    /// on its own line, including the block of its else clause, with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5403-003")]
    public async Task VerifyFixLaysOutIfStatementWithItsElseWithCarriageReturnLineFeed()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a)
                                    {
                                        if (a) {|RH5403:{|} M(a); }
                                        else {|RH5403:{|} M(a); M(a); }
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(bool a)
                                     {
                                         if (a)
                                         {
                                             M(a);
                                         }
                                         else
                                         {
                                             M(a);
                                             M(a);
                                         }
                                     }
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testCode), NormalizeToCarriageReturnLineFeed(fixedCode));
    }

    /// <summary>
    /// Verifies that the fix of a one-line loop body that holds a switch statement lays out every switch section and its
    /// first statement on lines of their own, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-BREAK-033")]
    [Requirement("FMT-BREAK-035")]
    public async Task VerifyFixOfABodyHoldingASwitchPutsEverySectionOnItsOwnLine()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int[] xs)
                                    {
                                        foreach (var x in xs) {|RH5403:{|} switch (x) { case 1: continue; case 2: return; } }
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int[] xs)
                                     {
                                         foreach (var x in xs)
                                         {
                                             switch (x)
                                             {
                                                 case 1:
                                                     continue;
                                                 case 2:
                                                     return;
                                             }
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
        await Verify(NormalizeToCarriageReturnLineFeed(testCode), NormalizeToCarriageReturnLineFeed(fixedCode));
    }

    /// <summary>
    /// Verifies that a refused comment in the else block withholds the fix for the single-lined if block: the fix
    /// formats the whole if statement, which the formatter refuses, although the reported block holds no refused shape
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-010")]
    public async Task NoFixIsOfferedWhenTheOwningStatementHoldsACommentTheFormatterRefuses()
    {
        const string source = """
                              internal class C
                              {
                                  private void M(bool a)
                                  {
                                      if (a) {|RH5403:{|} M(a); }
                                      else
                                      {
                                          int /* c */ y = 1;
                                      }
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
                              internal class TestClass
                              {
                                  void Method()
                                  {
                                      if (true) { if (true) { return; } }
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOfferedOnceASyntaxErrorIsAdded(source);
    }

    /// <summary>
    /// Verifies the spec example: no blank line is inserted above a comment that leads the statement whose block the
    /// fix lays out, where formatting the document would insert one
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH5403-003")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixInsertsNoBlankLineAboveCommentThatLeadsTheStatement(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        a = 1;
                                        // c
                                        if (a > 0) {|RH5403:{|} a++; }
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M(int a)
                                     {
                                         a = 1;
                                         // c
                                         if (a > 0)
                                         {
                                             a++;
                                         }
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that no blank line is inserted above the statement whose block the fix lays out when it directly
    /// follows another statement, where formatting the document would insert one
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixInsertsNoBlankLineAboveStatementBehindAnotherStatement(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        a = 1;
                                        if (a > 0) {|RH5403:{|} a++; }
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M(int a)
                                     {
                                         a = 1;
                                         if (a > 0)
                                         {
                                             a++;
                                         }
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that no blank line is inserted above the statement whose block the fix lays out when it follows another
    /// statement of a switch section, and that the statement behind it keeps its line
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixInsertsNoBlankLineAboveStatementBehindAnotherStatementOfASwitchSection(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        switch (a)
                                        {
                                            case 1:
                                                a = 2;
                                                if (a > 0) {|RH5403:{|} a++; }
                                                break;
                                        }
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M(int a)
                                     {
                                         switch (a)
                                         {
                                             case 1:
                                                 a = 2;
                                                 if (a > 0)
                                                 {
                                                     a++;
                                                 }
                                                 break;
                                         }
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that a comment above the statement whose block the fix lays out keeps its column when it stands in
    /// another column than the statement, and that the blank line above it stays
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixKeepsTheColumnOfACommentInAnotherColumnThanTheStatement(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        a = 1;

                                      // c
                                        if (a > 0) {|RH5403:{|} a++; }
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M(int a)
                                     {
                                         a = 1;

                                       // c
                                         if (a > 0)
                                         {
                                             a++;
                                         }
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that a conditional block directly above the statement whose block the fix lays out stays as written,
    /// with no blank line inserted above or below it
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixKeepsADirectiveDirectlyAboveTheStatement(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        a = 1;
                                #if DEBUG
                                        a = 2;
                                #endif
                                        if (a > 0) {|RH5403:{|} a++; }
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M(int a)
                                     {
                                         a = 1;
                                 #if DEBUG
                                         a = 2;
                                 #endif
                                         if (a > 0)
                                         {
                                             a++;
                                         }
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that the one blank line above a comment that leads the statement whose block the fix lays out stays one
    /// blank line
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixKeepsOneBlankLineAboveTheCommentThatLeadsTheStatement(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        a = 1;

                                        // c
                                        if (a > 0) {|RH5403:{|} a++; }
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M(int a)
                                     {
                                         a = 1;

                                         // c
                                         if (a > 0)
                                         {
                                             a++;
                                         }
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that a blank line between the opening brace of the method body and the statement whose block the fix
    /// lays out stays, where formatting the document would remove it
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixKeepsTheBlankLineBetweenTheOpeningBraceAndTheStatement(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void M(int a)
                                    {

                                        if (a > 0) {|RH5403:{|} a++; }
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M(int a)
                                     {

                                         if (a > 0)
                                         {
                                             a++;
                                         }
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that two blank lines above the statement whose block the fix lays out stay, where formatting the
    /// document would collapse them into one
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-006")]
    public async Task VerifyFixKeepsTwoBlankLinesAboveTheStatement(string endOfLine)
    {
        const string testData = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        a = 1;


                                        if (a > 0) {|RH5403:{|} a++; }
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M(int a)
                                     {
                                         a = 1;


                                         if (a > 0)
                                         {
                                             a++;
                                         }
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    /// <summary>
    /// Verifies that the fix of a statement indented with two tabs keeps the tabs in front of it and lays out the block
    /// from column 2, each tab counted as one column
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-006")]
    [Requirement("SHARED-FIX-007")]
    public async Task VerifyFixOfAStatementIndentedWithTabsKeepsTheTabsAndCountsEachAsOneColumn(string endOfLine)
    {
        const string testData = "class C\n{\n    void M(int a)\n    {\n\t\tif (a > 0) {|RH5403:{|} a++; }\n    }\n}";
        const string fixedData = "class C\n{\n    void M(int a)\n    {\n\t\tif (a > 0)\n  {\n      a++;\n  }\n    }\n}";

        await VerifyCodeFixWithEndOfLine(testData, fixedData, endOfLine);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        if (true) {|#0:{|} if (true) {|#1:{|} return; } }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         if (true)
                                         {
                                             if (true)
                                             {
                                                 return;
                                             }
                                         }
                                     }
                                 }
                                 """;

        // The inner if statement is entirely nested inside the outer if's single-line body, so the outer fix's
        // reformatted statement span fully contains the inner occurrence, exercising the batch fixer's
        // overlap-discarding path rather than two independently applied changes
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5403StatementMustNotBeOnSingleLineAnalyzer.DiagnosticId, AnalyzerResources.RH5403MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}