using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzer"/>
/// </summary>
[TestClass]
public class RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzerTests : AnalyzerTestsBase<RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzer, RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying that a multi-line expression-bodied get-only property is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultiLineExpressionBodiedPropertiesAreDetectedAndFixed()
    {
        const string testData = """
                                internal class RH5401
                                {
                                    {|#0:public int P2
                                            => 2;|}
                                }
                                """;
        const string fixedData = """
                                 internal class RH5401
                                 {
                                     public int P2 => 2;
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5401MessageFormat));
    }

    /// <summary>
    /// Verifying that multi-line expression-bodied get-only properties are detected
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultiLineExpressionBodiedPropertiesAreDetected()
    {
        const string testData = """
                                internal class RH5401
                                {
                                    public int P1 => 0;

                                    {|#0:public int P2
                                            => 2;|}

                                    public int P3 => 0
                                                     + 3;

                                    public int P4 { get; set; }
                                    public int P5 { get; }
                                    public int P6 { get; } = 6;

                                    public int P7
                                    {
                                        get => 7;
                                    }

                                    public int P8
                                    {
                                        get => P4;
                                        set => P4 = value;
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5401MessageFormat));
    }

    /// <summary>
    /// Verifying that an expression body which starts on the signature line but wraps onto later lines is not
    /// flagged, because the formatter never collapses the wrapped continuation (issue #247)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyWrappedExpressionStartingOnSignatureLineIsNotFlagged()
    {
        const string testData = """
                                internal class RH5401
                                {
                                    private int _value;

                                    public string Result => _value switch
                                    {
                                        1 => "a",
                                        _ => "b"
                                    };
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies no code fix is offered when the formatter cannot collapse the property to a single line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoCodeFixWhenPropertyCannotBeCollapsed()
    {
        const string testData = """
                                internal class RH5401
                                {
                                    public int[] Values =>
                                    [
                                        1,
                                        2,
                                    ];
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<PropertyDeclarationSyntax>()
                                                               .Single()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    #endregion // Tests
}