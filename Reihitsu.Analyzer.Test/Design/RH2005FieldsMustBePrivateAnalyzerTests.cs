using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Design;
using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH2005FieldsMustBePrivateAnalyzer"/> and <see cref="RH2005FieldsMustBePrivateCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH2005FieldsMustBePrivateAnalyzerTests : AnalyzerTestsBase<RH2005FieldsMustBePrivateAnalyzer, RH2005FieldsMustBePrivateCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying that public fields in classes and record classes trigger diagnostics and are fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsAndCodeFix()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                public class Sample
                                {
                                    public string {|#0:publicField|};

                                    protected int protectedField;

                                    internal bool internalField;

                                    protected internal int protectedInternalField;

                                    private protected int privateProtectedField;

                                    private string privateField;

                                    string implicitPrivateField;

                                    public static readonly string Shared = string.Empty;

                                    public const int Constant = 1;
                                }

                                public struct SampleStruct
                                {
                                    public int PublicField;
                                }

                                public record SampleRecord
                                {
                                    public int {|#1:recordField|};
                                }

                                public record struct SampleRecordStruct
                                {
                                    public int PublicField;
                                }
                                """;

        const string resultData = """
                                  namespace Reihitsu.Analyzer.Test.Design.Resources;

                                  public class Sample
                                  {
                                      private string publicField;

                                      protected int protectedField;

                                      internal bool internalField;

                                      protected internal int protectedInternalField;

                                      private protected int privateProtectedField;

                                      private string privateField;

                                      string implicitPrivateField;

                                      public static readonly string Shared = string.Empty;

                                      public const int Constant = 1;
                                  }

                                  public struct SampleStruct
                                  {
                                      public int PublicField;
                                  }

                                  public record SampleRecord
                                  {
                                      private int recordField;
                                  }

                                  public record struct SampleRecordStruct
                                  {
                                      public int PublicField;
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH2005FieldsMustBePrivateAnalyzer.DiagnosticId, AnalyzerResources.RH2005MessageFormat, 2));
    }

    /// <summary>
    /// Verifying that non-public fields in classes and record classes do not trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNonPublicFieldsDoNotTriggerDiagnostics()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources;

                                internal class SampleClass
                                {
                                    internal int InternalField;
                                    protected int ProtectedField;
                                    protected internal int ProtectedInternalField;
                                    private protected int PrivateProtectedField;
                                    private int PrivateField;
                                }

                                internal record SampleRecord
                                {
                                    internal int InternalField;
                                    protected int ProtectedField;
                                    protected internal int ProtectedInternalField;
                                    private protected int PrivateProtectedField;
                                    private int PrivateField;
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests
}