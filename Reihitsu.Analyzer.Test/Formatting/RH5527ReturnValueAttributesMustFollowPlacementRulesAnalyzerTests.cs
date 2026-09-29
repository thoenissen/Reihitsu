using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5527ReturnValueAttributesMustFollowPlacementRulesAnalyzer"/> and <see cref="RH5527ReturnValueAttributesMustFollowPlacementRulesCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5527ReturnValueAttributesMustFollowPlacementRulesAnalyzerTests : BatchCodeFixTestsBase<RH5527ReturnValueAttributesMustFollowPlacementRulesAnalyzer, RH5527ReturnValueAttributesMustFollowPlacementRulesCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that policy violations are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticAndCodeFixForPolicyViolation()
    {
        const string testData = """
                                internal class Example
                                {
                                    {|#0:[return: First]|} internal int M() => 0;
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     [return: First]
                                     internal int M() => 0;
                                 }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 sealed class SecondAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5527ReturnValueAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5527MessageFormat));
    }

    /// <summary>
    /// Verifies that splitting the attribute list inside an object initializer lands the member on the attribute
    /// list's own column rather than the enclosing brace-scope nesting level, which understates an anchor-derived
    /// column by not accounting for the initializer's own alignment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixAlignsToAttributeListColumnInsideObjectInitializer()
    {
        const string testData = """
                                internal class Example
                                {
                                    private readonly Config _config = new Config
                                    {
                                        Handler = () =>
                                        {
                                                                          {|#0:[return: First]|} int Local() => 0;
                                        }
                                    };
                                }
                                internal sealed class Config
                                {
                                    public System.Action Handler { get; set; }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     private readonly Config _config = new Config
                                     {
                                         Handler = () =>
                                         {
                                                                           [return: First]
                                                                           int Local() => 0;
                                         }
                                     };
                                 }
                                 internal sealed class Config
                                 {
                                     public System.Action Handler { get; set; }
                                 }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5527ReturnValueAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5527MessageFormat));
    }

    /// <summary>
    /// Verifies that compliant code is not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCompliantCode()
    {
        const string testData = """
                                internal class Example
                                {
                                    [return: First]
                                    internal int M() => 0;
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that commented violations are still reported without offering an unsafe code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticWithoutCodeFixWhenCommentsArePresent()
    {
        const string testData = """
                                internal class Example
                                {
                                    {|#0:[return: First /* keep */]|} internal int M() => 0;
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;
        const string codeFixData = """
                                   internal class Example
                                   {
                                       [return: First /* keep */] internal int M() => 0;
                                   }
                                   sealed class FirstAttribute : System.Attribute
                                   {
                                   }
                                   sealed class SecondAttribute : System.Attribute
                                   {
                                   }
                                   """;

        await Verify(testData,
                     Diagnostics(RH5527ReturnValueAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5527MessageFormat));

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5527ReturnValueAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<AttributeListSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that an explicit return specifier on a single-line property accessor is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForReturnSpecifierOnSingleLinePropertyAccessor()
    {
        const string testData = """
                                internal class Example
                                {
                                    public int Value { [return: First] get; set; }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an explicit return specifier on a single-line property accessor is not reported (CRLF)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForReturnSpecifierOnSingleLinePropertyAccessorCrLf()
    {
        const string testData = """
                                internal class Example
                                {
                                    public int Value { [return: First] get; set; }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData));
    }

    /// <summary>
    /// Verifies that an explicit return specifier on an accessor of a single-line interface property is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForReturnSpecifierOnSingleLineInterfacePropertyAccessor()
    {
        const string testData = """
                                internal interface IExample
                                {
                                    int Value { [return: First] get; }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an explicit return specifier on an accessor of a single-line property with an initializer is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForReturnSpecifierOnSingleLinePropertyAccessorWithInitializer()
    {
        const string testData = """
                                internal class Example
                                {
                                    public int Value { [return: First] get; set; } = 1;
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a comment after the accessor list does not remove the single-line exemption, because the formatter keeps such a property inline
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForReturnSpecifierOnSingleLinePropertyAccessorWithTrailingComment()
    {
        const string testData = """
                                internal class Example
                                {
                                    public int Value { [return: First] get; set; } // Comment
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the property's own attribute list on the preceding line does not remove the single-line exemption
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForReturnSpecifierOnSingleLinePropertyAccessorBelowPropertyAttributeList()
    {
        const string testData = """
                                internal class Example
                                {
                                    [Second]
                                    public int Value { [return: First] get; set; }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an accessor list with an expression-bodied accessor is still reported, because the formatter expands it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForReturnSpecifierOnSingleLinePropertyAccessorWithExpressionBody()
    {
        const string testData = """
                                internal class Example
                                {
                                    public int Value { {|#0:[return: First]|} get => 1; }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     public int Value { [return: First]
                                         get => 1; }
                                 }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 sealed class SecondAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5527ReturnValueAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5527MessageFormat));
    }

    /// <summary>
    /// Verifies that an accessor list containing a comment is still reported, because the formatter expands it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForReturnSpecifierOnSingleLinePropertyAccessorWithCommentInsideAccessorList()
    {
        const string testData = """
                                internal class Example
                                {
                                    public int Value { {|#0:[return: First]|} get; /* Comment */ set; }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     public int Value { [return: First]
                                         get; /* Comment */ set; }
                                 }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 sealed class SecondAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5527ReturnValueAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5527MessageFormat));
    }

    /// <summary>
    /// Verifies that an explicit return specifier on an accessor of a multi-line property is still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForReturnSpecifierOnMultiLinePropertyAccessor()
    {
        const string testData = """
                                internal class Example
                                {
                                    public int Value
                                    {
                                        {|#0:[return: First]|} get;
                                        set;
                                    }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     public int Value
                                     {
                                         [return: First]
                                         get;
                                         set;
                                     }
                                 }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 sealed class SecondAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5527ReturnValueAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5527MessageFormat));
    }

    /// <summary>
    /// Verifies that no code fix is offered for an explicit return specifier on an accessor of a single-line auto-property, which the formatter keeps inline
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoCodeFixForReturnSpecifierOnSingleLinePropertyAccessor()
    {
        const string codeFixData = """
                                   internal class Example
                                   {
                                       public int Value { [return: First] get; set; }
                                   }
                                   sealed class FirstAttribute : System.Attribute
                                   {
                                   }
                                   sealed class SecondAttribute : System.Attribute
                                   {
                                   }
                                   """;

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5527ReturnValueAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<AttributeListSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class Example
                                {
                                    {|#0:[return: First]|} {|#1:[return: Second]|} internal int M() => 0;
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;

        const string fixedCode = """
                                 internal class Example
                                 {
                                     [return: First]
                                     [return: Second]
                                     internal int M() => 0;
                                 }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 sealed class SecondAttribute : System.Attribute
                                 {
                                 }
                                 """;

        // The two attribute lists already share one line: under WellKnownFixAllProviders.BatchFixer both fixes
        // are computed against the same original document. The second list's own leading trivia is empty there
        // (the intervening whitespace belongs to the first list's close bracket's trailing trivia instead),
        // which used to make its derived indentation zero-width
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5527ReturnValueAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5527MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}