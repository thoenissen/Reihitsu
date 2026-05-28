using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Analyzer;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Analyzer;

/// <summary>
/// Tests for <see cref="RH0001ConfigurationFileMustBeValidAnalyzer"/>
/// </summary>
[TestClass]
public class RH0001ConfigurationFileMustBeValidAnalyzerTests : AnalyzerTestsBase<RH0001ConfigurationFileMustBeValidAnalyzer>
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
                                                         "naming":{
                                                            "allowedNamespaceDeclarations":[
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
                                                         "naming":{
                                                            "allowedNamespaceDeclarations":[
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
                                                         "naming":{
                                                            "Unknown":[
                                                            ]
                                                         }
                                                      }
                                                      """;

                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                     },
                     InvalidConfiguration("Unknown configuration setting 'naming.Unknown'.", 3, 8));
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
                                                         "naming":[
                                                         ]
                                                      }
                                                      """;

                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                     },
                     InvalidConfiguration("The 'naming' section must be a JSON object.", 2, 13));
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
                                                         "naming":{
                                                            "allowedNamespaceDeclarations":"TestNamespace"
                                                         }
                                                      }
                                                      """;

                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                     },
                     InvalidConfiguration("The 'naming.allowedNamespaceDeclarations' setting must be a JSON array.", 3, 39));
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
                                                         "naming":{
                                                            "allowedNamespaceDeclarations":[
                                                               1
                                                            ]
                                                         }
                                                      }
                                                      """;

                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                     },
                     InvalidConfiguration("Entries in 'naming.allowedNamespaceDeclarations' must be strings.", 4, 10));
    }

    /// <summary>
    /// Valid copyright configuration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ValidCopyrightConfiguration()
    {
        await Verify(TestCode,
                     test =>
                     {
                         const string configuration = """
                                                      {
                                                         "copyright":{
                                                            "copyrightText":"// <copyright file=\"{fileName}\" company=\"{companyName}\">\n// Copyright (c) {companyName}. All rights reserved.\n// </copyright>",
                                                            "companyName":"Example Software"
                                                         }
                                                      }
                                                      """;

                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                     });
    }

    /// <summary>
    /// Copyright section must be object
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task CopyrightSectionMustBeObject()
    {
        await Verify(TestCode,
                     test =>
                     {
                         const string configuration = """
                                                      {
                                                         "copyright":[
                                                         ]
                                                      }
                                                      """;

                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                     },
                     InvalidConfiguration("The 'copyright' section must be a JSON object.", 2, 16));
    }

    /// <summary>
    /// Copyright text required
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task CopyrightTextRequired()
    {
        await Verify(TestCode,
                     test =>
                     {
                         const string configuration = """
                                                      {
                                                         "copyright":{
                                                            "companyName":"Example Software"
                                                         }
                                                      }
                                                      """;

                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                     },
                     InvalidConfiguration("The 'copyright.copyrightText' setting is required and must not be empty.", 2, 16));
    }

    /// <summary>
    /// Copyright text must be string
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task CopyrightTextMustBeString()
    {
        await Verify(TestCode,
                     test =>
                     {
                         const string configuration = """
                                                      {
                                                         "copyright":{
                                                            "copyrightText":[
                                                            ]
                                                         }
                                                      }
                                                      """;

                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                     },
                     InvalidConfiguration("The 'copyright.copyrightText' setting is required and must not be empty.", 2, 16),
                     InvalidConfiguration("The 'copyright.copyrightText' setting must be a string.", 3, 23));
    }

    /// <summary>
    /// Missing placeholder setting
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task MissingPlaceholderSetting()
    {
        await Verify(TestCode,
                     test =>
                     {
                         const string configuration = """
                                                      {
                                                         "copyright":{
                                                            "copyrightText":"// <copyright file=\"{fileName}\" company=\"{companyName}\">\n// Copyright (c) {companyName}. All rights reserved.\n// </copyright>"
                                                         }
                                                      }
                                                      """;

                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                     },
                     InvalidConfiguration("The placeholder 'companyName' used in 'copyright.copyrightText' has no matching setting in 'copyright'.", 2, 16));
    }

    /// <summary>
    /// Placeholder setting must be string
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task PlaceholderSettingMustBeString()
    {
        await Verify(TestCode,
                     test =>
                     {
                         const string configuration = """
                                                      {
                                                         "copyright":{
                                                            "copyrightText":"// <copyright file=\"{fileName}\" company=\"{companyName}\">\n// Copyright (c) {companyName}. All rights reserved.\n// </copyright>",
                                                            "companyName":[
                                                            ]
                                                         }
                                                      }
                                                      """;

                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                     },
                     InvalidConfiguration("The placeholder 'companyName' used in 'copyright.copyrightText' has no matching setting in 'copyright'.", 2, 16),
                     InvalidConfiguration("The 'copyright.companyName' setting must be a string.", 4, 21));
    }

    /// <summary>
    /// File name placeholder must not be configured
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task FileNamePlaceholderMustNotBeConfigured()
    {
        await Verify(TestCode,
                     test =>
                     {
                         const string configuration = """
                                                      {
                                                         "copyright":{
                                                            "copyrightText":"// <copyright file=\"{fileName}\" company=\"{companyName}\">\n// Copyright (c) {companyName}. All rights reserved.\n// </copyright>",
                                                            "companyName":"Example Software",
                                                            "fileName":"Example.cs"
                                                         }
                                                      }
                                                      """;

                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                     },
                     InvalidConfiguration("The 'copyright.fileName' setting is reserved and must not be configured.", 5, 8));
    }

    /// <summary>
    /// Copyright text must be comment header
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task CopyrightTextMustBeCommentHeader()
    {
        await Verify(TestCode,
                     test =>
                     {
                         const string configuration = """
                                                      {
                                                         "copyright":{
                                                            "copyrightText":"Copyright (c) {companyName}.",
                                                            "companyName":"Example Software"
                                                         }
                                                      }
                                                      """;

                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                     },
                     InvalidConfiguration("The 'copyright.copyrightText' setting must be a comment header that uses either '//' line comments or '/* */' block comments.", 2, 16));
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
        return Diagnostic(RH0001ConfigurationFileMustBeValidAnalyzer.DiagnosticId).WithLocation("reihitsu.json", line, column)
                                                                                  .WithMessage(string.Format(AnalyzerResources.RH0001MessageFormat, message));
    }

    #endregion // Methods
}