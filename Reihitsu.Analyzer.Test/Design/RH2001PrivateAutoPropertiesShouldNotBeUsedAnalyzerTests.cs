using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Design;
using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer"/> and <see cref="RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests : AnalyzerTestsBase<RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer, RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider>
{
    #region Tests

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
                                    internal class RH2001
                                    {
                                        private bool {|#0:PrivateAutoProperty|} { get; set; }
                                    }
                                }
                                """;

        const string resultData = """
                                  namespace Reihitsu.Analyzer.Test.Design.Resources
                                  {
                                      internal class RH2001
                                      {
                                          private bool _privateAutoProperty;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2001MessageFormat));
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
                                    internal class RH2001
                                    {
                                        private static string {|#0:CacheKey|} { get; set; } = "cached";
                                    }
                                }
                                """;

        const string resultData = """
                                  namespace Reihitsu.Analyzer.Test.Design.Resources
                                  {
                                      internal class RH2001
                                      {
                                          private static string _cacheKey = "cached";
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2001MessageFormat));
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
                                    internal class RH2001
                                    {
                                        private int {|#0:Count|} { get; } = 1;
                                    }
                                }
                                """;

        const string resultData = """
                                  namespace Reihitsu.Analyzer.Test.Design.Resources
                                  {
                                      internal class RH2001
                                      {
                                          private readonly int _count = 1;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2001MessageFormat));
    }

    /// <summary>
    /// Verifies that attributed private auto-properties are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task AttributedPrivateAutoPropertyIsNotReported()
    {
        const string testData = """
                                using System;

                                namespace Reihitsu.Analyzer.Test.Design.Resources
                                {
                                    internal class RH2001
                                    {
                                        [Obsolete]
                                        private int Value { get; set; }
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the property is still converted to a field when renamed references precede the declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    /// <remarks>
    /// Renamed references before the declaration shift its start offset. The fix must re-locate the declaration
    /// reliably (not via span arithmetic) so the field conversion is not silently dropped in favour of a rename only
    /// </remarks>
    [TestMethod]
    public async Task VerifyPrivateAutoPropertyCodeFixConvertsFieldWhenReferencesPrecedeDeclaration()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources
                                {
                                    internal class RH2001
                                    {
                                        public int Sum()
                                        {
                                            return Value + Value + Value + Value + Value + Value + Value + Value + Value + Value + Value + Value + Value + Value + Value + Value;
                                        }

                                        private int {|#0:Value|} { get; set; }
                                    }
                                }
                                """;

        const string resultData = """
                                  namespace Reihitsu.Analyzer.Test.Design.Resources
                                  {
                                      internal class RH2001
                                      {
                                          public int Sum()
                                          {
                                              return _value + _value + _value + _value + _value + _value + _value + _value + _value + _value + _value + _value + _value + _value + _value + _value;
                                          }

                                          private int _value;
                                      }
                                  }
                                  """;

        await Verify(testData, resultData, Diagnostics(RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH2001MessageFormat));
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
                                    internal class RH2001
                                    {
                                        private int Value { get; init; }
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<PropertyDeclarationSyntax>()
                                                               .Single()
                                                               .Identifier
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that explicit interface implementation auto-properties are not reported, since converting them to
    /// a field would break the interface implementation (CS0535)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ExplicitInterfaceImplementationAutoPropertyIsNotReported()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources
                                {
                                    internal interface IFoo
                                    {
                                        int X { get; set; }
                                    }

                                    internal class RH2001 : IFoo
                                    {
                                        int IFoo.X { get; set; }
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the code fix refuses explicit interface implementation auto-properties even when a diagnostic
    /// is reported for one, independent of the analyzer-side exemption
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NoCodeFixForExplicitInterfaceImplementationAutoProperty()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources
                                {
                                    internal interface IFoo
                                    {
                                        int X { get; set; }
                                    }

                                    internal class RH2001 : IFoo
                                    {
                                        int IFoo.X { get; set; }
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<PropertyDeclarationSyntax>()
                                                               .Single(property => property.ExplicitInterfaceSpecifier != null)
                                                               .Identifier
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that the code fix is not offered when the computed field name is already used by another member of
    /// the containing type, to avoid producing a duplicate declaration (CS0102)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NoCodeFixForPrivateAutoPropertyWithFieldNameCollision()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources
                                {
                                    internal class RH2001
                                    {
                                        private readonly int _value;

                                        private int Value { get; }
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<PropertyDeclarationSyntax>()
                                                               .Single()
                                                               .Identifier
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that the generated field name is culture-invariant, so a property name starting with 'I' does not
    /// turn into a Turkish dotless i when the fix runs under the Turkish culture
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFieldNameConversionIsCultureInvariant()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Design.Resources
                                {
                                    internal class RH2001
                                    {
                                        private int Id { get; set; }
                                    }
                                }
                                """;

        var originalCulture = Thread.CurrentThread.CurrentCulture;
        var originalUiCulture = Thread.CurrentThread.CurrentUICulture;

        Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
        Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("tr-TR");

        try
        {
            var fixedSource = await ApplyCodeFixAsync(testData);

            Assert.Contains("_id", fixedSource);
            Assert.DoesNotContain("_ıd", fixedSource);
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = originalCulture;
            Thread.CurrentThread.CurrentUICulture = originalUiCulture;
        }
    }

    #endregion // Tests
}