using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5513MethodAttributesMustFollowPlacementRulesAnalyzer"/> and <see cref="RH5513MethodAttributesMustFollowPlacementRulesCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5513MethodAttributesMustFollowPlacementRulesAnalyzerTests : BatchCodeFixTestsBase<RH5513MethodAttributesMustFollowPlacementRulesAnalyzer, RH5513MethodAttributesMustFollowPlacementRulesCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that policy violations are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5513-001")]
    [Requirement("RH5513-003")]
    [Requirement("RH5513-004")]
    public async Task VerifyDiagnosticAndCodeFixForPolicyViolation()
    {
        const string testData = """
                                internal class Example
                                {
                                    {|#0:[First]|} internal void M() { }
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
                                     [First]
                                     internal void M() { }
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
                     Diagnostics(RH5513MethodAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5513MessageFormat));
    }

    /// <summary>
    /// Verifies that splitting the attribute list inside an object initializer lands the member on the attribute
    /// list's own column rather than the enclosing brace-scope nesting level, which understates an anchor-derived
    /// column by not accounting for the initializer's own alignment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5513-004")]
    [Requirement("SHARED-ATTR-009")]
    public async Task VerifyCodeFixAlignsToAttributeListColumnInsideObjectInitializer()
    {
        const string testData = """
                                internal class Example
                                {
                                    private readonly Config _config = new Config
                                    {
                                        Handler = () =>
                                        {
                                                                          {|#0:[First]|} void Local()
                                                                          {
                                                                          }
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
                                                                           [First]
                                                                           void Local()
                                                                           {
                                                                           }
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
                     Diagnostics(RH5513MethodAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5513MessageFormat));
    }

    /// <summary>
    /// Verifies the same anchor-column alignment inside an anonymous object, which is recognized by
    /// <see cref="Reihitsu.Core.SyntaxIndentationUtilities.ComputeBaseIndentLevel"/> exactly like an object
    /// initializer
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5513-004")]
    [Requirement("SHARED-ATTR-009")]
    public async Task VerifyCodeFixAlignsToAttributeListColumnInsideAnonymousObject()
    {
        const string testData = """
                                internal class Example
                                {
                                    private readonly object _config = new
                                    {
                                        Handler = (System.Action)(() =>
                                        {
                                                                          {|#0:[First]|} void Local()
                                                                          {
                                                                          }
                                        })
                                    };
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     private readonly object _config = new
                                     {
                                         Handler = (System.Action)(() =>
                                         {
                                                                           [First]
                                                                           void Local()
                                                                           {
                                                                           }
                                         })
                                     };
                                 }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5513MethodAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5513MessageFormat));
    }

    /// <summary>
    /// Verifies that compliant code is not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5513-001")]
    public async Task VerifyNoDiagnosticForCompliantCode()
    {
        const string testData = """
                                internal class Example
                                {
                                    [First]
                                    internal void M() { }
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
    [Requirement("RH5513-005")]
    [Requirement("SHARED-ATTR-011")]
    public async Task VerifyDiagnosticWithoutCodeFixWhenCommentsArePresent()
    {
        const string testData = """
                                internal class Example
                                {
                                    {|#0:[First /* keep */]|} internal void M() { }
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
                                       [First /* keep */] internal void M() { }
                                   }
                                   sealed class FirstAttribute : System.Attribute
                                   {
                                   }
                                   sealed class SecondAttribute : System.Attribute
                                   {
                                   }
                                   """;

        await Verify(testData,
                     Diagnostics(RH5513MethodAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5513MessageFormat));

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5513MethodAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<AttributeListSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that an attribute list of a local function is checked and the fix indents at the block's depth
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5513-001")]
    [Requirement("SHARED-ATTR-001")]
    [Requirement("SHARED-ATTR-008")]
    public async Task VerifyDiagnosticAndCodeFixForLocalFunction()
    {
        const string testData = """
                                internal class Example
                                {
                                    internal void M()
                                    {
                                        {|#0:[First]|} void Local() { }
                                    }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal void M()
                                     {
                                         [First]
                                         void Local() { }
                                     }
                                 }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5513MethodAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5513MessageFormat));
    }

    /// <summary>
    /// Verifies that an attribute list of a lambda is checked
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5513-001")]
    [Requirement("SHARED-ATTR-001")]
    public async Task VerifyDiagnosticAndCodeFixForLambda()
    {
        const string testData = """
                                internal class Example
                                {
                                    internal void M()
                                    {
                                        System.Func<int> f = {|#0:[First]|} () => 1;
                                    }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal void M()
                                     {
                                         System.Func<int> f = [First]
                                         () => 1;
                                     }
                                 }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5513MethodAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5513MessageFormat));
    }

    /// <summary>
    /// Verifies that an attribute list of an operator is checked
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5513-001")]
    [Requirement("SHARED-ATTR-001")]
    public async Task VerifyDiagnosticAndCodeFixForOperator()
    {
        const string testData = """
                                internal class Example
                                {
                                    {|#0:[First]|} public static Example operator +(Example left, Example right) => left;
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     [First]
                                     public static Example operator +(Example left, Example right) => left;
                                 }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5513MethodAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5513MessageFormat));
    }

    /// <summary>
    /// Verifies that an attribute list of a conversion operator is checked
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5513-001")]
    [Requirement("SHARED-ATTR-001")]
    public async Task VerifyDiagnosticAndCodeFixForConversionOperator()
    {
        const string testData = """
                                internal class Example
                                {
                                    {|#0:[First]|} public static implicit operator int(Example value) => 0;
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     [First]
                                     public static implicit operator int(Example value) => 0;
                                 }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5513MethodAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5513MessageFormat));
    }

    /// <summary>
    /// Verifies that an attribute list of a destructor is checked
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5513-001")]
    [Requirement("SHARED-ATTR-001")]
    public async Task VerifyDiagnosticAndCodeFixForDestructor()
    {
        const string testData = """
                                internal class Example
                                {
                                    {|#0:[First]|} ~Example() { }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     [First]
                                     ~Example() { }
                                 }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5513MethodAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5513MessageFormat));
    }

    /// <summary>
    /// Verifies that an attribute list with the type specifier on a method resolves to the method target
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5513-001")]
    [Requirement("SHARED-ATTR-001")]
    public async Task VerifyDiagnosticAndCodeFixForTypeSpecifier()
    {
        const string testData = """
                                internal class Example
                                {
                                    {|#0:[type: First]|} internal void M() { }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     [type: First]
                                     internal void M() { }
                                 }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5513MethodAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5513MessageFormat));
    }

    /// <summary>
    /// Verifies that an attribute list with the method specifier on a property resolves to the method target
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5513-001")]
    [Requirement("SHARED-ATTR-002")]
    public async Task VerifyDiagnosticAndCodeFixForMethodSpecifierOnProperty()
    {
        const string testData = """
                                internal class Example
                                {
                                    {|#0:[method: First]|} internal int Value { get; set; }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     [method: First]
                                     internal int Value { get; set; }
                                 }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5513MethodAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5513MessageFormat));
    }

    /// <summary>
    /// Verifies that an attribute list with a specifier that is not an attribute target is not checked
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5513-001")]
    [Requirement("SHARED-ATTR-002")]
    public async Task VerifyNoDiagnosticForUnknownSpecifier()
    {
        const string testData = """
                                internal class Example
                                {
                                    [unknown: First] internal void M() { }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an attribute list of an accessor is not checked, even though it resolves to the method target
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5513-002")]
    public async Task VerifyNoDiagnosticForAccessorAttributeList()
    {
        const string testData = """
                                internal class Example
                                {
                                    internal int Value
                                    {
                                        [First] get;
                                        set;
                                    }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the code fix indents at the nesting depth of a namespace and a type, and fixes every list of the declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5513-003")]
    [Requirement("RH5513-004")]
    [Requirement("SHARED-ATTR-004")]
    [Requirement("SHARED-ATTR-007")]
    [Requirement("SHARED-ATTR-008")]
    public async Task VerifyDiagnosticAndCodeFixForTwoListsInsideNamespace()
    {
        const string testData = """
                                namespace Sample
                                {
                                    internal class Example
                                    {
                                        {|#0:[First]|} {|#1:[Second]|} internal void M() { }
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
                                 namespace Sample
                                 {
                                     internal class Example
                                     {
                                         [First]
                                         [Second]
                                         internal void M() { }
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
                     Diagnostics(RH5513MethodAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5513MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that the code fix indents one level more inside the statements of a switch section
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5513-004")]
    [Requirement("SHARED-ATTR-008")]
    public async Task VerifyCodeFixIndentsInsideSwitchSection()
    {
        const string testData = """
                                internal class Example
                                {
                                    internal void M(int value)
                                    {
                                        switch (value)
                                        {
                                            case 1:
                                                {|#0:[First]|} void Local() { }
                                                break;
                                        }
                                    }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal void M(int value)
                                     {
                                         switch (value)
                                         {
                                             case 1:
                                                 [First]
                                                 void Local() { }
                                                 break;
                                         }
                                     }
                                 }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5513MethodAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5513MessageFormat));
    }

    /// <summary>
    /// Verifies that the code fix indents one level more inside a block of a switch section
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5513-004")]
    [Requirement("SHARED-ATTR-008")]
    public async Task VerifyCodeFixIndentsInsideBlockOfSwitchSection()
    {
        const string testData = """
                                internal class Example
                                {
                                    internal void M(int value)
                                    {
                                        switch (value)
                                        {
                                            case 1:
                                                {
                                                    {|#0:[First]|} void Local() { }
                                                    break;
                                                }
                                        }
                                    }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal void M(int value)
                                     {
                                         switch (value)
                                         {
                                             case 1:
                                                 {
                                                     [First]
                                                     void Local() { }
                                                     break;
                                                 }
                                         }
                                     }
                                 }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5513MethodAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5513MessageFormat));
    }

    /// <summary>
    /// Verifies that the code fix aligns with the declaration inside an array initializer
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5513-004")]
    [Requirement("SHARED-ATTR-009")]
    public async Task VerifyCodeFixAlignsToDeclarationColumnInsideArrayInitializer()
    {
        const string testData = """
                                internal class Example
                                {
                                    private readonly System.Action[] _actions =
                                    {
                                        () =>
                                        {
                                                {|#0:[First]|} void Local()
                                                {
                                                }
                                        }
                                    };
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     private readonly System.Action[] _actions =
                                     {
                                         () =>
                                         {
                                                 [First]
                                                 void Local()
                                                 {
                                                 }
                                         }
                                     };
                                 }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5513MethodAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5513MessageFormat));
    }

    /// <summary>
    /// Verifies that a documentation comment in front of the attribute list does not withhold the code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5513-004")]
    [Requirement("SHARED-ATTR-011")]
    public async Task VerifyDiagnosticAndCodeFixWithDocumentationCommentInFrontOfList()
    {
        const string testData = """
                                internal class Example
                                {
                                    /// <summary>M</summary>
                                    {|#0:[First]|} internal void M() { }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     /// <summary>M</summary>
                                     [First]
                                     internal void M() { }
                                 }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5513MethodAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5513MessageFormat));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class Example
                                {
                                    {|#0:[First]|} {|#1:[Second]|} internal void M() { }
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
                                     [First]
                                     [Second]
                                     internal void M() { }
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
                                  Diagnostics(RH5513MethodAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5513MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}