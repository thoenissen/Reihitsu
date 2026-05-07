using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH0222TupleElementCasingAnalyzer"/> and <see cref="RH0222TupleElementCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0222TupleElementCasingAnalyzerTests : AnalyzerTestsBase<RH0222TupleElementCasingAnalyzer, RH0222TupleElementCasingCodeFixProvider>
{
    #region Members

    /// <summary>
    /// Verifies diagnostics are reported for tuple type elements that are not PascalCase
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForTupleTypeElementWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class DataLoader
                                    {
                                        public (int {|#0:firstValue|}, int SecondValue) Load()
                                        {
                                            return default;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH0222TupleElementCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0222MessageFormat));
    }

    /// <summary>
    /// Verifies no code fix is offered for tuple type elements when usage sites would become stale
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NoCodeFixForTupleTypeElementWithMultipleUsageSites()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class DataLoader
                                    {
                                        public (int firstValue, int SecondValue) Load()
                                        {
                                            return (1, 2);
                                        }

                                        public int Sum()
                                        {
                                            var values = Load();

                                            return values.firstValue + Load().firstValue;
                                        }
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testCode,
                                                   RH0222TupleElementCasingAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<TupleElementSyntax>()
                                                               .Single(element => element.Identifier.ValueText == "firstValue")
                                                               .Identifier
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies multiple tuple type elements can produce multiple diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForMultipleTupleTypeElements()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class DataLoader
                                    {
                                        public (int {|#0:firstValue|}, int {|#1:secondValue|}) Load()
                                        {
                                            return default;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH0222TupleElementCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0222MessageFormat, 2));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for PascalCase tuple type elements
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForPascalCaseTupleTypeElements()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class DataLoader
                                    {
                                        public (int FirstValue, int SecondValue) Load()
                                        {
                                            return default;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies unnamed tuple elements do not report diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForUnnamedTupleTypeElements()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class DataLoader
                                    {
                                        public (int, int) Load()
                                        {
                                            return default;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    #endregion // Members
}