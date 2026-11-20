using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Cli.Abstractions;
using Reihitsu.Formatter.Utilities;

namespace Reihitsu.Cli;

/// <summary>
/// Entry point for the reihitsu-format CLI tool
/// </summary>
internal static class Program
{
    #region Constants

    /// <summary>
    /// The option that selects the C# language version
    /// </summary>
    private const string LanguageVersionOption = "--lang-version";

    /// <summary>
    /// The option that sets the maximum number of files processed at the same time
    /// </summary>
    private const string MaxParallelismOption = "--max-parallelism";

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Main entry point
    /// </summary>
    /// <param name="args">Command-line arguments</param>
    /// <returns>Exit code: 0 = success, 1 = formatting needed (--check/--dry-run), 2 = error</returns>
    public static async Task<int> Main(string[] args)
    {
        var result = ParseArguments(args);
        var exitCode = await HandleArgumentsWithoutFormattingAsync(result).ConfigureAwait(false);

        if (exitCode != null)
        {
            return exitCode.Value;
        }

        if (result.ExceedingLanguageVersion != null)
        {
            await Console.Error.WriteLineAsync($"Warning: C# language version '{result.ExceedingLanguageVersion}' is newer than the newest supported version {result.LanguageVersion.ToDisplayString()}; formatting for {result.LanguageVersion.ToDisplayString()}.");
        }

        var paths = new List<string>(result.Paths);

        if (paths.Count == 0)
        {
            paths.Add(Directory.GetCurrentDirectory());
        }

        return await RunFormatCommandAsync(result, paths.ToArray()).ConfigureAwait(false);
    }

    /// <summary>
    /// Creates the format command handler from every parsed option value
    /// </summary>
    /// <param name="result">The parsed command-line arguments</param>
    /// <param name="paths">The file or directory paths to process</param>
    /// <param name="dependencies">The command dependencies</param>
    /// <returns>The handler that runs the format command</returns>
    internal static FormatCommandHandler CreateHandler(ParseResult result, string[] paths, FormatCommandDependencies dependencies)
    {
        return new FormatCommandHandler(paths,
                                        result.CheckOnly,
                                        result.DryRun,
                                        result.Verbose,
                                        result.Force,
                                        result.Utf8Bom,
                                        result.LanguageVersion,
                                        dependencies,
                                        result.SuppressUnsupported,
                                        result.MaxParallelism);
    }

    /// <summary>
    /// Parses command-line arguments into a <see cref="ParseResult"/>
    /// </summary>
    /// <param name="args">The command-line arguments to parse</param>
    /// <returns>A <see cref="ParseResult"/> containing the parsed values</returns>
    internal static ParseResult ParseArguments(string[] args)
    {
        var paths = new List<string>();
        var result = new ParseResult(false, false, false, false, false, false, false, false, paths, null, LanguageVersionResolver.MaxSupportedLanguageVersion, null, null, Environment.ProcessorCount);
        var pathsOnly = false;
        string optionExpectingValue = null;

        foreach (var arg in args)
        {
            if (optionExpectingValue != null)
            {
                result = ApplyOptionValue(result, optionExpectingValue, arg);
                optionExpectingValue = null;
            }
            else if (pathsOnly)
            {
                paths.Add(arg);
            }
            else if (arg == "--")
            {
                // Everything after the "--" separator is treated as a path, even if it starts with "-".
                pathsOnly = true;
            }
            else if (arg is LanguageVersionOption or MaxParallelismOption)
            {
                optionExpectingValue = arg;
            }
            else if (ApplyFlag(result, arg) is { } flagResult)
            {
                result = flagResult;
            }
            else if (arg.StartsWith('-'))
            {
                result = result with
                                {
                                    UnknownOption = result.UnknownOption ?? arg
                                };
            }
            else
            {
                paths.Add(arg);
            }
        }

        // The option is the last argument, so its value is missing. LanguageVersionFacts.TryParse would accept a
        // missing value as the default version, which is why it never sees one.
        if (optionExpectingValue != null)
        {
            result = WithArgumentError(result, $"Missing value for {optionExpectingValue}.");
        }

        return result;
    }

    /// <summary>
    /// Applies an option that takes no value
    /// </summary>
    /// <param name="result">The arguments parsed so far</param>
    /// <param name="arg">The argument</param>
    /// <returns>The arguments with the option applied, or <see langword="null"/> when the argument is no such option</returns>
    private static ParseResult? ApplyFlag(ParseResult result, string arg)
    {
        return arg switch
               {
                   "--check" => result with
                                       {
                                           CheckOnly = true
                                       },
                   "--dry-run" => result with
                                         {
                                             DryRun = true
                                         },
                   "--verbose" => result with
                                         {
                                             Verbose = true
                                         },
                   "--force" => result with
                                       {
                                           Force = true
                                       },
                   "--utf8-bom" => result with
                                          {
                                              Utf8Bom = true
                                          },
                   "--suppress-unsupported" => result with
                                                      {
                                                          SuppressUnsupported = true
                                                      },
                   "--help" or "-h" => result with
                                              {
                                                  ShowHelp = true
                                              },
                   "--version" => result with
                                         {
                                             ShowVersion = true
                                         },
                   _ => null
               };
    }

    /// <summary>
    /// Applies the value of <c>--lang-version</c> or <c>--max-parallelism</c>
    /// </summary>
    /// <param name="result">The arguments parsed so far</param>
    /// <param name="option">The option the value belongs to</param>
    /// <param name="value">The value given to the option</param>
    /// <returns>The arguments with the value applied, or with an argument error when the value is invalid</returns>
    private static ParseResult ApplyOptionValue(ParseResult result, string option, string value)
    {
        if (option == MaxParallelismOption)
        {
            return TryParseMaxParallelism(value, out var requestedParallelism)
                       ? result with
                                {
                                    MaxParallelism = requestedParallelism
                                }
                       : WithArgumentError(result, $"Invalid value for {MaxParallelismOption}: '{value}'. Expected a whole number of 1 or more.");
        }

        if (LanguageVersionFacts.TryParse(value, out var requestedVersion))
        {
            return result with
                          {
                              LanguageVersion = LanguageVersionResolver.Resolve(requestedVersion),
                              ExceedingLanguageVersion = LanguageVersionResolver.ExceedsMaximum(requestedVersion) ? value : null
                          };
        }

        return WithArgumentError(result, $"Invalid value for {LanguageVersionOption}: '{value}'. Expected a C# language version such as 12, 12.0, latest, or preview.");
    }

    /// <summary>
    /// Records an argument error unless an earlier one is recorded, so the first error is reported
    /// </summary>
    /// <param name="result">The arguments parsed so far</param>
    /// <param name="argumentError">The error</param>
    /// <returns>The arguments with the first error recorded</returns>
    private static ParseResult WithArgumentError(ParseResult result, string argumentError)
    {
        return result with
                      {
                          ArgumentError = result.ArgumentError ?? argumentError
                      };
    }

    /// <summary>
    /// Handles the parsed arguments that end the run without formatting: an unknown option, an invalid option value,
    /// <c>--help</c>, <c>--version</c>, and the combination of <c>--check</c> with <c>--dry-run</c>
    /// </summary>
    /// <param name="result">The parsed command-line arguments</param>
    /// <returns>The exit code when the run ends here; otherwise, <see langword="null"/></returns>
    private static async Task<int?> HandleArgumentsWithoutFormattingAsync(ParseResult result)
    {
        if (result.UnknownOption != null)
        {
            await Console.Error.WriteLineAsync($"Unknown option: {result.UnknownOption}");

            PrintUsage(Console.Error);

            return ExitCodes.Error;
        }

        if (result.ArgumentError != null)
        {
            await Console.Error.WriteLineAsync(result.ArgumentError);

            PrintUsage(Console.Error);

            return ExitCodes.Error;
        }

        if (result.ShowHelp)
        {
            PrintUsage(Console.Out);

            return ExitCodes.Success;
        }

        if (result.ShowVersion)
        {
            await Console.Out.WriteLineAsync($"reihitsu-format {GetVersion()}");

            return ExitCodes.Success;
        }

        if (result.CheckOnly && result.DryRun)
        {
            await Console.Error.WriteLineAsync("Options --check and --dry-run cannot be combined.");

            PrintUsage(Console.Error);

            return ExitCodes.Error;
        }

        return null;
    }

    /// <summary>
    /// Runs the format command over the paths, canceling it gracefully on Ctrl+C
    /// </summary>
    /// <param name="result">The parsed command-line arguments</param>
    /// <param name="paths">The file or directory paths to process</param>
    /// <returns>The exit code of the format command</returns>
    private static async Task<int> RunFormatCommandAsync(ParseResult result, string[] paths)
    {
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
                var consoleInput = new DefaultConsoleInput();
                var formatter = new DefaultSourceFormatter();
                var diffGenerator = new DefaultDiffGenerator();

                var dependencies = new FormatCommandDependencies(fileSystem, console, consoleInput, formatter, diffGenerator);
                var handler = CreateHandler(result, paths, dependencies);

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
        writer.WriteLine("  --dry-run    Show what would change without applying (exit code 1 if changes would be made; cannot be combined with --check)");
        writer.WriteLine("  --verbose    Show detailed output for each file");
        writer.WriteLine($"  --force      Skip the confirmation prompt shown when more than {FormatCommandHandler.LargeRunConfirmationThreshold} files would be formatted");
        writer.WriteLine("  --utf8-bom   Write processed files as UTF-8 with a byte order mark");
        writer.WriteLine("  --suppress-unsupported");
        writer.WriteLine("               Report a file with syntax the formatter does not support as a warning instead of an error; the file is never changed");
        writer.WriteLine("  --lang-version <version>");
        writer.WriteLine("               C# language version to parse and format for, such as 12, 12.0, latest, or preview (default: latest)");
        writer.WriteLine($"               latest selects {LanguageVersionResolver.MaxSupportedLanguageVersion.ToDisplayString()}, the newest supported version; newer values such as preview are lowered to it");
        writer.WriteLine("               LangVersion from project files is not read");
        writer.WriteLine("  --max-parallelism <count>");
        writer.WriteLine("               Maximum number of files processed at the same time (default: the number of logical processors)");
        writer.WriteLine("  --help, -h   Show this help message");
        writer.WriteLine("  --version    Show version information");
        writer.WriteLine("  --           Treat all following arguments as paths");
    }

    /// <summary>
    /// Parses a <c>--max-parallelism</c> count: decimal digits <c>0</c>-<c>9</c> only, with a value from 1 to
    /// <see cref="int.MaxValue"/>; a sign, white space, any other character, and a larger value are rejected (CLI-023)
    /// </summary>
    /// <param name="value">The value given to the option</param>
    /// <param name="count">The parsed count, when the value is accepted</param>
    /// <returns><see langword="true"/> when the value is accepted; otherwise, <see langword="false"/></returns>
    private static bool TryParseMaxParallelism(string value, out int count)
    {
        // NumberStyles.None accepts the ASCII digits 0-9 only: no sign, white space, separator, or exponent
        return int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out count)
               && count >= 1;
    }

    /// <summary>
    /// Gets the CLI's version string
    /// </summary>
    /// <returns>The informational version reported by <c>--version</c></returns>
    private static string GetVersion()
    {
        var assembly = typeof(Program).Assembly;

        return assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? assembly.GetName().Version?.ToString() ?? "unknown";
    }

    #endregion // Methods
}