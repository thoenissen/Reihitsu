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
    [Requirement("RH2006-001")]
    [Requirement("RH2006-003")]
    [Requirement("RH2006-004")]
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
    [Requirement("SHARED-DEBUG-001")]
    [Requirement("RH2006-001")]
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
    [Requirement("RH2006-001")]
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
    [Requirement("SHARED-DEBUG-001")]
    [Requirement("RH2006-001")]
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

    /// <summary>
    /// Verifying that every form of a compile-time constant without text is reported, and that text and values not
    /// known at compile time are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DEBUG-001")]
    [Requirement("RH2006-001")]
    public async Task VerifyEveryFormOfConstantWithoutTextIsReported()
    {
        const string testData = """
                                using System.Diagnostics;

                                class C
                                {
                                    const string Empty = "";

                                    string Describe() => "described";

                                    void M(bool condition, string message, int value)
                                    {
                                        const string localEmpty = " ";

                                        {|#0:Debug.Assert(condition, null)|};
                                        {|#1:Debug.Assert(condition, default)|};
                                        {|#2:Debug.Assert(condition, (string)null)|};
                                        {|#3:Debug.Assert(condition, "")|};
                                        {|#4:Debug.Assert(condition, " \t\n")|};
                                        {|#5:Debug.Assert(condition, " ")|};
                                        {|#6:Debug.Assert(condition, Empty)|};
                                        {|#7:Debug.Assert(condition, localEmpty)|};
                                        {|#8:Debug.Assert(condition, "" + " ")|};
                                        {|#9:Debug.Assert(condition, $"")|};
                                        {|#10:Debug.Assert(condition, $"{Empty}")|};
                                        Debug.Assert(condition, "Condition must hold.");
                                        Debug.Assert(condition, message);
                                        Debug.Assert(condition, Describe());
                                        Debug.Assert(condition, $"Value {value}");
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH2006DebugAssertMustProvideMessageTextAnalyzer.DiagnosticId, AnalyzerResources.RH2006MessageFormat, 11));
    }

    /// <summary>
    /// Verifying the spec example of assertions with and without message text, called directly, through
    /// <see langword="using"/> <see langword="static"/>, and with named arguments
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2006-001")]
    [Requirement("RH2006-004")]
    public async Task VerifyAssertionsWithoutMessageTextAreReported()
    {
        const string testData = """
                                using System.Diagnostics;
                                using static System.Diagnostics.Debug;

                                class C
                                {
                                    void M(bool condition)
                                    {
                                        {|#0:Debug.Assert(condition)|};
                                        {|#1:Assert(condition)|};
                                        {|#2:Debug.Assert(condition, "")|};
                                        {|#3:Debug.Assert(message: "", condition: condition)|};
                                        Debug.Assert(condition, "Condition must hold.");
                                        Assert(condition, "Condition must hold.");
                                        Debug.Assert(message: "Condition must hold.", condition: condition);
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH2006DebugAssertMustProvideMessageTextAnalyzer.DiagnosticId, AnalyzerResources.RH2006MessageFormat, 4));
    }

    /// <summary>
    /// Verifying that only the message argument decides, whatever the detail message holds
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2006-002")]
    public async Task VerifyOnlyMessageArgumentIsChecked()
    {
        const string testData = """
                                using System.Diagnostics;

                                class C
                                {
                                    void M(bool condition)
                                    {
                                        Debug.Assert(condition, "Condition must hold.", "");
                                        Debug.Assert(condition, "Condition must hold.", null);
                                        {|#0:Debug.Assert(condition, "", "Detail")|};
                                        {|#1:Debug.Assert(condition, "", "Detail {0}", 1)|};
                                        {|#2:Debug.Assert(condition, detailMessage: "Detail", message: null)|};
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH2006DebugAssertMustProvideMessageTextAnalyzer.DiagnosticId, AnalyzerResources.RH2006MessageFormat, 3));
    }

    /// <summary>
    /// Verifying that methods named <c>Assert</c> on other types are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2006-003")]
    public async Task VerifyAssertMethodsOfOtherTypesAreNotReported()
    {
        const string testData = """
                                using System.Diagnostics;

                                class C
                                {
                                    void M(bool condition)
                                    {
                                        Trace.Assert(condition);
                                        Trace.Assert(condition, "");
                                        Check.Assert(condition);
                                    }

                                    static class Check
                                    {
                                        public static void Assert(bool condition)
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
    [Requirement("RH2006-004")]
    public async Task VerifyDiagnosticCoversWholeMultiLineInvocation()
    {
        const string testData = """
                                using System.Diagnostics;

                                class C
                                {
                                    void M(bool condition)
                                    {
                                        {|#0:System.Diagnostics.Debug.Assert(
                                            condition,
                                            "")|};
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH2006DebugAssertMustProvideMessageTextAnalyzer.DiagnosticId, AnalyzerResources.RH2006MessageFormat));
    }

    /// <summary>
    /// Characterizes <see cref="string.Empty"/> and conditional or <see langword="switch"/> expressions whose every
    /// result is a constant without text: none of them is reported, because only compile-time constants are judged.
    /// These cases are an open question about intended behavior, so this test pins today's behavior without stating
    /// it as intended
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyStringEmptyAndConditionalsWithoutTextAreNotReported()
    {
        const string testData = """
                                using System.Diagnostics;

                                class C
                                {
                                    void M(bool condition, bool flag, int mode)
                                    {
                                        Debug.Assert(condition, string.Empty);
                                        Debug.Assert(condition, flag ? "" : null);
                                        Debug.Assert(condition, mode switch { 0 => "", _ => " " });
                                        {|#0:Debug.Assert(condition, "")|};
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH2006DebugAssertMustProvideMessageTextAnalyzer.DiagnosticId, AnalyzerResources.RH2006MessageFormat));
    }

    #endregion // Tests
}