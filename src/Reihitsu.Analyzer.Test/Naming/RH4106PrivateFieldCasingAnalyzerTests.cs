using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Naming;
using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4106PrivateFieldCasingAnalyzer"/> and <see cref="RH4106PrivateFieldCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH4106PrivateFieldCasingAnalyzerTests : BatchCodeFixTestsBase<RH4106PrivateFieldCasingAnalyzer, RH4106PrivateFieldCasingCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported for private fields that do not use _camelCase and that references are renamed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4106-001")]
    [Requirement("RH4106-002")]
    [Requirement("RH4106-003")]
    [Requirement("RH4106-004")]
    [Requirement("RH4106-005")]
    [Requirement("SHARED-CASING-007")]
    public async Task VerifyDiagnosticsForPrivateFieldAndReferenceAreFixed()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceCache
                                    {
                                        private int {|#0:cacheCount|};

                                        public int GetCount()
                                        {
                                            return cacheCount;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class ResourceCache
                                     {
                                         private int _cacheCount;

                                         public int GetCount()
                                         {
                                             return _cacheCount;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4106PrivateFieldCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4106MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for a private field that already uses _camelCase
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4106-002")]
    [Requirement("SHARED-CASING-003")]
    public async Task VerifyNoDiagnosticsForUnderlineCamelCasePrivateField()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceCache
                                    {
                                        private int _cacheCount;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies diagnostics are reported for private readonly fields that do not use _camelCase
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4106-001")]
    [Requirement("RH4106-004")]
    public async Task VerifyDiagnosticsForPrivateReadonlyFieldWithoutUnderlinePrefix()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceCache
                                    {
                                        private readonly int {|#0:cacheLimit|} = 10;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class ResourceCache
                                     {
                                         private readonly int _cacheLimit = 10;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4106PrivateFieldCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4106MessageFormat));
    }

    /// <summary>
    /// Verifies diagnostics are reported for private static readonly fields that do not use _camelCase
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4106-001")]
    [Requirement("RH4106-004")]
    public async Task VerifyDiagnosticsForPrivateStaticReadonlyFieldWithoutUnderlinePrefix()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceCache
                                    {
                                        private static readonly int {|#0:cacheLimit|} = 10;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class ResourceCache
                                     {
                                         private static readonly int _cacheLimit = 10;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4106PrivateFieldCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4106MessageFormat));
    }

    /// <summary>
    /// Verifies private protected fields are not claimed by the private field rule because they are handled by the protected field rule (RH4107)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4106-001")]
    [Requirement("SHARED-CASING-004")]
    public async Task VerifyNoDiagnosticsForPrivateProtectedField()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceCache
                                    {
                                        private protected int CacheCount;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no code fix is offered for a letterless private field whose conversion cannot produce a valid identifier
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4106-006")]
    [Requirement("SHARED-CASING-008")]
    public async Task NoCodeFixForLetterlessPrivateField()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceCache
                                    {
                                        private int __;
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testCode,
                                                   RH4106PrivateFieldCasingAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<VariableDeclaratorSyntax>()
                                                               .Single(declarator => declarator.Identifier.ValueText == "__")
                                                               .Identifier
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that a field without an accessibility modifier is treated as effectively private
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4106-001")]
    [Requirement("RH4106-004")]
    [Requirement("SHARED-CASING-004")]
    public async Task VerifyImplicitlyPrivateFieldIsDetectedAndFixed()
    {
        const string testCode = """
                                internal class ResourceCache
                                {
                                    int {|#0:cacheCount|};
                                }
                                """;

        const string fixedCode = """
                                 internal class ResourceCache
                                 {
                                     int _cacheCount;
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4106PrivateFieldCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4106MessageFormat));
    }

    /// <summary>
    /// Verifies that _camelCase accepts a bare underscore and an underscore followed by a camelCase name, and rejects a second underscore, an uppercase letter, and a digit after the prefix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-003")]
    [Requirement("RH4106-002")]
    public async Task UnderlineCamelCaseAcceptsBareUnderscoreAndRejectsOtherPrefixesAndUppercaseOrDigitStarts()
    {
        const string testCode = """
                                public class C
                                {
                                    private int _;
                                    private int _value2;
                                    private int {|RH4106:__double|};
                                    private int {|RH4106:_Value|};
                                    private int {|RH4106:_1st|};
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that a field without accessibility modifier belongs to the private group in a class, struct, and record, and to the public group in an interface
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-004")]
    [Requirement("RH4106-001")]
    public async Task FieldsWithoutAccessibilityModifierBelongToThePrivateGroupOutsideInterfaces()
    {
        const string testCode = """
                                public class C
                                {
                                    int {|RH4106:a|};
                                    private int {|RH4106:b|};
                                }

                                public struct S
                                {
                                    int {|RH4106:a|};
                                }

                                public record R
                                {
                                    int {|RH4106:a|};
                                }

                                public interface I
                                {
                                    static int a;
                                    private static int {|RH4106:b|};
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that private, implicitly private, and static readonly fields are checked, while constants and fields of the protected group are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4106-001")]
    public async Task PrivateFieldsThatAreNotConstantsAreChecked()
    {
        const string testCode = """
                                public class C
                                {
                                    private int {|RH4106:MyField|};
                                    int {|RH4106:my_field2|};
                                    private static readonly int {|RH4106:MaxValue|} = 1;
                                    private const int Limit = 2;
                                    protected int Prot;
                                    private protected int PrivProt;

                                    public int Run() => MyField + my_field2 + MaxValue + Limit;
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that a checked field is reported when its name lacks the underscore prefix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4106-002")]
    public async Task CheckedFieldsThatAreNotUnderlineCamelCaseAreReported()
    {
        const string testCode = """
                                public class C
                                {
                                    private int _total;
                                    private int {|RH4106:total2|};
                                    private int {|RH4106:Count|};
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that each non-conforming declarator of one field declaration gets its own diagnostic on its identifier
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4106-003")]
    public async Task EachNonConformingDeclaratorIsReportedOnItsIdentifier()
    {
        const string testCode = """
                                public class C
                                {
                                    private int _ok, {|RH4106:Bad|}, {|RH4106:worse|};
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the fix renames an implicitly private field and a static readonly field to _camelCase
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4106-004")]
    public async Task FixRenamesImplicitlyPrivateAndStaticReadonlyFieldsToUnderlineCamelCase()
    {
        const string testCode = """
                                internal class C
                                {
                                    int {|RH4106:cacheCount|};
                                    private static readonly int {|RH4106:cacheLimit|} = 10;
                                }
                                """;

        const string fixedCode = """
                                 internal class C
                                 {
                                     int _cacheCount;
                                     private static readonly int _cacheLimit = 10;
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix derives the _camelCase name from PascalCase, shouted, m_-prefixed, double-underscore, and acronym names, and keeps an uppercase letter inside the first word
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-006")]
    [Requirement("RH4106-004")]
    public async Task FixDerivesUnderlineCamelCaseFromTheWordsOfTheOldName()
    {
        const string testCode = """
                                public class C
                                {
                                    private int {|RH4106:FirstName|};
                                    private int {|RH4106:LAST_NAME|};
                                    private int {|RH4106:m_value|};
                                    private int {|RH4106:__double|};
                                    private int {|RH4106:URL|};
                                    private int {|RH4106:iPhone|};

                                    public int Sum => FirstName + LAST_NAME + m_value + __double + URL + iPhone;
                                }
                                """;

        const string fixedCode = """
                                 public class C
                                 {
                                     private int _firstName;
                                     private int _lastName;
                                     private int _mValue;
                                     private int _double;
                                     private int _url;
                                     private int _iPhone;

                                     public int Sum => _firstName + _lastName + _mValue + _double + _url + _iPhone;
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix renames a reference inside a nameof expression together with the field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-007")]
    [Requirement("RH4106-005")]
    [Requirement("SHARED-FIX-003")]
    public async Task FixRenamesFieldReferencedInNameof()
    {
        const string testCode = """
                                public class C
                                {
                                    private int {|RH4106:Value|};

                                    public string Name => nameof(Value);
                                }
                                """;

        const string fixedCode = """
                                 public class C
                                 {
                                     private int _value;

                                     public string Name => nameof(_value);
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that a field without letters or digits is still reported, but no fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4106-006")]
    public async Task NoFixIsOfferedForFieldWithoutLetterOrDigit()
    {
        const string testCode = """
                                public class C
                                {
                                    private int {|RH4106:__|};
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a field whose derived name is its old name is still reported, but no fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-008")]
    [Requirement("RH4106-006")]
    public async Task NoFixIsOfferedWhenTheDerivedNameIsTheOldName()
    {
        const string testCode = """
                                public class C
                                {
                                    private int {|RH4106:_1st|};
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a field whose new name is already declared in the same type is still reported, but no fix is
    /// offered, because the renamed field would collide with the other field and not compile
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    [Requirement("RH4106-006")]
    [Requirement("SHARED-FIX-003")]
    [Requirement("SHARED-DIAG-011")]
    public async Task NoFixIsOfferedWhenTheNewNameIsTaken()
    {
        const string testCode = """
                                public class C
                                {
                                    private int {|RH4106:Value|};
                                    private int _value;
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that the field of the compile-error case is renamed once no other field takes its new name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task FixIsOfferedWhenTheRenameCausesNoCompileError()
    {
        const string testCode = """
                                public class C
                                {
                                    private int {|RH4106:Value|};
                                }
                                """;

        const string fixedCode = """
                                 public class C
                                 {
                                     private int _value;
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix renames a documentation-comment reference that binds to the field, and keeps a word of
    /// the comment's text that only reads like the field's name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task FixRenamesTheBoundDocumentationReferenceAndKeepsTheCommentText()
    {
        const string testCode = """
                                public class C
                                {
                                    /// <summary>Value is <see cref="Value"/>.</summary>
                                    private int {|RH4106:Value|};

                                    public int Get() => Value;
                                }
                                """;

        const string fixedCode = """
                                 public class C
                                 {
                                     /// <summary>Value is <see cref="_value"/>.</summary>
                                     private int _value;

                                     public int Get() => _value;
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix is offered when an excluded <c>#if</c> branch does not name the field, and leaves that
    /// branch unchanged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task FixLeavesAnExcludedBranchThatDoesNotNameTheFieldUnchanged()
    {
        const string testCode = """
                                public class C
                                {
                                    private int {|RH4106:Value|};

                                    public int M()
                                    {
                                #if NEVER
                                        return 0;
                                #else
                                        return Value;
                                #endif
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class C
                                 {
                                     private int _value;

                                     public int M()
                                     {
                                 #if NEVER
                                         return 0;
                                 #else
                                         return _value;
                                 #endif
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that a field whose derived name equals its old name is still reported, but no code action is
    /// registered for it, because the action could not change the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-005")]
    public async Task NoCodeActionIsRegisteredWhenTheFixWouldNotChangeTheDocument()
    {
        const string testCode = """
                                public class C
                                {
                                    private int {|RH4106:_1st|};
                                }
                                """;

        await Verify(testCode);

        var actions = await GetCodeFixActionsAsync(testCode.Replace("{|RH4106:", string.Empty).Replace("|}", string.Empty),
                                                   RH4106PrivateFieldCasingAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<VariableDeclaratorSyntax>()
                                                               .Single(declarator => declarator.Identifier.ValueText == "_1st")
                                                               .Identifier
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that the fix changes only the field's name and the reference to it, and keeps the two blank lines
    /// between the members and the unformatted spacing of the member that refers to the field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-006")]
    public async Task FixChangesOnlyTheNamesAndKeepsTheLayoutAroundThem()
    {
        const string testCode = """
                                public class C
                                {
                                    private int {|RH4106:Value|};


                                    public int  Get( ) => Value;
                                }
                                """;
        const string fixedCode = """
                                 public class C
                                 {
                                     private int _value;


                                     public int  Get( ) => _value;
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    [TestMethod]
    [Requirement("RH4106-007")]
    [Requirement("SHARED-CASING-010")]
    public override async Task FixAllCorrectsEveryDiagnostic()
    {
        await base.FixAllCorrectsEveryDiagnostic();
    }

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class TestClass
                                    {
                                        private int {|#0:Value|};

                                        private int {|#1:value|};

                                        private int {|#2:Count|};

                                        public int Sum => Value + value + Count;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class TestClass
                                     {
                                         private int _value;

                                         private int {|#1:value|};

                                         private int _count;

                                         public int Sum => _value + value + _count;
                                     }
                                 }
                                 """;

        const string batchFixedCode = """
                                      namespace Reihitsu.Analyzer.Test.Naming.Resources
                                      {
                                          public class TestClass
                                          {
                                              private int {|#0:Value|};

                                              private int {|#1:value|};

                                              private int _count;

                                              public int Sum => Value + value + _count;
                                          }
                                      }
                                      """;

        // "Value" and "value" both convert to "_value" inside the same type, so they form a duplicate-target
        // group that Fix All skips entirely, while the unrelated third field is still corrected
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH4106PrivateFieldCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4106MessageFormat, 3),
                                  config =>
                                  {
                                      // The two colliding fields are the one shape where the two paths differ: applying the fixes one
                                      // at a time corrects the first and then refuses the second, while Fix All drops the whole
                                      // duplicate-target group and corrects neither
                                      config.BatchFixedCode = batchFixedCode;

                                      config.FixedState.ExpectedDiagnostics.Add(Diagnostic(RH4106PrivateFieldCasingAnalyzer.DiagnosticId).WithLocation(1, DiagnosticLocationOptions.InterpretAsMarkupKey).WithMessage(AnalyzerResources.RH4106MessageFormat));

                                      config.BatchFixedState.ExpectedDiagnostics.Add(Diagnostic(RH4106PrivateFieldCasingAnalyzer.DiagnosticId).WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey).WithMessage(AnalyzerResources.RH4106MessageFormat));
                                      config.BatchFixedState.ExpectedDiagnostics.Add(Diagnostic(RH4106PrivateFieldCasingAnalyzer.DiagnosticId).WithLocation(1, DiagnosticLocationOptions.InterpretAsMarkupKey).WithMessage(AnalyzerResources.RH4106MessageFormat));

                                      // The two skipped candidates keep the batch loop running for a second, no-op pass
                                      config.NumberOfFixAllIterations = -2;
                                  });
    }

    #endregion // BatchCodeFixTestsBase
}