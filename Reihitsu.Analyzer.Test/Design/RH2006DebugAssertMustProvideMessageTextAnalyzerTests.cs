using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH2006DebugAssertMustProvideMessageTextAnalyzer"/>
/// </summary>
[TestClass]
public class RH2006DebugAssertMustProvideMessageTextAnalyzerTests : AnalyzerTestsBase<RH2006DebugAssertMustProvideMessageTextAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifying that Debug.Assert calls without message text trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnostics()
    {
        const string testData = """
                                using System.Diagnostics;
                                using static System.Diagnostics.Debug;

                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                internal static class Sample
                                {
                                    internal static void Verify(object value)
                                    {
                                        {|#0:Debug.Assert(value != null)|};
                                        {|#1:Assert(value != null)|};

                                        Debug.Assert(value != null, "Value must not be null.");
                                        Assert(value != null, "Value must not be null.");
                                        CustomDebug.Assert(value != null);
                                    }

                                    private static class CustomDebug
                                    {
                                        public static void Assert(bool condition)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH2006DebugAssertMustProvideMessageTextAnalyzer.DiagnosticId, AnalyzerResources.RH2006MessageFormat, 2));
    }

    /// <summary>
    /// Verifying that constant messages without text trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNullEmptyAndWhitespaceMessagesTriggerDiagnostics()
    {
        const string testData = """
                                using System.Diagnostics;

                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                internal static class Sample
                                {
                                    internal static void Verify(bool condition)
                                    {
                                        {|#0:Debug.Assert(condition, null)|};
                                        {|#1:Debug.Assert(condition, "")|};
                                        {|#2:Debug.Assert(condition, "  \t")|};
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH2006DebugAssertMustProvideMessageTextAnalyzer.DiagnosticId, AnalyzerResources.RH2006MessageFormat, 3));
    }

    /// <summary>
    /// Verifying that named arguments are associated with the message parameter rather than their source order
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyReversedNamedArgumentsUseMessageParameter()
    {
        const string testData = """
                                using System.Diagnostics;

                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                internal static class Sample
                                {
                                    internal static void Verify()
                                    {
                                        {|#0:Debug.Assert(message: "", condition: true)|};
                                        Debug.Assert(message: "Condition must hold.", condition: true);
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH2006DebugAssertMustProvideMessageTextAnalyzer.DiagnosticId, AnalyzerResources.RH2006MessageFormat));
    }

    /// <summary>
    /// Verifying that descriptive and nonconstant messages do not trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNonemptyAndNonconstantMessagesDoNotTriggerDiagnostics()
    {
        const string testData = """
                                using System.Diagnostics;

                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                internal static class Sample
                                {
                                    internal static void Verify(bool condition, string message)
                                    {
                                        Debug.Assert(condition, "Condition must hold.");
                                        Debug.Assert(condition, message);
                                    }
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests
}