using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0412NonPrivateEnumsMustBeDocumentedAnalyzer"/>.
/// </summary>
[TestClass]
public class RH0412NonPrivateEnumsMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH0412NonPrivateEnumsMustBeDocumentedAnalyzer>
{
    #region Diagnostic cases

    /// <summary>
    /// Verifies a diagnostic is reported for an internal enum without a documentation comment.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForEnumWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              internal enum {|#0:Status|}
                              {
                                  Active
                              }
                              """;

        await Verify(source, Diagnostics(RH0412NonPrivateEnumsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0412MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a public enum without a documentation comment.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPublicEnumWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              public enum {|#0:Direction|}
                              {
                                  North
                              }
                              """;

        await Verify(source, Diagnostics(RH0412NonPrivateEnumsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0412MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for an internal enum with only a remarks tag and no summary.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForEnumWithRemarksButNoSummary()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <remarks>Missing summary.</remarks>
                              internal enum {|#0:Priority|}
                              {
                                  Low
                              }
                              """;

        await Verify(source, Diagnostics(RH0412NonPrivateEnumsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0412MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a top-level enum with no explicit access modifier and no documentation.
    /// An enum declared at namespace scope without a modifier defaults to internal accessibility.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForTopLevelEnumWithNoModifierAndNoDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              enum {|#0:ImplicitInternalEnum|}
                              {
                                  ValueA
                              }
                              """;

        await Verify(source, Diagnostics(RH0412NonPrivateEnumsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0412MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a nested protected enum without documentation.
    /// Protected is a non-private modifier and therefore falls under the non-private accessibility group.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForNestedProtectedEnumWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Container.</summary>
                              public class Container
                              {
                                  protected enum {|#0:ProtectedStatus|}
                                  {
                                      On
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH0412NonPrivateEnumsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0412MessageFormat));
    }

    #endregion // Diagnostic cases

    #region No-diagnostic cases

    /// <summary>
    /// Verifies no diagnostic is reported for an internal enum with a summary tag.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForInternalEnumWithSummary()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>A documented internal enum.</summary>
                              internal enum DocumentedStatus
                              {
                                  Active
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a public enum documented with inheritdoc.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForPublicEnumWithInheritdoc()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <inheritdoc/>
                              public enum InheritedDirection
                              {
                                  North
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for an undocumented pure private nested enum.
    /// Pure private declarations are covered by RH0413, not by this analyzer.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForPurePrivateNestedEnum()
    {
        const string source = """
                              namespace TestNamespace;
                              
                               /// <summary>Container.</summary>
                               internal class Container
                               {
                                   private enum UndocumentedPrivateEnum
                                   {
                                       ValueA
                                   }
                               }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a nested internal enum with a summary tag.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForNestedInternalEnumWithSummary()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  /// <summary>A documented nested internal enum.</summary>
                                  internal enum DocumentedNestedEnum
                                  {
                                      ValueA
                                  }
                              }
                              """;

        await Verify(source);
    }

    #endregion // No-diagnostic cases
}