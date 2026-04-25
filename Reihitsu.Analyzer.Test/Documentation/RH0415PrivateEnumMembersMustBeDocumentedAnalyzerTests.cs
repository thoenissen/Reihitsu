using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH0415PrivateEnumMembersMustBeDocumentedAnalyzer"/>.
/// </summary>
[TestClass]
public class RH0415PrivateEnumMembersMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH0415PrivateEnumMembersMustBeDocumentedAnalyzer>
{
    #region Diagnostic cases

    /// <summary>
    /// Verifies a diagnostic is reported for an enum member in a private enum without required XML documentation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForEnumMemberInPrivateEnumWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;

                              internal class Container
                              {
                                  private enum Status
                                  {
                                      {|#0:Active|},
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH0415PrivateEnumMembersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0415MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a private enum member that has only a remarks tag but no summary.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForEnumMemberInPrivateEnumWithRemarksButNoSummary()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  private enum Priority
                                  {
                                      /// <remarks>Remarks only, no summary.</remarks>
                                      {|#0:Low|},
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH0415PrivateEnumMembersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0415MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for every undocumented member in a private enum.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForMultipleUndocumentedMembersInPrivateEnum()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  private enum State
                                  {
                                      {|#0:On|},
                                      {|#1:Off|},
                                      {|#2:Unknown|},
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH0415PrivateEnumMembersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0415MessageFormat, 3));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a member in a private enum that is itself nested inside a private class.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForEnumMemberInDeeplyNestedPrivateEnum()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Outer.</summary>
                              internal class Outer
                              {
                                  /// <summary>Inner.</summary>
                                  private class Inner
                                  {
                                      private enum DeepEnum
                                      {
                                          {|#0:ValueA|},
                                      }
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH0415PrivateEnumMembersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH0415MessageFormat));
    }

    #endregion // Diagnostic cases

    #region No-diagnostic cases

    /// <summary>
    /// Verifies no diagnostic is reported for a private enum member with a summary tag.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForEnumMemberWithSummaryInPrivateEnum()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  private enum Status
                                  {
                                      /// <summary>The active state.</summary>
                                      Active,
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a private enum member with an inheritdoc tag.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForEnumMemberWithInheritdocInPrivateEnum()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  private enum Status
                                  {
                                      /// <inheritdoc/>
                                      Active,
                                  }
                              }
                              """;

        await Verify(source);
    }

    #endregion // No-diagnostic cases

    #region Access-boundary cases

    /// <summary>
    /// Verifies no diagnostic is reported by this analyzer for an undocumented member in an explicit internal enum.
    /// Members of non-private enums are covered by RH0414, not by this analyzer.
    /// The declaration is intentionally left undocumented to confirm the routing decision.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForUndocumentedMemberInInternalEnum()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Container.</summary>
                              internal class Container
                              {
                                  internal enum InternalStatus
                                  {
                                      Active,
                                  }
                              }
                              """;

        await Verify(source);
    }

    #endregion // Access-boundary cases
}