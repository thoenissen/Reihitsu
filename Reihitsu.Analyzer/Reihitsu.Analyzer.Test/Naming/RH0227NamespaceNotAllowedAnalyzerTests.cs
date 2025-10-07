using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Naming.Resources;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH0227NamespaceNotAllowedAnalyzer"/>
/// </summary>
[TestClass]
public class RH0227NamespaceNotAllowedAnalyzerTests : AnalyzerTestsBase<RH0227NamespaceNotAllowedAnalyzer>
{
    /// <summary>
    /// Invalid namespace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task InvalidNamespace()
    {
        var expectedCase0 = Diagnostic().WithSpan(3, 11, 3, 24)
                                        .WithMessage(AnalyzerResources.RH0227MessageFormat);

        await VerifyCodeFixAsync(TestData.RH0227_TestData,
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
                                 expectedCase0);
    }

    /// <summary>
    /// Valid namespace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ValidNamespace()
    {
        await VerifyCodeFixAsync(TestData.RH0227_TestData,
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
        await VerifyCodeFixAsync(TestData.RH0227_TestData);
    }

    /// <summary>
    /// Empty namespace configuration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task EmptyConfiguration()
    {
        await VerifyCodeFixAsync(TestData.RH0227_TestData,
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
}