using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer"/>
/// </summary>
[TestClass]
public class RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzerTests : AnalyzerTestsBase<RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer, RH5408SimpleAutoPropertiesShouldBeSingleLinedCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying that a multi-line get-only auto-property is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultiLineGetOnlyAutoPropertyIsDetectedAndFixed()
    {
        const string testData = """
                                internal class RH5408
                                {
                                    {|#0:public int Value
                                    {
                                        get;
                                    }|}
                                }
                                """;
        const string fixedData = """
                                 internal class RH5408
                                 {
                                     public int Value { get; }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5408MessageFormat));
    }

    /// <summary>
    /// Verifying that a multi-line get/set auto-property is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultiLineGetSetAutoPropertyIsDetectedAndFixed()
    {
        const string testData = """
                                internal class RH5408
                                {
                                    {|#0:public int Value
                                    {
                                        get;
                                        set;
                                    }|}
                                }
                                """;
        const string fixedData = """
                                 internal class RH5408
                                 {
                                     public int Value { get; set; }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5408MessageFormat));
    }

    /// <summary>
    /// Verifying that multiple multi-line auto-properties are detected
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultipleMultiLineAutoPropertiesAreDetected()
    {
        const string testData = """
                                internal class RH5408
                                {
                                    public int A { get; set; }

                                    {|#0:public int B
                                    {
                                        get;
                                        set;
                                    }|}

                                    {|#1:public string C
                                    {
                                        get;
                                    }|}
                                }
                                """;

        await Verify(testData, Diagnostics(RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5408MessageFormat, 2));
    }

    /// <summary>
    /// Verifying that single-line auto-properties are not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySingleLineAutoPropertyIsNotFlagged()
    {
        const string testData = """
                                internal class RH5408
                                {
                                    public int Value { get; set; }
                                    public int ReadOnly { get; }
                                    public int InitOnly { get; init; }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that a multi-line auto-property with property attributes is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyPropertyAttributedAutoPropertyIsDetectedAndFixed()
    {
        const string testData = """
                                sealed class TestAttribute : System.Attribute
                                {
                                }

                                internal class RH5408
                                {
                                    [Test]
                                    {|#0:public int Value
                                    {
                                        get;
                                        set;
                                    }|}
                                }
                                """;
        const string fixedData = """
                                 sealed class TestAttribute : System.Attribute
                                 {
                                 }

                                 internal class RH5408
                                 {
                                     [Test]
                                     public int Value { get; set; }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5408MessageFormat));
    }

    /// <summary>
    /// Verifying that a multi-line auto-property with accessor attributes is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyAccessorAttributedAutoPropertyIsDetectedAndFixed()
    {
        const string testData = """
                                sealed class TestAttribute : System.Attribute
                                {
                                }

                                internal class RH5408
                                {
                                    {|#0:public int Value
                                    {
                                        [Test]
                                        get;
                                        [Test]
                                        set;
                                    }|}
                                }
                                """;
        const string fixedData = """
                                 sealed class TestAttribute : System.Attribute
                                 {
                                 }

                                 internal class RH5408
                                 {
                                     public int Value { [Test] get; [Test] set; }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5408MessageFormat));
    }

    /// <summary>
    /// Verifying that long signatures that remain multi-line are not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyWrappedSignatureAutoPropertyIsNotFlagged()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class RH5408
                                {
                                    public Dictionary<string,
                                                      string> Value
                                    {
                                        get;
                                        set;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that expression-bodied properties are not flagged (covered by RH5401)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyExpressionBodiedPropertyIsNotFlagged()
    {
        const string testData = """
                                internal class RH5408
                                {
                                    public int Value => 42;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that properties with accessor bodies are not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyPropertyWithAccessorBodyIsNotFlagged()
    {
        const string testData = """
                                internal class RH5408
                                {
                                    private int _value;

                                    public int Value
                                    {
                                        get
                                        {
                                            return _value;
                                        }
                                        set
                                        {
                                            _value = value;
                                        }
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that a multi-line get/init auto-property is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultiLineGetInitAutoPropertyIsDetectedAndFixed()
    {
        const string testData = """
                                internal class RH5408
                                {
                                    {|#0:public int Value
                                    {
                                        get;
                                        init;
                                    }|}
                                }
                                """;
        const string fixedData = """
                                 internal class RH5408
                                 {
                                     public int Value { get; init; }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5408MessageFormat));
    }

    /// <summary>
    /// Verifying that multi-line auto-properties with multi-line initializers are not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyAutoPropertyWithMultiLineInitializerIsNotFlagged()
    {
        const string testData = """
                                internal class RH5408
                                {
                                    public string Value
                                    {
                                        get;
                                        set;
                                    } =
                                        string.Concat("a",
                                                      "b");
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that commented auto-properties are reported but do not offer an unsafe code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentedAutoPropertyIsReportedWithoutCodeFix()
    {
        const string testData = """
                                internal class RH5408
                                {
                                    {|#0:public int Value
                                    {
                                        // Comment
                                        get;
                                    }|}
                                }
                                """;
        const string codeFixData = """
                                   internal class RH5408
                                   {
                                       public int Value
                                       {
                                           // Comment
                                           get;
                                       }
                                   }
                                   """;

        await Verify(testData,
                     Diagnostics(RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5408MessageFormat));

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<PropertyDeclarationSyntax>()
                                                               .Single()
                                                               .Identifier
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    #endregion // Tests
}