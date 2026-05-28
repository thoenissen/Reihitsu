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
    /// Verifying that exposed fields trigger diagnostics and are fixed
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

                                    protected int {|#1:protectedField|};

                                    internal bool {|#2:internalField|};

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
                                    public int {|#3:recordField|};
                                }
                                """;

        const string resultData = """
                                  namespace Reihitsu.Analyzer.Test.Design.Resources;

                                  public class Sample
                                  {
                                      private string publicField;

                                      private int protectedField;

                                      private bool internalField;

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
                                  """;

        await Verify(testData, resultData, Diagnostics(RH2005FieldsMustBePrivateAnalyzer.DiagnosticId, AnalyzerResources.RH2005MessageFormat, 4));
    }

    #endregion // Tests
}