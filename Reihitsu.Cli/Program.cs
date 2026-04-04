using System.Collections.Generic;
using System.Threading.Tasks;

using Reihitsu.Cli.Abstractions;

namespace Reihitsu.Cli;

/// <summary>
/// Entry point for the reihitsu-format CLI tool.
/// </summary>
internal static class Program
{
    #region Methods

    /// <summary>
    /// Main entry point.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    /// <returns>Exit code: 0 = success, 1 = formatting needed (--check), 2 = error.</returns>
    public static async Task<int> Main(string[] args)
    {
        var result = ParseArguments(args);

        if (result.UnknownOption != null)
        {
            await Console.Error.WriteLineAsync($"Unknown option: {result.UnknownOption}");

            PrintUsage();

            return ExitCodes.Error;
        }

        if (result.ShowHelp)
        {
            PrintUsage();

            return ExitCodes.Success;
        }

        var paths = new List<string>(result.Paths);

        if (paths.Count == 0)
        {
            paths.Add(Directory.GetCurrentDirectory());
        }

        var path = paths.FirstOrDefault(path => File.Exists(path) == false && Directory.Exists(path) == false);

        if (path != null)
        {
            await Console.Error.WriteLineAsync($"Path not found: {path}");

            return ExitCodes.Error;
        }

        try
        {
            var fileSystem = new DefaultFileSystem();
            var console = new DefaultConsoleOutput();
            var formatter = new DefaultSourceFormatter();
            var diffGenerator = new DefaultDiffGenerator();

            var handler = new FormatCommandHandler(paths.ToArray(), result.CheckOnly, result.DryRun, result.Verbose, fileSystem, console, formatter, diffGenerator);

            return await handler.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Error: {ex.Message}");

            return ExitCodes.Error;
        }
    }

    /// <summary>
    /// Parses command-line arguments into a <see cref="ParseResult"/>.
    /// </summary>
    /// <param name="args">The command-line arguments to parse.</param>
    /// <returns>A <see cref="ParseResult"/> containing the parsed values.</returns>
    internal static ParseResult ParseArguments(string[] args)
    {
        var checkOnly = false;
        var dryRun = false;
        var verbose = false;
        var showHelp = false;
        var paths = new List<string>();
        string unknownOption = null;

        foreach (var arg in args)
        {
            switch (arg)
            {
                case "--check":
                    {
                        checkOnly = true;
                    }
                    break;

                case "--dry-run":
                    {
                        dryRun = true;
                    }
                    break;

                case "--verbose":
                    {
                        verbose = true;
                    }
                    break;

                case "--help":
                case "-h":
                    {
                        showHelp = true;
                    }
                    break;

                default:
                    {
                        if (arg.StartsWith('-'))
                        {
                            unknownOption ??= arg;
                        }
                        else
                        {
                            paths.Add(arg);
                        }
                    }
                    break;
            }
        }

        return new ParseResult(checkOnly, dryRun, verbose, showHelp, paths, unknownOption);
    }

    /// <summary>
    /// Prints the usage information to the console.
    /// </summary>
    private static void PrintUsage()
    {
        Console.WriteLine("reihitsu-format [options] [<paths>...]");
        Console.WriteLine();
        Console.WriteLine("Arguments:");
        Console.WriteLine("  <paths>    One or more files or directories to format (default: current directory)");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --check      Check if files are formatted (exit code 1 if not); don't write changes");
        Console.WriteLine("  --dry-run    Show what would change without applying");
        Console.WriteLine("  --verbose    Show detailed output for each file");
        Console.WriteLine("  --help       Show this help message");
    }

    #endregion // Methods
}