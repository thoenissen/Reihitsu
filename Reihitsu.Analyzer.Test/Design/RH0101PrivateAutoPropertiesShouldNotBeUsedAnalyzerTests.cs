using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH0101PrivateAutoPropertiesShouldNotBeUsedAnalyzer"/> and <see cref="RH0101PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0101PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests : AnalyzerTestsBase<RH0101PrivateAutoPropertiesShouldNotBeUsedAnalyzer, RH0101PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider>
{
    #region Members

    /// <summary>
    /// Verifies that a private auto-property is reported and converted to a backing field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyPrivateAutoPropertyDiagnostic()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources
                                {
                                    internal class RH0101
                                    {
                                        private bool {|#0:PrivateAutoProperty|} { get; set; }
                                    }
                                }
                                """;

        const string resultData = """
                                  namespace Reihitsu.Analyzer.Test.Design.Resources
                                  {
                                      internal class RH0101
                                      {
                                          private bool _privateAutoProperty;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH0101PrivateAutoPropertiesShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH0101MessageFormat));
    }

    /// <summary>
    /// Verifies that modifiers and initializers are preserved for supported property shapes
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyPrivateStaticAutoPropertyCodeFixPreservesModifiersAndInitializer()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources
                                {
                                    internal class RH0101
                                    {
                                        private static string {|#0:CacheKey|} { get; set; } = "cached";
                                    }
                                }
                                """;

        const string resultData = """
                                  namespace Reihitsu.Analyzer.Test.Design.Resources
                                  {
                                      internal class RH0101
                                      {
                                          private static string _cacheKey = "cached";
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH0101PrivateAutoPropertiesShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH0101MessageFormat));
    }

    /// <summary>
    /// Verifies that get-only auto-properties are converted to readonly fields
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyPrivateGetOnlyAutoPropertyCodeFixCreatesReadonlyField()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources
                                {
                                    internal class RH0101
                                    {
                                        private int {|#0:Count|} { get; } = 1;
                                    }
                                }
                                """;

        const string resultData = """
                                  namespace Reihitsu.Analyzer.Test.Design.Resources
                                  {
                                      internal class RH0101
                                      {
                                          private readonly int _count = 1;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH0101PrivateAutoPropertiesShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH0101MessageFormat));
    }

    /// <summary>
    /// Verifies that attributed private auto-properties do not receive an unsafe code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NoCodeFixForAttributedPrivateAutoProperty()
    {
        const string testData = """
                                using System;

                                namespace Reihitsu.Analyzer.Test.Design.Resources
                                {
                                    internal class RH0101
                                    {
                                        [Obsolete]
                                        private int Value { get; set; }
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH0101PrivateAutoPropertiesShouldNotBeUsedAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<PropertyDeclarationSyntax>()
                                                               .Single()
                                                               .Identifier
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that init-only auto-properties do not receive an unsafe code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NoCodeFixForInitOnlyPrivateAutoProperty()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources
                                {
                                    internal class RH0101
                                    {
                                        private int Value { get; init; }
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH0101PrivateAutoPropertiesShouldNotBeUsedAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<PropertyDeclarationSyntax>()
                                                               .Single()
                                                               .Identifier
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    #endregion // Members
}