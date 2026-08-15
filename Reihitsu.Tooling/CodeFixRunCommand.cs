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

    #region Fields

    /// <summary>
    /// Line-ending variants exercised for every fixture
    /// </summary>
    private static readonly string[] _lineEndings = [FixtureLineEndings.LineFeed, FixtureLineEndings.CarriageReturnLineFeed];

    #endregion // Fields

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
            return await ExecuteCoreAsync(diagnosticId, directory, maximumIterations, output, error, cancellationToken).ConfigureAwait(false);
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
    /// Validates the fixture run and executes every line-ending arm
    /// </summary>
    /// <param name="diagnosticId">Diagnostic ID whose code fix is exercised</param>
    /// <param name="directory">Fixture directory</param>
    /// <param name="maximumIterations">Maximum number of code actions per fixture arm</param>
    /// <param name="output">Standard output writer</param>
    /// <param name="error">Standard error writer</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The fixture command exit code</returns>
    private static async Task<int> ExecuteCoreAsync(string diagnosticId,
                                                    string directory,
                                                    int maximumIterations,
                                                    TextWriter output,
                                                    TextWriter error,
                                                    CancellationToken cancellationToken)
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

        var results = await RunFixturesAsync(fixtures, fullDirectory, target, maximumIterations, output, cancellationToken).ConfigureAwait(false);

        WriteSummary(output, fixtures.Count, results.Counters, results.LineEndingDrifts);

        return GetExitCode(results.Counters);
    }

    /// <summary>
    /// Runs every fixture with both supported line endings
    /// </summary>
    /// <param name="fixtures">Fixture paths</param>
    /// <param name="fullDirectory">Absolute fixture directory</param>
    /// <param name="target">Resolved analyzer and code fix target</param>
    /// <param name="maximumIterations">Maximum code actions per fixture arm</param>
    /// <param name="output">Standard output writer</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Outcome counters and line-ending drift count</returns>
    private static async Task<(Dictionary<FixtureOutcome, int> Counters, int LineEndingDrifts)> RunFixturesAsync(IReadOnlyCollection<string> fixtures,
                                                                                                                 string fullDirectory,
                                                                                                                 CodeFixTarget target,
                                                                                                                 int maximumIterations,
                                                                                                                 TextWriter output,
                                                                                                                 CancellationToken cancellationToken)
    {
        var counters = new Dictionary<FixtureOutcome, int>();
        var lineEndingDrifts = 0;

        foreach (var fixture in fixtures)
        {
            var displayPath = Path.GetRelativePath(fullDirectory, fixture).Replace('\\', '/');
            var source = await File.ReadAllTextAsync(fixture, cancellationToken).ConfigureAwait(false);

            foreach (var lineEnding in _lineEndings)
            {
                var result = await FixtureRunner.RunAsync(target, source, lineEnding, maximumIterations, cancellationToken).ConfigureAwait(false);

                counters[result.Outcome] = counters.GetValueOrDefault(result.Outcome) + 1;
                lineEndingDrifts += result.PreservedLineEnding ? 0 : 1;

                WriteFixtureReport(output, displayPath, lineEnding, result);
            }
        }

        return (counters, lineEndingDrifts);
    }

    /// <summary>
    /// Writes the aggregate fixture run summary
    /// </summary>
    /// <param name="output">Standard output writer</param>
    /// <param name="fixtureCount">Number of fixture files</param>
    /// <param name="counters">Outcome counters</param>
    /// <param name="lineEndingDrifts">Number of arms that changed line-ending style</param>
    private static void WriteSummary(TextWriter output,
                                     int fixtureCount,
                                     IReadOnlyDictionary<FixtureOutcome, int> counters,
                                     int lineEndingDrifts)
    {
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
        output.WriteLine($"CODE-FIX RUN: {fixtureCount} fixture(s) x 2 endings — {summary}");
    }

    /// <summary>
    /// Maps aggregate fixture outcomes to the command exit code
    /// </summary>
    /// <param name="counters">Outcome counters</param>
    /// <returns>The fixture command exit code</returns>
    private static int GetExitCode(IReadOnlyDictionary<FixtureOutcome, int> counters)
    {
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

        var positions = (DiagnosticId: (string)null, Directory: (string)null);
        var state = (IterationsSpecified: false, PositionalOnly: false, ShowHelp: false);

        var argumentIndex = 0;

        while (argumentIndex < args.Length)
        {
            var argument = args[argumentIndex++];

            if (TryParseArgument(args, ref argumentIndex, argument, ref positions, ref maximumIterations, ref state, out error) == false)
            {
                return false;
            }
        }

        diagnosticId = positions.DiagnosticId;
        directory = positions.Directory;
        showHelp = state.ShowHelp;

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

    /// <summary>
    /// Parses one code-fix fixture command argument
    /// </summary>
    /// <param name="args">Complete argument array</param>
    /// <param name="argumentIndex">Index of the next unread argument</param>
    /// <param name="argument">Current argument</param>
    /// <param name="positions">Parsed diagnostic ID and fixture directory</param>
    /// <param name="maximumIterations">Parsed iteration limit</param>
    /// <param name="state">Mutable option parsing state</param>
    /// <param name="error">Argument error when parsing fails</param>
    /// <returns><see langword="true"/> when the argument is valid; otherwise, <see langword="false"/></returns>
    private static bool TryParseArgument(string[] args,
                                         ref int argumentIndex,
                                         string argument,
                                         ref (string DiagnosticId, string Directory) positions,
                                         ref int maximumIterations,
                                         ref (bool IterationsSpecified, bool PositionalOnly, bool ShowHelp) state,
                                         out string error)
    {
        error = null;

        if (state.PositionalOnly == false && (argument is "--help" or "-h"))
        {
            state.ShowHelp = true;

            return true;
        }

        if (state.PositionalOnly == false && argument == "--")
        {
            state.PositionalOnly = true;

            return true;
        }

        if (state.PositionalOnly == false && argument == "--max-iterations")
        {
            return TryParseMaximumIterations(args, ref argumentIndex, ref maximumIterations, ref state, out error);
        }

        if (state.PositionalOnly == false && argument.StartsWith('-'))
        {
            error = $"unknown argument '{argument}'.";

            return false;
        }

        return TryAddPosition(argument, ref positions, out error);
    }

    /// <summary>
    /// Parses the value of the <c>--max-iterations</c> option
    /// </summary>
    /// <param name="args">Complete argument array</param>
    /// <param name="argumentIndex">Index of the option value</param>
    /// <param name="maximumIterations">Parsed iteration limit</param>
    /// <param name="state">Mutable option parsing state</param>
    /// <param name="error">Argument error when parsing fails</param>
    /// <returns><see langword="true"/> when the option is valid; otherwise, <see langword="false"/></returns>
    private static bool TryParseMaximumIterations(string[] args,
                                                  ref int argumentIndex,
                                                  ref int maximumIterations,
                                                  ref (bool IterationsSpecified, bool PositionalOnly, bool ShowHelp) state,
                                                  out string error)
    {
        error = null;

        if (state.IterationsSpecified)
        {
            error = "--max-iterations may only be specified once.";

            return false;
        }

        if (argumentIndex >= args.Length
            || int.TryParse(args[argumentIndex++], out maximumIterations) == false
            || maximumIterations <= 0)
        {
            error = "--max-iterations requires a positive integer.";

            return false;
        }

        state.IterationsSpecified = true;

        return true;
    }

    /// <summary>
    /// Adds one positional argument to the next available slot
    /// </summary>
    /// <param name="argument">Positional argument</param>
    /// <param name="positions">Parsed diagnostic ID and fixture directory</param>
    /// <param name="error">Argument error when no slot remains</param>
    /// <returns><see langword="true"/> when the argument was added; otherwise, <see langword="false"/></returns>
    private static bool TryAddPosition(string argument,
                                       ref (string DiagnosticId, string Directory) positions,
                                       out string error)
    {
        error = null;

        if (positions.DiagnosticId == null)
        {
            positions.DiagnosticId = argument;

            return true;
        }

        if (positions.Directory == null)
        {
            positions.Directory = argument;

            return true;
        }

        error = "expected exactly one diagnostic ID and one fixture directory.";

        return false;
    }

    #endregion // Methods
}