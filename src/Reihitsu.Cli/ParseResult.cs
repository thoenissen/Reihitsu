using System.Collections.Generic;

using Microsoft.CodeAnalysis.CSharp;

namespace Reihitsu.Cli;

/// <summary>
/// Represents the result of parsing command-line arguments
/// </summary>
/// <param name="CheckOnly">Whether --check was specified</param>
/// <param name="DryRun">Whether --dry-run was specified</param>
/// <param name="Verbose">Whether --verbose was specified</param>
/// <param name="Force">Whether --force was specified</param>
/// <param name="Utf8Bom">Whether --utf8-bom was specified</param>
/// <param name="ShowHelp">Whether --help was specified</param>
/// <param name="ShowVersion">Whether --version was specified</param>
/// <param name="Paths">The list of file/directory paths</param>
/// <param name="UnknownOption">The first unrecognized option, or null</param>
/// <param name="LanguageVersion">The resolved C# language version selected by --lang-version, clamped to the newest supported version</param>
/// <param name="ExceedingLanguageVersion">The --lang-version value that named a version newer than the newest supported one, or null</param>
/// <param name="ArgumentError">The first error in an option value, or null</param>
internal readonly record struct ParseResult(bool CheckOnly,
                                            bool DryRun,
                                            bool Verbose,
                                            bool Force,
                                            bool Utf8Bom,
                                            bool ShowHelp,
                                            bool ShowVersion,
                                            IReadOnlyList<string> Paths,
                                            string UnknownOption,
                                            LanguageVersion LanguageVersion,
                                            string ExceedingLanguageVersion,
                                            string ArgumentError);