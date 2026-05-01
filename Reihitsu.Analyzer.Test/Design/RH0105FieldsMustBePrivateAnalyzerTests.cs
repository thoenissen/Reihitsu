using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH0105FieldsMustBePrivateAnalyzer"/> and <see cref="RH0105FieldsMustBePrivateCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0105FieldsMustBePrivateAnalyzerTests : AnalyzerTestsBase<RH0105FieldsMustBePrivateAnalyzer, RH0105FieldsMustBePrivateCodeFixProvider>
{
    #region Members

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

        await Verify(testData, resultData, Diagnostics(RH0105FieldsMustBePrivateAnalyzer.DiagnosticId, AnalyzerResources.RH0105MessageFormat, 4));
    }

    #endregion // Members
}