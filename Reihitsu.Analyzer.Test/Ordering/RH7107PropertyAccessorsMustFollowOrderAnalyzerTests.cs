using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Ordering;

/// <summary>
/// Test methods for <see cref="RH7107PropertyAccessorsMustFollowOrderAnalyzer"/> and <see cref="RH7107PropertyAccessorsMustFollowOrderCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH7107PropertyAccessorsMustFollowOrderAnalyzerTests : AnalyzerTestsBase<RH7107PropertyAccessorsMustFollowOrderAnalyzer, RH7107PropertyAccessorsMustFollowOrderCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying property accessors are reported and fixed when get appears after set
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task PropertyAccessorsAreReportedAndFixedWhenGetAppearsAfterSet()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    public string Name
                                    {
                                        set
                                        {
                                        }

                                        {|#0:get|}
                                        {
                                            return string.Empty;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class TestClass
                                 {
                                     public string Name
                                     {
                                         get
                                         {
                                             return string.Empty;
                                         }
                                         set
                                         {
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7107PropertyAccessorsMustFollowOrderAnalyzer.DiagnosticId, AnalyzerResources.RH7107MessageFormat));
    }

    /// <summary>
    /// Verifying no code fix is offered when a preprocessor directive sits in the affected leading trivia,
    /// since moving the accessor would split the conditional-compilation pair
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NoCodeFixWhenDirectivesAreInAccessorLeadingTrivia()
    {
        const string testCode = """
                                public class TestClass
                                {
                                    public int X
                                    {
                                #if DEBUG
                                        set
                                        {
                                        }
                                #endif
                                        get
                                        {
                                            return 0;
                                        }
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testCode,
                                                   RH7107PropertyAccessorsMustFollowOrderAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<AccessorDeclarationSyntax>()
                                                               .Single(accessor => accessor.Kind() == SyntaxKind.GetAccessorDeclaration)
                                                               .Keyword
                                                               .GetLocation(),
                                                   "DEBUG");

        Assert.IsEmpty(actions);
    }

    #endregion // Tests
}