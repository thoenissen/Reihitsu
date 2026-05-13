using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0396UsingDeclarationsShouldNotBeUsedAnalyzer"/> and <see cref="RH0396UsingDeclarationsShouldNotBeUsedCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0396UsingDeclarationsShouldNotBeUsedAnalyzerTests : AnalyzerTestsBase<RH0396UsingDeclarationsShouldNotBeUsedAnalyzer, RH0396UsingDeclarationsShouldNotBeUsedCodeFixProvider>
{
    #region Members

    /// <summary>
    /// Verifies a diagnostic is reported when a using declaration is used
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticWhenUsingDeclarationIsUsed()
    {
        const string testCode = """
                                using System.IO;

                                internal class RH0396
                                {
                                    public void Execute()
                                    {
                                        {|#0:using|} var stream = new MemoryStream();
                                        _ = stream;
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH0396UsingDeclarationsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH0396MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported when a using declaration uses an explicit type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticWhenUsingDeclarationIsUsedWithExplicitType()
    {
        const string testCode = """
                                using System.IO;

                                internal class RH0396
                                {
                                    public void Execute()
                                    {
                                        {|#0:using|} System.IO.MemoryStream stream = new MemoryStream();
                                        _ = stream;
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH0396UsingDeclarationsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH0396MessageFormat));
    }

    /// <summary>
    /// Verifies diagnostics are reported for multiple using declarations
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticWhenMultipleUsingDeclarationsArePresent()
    {
        const string testCode = """
                                using System.IO;

                                internal class RH0396
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

        await Verify(testCode, Diagnostics(RH0396UsingDeclarationsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH0396MessageFormat, 2));
    }

    /// <summary>
    /// Verifies no diagnostic is reported when a using statement block is used
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticWhenUsingStatementBlockIsUsed()
    {
        const string testCode = """
                                using System.IO;

                                internal class RH0396
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
    /// Verifies no diagnostic is reported when a using directive is used
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticWhenUsingDirectiveIsUsed()
    {
        const string testCode = """
                                using System;

                                internal class RH0396
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
    public async Task VerifyCodeFixTransformsUsingDeclarationToUsingStatementBlock()
    {
        const string testCode = """
                                using System.IO;

                                internal class RH0396
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

                                 internal class RH0396
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

        await Verify(testCode, fixedCode, Diagnostics(RH0396UsingDeclarationsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH0396MessageFormat));
    }

    /// <summary>
    /// Verifies the code fix creates an empty using statement block when no statements follow
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixTransformsUsingDeclarationWithNoFollowingStatements()
    {
        const string testCode = """
                                using System.IO;

                                internal class RH0396
                                {
                                    public void Execute()
                                    {
                                        {|#0:using|} var stream = new MemoryStream();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.IO;

                                 internal class RH0396
                                 {
                                     public void Execute()
                                     {
                                         using (var stream = new MemoryStream())
                                         {
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0396UsingDeclarationsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH0396MessageFormat));
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

                                internal class RH0396
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

                                 internal class RH0396
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

        await Verify(testCode, fixedCode, Diagnostics(RH0396UsingDeclarationsShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH0396MessageFormat));
    }

    #endregion // Members
}