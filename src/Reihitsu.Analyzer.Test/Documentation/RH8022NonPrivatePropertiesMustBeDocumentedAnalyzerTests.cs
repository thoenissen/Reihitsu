using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8022NonPrivatePropertiesMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH8022NonPrivatePropertiesMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH8022NonPrivatePropertiesMustBeDocumentedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic is reported for a declaration without required XML documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8022-001")]
    [Requirement("RH8022-002")]
    public async Task VerifyDiagnosticForPropertyWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Stores values.</summary>
                              internal class TestClass
                              {
                                  internal int {|#0:Value|} { get; }
                              }
                              """;

        await Verify(source, Diagnostics(RH8022NonPrivatePropertiesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8022MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when documentation mode is none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8022-001")]
    [Requirement("SHARED-DOC-008")]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              internal class TestClass { internal int Value { get; } }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    /// <summary>
    /// Verifies extension member properties are still validated by this rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8022-001")]
    [Requirement("RH8022-002")]
    [Requirement("SHARED-DOC-001")]
    public async Task VerifyDiagnosticForUndocumentedExtensionMemberProperty()
    {
        const string source = """
                              public static class Extensions
                              {
                                  /// <summary>Provides text helpers.</summary>
                                  /// <param name="value">The source text.</param>
                                  extension(string value)
                                  {
                                      public int {|#0:Length|} => value.Length;
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8022NonPrivatePropertiesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8022MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example: undocumented non-private properties are reported on their names, while documented and private properties and indexers are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8022-001")]
    [Requirement("RH8022-002")]
    public async Task VerifySpecificationExampleReportsUndocumentedNonPrivateProperties()
    {
        const string source = """
                              /// <summary>
                              /// Settings
                              /// </summary>
                              public class Settings
                              {
                                  public string {|#0:Name|} { get; set; }

                                  /// <remarks>
                                  /// No summary
                                  /// </remarks>
                                  internal int {|#1:Count|} => 0;

                                  public int {|#2:Version|} { get; private set; }

                                  protected static bool {|#3:Enabled|} { get; }

                                  /// <summary>
                                  /// Gets the profile
                                  /// </summary>
                                  public string Profile { get; }

                                  private int Hidden { get; }

                                  public int this[int index] => index;
                              }

                              /// <summary>
                              /// Contract
                              /// </summary>
                              public interface IContract
                              {
                                  int {|#4:Value|} { get; }
                              }

                              /// <summary>
                              /// Extensions
                              /// </summary>
                              public static class Extensions
                              {
                                  /// <summary>
                                  /// Text values
                                  /// </summary>
                                  /// <param name="value">Value</param>
                                  extension(string value)
                                  {
                                      public int {|#5:Words|} => 0;

                                      int Hidden => 0;
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8022NonPrivatePropertiesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8022MessageFormat, 6));
    }

    /// <summary>
    /// Verifies a property whose accessor is more restricted than the property stays in the non-private group, and a private property is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8022-001")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyGroupIsDecidedByPropertyModifiersNotByAccessorModifiers()
    {
        const string source = """
                              /// <summary>Settings</summary>
                              public class Settings
                              {
                                  public int {|#0:Version|} { get; private set; }

                                  internal string {|#1:Name|} { private get; set; }

                                  private int Hidden { get; set; }
                              }
                              """;

        await Verify(source, Diagnostics(RH8022NonPrivatePropertiesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8022MessageFormat, 2));
    }

    /// <summary>
    /// Verifies undocumented interface properties without an access modifier, static abstract ones included, are reported while a private interface property is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8022-001")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyDiagnosticForInterfacePropertiesWithoutAccessModifier()
    {
        const string source = """
                              /// <summary>Contract</summary>
                              /// <typeparam name="TSelf">Self</typeparam>
                              public interface IContract<TSelf> where TSelf : IContract<TSelf>
                              {
                                  int {|#0:Value|} { get; }

                                  static abstract TSelf {|#1:Default|} { get; }

                                  private int Hidden => 0;
                              }
                              """;

        await Verify(source, Diagnostics(RH8022NonPrivatePropertiesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8022MessageFormat, 2));
    }

    /// <summary>
    /// Verifies undocumented auto-implemented, expression-bodied, accessor-bodied, abstract, static, and override properties are all reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8022-001")]
    public async Task VerifyDiagnosticForPropertiesWhateverTheirForm()
    {
        const string source = """
                              /// <summary>Shape</summary>
                              public abstract class Shape
                              {
                                  public int {|#0:Auto|} { get; set; }

                                  public int {|#1:Arrow|} => 0;

                                  public int {|#2:Accessors|}
                                  {
                                      get
                                      {
                                          return 0;
                                      }
                                  }

                                  public abstract int {|#3:Sides|} { get; }

                                  public static int {|#4:Count|} { get; } = 1;
                              }

                              /// <summary>Square</summary>
                              public class Square : Shape
                              {
                                  public override int {|#5:Sides|} => 4;
                              }
                              """;

        await Verify(source, Diagnostics(RH8022NonPrivatePropertiesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8022MessageFormat, 6));
    }

    /// <summary>
    /// Verifies undocumented properties declared in the body of a struct, a record, and a record struct are reported, while positional record properties are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8022-001")]
    [Requirement("SHARED-DOC-001")]
    public async Task VerifyDiagnosticForPropertiesOfStructRecordAndRecordStruct()
    {
        const string source = """
                              /// <summary>Point</summary>
                              public struct Point
                              {
                                  public int {|#0:X|} { get; set; }
                              }

                              /// <summary>Person</summary>
                              /// <param name="Name">Name</param>
                              public record Person(string Name)
                              {
                                  public int {|#1:Age|} { get; init; }
                              }

                              /// <summary>Pair</summary>
                              /// <param name="Left">Left</param>
                              public record struct Pair(int Left)
                              {
                                  public int {|#2:Right|} { get; set; }
                              }
                              """;

        await Verify(source, Diagnostics(RH8022NonPrivatePropertiesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8022MessageFormat, 3));
    }

    /// <summary>
    /// Verifies an explicit property implementation declared in an interface is not reported, while an undocumented interface property is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8022-001")]
    [Requirement("SHARED-DOC-009")]
    public async Task VerifyNoDiagnosticForExplicitPropertyImplementationInInterface()
    {
        const string source = """
                              /// <summary>Contract</summary>
                              public interface IContract
                              {
                                  /// <summary>Gets the value</summary>
                                  int Value { get; }
                              }

                              /// <summary>Derived contract</summary>
                              public interface IDerived : IContract
                              {
                                  int IContract.Value => 0;

                                  int {|#0:Other|} { get; }
                              }
                              """;

        await Verify(source, Diagnostics(RH8022NonPrivatePropertiesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8022MessageFormat));
    }

    /// <summary>
    /// Verifies the diagnostic for an undocumented property with a verbatim identifier covers the whole identifier, the at sign included
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8022-002")]
    public async Task VerifyDiagnosticForPropertyWithVerbatimIdentifierCoversTheAtSign()
    {
        const string source = """
                              /// <summary>Settings</summary>
                              public class Settings
                              {
                                  public int {|#0:@class|} { get; }
                              }
                              """;

        await Verify(source, Diagnostics(RH8022NonPrivatePropertiesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8022MessageFormat));
    }

    /// <summary>
    /// Verifies extension block properties without a modifier or with private are not reported, while an internal one is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8022-001")]
    [Requirement("SHARED-DOC-002")]
    public async Task VerifyGroupOfExtensionBlockProperties()
    {
        const string source = """
                              /// <summary>Extensions</summary>
                              public static class Extensions
                              {
                                  /// <summary>Text values</summary>
                                  /// <param name="value">Value</param>
                                  extension(string value)
                                  {
                                      int Hidden => 0;

                                      private int Explicit => 0;

                                      internal int {|#0:Assembly|} => 0;
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8022NonPrivatePropertiesMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8022MessageFormat));
    }

    #endregion // Tests
}