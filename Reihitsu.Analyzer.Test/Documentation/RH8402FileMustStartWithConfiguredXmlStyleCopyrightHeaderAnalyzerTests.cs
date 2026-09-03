using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Documentation;
using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Test methods for <see cref="RH8402FileMustStartWithConfiguredXmlStyleCopyrightHeaderAnalyzer"/> and <see cref="RH8402FileMustStartWithConfiguredXmlStyleCopyrightHeaderCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH8402FileMustStartWithConfiguredXmlStyleCopyrightHeaderAnalyzerTests : BatchCodeFixTestsBase<RH8402FileMustStartWithConfiguredXmlStyleCopyrightHeaderAnalyzer, RH8402FileMustStartWithConfiguredXmlStyleCopyrightHeaderCodeFixProvider>
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
    /// Valid configured line comment header
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
                     test => test.TestState.AdditionalFiles.Add(("reihitsu.json",
                                                                 CreateCopyrightConfiguration("// <copyright file=\"{fileName}\" company=\"{companyName}\">\n// Copyright (c) {companyName}. All rights reserved.\n// </copyright>",
                                                                                              ("companyName", "Example Software")))));
    }

    /// <summary>
    /// Valid configured header with additional placeholders
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ValidConfiguredHeaderWithAdditionalPlaceholders()
    {
        const string testData = """
                                // <copyright file="Test0.cs" company="Example Software">
                                // Copyright (c) Example Software. All rights reserved.
                                // Licensed under the MIT license. See the LICENSE file in the project root for full license information.
                                // </copyright>
                                namespace TestNamespace
                                {
                                }
                                """;

        await Verify(testData,
                     test => test.TestState.AdditionalFiles.Add(("reihitsu.json",
                                                                 CreateCopyrightConfiguration("// <copyright file=\"{fileName}\" company=\"{companyName}\">\n// Copyright (c) {companyName}. All rights reserved.\n// Licensed under the {licenseName} license. See the {licenseFile} file in the project root for full license information.\n// </copyright>",
                                                                                              ("companyName", "Example Software"),
                                                                                              ("licenseName", "MIT"),
                                                                                              ("licenseFile", "LICENSE")))));
    }

    /// <summary>
    /// Valid configured header with separator lines
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ValidConfiguredHeaderWithSeparatorLines()
    {
        const string testData = """
                                // ----------------------------------------------------------------------
                                // <copyright file="Test0.cs" company="Example Software">
                                // Copyright (c) Example Software. All rights reserved.
                                // </copyright>
                                // ----------------------------------------------------------------------
                                namespace TestNamespace
                                {
                                }
                                """;

        await Verify(testData,
                     test => test.TestState.AdditionalFiles.Add(("reihitsu.json",
                                                                 CreateCopyrightConfiguration("// ----------------------------------------------------------------------\n// <copyright file=\"{fileName}\" company=\"{companyName}\">\n// Copyright (c) {companyName}. All rights reserved.\n// </copyright>\n// ----------------------------------------------------------------------",
                                                                                              ("companyName", "Example Software")))));
    }

    /// <summary>
    /// Valid configured single line comment header
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ValidConfiguredHeaderWithSingleLineComment()
    {
        const string testData = """
                                // Copyright (c) Example Software. All rights reserved.
                                namespace TestNamespace
                                {
                                }
                                """;

        await Verify(testData,
                     test => test.TestState.AdditionalFiles.Add(("reihitsu.json",
                                                                 CreateCopyrightConfiguration("// Copyright (c) {companyName}. All rights reserved.",
                                                                                              ("companyName", "Example Software")))));
    }

    /// <summary>
    /// Valid configured single line block comment header
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ValidConfiguredHeaderWithSingleLineBlockComment()
    {
        const string testData = """
                                /* Copyright (c) Example Software. All rights reserved. */
                                namespace TestNamespace
                                {
                                }
                                """;

        await Verify(testData,
                     test => test.TestState.AdditionalFiles.Add(("reihitsu.json",
                                                                 CreateCopyrightConfiguration("/* Copyright (c) {companyName}. All rights reserved. */",
                                                                                              ("companyName", "Example Software")))));
    }

    /// <summary>
    /// Valid configured block comment header with asterisk lines
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ValidConfiguredHeaderWithAsteriskBlockComment()
    {
        const string testData = """
                                /* <copyright file="Test0.cs" company="Example Software">
                                 * Copyright (c) Example Software. All rights reserved.
                                 * </copyright>
                                 */
                                namespace TestNamespace
                                {
                                }
                                """;

        await Verify(testData,
                     test => test.TestState.AdditionalFiles.Add(("reihitsu.json",
                                                                 CreateCopyrightConfiguration("/* <copyright file=\"{fileName}\" company=\"{companyName}\">\n * Copyright (c) {companyName}. All rights reserved.\n * </copyright>\n */",
                                                                                              ("companyName", "Example Software")))));
    }

    /// <summary>
    /// Valid configured indented block comment header
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ValidConfiguredHeaderWithIndentedBlockComment()
    {
        const string testData = """
                                /*
                                  Copyright (c) Example Software. All rights reserved.
                                */
                                namespace TestNamespace
                                {
                                }
                                """;

        await Verify(testData,
                     test => test.TestState.AdditionalFiles.Add(("reihitsu.json",
                                                                 CreateCopyrightConfiguration("/*\n  Copyright (c) {companyName}. All rights reserved.\n*/",
                                                                                              ("companyName", "Example Software")))));
    }

    /// <summary>
    /// Missing configured header
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task MissingConfiguredHeader()
    {
        const string fixedCode = """
                                 // Copyright (c) Example Software. All rights reserved.
                                 namespace TestNamespace
                                 {
                                 }
                                 """;

        await Verify(TestCode,
                     fixedCode,
                     test =>
                     {
                         var configuration = CreateCopyrightConfiguration("// Copyright (c) {companyName}. All rights reserved.",
                                                                          ("companyName", "Example Software"));

                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                         test.FixedState.AdditionalFiles.Add(("reihitsu.json", configuration));
                         test.TestBehaviors |= TestBehaviors.SkipSuppressionCheck;
                     },
                     Diagnostic(RH8402FileMustStartWithConfiguredXmlStyleCopyrightHeaderAnalyzer.DiagnosticId).WithSpan(1, 1, 1, 1).WithMessage(AnalyzerResources.RH8402MessageFormat));
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
                         var configuration = CreateCopyrightConfiguration("// <copyright file=\"{fileName}\" company=\"{companyName}\">\n// Copyright (c) {companyName}. All rights reserved.\n// </copyright>",
                                                                          ("companyName", "Example Software"));

                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                         test.FixedState.AdditionalFiles.Add(("reihitsu.json", configuration));
                         test.TestBehaviors |= TestBehaviors.SkipSuppressionCheck;
                     },
                     Diagnostic(RH8402FileMustStartWithConfiguredXmlStyleCopyrightHeaderAnalyzer.DiagnosticId).WithSpan(1, 1, 1, 1).WithMessage(AnalyzerResources.RH8402MessageFormat));
    }

    /// <summary>
    /// Mismatched block comment header
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task MismatchedBlockCommentHeader()
    {
        const string testData = """
                                /* <copyright file="Test0.cs" company="Other Company">
                                 * Copyright (c) Other Company. All rights reserved.
                                 * </copyright>
                                 */
                                namespace TestNamespace
                                {
                                }
                                """;
        const string fixedCode = """
                                 /* <copyright file="Test0.cs" company="Example Software">
                                  * Copyright (c) Example Software. All rights reserved.
                                  * </copyright>
                                  */
                                 namespace TestNamespace
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedCode,
                     test =>
                     {
                         var configuration = CreateCopyrightConfiguration("/* <copyright file=\"{fileName}\" company=\"{companyName}\">\n * Copyright (c) {companyName}. All rights reserved.\n * </copyright>\n */",
                                                                          ("companyName", "Example Software"));

                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                         test.FixedState.AdditionalFiles.Add(("reihitsu.json", configuration));
                         test.TestBehaviors |= TestBehaviors.SkipSuppressionCheck;
                     },
                     Diagnostic(RH8402FileMustStartWithConfiguredXmlStyleCopyrightHeaderAnalyzer.DiagnosticId).WithSpan(1, 1, 1, 1).WithMessage(AnalyzerResources.RH8402MessageFormat));
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
                     test => test.TestState.AdditionalFiles.Add(("reihitsu.json",
                                                                 CreateCopyrightConfiguration("// <copyright file=\"{fileName}\" company=\"{companyName}\">\n// Copyright (c) {companyName}. All rights reserved.\n// </copyright>",
                                                                                              ("companyName", "Example Software")))));
    }

    /// <summary>
    /// A leading comment that is not a copyright header is preserved when the header is inserted
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NonCopyrightLeadingCommentIsPreserved()
    {
        const string testData = """
                                // TODO: refactor this namespace
                                namespace TestNamespace
                                {
                                }
                                """;
        const string fixedCode = """
                                 // Copyright (c) Example Software. All rights reserved.
                                 // TODO: refactor this namespace
                                 namespace TestNamespace
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedCode,
                     test =>
                     {
                         var configuration = CreateCopyrightConfiguration("// Copyright (c) {companyName}. All rights reserved.",
                                                                          ("companyName", "Example Software"));

                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                         test.FixedState.AdditionalFiles.Add(("reihitsu.json", configuration));
                         test.TestBehaviors |= TestBehaviors.SkipSuppressionCheck;
                     },
                     Diagnostic(RH8402FileMustStartWithConfiguredXmlStyleCopyrightHeaderAnalyzer.DiagnosticId).WithSpan(1, 1, 1, 1).WithMessage(AnalyzerResources.RH8402MessageFormat));
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

    #region Methods

    /// <summary>
    /// Creates a copyright configuration
    /// </summary>
    /// <param name="copyrightText">Configured copyright text</param>
    /// <param name="placeholders">Placeholder values</param>
    /// <returns>Serialized configuration</returns>
    private static string CreateCopyrightConfiguration(string copyrightText, params (string Name, string Value)[] placeholders)
    {
        var copyrightConfiguration = new Dictionary<string, string>
                                     {
                                         ["copyrightText"] = copyrightText
                                     };

        foreach (var (name, value) in placeholders)
        {
            copyrightConfiguration[name] = value;
        }

        return JsonSerializer.Serialize(new Dictionary<string, Dictionary<string, string>>
                                        {
                                            ["copyright"] = copyrightConfiguration
                                        });
    }

    #endregion // Methods

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string template = "// <copyright file=\"{fileName}\" company=\"{companyName}\">\n// Copyright (c) {companyName}. All rights reserved.\n// </copyright>";

        const string secondSource = """
                                    namespace TestNamespace
                                    {
                                    }
                                    """;
        const string secondFixedSource = """
                                         // <copyright file="Test1.cs" company="Example Software">
                                         // Copyright (c) Example Software. All rights reserved.
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

        // This rule reports at most one diagnostic per file, so the Fix All scenario needs two documents to
        // report two diagnostics at all. The {fileName} placeholder in the template makes the scenario prove
        // the header is resolved per document rather than reusing the first document's resolved header
        return new FixAllScenario(TestCode,
                                  fixedCode,
                                  [
                                      Diagnostic(RH8402FileMustStartWithConfiguredXmlStyleCopyrightHeaderAnalyzer.DiagnosticId).WithSpan("/0/Test0.cs", 1, 1, 1, 1).WithMessage(AnalyzerResources.RH8402MessageFormat),
                                      Diagnostic(RH8402FileMustStartWithConfiguredXmlStyleCopyrightHeaderAnalyzer.DiagnosticId).WithSpan("/0/Test1.cs", 1, 1, 1, 1).WithMessage(AnalyzerResources.RH8402MessageFormat)
                                  ],
                                  config =>
                                  {
                                      var configuration = CreateCopyrightConfiguration(template, ("companyName", "Example Software"));

                                      config.TestState.Sources.Add(("/0/Test1.cs", secondSource));
                                      config.FixedState.Sources.Add(("/0/Test1.cs", secondFixedSource));
                                      config.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                                      config.FixedState.AdditionalFiles.Add(("reihitsu.json", configuration));
                                      config.TestBehaviors |= TestBehaviors.SkipSuppressionCheck;

                                      // Fix All in document scope corrects one document per iteration, and this rule reports at most one
                                      // diagnostic per file, so two documents need at most two iterations
                                      config.NumberOfFixAllIterations = -2;
                                  });
    }

    #endregion // BatchCodeFixTestsBase
}