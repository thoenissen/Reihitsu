using System.Collections.Generic;
using System.Threading.Tasks;

using Reihitsu.Cli.Abstractions;

namespace Reihitsu.Cli;

/// <summary>
/// Entry point for the reihitsu-format CLI tool
/// </summary>
internal static class Program
{
    #region Methods

    /// <summary>
    /// Main entry point
    /// </summary>
    /// <param name="args">Command-line arguments</param>
    /// <returns>Exit code: 0 = success, 1 = formatting needed (--check), 2 = error</returns>
    public static async Task<int> Main(string[] args)
    {
        var result = ParseArguments(args);

        if (result.UnknownOption != null)
        {
            await Console.Error.WriteLineAsync($"Unknown option: {result.UnknownOption}");

            PrintUsage(Console.Error);

            return ExitCodes.Error;
        }

        if (result.ShowHelp)
        {
            PrintUsage(Console.Out);

            return ExitCodes.Success;
        }

        if (result.CheckOnly && result.DryRun)
        {
            await Console.Error.WriteLineAsync("Options --check and --dry-run cannot be combined.");

            PrintUsage(Console.Error);

            return ExitCodes.Error;
        }

        var paths = new List<string>(result.Paths);

        if (paths.Count == 0)
        {
            paths.Add(Directory.GetCurrentDirectory());
        }

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            ConsoleCancelEventHandler cancelHandler = (_, eventArgs) =>
                                                      {
                                                          // Cancel the run gracefully instead of letting the runtime terminate the process abruptly.
                                                          eventArgs.Cancel = true;
                                                          cancellationTokenSource.Cancel();
                                                      };

            Console.CancelKeyPress += cancelHandler;

            try
            {
                var fileSystem = new DefaultFileSystem();
                var console = new DefaultConsoleOutput();
                var formatter = new DefaultSourceFormatter();
                var diffGenerator = new DefaultDiffGenerator();

                var dependencies = new FormatCommandDependencies(fileSystem, console, formatter, diffGenerator);
                var handler = new FormatCommandHandler(paths.ToArray(), result.CheckOnly, result.DryRun, result.Verbose, dependencies);

                return await handler.ExecuteAsync(cancellationTokenSource.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                await Console.Error.WriteLineAsync("Operation canceled.");

                return ExitCodes.Error;
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync($"Error: {ex.Message}");

                return ExitCodes.Error;
            }
            finally
            {
                Console.CancelKeyPress -= cancelHandler;
            }
        }
    }

    /// <summary>
    /// Parses command-line arguments into a <see cref="ParseResult"/>
    /// </summary>
    /// <param name="args">The command-line arguments to parse</param>
    /// <returns>A <see cref="ParseResult"/> containing the parsed values</returns>
    internal static ParseResult ParseArguments(string[] args)
    {
        var checkOnly = false;
        var dryRun = false;
        var verbose = false;
        var showHelp = false;
        var paths = new List<string>();
        string unknownOption = null;
        var pathsOnly = false;

        foreach (var arg in args)
        {
            if (pathsOnly)
            {
                paths.Add(arg);

                continue;
            }

            switch (arg)
            {
                case "--":
                    {
                        // Everything after the "--" separator is treated as a path, even if it starts with "-".
                        pathsOnly = true;
                    }
                    break;

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
    /// Prints the usage information to the specified writer
    /// </summary>
    /// <param name="writer">The writer to print the usage information to (standard output for help, standard error for argument errors)</param>
    private static void PrintUsage(TextWriter writer)
    {
        writer.WriteLine("reihitsu-format [options] [--] [<paths>...]");
        writer.WriteLine();
        writer.WriteLine("Arguments:");
        writer.WriteLine("  <paths>      One or more files or directories to format (default: current directory)");
        writer.WriteLine();
        writer.WriteLine("Options:");
        writer.WriteLine("  --check      Check if files are formatted (exit code 1 if not); don't write changes");
        writer.WriteLine("  --dry-run    Show what would change without applying (cannot be combined with --check)");
        writer.WriteLine("  --verbose    Show detailed output for each file");
        writer.WriteLine("  --help, -h   Show this help message");
        writer.WriteLine("  --           Treat all following arguments as paths");
    }

    #endregion // Methods
}