using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH7004UsingDeclarationsShouldNotBeUsedAnalyzer"/> and <see cref="RH7004UsingDeclarationsShouldNotBeUsedCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH7004UsingDeclarationsShouldNotBeUsedAnalyzerTests : BatchCodeFixTestsBase<RH7004UsingDeclarationsShouldNotBeUsedAnalyzer, RH7004UsingDeclarationsShouldNotBeUsedCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic is reported when a using declaration is used
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7004-001")]
    [Requirement("RH7004-003")]
    public async Task VerifyDiagnosticWhenUsingDeclarationIsUsed()
    {
        const string testCode = """
                                using System.IO;

                                internal class RH7004
                                {
                                    public void Execute()
                                    {
                                        {|#0:using|} var stream = new MemoryStream();
                                        _ = stream;
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH7004UsingDeclarationsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH7004MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported when a using declaration uses an explicit type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7004-001")]
    public async Task VerifyDiagnosticWhenUsingDeclarationIsUsedWithExplicitType()
    {
        const string testCode = """
                                using System.IO;

                                internal class RH7004
                                {
                                    public void Execute()
                                    {
                                        {|#0:using|} System.IO.MemoryStream stream = new MemoryStream();
                                        _ = stream;
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH7004UsingDeclarationsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH7004MessageFormat));
    }

    /// <summary>
    /// Verifies diagnostics are reported for multiple using declarations
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7004-001")]
    [Requirement("RH7004-003")]
    public async Task VerifyDiagnosticWhenMultipleUsingDeclarationsArePresent()
    {
        const string testCode = """
                                using System.IO;

                                internal class RH7004
                                {
                                    public void Execute()
                                    {
                                        {|#0:using|} var first = new MemoryStream();
                                        {|#1:using|} var second = new MemoryStream();
                                        _ = first;
                                        _ = second;
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH7004UsingDeclarationsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH7004MessageFormat, 2));
    }

    /// <summary>
    /// Verifies a diagnostic is reported when an await using declaration is used
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7004-001")]
    [Requirement("RH7004-003")]
    public async Task VerifyDiagnosticWhenAwaitUsingDeclarationIsUsed()
    {
        const string testCode = """
                                using System.Threading.Tasks;

                                internal class RH7004
                                {
                                    public async Task ExecuteAsync()
                                    {
                                        await {|#0:using|} var resource = new AsyncResource();
                                        _ = resource;
                                    }

                                    private sealed class AsyncResource : System.IAsyncDisposable
                                    {
                                        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH7004UsingDeclarationsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH7004MessageFormat));
    }

    /// <summary>
    /// Verifies the code fix preserves the await keyword when transforming an await using declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7004-004")]
    public async Task VerifyCodeFixPreservesAwaitKeywordForAwaitUsingDeclaration()
    {
        const string testCode = """
                                using System.Threading.Tasks;

                                internal class RH7004
                                {
                                    public async Task ExecuteAsync()
                                    {
                                        await {|#0:using|} var resource = new AsyncResource();
                                        Consume(resource);
                                    }

                                    private void Consume(AsyncResource resource)
                                    {
                                    }

                                    private sealed class AsyncResource : System.IAsyncDisposable
                                    {
                                        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.Threading.Tasks;

                                 internal class RH7004
                                 {
                                     public async Task ExecuteAsync()
                                     {
                                         await using (var resource = new AsyncResource())
                                         {
                                             Consume(resource);
                                         }
                                     }

                                     private void Consume(AsyncResource resource)
                                     {
                                     }

                                     private sealed class AsyncResource : System.IAsyncDisposable
                                     {
                                         public ValueTask DisposeAsync() => ValueTask.CompletedTask;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7004UsingDeclarationsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH7004MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostic is reported when a using statement block is used
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7004-002")]
    public async Task VerifyNoDiagnosticWhenUsingStatementBlockIsUsed()
    {
        const string testCode = """
                                using System.IO;

                                internal class RH7004
                                {
                                    public void Execute()
                                    {
                                        using (var stream = new MemoryStream())
                                        {
                                            _ = stream;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostic is reported when an await using statement block is used
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7004-002")]
    public async Task VerifyNoDiagnosticWhenAwaitUsingStatementBlockIsUsed()
    {
        const string testCode = """
                                using System.Threading.Tasks;

                                internal class RH7004
                                {
                                    public async Task ExecuteAsync()
                                    {
                                        await using (var resource = new AsyncResource())
                                        {
                                            _ = resource;
                                        }
                                    }

                                    private sealed class AsyncResource : System.IAsyncDisposable
                                    {
                                        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostic is reported when a using directive is used
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticWhenUsingDirectiveIsUsed()
    {
        const string testCode = """
                                using System;

                                internal class RH7004
                                {
                                    public void Execute()
                                    {
                                        Console.WriteLine(string.Empty);
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the code fix transforms a using declaration into a using statement block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7004-004")]
    public async Task VerifyCodeFixTransformsUsingDeclarationToUsingStatementBlock()
    {
        const string testCode = """
                                using System.IO;

                                internal class RH7004
                                {
                                    public void Execute()
                                    {
                                        {|#0:using|} var stream = new MemoryStream();
                                        Consume(stream);
                                    }

                                    private void Consume(MemoryStream stream)
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.IO;

                                 internal class RH7004
                                 {
                                     public void Execute()
                                     {
                                         using (var stream = new MemoryStream())
                                         {
                                             Consume(stream);
                                         }
                                     }

                                     private void Consume(MemoryStream stream)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7004UsingDeclarationsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH7004MessageFormat));
    }

    /// <summary>
    /// Verifies the code fix creates an empty using statement block when no statements follow
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7004-004")]
    public async Task VerifyCodeFixTransformsUsingDeclarationWithNoFollowingStatements()
    {
        const string testCode = """
                                using System.IO;

                                internal class RH7004
                                {
                                    public void Execute()
                                    {
                                        {|#0:using|} var stream = new MemoryStream();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.IO;

                                 internal class RH7004
                                 {
                                     public void Execute()
                                     {
                                         using (var stream = new MemoryStream())
                                         {
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7004UsingDeclarationsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH7004MessageFormat));
    }

    /// <summary>
    /// Verifies the code fix moves multiple following statements into the using statement block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixTransformsUsingDeclarationWithMultipleFollowingStatements()
    {
        const string testCode = """
                                using System;
                                using System.IO;

                                internal class RH7004
                                {
                                    public void Execute()
                                    {
                                        var value = 0;
                                        {|#0:using|} var stream = new MemoryStream();
                                        Consume(stream);
                                        value++;
                                        Console.WriteLine(value);
                                    }

                                    private void Consume(MemoryStream stream)
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;
                                 using System.IO;

                                 internal class RH7004
                                 {
                                     public void Execute()
                                     {
                                         var value = 0;

                                         using (var stream = new MemoryStream())
                                         {
                                             Consume(stream);
                                             value++;
                                             Console.WriteLine(value);
                                         }
                                     }

                                     private void Consume(MemoryStream stream)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7004UsingDeclarationsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH7004MessageFormat));
    }

    /// <summary>
    /// Verifies the code fix is not offered when a local function follows the using declaration, because wrapping it
    /// into the using body would hide it from earlier statements (CS0103)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7004-006")]
    public async Task VerifyCodeFixIsNotOfferedWhenFollowingStatementsContainLocalFunction()
    {
        const string testCode = """
                                using System.IO;

                                internal class RH7004
                                {
                                    public void Execute()
                                    {
                                        Helper();
                                        using var stream = new MemoryStream();

                                        void Helper()
                                        {
                                        }
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testCode,
                                                   RH7004UsingDeclarationsShouldNotBeUsedAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<LocalDeclarationStatementSyntax>()
                                                               .First(static declaration => declaration.UsingKeyword.RawKind != 0)
                                                               .UsingKeyword
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies the code fix is not offered when a labeled statement follows the using declaration, because wrapping it
    /// into the using body would break a <c>goto</c> from earlier statements (CS0159)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7004-006")]
    public async Task VerifyCodeFixIsNotOfferedWhenFollowingStatementsContainLabeledStatement()
    {
        const string testCode = """
                                using System.IO;

                                internal class RH7004
                                {
                                    public void Execute()
                                    {
                                        goto Cleanup;
                                        using var stream = new MemoryStream();

                                    Cleanup:
                                        _ = stream;
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testCode,
                                                   RH7004UsingDeclarationsShouldNotBeUsedAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<LocalDeclarationStatementSyntax>()
                                                               .First(static declaration => declaration.UsingKeyword.RawKind != 0)
                                                               .UsingKeyword
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies the specification example: using declarations with <c>var</c>, an explicit type, several declarators,
    /// <c>await</c>, and inside a lambda are each reported once on their <c>using</c> keyword
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7004-001")]
    [Requirement("RH7004-003")]
    public async Task EveryKindOfUsingDeclarationIsReportedOnItsUsingKeyword()
    {
        const string source = """
                              using System;
                              using System.IO;
                              using System.Threading.Tasks;

                              class C
                              {
                                  async Task M(IAsyncDisposable resource)
                                  {
                                      {|RH7004:using|} var stream = new MemoryStream();
                                      {|RH7004:using|} MemoryStream first = new MemoryStream(), second = new MemoryStream();
                                      await {|RH7004:using|} var handle = resource;

                                      Action action = () =>
                                      {
                                          {|RH7004:using|} var inner = new MemoryStream();
                                      };
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies that using declarations in a local function body and in a nested block are reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7004-001")]
    public async Task UsingDeclarationsInLocalFunctionsAndNestedBlocksAreReported()
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  void M(bool condition)
                                  {
                                      if (condition)
                                      {
                                          {|RH7004:using|} var stream = new MemoryStream();
                                      }

                                      void Local()
                                      {
                                          {|RH7004:using|} var inner = new MemoryStream();
                                      }
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies the specification example: a <c>using</c> statement is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7004-002")]
    public async Task UsingStatementIsNotReported()
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  void M()
                                  {
                                      using (var stream = new MemoryStream())
                                      {
                                          stream.Flush();
                                      }
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies the specification example: the fix wraps the following statement into the block of a new
    /// <c>using</c> statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7004-004")]
    public async Task FixWrapsTheFollowingStatementIntoAUsingStatement()
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  void M()
                                  {
                                      {|RH7004:using|} var stream = new MemoryStream();
                                      stream.Flush();
                                  }
                              }
                              """;

        const string fixedSource = """
                                   using System.IO;

                                   class C
                                   {
                                       void M()
                                       {
                                           using (var stream = new MemoryStream())
                                           {
                                               stream.Flush();
                                           }
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies the specification example: the fix keeps <c>await</c> and every declarator, and writes an empty block
    /// when no statement follows the declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7004-004")]
    public async Task FixKeepsAwaitAndEveryDeclaratorAndWritesAnEmptyBlock()
    {
        const string source = """
                              using System;
                              using System.IO;

                              class C
                              {
                                  async System.Threading.Tasks.Task M(IAsyncDisposable resource)
                                  {
                                      await {|RH7004:using|} var handle = resource;
                                  }

                                  void N()
                                  {
                                      {|RH7004:using|} MemoryStream first = new MemoryStream(), second = new MemoryStream();
                                  }
                              }
                              """;

        const string fixedSource = """
                                   using System;
                                   using System.IO;

                                   class C
                                   {
                                       async System.Threading.Tasks.Task M(IAsyncDisposable resource)
                                       {
                                           await using (var handle = resource)
                                           {
                                           }
                                       }

                                       void N()
                                       {
                                           using (MemoryStream first = new MemoryStream(), second = new MemoryStream())
                                           {
                                           }
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies the specification example: a using declaration among top-level statements is reported, and no code
    /// fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7004-005")]
    public async Task TopLevelUsingDeclarationIsReportedWithoutFix()
    {
        const string source = """
                              using System.IO;

                              {|RH7004:using|} var stream = new MemoryStream();
                              stream.Flush();
                              """;

        await VerifyNoCodeFixIsOffered(source, test => test.TestState.OutputKind = OutputKind.ConsoleApplication);
    }

    /// <summary>
    /// Verifies the specification example: a local function two statements after the using declaration withholds the
    /// fix, and the diagnostic is still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7004-006")]
    public async Task LaterLocalFunctionOfTheBlockWithholdsTheFix()
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  void M()
                                  {
                                      Helper();
                                      {|RH7004:using|} var stream = new MemoryStream();
                                      stream.Flush();

                                      void Helper()
                                      {
                                      }
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a local function directly after the using declaration withholds the fix, and the diagnostic is
    /// still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7004-006")]
    public async Task LocalFunctionDirectlyAfterTheDeclarationWithholdsTheFix()
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  void M()
                                  {
                                      Helper();
                                      {|RH7004:using|} var stream = new MemoryStream();

                                      void Helper()
                                      {
                                      }
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a label after the using declaration withholds the fix, and the diagnostic is still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7004-006")]
    public async Task LaterLabelOfTheBlockWithholdsTheFix()
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  void M()
                                  {
                                      {|RH7004:using|} var stream = new MemoryStream();
                                      stream.Flush();

                                  Done:
                                      return;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies the specification example: a local function nested in a following statement moves with it into the
    /// new block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7004-004")]
    [Requirement("RH7004-006")]
    public async Task LocalFunctionNestedInAFollowingStatementDoesNotWithholdTheFix()
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  void M()
                                  {
                                      {|RH7004:using|} var stream = new MemoryStream();
                                      if (stream.CanRead)
                                      {
                                          void Helper()
                                          {
                                          }
                                      }
                                  }
                              }
                              """;

        const string fixedSource = """
                                   using System.IO;

                                   class C
                                   {
                                       void M()
                                       {
                                           using (var stream = new MemoryStream())
                                           {
                                               if (stream.CanRead)
                                               {
                                                   void Helper()
                                                   {
                                                   }
                                               }
                                           }
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying that the fix moves every following statement of the block into the new block, in their order
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7004-004")]
    public async Task FixMovesEveryFollowingStatementInTheirOrder()
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  void M()
                                  {
                                      {|RH7004:using|} var stream = new MemoryStream();
                                      stream.Flush();
                                      stream.Close();
                                  }
                              }
                              """;

        const string fixedSource = """
                                   using System.IO;

                                   class C
                                   {
                                       void M()
                                       {
                                           using (var stream = new MemoryStream())
                                           {
                                               stream.Flush();
                                               stream.Close();
                                           }
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
                                using System.IO;

                                internal class RH7004
                                {
                                    public void Execute()
                                    {
                                        {|#0:using|} var first = new MemoryStream();
                                        {|#1:using|} var second = new MemoryStream();
                                        _ = first;
                                        _ = second;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.IO;

                                 internal class RH7004
                                 {
                                     public void Execute()
                                     {
                                         using (var first = new MemoryStream())
                                         {
                                             using (var second = new MemoryStream())
                                             {
                                                 _ = first;
                                                 _ = second;
                                             }
                                         }
                                     }
                                 }
                                 """;

        // The first using declaration's fix wraps every following statement -- including the second using
        // declaration itself -- into a new block, so its replacement span is a superset of the second
        // declaration's own fix span. The batch fixer discards the second fix as conflicting in the first pass
        // and needs another pass, once the first wrap has moved it into its new block, to convert it as well
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH7004UsingDeclarationsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH7004MessageFormat, 2),
                                  Configure: config => config.NumberOfFixAllIterations = 2);
    }

    #endregion // BatchCodeFixTestsBase
}