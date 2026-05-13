using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Test methods for <see cref="RH0452FileMustStartWithConfiguredXmlStyleCopyrightHeaderAnalyzer"/> and <see cref="RH0452FileMustStartWithConfiguredXmlStyleCopyrightHeaderCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0452FileMustStartWithConfiguredXmlStyleCopyrightHeaderAnalyzerTests : AnalyzerTestsBase<RH0452FileMustStartWithConfiguredXmlStyleCopyrightHeaderAnalyzer, RH0452FileMustStartWithConfiguredXmlStyleCopyrightHeaderCodeFixProvider>
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
                                // <copyright file="Test0.cs" company="Example Software">
                                // Copyright (c) Example Software. All rights reserved.
                                // </copyright>
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
                                                            "copyrightText":"// <copyright file=\"{fileName}\" company=\"{companyName}\">\n// Copyright (c) {companyName}. All rights reserved.\n// </copyright>",
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
                                 // <copyright file="Test0.cs" company="Example Software">
                                 // Copyright (c) Example Software. All rights reserved.
                                 // </copyright>
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
                                                            "copyrightText":"// <copyright file=\"{fileName}\" company=\"{companyName}\">\n// Copyright (c) {companyName}. All rights reserved.\n// </copyright>",
                                                            "companyName":"Example Software"
                                                         }
                                                      }
                                                      """;

                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                         test.FixedState.AdditionalFiles.Add(("reihitsu.json", configuration));
                         test.TestBehaviors |= TestBehaviors.SkipSuppressionCheck;
                     },
                     Diagnostic(RH0452FileMustStartWithConfiguredXmlStyleCopyrightHeaderAnalyzer.DiagnosticId).WithSpan(1, 1, 1, 1).WithMessage(AnalyzerResources.RH0452MessageFormat));
    }

    /// <summary>
    /// Mismatched configured header
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task MismatchedConfiguredHeader()
    {
        const string testData = """
                                // <copyright file="Test0.cs" company="Other Company">
                                // Copyright (c) Other Company. All rights reserved.
                                // </copyright>
                                namespace TestNamespace
                                {
                                }
                                """;
        const string fixedCode = """
                                 // <copyright file="Test0.cs" company="Example Software">
                                 // Copyright (c) Example Software. All rights reserved.
                                 // </copyright>
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
                                                            "copyrightText":"// <copyright file=\"{fileName}\" company=\"{companyName}\">\n// Copyright (c) {companyName}. All rights reserved.\n// </copyright>",
                                                            "companyName":"Example Software"
                                                         }
                                                      }
                                                      """;

                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                         test.FixedState.AdditionalFiles.Add(("reihitsu.json", configuration));
                         test.TestBehaviors |= TestBehaviors.SkipSuppressionCheck;
                     },
                     Diagnostic(RH0452FileMustStartWithConfiguredXmlStyleCopyrightHeaderAnalyzer.DiagnosticId).WithSpan(1, 1, 1, 1).WithMessage(AnalyzerResources.RH0452MessageFormat));
    }

    /// <summary>
    /// Supports file name placeholder
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task SupportsFileNamePlaceholder()
    {
        const string testData = """
                                // <copyright file="Test0.cs" company="Example Software">
                                // Copyright (c) Example Software. All rights reserved.
                                // </copyright>
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
                                                            "copyrightText":"// <copyright file=\"{fileName}\" company=\"{companyName}\">\n// Copyright (c) {companyName}. All rights reserved.\n// </copyright>",
                                                            "companyName":"Example Software"
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