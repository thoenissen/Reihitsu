using Reihitsu.Tooling.Enumerations;

namespace Reihitsu.Tooling;

/// <summary>
/// Runs the code-fix surface of one diagnostic ID over a directory of fixtures, under both line endings
/// </summary>
public static class CodeFixRunCommand
{
    #region Constants

    /// <summary>
    /// Maximum number of code actions applied per fixture when the caller provides no override
    /// </summary>
    private const int DefaultMaximumIterations = 10;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Executes the repository-only code-fix fixture run
    /// </summary>
    /// <param name="args">Command arguments</param>
    /// <param name="output">Standard output writer</param>
    /// <param name="error">Standard error writer</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// Exit code 0 when every fixture reached a supported end state, 1 when a fixture never stopped reporting the
    /// diagnostic, or 2 when the command could not run
    /// </returns>
    public static async Task<int> ExecuteAsync(string[] args,
                                               TextWriter output,
                                               TextWriter error,
                                               CancellationToken cancellationToken = default)
    {
        if (TryParseArguments(args, out var diagnosticId, out var directory, out var maximumIterations, out var showHelp, out var parseError) == false)
        {
            error.WriteLine($"apply-fix: {parseError}");
            PrintUsage(error);

            return ExitCodes.Error;
        }

        if (showHelp)
        {
            PrintUsage(output);

            return ExitCodes.Success;
        }

        try
        {
            var fullDirectory = Path.GetFullPath(directory);

            if (Directory.Exists(fullDirectory) == false)
            {
                error.WriteLine($"apply-fix: fixture directory not found: {directory}");

                return ExitCodes.Error;
            }

            // Every C# file counts as a fixture: unlike a project tree, a fixture directory has no generated files
            // to skip, and silently skipping one would be indistinguishable from a fixture that reports nothing.
            var fixtures = Directory.EnumerateFiles(fullDirectory, "*.cs", SearchOption.AllDirectories)
                                    .OrderBy(path => path, StringComparer.Ordinal)
                                    .ToList();

            if (fixtures.Count == 0)
            {
                error.WriteLine($"apply-fix: no C# fixture found under {fullDirectory}");

                return ExitCodes.Error;
            }

            if (CodeFixTargetResolver.TryResolve(diagnosticId, out var target, out var resolveError) == false)
            {
                error.WriteLine($"apply-fix: {resolveError}");

                return ExitCodes.Error;
            }

            var analyzerNames = string.Join(", ", target.Analyzers.Select(analyzer => analyzer.GetType().Name).OrderBy(name => name, StringComparer.Ordinal));

            output.WriteLine($"{diagnosticId} -> {target.CodeFixProvider.GetType().Name} / {analyzerNames}");
            output.WriteLine();

            var counters = new Dictionary<FixtureOutcome, int>();
            var lineEndingDrifts = 0;

            foreach (var fixture in fixtures)
            {
                var displayPath = Path.GetRelativePath(fullDirectory, fixture).Replace('\\', '/');
                var source = await File.ReadAllTextAsync(fixture, cancellationToken).ConfigureAwait(false);

                foreach (var lineEnding in new[] { FixtureLineEndings.LineFeed, FixtureLineEndings.CarriageReturnLineFeed })
                {
                    var result = await FixtureRunner.RunAsync(target, source, lineEnding, maximumIterations, cancellationToken).ConfigureAwait(false);

                    counters[result.Outcome] = counters.GetValueOrDefault(result.Outcome) + 1;

                    if (result.PreservedLineEnding == false)
                    {
                        lineEndingDrifts++;
                    }

                    WriteFixtureReport(output, displayPath, lineEnding, result);
                }
            }

            var summary = string.Join(", ",
                                      $"{counters.GetValueOrDefault(FixtureOutcome.Fixed)} fixed",
                                      $"{counters.GetValueOrDefault(FixtureOutcome.NoFixOffered)} no-fix-offered",
                                      $"{counters.GetValueOrDefault(FixtureOutcome.NoDiagnostic)} no-diagnostic",
                                      $"{counters.GetValueOrDefault(FixtureOutcome.NotConverged)} not-converged",
                                      $"{counters.GetValueOrDefault(FixtureOutcome.NoProgress)} no-progress",
                                      $"{counters.GetValueOrDefault(FixtureOutcome.AnalyzerFailure)} analyzer-failure",
                                      $"{counters.GetValueOrDefault(FixtureOutcome.ParseError)} parse-error",
                                      $"{counters.GetValueOrDefault(FixtureOutcome.InvalidResult)} invalid-result",
                                      $"{lineEndingDrifts} line-ending-drift");

            output.WriteLine();
            output.WriteLine($"CODE-FIX RUN: {fixtures.Count} fixture(s) x 2 endings — {summary}");

            // An analyzer that threw observed nothing about the rule, so it is a failure to run rather than a
            // fixture result — reporting it as anything else would let a crash read as "does not reproduce".
            if (counters.GetValueOrDefault(FixtureOutcome.ParseError) > 0
                || counters.GetValueOrDefault(FixtureOutcome.InvalidResult) > 0
                || counters.GetValueOrDefault(FixtureOutcome.AnalyzerFailure) > 0)
            {
                return ExitCodes.Error;
            }

            return counters.GetValueOrDefault(FixtureOutcome.NotConverged) > 0
                   || counters.GetValueOrDefault(FixtureOutcome.NoProgress) > 0
                       ? ExitCodes.NotConverged
                       : ExitCodes.Success;
        }
        catch (OperationCanceledException)
        {
            error.WriteLine("apply-fix: operation canceled.");

            return ExitCodes.Error;
        }
        catch (Exception exception)
        {
            error.WriteLine($"apply-fix: {exception.Message}");

            return ExitCodes.Error;
        }
    }

    /// <summary>
    /// Writes the header, the optional line-ending note, and the diff of one fixture arm
    /// </summary>
    /// <param name="output">Standard output writer</param>
    /// <param name="displayPath">Fixture path relative to the fixture directory</param>
    /// <param name="lineEnding">Line ending of this arm</param>
    /// <param name="result">Result of this arm</param>
    private static void WriteFixtureReport(TextWriter output, string displayPath, string lineEnding, FixtureRunResult result)
    {
        var drift = result.PreservedLineEnding ? string.Empty : " [line-ending-drift]";

        output.WriteLine($"== {displayPath} [{FixtureLineEndings.GetName(lineEnding)}] == {DescribeOutcome(result)}{drift}");

        if (result.PreservedLineEnding == false)
        {
            output.WriteLine($"Line endings: {FixtureLineEndings.Describe(result.FinalSource)}");
        }

        var diff = UnifiedDiffWriter.Generate(displayPath, result.OriginalSource, result.FinalSource);

        if (diff.Length > 0)
        {
            output.Write(diff);
        }
    }

    /// <summary>
    /// Renders the status token of a fixture arm
    /// </summary>
    /// <param name="result">Result to describe</param>
    /// <returns>The status token</returns>
    private static string DescribeOutcome(FixtureRunResult result)
    {
        return result.Outcome switch
               {
                   FixtureOutcome.NoDiagnostic => "no-diagnostic",
                   FixtureOutcome.NoFixOffered => "no-fix-offered",
                   FixtureOutcome.Fixed => $"fixed ({result.RegisteredActions} action(s), {result.Iterations} iteration(s))",
                   FixtureOutcome.NotConverged => $"not-converged ({result.Iterations} iteration(s))",
                   FixtureOutcome.NoProgress => $"no-progress ({result.Iterations} iteration(s))",
                   FixtureOutcome.AnalyzerFailure => "analyzer-failure",
                   FixtureOutcome.ParseError => "parse-error",
                   FixtureOutcome.InvalidResult => $"invalid-result ({result.Iterations} iteration(s))",
                   _ => result.Outcome.ToString()
               };
    }

    /// <summary>
    /// Prints command usage
    /// </summary>
    /// <param name="writer">Writer that receives the usage text</param>
    private static void PrintUsage(TextWriter writer)
    {
        writer.WriteLine("usage: apply-fix <diagnostic-id> <fixture-directory> [--max-iterations <positive-integer>]");
        writer.WriteLine("       apply-fix --help");
    }

    /// <summary>
    /// Parses the command arguments
    /// </summary>
    /// <param name="args">Arguments to parse</param>
    /// <param name="diagnosticId">Diagnostic ID whose code-fix surface is exercised</param>
    /// <param name="directory">Directory holding the fixtures</param>
    /// <param name="maximumIterations">Maximum number of code actions applied per fixture</param>
    /// <param name="showHelp">Whether usage should be printed</param>
    /// <param name="error">Argument error when parsing fails</param>
    /// <returns><see langword="true"/> when the arguments are valid; otherwise, <see langword="false"/></returns>
    private static bool TryParseArguments(string[] args,
                                          out string diagnosticId,
                                          out string directory,
                                          out int maximumIterations,
                                          out bool showHelp,
                                          out string error)
    {
        diagnosticId = null;
        directory = null;
        maximumIterations = DefaultMaximumIterations;
        showHelp = false;
        error = null;

        var iterationsSpecified = false;
        var positionalOnly = false;

        for (var argumentIndex = 0; argumentIndex < args.Length; argumentIndex++)
        {
            var argument = args[argumentIndex];

            if (positionalOnly == false && (argument is "--help" or "-h"))
            {
                showHelp = true;

                continue;
            }

            if (positionalOnly == false && argument == "--")
            {
                positionalOnly = true;

                continue;
            }

            if (positionalOnly == false && argument == "--max-iterations")
            {
                if (iterationsSpecified)
                {
                    error = "--max-iterations may only be specified once.";

                    return false;
                }

                if (argumentIndex + 1 >= args.Length
                    || int.TryParse(args[++argumentIndex], out maximumIterations) == false
                    || maximumIterations <= 0)
                {
                    error = "--max-iterations requires a positive integer.";

                    return false;
                }

                iterationsSpecified = true;

                continue;
            }

            if (positionalOnly == false && argument.StartsWith('-'))
            {
                error = $"unknown argument '{argument}'.";

                return false;
            }

            if (diagnosticId == null)
            {
                diagnosticId = argument;

                continue;
            }

            if (directory == null)
            {
                directory = argument;

                continue;
            }

            error = "expected exactly one diagnostic ID and one fixture directory.";

            return false;
        }

        if (showHelp)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(diagnosticId) || string.IsNullOrWhiteSpace(directory))
        {
            error = "expected one diagnostic ID and one fixture directory.";

            return false;
        }

        return true;
    }

    #endregion // Methods
}