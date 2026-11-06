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

    /// <summary>
    /// Verifies that the statements the fix wraps into the new block are only indented one level deeper: every line of a
    /// multi-line statement follows the new nesting, and the blank line between the moved statements stays
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-007")]
    public async Task FixOnlyReindentsTheStatementsItWrapsIntoTheNewBlock()
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
                                          stream.Flush();
                                      }

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
                                               if (stream.CanRead)
                                               {
                                                   stream.Flush();
                                               }

                                               stream.Flush();
                                           }
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies the specification example: a comment behind the using declaration at the end of its line stays behind the
    /// header of the <c>using</c> statement, on the header's line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7004-004")]
    public async Task FixKeepsACommentBehindTheDeclarationOnTheHeaderLine()
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  void M()
                                  {
                                      {|RH7004:using|} var s = new MemoryStream(); // resource
                                      s.Flush();
                                  }
                              }
                              """;

        const string fixedSource = """
                                   using System.IO;

                                   class C
                                   {
                                       void M()
                                       {
                                           using (var s = new MemoryStream()) // resource
                                           {
                                               s.Flush();
                                           }
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies that a comment behind the using declaration stays on the header's line under LF and under CRLF line
    /// endings, with the document's line ending kept
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7004-004")]
    public async Task FixKeepsACommentBehindTheDeclarationOnTheHeaderLineUnderBothLineEndings()
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  void M()
                                  {
                                      using var s = new MemoryStream(); // resource
                                      s.Flush();
                                  }
                              }
                              """;

        const string fixedSource = """
                                   using System.IO;

                                   class C
                                   {
                                       void M()
                                       {
                                           using (var s = new MemoryStream()) // resource
                                           {
                                               s.Flush();
                                           }
                                       }
                                   }
                                   """;

        Assert.AreEqual(fixedSource, await ApplyCodeFixAsync(source), "Unexpected fix result under LF line endings.");
        Assert.AreEqual(NormalizeToCarriageReturnLineFeed(fixedSource), await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(source)), "Unexpected fix result under CRLF line endings.");
    }

    /// <summary>
    /// Verifies the specification example with <c>DEBUG</c> defined: the block of the new <c>using</c> statement would end
    /// between <c>#if DEBUG</c> and <c>#endif</c>, so no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task NoCodeFixWhenTheNewBlockWouldEndInsideAConditionalPairWithTheSymbolDefined()
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  void M()
                                  {
                                      {|RH7004:using|} var s = new MemoryStream();
                              #if DEBUG
                                      s.Flush();
                              #endif
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source, DocumentationMode.Diagnose, "DEBUG");
    }

    /// <summary>
    /// Verifies the specification example without <c>DEBUG</c> defined: the block of the new <c>using</c> statement would
    /// end in front of <c>#if DEBUG</c>, so a build with <c>DEBUG</c> would call the method outside the scope of the
    /// resource, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task NoCodeFixWhenTheNewBlockWouldEndInFrontOfAConditionalPairWithTheSymbolUndefined()
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  void M()
                                  {
                                      {|RH7004:using|} var s = new MemoryStream();
                              #if DEBUG
                                      s.Flush();
                              #endif
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies the specification example with <c>DEBUG</c> defined: a conditional pair that lies wholly in the statements
    /// the fix moves into the block moves with them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task FixMovesABalancedConditionalPairOfTheFollowingStatementsIntoTheBlockWithTheSymbolDefined()
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  void M()
                                  {
                                      {|RH7004:using|} var s = new MemoryStream();
                              #if DEBUG
                                      s.Flush();
                              #endif
                                      s.Close();
                                  }
                              }
                              """;

        const string fixedSource = """
                                   using System.IO;

                                   class C
                                   {
                                       void M()
                                       {
                                           using (var s = new MemoryStream())
                                           {
                                   #if DEBUG
                                               s.Flush();
                                   #endif
                                               s.Close();
                                           }
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource, test => test.SolutionTransforms.Add(DefineDebug));
    }

    /// <summary>
    /// Verifies the specification example without <c>DEBUG</c> defined: a conditional pair that lies wholly in the
    /// statements the fix moves into the block moves with them, and its disabled text keeps its text
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task FixMovesABalancedConditionalPairOfTheFollowingStatementsIntoTheBlockWithTheSymbolUndefined()
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  void M()
                                  {
                                      {|RH7004:using|} var s = new MemoryStream();
                              #if DEBUG
                                      s.Flush();
                              #endif
                                      s.Close();
                                  }
                              }
                              """;

        const string fixedSource = """
                                   using System.IO;

                                   class C
                                   {
                                       void M()
                                       {
                                           using (var s = new MemoryStream())
                                           {
                                   #if DEBUG
                                           s.Flush();
                                   #endif
                                               s.Close();
                                           }
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies the specification example: both braces of the new block lie between the two directives of a pragma pair,
    /// which keep governing the same code, so the fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task FixIsOfferedForAPragmaPairAroundTheDeclarationAndEveryFollowingStatement()
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  void M()
                                  {
                              #pragma warning disable CS0618
                                      {|RH7004:using|} var s = new MemoryStream();
                                      s.Flush();
                              #pragma warning restore CS0618
                                  }
                              }
                              """;

        const string fixedSource = """
                                   using System.IO;

                                   class C
                                   {
                                       void M()
                                       {
                                   #pragma warning disable CS0618
                                           using (var s = new MemoryStream())
                                           {
                                               s.Flush();
                                           }
                                   #pragma warning restore CS0618
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies that no code fix is offered when the new block would take the opening directive of a pragma pair in
    /// front of the following statement into the block and leave its partner behind the block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task NoCodeFixWhenTheNewBlockWouldSplitAPragmaPair()
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  void M()
                                  {
                                      {|RH7004:using|} var s = new MemoryStream();
                              #pragma warning disable CS0618
                                      s.Flush();
                              #pragma warning restore CS0618
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that no code fix is offered when the new block would take the opening directive of a nullable pair in
    /// front of the following statement into the block and leave its partner behind the block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task NoCodeFixWhenTheNewBlockWouldSplitANullablePair()
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  void M()
                                  {
                                      {|RH7004:using|} var s = new MemoryStream();
                              #nullable disable
                                      s.Flush();
                              #nullable restore
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that no code fix is offered when the new block would take the opening directive of a region pair in
    /// front of the following statement into the block and leave its partner behind the block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task NoCodeFixWhenTheNewBlockWouldSplitARegionPair()
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  void M()
                                  {
                                      {|RH7004:using|} var s = new MemoryStream();
                                      #region Work
                                      s.Flush();
                                      #endregion
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a comment behind the using keyword stays behind it, in the header the declaration is rewritten
    /// into
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task FixKeepsACommentBehindTheUsingKeyword()
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  void M()
                                  {
                                      {|RH7004:using|} /* c */ var s = new MemoryStream();
                                      s.Flush();
                                  }
                              }
                              """;

        const string fixedSource = """
                                   using System.IO;

                                   class C
                                   {
                                       void M()
                                       {
                                           using /* c */ (var s = new MemoryStream())
                                           {
                                               s.Flush();
                                           }
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies that a comment on the line in front of the semicolon stays on the line in front of the closing parenthesis
    /// the semicolon is rewritten into, laid out as the formatter lays it out
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task FixKeepsACommentInFrontOfTheSemicolon()
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  void M()
                                  {
                                      {|RH7004:using|} var s = new MemoryStream()
                                          // c
                                          ;
                                      s.Flush();
                                  }
                              }
                              """;

        const string fixedSource = """
                                   using System.IO;

                                   class C
                                   {
                                       void M()
                                       {
                                           using (var s = new MemoryStream()

                                           // c
                                           )
                                           {
                                               s.Flush();
                                           }
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies that a comment behind the using declaration that does not end its line stays behind the new block, in
    /// front of the code that follows it on its line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7004-004")]
    public async Task FixKeepsACommentThatDoesNotEndItsLineBehindTheNewBlock()
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  void M()
                                  {
                                      {|RH7004:using|} var s = new MemoryStream(); /* c */ }
                              }
                              """;

        const string fixedSource = """
                                   using System.IO;

                                   class C
                                   {
                                       void M()
                                       {
                                           using (var s = new MemoryStream())
                                           {
                                           } /* c */ }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies that a comment behind the using declaration that does not end its line stays behind the new block under LF
    /// and under CRLF line endings, with the document's line ending kept
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7004-004")]
    public async Task FixKeepsACommentThatDoesNotEndItsLineBehindTheNewBlockUnderBothLineEndings()
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  void M()
                                  {
                                      using var s = new MemoryStream(); /* c */ }
                              }
                              """;

        const string fixedSource = """
                                   using System.IO;

                                   class C
                                   {
                                       void M()
                                       {
                                           using (var s = new MemoryStream())
                                           {
                                           } /* c */ }
                                   }
                                   """;

        Assert.AreEqual(fixedSource, await ApplyCodeFixAsync(source), "Unexpected fix result under LF line endings.");
        Assert.AreEqual(NormalizeToCarriageReturnLineFeed(fixedSource), await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(source)), "Unexpected fix result under CRLF line endings.");
    }

    /// <summary>
    /// Verifies that the fix is offered when the declaration and every following statement lie inside a conditional
    /// pair whose <c>#endif</c> stands in front of the enclosing closing brace: both braces of the new block lie inside
    /// the pair
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7004-004")]
    [Requirement("SHARED-FIX-003")]
    public async Task FixIsOfferedInsideAConditionalPairThatEndsInFrontOfTheClosingBrace()
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  void M()
                                  {
                              #if DEBUG
                                      {|RH7004:using|} var s = new MemoryStream();
                                      s.Flush();
                              #endif
                                  }
                              }
                              """;

        const string fixedSource = """
                                   using System.IO;

                                   class C
                                   {
                                       void M()
                                       {
                                   #if DEBUG
                                           using (var s = new MemoryStream())
                                           {
                                               s.Flush();
                                           }
                                   #endif
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource, test => test.SolutionTransforms.Add(DefineDebug));
    }

    /// <summary>
    /// Verifies that the fix is offered when the declaration and every following statement lie in the active branch of a
    /// conditional pair whose <c>#else</c> branch and <c>#endif</c> stand in front of the enclosing closing brace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7004-004")]
    [Requirement("SHARED-FIX-003")]
    public async Task FixIsOfferedInsideTheActiveBranchOfAConditionalPairWhoseElseBranchEndsInFrontOfTheClosingBrace()
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  void M()
                                  {
                              #if DEBUG
                                      {|RH7004:using|} var s = new MemoryStream();
                                      s.Flush();
                              #else
                                      System.Console.WriteLine();
                              #endif
                                  }
                              }
                              """;

        const string fixedSource = """
                                   using System.IO;

                                   class C
                                   {
                                       void M()
                                       {
                                   #if DEBUG
                                           using (var s = new MemoryStream())
                                           {
                                               s.Flush();
                                           }
                                   #else
                                           System.Console.WriteLine();
                                   #endif
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource, test => test.SolutionTransforms.Add(DefineDebug));
    }

    /// <summary>
    /// Verifies that a <c>#pragma warning disable</c> among the following statements that no directive restores moves into
    /// the block with them: it has no partner the new block could split
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7004-004")]
    [Requirement("SHARED-FIX-003")]
    public async Task FixIsOfferedForAPragmaWarningDirectiveWithoutRestoreAmongTheFollowingStatements()
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  void M()
                                  {
                                      {|RH7004:using|} var s = new MemoryStream();
                                      s.Flush();
                              #pragma warning disable CS0618
                                      s.Close();
                                  }
                              }
                              """;

        const string fixedSource = """
                                   using System.IO;

                                   class C
                                   {
                                       void M()
                                       {
                                           using (var s = new MemoryStream())
                                           {
                                               s.Flush();
                                   #pragma warning disable CS0618
                                               s.Close();
                                           }
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies that a <c>#pragma warning disable</c> without restore among the following statements does not withhold
    /// the fix under LF and under CRLF line endings, with the document's line ending kept
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7004-004")]
    [Requirement("SHARED-FIX-003")]
    public async Task FixIsOfferedForAPragmaWarningDirectiveWithoutRestoreAmongTheFollowingStatementsUnderBothLineEndings()
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  void M()
                                  {
                                      using var s = new MemoryStream();
                                      s.Flush();
                              #pragma warning disable CS0618
                                      s.Close();
                                  }
                              }
                              """;

        const string fixedSource = """
                                   using System.IO;

                                   class C
                                   {
                                       void M()
                                       {
                                           using (var s = new MemoryStream())
                                           {
                                               s.Flush();
                                   #pragma warning disable CS0618
                                               s.Close();
                                           }
                                       }
                                   }
                                   """;

        Assert.AreEqual(fixedSource, await ApplyCodeFixAsync(source), "Unexpected fix result under LF line endings.");
        Assert.AreEqual(NormalizeToCarriageReturnLineFeed(fixedSource), await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(source)), "Unexpected fix result under CRLF line endings.");
    }

    /// <summary>
    /// Verifies that a <c>#nullable</c> directive among the following statements that no later <c>#nullable</c> directive
    /// ends moves into the block with them: it has no partner the new block could split
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7004-004")]
    [Requirement("SHARED-FIX-003")]
    public async Task FixIsOfferedForANullableDirectiveWithoutPartnerAmongTheFollowingStatements()
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  void M()
                                  {
                                      {|RH7004:using|} var s = new MemoryStream();
                                      s.Flush();
                              #nullable enable
                                      s.Close();
                                  }
                              }
                              """;

        const string fixedSource = """
                                   using System.IO;

                                   class C
                                   {
                                       void M()
                                       {
                                           using (var s = new MemoryStream())
                                           {
                                               s.Flush();
                                   #nullable enable
                                               s.Close();
                                           }
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies that a <c>#pragma warning disable</c> of one warning in front of the type does not pair with a balanced
    /// pair of another warning among the following statements: neither pair is split, so the fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7004-004")]
    [Requirement("SHARED-FIX-003")]
    public async Task FixIsOfferedForAPragmaPairOfTheFollowingStatementsBehindADisableOfAnotherWarning()
    {
        const string source = """
                              using System.IO;

                              #pragma warning disable CS0169
                              class C
                              {
                                  void M()
                                  {
                                      {|RH7004:using|} var s = new MemoryStream();
                                      s.Flush();
                              #pragma warning disable CS0618
                                      s.Close();
                              #pragma warning restore CS0618
                                      s.Dispose();
                                  }
                              }
                              """;

        const string fixedSource = """
                                   using System.IO;

                                   #pragma warning disable CS0169
                                   class C
                                   {
                                       void M()
                                       {
                                           using (var s = new MemoryStream())
                                           {
                                               s.Flush();
                                   #pragma warning disable CS0618
                                               s.Close();
                                   #pragma warning restore CS0618
                                               s.Dispose();
                                           }
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
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

    #endregion // Methods

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