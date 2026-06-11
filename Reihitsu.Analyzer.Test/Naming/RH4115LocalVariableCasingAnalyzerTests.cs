using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Naming;
using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4115LocalVariableCasingAnalyzer"/> and <see cref="RH4115LocalVariableCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH4115LocalVariableCasingAnalyzerTests : AnalyzerTestsBase<RH4115LocalVariableCasingAnalyzer, RH4115LocalVariableCasingCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported for local variables that are not camelCase and that references are renamed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForLocalVariableAndReferenceAreFixed()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class DataLoader
                                    {
                                        public int Load()
                                        {
                                            int {|#0:ResultCount|} = 42;

                                            return ResultCount;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class DataLoader
                                     {
                                         public int Load()
                                         {
                                             int resultCount = 42;

                                             return resultCount;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4115LocalVariableCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4115MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for camelCase local variables
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForCamelCaseLocalVariable()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class DataLoader
                                    {
                                        public int Load()
                                        {
                                            int resultCount = 42;

                                            return resultCount;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies multiple local variables in a single declaration can produce multiple diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForMultipleLocalVariables()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class DataLoader
                                    {
                                        public void Load()
                                        {
                                            int {|#0:ResultCount|} = 42, {|#1:RetryCount|} = 2;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH4115LocalVariableCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4115MessageFormat, 2));
    }

    /// <summary>
    /// Verifies no code fix is offered for a letterless local variable whose conversion cannot produce a valid identifier
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NoCodeFixForLetterlessLocalVariable()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class DataLoader
                                    {
                                        public void Load()
                                        {
                                            int _ = 42;
                                        }
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testCode,
                                                   RH4115LocalVariableCasingAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<VariableDeclaratorSyntax>()
                                                               .Single(declarator => declarator.Identifier.ValueText == "_")
                                                               .Identifier
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    #endregion // Tests
}