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
                     Diagnostic(RH4009NamespaceNotAllowedAnalyzer.DiagnosticId).WithSpan(3, 11, 3, 24).WithMessage(AnalyzerResources.RH4009MessageFormat));
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

    #endregion // Methods
}