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
    /// Verifying that an auto-property carrying a comment inside its accessor list is not flagged, because the
    /// formatter bails out on accessor-list comments and never collapses the property (issue #247)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentedAccessorListAutoPropertyIsNotFlagged()
    {
        const string testData = """
                                internal class RH5408
                                {
                                    public int Value
                                    {
                                        // Comment
                                        get;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that an auto-property carrying a comment between its accessors is not flagged (issue #247)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBetweenAccessorsAutoPropertyIsNotFlagged()
    {
        const string testData = """
                                internal class RH5408
                                {
                                    public int Value
                                    {
                                        get;
                                        // Comment
                                        set;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that an auto-property carrying a comment in the gap between the signature and the accessor list
    /// is not flagged, because the formatter refuses to join the accessor brace across that comment (issue #444)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentInSignatureGapAutoPropertyIsNotFlagged()
    {
        const string testData = """
                                internal class RH5408
                                {
                                    public int Value // note
                                    {
                                        get;
                                        set;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that an auto-property carrying a preprocessor directive in the gap between the signature and the
    /// accessor list is not flagged, because the formatter refuses to join the accessor brace across that directive
    /// (issue #444)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDirectiveInSignatureGapAutoPropertyIsNotFlagged()
    {
        const string testData = """
                                internal class RH5408
                                {
                                    public int Value
                                #if FEATURE
                                #endif
                                    {
                                        get;
                                        set;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that no code fix action is registered for an auto-property carrying a comment in the gap between
    /// the signature and the accessor list, so the code fix does not offer a no-op action (issue #444)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentInSignatureGapAutoPropertyIsNotOfferedACodeFix()
    {
        const string codeFixData = """
                                   internal class RH5408
                                   {
                                       public int Value // note
                                       {
                                           get;
                                           set;
                                       }
                                   }
                                   """;

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<PropertyDeclarationSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that an accessor list carrying a documentation comment is not flagged, because collapsing it
    /// would delete the comment (issue #420)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForDocumentedAccessorList()
    {
        const string testData = """
                                internal class RH5408
                                {
                                    public int Value
                                    {
                                        /// <summary>
                                        /// Getter
                                        /// </summary>
                                        get;
                                        set;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that no code fix is offered for an accessor list carrying a documentation comment, so the fix
    /// cannot delete it (issue #420)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDocumentedAccessorListIsNotOfferedACodeFix()
    {
        const string codeFixData = """
                                   internal class RH5408
                                   {
                                       public int Value
                                       {
                                           /// <summary>
                                           /// Getter
                                           /// </summary>
                                           get;
                                           set;
                                       }
                                   }
                                   """;

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<PropertyDeclarationSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying that a multi-line get/set auto-property followed by a trailing comment is detected and fixed,
    /// because the comment sits outside the accessor list and is never crossed by the collapse (issue #604)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyAutoPropertyWithTrailingCommentIsDetectedAndFixed()
    {
        const string testData = """
                                internal class RH5408
                                {
                                    {|#0:public int Value
                                    {
                                        get;
                                        set;
                                    }|} // explanation
                                }
                                """;
        const string fixedData = """
                                 internal class RH5408
                                 {
                                     public int Value { get; set; } // explanation
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5408MessageFormat));
    }

    /// <summary>
    /// Verifying that a multi-line get-only auto-property followed by a trailing comment is detected and fixed
    /// (issue #604)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyGetOnlyAutoPropertyWithTrailingCommentIsDetectedAndFixed()
    {
        const string testData = """
                                internal class RH5408
                                {
                                    {|#0:public int Value
                                    {
                                        get;
                                    }|} // explanation
                                }
                                """;
        const string fixedData = """
                                 internal class RH5408
                                 {
                                     public int Value { get; } // explanation
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5408MessageFormat));
    }

    /// <summary>
    /// Verifying that a multi-line auto-property followed by a trailing block comment is detected and fixed
    /// (issue #604)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyAutoPropertyWithTrailingBlockCommentIsDetectedAndFixed()
    {
        const string testData = """
                                internal class RH5408
                                {
                                    {|#0:public int Value
                                    {
                                        get;
                                        set;
                                    }|} /* explanation */
                                }
                                """;
        const string fixedData = """
                                 internal class RH5408
                                 {
                                     public int Value { get; set; } /* explanation */
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5408MessageFormat));
    }

    /// <summary>
    /// Verifying that two multi-line auto-properties with trailing comments in one document are both detected and
    /// fixed together, so a Fix All pass converges (issue #604)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultipleAutoPropertiesWithTrailingCommentsAreDetectedAndFixed()
    {
        const string testData = """
                                internal class RH5408
                                {
                                    {|#0:public int First
                                    {
                                        get;
                                        set;
                                    }|} // first

                                    {|#1:public int Second
                                    {
                                        get;
                                    }|} /* second */
                                }
                                """;
        const string fixedData = """
                                 internal class RH5408
                                 {
                                     public int First { get; set; } // first

                                     public int Second { get; } /* second */
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5408MessageFormat, 2));
    }

    /// <summary>
    /// Verifying that an auto-property carrying a comment between its accessor list and its initializer is not
    /// flagged, because the formatter refuses to join the initializer across that comment (issue #604)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentInInitializerGapAutoPropertyIsNotFlagged()
    {
        const string testData = """
                                internal class RH5408
                                {
                                    public int Value { get; set; } // note
                                    = 1;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that an auto-property carrying a multi-line block comment between its accessor list and its
    /// initializer is not flagged (issue #604)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyBlockCommentInInitializerGapAutoPropertyIsNotFlagged()
    {
        const string testData = """
                                internal class RH5408
                                {
                                    public int Value { get; set; } /* note
                                       more */ = 2;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that an auto-property carrying a directive between its accessor list and its initializer is not
    /// flagged (issue #604)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDirectiveInInitializerGapAutoPropertyIsNotFlagged()
    {
        const string testData = """
                                internal class RH5408
                                {
                                    public int Value { get; set; }
                                #if FEATURE
                                #endif
                                    = 1;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that an auto-property whose initializer sits on its own line without intervening trivia is still
    /// flagged, because the formatter joins the initializer in that case (issue #604)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyOwnLineInitializerAutoPropertyIsDetectedAndFixed()
    {
        const string testData = """
                                internal class RH5408
                                {
                                    {|#0:public int Value { get; set; }
                                    = 1;|}
                                }
                                """;
        const string fixedData = """
                                 internal class RH5408
                                 {
                                     public int Value { get; set; } = 1;
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5408MessageFormat));
    }

    /// <summary>
    /// Verifying that no code fix action is registered for an auto-property carrying a comment between its accessor
    /// list and its initializer, so the code fix does not offer a no-op action (issue #604)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentInInitializerGapAutoPropertyIsNotOfferedACodeFix()
    {
        const string codeFixData = """
                                   internal class RH5408
                                   {
                                       public int Value { get; set; } // note
                                       = 1;
                                   }
                                   """;

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<PropertyDeclarationSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifying that an auto-property whose initializer gap sits on one line is still detected and fixed, because
    /// the formatter never has to join that gap (issue #604)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySameLineInitializerGapAutoPropertyIsDetectedAndFixed()
    {
        const string testData = """
                                internal class RH5408
                                {
                                    {|#0:public int Value
                                    {
                                        get;
                                        set;
                                    } /* note */ = 1;|}
                                }
                                """;
        const string fixedData = """
                                 internal class RH5408
                                 {
                                     public int Value { get; set; } /* note */ = 1;
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5408MessageFormat));
    }

    /// <summary>
    /// Verifying that a multi-line auto-property with an accessor modifier and a trailing comment is detected and
    /// fixed, and that the fix collapses the modifier together with its keyword (issue #604)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyAccessorModifierAutoPropertyWithTrailingCommentIsDetectedAndFixed()
    {
        const string testData = """
                                internal class RH5408
                                {
                                    {|#0:public int Value
                                    {
                                        get;
                                        private set;
                                    }|} // explanation
                                }
                                """;
        const string fixedData = """
                                 internal class RH5408
                                 {
                                     public int Value { get; private set; } // explanation
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5408MessageFormat));
    }

    /// <summary>
    /// Verifying that an auto-property whose terminating semicolon sits on its own line is detected and fixed,
    /// because the formatter joins that gap (issue #612)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyOwnLineDeclarationSemicolonAutoPropertyIsDetectedAndFixed()
    {
        const string testData = """
                                internal class RH5408
                                {
                                    {|#0:public int Value { get; set; } = 1
                                        ;|}
                                }
                                """;
        const string fixedData = """
                                 internal class RH5408
                                 {
                                     public int Value { get; set; } = 1;
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5408MessageFormat));
    }

    /// <summary>
    /// Verifying that a get/init auto-property whose terminating semicolon sits on its own line is detected and
    /// fixed, so the accessor kind does not change the outcome (issue #612)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyOwnLineDeclarationSemicolonInitAutoPropertyIsDetectedAndFixed()
    {
        const string testData = """
                                internal class RH5408
                                {
                                    {|#0:public int Value { get; init; } = 1
                                        ;|}
                                }
                                """;
        const string fixedData = """
                                 internal class RH5408
                                 {
                                     public int Value { get; init; } = 1;
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5408MessageFormat));
    }

    /// <summary>
    /// Verifying that an auto-property combining a multi-line accessor list with an own-line terminating semicolon
    /// is detected and fixed in a single pass (issue #612)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultiLineAccessorListWithOwnLineDeclarationSemicolonIsDetectedAndFixed()
    {
        const string testData = """
                                internal class RH5408
                                {
                                    {|#0:public int Value
                                    {
                                        get;
                                        set;
                                    } = 1
                                        ;|}
                                }
                                """;
        const string fixedData = """
                                 internal class RH5408
                                 {
                                     public int Value { get; set; } = 1;
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5408MessageFormat));
    }

    /// <summary>
    /// Verifying that an auto-property whose initializer and terminating semicolon both sit on their own lines is
    /// detected and fixed in a single pass (issue #612)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyOwnLineInitializerAndDeclarationSemicolonIsDetectedAndFixed()
    {
        const string testData = """
                                internal class RH5408
                                {
                                    {|#0:public int Value { get; set; }
                                        = 1
                                        ;|}
                                }
                                """;
        const string fixedData = """
                                 internal class RH5408
                                 {
                                     public int Value { get; set; } = 1;
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5408MessageFormat));
    }

    /// <summary>
    /// Verifying that several auto-properties with own-line terminating semicolons in one document are all detected
    /// and fixed together (issue #612)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultipleOwnLineDeclarationSemicolonAutoPropertiesAreDetectedAndFixed()
    {
        const string testData = """
                                internal class RH5408
                                {
                                    {|#0:public int First { get; set; } = 1
                                        ;|}

                                    {|#1:public int Second { get; set; } = 2
                                        ;|}
                                }
                                """;
        const string fixedData = """
                                 internal class RH5408
                                 {
                                     public int First { get; set; } = 1;

                                     public int Second { get; set; } = 2;
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5408MessageFormat, 2));
    }

    /// <summary>
    /// Verifying that a comment in the gap between the initializer value and the terminating semicolon exempts the
    /// auto-property, because the formatter refuses to join across that comment (issue #612)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentInDeclarationSemicolonGapAutoPropertyIsNotFlagged()
    {
        const string testData = """
                                internal class RH5408
                                {
                                    public int Value { get; set; } = 1
                                        // note
                                        ;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that a preprocessor directive in the gap between the initializer value and the terminating
    /// semicolon exempts the auto-property, because the formatter refuses to join across that directive (issue #612)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDirectiveInDeclarationSemicolonGapAutoPropertyIsNotFlagged()
    {
        const string testData = """
                                internal class RH5408
                                {
                                    public int Value { get; set; } = 1
                                #if FEATURE
                                #endif
                                        ;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that a region directive around the terminating semicolon exempts the auto-property, because the
    /// formatter refuses to join across that directive (issue #612)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRegionAroundDeclarationSemicolonAutoPropertyIsNotFlagged()
    {
        const string testData = """
                                internal class RH5408
                                {
                                    public int Value { get; set; } = 1
                                #region Terminator
                                        ;
                                #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that a comment trailing the initializer value on its own line keeps exempting the auto-property
    /// through the initializer guard, so the semicolon-gap guard does not claim a shape it does not own (issue #612)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTrailingCommentBeforeDeclarationSemicolonAutoPropertyIsNotFlagged()
    {
        const string testData = """
                                internal class RH5408
                                {
                                    public int Value { get; set; } = 1 // note
                                        ;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that no code fix is offered for an auto-property whose terminating semicolon is separated by a
    /// comment, because the formatter cannot collapse that shape (issue #612)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentInDeclarationSemicolonGapAutoPropertyIsNotOfferedACodeFix()
    {
        const string codeFixData = """
                                   internal class RH5408
                                   {
                                       public int Value { get; set; } = 1
                                           // note
                                           ;
                                   }
                                   """;

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<PropertyDeclarationSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    #endregion // Tests
}