using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH7308StandardRegionsShouldContainOnlyMatchingMemberKindAnalyzer"/>
/// </summary>
[TestClass]
public class RH7308StandardRegionsShouldContainOnlyMatchingMemberKindAnalyzerTests : AnalyzerTestsBase<RH7308StandardRegionsShouldContainOnlyMatchingMemberKindAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that a property placed in a <c>Fields</c> region produces a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPropertyInFieldsRegion()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Fields

                                    private int _value;

                                    public int {|#0:Value|} => _value;

                                    #endregion // Fields
                                }
                                """;

        await Verify(testData, Diagnostics(RH7308StandardRegionsShouldContainOnlyMatchingMemberKindAnalyzer.DiagnosticId, CreateMessage("Fields", "field declarations")));
    }

    /// <summary>
    /// Verifies that a method placed in a <c>Fields</c> region produces a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForMethodInFieldsRegion()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Fields

                                    private int _value;

                                    public void {|#0:Reset|}()
                                    {
                                    }

                                    #endregion // Fields
                                }
                                """;

        await Verify(testData, Diagnostics(RH7308StandardRegionsShouldContainOnlyMatchingMemberKindAnalyzer.DiagnosticId, CreateMessage("Fields", "field declarations")));
    }

    /// <summary>
    /// Verifies that a field placed in a <c>Properties</c> region produces a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForFieldInPropertiesRegion()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Properties

                                    private int {|#0:_value|};

                                    public int Value { get; set; }

                                    #endregion // Properties
                                }
                                """;

        await Verify(testData, Diagnostics(RH7308StandardRegionsShouldContainOnlyMatchingMemberKindAnalyzer.DiagnosticId, CreateMessage("Properties", "property declarations")));
    }

    /// <summary>
    /// Verifies that a constructor placed in a <c>Methods</c> region produces a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForConstructorInMethodsRegion()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Methods

                                    public {|#0:TestClass|}()
                                    {
                                    }

                                    public void Run()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        await Verify(testData, Diagnostics(RH7308StandardRegionsShouldContainOnlyMatchingMemberKindAnalyzer.DiagnosticId, CreateMessage("Methods", "method declarations")));
    }

    /// <summary>
    /// Verifies that an event placed in a <c>Methods</c> region produces a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForEventInMethodsRegion()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    #region Methods

                                    public event Action {|#0:Changed|};

                                    public void Run()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        await Verify(testData, Diagnostics(RH7308StandardRegionsShouldContainOnlyMatchingMemberKindAnalyzer.DiagnosticId, CreateMessage("Methods", "method declarations")));
    }

    /// <summary>
    /// Verifies that members placed in their matching standard regions do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForMatchingStandardRegions()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Fields

                                    private int _value;

                                    #endregion // Fields

                                    #region Constructors

                                    public TestClass()
                                    {
                                    }

                                    #endregion // Constructors

                                    #region Properties

                                    public int Value => _value;

                                    #endregion // Properties

                                    #region Methods

                                    public void Run()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a member placed in a non-standard region does not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForNonStandardRegion()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Helpers

                                    private int _value;

                                    public int Value => _value;

                                    #endregion // Helpers
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a nested type placed in a standard region does not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForNestedTypeInStandardRegion()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Fields

                                    private int _value;

                                    private class Nested
                                    {
                                    }

                                    #endregion // Fields
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an untracked member kind placed in a standard region does not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForUntrackedMemberKindInStandardRegion()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Fields

                                    private int _value;

                                    private delegate void Handler();

                                    #endregion // Fields
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that members outside of any region do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForMembersWithoutRegions()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int _value;

                                    public int Value => _value;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a singular region label is recognized
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPropertyInSingularFieldRegion()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Field

                                    private int _value;

                                    public int {|#0:Value|} => _value;

                                    #endregion // Field
                                }
                                """;

        await Verify(testData, Diagnostics(RH7308StandardRegionsShouldContainOnlyMatchingMemberKindAnalyzer.DiagnosticId, CreateMessage("Field", "field declarations")));
    }

    /// <summary>
    /// Verifies that a region label is matched case-insensitively
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForRegionLabelMatchedCaseInsensitively()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region FIELDS

                                    private int _value;

                                    public int {|#0:Value|} => _value;

                                    #endregion // FIELDS
                                }
                                """;

        await Verify(testData, Diagnostics(RH7308StandardRegionsShouldContainOnlyMatchingMemberKindAnalyzer.DiagnosticId, CreateMessage("FIELDS", "field declarations")));
    }

    /// <summary>
    /// Verifies that a modifier-qualified region label is recognized by its member kind noun
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPropertyInPrivateMethodsRegion()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Private methods

                                    private int {|#0:Value|} => 0;

                                    private void Run()
                                    {
                                    }

                                    #endregion // Private methods
                                }
                                """;

        await Verify(testData, Diagnostics(RH7308StandardRegionsShouldContainOnlyMatchingMemberKindAnalyzer.DiagnosticId, CreateMessage("Private methods", "method declarations")));
    }

    /// <summary>
    /// Verifies that a matching member in a modifier-qualified region does not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForMethodInProtectedMethodsRegion()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Protected methods

                                    protected void Run()
                                    {
                                    }

                                    #endregion // Protected methods
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a combined region label accepts every listed member kind
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForCombinedConstructorsAndFinalizerRegion()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Constructors / Finalizer

                                    public TestClass()
                                    {
                                    }

                                    ~TestClass()
                                    {
                                    }

                                    #endregion // Constructors / Finalizer
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a member not listed in a combined region label produces a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForMethodInCombinedConstructorsAndFinalizerRegion()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Constructors / Finalizer

                                    public TestClass()
                                    {
                                    }

                                    public void {|#0:Run|}()
                                    {
                                    }

                                    #endregion // Constructors / Finalizer
                                }
                                """;

        await Verify(testData, Diagnostics(RH7308StandardRegionsShouldContainOnlyMatchingMemberKindAnalyzer.DiagnosticId, CreateMessage("Constructors / Finalizer", "constructor or finalizer declarations")));
    }

    /// <summary>
    /// Verifies that a <c>Constants</c> region is treated as a field region
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPropertyInConstantsRegion()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Constants

                                    private const int Limit = 1;

                                    public int {|#0:Value|} => Limit;

                                    #endregion // Constants
                                }
                                """;

        await Verify(testData, Diagnostics(RH7308StandardRegionsShouldContainOnlyMatchingMemberKindAnalyzer.DiagnosticId, CreateMessage("Constants", "field declarations")));
    }

    /// <summary>
    /// Verifies that a finalizer placed in a <c>Methods</c> region produces a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForFinalizerInMethodsRegion()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Methods

                                    ~{|#0:TestClass|}()
                                    {
                                    }

                                    public void Run()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        await Verify(testData, Diagnostics(RH7308StandardRegionsShouldContainOnlyMatchingMemberKindAnalyzer.DiagnosticId, CreateMessage("Methods", "method declarations")));
    }

    /// <summary>
    /// Verifies that interface members are also checked
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForMethodInInterfacePropertiesRegion()
    {
        const string testData = """
                                internal interface ITestInterface
                                {
                                    #region Properties

                                    int Value { get; }

                                    void {|#0:Run|}();

                                    #endregion // Properties
                                }
                                """;

        await Verify(testData, Diagnostics(RH7308StandardRegionsShouldContainOnlyMatchingMemberKindAnalyzer.DiagnosticId, CreateMessage("Properties", "property declarations")));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Creates the expected diagnostic message for the given region and expected member kind
    /// </summary>
    /// <param name="regionName">Region name</param>
    /// <param name="expectedKind">Expected member kind</param>
    /// <returns>Diagnostic message</returns>
    private static string CreateMessage(string regionName, string expectedKind)
    {
        return string.Format(AnalyzerResources.RH7308MessageFormat, regionName, expectedKind);
    }

    #endregion // Methods
}