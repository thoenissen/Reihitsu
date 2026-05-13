using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Analyzer;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Analyzer;

/// <summary>
/// Tests for <see cref="RH0701ConfigurationFileMustBeValidAnalyzer"/>
/// </summary>
[TestClass]
public class RH0701ConfigurationFileMustBeValidAnalyzerTests : AnalyzerTestsBase<RH0701ConfigurationFileMustBeValidAnalyzer>
{
    #region Constants

    /// <summary>
    /// Test code
    /// </summary>
    private const string TestCode = """
                                    namespace TestNamespace
                                    {
                                    }
                                    """;

    #endregion // Constants

    #region Tests

    /// <summary>
    /// Valid configuration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ValidConfiguration()
    {
        await Verify(TestCode,
                     test =>
                     {
                         const string configuration = """
                                                      {
                                                         "Naming":{
                                                            "AllowedNamespaceDeclarations":[
                                                               "TestNamespace"
                                                            ]
                                                         }
                                                      }
                                                      """;

                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                     });
    }

    /// <summary>
    /// Empty configuration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task EmptyConfiguration()
    {
        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("reihitsu.json", string.Empty)),
                     InvalidConfiguration("The configuration file must not be empty or whitespace-only.", 1, 1));
    }

    /// <summary>
    /// Whitespace configuration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task WhitespaceConfiguration()
    {
        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("reihitsu.json", "   ")),
                     InvalidConfiguration("The configuration file must not be empty or whitespace-only.", 1, 1));
    }

    /// <summary>
    /// Invalid JSON
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task InvalidJson()
    {
        await Verify(TestCode,
                     test =>
                     {
                         const string configuration = """
                                                      {
                                                         "Naming":{
                                                            "AllowedNamespaceDeclarations":[
                                                               "TestNamespace"
                                                         }
                                                      }
                                                      """;

                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                     },
                     InvalidConfiguration("The configuration file contains invalid JSON syntax.", 5, 4));
    }

    /// <summary>
    /// Unknown top level section
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnknownTopLevelSection()
    {
        await Verify(TestCode,
                     test =>
                     {
                         const string configuration = """
                                                      {
                                                         "Unknown":{
                                                         }
                                                      }
                                                      """;

                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                     },
                     InvalidConfiguration("Unknown configuration section 'Unknown'.", 2, 5));
    }

    /// <summary>
    /// Unknown naming property
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnknownNamingProperty()
    {
        await Verify(TestCode,
                     test =>
                     {
                         const string configuration = """
                                                      {
                                                         "Naming":{
                                                            "Unknown":[
                                                            ]
                                                         }
                                                      }
                                                      """;

                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                     },
                     InvalidConfiguration("Unknown configuration setting 'Naming.Unknown'.", 3, 8));
    }

    /// <summary>
    /// Naming section must be object
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NamingSectionMustBeObject()
    {
        await Verify(TestCode,
                     test =>
                     {
                         const string configuration = """
                                                      {
                                                         "Naming":[
                                                         ]
                                                      }
                                                      """;

                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                     },
                     InvalidConfiguration("The 'Naming' section must be a JSON object.", 2, 13));
    }

    /// <summary>
    /// Allowed namespace declarations must be array
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task AllowedNamespaceDeclarationsMustBeArray()
    {
        await Verify(TestCode,
                     test =>
                     {
                         const string configuration = """
                                                      {
                                                         "Naming":{
                                                            "AllowedNamespaceDeclarations":"TestNamespace"
                                                         }
                                                      }
                                                      """;

                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                     },
                     InvalidConfiguration("The 'Naming.AllowedNamespaceDeclarations' setting must be a JSON array.", 3, 39));
    }

    /// <summary>
    /// Allowed namespace declarations entries must be strings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task AllowedNamespaceDeclarationsEntriesMustBeStrings()
    {
        await Verify(TestCode,
                     test =>
                     {
                         const string configuration = """
                                                      {
                                                         "Naming":{
                                                            "AllowedNamespaceDeclarations":[
                                                               1
                                                            ]
                                                         }
                                                      }
                                                      """;

                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                     },
                     InvalidConfiguration("Entries in 'Naming.AllowedNamespaceDeclarations' must be strings.", 4, 10));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Invalid configuration diagnostic
    /// </summary>
    /// <param name="message">Message</param>
    /// <param name="line">Line</param>
    /// <param name="column">Column</param>
    /// <returns>Diagnostic</returns>
    private static DiagnosticResult InvalidConfiguration(string message, int line, int column)
    {
        return Diagnostic(RH0701ConfigurationFileMustBeValidAnalyzer.DiagnosticId).WithLocation("reihitsu.json", line, column)
                                                                                  .WithMessage(string.Format(AnalyzerResources.RH0701MessageFormat, message));
    }

    #endregion // Methods
}