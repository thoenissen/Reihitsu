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
public class RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzerTests : BatchCodeFixTestsBase<RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzer, RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying that a multi-line expression-bodied get-only property is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5401-001")]
    [Requirement("RH5401-002")]
    [Requirement("RH5401-003")]
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
    [Requirement("RH5401-001")]
    [Requirement("RH5401-002")]
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
    /// flagged, because the formatter never collapses the wrapped continuation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5401-001")]
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
    [Requirement("RH5401-004")]
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

    /// <summary>
    /// Verifies that a property whose arrow, or whose first expression token, is below its name is reported on the
    /// whole property, while one whose arrow and first expression token are on the line of its name is not reported,
    /// also when the expression continues on later lines
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5401-001")]
    [Requirement("RH5401-002")]
    public async Task VerifyArrowOrExpressionBelowNameIsReportedOnProperty()
    {
        const string testCode = """
                                class C
                                {
                                    {|RH5401:int A
                                        => 1;|}

                                    {|RH5401:int B =>
                                        1;|}

                                    int D => 1
                                             + 2;
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the diagnostic covers the whole property declaration, its attribute lists and modifiers included
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5401-002")]
    public async Task VerifyDiagnosticCoversAttributeListsAndModifiers()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    {|RH5401:[Obsolete]
                                    int A
                                        => 1;|}
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the fix puts the arrow and the expression on the line of the property name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5401-003")]
    public async Task VerifyFixPutsPropertyOnOneLine()
    {
        const string testCode = """
                                class C
                                {
                                    {|RH5401:int A
                                        => 1;|}

                                    {|RH5401:int B =>
                                        1;|}
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     int A => 1;

                                     int B => 1;
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix puts the arrow and the expression on the line of the property name, with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5401-003")]
    public async Task VerifyFixPutsPropertyOnOneLineWithCarriageReturnLineFeed()
    {
        const string testCode = """
                                class C
                                {
                                    {|RH5401:int A
                                        => 1;|}

                                    {|RH5401:int B =>
                                        1;|}
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     int A => 1;

                                     int B => 1;
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testCode), NormalizeToCarriageReturnLineFeed(fixedCode));
    }

    /// <summary>
    /// Verifies that the fix puts an explicit interface implementation of a property on one line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5401-003")]
    public async Task VerifyFixPutsExplicitInterfaceImplementationOnOneLine()
    {
        const string testCode = """
                                interface I
                                {
                                    int P { get; }
                                }

                                class C : I
                                {
                                    {|RH5401:int I.P
                                        => 1;|}
                                }
                                """;
        const string fixedCode = """
                                 interface I
                                 {
                                     int P { get; }
                                 }

                                 class C : I
                                 {
                                     int I.P => 1;
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix puts an explicit interface implementation of a property on one line, with CRLF line
    /// endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5401-003")]
    public async Task VerifyFixPutsExplicitInterfaceImplementationOnOneLineWithCarriageReturnLineFeed()
    {
        const string testCode = """
                                interface I
                                {
                                    int P { get; }
                                }

                                class C : I
                                {
                                    {|RH5401:int I.P
                                        => 1;|}
                                }
                                """;
        const string fixedCode = """
                                 interface I
                                 {
                                     int P { get; }
                                 }

                                 class C : I
                                 {
                                     int I.P => 1;
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testCode), NormalizeToCarriageReturnLineFeed(fixedCode));
    }

    /// <summary>
    /// Verifies that the fix is withheld, while the diagnostic stays, when the expression stays wrapped or a comment
    /// keeps the arrow on a later line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5401-004")]
    public async Task VerifyFixIsWithheldWhenPropertyStaysOnSeveralLines()
    {
        const string testCode = """
                                class C
                                {
                                    {|RH5401:int A =>
                                        1
                                        + 2;|}

                                    {|RH5401:int B // note
                                        => 1;|}
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class RH5401
                                {
                                    {|#0:public int P1
                                            => 1;|}
                                    {|#1:public int P2
                                            => 2;|}
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5401
                                 {
                                     public int P1 => 1;
                                     public int P2 => 2;
                                 }
                                 """;

        // The two properties are adjacent with no blank line between them, so the first fix's replacement span
        // directly abuts the second occurrence's leading trivia
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5401MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}