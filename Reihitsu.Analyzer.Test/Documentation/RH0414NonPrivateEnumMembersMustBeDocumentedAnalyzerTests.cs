using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0414NonPrivateEnumMembersMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH0414NonPrivateEnumMembersMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH0414NonPrivateEnumMembersMustBeDocumentedAnalyzer>
{
    #region Diagnostic cases

    /// <summary>
    /// Verifies a diagnostic is reported for an enum member without required XML documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForEnumMemberInNonPrivateEnumWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;

                              internal enum Status
                              {
                                  {|#0:Active|},
                              }
                              """;

        await Verify(source, Diagnostics(RH0414NonPrivateEnumMembersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0414MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for an undocumented member in a public enum
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPublicEnumMemberWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>A direction.</summary>
                              public enum Direction
                              {
                                  {|#0:North|},
                              }
                              """;

        await Verify(source, Diagnostics(RH0414NonPrivateEnumMembersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0414MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a member in a protected nested enum without documentation.
    /// Protected is a non-private modifier; its members therefore fall under the non-private group
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForProtectedEnumMemberWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Container.</summary>
                              public class Container
                              {
                                  /// <summary>A nested enum.</summary>
                                  protected enum Priority
                                  {
                                      {|#0:High|},
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH0414NonPrivateEnumMembersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0414MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a member in an enum with no explicit access modifier at namespace scope.
    /// A top-level enum with no modifier defaults to internal, which is non-private
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForEnumMemberInImplicitInternalEnumWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;

                              enum ImplicitInternalEnum
                              {
                                  {|#0:ValueA|},
                              }
                              """;

        await Verify(source, Diagnostics(RH0414NonPrivateEnumMembersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0414MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for an enum member that has a remarks tag but no summary tag.
    /// A remarks-only comment does not satisfy the documentation requirement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForEnumMemberWithRemarksButNoSummary()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>A status enum.</summary>
                              internal enum Status
                              {
                                  /// <remarks>Remarks only, no summary.</remarks>
                                  {|#0:Active|},
                              }
                              """;

        await Verify(source, Diagnostics(RH0414NonPrivateEnumMembersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0414MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for each undocumented member in a public enum.
    /// Every member without documentation is reported independently
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForMultipleUndocumentedMembersInPublicEnum()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>A color enum.</summary>
                              public enum Color
                              {
                                  {|#0:Red|},
                                  {|#1:Green|},
                                  {|#2:Blue|},
                              }
                              """;

        await Verify(source, Diagnostics(RH0414NonPrivateEnumMembersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0414MessageFormat, 3));
    }

    #endregion // Diagnostic cases

    #region No-diagnostic cases

    /// <summary>
    /// Verifies no diagnostic is reported for an enum member with a summary tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForEnumMemberWithSummary()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>A status enum.</summary>
                              internal enum Status
                              {
                                  /// <summary>The active state.</summary>
                                  Active,
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for an enum member with an inheritdoc tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForEnumMemberWithInheritdoc()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>A direction enum.</summary>
                              public enum Direction
                              {
                                  /// <inheritdoc/>
                                  North,
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported when all members in an internal enum are documented
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForAllMembersDocumentedInInternalEnum()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>A severity enum.</summary>
                              internal enum Severity
                              {
                                  /// <summary>Low severity.</summary>
                                  Low,

                                  /// <summary>High severity.</summary>
                                  High,
                              }
                              """;

        await Verify(source);
    }

    #endregion // No-diagnostic cases

    #region Access-boundary cases

    /// <summary>
    /// Verifies no diagnostic is reported by this analyzer for an undocumented member in a private enum.
    /// Members of private enums are covered by RH0415, not by this analyzer.
    /// The declaration is intentionally left undocumented to confirm the routing decision
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForUndocumentedMemberInPrivateEnum()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  private enum PrivateStatus
                                  {
                                      Active,
                                  }
                              }
                              """;

        await Verify(source);
    }

    #endregion // Access-boundary cases
}