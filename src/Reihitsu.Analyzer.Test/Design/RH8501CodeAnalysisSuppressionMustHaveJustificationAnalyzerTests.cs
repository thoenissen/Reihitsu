using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH8501CodeAnalysisSuppressionMustHaveJustificationAnalyzer"/>
/// </summary>
[TestClass]
public class RH8501CodeAnalysisSuppressionMustHaveJustificationAnalyzerTests : AnalyzerTestsBase<RH8501CodeAnalysisSuppressionMustHaveJustificationAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifying that missing or empty suppression justifications trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8501-001")]
    [Requirement("RH8501-002")]
    [Requirement("RH8501-003")]
    public async Task VerifyDiagnostics()
    {
        const string testData = """
                                using System.Diagnostics.CodeAnalysis;

                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                internal class Sample
                                {
                                    private const string Reason = "Used for testing";

                                    [{|#0:SuppressMessage|}("Microsoft.Performance", "CA1822")]
                                    internal void WithoutJustification()
                                    {
                                    }

                                    [{|#1:SuppressMessage|}("Microsoft.Performance", "CA1822", Justification = "")]
                                    internal void WithEmptyJustification()
                                    {
                                    }

                                    [SuppressMessage("Microsoft.Performance", "CA1822", Justification = Reason)]
                                    internal void WithJustification()
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH8501CodeAnalysisSuppressionMustHaveJustificationAnalyzer.DiagnosticId, AnalyzerResources.RH8501MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that a suppression without a justification is reported on its name however it is written and
    /// whatever it targets, while an attribute of the same name that binds to another type is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8501-001")]
    [Requirement("RH8501-003")]
    public async Task VerifySuppressionsWithoutJustificationAreReportedHoweverWritten()
    {
        const string testData = """
                                using System.Diagnostics.CodeAnalysis;
                                using Suppress = System.Diagnostics.CodeAnalysis.SuppressMessageAttribute;

                                [assembly: {|#0:SuppressMessage|}("Design", "CA1000", Scope = "member", Target = "~M:C.M")]

                                namespace Other
                                {
                                    public class SuppressMessageAttribute : System.Attribute
                                    {
                                        public SuppressMessageAttribute(string category, string checkId)
                                        {
                                        }
                                    }
                                }

                                class C
                                {
                                    [{|#1:SuppressMessage|}("Design", "CA1000")]
                                    void M()
                                    {
                                    }

                                    [{|#2:System.Diagnostics.CodeAnalysis.SuppressMessageAttribute|}("Design", "CA1000", MessageId = "Id")]
                                    void N()
                                    {
                                    }

                                    [{|#3:Suppress|}("Design", "CA1000")]
                                    void O()
                                    {
                                    }

                                    [Other.SuppressMessage("Design", "CA1000")]
                                    void P()
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH8501CodeAnalysisSuppressionMustHaveJustificationAnalyzer.DiagnosticId, AnalyzerResources.RH8501MessageFormat, 4));
    }

    /// <summary>
    /// Verifies that a suppression targeting the module without a justification is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8501-001")]
    [Requirement("RH8501-003")]
    public async Task VerifyModuleSuppressionWithoutJustificationIsReported()
    {
        const string testData = """
                                using System.Diagnostics.CodeAnalysis;

                                [module: {|#0:SuppressMessage|}("Design", "CA1000")]

                                class C
                                {
                                }
                                """;

        await Verify(testData, Diagnostics(RH8501CodeAnalysisSuppressionMustHaveJustificationAnalyzer.DiagnosticId, AnalyzerResources.RH8501MessageFormat));
    }

    /// <summary>
    /// Verifies that a justification that is empty, only white space, <see langword="null"/>, or an empty constant is
    /// reported, while a constant and a concatenation that yield text are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8501-002")]
    public async Task VerifyJustificationsWithoutTextAreReported()
    {
        const string testData = """
                                using System.Diagnostics.CodeAnalysis;

                                class C
                                {
                                    const string Reason = "Validated by the caller.";
                                    const string Empty = "";

                                    [{|#0:SuppressMessage|}("Design", "CA1000", Justification = "")]
                                    void M()
                                    {
                                    }

                                    [{|#1:SuppressMessage|}("Design", "CA1000", Justification = "   ")]
                                    void N()
                                    {
                                    }

                                    [{|#2:SuppressMessage|}("Design", "CA1000", Justification = null)]
                                    void O()
                                    {
                                    }

                                    [{|#3:SuppressMessage|}("Design", "CA1000", Justification = Empty)]
                                    void P()
                                    {
                                    }

                                    [SuppressMessage("Design", "CA1000", Justification = Reason)]
                                    void Q()
                                    {
                                    }

                                    [SuppressMessage("Design", "CA1000", Justification = "Validated " + "by the caller.")]
                                    void R()
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH8501CodeAnalysisSuppressionMustHaveJustificationAnalyzer.DiagnosticId, AnalyzerResources.RH8501MessageFormat, 4));
    }

    /// <summary>
    /// Verifies that a justification written with <see langword="nameof"/> is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8501-002")]
    public async Task VerifyNameofJustificationDoesNotProduceDiagnostics()
    {
        const string testData = """
                                using System.Diagnostics.CodeAnalysis;

                                class C
                                {
                                    [SuppressMessage("Design", "CA1000", Justification = nameof(C))]
                                    void M()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests
}