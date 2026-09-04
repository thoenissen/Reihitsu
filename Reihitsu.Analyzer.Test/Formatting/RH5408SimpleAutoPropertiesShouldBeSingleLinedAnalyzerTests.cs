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
public class RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzerTests : BatchCodeFixTestsBase<RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer, RH5408SimpleAutoPropertiesShouldBeSingleLinedCodeFixProvider>
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
    /// Verifying that a multi-line auto-property with accessor attributes is not flagged, because an accessor
    /// carrying its own attribute list is no longer considered simple — its layout belongs to RH5530/RH5531
    /// instead, and RH5408 must not force it onto one line (issue #729)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyAccessorAttributedAutoPropertyIsNotFlagged()
    {
        const string testData = """
                                sealed class TestAttribute : System.Attribute
                                {
                                }

                                internal class RH5408
                                {
                                    public int Value
                                    {
                                        [Test]
                                        get;
                                        [Test]
                                        set;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that a multi-line auto-property combining a property-level attribute with accessor-level
    /// attributes is not flagged. This is the issue's exact reported shape: the shipped code fix could not
    /// converge on it because the analyzer kept reporting the same location after every fix application
    /// (issue #729)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyPropertyAndAccessorAttributedAutoPropertyIsNotFlagged()
    {
        const string testData = """
                                sealed class TestAttribute : System.Attribute
                                {
                                }

                                internal class RH5408
                                {
                                    [Test]
                                    public int Value
                                    {
                                        [Test]
                                        get;
                                        [Test]
                                        set;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that a multi-line auto-property is not flagged when only one of its two accessors carries an
    /// attribute list, so the exemption applies per accessor list rather than requiring every accessor to carry
    /// one (issue #729)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySingleAttributedAccessorAutoPropertyIsNotFlagged()
    {
        const string testData = """
                                sealed class TestAttribute : System.Attribute
                                {
                                }

                                internal class RH5408
                                {
                                    public int Value
                                    {
                                        [Test]
                                        get;
                                        set;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that no code fix action is registered for an auto-property carrying an accessor-level
    /// attribute, so a diagnostic from an earlier analyzer version or a stale IDE session is never offered a
    /// fix that would force the accessor-attributed shape onto one line (issue #729)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyAccessorAttributedAutoPropertyIsNotOfferedACodeFix()
    {
        const string codeFixData = """
                                   sealed class TestAttribute : System.Attribute
                                   {
                                   }

                                   internal class RH5408
                                   {
                                       public int Value
                                       {
                                           [Test]
                                           get;
                                           [Test]
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

    /// <summary>
    /// Verifying that a comment between the initializer value and a semicolon on the same line no longer exempts the
    /// property. The collapse never joins across that gap, and the formatter already reformats the shape (issue #650)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBetweenInitializerValueAndSemicolonIsDetectedAndFixed()
    {
        const string testData = """
                                internal class RH5408
                                {
                                    {|#0:public int Value
                                    {
                                        get;
                                        set;
                                    } = 1 /* note */;|}
                                }
                                """;
        const string fixedData = """
                                 internal class RH5408
                                 {
                                     public int Value { get; set; } = 1 /* note */;
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5408MessageFormat));
    }

    /// <summary>
    /// Verifying that the documentation comment of a property carrying a trailing initializer comment survives the
    /// collapse (issue #650)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDocumentedPropertyWithTrailingInitializerCommentIsDetectedAndFixed()
    {
        const string testData = """
                                internal class RH5408
                                {
                                    /// <summary>The value.</summary>
                                    {|#0:public int Value
                                    {
                                        get;
                                        set;
                                    } = 1 /* note */;|}
                                }
                                """;
        const string fixedData = """
                                 internal class RH5408
                                 {
                                     /// <summary>
                                     /// The value.
                                     /// </summary>
                                     public int Value { get; set; } = 1 /* note */;
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5408MessageFormat));
    }

    /// <summary>
    /// Verifying that the documentation comment of a property without a trailing initializer comment is expanded the
    /// same way. This control pins the expansion as pre-existing behavior of the documentation phase rather than a
    /// side effect of the guard narrowing in issue #650
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDocumentedPropertyWithoutTrailingInitializerCommentIsDetectedAndFixed()
    {
        const string testData = """
                                internal class RH5408
                                {
                                    /// <summary>The value.</summary>
                                    {|#0:public int Value
                                    {
                                        get;
                                        set;
                                    } = 1;|}
                                }
                                """;
        const string fixedData = """
                                 internal class RH5408
                                 {
                                     /// <summary>
                                     /// The value.
                                     /// </summary>
                                     public int Value { get; set; } = 1;
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5408MessageFormat));
    }

    /// <summary>
    /// Verifying that an init accessor with a trailing initializer comment is collapsed (issue #650)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyInitPropertyWithTrailingInitializerCommentIsDetectedAndFixed()
    {
        const string testData = """
                                internal class RH5408
                                {
                                    {|#0:public int Value
                                    {
                                        get;
                                        init;
                                    } = 1 /* note */;|}
                                }
                                """;
        const string fixedData = """
                                 internal class RH5408
                                 {
                                     public int Value { get; init; } = 1 /* note */;
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5408MessageFormat));
    }

    /// <summary>
    /// Verifying that an accessor modifier is preserved when a property with a trailing initializer comment is
    /// collapsed (issue #650)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyAccessorModifierWithTrailingInitializerCommentIsDetectedAndFixed()
    {
        const string testData = """
                                internal class RH5408
                                {
                                    {|#0:public int Value
                                    {
                                        get;
                                        private set;
                                    } = 1 /* note */;|}
                                }
                                """;
        const string fixedData = """
                                 internal class RH5408
                                 {
                                     public int Value { get; private set; } = 1 /* note */;
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5408MessageFormat));
    }

    /// <summary>
    /// Verifying that a single-line generic initializer value with a trailing comment is collapsed (issue #650)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyGenericInitializerWithTrailingCommentIsDetectedAndFixed()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class RH5408
                                {
                                    {|#0:public Dictionary<int, string> Value
                                    {
                                        get;
                                        set;
                                    } = new Dictionary<int, string>() /* note */;|}
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 internal class RH5408
                                 {
                                     public Dictionary<int, string> Value { get; set; } = new Dictionary<int, string>() /* note */;
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5408MessageFormat));
    }

    /// <summary>
    /// Verifying that a comment before the initializer equals token still exempts the property. The gap guard between
    /// the accessor list and the initializer is, after issue #650, the only owner of that region
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBeforeInitializerEqualsIsNotReported()
    {
        const string testData = """
                                internal class RH5408
                                {
                                    public int Value
                                    {
                                        get;
                                        set;
                                    }
                                        /* note */ = 1;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that a directive before the initializer equals token still exempts the property. The gap guard
    /// between the accessor list and the initializer is, after issue #650, the only owner of that region
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDirectiveBeforeInitializerEqualsIsNotReported()
    {
        const string testData = """
                                internal class RH5408
                                {
                                    public int Value
                                    {
                                        get;
                                        set;
                                    }
                                #if FEATURE
                                #endif
                                        = 1;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that a line comment between the initializer value and the semicolon still exempts the property. Such
    /// a comment runs to the end of the line, so the gap spans lines and its own guard owns it (issue #650)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyLineCommentBetweenInitializerValueAndSemicolonIsNotReported()
    {
        const string testData = """
                                internal class RH5408
                                {
                                    public int Value
                                    {
                                        get;
                                        set;
                                    } = 1 // note
                                        ;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that a block comment between the initializer value and a semicolon on a later line still exempts the
    /// property. This is the negative side of the same boundary and the same trivia kind as the reported shape, so it
    /// pins the line-span condition rather than the comment kind (issue #650)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyBlockCommentBeforeSemicolonOnItsOwnLineIsNotReported()
    {
        const string testData = """
                                internal class RH5408
                                {
                                    public int Value
                                    {
                                        get;
                                        set;
                                    } = 1 /* note */
                                        ;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that a comment between the equals token and the initializer value still exempts the property. The
    /// collapse would be safe there, but the interior guard is the only owner of that gap, so the shape is left to
    /// manual correction (issue #650)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBetweenEqualsAndInitializerValueIsNotReported()
    {
        const string testData = """
                                internal class RH5408
                                {
                                    public int Value
                                    {
                                        get;
                                        set;
                                    } = /* note */ 1;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that a multi-line initializer value with a trailing comment still exempts the property (issue #650)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultiLineInitializerValueWithTrailingCommentIsNotReported()
    {
        const string testData = """
                                internal class RH5408
                                {
                                    public int Value
                                    {
                                        get;
                                        set;
                                    } = 1
                                        + 2 /* note */;
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class RH5408
                                {
                                    {|#0:public int A
                                    {
                                        get;
                                        set;
                                    }|}
                                    {|#1:public int B
                                    {
                                        get;
                                        set;
                                    }|}
                                }
                                """;

        const string fixedCode = """
                                 internal class RH5408
                                 {
                                     public int A { get; set; }
                                     public int B { get; set; }
                                 }
                                 """;

        // The two properties are adjacent with no blank line between them, so the first fix's collapsed
        // replacement span directly abuts the second occurrence's leading trivia
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5408MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}