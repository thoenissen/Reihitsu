using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH7303DoNotPlaceRegionsWithinElementsAnalyzer"/> and <see cref="RH7303DoNotPlaceRegionsWithinElementsCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH7303DoNotPlaceRegionsWithinElementsAnalyzerTests : BatchCodeFixTestsBase<RH7303DoNotPlaceRegionsWithinElementsAnalyzer, RH7303DoNotPlaceRegionsWithinElementsCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that type-level regions do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7303-002")]
    public async Task VerifyNoDiagnosticsForTypeLevelRegions()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Fields
                                    private int field;
                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that regions within elements are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7303-001")]
    [Requirement("RH7303-003")]
    public async Task VerifyRegionsWithinMethodBodiesAreDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        {|#0:#region Helper|}
                                        var value = 1;
                                        {|#1:#endregion|}
                                    }
                                }
                                """;

        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         var value = 1;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH7303DoNotPlaceRegionsWithinElementsAnalyzer.DiagnosticId, AnalyzerResources.RH7303MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that region directives in an accessor list, a lambda body, and an excluded branch of a method body are each reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7303-001")]
    [Requirement("RH7303-003")]
    [Requirement("SHARED-TYPEREGION-005")]
    public async Task RegionDirectivesInsideExecutableCodeAreReported()
    {
        const string testData = """
                                class C
                                {
                                    int P
                                    {
                                        {|RH7303:#region Accessors|}
                                        get { return 1; }
                                        {|RH7303:#endregion|}
                                    }

                                    System.Func<int> F = () =>
                                    {
                                        {|RH7303:#region Body|}
                                        return 1;
                                        {|RH7303:#endregion|}
                                    };

                                    void M()
                                    {
                                #if false
                                        {|RH7303:#region Inactive|}
                                        {|RH7303:#endregion|}
                                #endif
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that region directives in an accessor list and a block are reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7303-001")]
    [Requirement("SHARED-TYPEREGION-005")]
    public async Task RegionDirectivesInAnAccessorListAndABlockAreReported()
    {
        const string testData = """
                                class C
                                {
                                    int P
                                    {
                                        {|RH7303:#region Accessors|}
                                        get { return 1; }
                                        {|RH7303:#endregion|}
                                    }

                                    void M()
                                    {
                                        {|RH7303:#region Work|}
                                        M();
                                        {|RH7303:#endregion|}
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that region directives between members, enum members, namespace declarations, and at the top level of a file are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7303-002")]
    [Requirement("SHARED-TYPEREGION-005")]
    public async Task RegionDirectivesBetweenDeclarationsAreNotReported()
    {
        const string testData = """
                                #region Types
                                namespace N
                                {
                                    #region Classes
                                    class C
                                    {
                                        #region Fields
                                        int _a;
                                        #endregion
                                    }
                                    #endregion

                                    enum E
                                    {
                                        #region Values
                                        A,
                                        #endregion
                                    }
                                }
                                #endregion
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the code fix removes both directive lines of a region between two statements and keeps the enclosed code
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7303-004")]
    public async Task CodeFixRemovesBothDirectiveLinesBetweenStatements()
    {
        const string testData = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = 1;
                                        {|RH7303:#region Work|}
                                        x++;
                                        {|RH7303:#endregion // Work|}
                                        x--;
                                    }
                                }
                                """;

        const string fixedData = """
                                 class C
                                 {
                                     void M()
                                     {
                                         var x = 1;
                                         x++;
                                         x--;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the code fix removes both directive lines with their carriage-return line-feed line breaks and leaves every other line unchanged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7303-004")]
    public async Task CodeFixRemovesBothDirectiveLinesWithCarriageReturnLineFeed()
    {
        const string testData = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = 1;
                                        #region Work
                                        x++;
                                        #endregion // Work
                                        x--;
                                    }
                                }
                                """;

        const string fixedData = """
                                 class C
                                 {
                                     void M()
                                     {
                                         var x = 1;
                                         x++;
                                         x--;
                                     }
                                 }
                                 """;

        var actual = await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(testData));

        Assert.AreEqual(NormalizeToCarriageReturnLineFeed(fixedData), actual);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        {|#0:#region Helper|}
                                        var value = 1;
                                        {|#1:#endregion|}
                                    }
                                }
                                """;

        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         var value = 1;
                                     }
                                 }
                                 """;

        // Both diagnostics belong to the same misplaced #region/#endregion pair, and the fix registered for
        // either one removes both directive lines: the two computed fixes target the exact same span, the
        // strongest form of interference in this batch, and the batch fixer must still converge to removing
        // them once
        return new FixAllScenario(testData, fixedData, Diagnostics(RH7303DoNotPlaceRegionsWithinElementsAnalyzer.DiagnosticId, AnalyzerResources.RH7303MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}