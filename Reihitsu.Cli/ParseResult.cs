using System.Collections.Generic;

namespace Reihitsu.Cli;

/// <summary>
/// Represents the result of parsing command-line arguments.
/// </summary>
/// <param name="CheckOnly">Whether --check was specified.</param>
/// <param name="DryRun">Whether --dry-run was specified.</param>
/// <param name="Verbose">Whether --verbose was specified.</param>
/// <param name="ShowHelp">Whether --help was specified.</param>
/// <param name="Paths">The list of file/directory paths.</param>
/// <param name="UnknownOption">The first unrecognized option, or null.</param>
internal readonly record struct ParseResult(bool CheckOnly, bool DryRun, bool Verbose, bool ShowHelp, IReadOnlyList<string> Paths, string UnknownOption);