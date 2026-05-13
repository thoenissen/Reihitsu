using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Analyzer;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Analyzer;

/// <summary>
/// Test methods for <see cref="RH0702FileMustStartWithConfiguredCopyrightHeaderAnalyzer"/> and <see cref="RH0702FileMustStartWithConfiguredCopyrightHeaderCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0702FileMustStartWithConfiguredCopyrightHeaderAnalyzerTests : AnalyzerTestsBase<RH0702FileMustStartWithConfiguredCopyrightHeaderAnalyzer, RH0702FileMustStartWithConfiguredCopyrightHeaderCodeFixProvider>
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
    /// Valid configured header
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ValidConfiguredHeader()
    {
        const string testData = """
                                // Copyright (c) Example Software.
                                namespace TestNamespace
                                {
                                }
                                """;

        await Verify(testData,
                     test =>
                     {
                         const string configuration = """
                                                      {
                                                         "Copyright":{
                                                            "copyrightText":"// Copyright (c) {companyName}.",
                                                            "companyName":"Example Software"
                                                         }
                                                      }
                                                      """;

                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                     });
    }

    /// <summary>
    /// Missing configured header
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task MissingConfiguredHeader()
    {
        const string fixedCode = """
                                 // Copyright (c) Example Software.
                                 namespace TestNamespace
                                 {
                                 }
                                 """;

        await Verify(TestCode,
                     fixedCode,
                     test =>
                     {
                         const string configuration = """
                                                      {
                                                         "Copyright":{
                                                            "copyrightText":"// Copyright (c) {companyName}.",
                                                            "companyName":"Example Software"
                                                         }
                                                      }
                                                      """;

                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                         test.FixedState.AdditionalFiles.Add(("reihitsu.json", configuration));
                         test.TestBehaviors |= TestBehaviors.SkipSuppressionCheck;
                     },
                     Diagnostic(RH0702FileMustStartWithConfiguredCopyrightHeaderAnalyzer.DiagnosticId).WithSpan(1, 1, 1, 1).WithMessage(AnalyzerResources.RH0702MessageFormat));
    }

    /// <summary>
    /// Mismatched configured header
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task MismatchedConfiguredHeader()
    {
        const string testData = """
                                // Copyright (c) Other Company.
                                namespace TestNamespace
                                {
                                }
                                """;
        const string fixedCode = """
                                 // Copyright (c) Example Software.
                                 namespace TestNamespace
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedCode,
                     test =>
                     {
                         const string configuration = """
                                                      {
                                                         "Copyright":{
                                                            "copyrightText":"// Copyright (c) {companyName}.",
                                                            "companyName":"Example Software"
                                                         }
                                                      }
                                                      """;

                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                         test.FixedState.AdditionalFiles.Add(("reihitsu.json", configuration));
                         test.TestBehaviors |= TestBehaviors.SkipSuppressionCheck;
                     },
                     Diagnostic(RH0702FileMustStartWithConfiguredCopyrightHeaderAnalyzer.DiagnosticId).WithSpan(1, 1, 1, 1).WithMessage(AnalyzerResources.RH0702MessageFormat));
    }

    /// <summary>
    /// Supports file name placeholder
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task SupportsFileNamePlaceholder()
    {
        const string testData = """
                                // Test0.cs
                                namespace TestNamespace
                                {
                                }
                                """;

        await Verify(testData,
                     test =>
                     {
                         const string configuration = """
                                                      {
                                                         "Copyright":{
                                                            "copyrightText":"// {fileName}"
                                                         }
                                                      }
                                                      """;

                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                     });
    }

    /// <summary>
    /// No configuration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NoConfiguration()
    {
        await Verify(TestCode);
    }

    #endregion // Tests
}