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

        await Verify(testCode, fixedCode, Diagnostics(RH4106PrivateFieldCasingAnalyzer.DiagnosticId, MessageFormat));
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

        await Verify(testCode, fixedCode, Diagnostics(RH4106PrivateFieldCasingAnalyzer.DiagnosticId, MessageFormat));
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

        await Verify(testCode, fixedCode, Diagnostics(RH4106PrivateFieldCasingAnalyzer.DiagnosticId, MessageFormat));
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

        await Verify(testCode, fixedCode, Diagnostics(RH4106PrivateFieldCasingAnalyzer.DiagnosticId, MessageFormat));
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
    [Requirement("SHARED-CASING-007")]
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

    /// <summary>
    /// Verifies that a field keeps its diagnostic but gets no fix when an excluded <c>#if</c> branch references it, because the rename would leave that reference behind
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-007")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenAnExcludedIfBranchReferencesTheField()
    {
        const string testCode = """
                                public class C
                                {
                                    private int {|RH4106:Value|};

                                    public int Get()
                                    {
                                #if NEVER
                                        return Value + 1;
                                #else
                                        return Value;
                                #endif
                                    }
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a field keeps its diagnostic but gets no fix when an excluded <c>#elif</c> branch references it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-007")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenAnExcludedElifBranchReferencesTheField()
    {
        const string testCode = """
                                public class C
                                {
                                    private int {|RH4106:Value|};

                                    public int Get()
                                    {
                                #if NEVER
                                        return 0;
                                #elif OTHER
                                        return Value + 1;
                                #else
                                        return Value;
                                #endif
                                    }
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a field keeps its diagnostic but gets no fix when an <c>#if false</c> block references it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-007")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenAnIfFalseBlockReferencesTheField()
    {
        const string testCode = """
                                public class C
                                {
                                    private int {|RH4106:Value|};

                                    public int Get()
                                    {
                                #if false
                                        return Value + 1;
                                #endif
                                        return Value;
                                    }
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a field keeps its diagnostic but gets no fix when an excluded branch declares the same field, which the rename would leave under the old name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-007")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenAnExcludedBranchDeclaresTheFieldAgain()
    {
        const string testCode = """
                                public class C
                                {
                                #if NEVER
                                    private int Value = 1;
                                #else
                                    private int {|RH4106:Value|};
                                #endif

                                    public int Get() => Value;
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a field keeps its diagnostic but gets no fix when an excluded branch declares a member of the new name, which would be declared twice in a build that includes the branch
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-007")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenAnExcludedBranchDeclaresTheNewName()
    {
        const string testCode = """
                                public class C
                                {
                                    private int {|RH4106:Value|};

                                #if NEVER
                                    private int _value;
                                #endif

                                    public int Get() => Value;
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a field keeps its diagnostic but gets no fix when an excluded <c>#if</c> branch references it, also in a document with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-007")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenAnExcludedIfBranchReferencesTheFieldWithCarriageReturnLineFeed()
    {
        var testCode = NormalizeToCarriageReturnLineFeed("""
                                                         public class C
                                                         {
                                                             private int {|RH4106:Value|};

                                                             public int Get()
                                                             {
                                                         #if NEVER
                                                                 return Value + 1;
                                                         #else
                                                                 return Value;
                                                         #endif
                                                             }
                                                         }
                                                         """);

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies that a field keeps its diagnostic but gets no fix when an excluded <c>#if</c> branch of another part of its partial class, declared in another file, references it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-007")]
    [Requirement("SHARED-FIX-003")]
    public async Task NoFixIsOfferedWhenAnExcludedBranchOfAnotherPartReferencesTheField()
    {
        const string testCode = """
                                public partial class C
                                {
                                    private int {|RH4106:Value|};

                                    public int Get()
                                    {
                                        return Value;
                                    }
                                }
                                """;

        const string otherPartCode = """
                                     public partial class C
                                     {
                                     #if NEVER
                                         public int Other()
                                         {
                                             return Value;
                                         }
                                     #endif
                                     }
                                     """;

        await Verify(testCode,
                     testCode,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", otherPartCode));
                         test.FixedState.Sources.Add(("/0/Test1.cs", otherPartCode));
                     });
    }

    /// <summary>
    /// Verifies that the fix is offered when only an excluded <c>#if</c> branch of an unrelated type in another file names the field's old and new names, because the field cannot be referred to there, and leaves that file unchanged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-007")]
    public async Task FixIsOfferedWhenOnlyAnExcludedBranchOfAnUnrelatedTypeInAnotherFileNamesTheField()
    {
        const string testCode = """
                                public class C
                                {
                                    private int {|RH4106:Value|};

                                    public int Get()
                                    {
                                        return Value;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class C
                                 {
                                     private int _value;

                                     public int Get()
                                     {
                                         return _value;
                                     }
                                 }
                                 """;

        const string otherFileCode = """
                                     public class D
                                     {
                                     #if NEVER
                                         private int Value;
                                         private int _value;

                                         public int Other()
                                         {
                                             return Value + _value;
                                         }
                                     #endif
                                     }
                                     """;

        await Verify(testCode,
                     fixedCode,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", otherFileCode));
                         test.FixedState.Sources.Add(("/0/Test1.cs", otherFileCode));
                     });
    }

    /// <summary>
    /// Verifies the example of derived names that the rule still reports: every field is still reported, and no fix is
    /// offered for any of them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-008")]
    [Requirement("RH4106-006")]
    public async Task NoFixIsOfferedForAnyFieldWhoseDerivedNameTheRuleStillReports()
    {
        const string testCode = """
                                public class C
                                {
                                    private int {|RH4106:__1st|};
                                    private int {|RH4106:_1_value|};
                                    private int {|RH4106:注文|};
                                    private int {|RH4106:注文_明細|};
                                    private int {|RH4106:ϒ_x|};
                                }
                                """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies, one name at a time, that a field whose derived name the rule still reports is still reported but not
    /// fixed: a digit, a letter without case, or a letter without a lowercase mapping would follow the prefix
    /// </summary>
    /// <param name="oldName">The reported field name</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-008")]
    [Requirement("RH4106-006")]
    [DataRow("__1st")]
    [DataRow("_1_value")]
    [DataRow("注文")]
    [DataRow("注文_明細")]
    [DataRow("ϒ_x")]
    [DataRow("_ϒ_x")]
    [DataRow("ϒx")]
    public async Task NoFixIsOfferedWhenTheRuleStillReportsTheDerivedName(string oldName)
    {
        var testCode = $$"""
                         public class C
                         {
                             private int {|RH4106:{{oldName}}|};
                         }
                         """;

        await Verify(testCode, testCode);
    }

    /// <summary>
    /// Verifies the example of a derived name that the rule accepts: the first word starts with <c>ß</c>, a lowercase
    /// letter, so <c>_ßX</c> is in <c>_camelCase</c> and the field is renamed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-008")]
    [Requirement("RH4106-004")]
    public async Task FixIsOfferedWhenTheRuleAcceptsTheDerivedNameOfAnUncoveredOldName()
    {
        const string testCode = """
                                public class C
                                {
                                    private int {|RH4106:ß_x|};
                                }
                                """;

        const string fixedCode = """
                                 public class C
                                 {
                                     private int _ßX;
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that Fix All leaves the fields whose derived names the rule still reports, and renames the other field of
    /// the document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-008")]
    [Requirement("RH4106-006")]
    public async Task FixAllSkipsTheFieldsWhoseDerivedNamesTheRuleStillReports()
    {
        const string testCode = """
                                public class C
                                {
                                    private int {|RH4106:__1st|};
                                    private int {|RH4106:注文|};
                                    private int {|RH4106:ϒ_x|};
                                    private int {|RH4106:CacheCount|};
                                }
                                """;

        const string fixedCode = """
                                 public class C
                                 {
                                     private int {|RH4106:__1st|};
                                     private int {|RH4106:注文|};
                                     private int {|RH4106:ϒ_x|};
                                     private int _cacheCount;
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     static test =>
                            {
                                // The diagnostics that keep no fix are marked in the expected source
                                test.FixedState.MarkupHandling = MarkupMode.Allow;
                                test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                                test.NumberOfFixAllIterations = 1;
                            });
    }

    /// <summary>
    /// Verifies that the fix writes a leading acronym of the field name in lowercase, and keeps the case of the acronym's
    /// last letter when a lowercase letter follows it directly
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-006")]
    [Requirement("RH4106-004")]
    public async Task FixWritesALeadingAcronymInLowercase()
    {
        const string testCode = """
                                public class C
                                {
                                    private int {|RH4106:HTTPClient|};
                                    private int {|RH4106:XMLParser|};
                                    private int {|RH4106:IOStream|};
                                    private int {|RH4106:HTTP2Client|};
                                    private int {|RH4106:ABc|};
                                    private int {|RH4106:URLs|};

                                    public int Sum => HTTPClient + XMLParser + IOStream + HTTP2Client + ABc + URLs;
                                }
                                """;

        const string fixedCode = """
                                 public class C
                                 {
                                     private int _httpClient;
                                     private int _xmlParser;
                                     private int _ioStream;
                                     private int _http2Client;
                                     private int _aBc;
                                     private int _urLs;

                                     public int Sum => _httpClient + _xmlParser + _ioStream + _http2Client + _aBc + _urLs;
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the leading run of uppercase letters the fix lowercases ends with the first word, so the words behind an
    /// underscore keep their case
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-006")]
    [Requirement("RH4106-004")]
    public async Task FixTakesTheLeadingUppercaseRunFromTheFirstWordOnly()
    {
        const string testCode = """
                                public class C
                                {
                                    private int {|RH4106:A_B|};
                                    private int {|RH4106:A_B_Cd|};
                                    private int {|RH4106:m_HTTPClient|};
                                    private int {|RH4106:X_Y_Z|};
                                    private int {|RH4106:XML_Parser|};

                                    public int Sum => A_B + A_B_Cd + m_HTTPClient + X_Y_Z + XML_Parser;
                                }
                                """;

        const string fixedCode = """
                                 public class C
                                 {
                                     private int _aB;
                                     private int _aBCd;
                                     private int _mHTTPClient;
                                     private int _xYZ;
                                     private int _xmlParser;

                                     public int Sum => _aB + _aBCd + _mHTTPClient + _xYZ + _xmlParser;
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that a field with a leading acronym, whose derived name equals the derived name of another field of the
    /// class, is skipped by Fix All together with that field while Fix All still renames a field whose derived name collides
    /// with nothing, and that a single fix renames the first colliding field and is then withheld for the second
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-010")]
    [Requirement("RH4106-007")]
    public async Task FixAllSkipsAFieldWithALeadingAcronymThatCollidesWithAnotherField()
    {
        const string testCode = """
                                public class C
                                {
                                    private int {|RH4106:HTTPClient|};
                                    private int {|RH4106:HttpClient|};
                                    private int {|RH4106:Count|};

                                    public int Sum => HTTPClient + HttpClient + Count;
                                }
                                """;

        const string fixedCode = """
                                 public class C
                                 {
                                     private int _httpClient;
                                     private int {|RH4106:HttpClient|};
                                     private int _count;

                                     public int Sum => _httpClient + HttpClient + _count;
                                 }
                                 """;

        const string batchFixedCode = """
                                      public class C
                                      {
                                          private int {|RH4106:HTTPClient|};
                                          private int {|RH4106:HttpClient|};
                                          private int _count;

                                          public int Sum => HTTPClient + HttpClient + _count;
                                      }
                                      """;

        await Verify(testCode,
                     fixedCode,
                     static test =>
                            {
                                // The diagnostics whose fix is withheld stay and are marked in the expected sources
                                test.FixedState.MarkupHandling = MarkupMode.Allow;
                                test.BatchFixedState.Sources.Add(batchFixedCode);
                                test.BatchFixedState.MarkupHandling = MarkupMode.Allow;

                                // The skipped group keeps the Fix All loop running for a second pass that changes nothing
                                test.NumberOfFixAllIterations = -2;
                            });
    }

    /// <summary>
    /// Verifies that a combining mark, a letter number, and connector punctuation other than <c>_</c> are accepted behind the
    /// prefix, while a name whose first letter behind the prefix is uppercase is still reported and keeps its combining mark
    /// in the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-003")]
    [Requirement("SHARED-CASING-006")]
    public async Task UnderlineCamelCaseAcceptsCombiningMarksLetterNumbersAndConnectorPunctuation()
    {
        const string acute = "\u0301";
        const string twelve = "\u216B";
        const string undertie = "\u203F";

        const string testCode = $$"""
                                  public class C
                                  {
                                      private int _cafe{{acute}};
                                      private int _class{{twelve}};
                                      private int _order{{undertie}}line;
                                      private int {|RH4106:_Re{{acute}}sume|};

                                      public int Sum => _cafe{{acute}} + _class{{twelve}} + _order{{undertie}}line + _Re{{acute}}sume;
                                  }
                                  """;

        const string fixedCode = $$"""
                                   public class C
                                   {
                                       private int _cafe{{acute}};
                                       private int _class{{twelve}};
                                       private int _order{{undertie}}line;
                                       private int _re{{acute}}sume;

                                       public int Sum => _cafe{{acute}} + _class{{twelve}} + _order{{undertie}}line + _re{{acute}}sume;
                                   }
                                   """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies, one expression at a time, that the fix renames the field and qualifies its reference with
    /// <see langword="this"/> when a local of the method has the field's new name, because the renamer only renames and
    /// qualifies the reference and changes no other code
    /// </summary>
    /// <param name="expression">The returned expression, which reads the field and the local</param>
    /// <param name="fixedExpression">The returned expression after the rename</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    [DataRow("Value + _value + other", "this._value + _value + other")]
    [DataRow("Value + _value + 1", "this._value + _value + 1")]
    [DataRow("Value + _value + _value", "this._value + _value + _value")]
    [DataRow("Value * _value * Value", "this._value * _value * this._value")]
    [DataRow("Value + _value", "this._value + _value")]
    [DataRow("_value + Value + other", "_value + this._value + other")]
    [DataRow("other + Value + _value", "other + this._value + _value")]
    [DataRow("Value + _value - Value", "this._value + _value - this._value")]
    public async Task FixQualifiesTheFieldReferenceNextToALocalWithTheNewName(string expression, string fixedExpression)
    {
        var testCode = $$"""
                         class Sample
                         {
                             private int {|RH4106:Value|};

                             int Get(int other)
                             {
                                 var _value = 1;

                                 return {{expression}};
                             }
                         }
                         """;

        var fixedCode = $$"""
                          class Sample
                          {
                              private int _value;

                              int Get(int other)
                              {
                                  var _value = 1;

                                  return {{fixedExpression}};
                              }
                          }
                          """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix renames the field and qualifies its reference with <see langword="this"/> when a
    /// deconstruction variable of the method has the field's new name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    public async Task FixQualifiesTheFieldReferenceNextToADeconstructionVariableWithTheNewName()
    {
        const string testCode = """
                                class Sample
                                {
                                    private int {|RH4106:Value|};

                                    int Get((int, int) pair)
                                    {
                                        var (_value, other) = pair;

                                        return Value + _value + other;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class Sample
                                 {
                                     private int _value;

                                     int Get((int, int) pair)
                                     {
                                         var (_value, other) = pair;

                                         return this._value + _value + other;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix renames the field and qualifies its reference with <see langword="this"/> when a
    /// parameter of the method has the field's new name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    public async Task FixQualifiesTheFieldReferenceNextToAParameterWithTheNewName()
    {
        const string testCode = """
                                class Sample
                                {
                                    private int {|RH4106:Value|};

                                    int Get(int _value, int other)
                                    {
                                        return Value + _value + other;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class Sample
                                 {
                                     private int _value;

                                     int Get(int _value, int other)
                                     {
                                         return this._value + _value + other;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix renames the field and qualifies its reference with <see langword="this"/> inside an
    /// argument of a call when a local of the method has the field's new name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    public async Task FixQualifiesTheFieldReferenceInsideAnArgumentNextToALocalWithTheNewName()
    {
        const string testCode = """
                                class Box
                                {
                                    public void Write(object value)
                                    {
                                    }
                                }

                                class Sample
                                {
                                    private int {|RH4106:Value|};

                                    void Put(Box box)
                                    {
                                        var _value = 1;

                                        box.Write(Value + _value + 2);
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class Box
                                 {
                                     public void Write(object value)
                                     {
                                     }
                                 }

                                 class Sample
                                 {
                                     private int _value;

                                     void Put(Box box)
                                     {
                                         var _value = 1;

                                         box.Write(this._value + _value + 2);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies, one expression at a time, that the fix renames a static field and qualifies its reference with its
    /// type when a local of the method has the field's new name
    /// </summary>
    /// <param name="expression">The returned expression, which reads the field and the local</param>
    /// <param name="fixedExpression">The returned expression after the rename</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    [DataRow("Value + _value + other", "Sample._value + _value + other")]
    [DataRow("Value + _value", "Sample._value + _value")]
    public async Task FixQualifiesTheStaticFieldReferenceWithItsTypeNextToALocalWithTheNewName(string expression, string fixedExpression)
    {
        var testCode = $$"""
                         class Sample
                         {
                             private static int {|RH4106:Value|};

                             static int Get(int other)
                             {
                                 var _value = 1;

                                 return {{expression}};
                             }
                         }
                         """;

        var fixedCode = $$"""
                          class Sample
                          {
                              private static int _value;

                              static int Get(int other)
                              {
                                  var _value = 1;

                                  return {{fixedExpression}};
                              }
                          }
                          """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix renames a static field and qualifies its reference with its type written with its namespace when
    /// a local has the field's new name and a nested type of the referencing type has the name of the field's type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    public async Task FixQualifiesTheStaticFieldReferenceWithItsTypeAndNamespaceWhenANestedTypeHidesTheTypeName()
    {
        const string testCode = """
                                namespace N
                                {
                                    class Sample
                                    {
                                        private static int {|RH4106:Value|};

                                        class Inner
                                        {
                                            class Sample
                                            {
                                            }

                                            static int Get(int other)
                                            {
                                                var _value = 1;

                                                return Value + _value + other;
                                            }
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace N
                                 {
                                     class Sample
                                     {
                                         private static int _value;

                                         class Inner
                                         {
                                             class Sample
                                             {
                                             }

                                             static int Get(int other)
                                             {
                                                 var _value = 1;

                                                 return N.Sample._value + _value + other;
                                             }
                                         }
                                     }
                                 }
                                 """;

        // The renamer builds the multi-part qualifier as a qualified name, while the same text parses as a chain of member
        // accesses; the fixed text is compared, the syntax structure of the fix result is not
        await Verify(testCode, fixedCode, test => test.CodeActionValidationMode = CodeActionValidationMode.None);
    }

    /// <summary>
    /// Verifies that the fix renames a static field and qualifies its reference with its type behind <c>global::</c> when a
    /// local has the field's new name and a nested type of the referencing type has the name of the field's type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    public async Task FixQualifiesTheStaticFieldReferenceWithGlobalAndItsTypeWhenANestedTypeHidesTheTypeName()
    {
        const string testCode = """
                                class Sample
                                {
                                    private static int {|RH4106:Value|};

                                    class Inner
                                    {
                                        class Sample
                                        {
                                        }

                                        static int Get(int other)
                                        {
                                            var _value = 1;

                                            return Value + _value + other;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class Sample
                                 {
                                     private static int _value;

                                     class Inner
                                     {
                                         class Sample
                                         {
                                         }

                                         static int Get(int other)
                                         {
                                             var _value = 1;

                                             return global::Sample._value + _value + other;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix renames the field and qualifies the assigned reference with <see langword="this"/> when a
    /// local of the method has the field's new name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    public async Task FixQualifiesTheAssignedFieldReferenceNextToALocalWithTheNewName()
    {
        const string testCode = """
                                class Sample
                                {
                                    private int {|RH4106:Value|};

                                    void Set(int other)
                                    {
                                        var _value = other;

                                        Value = _value;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class Sample
                                 {
                                     private int _value;

                                     void Set(int other)
                                     {
                                         var _value = other;

                                         this._value = _value;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that Fix All renames two fields and qualifies the references of both with <see langword="this"/> when
    /// locals of the method have their new names
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-009")]
    public async Task FixAllQualifiesTheReferencesOfEveryFieldNextToLocalsWithTheNewNames()
    {
        const string testCode = """
                                class Sample
                                {
                                    private int {|RH4106:Value|};

                                    private int {|RH4106:Count|};

                                    int Get(int other)
                                    {
                                        var _value = 1;
                                        var _count = 2;

                                        return Value + _value + other + Count + _count + other;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class Sample
                                 {
                                     private int _value;

                                     private int _count;

                                     int Get(int other)
                                     {
                                         var _value = 1;
                                         var _count = 2;

                                         return this._value + _value + other + this._count + _count + other;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix keeps the name of an anonymous object member that takes its name from the field and is read by
    /// that name, so the read still compiles
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-015")]
    public async Task FixKeepsTheNameOfAnAnonymousObjectMemberThatTakesItsNameFromTheField()
    {
        const string testCode = """
                                class Sample
                                {
                                    private int {|RH4106:Value|};

                                    int Get()
                                    {
                                        var x = new { Value };

                                        return x.Value;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class Sample
                                 {
                                     private int _value;

                                     int Get()
                                     {
                                         var x = new { Value = _value };

                                         return x.Value;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix keeps the name of an anonymous object member whose whole expression is a member access that ends
    /// with the field, while a member with a name of its own is only renamed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-015")]
    public async Task FixKeepsTheNameOfAnAnonymousObjectMemberNamedByAMemberAccess()
    {
        const string testCode = """
                                class C
                                {
                                    private int {|RH4106:Value|};

                                    int M(C other)
                                    {
                                        var item = new { this.Value, Other = other?.Value };

                                        return item.Value + (item.Other ?? 0);
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     private int _value;

                                     int M(C other)
                                     {
                                         var item = new { Value = this._value, Other = other?._value };

                                         return item.Value + (item.Other ?? 0);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix keeps the names of anonymous object members named by a member access and by the field itself
    /// in two methods of one type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-015")]
    public async Task FixKeepsTheNamesOfAnonymousObjectMembersInEveryMethod()
    {
        const string testCode = """
                                class C
                                {
                                    private int {|RH4106:Value|};

                                    string M()
                                    {
                                        return new { this.Value }.ToString();
                                    }

                                    int N()
                                    {
                                        var item = new { Value };

                                        return item.Value;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     private int _value;

                                     string M()
                                     {
                                         return new { Value = this._value }.ToString();
                                     }

                                     int N()
                                     {
                                         var item = new { Value = _value };

                                         return item.Value;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix keeps the name of an anonymous object member whose whole expression is a conditional access
    /// that ends with the field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-015")]
    public async Task FixKeepsTheNameOfAnAnonymousObjectMemberNamedByAConditionalAccess()
    {
        const string testCode = """
                                class C
                                {
                                    private int {|RH4106:Value|};

                                    string M(C other)
                                    {
                                        return new { other?.Value }.ToString();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     private int _value;

                                     string M(C other)
                                     {
                                         return new { Value = other?._value }.ToString();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix keeps the name of an anonymous object member whose whole expression is a conditional access,
    /// a null-forgiving member access, or a nested conditional access that ends with the field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-015")]
    public async Task FixKeepsTheNameOfAnAnonymousObjectMemberNamedByTheTailOfAConditionalAccess()
    {
        const string testCode = """
                                class C
                                {
                                    private int {|RH4106:Value|};
                                    private C _next;

                                    string M(C other)
                                    {
                                        var a = new { other?._next.Value };
                                        var b = new { other?._next?.Value };
                                        var c = new { other!.Value };

                                        return a.Value.ToString() + b.Value + c.Value + _next;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     private int _value;
                                     private C _next;

                                     string M(C other)
                                     {
                                         var a = new { Value = other?._next._value };
                                         var b = new { Value = other?._next?._value };
                                         var c = new { Value = other!._value };

                                         return a.Value.ToString() + b.Value + c.Value + _next;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix writes no name for an anonymous object member whose name the compiler takes from another
    /// identifier, because the field is the receiver of a conditional access in the member's expression
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-015")]
    public async Task FixWritesNoNameForAnAnonymousObjectMemberWhoseConditionalAccessReceiverIsTheField()
    {
        const string testCode = """
                                class C
                                {
                                    private string {|RH4106:Text|};
                                    private C _next;

                                    string M(C other)
                                    {
                                        var d = new { Text?.Length };
                                        var e = new { this.Text?.Length };
                                        var f = new { other?._next.Text.Length };

                                        return d.Length.ToString() + e.Length + f.Length + _next;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 class C
                                 {
                                     private string _text;
                                     private C _next;

                                     string M(C other)
                                     {
                                         var d = new { _text?.Length };
                                         var e = new { this._text?.Length };
                                         var f = new { other?._next._text.Length };

                                         return d.Length.ToString() + e.Length + f.Length + _next;
                                     }
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
                                  Diagnostics(RH4106PrivateFieldCasingAnalyzer.DiagnosticId, MessageFormat, 3),
                                  config =>
                                  {
                                      // The two colliding fields are the one shape where the two paths differ: applying the fixes one
                                      // at a time corrects the first and then refuses the second, while Fix All drops the whole
                                      // duplicate-target group and corrects neither
                                      config.BatchFixedCode = batchFixedCode;

                                      config.FixedState.ExpectedDiagnostics.Add(Diagnostic(RH4106PrivateFieldCasingAnalyzer.DiagnosticId).WithLocation(1, DiagnosticLocationOptions.InterpretAsMarkupKey).WithMessage(MessageFormat));

                                      config.BatchFixedState.ExpectedDiagnostics.Add(Diagnostic(RH4106PrivateFieldCasingAnalyzer.DiagnosticId).WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey).WithMessage(MessageFormat));
                                      config.BatchFixedState.ExpectedDiagnostics.Add(Diagnostic(RH4106PrivateFieldCasingAnalyzer.DiagnosticId).WithLocation(1, DiagnosticLocationOptions.InterpretAsMarkupKey).WithMessage(MessageFormat));

                                      // The two skipped candidates keep the batch loop running for a second, no-op pass
                                      config.NumberOfFixAllIterations = -2;
                                  });
    }

    #endregion // BatchCodeFixTestsBase
}