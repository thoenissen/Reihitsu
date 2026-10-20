using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH2007DebugFailMustProvideMessageTextAnalyzer"/>
/// </summary>
[TestClass]
public class RH2007DebugFailMustProvideMessageTextAnalyzerTests : AnalyzerTestsBase<RH2007DebugFailMustProvideMessageTextAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifying that Debug.Fail calls with null or whitespace message text trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2007-001")]
    [Requirement("RH2007-002")]
    [Requirement("RH2007-003")]
    [Requirement("RH2007-004")]
    public async Task VerifyDiagnostics()
    {
        const string testData = """
                                using System.Diagnostics;
                                using static System.Diagnostics.Debug;

                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                internal static class Sample
                                {
                                    internal static void Verify()
                                    {
                                        {|#0:Debug.Fail(null)|};
                                        {|#1:Fail("")|};
                                        {|#2:Debug.Fail("   ", "detail")|};

                                        Debug.Fail("Do not get here.");
                                        Fail("Do not get here.");
                                        Debug.Fail("Do not get here.", "detail");
                                        CustomDebug.Fail(null);
                                    }

                                    private static class CustomDebug
                                    {
                                        public static void Fail(string message)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH2007DebugFailMustProvideMessageTextAnalyzer.DiagnosticId, AnalyzerResources.RH2007MessageFormat, 3));
    }

    /// <summary>
    /// Verifies that the message parameter is analyzed when named arguments are in reverse source order
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2007-001")]
    public async Task VerifyReorderedNamedMessageArgumentIsAnalyzed()
    {
        const string testData = """
                                using System.Diagnostics;

                                internal static class Sample
                                {
                                    internal static void Verify()
                                    {
                                        {|#0:Debug.Fail(detailMessage: "detail", message: "")|};
                                        Debug.Fail(detailMessage: "detail", message: "Do not get here.");
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH2007DebugFailMustProvideMessageTextAnalyzer.DiagnosticId, AnalyzerResources.RH2007MessageFormat));
    }

    /// <summary>
    /// Verifying that every form of a compile-time constant without text is reported, and that text and values not
    /// known at compile time are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DEBUG-001")]
    [Requirement("RH2007-001")]
    public async Task VerifyEveryFormOfConstantWithoutTextIsReported()
    {
        const string testData = """
                                using System.Diagnostics;

                                class C
                                {
                                    const string Empty = "";

                                    string Describe() => "described";

                                    void M(string message, int value)
                                    {
                                        {|#0:Debug.Fail(null)|};
                                        {|#1:Debug.Fail(default)|};
                                        {|#2:Debug.Fail((string)null)|};
                                        {|#3:Debug.Fail("")|};
                                        {|#4:Debug.Fail(" \t\n")|};
                                        {|#5:Debug.Fail(" ")|};
                                        {|#6:Debug.Fail(Empty)|};
                                        {|#7:Debug.Fail("" + " ")|};
                                        {|#8:Debug.Fail($"")|};
                                        {|#9:Debug.Fail($"{Empty}")|};
                                        Debug.Fail("Unreachable branch.");
                                        Debug.Fail(message);
                                        Debug.Fail(Describe());
                                        Debug.Fail($"Value {value}");
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH2007DebugFailMustProvideMessageTextAnalyzer.DiagnosticId, AnalyzerResources.RH2007MessageFormat, 10));
    }

    /// <summary>
    /// Verifying the spec example of failures with and without message text, called directly, through
    /// <see langword="using"/> <see langword="static"/>, and with named arguments
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2007-001")]
    [Requirement("RH2007-004")]
    public async Task VerifyFailuresWithoutMessageTextAreReported()
    {
        const string testData = """
                                using System.Diagnostics;
                                using static System.Diagnostics.Debug;

                                class C
                                {
                                    void M()
                                    {
                                        {|#0:Debug.Fail(null)|};
                                        {|#1:Fail("")|};
                                        {|#2:Debug.Fail(detailMessage: "Detail", message: "")|};
                                        Debug.Fail("Unreachable branch.");
                                        Fail("Unreachable branch.");
                                        Debug.Fail(detailMessage: "Detail", message: "Unreachable branch.");
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH2007DebugFailMustProvideMessageTextAnalyzer.DiagnosticId, AnalyzerResources.RH2007MessageFormat, 3));
    }

    /// <summary>
    /// Verifying that only the message argument decides, whatever the detail message holds
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2007-002")]
    public async Task VerifyOnlyMessageArgumentIsChecked()
    {
        const string testData = """
                                using System.Diagnostics;

                                class C
                                {
                                    void M()
                                    {
                                        Debug.Fail("Unreachable branch.", "");
                                        Debug.Fail("Unreachable branch.", null);
                                        {|#0:Debug.Fail("   ", "Detail")|};
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH2007DebugFailMustProvideMessageTextAnalyzer.DiagnosticId, AnalyzerResources.RH2007MessageFormat));
    }

    /// <summary>
    /// Verifying that methods named <c>Fail</c> on other types are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2007-003")]
    public async Task VerifyFailMethodsOfOtherTypesAreNotReported()
    {
        const string testData = """
                                using System.Diagnostics;

                                class C
                                {
                                    void M()
                                    {
                                        Trace.Fail("");
                                        Check.Fail(null);
                                    }

                                    static class Check
                                    {
                                        public static void Fail(string message)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that the diagnostic covers the whole invocation when the call spans several lines
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2007-004")]
    public async Task VerifyDiagnosticCoversWholeMultiLineInvocation()
    {
        const string testData = """
                                class C
                                {
                                    void M()
                                    {
                                        {|#0:System.Diagnostics.Debug.Fail(
                                            "",
                                            "Detail")|};
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH2007DebugFailMustProvideMessageTextAnalyzer.DiagnosticId, AnalyzerResources.RH2007MessageFormat));
    }

    /// <summary>
    /// Characterizes <see cref="string.Empty"/> and a conditional expression whose every result is a constant without
    /// text: neither is reported, because only compile-time constants are judged. These cases are an open question
    /// about intended behavior, so this test pins today's behavior without stating it as intended
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyStringEmptyAndConditionalsWithoutTextAreNotReported()
    {
        const string testData = """
                                using System.Diagnostics;

                                class C
                                {
                                    void M(bool flag)
                                    {
                                        Debug.Fail(string.Empty);
                                        Debug.Fail(flag ? " " : "");
                                    }
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests
}