using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4009NamespaceNotAllowedAnalyzer"/>
/// </summary>
[TestClass]
public class RH4009NamespaceNotAllowedAnalyzerTests : AnalyzerTestsBase<RH4009NamespaceNotAllowedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Test code for namespace validation
    /// </summary>
    private const string TestCode = """
                                    using System;

                                    namespace TestNameSpace
                                    {
                                    }
                                    """;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Invalid namespace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4009-002")]
    public async Task InvalidNamespace()
    {
        await Verify(TestCode,
                     test =>
                     {
                         const string configuration = """
                                                      {
                                                         "naming":{
                                                            "allowedNamespaceDeclarations":[
                                                               "NamespaceName"
                                                            ]
                                                         }
                                                      }
                                                      """;

                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                     },
                     Diagnostic(RH4009NamespaceNotAllowedAnalyzer.DiagnosticId).WithSpan(3, 11, 3, 24).WithMessage(MessageFormat));
    }

    /// <summary>
    /// Valid namespace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4009-002")]
    public async Task ValidNamespace()
    {
        await Verify(TestCode,
                     test =>
                     {
                         const string configuration = """
                                                      {
                                                         "naming":{
                                                            "allowedNamespaceDeclarations":[
                                                               "TestNameSpace"
                                                            ]
                                                         }
                                                      }
                                                      """;

                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                     });
    }

    /// <summary>
    /// No configuration file
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4009-001")]
    public async Task NoConfiguration()
    {
        const string testCode = """
                                namespace Company.Product
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Empty namespace configuration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4009-001")]
    public async Task EmptyConfiguration()
    {
        const string testCode = """
                                namespace Company.Product
                                {
                                }
                                """;

        await Verify(testCode,
                     test =>
                     {
                         const string configuration = """
                                                      {
                                                         "naming":{
                                                            "allowedNamespaceDeclarations":[
                                                            ]
                                                         }
                                                      }
                                                      """;

                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                     });
    }

    /// <summary>
    /// Invalid configuration json
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4009-001")]
    public async Task InvalidConfiguration()
    {
        const string testCode = """
                                namespace Company.Product
                                {
                                }
                                """;

        await Verify(testCode,
                     test =>
                     {
                         const string configuration = """
                                                      {
                                                         "naming":{
                                                            "allowedNamespaceDeclarations":[
                                                               "TestNameSpace"
                                                         }
                                                      }
                                                      """;

                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                     });
    }

    /// <summary>
    /// Verifies that nothing is reported when the naming section has no allow-list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4009-001")]
    public async Task NamingSectionWithoutAllowListIsInactive()
    {
        const string testCode = """
                                namespace Company.Product
                                {
                                }
                                """;

        await Verify(testCode,
                     test =>
                     {
                         const string configuration = """
                                                      { "naming": { } }
                                                      """;

                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                     });
    }

    /// <summary>
    /// Verifies that nothing is reported when the allow-list has no entry that is a string
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4009-001")]
    public async Task AllowListWithoutStringEntryIsInactive()
    {
        const string testCode = """
                                namespace Company.Product
                                {
                                }
                                """;

        await Verify(testCode,
                     test =>
                     {
                         const string configuration = """
                                                      { "naming": { "allowedNamespaceDeclarations": [ 1 ] } }
                                                      """;

                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                     });
    }

    /// <summary>
    /// Verifies that an entry that is not a string is ignored, while the string entries of the list keep the rule active
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4009-001")]
    public async Task EntriesThatAreNotStringsAreIgnoredAndStringEntriesStayInEffect()
    {
        const string testCode = """
                                namespace {|RH4009:Company.Product|}
                                {
                                }
                                """;

        await Verify(testCode,
                     test =>
                     {
                         const string configuration = """
                                                      { "naming": { "allowedNamespaceDeclarations": [ "A", 1 ] } }
                                                      """;

                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                     });
    }

    /// <summary>
    /// Verifies that a child namespace, a namespace in other casing, and a file-scoped namespace of another file are reported on the whole name, while the listed namespace is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4009-002")]
    public async Task NamespacesNotExactlyOnTheListAreReportedInEveryFile()
    {
        const string testCode = """
                                namespace MyCompany.Project
                                {
                                }

                                namespace {|RH4009:MyCompany.Project.Data|}
                                {
                                }

                                namespace {|RH4009:mycompany.project|}
                                {
                                }
                                """;

        const string otherFileCode = """
                                     namespace {|RH4009:MyCompany.Internal|};
                                     """;

        await Verify(testCode,
                     test =>
                     {
                         const string configuration = """
                                                      { "naming": { "allowedNamespaceDeclarations": [ "MyCompany.Project" ] } }
                                                      """;

                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                         test.TestState.Sources.Add(("/0/Test1.cs", otherFileCode));
                     });
    }

    /// <summary>
    /// Verifies that nothing is reported when the configuration file ends inside its root object
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4009-001")]
    public async Task ConfigurationMalformedInsideItsRootObjectIsInactive()
    {
        const string testCode = """
                                namespace Company.Product
                                {
                                }
                                """;

        await Verify(testCode,
                     test =>
                     {
                         const string configuration = """
                                                      { "naming": { "allowedNamespaceDeclarations": [ "A" ]
                                                      """;

                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                     });
    }

    /// <summary>
    /// Verifies the specification example: the verbatim prefix and whitespace or comments inside a namespace name do
    /// not change the name, and a name that is not on the list is reported on its whole name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4009-002")]
    public async Task NamespaceNamesAreComparedByTheirValue()
    {
        const string testCode = """
                                namespace @MyCompany.Project
                                {
                                }

                                namespace MyCompany . Project
                                {
                                }

                                namespace MyCompany./* x */Project
                                {
                                }

                                namespace {|RH4009:@MyCompany . Data|}
                                {
                                }
                                """;

        await VerifyWithAllowList(testCode, "MyCompany.Project");
    }

    /// <summary>
    /// Verifies that the verbatim prefix on the first part of a namespace name does not change the name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4009-002")]
    public async Task VerbatimPrefixOnFirstPartIsNotPartOfTheName()
    {
        const string testCode = """
                                namespace @MyCompany.Project
                                {
                                }
                                """;

        await VerifyWithAllowList(testCode, "MyCompany.Project");
    }

    /// <summary>
    /// Verifies that the verbatim prefix on a later part of a namespace name does not change the name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4009-002")]
    public async Task VerbatimPrefixOnLaterPartIsNotPartOfTheName()
    {
        const string testCode = """
                                namespace MyCompany.@Project
                                {
                                }
                                """;

        await VerifyWithAllowList(testCode, "MyCompany.Project");
    }

    /// <summary>
    /// Verifies that spaces around the dot of a namespace name do not change the name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4009-002")]
    public async Task SpacesAroundDotAreNotPartOfTheName()
    {
        const string testCode = """
                                namespace MyCompany . Project
                                {
                                }
                                """;

        await VerifyWithAllowList(testCode, "MyCompany.Project");
    }

    /// <summary>
    /// Verifies that a comment between the parts of a namespace name does not change the name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4009-002")]
    public async Task CommentBetweenPartsIsNotPartOfTheName()
    {
        const string testCode = """
                                namespace MyCompany./* x */Project
                                {
                                }
                                """;

        await VerifyWithAllowList(testCode, "MyCompany.Project");
    }

    /// <summary>
    /// Verifies that a line break between the parts of a namespace name does not change the name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4009-002")]
    public async Task LineBreakBetweenPartsIsNotPartOfTheName()
    {
        const string testCode = """
                                namespace MyCompany
                                    .Project
                                {
                                }
                                """;

        await VerifyWithAllowList(testCode, "MyCompany.Project");
    }

    /// <summary>
    /// Verifies that the verbatim prefix and whitespace in the name of a file-scoped namespace do not change the name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4009-002")]
    public async Task FileScopedNamespaceNameIsComparedByItsValue()
    {
        const string testCode = """
                                namespace @MyCompany . Project;
                                """;

        await VerifyWithAllowList(testCode, "MyCompany.Project");
    }

    /// <summary>
    /// Verifies that a file-scoped namespace written with the verbatim prefix and whitespace that is not on the list is
    /// reported on its whole name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4009-002")]
    public async Task FileScopedNamespaceNotOnTheListIsReportedOnItsWholeWrittenName()
    {
        const string testCode = """
                                namespace {|RH4009:@MyCompany . Data|};
                                """;

        await VerifyWithAllowList(testCode, "MyCompany.Project");
    }

    /// <summary>
    /// Verifies that a namespace name written with a Unicode escape is compared by the character the escape stands for
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4009-002")]
    public async Task UnicodeEscapeInNamespaceNameIsReplacedByItsCharacter()
    {
        const string testCode = """
                                namespace \u0041.B
                                {
                                }
                                """;

        await VerifyWithAllowList(testCode, "A.B");
    }

    /// <summary>
    /// Verifies that an entry is compared as it is written, so an entry with the verbatim prefix matches no namespace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4009-002")]
    public async Task EntryWithVerbatimPrefixMatchesNoNamespace()
    {
        const string testCode = """
                                namespace {|RH4009:MyCompany.Project|}
                                {
                                }

                                namespace {|RH4009:@MyCompany.Project|}
                                {
                                }
                                """;

        await VerifyWithAllowList(testCode, "@MyCompany.Project");
    }

    /// <summary>
    /// Verifies that nothing is reported when the configuration file holds a token after its root object
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4009-001")]
    public async Task ConfigurationWithTokenAfterItsRootObjectIsInactive()
    {
        const string testCode = """
                                namespace Company.Product
                                {
                                }
                                """;

        await Verify(testCode,
                     test =>
                     {
                         const string configuration = """
                                                      { "naming": { "allowedNamespaceDeclarations": [ "A" ] } } junk
                                                      """;

                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                     });
    }

    /// <summary>
    /// Verifies that nothing is reported when the configuration file holds a second value after its root object
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4009-001")]
    public async Task ConfigurationWithSecondValueAfterItsRootObjectIsInactive()
    {
        const string testCode = """
                                namespace Company.Product
                                {
                                }
                                """;

        await Verify(testCode,
                     test =>
                     {
                         const string configuration = """
                                                      { "naming": { "allowedNamespaceDeclarations": [ "A" ] } } { }
                                                      """;

                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                     });
    }

    /// <summary>
    /// Verifies that nothing is reported when the configuration file holds a comment after its root object
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4009-001")]
    public async Task ConfigurationWithCommentAfterItsRootObjectIsInactive()
    {
        const string testCode = """
                                namespace Company.Product
                                {
                                }
                                """;

        await Verify(testCode,
                     test =>
                     {
                         const string configuration = """
                                                      { "naming": { "allowedNamespaceDeclarations": [ "A" ] } } // c
                                                      """;

                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                     });
    }

    /// <summary>
    /// Verifies that JSON white space after the root object keeps the configuration in effect
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4009-001")]
    public async Task ConfigurationWithOnlyWhiteSpaceAfterItsRootObjectStaysInEffect()
    {
        const string testCode = """
                                namespace {|RH4009:Company.Product|}
                                {
                                }
                                """;

        await Verify(testCode,
                     test =>
                     {
                         const string configuration = "{ \"naming\": { \"allowedNamespaceDeclarations\": [ \"A\" ] } } \t\r\n\n  ";

                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                     });
    }

    /// <summary>
    /// Verifies the specification example: a nested namespace whose full name is on the list is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4009-003")]
    public async Task NestedNamespaceWhoseFullNameIsOnTheListIsNotReported()
    {
        const string testCode = """
                                namespace A
                                {
                                    namespace B
                                    {
                                    }
                                }
                                """;

        await VerifyWithAllowListEntries(testCode, "A", "A.B");
    }

    /// <summary>
    /// Verifies the specification example: a nested namespace whose own name, but not its full name, is on the list is reported on its own name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4009-003")]
    public async Task NestedNamespaceWhoseOwnNameOnlyIsOnTheListIsReported()
    {
        const string testCode = """
                                namespace A
                                {
                                    namespace {|RH4009:B|}
                                    {
                                    }
                                }
                                """;

        await VerifyWithAllowListEntries(testCode, "A", "B");
    }

    /// <summary>
    /// Verifies the specification example: a namespace declared at file level is not nested, so it is compared by its own name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4009-003")]
    public async Task FileLevelNamespaceNextToAnotherIsComparedByItsOwnName()
    {
        const string testCode = """
                                namespace A
                                {
                                }

                                namespace B
                                {
                                }
                                """;

        await VerifyWithAllowListEntries(testCode, "A", "B");
    }

    /// <summary>
    /// Verifies the specification example: dotted names and several levels of nesting are joined into the full name, and only the nested namespace whose full name is not on the list is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4009-003")]
    public async Task NamespacesNestedOnSeveralLevelsAreComparedByTheirFullNames()
    {
        const string testCode = """
                                namespace A
                                {
                                    namespace B.C
                                    {
                                        namespace D
                                        {
                                        }

                                        namespace {|RH4009:E|}
                                        {
                                        }
                                    }
                                }
                                """;

        await VerifyWithAllowListEntries(testCode, "A", "A.B.C", "A.B.C.D");
    }

    /// <summary>
    /// Verifies the specification example: each enclosing namespace is compared on its own, so the outer namespace is reported and the nested one whose full name is on the list is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4009-003")]
    public async Task EnclosingNamespaceIsComparedOnItsOwn()
    {
        const string testCode = """
                                namespace {|RH4009:A|}
                                {
                                    namespace B
                                    {
                                    }
                                }
                                """;

        await VerifyWithAllowListEntries(testCode, "A.B");
    }

    /// <summary>
    /// Verifies that an intermediate enclosing namespace is compared by its own full name, so it is reported while the namespace nested in it, whose full name is on the list, is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4009-003")]
    public async Task IntermediateEnclosingNamespaceIsComparedByItsFullName()
    {
        const string testCode = """
                                namespace A
                                {
                                    namespace {|RH4009:B|}
                                    {
                                        namespace C
                                        {
                                        }
                                    }
                                }
                                """;

        await VerifyWithAllowListEntries(testCode, "A", "A.B.C");
    }

    /// <summary>
    /// Verifies that a nested namespace with a dotted name whose full name is not on the list is reported on its whole own name, also when
    /// its own name is on the list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4009-003")]
    public async Task NestedNamespaceIsReportedOnItsWholeOwnName()
    {
        const string testCode = """
                                namespace A
                                {
                                    namespace {|RH4009:B.C|}
                                    {
                                    }
                                }
                                """;

        await VerifyWithAllowListEntries(testCode, "A", "B.C");
    }

    /// <summary>
    /// Verifies that the full name of a nested namespace is compared case-sensitively, and that its own name on the list does not allow it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4009-003")]
    public async Task FullNameOfNestedNamespaceIsComparedCaseSensitively()
    {
        const string testCode = """
                                namespace A
                                {
                                    namespace {|RH4009:B|}
                                    {
                                    }
                                }
                                """;

        await VerifyWithAllowListEntries(testCode, "A", "B", "a.B");
    }

    /// <summary>
    /// Verifies that the verbatim prefix on the name of an enclosing namespace is not part of the full name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4009-003")]
    public async Task VerbatimPrefixOnEnclosingNamespaceIsNotPartOfTheFullName()
    {
        const string testCode = """
                                namespace @A
                                {
                                    namespace B
                                    {
                                    }
                                }
                                """;

        await VerifyWithAllowListEntries(testCode, "A", "A.B");
    }

    /// <summary>
    /// Verifies that the verbatim prefix on the name of a nested namespace is not part of the full name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4009-003")]
    public async Task VerbatimPrefixOnNestedNamespaceIsNotPartOfTheFullName()
    {
        const string testCode = """
                                namespace A
                                {
                                    namespace @B
                                    {
                                    }
                                }
                                """;

        await VerifyWithAllowListEntries(testCode, "A", "A.B");
    }

    /// <summary>
    /// Verifies that a Unicode escape in the name of an enclosing namespace is replaced by its character in the full name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4009-003")]
    public async Task UnicodeEscapeInEnclosingNamespaceIsReplacedInTheFullName()
    {
        const string testCode = """
                                namespace \u0041
                                {
                                    namespace B
                                    {
                                    }
                                }
                                """;

        await VerifyWithAllowListEntries(testCode, "A", "A.B");
    }

    /// <summary>
    /// Verifies that whitespace and a comment inside the dotted name of an enclosing namespace are not part of the full name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4009-003")]
    public async Task WhitespaceAndCommentInEnclosingNamespaceNameAreNotPartOfTheFullName()
    {
        const string testCode = """
                                namespace A . /* x */ B
                                {
                                    namespace C
                                    {
                                    }
                                }
                                """;

        await VerifyWithAllowListEntries(testCode, "A.B", "A.B.C");
    }

    #endregion // Methods

    #region Helpers

    /// <summary>
    /// Verifies the analyzer with a configuration whose allow-list holds the given entry
    /// </summary>
    /// <param name="testCode">Test code</param>
    /// <param name="allowedNamespace">The only entry of the allow-list</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task VerifyWithAllowList(string testCode, string allowedNamespace)
    {
        await Verify(testCode,
                     test =>
                     {
                         var configuration = $$"""
                                               { "naming": { "allowedNamespaceDeclarations": [ "{{allowedNamespace}}" ] } }
                                               """;

                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                     });
    }

    /// <summary>
    /// Verifies the analyzer with a configuration whose allow-list holds the given entries
    /// </summary>
    /// <param name="testCode">Test code</param>
    /// <param name="allowedNamespaces">The entries of the allow-list</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task VerifyWithAllowListEntries(string testCode, params string[] allowedNamespaces)
    {
        await Verify(testCode,
                     test =>
                     {
                         var entries = string.Join(", ", allowedNamespaces.Select(allowedNamespace => $"\"{allowedNamespace}\""));
                         var configuration = $$"""
                                               { "naming": { "allowedNamespaceDeclarations": [ {{entries}} ] } }
                                               """;

                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                     });
    }

    #endregion // Helpers
}