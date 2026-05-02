using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0387TypesShouldBeOrganizedWithRegionsAnalyzer"/>
/// </summary>
[TestClass]
public class RH0387TypesShouldBeOrganizedWithRegionsAnalyzerTests : AnalyzerTestsBase<RH0387TypesShouldBeOrganizedWithRegionsAnalyzer>
{
    #region Members

    /// <summary>
    /// Verifies that empty classes do not require regions
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
    /// Verifies that property-only classes must also use regions
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPropertyOnlyClassWithoutRegions()
    {
        const string testData = """
                                internal class {|#0:Example|}
                                {
                                    public string Name { get; set; }

                                    internal int Age { get; set; }
                                }
                                """;

        await Verify(testData, Diagnostics(RH0387TypesShouldBeOrganizedWithRegionsAnalyzer.DiagnosticId, AnalyzerResources.RH0387MessageFormat));
    }

    /// <summary>
    /// Verifies that interfaces organized with regions do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenInterfaceMembersAreOrganizedWithRegions()
    {
        const string testData = """
                                internal interface IExample
                                {
                                    #region Properties

                                    string Name { get; }

                                    #endregion // Properties

                                    #region Events

                                    event System.Action Saved;

                                    #endregion // Events

                                    #region Indexers

                                    string this[int index] { get; }

                                    #endregion // Indexers

                                    #region Methods

                                    void Load();

                                    #endregion // Methods

                                    #region Types

                                    interface INested
                                    {
                                    }

                                    #endregion // Types
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that mixed class members organized with regions do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

                                    #region Types

                                    internal class Nested
                                    {
                                    }

                                    #endregion // Types
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that classes with non-property members must use regions
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticWhenClassMembersAreNotOrganizedWithRegions()
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

                                    internal class Nested
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH0387TypesShouldBeOrganizedWithRegionsAnalyzer.DiagnosticId, AnalyzerResources.RH0387MessageFormat));
    }

    /// <summary>
    /// Verifies that types with mixed members still report diagnostics without regions
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticWhenTypeContainsPropertiesAndMethodsWithoutRegions()
    {
        const string testData = """
                                internal class {|#0:Example|}
                                {
                                    public string Name { get; set; }

                                    public int Age { get; set; }

                                    public string Address { get; set; }

                                    public string Phone { get; set; }

                                    internal void Save()
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH0387TypesShouldBeOrganizedWithRegionsAnalyzer.DiagnosticId, AnalyzerResources.RH0387MessageFormat));
    }

    /// <summary>
    /// Verifies that a type is still reported when only some members are placed inside regions
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

                                    public int Age { get; set; }

                                    #endregion // Properties

                                    internal Example()
                                    {
                                    }

                                    internal void Save()
                                    {
                                    }

                                    internal class Nested
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH0387TypesShouldBeOrganizedWithRegionsAnalyzer.DiagnosticId, AnalyzerResources.RH0387MessageFormat));
    }

    /// <summary>
    /// Verifies that records with non-property members must use regions
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticWhenRecordMembersAreNotOrganizedWithRegions()
    {
        const string testData = """
                                internal record {|#0:Example|}
                                {
                                    private readonly string _name;

                                    public string Name { get; init; }

                                    internal Example(string name)
                                    {
                                        _name = name;
                                    }

                                    internal void Save()
                                    {
                                    }

                                    internal class Nested
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH0387TypesShouldBeOrganizedWithRegionsAnalyzer.DiagnosticId, AnalyzerResources.RH0387MessageFormat));
    }

    /// <summary>
    /// Verifies that structs with non-property members must use regions
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticWhenStructMembersAreNotOrganizedWithRegions()
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

                                    internal void Save()
                                    {
                                    }

                                    internal class Nested
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH0387TypesShouldBeOrganizedWithRegionsAnalyzer.DiagnosticId, AnalyzerResources.RH0387MessageFormat));
    }

    /// <summary>
    /// Verifies that nested types without regions do not trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForNestedTypeWithoutRegions()
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
    /// Verifies that nested types with regions do not trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForNestedTypesAndRegions()
    {
        const string testData = """
                                public class Root
                                {
                                    #region Constants

                                    public static class NestedOne
                                    {
                                        public const string One = "One";
                                        public const string Two = "Two";
                                    }

                                    public static class NestedTwo
                                    {
                                        public const string Value = "Value";
                                    }

                                    #endregion // Constants
                                
                                    #region Properties

                                    #region Data
                                    
                                    public string Data { get; set; }

                                    #endregion // Data
                                
                                    #region Flags

                                    public bool Flag { get; set; }
                                
                                    #endregion // Flags

                                    #endregion // Properties
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Members
}