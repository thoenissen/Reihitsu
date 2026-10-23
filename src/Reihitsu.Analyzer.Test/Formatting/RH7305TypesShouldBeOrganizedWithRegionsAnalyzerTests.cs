using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH7305TypesShouldBeOrganizedWithRegionsAnalyzer"/>
/// </summary>
[TestClass]
public class RH7305TypesShouldBeOrganizedWithRegionsAnalyzerTests : AnalyzerTestsBase<RH7305TypesShouldBeOrganizedWithRegionsAnalyzer>
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
    /// Verifies that property-only classes must also use regions
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7305-001")]
    public async Task VerifyDiagnosticForPropertyOnlyClassWithoutRegions()
    {
        const string testData = """
                                internal class {|#0:Example|}
                                {
                                    public string Name { get; set; }

                                    internal int Age { get; set; }
                                }
                                """;

        await Verify(testData, Diagnostics(RH7305TypesShouldBeOrganizedWithRegionsAnalyzer.DiagnosticId, AnalyzerResources.RH7305MessageFormat));
    }

    /// <summary>
    /// Verifies that interfaces organized with regions do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7305-002")]
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
    [Requirement("RH7305-002")]
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
    [Requirement("RH7305-001")]
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

        await Verify(testData, Diagnostics(RH7305TypesShouldBeOrganizedWithRegionsAnalyzer.DiagnosticId, AnalyzerResources.RH7305MessageFormat));
    }

    /// <summary>
    /// Verifies that types with mixed members still report diagnostics without regions
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7305-001")]
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

        await Verify(testData, Diagnostics(RH7305TypesShouldBeOrganizedWithRegionsAnalyzer.DiagnosticId, AnalyzerResources.RH7305MessageFormat));
    }

    /// <summary>
    /// Verifies that a type is still reported when only some members are placed inside regions
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7305-001")]
    [Requirement("SHARED-TYPEREGION-003")]
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

        await Verify(testData, Diagnostics(RH7305TypesShouldBeOrganizedWithRegionsAnalyzer.DiagnosticId, AnalyzerResources.RH7305MessageFormat));
    }

    /// <summary>
    /// Verifies that records with non-property members must use regions
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7305-001")]
    [Requirement("SHARED-TYPEREGION-001")]
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

        await Verify(testData, Diagnostics(RH7305TypesShouldBeOrganizedWithRegionsAnalyzer.DiagnosticId, AnalyzerResources.RH7305MessageFormat));
    }

    /// <summary>
    /// Verifies that structs with non-property members must use regions
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7305-001")]
    [Requirement("SHARED-TYPEREGION-001")]
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

        await Verify(testData, Diagnostics(RH7305TypesShouldBeOrganizedWithRegionsAnalyzer.DiagnosticId, AnalyzerResources.RH7305MessageFormat));
    }

    /// <summary>
    /// Verifies that nested types without regions do not trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TYPEREGION-001")]
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

    /// <summary>
    /// Verifies that custom-named top-level regions around direct members do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7305-002")]
    public async Task VerifyNoDiagnosticsWhenTopLevelMembersUseCustomNamedRegions()
    {
        const string testData = """
                                using System;
                                using System.Threading;
                                using System.Threading.Tasks;

                                public sealed class LockFactory : IDisposable
                                {
                                    #region Fields

                                    private readonly SemaphoreSlim _semaphore;

                                    #endregion // Fields

                                    #region Constructor

                                    public LockFactory()
                                    {
                                        _semaphore = new SemaphoreSlim(1, 1);
                                    }

                                    #endregion // Constructor

                                    #region Methods

                                    public async Task<IDisposable> CreateLockAsync()
                                    {
                                        await _semaphore.WaitAsync().ConfigureAwait(false);

                                        return new LockContainer(_semaphore);
                                    }

                                    public IDisposable CreateLock()
                                    {
                                        _semaphore.Wait();

                                        return new LockContainer(_semaphore);
                                    }

                                    #endregion // Methods

                                    #region Nested classes

                                    private sealed class LockContainer : IAsyncDisposable, IDisposable
                                    {
                                        #region Fields

                                        private readonly SemaphoreSlim _semaphore;

                                        #endregion // Fields

                                        #region Constructor

                                        public LockContainer(SemaphoreSlim semaphore)
                                        {
                                            _semaphore = semaphore;
                                        }

                                        #endregion // Constructor

                                        #region IDisposable

                                        public void Dispose()
                                        {
                                            _semaphore.Release();
                                        }

                                        #endregion // IDisposable

                                        #region IAsyncDisposable

                                        public ValueTask DisposeAsync()
                                        {
                                            Dispose();

                                            return ValueTask.CompletedTask;
                                        }

                                        #endregion // IAsyncDisposable
                                    }

                                    #endregion // Nested classes

                                    #region IDisposable

                                    public void Dispose()
                                    {
                                        _semaphore.Dispose();
                                    }

                                    #endregion // IDisposable
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a region wrapping the type itself does not count as member organization
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TYPEREGION-003")]
    public async Task VerifyDiagnosticWhenOnlyTheTypeIsWrappedInARegion()
    {
        const string testData = """
                                #region Services

                                internal class {|#0:Example|}
                                {
                                    private string _name;

                                    public string Name => _name;
                                }

                                #endregion // Services
                                """;

        await Verify(testData, Diagnostics(RH7305TypesShouldBeOrganizedWithRegionsAnalyzer.DiagnosticId, AnalyzerResources.RH7305MessageFormat));
    }

    /// <summary>
    /// Verifies that regions inside a method body do not count as member organization
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TYPEREGION-003")]
    public async Task VerifyDiagnosticWhenOnlyMethodBodyContainsARegion()
    {
        const string testData = """
                                internal class {|#0:Example|}
                                {
                                    internal void Save()
                                    {
                                        #region Local logic

                                        System.Console.WriteLine(string.Empty);
                                        
                                        #endregion // Local logic
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH7305TypesShouldBeOrganizedWithRegionsAnalyzer.DiagnosticId, AnalyzerResources.RH7305MessageFormat));
    }

    /// <summary>
    /// Verifies that a type is reported also when all its members are of one kind
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7305-001")]
    [Requirement("SHARED-TYPEREGION-004")]
    public async Task TypeWithMembersOfOneKindOutsideRegionsIsReported()
    {
        const string testData = """
                                class {|RH7305:C|}
                                {
                                    public int P { get; set; }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a type with regions that leaves one member outside them is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7305-001")]
    [Requirement("SHARED-TYPEREGION-003")]
    public async Task TypeWithOneMemberOutsideTheRegionsIsReported()
    {
        const string testData = """
                                class {|RH7305:C|}
                                {
                                    #region Methods

                                    void M()
                                    {
                                    }

                                    #endregion

                                    void N()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a type in which every member lies inside a region is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7305-002")]
    [Requirement("SHARED-TYPEREGION-003")]
    public async Task TypeWithEveryMemberInARegionIsNotReported()
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

    /// <summary>
    /// Verifies that a nested type outside every region makes the enclosing type reported, while the nested type and an enum are not checked
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TYPEREGION-001")]
    [Requirement("SHARED-TYPEREGION-002")]
    public async Task NestedTypesAndEnumsAreNotCheckedButCountAsMembers()
    {
        const string testData = """
                                class {|RH7305:C|}
                                {
                                    #region Methods

                                    void M()
                                    {
                                    }

                                    #endregion

                                    class D
                                    {
                                        int _x;

                                        void N()
                                        {
                                        }
                                    }
                                }

                                enum E
                                {
                                    A,
                                    B,
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that only the declaration of a partial type with a member outside every region is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TYPEREGION-001")]
    public async Task EachDeclarationOfAPartialTypeIsCheckedOnItsOwn()
    {
        const string testData = """
                                partial class P
                                {
                                    #region Methods

                                    void A()
                                    {
                                    }

                                    #endregion
                                }

                                partial class {|RH7305:P|}
                                {
                                    void B()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an empty class and a positional record without members of its own are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TYPEREGION-002")]
    public async Task TypesWithoutCountedMembersAreNotReported()
    {
        const string testData = """
                                class C
                                {
                                }

                                record R(int X);
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a member inside a region nested in a top-level region counts as organized
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TYPEREGION-003")]
    public async Task MemberInANestedRegionCounts()
    {
        const string testData = """
                                class C
                                {
                                    #region Members

                                    #region Fields

                                    int _a;

                                    #endregion

                                    void M()
                                    {
                                    }

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a region around the type and a region inside a method body do not organize the type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TYPEREGION-003")]
    public async Task RegionsAroundTheTypeOrInsideABodyDoNotCount()
    {
        const string testData = """
                                #region Types
                                class {|RH7305:C|}
                                {
                                    void M()
                                    {
                                        #region Body
                                        var a = 1;
                                        #endregion
                                    }
                                }
                                #endregion
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a type with several members outside regions gets one diagnostic on its name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TYPEREGION-004")]
    public async Task OneDiagnosticOnTheTypeNameForSeveralMembers()
    {
        const string testData = """
                                interface {|RH7305:I|}
                                {
                                    void M();

                                    int P { get; }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies the specification example of a type whose only member is a nested enum that lies in a region and has a
    /// region among its values: the type is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-TYPEREGION-002")]
    public async Task NestedEnumInARegionIsOneMemberWhateverRegionsItHasAmongItsValues()
    {
        const string testData = """
                                class C
                                {
                                    #region Types

                                    enum E
                                    {
                                        #region Values
                                        A,
                                        #endregion
                                    }

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests
}