using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Ordering;

/// <summary>
/// Test methods for <see cref="RH7110MembersMustBeOrderedByAccessibilityAnalyzer"/>
/// </summary>
[TestClass]
public class RH7110MembersMustBeOrderedByAccessibilityAnalyzerTests : AnalyzerTestsBase<RH7110MembersMustBeOrderedByAccessibilityAnalyzer>
{
    #region Diagnostic cases

    /// <summary>
    /// Verifying a public method after a private method of the same kind is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task PublicMethodAfterPrivateMethodIsReported()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    private void Helper()
                                    {
                                    }

                                    public void {|#0:Run|}()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH7110MembersMustBeOrderedByAccessibilityAnalyzer.DiagnosticId, AnalyzerResources.RH7110MessageFormat));
    }

    /// <summary>
    /// Verifying a protected member after a private member of the same kind is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ProtectedMethodAfterPrivateMethodIsReported()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    private void Helper()
                                    {
                                    }

                                    protected void {|#0:Run|}()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH7110MembersMustBeOrderedByAccessibilityAnalyzer.DiagnosticId, AnalyzerResources.RH7110MessageFormat));
    }

    /// <summary>
    /// Verifying an internal member after a protected member of the same kind is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task InternalMethodAfterProtectedMethodIsReported()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    protected void Helper()
                                    {
                                    }

                                    internal void {|#0:Run|}()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH7110MembersMustBeOrderedByAccessibilityAnalyzer.DiagnosticId, AnalyzerResources.RH7110MessageFormat));
    }

    /// <summary>
    /// Verifying a protected internal member after a protected member of the same kind is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ProtectedInternalMethodAfterProtectedMethodIsReported()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    protected void Helper()
                                    {
                                    }

                                    protected internal void {|#0:Run|}()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH7110MembersMustBeOrderedByAccessibilityAnalyzer.DiagnosticId, AnalyzerResources.RH7110MessageFormat));
    }

    /// <summary>
    /// Verifying a public field after a private field is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task PublicFieldAfterPrivateFieldIsReported()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    private int _value;

                                    public int {|#0:Value|};
                                }
                                """;

        await Verify(testCode, Diagnostics(RH7110MembersMustBeOrderedByAccessibilityAnalyzer.DiagnosticId, AnalyzerResources.RH7110MessageFormat));
    }

    /// <summary>
    /// Verifying a public member after a private member of the same kind within the same region is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task PublicMemberAfterPrivateMemberOfSameKindWithinSameRegionIsReported()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    #region Methods
                                    private void Helper()
                                    {
                                    }

                                    public void {|#0:Run|}()
                                    {
                                    }
                                    #endregion
                                }
                                """;

        await Verify(testCode, Diagnostics(RH7110MembersMustBeOrderedByAccessibilityAnalyzer.DiagnosticId, AnalyzerResources.RH7110MessageFormat));
    }

    #endregion // Diagnostic cases

    #region Non-diagnostic cases

    /// <summary>
    /// Verifying members ordered from most to least accessible produce no diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task MembersOrderedFromMostToLeastAccessibleProduceNoDiagnostic()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    public void Run()
                                    {
                                    }

                                    internal void Configure()
                                    {
                                    }

                                    protected void Prepare()
                                    {
                                    }

                                    private void Helper()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying the static ordering within an accessibility group does not conflict with this rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task StaticBeforeInstanceWithinAccessibilityGroupProducesNoDiagnostic()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    public static void Create()
                                    {
                                    }

                                    public void Run()
                                    {
                                    }

                                    private static void CreateHelper()
                                    {
                                    }

                                    private void Helper()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying members of different kinds are not compared against each other
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task DifferentKindsAreNotComparedAgainstEachOther()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    private int _value;

                                    public void Run()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying accessibility ordering is evaluated within each region independently
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task OrderingIsEvaluatedWithinEachRegionIndependently()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    #region First
                                    private void FirstHelper()
                                    {
                                    }
                                    #endregion

                                    #region Second
                                    public void SecondRun()
                                    {
                                    }
                                    #endregion
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying members without an explicit accessibility modifier are ignored
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task MembersWithoutExplicitAccessibilityAreIgnored()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    void Helper()
                                    {
                                    }

                                    public void Run()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    #endregion // Non-diagnostic cases
}