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

                                    public int this[int index] => _value;

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