using Reihitsu.Cli.Abstractions;

namespace Reihitsu.Cli;

/// <summary>
/// Provides the service dependencies required by <see cref="FormatCommandHandler"/>.
/// </summary>
/// <param name="FileSystem">The file system abstraction.</param>
/// <param name="Console">The console output abstraction.</param>
/// <param name="Formatter">The source formatter abstraction.</param>
/// <param name="DiffGenerator">The diff generator abstraction.</param>
internal sealed record FormatCommandDependencies(IFileSystem FileSystem, IConsoleOutput Console, ISourceFormatter Formatter, IDiffGenerator DiffGenerator);