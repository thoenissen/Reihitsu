using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5531AccessorAttributeListsMustFollowShapeRulesAnalyzer"/> and <see cref="RH5531AccessorAttributeListsMustFollowShapeRulesCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5531AccessorAttributeListsMustFollowShapeRulesAnalyzerTests : AnalyzerTestsBase<RH5531AccessorAttributeListsMustFollowShapeRulesAnalyzer, RH5531AccessorAttributeListsMustFollowShapeRulesCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that multiline accessor violations are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticAndCodeFixForMultilineAccessorPolicyViolation()
    {
        const string testData = """
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                internal class Example
                                {
                                    internal int Value
                                    {
                                        {|#0:[First, Second]|}
                                        get;
                                        set;
                                    }
                                }
                                """;
        const string fixedData = """
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 sealed class SecondAttribute : System.Attribute
                                 {
                                 }
                                 internal class Example
                                 {
                                     internal int Value
                                     {
                                         [First]
                                         [Second]
                                         get;
                                         set;
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5531AccessorAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5531MessageFormat));
    }

    /// <summary>
    /// Verifies that compliant multiline accessor code is not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCompliantMultilineAccessorCode()
    {
        const string testData = """
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                internal class Example
                                {
                                    internal int Value
                                    {
                                        [First]
                                        [Second]
                                        get;
                                        set;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that single-line accessors prefer merged attribute lists
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticAndCodeFixForSingleLineAccessorPolicyViolation()
    {
        const string testData = """
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                internal class Example
                                {
                                    internal int Value { [First]{|#0:[Second]|} get; set; }
                                }
                                """;
        const string fixedData = """
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 sealed class SecondAttribute : System.Attribute
                                 {
                                 }
                                 internal class Example
                                 {
                                     internal int Value { [First, Second] get; set; }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5531AccessorAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5531MessageFormat));
    }

    /// <summary>
    /// Verifies that compliant single-line accessor code is not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCompliantSingleLineAccessorCode()
    {
        const string testData = """
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                internal class Example
                                {
                                    internal int Value { [First, Second] get; set; }
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
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                internal class Example
                                {
                                    internal int Value
                                    {
                                        {|#0:[First, /* keep */ Second]|}
                                        get;
                                        set;
                                    }
                                }
                                """;
        const string codeFixData = """
                                   sealed class FirstAttribute : System.Attribute
                                   {
                                   }
                                   sealed class SecondAttribute : System.Attribute
                                   {
                                   }
                                   internal class Example
                                   {
                                       internal int Value
                                       {
                                           [First, /* keep */ Second]
                                           get;
                                           set;
                                       }
                                   }
                                   """;

        await Verify(testData,
                     Diagnostics(RH5531AccessorAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5531MessageFormat));

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5531AccessorAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<AttributeListSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    #endregion // Tests
}