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
    /// Verifies the code fix moves multiple following statements into the using statement block, and inserts no blank
    /// line in front of the using statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-006")]
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
    /// Verifies that without <c>DEBUG</c> defined, a conditional pair that lies wholly in the statements the fix would move
    /// into the block holds disabled text, which withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7004-007")]
    public async Task NoCodeFixForABalancedConditionalPairOfTheFollowingStatementsWithTheSymbolUndefined()
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

        await VerifyNoCodeFixIsOffered(source);
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
    /// Verifying the specification example: a comment in the leading trivia of the declaration's semicolon withholds the
    /// fix, because it would end up in front of the new closing parenthesis, on lines the fix would have to lay out
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7004-007")]
    public async Task NoFixIsOfferedForACommentInFrontOfTheSemicolon()
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

        await VerifyNoCodeFixIsOffered(source);
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

    /// <summary>
    /// Verifies that a <c>#pragma warning disable</c> that names a warning by its number pairs with a
    /// <c>#pragma warning restore</c> that names it with its <c>CS</c> prefix, so no code fix is offered when the new
    /// block would split them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task NoCodeFixWhenTheNewBlockWouldSplitAPragmaPairThatNamesTheWarningOnceByNumberAndOnceWithItsPrefix()
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  void M()
                                  {
                                      {|RH7004:using|} var s = new MemoryStream();
                              #pragma warning disable 219
                                      s.Flush();
                              #pragma warning restore CS0219
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a <c>#pragma warning restore</c> without a warning ends a <c>#pragma warning disable</c> of one warning,
    /// so no code fix is offered when the new block would split them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task NoCodeFixWhenTheNewBlockWouldSplitADisableOfOneWarningFromARestoreOfEveryWarning()
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  void M()
                                  {
                                      {|RH7004:using|} var s = new MemoryStream();
                              #pragma warning disable CS0219
                                      s.Flush();
                              #pragma warning restore
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that two <c>#nullable</c> directives of the warnings setting pair, so no code fix is offered when the new
    /// block would split them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task NoCodeFixWhenTheNewBlockWouldSplitANullablePairOfTheWarningsSetting()
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  void M()
                                  {
                                      {|RH7004:using|} var s = new MemoryStream();
                              #nullable disable warnings
                                      s.Flush();
                              #nullable restore warnings
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a <c>#nullable</c> directive of the warnings setting ends the warnings setting of a <c>#nullable</c>
    /// directive without setting, so no code fix is offered when the new block would split them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task NoCodeFixWhenTheNewBlockWouldSplitANullableDirectiveOfBothSettingsFromOneOfTheWarningsSetting()
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
                              #nullable restore warnings
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that <c>#nullable</c> directives of different settings do not pair: the directive of the annotations
    /// setting moves into the block, the one of the warnings setting stays behind it, and the fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7004-004")]
    [Requirement("SHARED-FIX-003")]
    public async Task FixIsOfferedForNullableDirectivesOfDifferentSettingsOnBothSidesOfTheNewBlock()
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  void M()
                                  {
                                      {|RH7004:using|} var s = new MemoryStream();
                              #nullable disable annotations
                                      s.Flush();
                              #nullable enable warnings
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
                                   #nullable disable annotations
                                               s.Flush();
                                           }
                                   #nullable enable warnings
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying the reported example: the moved statements keep their spacing, the unbraced embedded statement, and their
    /// line breaks, and no blank line is inserted between them
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-007")]
    public async Task MovedStatementsKeepTheirLayout(string endOfLine)
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  int M()
                                  {
                                      {|RH7004:using|} var s = new MemoryStream();
                                      s.Flush( );
                                      if (true) s.Flush();
                                      var n = 1;
                                      return n;
                                  }
                              }
                              """;

        const string fixedSource = """
                                   using System.IO;

                                   class C
                                   {
                                       int M()
                                       {
                                           using (var s = new MemoryStream())
                                           {
                                               s.Flush( );
                                               if (true) s.Flush();
                                               var n = 1;
                                               return n;
                                           }
                                       }
                                   }
                                   """;

        await VerifyCodeFixWithEndOfLine(source, fixedSource, endOfLine);
    }

    /// <summary>
    /// Verifying the reported example: no blank line is inserted in front of the new using statement, behind the statement
    /// in front of it, nor between the moved statements
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-006")]
    public async Task NoBlankLineIsInsertedInFrontOfTheNewUsingStatement(string endOfLine)
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  int M()
                                  {
                                      var x = 0;
                                      {|RH7004:using|} var s = new MemoryStream();
                                      x++;
                                      return x;
                                  }
                              }
                              """;

        const string fixedSource = """
                                   using System.IO;

                                   class C
                                   {
                                       int M()
                                       {
                                           var x = 0;
                                           using (var s = new MemoryStream())
                                           {
                                               x++;
                                               return x;
                                           }
                                       }
                                   }
                                   """;

        await VerifyCodeFixWithEndOfLine(source, fixedSource, endOfLine);
    }

    /// <summary>
    /// Verifying the reported example: no blank line is inserted in front of or behind an <c>#endregion</c> that stands in
    /// front of the using declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-006")]
    public async Task NoBlankLineIsInsertedAroundAnEndRegionInFrontOfTheUsingDeclaration()
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  void M()
                                  {
                                      #region Setup
                                      var n = 1;
                                      #endregion
                                      {|RH7004:using|} var s = new MemoryStream();
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
                                           #region Setup
                                           var n = 1;
                                           #endregion
                                           using (var s = new MemoryStream())
                                           {
                                               s.Flush();
                                           }
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying the specification example: every line of the moved statements moves four columns to the right, the
    /// continuation line included, although formatting the result would align it under the first operand
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-007")]
    public async Task ContinuationLineOfAMovedStatementMovesOneLevelWithIt(string endOfLine)
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  int M(int a, int b)
                                  {
                                      {|RH7004:using|} var s = new MemoryStream();
                                      var total = a
                                          + b;
                                      return total;
                                  }
                              }
                              """;

        const string fixedSource = """
                                   using System.IO;

                                   class C
                                   {
                                       int M(int a, int b)
                                       {
                                           using (var s = new MemoryStream())
                                           {
                                               var total = a
                                                   + b;
                                               return total;
                                           }
                                       }
                                   }
                                   """;

        await VerifyCodeFixWithEndOfLine(source, fixedSource, endOfLine);
    }

    /// <summary>
    /// Verifying that a continuation line aligned to an argument on the line above moves four columns with it and stays
    /// aligned
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-007")]
    public async Task AlignedContinuationLineOfAMovedStatementStaysAligned()
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  void M(int a, int b)
                                  {
                                      {|RH7004:using|} var s = new MemoryStream();
                                      M(a,
                                        b);
                                  }
                              }
                              """;

        const string fixedSource = """
                                   using System.IO;

                                   class C
                                   {
                                       void M(int a, int b)
                                       {
                                           using (var s = new MemoryStream())
                                           {
                                               M(a,
                                                 b);
                                           }
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying the specification example: among the moved statements, the <c>#pragma</c> lines and a comment in column 0
    /// keep their column, and the <c>#region</c> and <c>#endregion</c> lines move with the statements
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-007")]
    public async Task ColumnZeroCommentAndDirectiveLinesAmongTheMovedStatements(string endOfLine)
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  void M()
                                  {
                                      {|RH7004:using|} var s = new MemoryStream();
                              #pragma warning disable CS0219
                                      var unused = 1;
                              #pragma warning restore CS0219
                                      #region Work
                                      s.Flush();
                                      #endregion
                              //        s.Close();
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
                                           using (var s = new MemoryStream())
                                           {
                                   #pragma warning disable CS0219
                                               var unused = 1;
                                   #pragma warning restore CS0219
                                               #region Work
                                               s.Flush();
                                               #endregion
                                   //        s.Close();
                                               s.Flush();
                                           }
                                       }
                                   }
                                   """;

        await VerifyCodeFixWithEndOfLine(source, fixedSource, endOfLine);
    }

    /// <summary>
    /// Verifying the specification example: a raw string literal over several lines among the moved statements withholds
    /// the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7004-007")]
    public async Task NoCodeFixForARawStringOverSeveralLinesAmongTheMovedStatements()
    {
        const string source = """"
                              using System.IO;

                              class C
                              {
                                  string M()
                                  {
                                      {|RH7004:using|} var s = new MemoryStream();
                                      var t = """
                                          a
                                          """;
                                      return t;
                                  }
                              }
                              """";

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying the specification example: a verbatim string over two lines among the moved statements withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7004-007")]
    public async Task NoCodeFixForAVerbatimStringOverTwoLinesAmongTheMovedStatements()
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  string M()
                                  {
                                      {|RH7004:using|} var s = new MemoryStream();
                                      var t = @"a
                                b";
                                      return t;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that an interpolated verbatim string over two lines among the moved statements withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7004-007")]
    public async Task NoCodeFixForAnInterpolatedVerbatimStringOverTwoLinesAmongTheMovedStatements()
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  string M(int a)
                                  {
                                      {|RH7004:using|} var s = new MemoryStream();
                                      var t = $@"{a}
                                b";
                                      return t;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying the specification example: a comment over two lines among the moved statements withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7004-007")]
    public async Task NoCodeFixForACommentOverTwoLinesAmongTheMovedStatements()
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  void M()
                                  {
                                      {|RH7004:using|} var s = new MemoryStream();
                                      /* one
                                         two */
                                      s.Flush();
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that a comment over two lines inside a moved statement withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7004-007")]
    public async Task NoCodeFixForACommentOverTwoLinesInsideAMovedStatement()
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  void M()
                                  {
                                      {|RH7004:using|} var s = new MemoryStream();
                                      s.Write(new byte[0], 0, /* one
                                         two */ 0);
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying the specification example: disabled text among the moved statements withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7004-007")]
    public async Task NoCodeFixForDisabledTextAmongTheMovedStatements()
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  void M()
                                  {
                                      {|RH7004:using|} var s = new MemoryStream();
                              #if NEVER
                                      s.Close();
                              #endif
                                      s.Flush();
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying the specification example: a statement that starts on the line of the using declaration withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7004-007")]
    public async Task NoCodeFixForAStatementOnTheLineOfTheUsingDeclaration()
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  void M()
                                  {
                                      {|RH7004:using|} var s = new MemoryStream(); s.Flush();
                                      s.Flush();
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that a directive in the leading trivia of the declaration's semicolon withholds the fix, as a comment there
    /// does
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7004-007")]
    public async Task NoFixIsOfferedForADirectiveInFrontOfTheSemicolon()
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  void M()
                                  {
                                      {|RH7004:using|} var s = new MemoryStream()
                              #pragma warning disable CS0219
                                          ;
                                      s.Flush();
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying the specification example: another statement in front of the using declaration on its line withholds
    /// the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7004-007")]
    public async Task NoCodeFixForCodeInFrontOfTheUsingDeclarationOnItsLine()
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  void M()
                                  {
                                      int a = 0; {|RH7004:using|} var s = new MemoryStream();
                                      s.Flush();
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying that the opening brace of the block in front of the using declaration on its line withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7004-007")]
    public async Task NoCodeFixForTheOpeningBraceOfTheBlockInFrontOfTheUsingDeclarationOnItsLine()
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  void M()
                                  { {|RH7004:using|} var s = new MemoryStream();
                                      s.Flush();
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying the specification example: the last moved statement ending on the line of the block's closing brace
    /// withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7004-007")]
    public async Task NoCodeFixWhenTheLastMovedStatementEndsOnTheLineOfTheClosingBrace()
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  void M()
                                  {
                                      {|RH7004:using|} var s = new MemoryStream();
                                      s.Flush(); }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifying the specification example: a line that holds code moves one indentation level with the other moved
    /// statements, also where a comment in front of the code starts in column 0, because it is no comment line
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-007")]
    public async Task CodeLineWithAColumnZeroCommentInFrontOfItMovesWithTheStatements(string endOfLine)
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  void M()
                                  {
                                      {|RH7004:using|} var s = new MemoryStream();
                              /* x */ s.Flush();
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
                                           using (var s = new MemoryStream())
                                           {
                                       /* x */ s.Flush();
                                               s.Flush();
                                           }
                                       }
                                   }
                                   """;

        await VerifyCodeFixWithEndOfLine(source, fixedSource, endOfLine);
    }

    /// <summary>
    /// Verifying that a line that holds nothing but comments, the first of them a delimited comment in column 0, is a
    /// comment line and keeps column 0 among the moved statements
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-007")]
    public async Task LineOfOnlyCommentsStartingWithADelimitedCommentInColumnZeroKeepsItsColumn(string endOfLine)
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  void M()
                                  {
                                      {|RH7004:using|} var s = new MemoryStream();
                              /* x */ // s.Close();
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
                                           using (var s = new MemoryStream())
                                           {
                                   /* x */ // s.Close();
                                               s.Flush();
                                           }
                                       }
                                   }
                                   """;

        await VerifyCodeFixWithEndOfLine(source, fixedSource, endOfLine);
    }

    /// <summary>
    /// Verifying the specification example: a string literal and a comment on one line among the moved statements keep the
    /// fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("RH7004-007")]
    public async Task StringLiteralAndCommentOnOneLineKeepTheFix(string endOfLine)
    {
        const string source = """
                              using System.IO;

                              class C
                              {
                                  string M()
                                  {
                                      {|RH7004:using|} var s = new MemoryStream();
                                      var t = "a"; /* b */
                                      return t;
                                  }
                              }
                              """;

        const string fixedSource = """
                                   using System.IO;

                                   class C
                                   {
                                       string M()
                                       {
                                           using (var s = new MemoryStream())
                                           {
                                               var t = "a"; /* b */
                                               return t;
                                           }
                                       }
                                   }
                                   """;

        await VerifyCodeFixWithEndOfLine(source, fixedSource, endOfLine);
    }

    /// <summary>
    /// Verifying that a raw string literal on one line among the moved statements keeps the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7004-007")]
    public async Task RawStringOnOneLineKeepsTheFix()
    {
        const string source = """"
                              using System.IO;

                              class C
                              {
                                  string M()
                                  {
                                      {|RH7004:using|} var s = new MemoryStream();
                                      var t = """a""";
                                      return t;
                                  }
                              }
                              """";

        const string fixedSource = """"
                                   using System.IO;

                                   class C
                                   {
                                       string M()
                                       {
                                           using (var s = new MemoryStream())
                                           {
                                               var t = """a""";
                                               return t;
                                           }
                                       }
                                   }
                                   """";

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifying that a blank line that holds whitespace among the moved statements stays a blank line and is written
    /// empty
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("SHARED-FIX-007")]
    public async Task WhitespaceOnlyLineAmongTheMovedStatementsIsWrittenEmpty(string endOfLine)
    {
        var source = ExpandWhitespaceMarkers("""
                                             using System.IO;

                                             class C
                                             {
                                                 void M()
                                                 {
                                                     {|RH7004:using|} var s = new MemoryStream();
                                                     s.Flush();
                                             ········
                                                     s.Close();
                                                 }
                                             }
                                             """);

        const string fixedSource = """
                                   using System.IO;

                                   class C
                                   {
                                       void M()
                                       {
                                           using (var s = new MemoryStream())
                                           {
                                               s.Flush();

                                               s.Close();
                                           }
                                       }
                                   }
                                   """;

        await VerifyCodeFixWithEndOfLine(source, fixedSource, endOfLine);
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