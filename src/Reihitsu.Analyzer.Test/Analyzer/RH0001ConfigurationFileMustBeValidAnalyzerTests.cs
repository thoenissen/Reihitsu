using System.IO;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Analyzer;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.SelfHosting.Utilities;

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
    [Requirement("RH0001-001")]
    [Requirement("RH0001-005")]
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
    /// Verifies that similarly named files are ignored and the exact configuration file is loaded
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH0001-001")]
    public async Task SimilarFileNameIsIgnoredAndExactConfigurationFileIsLoaded()
    {
        await Verify(TestCode,
                     test =>
                     {
                         test.TestState.AdditionalFiles.Add(("myreihitsu.json", "{}"));
                         test.TestState.AdditionalFiles.Add(("reihitsu.json", string.Empty));
                     },
                     InvalidConfiguration("The configuration file must not be empty or whitespace-only.", 1, 1));
    }

    /// <summary>
    /// Verifies that configuration file matching is case-insensitive
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH0001-001")]
    public async Task UppercaseConfigurationFileNameIsLoaded()
    {
        const string configurationPath = "REIHITSU.JSON";

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add((configurationPath, string.Empty)),
                     InvalidConfiguration(configurationPath, "The configuration file must not be empty or whitespace-only.", 1, 1));
    }

    /// <summary>
    /// Verifies that an exact configuration file in a subdirectory is loaded
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH0001-001")]
    public async Task ConfigurationFileInSubdirectoryIsLoaded()
    {
        var configurationPath = Path.Combine("settings", "reihitsu.json");

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add((configurationPath, string.Empty)),
                     InvalidConfiguration(configurationPath, "The configuration file must not be empty or whitespace-only.", 1, 1));
    }

    /// <summary>
    /// Verifies that a prefixed configuration lookalike is ignored when no exact file exists
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH0001-001")]
    public async Task MyReihitsuConfigurationLookalikeIsIgnored()
    {
        await Verify(TestCode, test => test.TestState.AdditionalFiles.Add(("myreihitsu.json", string.Empty)));
    }

    /// <summary>
    /// Verifies that a hyphen-prefixed configuration lookalike is ignored when no exact file exists
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH0001-001")]
    public async Task TeamReihitsuConfigurationLookalikeIsIgnored()
    {
        await Verify(TestCode, test => test.TestState.AdditionalFiles.Add(("team-reihitsu.json", string.Empty)));
    }

    /// <summary>
    /// Verifies that a backup file is ignored when no exact configuration file exists
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH0001-001")]
    public async Task ConfigurationBackupFileIsIgnored()
    {
        await Verify(TestCode, test => test.TestState.AdditionalFiles.Add(("reihitsu.json.bak", string.Empty)));
    }

    /// <summary>
    /// Verifies that the first exact configuration file wins over a later lookalike
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH0001-001")]
    public async Task ExactConfigurationFileBeforeLookalikeIsLoaded()
    {
        await Verify(TestCode,
                     test =>
                     {
                         test.TestState.AdditionalFiles.Add(("reihitsu.json", string.Empty));
                         test.TestState.AdditionalFiles.Add(("myreihitsu.json", "{}"));
                     },
                     InvalidConfiguration("The configuration file must not be empty or whitespace-only.", 1, 1));
    }

    /// <summary>
    /// Empty configuration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH0001-003")]
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
    [Requirement("RH0001-003")]
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
    [Requirement("RH0001-003")]
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
    /// Verifies that a non-ASCII character before a JSON syntax error on the same line does not shift the reported
    /// location, because the byte position reported by the JSON reader is converted to a UTF-16 character position
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH0001-002")]
    [Requirement("RH0001-003")]
    public async Task InvalidJsonWithNonAsciiCharacterBeforeErrorDoesNotShiftLocation()
    {
        await Verify(TestCode,
                     test =>
                     {
                         const string configuration = """
                                                      {
                                                         "naming":{
                                                            "allowedNamespaceDeclarations":[
                                                               "Nämespace" "Other"
                                                            ]
                                                         }
                                                      }
                                                      """;

                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                     },
                     InvalidConfiguration("The configuration file contains invalid JSON syntax.", 4, 22));
    }

    /// <summary>
    /// Unknown top level section
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH0001-002")]
    [Requirement("RH0001-004")]
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
    [Requirement("RH0001-002")]
    [Requirement("RH0001-005")]
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
    [Requirement("RH0001-004")]
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
    [Requirement("RH0001-005")]
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
    [Requirement("RH0001-002")]
    [Requirement("RH0001-005")]
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
    /// Verifies that a non-ASCII character before a validation error does not shift the reported location, because
    /// the byte offsets produced by the JSON reader are converted to UTF-16 character positions
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH0001-002")]
    public async Task NonAsciiCharacterBeforeErrorDoesNotShiftLocation()
    {
        await Verify(TestCode,
                     test =>
                     {
                         const string configuration = """
                                                      {
                                                         "naming":{
                                                            "allowedNamespaceDeclarations":[
                                                               "Nämespace",
                                                               1
                                                            ]
                                                         }
                                                      }
                                                      """;

                         test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration));
                     },
                     InvalidConfiguration("Entries in 'naming.allowedNamespaceDeclarations' must be strings.", 5, 10));
    }

    /// <summary>
    /// Valid copyright configuration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH0001-006")]
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
    [Requirement("RH0001-004")]
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
    [Requirement("RH0001-006")]
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
    [Requirement("RH0001-006")]
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
    [Requirement("RH0001-006")]
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
    [Requirement("RH0001-006")]
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
    [Requirement("RH0001-006")]
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
    [Requirement("RH0001-006")]
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

    /// <summary>
    /// Verifying that no code fix provider is registered for the rule
    /// </summary>
    [TestMethod]
    public void VerifyNoCodeFixIsOffered()
    {
        var codeFixProviders = AnalyzerMetadataDiscovery.DiscoverCodeFixProviders();

        Assert.DoesNotContain(codeFixProvider => codeFixProvider.DiagnosticId == RH0001ConfigurationFileMustBeValidAnalyzer.DiagnosticId, codeFixProviders);
    }

    /// <summary>
    /// Verifies that a token after the root object is invalid JSON syntax, reported at its first character
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH0001-003")]
    public async Task TokenAfterRootObjectIsInvalidJson()
    {
        await VerifyConfiguration("{ } junk", InvalidConfiguration("The configuration file contains invalid JSON syntax.", 1, 5));
    }

    /// <summary>
    /// Verifies that a second value after the root object is invalid JSON syntax, reported at its first character
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH0001-003")]
    public async Task SecondValueAfterRootObjectIsInvalidJson()
    {
        await VerifyConfiguration("{ } { }", InvalidConfiguration("The configuration file contains invalid JSON syntax.", 1, 5));
    }

    /// <summary>
    /// Verifies that a line comment after the root object is invalid JSON syntax, reported at the slash that starts it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH0001-003")]
    public async Task LineCommentAfterRootObjectIsInvalidJson()
    {
        await VerifyConfiguration("{ } // c", InvalidConfiguration("The configuration file contains invalid JSON syntax.", 1, 5));
    }

    /// <summary>
    /// Verifies that a block comment after the root object is invalid JSON syntax, reported at the slash that starts it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH0001-003")]
    public async Task BlockCommentAfterRootObjectIsInvalidJson()
    {
        await VerifyConfiguration("{ } /* c */", InvalidConfiguration("The configuration file contains invalid JSON syntax.", 1, 5));
    }

    /// <summary>
    /// Verifies that a token on a later line after the root object is reported on that line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH0001-003")]
    public async Task TokenOnLaterLineAfterRootObjectIsInvalidJson()
    {
        await VerifyConfiguration("{\n}\n\n  junk\n", InvalidConfiguration("The configuration file contains invalid JSON syntax.", 4, 3));
    }

    /// <summary>
    /// Verifies that a token on a later line after the root object is reported on that line in a file with CRLF line
    /// endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH0001-003")]
    public async Task TokenOnLaterLineAfterRootObjectWithCarriageReturnLineFeedIsInvalidJson()
    {
        await VerifyConfiguration("{\r\n}\r\n\r\n  junk\r\n", InvalidConfiguration("The configuration file contains invalid JSON syntax.", 4, 3));
    }

    /// <summary>
    /// Verifies that white space that is not JSON white space after the root object is invalid JSON syntax, reported at
    /// its character position
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH0001-002")]
    [Requirement("RH0001-003")]
    public async Task NonJsonWhiteSpaceAfterRootObjectIsInvalidJson()
    {
        await VerifyConfiguration("{ \"naming\": { \"allowedNamespaceDeclarations\": [ \"Nämespace\" ] } }\u00A0",
                                  InvalidConfiguration("The configuration file contains invalid JSON syntax.", 1, 66));
    }

    /// <summary>
    /// Verifies that spaces, tabs, and line breaks after the root object leave the file valid
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH0001-003")]
    public async Task JsonWhiteSpaceAfterRootObjectIsValid()
    {
        await VerifyConfiguration("{ }  \t\r\n\n\t  \r\n");
    }

    /// <summary>
    /// Verifies that a file with content after its root object is not validated further, so an unknown section of the
    /// root object is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH0001-003")]
    public async Task ContentAfterRootObjectReportsOnlyInvalidJson()
    {
        await VerifyConfiguration("{ \"formatting\": 1 } junk", InvalidConfiguration("The configuration file contains invalid JSON syntax.", 1, 21));
    }

    /// <summary>
    /// Verifies that the root object's own problems are still reported when only white space follows it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH0001-004")]
    public async Task UnknownSectionIsReportedWhenOnlyWhiteSpaceFollowsRootObject()
    {
        await VerifyConfiguration("{ \"formatting\": 1 }\n  \n", InvalidConfiguration("Unknown configuration section 'formatting'.", 1, 4));
    }

    /// <summary>
    /// Verifies that content after a root array is invalid JSON syntax, and that the root is not validated further
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH0001-003")]
    public async Task ContentAfterRootArrayReportsOnlyInvalidJson()
    {
        await VerifyConfiguration("[ ] junk", InvalidConfiguration("The configuration file contains invalid JSON syntax.", 1, 5));
    }

    /// <summary>
    /// Verifies that content after a root string is invalid JSON syntax, and that the root is not validated further
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH0001-003")]
    public async Task ContentAfterRootStringReportsOnlyInvalidJson()
    {
        await VerifyConfiguration("\"a\" junk", InvalidConfiguration("The configuration file contains invalid JSON syntax.", 1, 5));
    }

    /// <summary>
    /// Verifies that a root array followed only by white space is reported as a root that is not an object
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH0001-004")]
    public async Task RootArrayFollowedOnlyByWhiteSpaceIsReportedAsNotAnObject()
    {
        await VerifyConfiguration("[ ]  \n", InvalidConfiguration("The configuration root must be a JSON object.", 1, 1));
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
        return InvalidConfiguration("reihitsu.json", message, line, column);
    }

    /// <summary>
    /// Invalid configuration diagnostic
    /// </summary>
    /// <param name="configurationPath">Configuration path</param>
    /// <param name="message">Message</param>
    /// <param name="line">Line</param>
    /// <param name="column">Column</param>
    /// <returns>Diagnostic</returns>
    private static DiagnosticResult InvalidConfiguration(string configurationPath, string message, int line, int column)
    {
        return Diagnostic(RH0001ConfigurationFileMustBeValidAnalyzer.DiagnosticId).WithLocation(configurationPath, line, column)
                                                                                  .WithMessage(string.Format(AnalyzerResources.RH0001MessageFormat, message));
    }

    /// <summary>
    /// Verifies the analyzer with the given text of <c>reihitsu.json</c>
    /// </summary>
    /// <param name="configuration">Text of the configuration file</param>
    /// <param name="expected">Expected diagnostics</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task VerifyConfiguration(string configuration, params DiagnosticResult[] expected)
    {
        await Verify(TestCode, test => test.TestState.AdditionalFiles.Add(("reihitsu.json", configuration)), expected);
    }

    #endregion // Methods
}