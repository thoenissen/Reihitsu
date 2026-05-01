using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH0227NamespaceNotAllowedAnalyzer"/>
/// </summary>
[TestClass]
public class RH0227NamespaceNotAllowedAnalyzerTests : AnalyzerTestsBase<RH0227NamespaceNotAllowedAnalyzer>
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
    public async Task InvalidNamespace()
    {
        await Verify(TestCode,
                     test =>
                     {
                         const string configuration = """
                                                      {
                                                         "Naming":{
                                                            "AllowedNamespaceDeclarations":[
                                                               "NamespaceName"
                                                            ]
                                                         }
                                                      }
                                                      """;

                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                     },
                     Diagnostic(RH0227NamespaceNotAllowedAnalyzer.DiagnosticId).WithSpan(3, 11, 3, 24).WithMessage(AnalyzerResources.RH0227MessageFormat));
    }

    /// <summary>
    /// Valid namespace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ValidNamespace()
    {
        await Verify(TestCode,
                     test =>
                     {
                         const string configuration = """
                                                      {
                                                         "Naming":{
                                                            "AllowedNamespaceDeclarations":[
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
    public async Task NoConfiguration()
    {
        await Verify(TestCode);
    }

    /// <summary>
    /// Empty namespace configuration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task EmptyConfiguration()
    {
        await Verify(TestCode,
                     test =>
                     {
                         const string configuration = """
                                                      {
                                                         "Naming":{
                                                            "AllowedNamespaceDeclarations":[
                                                            ]
                                                         }
                                                      }
                                                      """;

                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                     });
    }

    #endregion // Methods
}