using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH7305ATypesShouldBeOrganizedWithRegionsForMultipleKindsAnalyzer"/>
/// </summary>
[TestClass]
public class RH7305ATypesShouldBeOrganizedWithRegionsForMultipleKindsAnalyzerTests : AnalyzerTestsBase<RH7305ATypesShouldBeOrganizedWithRegionsForMultipleKindsAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that empty classes do not require regions
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TYPEREGION-002")]
    public async Task VerifyNoDiagnosticsForEmptyClass()
    {
        const string testData = """
                                internal class Example
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a class with only properties does not require regions
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7305A-002")]
    public async Task VerifyNoDiagnosticsForPropertyOnlyClassWithoutRegions()
    {
        const string testData = """
                                internal class Example
                                {
                                    public string Name { get; set; }

                                    internal int Age { get; set; }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a class with only methods does not require regions
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7305A-002")]
    public async Task VerifyNoDiagnosticsForMethodOnlyClassWithoutRegions()
    {
        const string testData = """
                                internal class Example
                                {
                                    internal void Save()
                                    {
                                    }

                                    internal void Load()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a class with only fields does not require regions
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7305A-002")]
    public async Task VerifyNoDiagnosticsForFieldOnlyClassWithoutRegions()
    {
        const string testData = """
                                internal class Example
                                {
                                    internal string Name;

                                    internal int Age;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a class with mixed member kinds and no regions produces a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7305A-001")]
    public async Task VerifyDiagnosticWhenClassHasMixedMemberKindsWithoutRegions()
    {
        const string testData = """
                                internal class {|#0:Example|}
                                {
                                    private string _name;

                                    public string Name { get; set; }

                                    internal Example()
                                    {
                                    }

                                    internal void Save()
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH7305ATypesShouldBeOrganizedWithRegionsForMultipleKindsAnalyzer.DiagnosticId, AnalyzerResources.RH7305AMessageFormat));
    }

    /// <summary>
    /// Verifies that a class with properties and methods but no regions produces a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7305A-001")]
    public async Task VerifyDiagnosticWhenClassHasPropertiesAndMethodsWithoutRegions()
    {
        const string testData = """
                                internal class {|#0:Example|}
                                {
                                    public string Name { get; set; }

                                    internal void Save()
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH7305ATypesShouldBeOrganizedWithRegionsForMultipleKindsAnalyzer.DiagnosticId, AnalyzerResources.RH7305AMessageFormat));
    }

    /// <summary>
    /// Verifies that a class with mixed member kinds and all members in regions does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7305A-003")]
    public async Task VerifyNoDiagnosticsWhenClassMembersAreOrganizedWithRegions()
    {
        const string testData = """
                                internal class Example
                                {
                                    #region Fields

                                    private string _name;

                                    #endregion // Fields

                                    #region Properties

                                    public string Name => _name;

                                    #endregion // Properties

                                    #region Constructors

                                    internal Example(string name)
                                    {
                                        _name = name;
                                    }

                                    #endregion // Constructors

                                    #region Methods

                                    internal void Save()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a type with mixed members still reports a diagnostic when only some members are in regions
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7305A-001")]
    public async Task VerifyDiagnosticWhenTypeContainsMembersOutsideRegions()
    {
        const string testData = """
                                internal class {|#0:Example|}
                                {
                                    #region Fields

                                    private string _name;

                                    #endregion // Fields

                                    #region Properties

                                    public string Name { get; set; }

                                    #endregion // Properties

                                    internal Example()
                                    {
                                    }

                                    internal void Save()
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH7305ATypesShouldBeOrganizedWithRegionsForMultipleKindsAnalyzer.DiagnosticId, AnalyzerResources.RH7305AMessageFormat));
    }

    /// <summary>
    /// Verifies that nested types without regions do not trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TYPEREGION-001")]
    public async Task VerifyNoDiagnosticsForNestedTypeWithMixedMembersWithoutRegions()
    {
        const string testData = """
                                internal class Example
                                {
                                    #region Fields

                                    private string _name;

                                    #endregion // Fields

                                    #region Properties

                                    public string Name => _name;

                                    #endregion // Properties

                                    #region Types

                                    internal class Nested
                                    {
                                        private int _value;

                                        public int Value { get; set; }
                                    }

                                    #endregion // Types
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a struct with mixed member kinds and no regions produces a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7305A-001")]
    public async Task VerifyDiagnosticWhenStructHasMixedMemberKindsWithoutRegions()
    {
        const string testData = """
                                internal struct {|#0:Example|}
                                {
                                    internal int Value;

                                    internal string Name { get; set; }

                                    public Example()
                                    {
                                        Name = string.Empty;
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH7305ATypesShouldBeOrganizedWithRegionsForMultipleKindsAnalyzer.DiagnosticId, AnalyzerResources.RH7305AMessageFormat));
    }

    /// <summary>
    /// Verifies that a record with only constructors does not require regions
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7305A-002")]
    public async Task VerifyNoDiagnosticsForRecordWithOnlyConstructors()
    {
        const string testData = """
                                internal record Example
                                {
                                    internal Example()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a type with members of two kinds outside regions is reported on its name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7305A-001")]
    [Requirement("SHARED-TYPEREGION-004")]
    public async Task TypeMixingFieldsAndMethodsOutsideRegionsIsReported()
    {
        const string testData = """
                                class {|RH7305A:C|}
                                {
                                    private int _a;

                                    public void M()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a nested type outside the regions counts as a kind of its own
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7305A-001")]
    [Requirement("SHARED-TYPEREGION-002")]
    [Requirement("SHARED-TYPEREGION-003")]
    public async Task NestedTypeCountsAsAKindOfItsOwn()
    {
        const string testData = """
                                class {|RH7305A:C|}
                                {
                                    #region Methods

                                    void M()
                                    {
                                    }

                                    #endregion

                                    class D
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an event field and an event with accessors are one kind
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7305A-002")]
    public async Task EventFieldAndEventWithAccessorsAreOneKind()
    {
        const string testData = """
                                class C
                                {
                                    public event System.EventHandler A;

                                    public event System.EventHandler B
                                    {
                                        add { }
                                        remove { }
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that operators and conversion operators are one kind
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7305A-002")]
    public async Task OperatorsAndConversionOperatorsAreOneKind()
    {
        const string testData = """
                                class C
                                {
                                    public static C operator +(C left, C right) => left;

                                    public static implicit operator int(C value) => 0;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a type adding a member of another kind is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7305A-001")]
    [Requirement("RH7305A-002")]
    public async Task EventAndOperatorAreTwoKinds()
    {
        const string testData = """
                                class {|RH7305A:C|}
                                {
                                    public event System.EventHandler A;

                                    public static C operator +(C left, C right) => left;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a type in which every member lies inside a region is not reported, whatever kinds of member it declares
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7305A-003")]
    [Requirement("SHARED-TYPEREGION-003")]
    public async Task OrganizedTypeWithSeveralKindsIsNotReported()
    {
        const string testData = """
                                class C
                                {
                                    #region Fields

                                    private int _a;

                                    #endregion // Fields

                                    #region Methods

                                    public void M()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests
}