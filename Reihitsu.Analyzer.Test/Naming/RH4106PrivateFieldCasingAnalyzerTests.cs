using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Naming;
using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4106PrivateFieldCasingAnalyzer"/> and <see cref="RH4106PrivateFieldCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH4106PrivateFieldCasingAnalyzerTests : BatchCodeFixTestsBase<RH4106PrivateFieldCasingAnalyzer, RH4106PrivateFieldCasingCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported for private fields that do not use _camelCase and that references are renamed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForPrivateFieldAndReferenceAreFixed()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceCache
                                    {
                                        private int {|#0:cacheCount|};

                                        public int GetCount()
                                        {
                                            return cacheCount;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class ResourceCache
                                     {
                                         private int _cacheCount;

                                         public int GetCount()
                                         {
                                             return _cacheCount;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4106PrivateFieldCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4106MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for a private field that already uses _camelCase
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForUnderlineCamelCasePrivateField()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceCache
                                    {
                                        private int _cacheCount;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies diagnostics are reported for private readonly fields that do not use _camelCase
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForPrivateReadonlyFieldWithoutUnderlinePrefix()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceCache
                                    {
                                        private readonly int {|#0:cacheLimit|} = 10;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class ResourceCache
                                     {
                                         private readonly int _cacheLimit = 10;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4106PrivateFieldCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4106MessageFormat));
    }

    /// <summary>
    /// Verifies diagnostics are reported for private static readonly fields that do not use _camelCase
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForPrivateStaticReadonlyFieldWithoutUnderlinePrefix()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceCache
                                    {
                                        private static readonly int {|#0:cacheLimit|} = 10;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class ResourceCache
                                     {
                                         private static readonly int _cacheLimit = 10;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4106PrivateFieldCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4106MessageFormat));
    }

    /// <summary>
    /// Verifies private protected fields are not claimed by the private field rule because they are handled by the protected field rule (RH4107)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForPrivateProtectedField()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceCache
                                    {
                                        private protected int CacheCount;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no code fix is offered for a letterless private field whose conversion cannot produce a valid identifier
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NoCodeFixForLetterlessPrivateField()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceCache
                                    {
                                        private int __;
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testCode,
                                                   RH4106PrivateFieldCasingAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<VariableDeclaratorSyntax>()
                                                               .Single(declarator => declarator.Identifier.ValueText == "__")
                                                               .Identifier
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that a field without an accessibility modifier is treated as effectively private
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyImplicitlyPrivateFieldIsDetectedAndFixed()
    {
        const string testCode = """
                                internal class ResourceCache
                                {
                                    int {|#0:cacheCount|};
                                }
                                """;

        const string fixedCode = """
                                 internal class ResourceCache
                                 {
                                     int _cacheCount;
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4106PrivateFieldCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4106MessageFormat));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class TestClass
                                    {
                                        private int {|#0:Value|};

                                        private int {|#1:value|};

                                        private int {|#2:Count|};

                                        public int Sum => Value + value + Count;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class TestClass
                                     {
                                         private int _value;

                                         private int {|#1:value|};

                                         private int _count;

                                         public int Sum => _value + value + _count;
                                     }
                                 }
                                 """;

        const string batchFixedCode = """
                                      namespace Reihitsu.Analyzer.Test.Naming.Resources
                                      {
                                          public class TestClass
                                          {
                                              private int {|#0:Value|};

                                              private int {|#1:value|};

                                              private int _count;

                                              public int Sum => Value + value + _count;
                                          }
                                      }
                                      """;

        // "Value" and "value" both convert to "_value" inside the same type, so they form a duplicate-target
        // group that Fix All skips entirely, while the unrelated third field is still corrected
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH4106PrivateFieldCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4106MessageFormat, 3),
                                  config =>
                                  {
                                      // The two colliding fields are the one shape where the two paths differ: applying the fixes one
                                      // at a time corrects the first and then refuses the second, while Fix All drops the whole
                                      // duplicate-target group and corrects neither
                                      config.BatchFixedCode = batchFixedCode;

                                      config.FixedState.ExpectedDiagnostics.Add(Diagnostic(RH4106PrivateFieldCasingAnalyzer.DiagnosticId).WithLocation(1, DiagnosticLocationOptions.InterpretAsMarkupKey).WithMessage(AnalyzerResources.RH4106MessageFormat));

                                      config.BatchFixedState.ExpectedDiagnostics.Add(Diagnostic(RH4106PrivateFieldCasingAnalyzer.DiagnosticId).WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey).WithMessage(AnalyzerResources.RH4106MessageFormat));
                                      config.BatchFixedState.ExpectedDiagnostics.Add(Diagnostic(RH4106PrivateFieldCasingAnalyzer.DiagnosticId).WithLocation(1, DiagnosticLocationOptions.InterpretAsMarkupKey).WithMessage(AnalyzerResources.RH4106MessageFormat));

                                      // The two skipped candidates keep the batch loop running for a second, no-op pass
                                      config.NumberOfFixAllIterations = -2;
                                  });
    }

    #endregion // BatchCodeFixTestsBase
}