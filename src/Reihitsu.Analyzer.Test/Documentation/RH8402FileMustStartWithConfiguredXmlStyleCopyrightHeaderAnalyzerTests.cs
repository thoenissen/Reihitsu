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

    /// <summary>
    /// Copyright text of the configuration that the specification examples use
    /// </summary>
    private const string SpecificationCopyrightText = "// <copyright file=\"{fileName}\" company=\"{company}\">\n// Copyright (c) {company}.\n// </copyright>";

    /// <summary>
    /// Path of the file that the specification examples check
    /// </summary>
    private const string SpecificationFilePath = "/0/C.cs";

    #endregion // Constants

    #region Tests

    /// <summary>
    /// Valid configured line comment header
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8402-002")]
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
    [Requirement("RH8402-002")]
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
    [Requirement("RH8402-002")]
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
    [Requirement("RH8402-002")]
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
    [Requirement("RH8402-002")]
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
    [Requirement("RH8402-002")]
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
    [Requirement("RH8402-002")]
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
    [Requirement("RH8402-003")]
    [Requirement("RH8402-004")]
    [Requirement("RH8402-005")]
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
    /// A leading comment that is not a copyright header is preserved when the header is inserted
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8402-003")]
    [Requirement("RH8402-004")]
    [Requirement("RH8402-005")]
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
    [Requirement("RH8402-001")]
    public async Task NoConfiguration()
    {
        await Verify(TestCode);
    }

    /// <summary>
    /// Verifies that nothing is reported when the configuration has no copyright section
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8402-001")]
    public async Task NoCopyrightSection()
    {
        await VerifySpecificationFile("namespace N;\n", "{}");
    }

    /// <summary>
    /// Verifies that nothing is reported when the configured copyright text is only white space
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8402-001")]
    public async Task WhitespaceCopyrightText()
    {
        await VerifySpecificationFile("namespace N;\n", CreateCopyrightConfiguration("   ", ("company", "Example")));
    }

    /// <summary>
    /// Verifies that nothing is reported when the configuration has a problem, here a placeholder without a setting
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8402-001")]
    public async Task ConfigurationWithProblem()
    {
        await VerifySpecificationFile("namespace N;\n", CreateCopyrightConfiguration("// Copyright (c) {owner}."));
    }

    /// <summary>
    /// Verifies that a file starting with the configured header, its placeholders and the file name filled in, is
    /// not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8402-002")]
    public async Task SpecificationHeaderWithPlaceholders()
    {
        const string source = """
                              // <copyright file="C.cs" company="Example">
                              // Copyright (c) Example.
                              // </copyright>
                              namespace N;

                              class C
                              {
                              }
                              """;

        await VerifySpecificationFile(source, CreateSpecificationConfiguration());
    }

    /// <summary>
    /// Verifies that the header is compared in the line ending of a CRLF file, although the configuration writes
    /// its line breaks as <c>\n</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8402-002")]
    public async Task SpecificationHeaderWithCarriageReturnLineFeed()
    {
        const string source = "// <copyright file=\"C.cs\" company=\"Example\">\r\n// Copyright (c) Example.\r\n// </copyright>\r\nnamespace N;\r\n";

        await VerifySpecificationFile(source, CreateSpecificationConfiguration());
    }

    /// <summary>
    /// Verifies that a file without the header, with a header naming another file, and with a blank line in front of
    /// the header is reported at the start of the file
    /// </summary>
    /// <param name="source">Checked file</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("namespace N;\n\nclass C\n{\n}\n")]
    [DataRow("// <copyright file=\"Other.cs\" company=\"Example\">\n// Copyright (c) Example.\n// </copyright>\nnamespace N;\n")]
    [DataRow("\n// <copyright file=\"C.cs\" company=\"Example\">\n// Copyright (c) Example.\n// </copyright>\nnamespace N;\n")]
    [Requirement("RH8402-003")]
    [Requirement("RH8402-004")]
    public async Task FileNotStartingWithHeader(string source)
    {
        await VerifySpecificationFile(source, CreateSpecificationConfiguration(), CreateSpecificationFileDiagnostic());
    }

    /// <summary>
    /// Verifies that a file not starting with the header is reported in a syntax tree whose documentation mode is parse,
    /// as in the default mode diagnose
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8402-003")]
    public async Task FileNotStartingWithHeaderWhenDocumentationModeIsParse()
    {
        const string source = "namespace N;\n\nclass C\n{\n}\n";
        var configuration = CreateSpecificationConfiguration();

        await Verify(source,
                     test =>
                     {
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add((SpecificationFilePath, source));
                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                         test.TestBehaviors |= TestBehaviors.SkipSuppressionCheck;
                         test.SolutionTransforms.Add(ApplyDocumentationModeParseToTestProject);
                     },
                     CreateSpecificationFileDiagnostic());
    }

    /// <summary>
    /// Verifies that a header that differs only in casing is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8402-003")]
    public async Task HeaderWithDifferentCasing()
    {
        const string source = "// <copyright file=\"C.cs\" company=\"example\">\n// Copyright (c) example.\n// </copyright>\nnamespace N;\n";

        await VerifySpecificationFile(source, CreateSpecificationConfiguration(), CreateSpecificationFileDiagnostic());
    }

    /// <summary>
    /// Verifies that an empty file is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8402-003")]
    [Requirement("RH8402-004")]
    public async Task EmptyFile()
    {
        await VerifySpecificationFile(string.Empty, CreateSpecificationConfiguration(), CreateSpecificationFileDiagnostic());
    }

    /// <summary>
    /// Verifies that the fix writes the header and a line break in front of the code of a one-line file
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8402-005")]
    public async Task FixInsertsHeaderInFrontOfCode()
    {
        await VerifySpecificationFileFix("namespace N;",
                                         "// <copyright file=\"C.cs\" company=\"Example\">\n// Copyright (c) Example.\n// </copyright>\nnamespace N;");
    }

    /// <summary>
    /// Verifies that the fix replaces a leading blank line with the header
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8402-005")]
    public async Task FixReplacesLeadingBlankLine()
    {
        await VerifySpecificationFileFix("\nnamespace N;",
                                         "// <copyright file=\"C.cs\" company=\"Example\">\n// Copyright (c) Example.\n// </copyright>\nnamespace N;");
    }

    /// <summary>
    /// Verifies that the fix inserts the header in front of a leading comment, which stays below it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8402-005")]
    public async Task FixKeepsLeadingCommentBelowHeader()
    {
        await VerifySpecificationFileFix("// Orders and invoices\nnamespace N;",
                                         "// <copyright file=\"C.cs\" company=\"Example\">\n// Copyright (c) Example.\n// </copyright>\n// Orders and invoices\nnamespace N;");
    }

    /// <summary>
    /// Verifies that the fix writes the header in front of a preprocessor directive that starts the file
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8402-005")]
    public async Task FixInsertsHeaderInFrontOfDirective()
    {
        await VerifySpecificationFileFix("#nullable enable\nnamespace N;\n",
                                         "// <copyright file=\"C.cs\" company=\"Example\">\n// Copyright (c) Example.\n// </copyright>\n#nullable enable\nnamespace N;\n");
    }

    /// <summary>
    /// Verifies that the fix writes the header in front of a documentation comment that starts the file
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8402-005")]
    public async Task FixInsertsHeaderInFrontOfDocumentationComment()
    {
        await VerifySpecificationFileFix("/// <summary>Type</summary>\nclass C\n{\n}\n",
                                         "// <copyright file=\"C.cs\" company=\"Example\">\n// Copyright (c) Example.\n// </copyright>\n/// <summary>Type</summary>\nclass C\n{\n}\n");
    }

    /// <summary>
    /// Verifies that the fix writes the header's line breaks and the separating line break in the line ending of a
    /// CRLF file
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8402-002")]
    [Requirement("RH8402-005")]
    public async Task FixWritesHeaderWithCarriageReturnLineFeed()
    {
        await VerifySpecificationFileFix("namespace N;\r\n\r\nclass C\r\n{\r\n}\r\n",
                                         "// <copyright file=\"C.cs\" company=\"Example\">\r\n// Copyright (c) Example.\r\n// </copyright>\r\nnamespace N;\r\n\r\nclass C\r\n{\r\n}\r\n");
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

    /// <summary>
    /// Creates the configuration that the specification examples use
    /// </summary>
    /// <returns>Serialized configuration</returns>
    private static string CreateSpecificationConfiguration()
    {
        return CreateCopyrightConfiguration(SpecificationCopyrightText, ("company", "Example"));
    }

    /// <summary>
    /// Creates the diagnostic expected at the start of the file that the specification examples check
    /// </summary>
    /// <returns>Expected diagnostic</returns>
    private static DiagnosticResult CreateSpecificationFileDiagnostic()
    {
        return Diagnostic(RH8402FileMustStartWithConfiguredXmlStyleCopyrightHeaderAnalyzer.DiagnosticId).WithSpan(SpecificationFilePath, 1, 1, 1, 1)
                                                                                                        .WithMessage(AnalyzerResources.RH8402MessageFormat);
    }

    /// <summary>
    /// Verifies the analyzer on the file that the specification examples check
    /// </summary>
    /// <param name="source">Text of the checked file</param>
    /// <param name="configuration">Serialized configuration</param>
    /// <param name="expected">Expected diagnostics</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task VerifySpecificationFile(string source, string configuration, params DiagnosticResult[] expected)
    {
        await Verify(source,
                     test =>
                     {
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add((SpecificationFilePath, source));
                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                         test.TestBehaviors |= TestBehaviors.SkipSuppressionCheck;
                     },
                     expected);
    }

    /// <summary>
    /// Verifies the diagnostic and the code fix on the file that the specification examples check
    /// </summary>
    /// <param name="source">Text of the checked file</param>
    /// <param name="fixedSource">Expected text after the fix</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task VerifySpecificationFileFix(string source, string fixedSource)
    {
        var configuration = CreateSpecificationConfiguration();

        await Verify(source,
                     fixedSource,
                     test =>
                     {
                         test.TestState.Sources.Clear();
                         test.TestState.Sources.Add((SpecificationFilePath, source));
                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                         test.FixedState.Sources.Clear();
                         test.FixedState.Sources.Add((SpecificationFilePath, fixedSource));
                         test.FixedState.AdditionalFiles.Add(("reihitsu.json", configuration));
                         test.TestBehaviors |= TestBehaviors.SkipSuppressionCheck;
                     },
                     CreateSpecificationFileDiagnostic());
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