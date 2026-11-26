using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Cli.Abstractions;
using Reihitsu.Formatter;
using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline;
using Reihitsu.Formatter.Utilities;

namespace Reihitsu.Cli.Diagnostics;

/// <summary>
/// Runs the formatter repeatedly while reporting the source changes made by each phase the pipeline reports, the
/// preparation phase as its two sub-phases
/// </summary>
internal static class PipelineTraceCommand
{
    #region Constants

    /// <summary>
    /// Maximum number of formatting passes used when the caller does not provide an override
    /// </summary>
    private const int DefaultPasses = 3;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Executes the repository-only pipeline trace command
    /// </summary>
    /// <param name="args">Trace command arguments</param>
    /// <param name="output">Standard output writer</param>
    /// <param name="error">Standard error writer</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// Exit code 0 when the input stabilizes or is legitimately skipped, 1 when it still changes after the requested
    /// passes, or 2 when the command cannot run
    /// </returns>
    internal static async Task<int> ExecuteAsync(string[] args,
                                                 TextWriter output,
                                                 TextWriter error,
                                                 CancellationToken cancellationToken = default)
    {
        if (TryParseArguments(args, out var filePath, out var passes, out var languageVersion, out var showHelp, out var parseError) == false)
        {
            await error.WriteLineAsync($"trace: {parseError}", cancellationToken).ConfigureAwait(false);
            PrintUsage(error);

            return ExitCodes.Error;
        }

        if (showHelp)
        {
            PrintUsage(output);

            return ExitCodes.Success;
        }

        if (languageVersion.ExceedingValue != null)
        {
            var newestVersion = languageVersion.Resolved.ToDisplayString();

            await error.WriteLineAsync($"trace: Warning: C# language version '{languageVersion.ExceedingValue}' is newer than the newest supported version {newestVersion}; formatting for {newestVersion}.", cancellationToken).ConfigureAwait(false);
        }

        try
        {
            return await ExecuteCoreAsync(filePath, passes, languageVersion.Resolved, output, error, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            await error.WriteLineAsync("trace: operation canceled.", CancellationToken.None).ConfigureAwait(false);

            return ExitCodes.Error;
        }
        catch (Exception exception)
        {
            await error.WriteLineAsync($"trace: {exception.Message}", CancellationToken.None).ConfigureAwait(false);

            return ExitCodes.Error;
        }
    }

    /// <summary>
    /// Validates the trace input and executes the requested formatting passes
    /// </summary>
    /// <param name="filePath">Input file path</param>
    /// <param name="passes">Maximum number of formatting passes</param>
    /// <param name="languageVersion">Resolved C# language version every pass is parsed and formatted for</param>
    /// <param name="output">Standard output writer</param>
    /// <param name="error">Standard error writer</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The trace command exit code</returns>
    private static async Task<int> ExecuteCoreAsync(string filePath,
                                                    int passes,
                                                    LanguageVersion languageVersion,
                                                    TextWriter output,
                                                    TextWriter error,
                                                    CancellationToken cancellationToken)
    {
        var fileSystem = new DefaultFileSystem();
        var fullPath = fileSystem.GetFullPath(filePath);

        if (fileSystem.FileExists(fullPath) == false)
        {
            var message = fileSystem.DirectoryExists(fullPath)
                              ? $"trace: '{filePath}' is a directory; expected one C# file."
                              : $"trace: file not found: {filePath}";

            await error.WriteLineAsync(message, cancellationToken).ConfigureAwait(false);

            return ExitCodes.Error;
        }

        if (GeneratedFileUtilities.IsGeneratedFile(fullPath))
        {
            await output.WriteLineAsync($"Skipped (generated): {fullPath}", cancellationToken).ConfigureAwait(false);

            return ExitCodes.Success;
        }

        var fileRead = await fileSystem.ReadFileAsync(fullPath, cancellationToken).ConfigureAwait(false);

        await output.WriteLineAsync($"Tracing {fullPath} (maximum passes: {passes})", cancellationToken).ConfigureAwait(false);

        return TracePasses(fileRead.Content, fullPath, passes, languageVersion, output, error, cancellationToken);
    }

    /// <summary>
    /// Runs formatting passes until the source stabilizes or the pass limit is reached
    /// </summary>
    /// <param name="currentContent">Initial source text</param>
    /// <param name="fullPath">Absolute input path, which also names the file in every diff header</param>
    /// <param name="passes">Maximum number of passes</param>
    /// <param name="languageVersion">Resolved C# language version every pass is parsed and formatted for</param>
    /// <param name="output">Standard output writer</param>
    /// <param name="error">Standard error writer</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The trace command exit code</returns>
    private static int TracePasses(string currentContent,
                                   string fullPath,
                                   int passes,
                                   LanguageVersion languageVersion,
                                   TextWriter output,
                                   TextWriter error,
                                   CancellationToken cancellationToken)
    {
        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(languageVersion);

        for (var pass = 1; pass <= passes; pass++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var syntaxTree = CSharpSyntaxTree.ParseText(currentContent, parseOptions, fullPath, cancellationToken: cancellationToken);
            var root = syntaxTree.GetRoot(cancellationToken);
            var invalidExitCode = GetInvalidTraceExitCode(syntaxTree, root, fullPath, pass, output, error);

            if (invalidExitCode.HasValue)
            {
                return invalidExitCode.Value;
            }

            output.WriteLine($"Pass {pass}");

            var passInput = currentContent;
            var context = new FormattingContext(ReihitsuFormatterHelpers.DetectEndOfLine(root),
                                                languageVersion: languageVersion);

            var passResult = ExecuteFormattingPass(root, context, fullPath, pass, output, error, cancellationToken);

            if (passResult.Success == false)
            {
                return ExitCodes.Error;
            }

            currentContent = passResult.Root.ToFullString();

            if (string.Equals(passInput, currentContent, StringComparison.Ordinal))
            {
                output.WriteLine($"Stable after pass {pass} ({passResult.ChangedPhaseCount} phase changes observed).");

                return ExitCodes.Success;
            }

            output.WriteLine($"Pass {pass} changed source in {passResult.ChangedPhaseCount} phase(s).");
        }

        output.WriteLine($"Not stable after {passes} pass(es).");

        return ExitCodes.FormattingNeeded;
    }

    /// <summary>
    /// Validates a parsed pass and determines whether tracing must stop: whole-document formatting leaves a source with
    /// syntax errors, a generated source, and a source that holds code hidden in another build configuration unchanged,
    /// checked in that order
    /// </summary>
    /// <param name="syntaxTree">Parsed source tree</param>
    /// <param name="root">Parsed source root</param>
    /// <param name="fullPath">Absolute input path</param>
    /// <param name="pass">Current pass number</param>
    /// <param name="output">Standard output writer</param>
    /// <param name="error">Standard error writer</param>
    /// <returns>The terminal exit code when tracing must stop; otherwise, <see langword="null"/></returns>
    private static int? GetInvalidTraceExitCode(SyntaxTree syntaxTree,
                                                SyntaxNode root,
                                                string fullPath,
                                                int pass,
                                                TextWriter output,
                                                TextWriter error)
    {
        if (ReihitsuFormatterHelpers.HasSyntaxErrors(syntaxTree))
        {
            WriteInvalidPassMessage(pass, fullPath, "syntax errors", "syntax-invalid source", output, error);

            return pass == 1 ? ExitCodes.Success : ExitCodes.Error;
        }

        if (ReihitsuFormatterHelpers.IsAutoGeneratedSource(root))
        {
            WriteInvalidPassMessage(pass, fullPath, "generated", "generated source", output, error);

            return pass == 1 ? ExitCodes.Success : ExitCodes.Error;
        }

        if (ReihitsuFormatter.HoldsHidingCarrier(syntaxTree))
        {
            WriteInvalidPassMessage(pass, fullPath, "code hidden in another build configuration", "source with code hidden in another build configuration", output, error);

            return pass == 1 ? ExitCodes.Success : ExitCodes.Error;
        }

        return null;
    }

    /// <summary>
    /// Writes the appropriate first-pass skip or later-pass failure message
    /// </summary>
    /// <param name="pass">Current pass number</param>
    /// <param name="fullPath">Absolute input path</param>
    /// <param name="skipReason">Reason displayed when the original input is skipped</param>
    /// <param name="failureReason">Reason displayed when a later pass becomes invalid</param>
    /// <param name="output">Standard output writer</param>
    /// <param name="error">Standard error writer</param>
    private static void WriteInvalidPassMessage(int pass,
                                                string fullPath,
                                                string skipReason,
                                                string failureReason,
                                                TextWriter output,
                                                TextWriter error)
    {
        if (pass == 1)
        {
            output.WriteLine($"Skipped ({skipReason}): {fullPath}");

            return;
        }

        error.WriteLine($"trace: pass {pass} produced {failureReason} before the next pass could run.");
    }

    /// <summary>
    /// Executes one formatting pass and reports each phase that changes source text
    /// </summary>
    /// <param name="root">Input root</param>
    /// <param name="context">Formatting context</param>
    /// <param name="fullPath">Absolute input path, which names the file in every diff header</param>
    /// <param name="pass">Current pass number</param>
    /// <param name="output">Standard output writer</param>
    /// <param name="error">Standard error writer</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The pass status, formatted root, and number of phases that changed source text</returns>
    private static (bool Success, SyntaxNode Root, int ChangedPhaseCount) ExecuteFormattingPass(SyntaxNode root,
                                                                                                FormattingContext context,
                                                                                                string fullPath,
                                                                                                int pass,
                                                                                                TextWriter output,
                                                                                                TextWriter error,
                                                                                                CancellationToken cancellationToken)
    {
        var observedChanges = 0;
        string activePhase = null;

        try
        {
            var result = FormattingPipeline.Execute(root,
                                                    context,
                                                    (phaseName, phaseInput, phaseOutput) =>
                                                    {
                                                        activePhase = phaseName;

                                                        var before = phaseInput.ToFullString();
                                                        var after = phaseOutput.ToFullString();

                                                        if (string.Equals(before, after, StringComparison.Ordinal))
                                                        {
                                                            return;
                                                        }

                                                        observedChanges++;
                                                        WritePhaseChange(output, fullPath, pass, phaseName, before, after);
                                                    },
                                                    cancellationToken);

            if (result.Unsupported != null)
            {
                error.WriteLine($"trace: pass {pass}: unsupported syntax: {result.Unsupported.Reason}. The input was left unchanged.");
            }

            return (true, result.Output, observedChanges);
        }
        catch (InvalidOperationException exception) when (exception.Message.StartsWith("Formatting phase '", StringComparison.Ordinal))
        {
            error.WriteLine($"trace: pass {pass}: {exception.Message} {exception.InnerException?.Message}");
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            var phaseContext = activePhase == null ? string.Empty : $", phase {activePhase}";

            error.WriteLine($"trace: pass {pass}{phaseContext}: {exception.Message}");
        }

        return (false, null, observedChanges);
    }

    /// <summary>
    /// Writes the diff or line-ending change produced by one formatter phase. The diff header names the file as the diff of
    /// <c>reihitsu-format --dry-run</c> does, relative to the current directory when the file lies inside it and by its full
    /// path otherwise
    /// </summary>
    /// <param name="output">Standard output writer</param>
    /// <param name="fullPath">Absolute input path</param>
    /// <param name="pass">Current pass number</param>
    /// <param name="phaseName">Formatter phase name</param>
    /// <param name="before">Input text</param>
    /// <param name="after">Output text</param>
    private static void WritePhaseChange(TextWriter output,
                                         string fullPath,
                                         int pass,
                                         string phaseName,
                                         string before,
                                         string after)
    {
        output.WriteLine($"== Pass {pass}: {phaseName} ==");

        var diff = new DefaultDiffGenerator().Generate(fullPath, before, after);

        if (diff.Length > 0)
        {
            output.Write(diff);

            return;
        }

        output.WriteLine($"Line endings: {DescribeLineEndings(before)} -> {DescribeLineEndings(after)}");
    }

    /// <summary>
    /// Describes the ordinary line-ending sequences contained in source text
    /// </summary>
    /// <param name="text">Source text to inspect</param>
    /// <returns>Counts for CRLF, LF, and CR line endings</returns>
    private static string DescribeLineEndings(string text)
    {
        var crlf = 0;
        var lf = 0;
        var cr = 0;

        var index = 0;

        while (index < text.Length)
        {
            if (text[index] == '\r')
            {
                if (index + 1 < text.Length && text[index + 1] == '\n')
                {
                    crlf++;
                    index++;
                }
                else
                {
                    cr++;
                }
            }
            else if (text[index] == '\n')
            {
                lf++;
            }

            index++;
        }

        return $"CRLF={crlf}, LF={lf}, CR={cr}";
    }

    /// <summary>
    /// Prints trace command usage
    /// </summary>
    /// <param name="writer">Writer that receives the usage text</param>
    private static void PrintUsage(TextWriter writer)
    {
        writer.WriteLine("usage: trace <file> [--passes <positive-integer>] [--lang-version <version>]");
        writer.WriteLine("       trace --help");
    }

    /// <summary>
    /// Parses trace command arguments
    /// </summary>
    /// <param name="args">Arguments to parse</param>
    /// <param name="filePath">The single input file path</param>
    /// <param name="passes">Maximum number of formatting passes</param>
    /// <param name="languageVersion">
    /// Resolved C# language version, the newest supported one without <c>--lang-version</c>, and the option's value when it
    /// names a version newer than that
    /// </param>
    /// <param name="showHelp">Whether usage should be printed</param>
    /// <param name="error">Argument error when parsing fails</param>
    /// <returns><see langword="true"/> when the arguments are valid; otherwise, <see langword="false"/></returns>
    private static bool TryParseArguments(string[] args,
                                          out string filePath,
                                          out int passes,
                                          out (LanguageVersion Resolved, string ExceedingValue) languageVersion,
                                          out bool showHelp,
                                          out string error)
    {
        filePath = null;
        passes = DefaultPasses;
        languageVersion = (LanguageVersionResolver.MaxSupportedLanguageVersion, null);
        showHelp = false;
        error = null;

        var state = (PassesSpecified: false, LanguageVersionSpecified: false, PathsOnly: false, ShowHelp: false);

        var argumentIndex = 0;

        while (argumentIndex < args.Length)
        {
            var argument = args[argumentIndex++];

            if (TryParseArgument(args,
                                 ref argumentIndex,
                                 argument,
                                 ref filePath,
                                 ref passes,
                                 ref languageVersion,
                                 ref state,
                                 out error) == false)
            {
                return false;
            }
        }

        showHelp = state.ShowHelp;

        if (showHelp)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(filePath))
        {
            error = "expected one input file.";

            return false;
        }

        return true;
    }

    /// <summary>
    /// Parses one trace command argument
    /// </summary>
    /// <param name="args">Complete argument array</param>
    /// <param name="argumentIndex">Index of the next unread argument</param>
    /// <param name="argument">Current argument</param>
    /// <param name="filePath">Parsed input file path</param>
    /// <param name="passes">Parsed pass limit</param>
    /// <param name="languageVersion">Parsed language version</param>
    /// <param name="state">Mutable option parsing state</param>
    /// <param name="error">Argument error when parsing fails</param>
    /// <returns><see langword="true"/> when the argument is valid; otherwise, <see langword="false"/></returns>
    private static bool TryParseArgument(string[] args,
                                         ref int argumentIndex,
                                         string argument,
                                         ref string filePath,
                                         ref int passes,
                                         ref (LanguageVersion Resolved, string ExceedingValue) languageVersion,
                                         ref (bool PassesSpecified, bool LanguageVersionSpecified, bool PathsOnly, bool ShowHelp) state,
                                         out string error)
    {
        error = null;

        if (state.PathsOnly == false && (argument is "--help" or "-h"))
        {
            state.ShowHelp = true;

            return true;
        }

        if (state.PathsOnly == false && argument == "--")
        {
            state.PathsOnly = true;

            return true;
        }

        if (state.PathsOnly == false && argument == "--passes")
        {
            return TryParsePasses(args, ref argumentIndex, ref passes, ref state, out error);
        }

        if (state.PathsOnly == false && argument == "--lang-version")
        {
            return TryParseLanguageVersion(args, ref argumentIndex, ref languageVersion, ref state, out error);
        }

        if (state.PathsOnly == false && argument.StartsWith('-'))
        {
            error = $"unknown argument '{argument}'.";

            return false;
        }

        if (filePath != null)
        {
            error = "expected exactly one input file.";

            return false;
        }

        filePath = argument;

        return true;
    }

    /// <summary>
    /// Parses the value of the <c>--passes</c> option
    /// </summary>
    /// <param name="args">Complete argument array</param>
    /// <param name="argumentIndex">Index of the option value</param>
    /// <param name="passes">Parsed pass limit</param>
    /// <param name="state">Mutable option parsing state</param>
    /// <param name="error">Argument error when parsing fails</param>
    /// <returns><see langword="true"/> when the option is valid; otherwise, <see langword="false"/></returns>
    private static bool TryParsePasses(string[] args,
                                       ref int argumentIndex,
                                       ref int passes,
                                       ref (bool PassesSpecified, bool LanguageVersionSpecified, bool PathsOnly, bool ShowHelp) state,
                                       out string error)
    {
        error = null;

        if (state.PassesSpecified)
        {
            error = "--passes may only be specified once.";

            return false;
        }

        if (argumentIndex >= args.Length
            || int.TryParse(args[argumentIndex++], out passes) == false
            || passes <= 0)
        {
            error = "--passes requires a positive integer.";

            return false;
        }

        state.PassesSpecified = true;

        return true;
    }

    /// <summary>
    /// Parses the value of the <c>--lang-version</c> option, which is always the argument behind it, and resolves it as
    /// <c>reihitsu-format</c> does
    /// </summary>
    /// <param name="args">Complete argument array</param>
    /// <param name="argumentIndex">Index of the option value</param>
    /// <param name="languageVersion">Resolved language version and the value when it names a version newer than the newest supported one</param>
    /// <param name="state">Mutable option parsing state</param>
    /// <param name="error">Argument error when parsing fails</param>
    /// <returns><see langword="true"/> when the option is valid; otherwise, <see langword="false"/></returns>
    private static bool TryParseLanguageVersion(string[] args,
                                                ref int argumentIndex,
                                                ref (LanguageVersion Resolved, string ExceedingValue) languageVersion,
                                                ref (bool PassesSpecified, bool LanguageVersionSpecified, bool PathsOnly, bool ShowHelp) state,
                                                out string error)
    {
        error = null;

        if (state.LanguageVersionSpecified)
        {
            error = "--lang-version may only be specified once.";

            return false;
        }

        // LanguageVersionFacts.TryParse would accept a missing value as the default version, which is why it never sees one.
        if (argumentIndex >= args.Length)
        {
            error = "missing value for --lang-version.";

            return false;
        }

        var value = args[argumentIndex++];

        if (LanguageVersionFacts.TryParse(value, out var requestedVersion) == false)
        {
            error = $"invalid value for --lang-version: '{value}'. Expected a C# language version such as 12, 12.0, latest, or preview.";

            return false;
        }

        languageVersion = (LanguageVersionResolver.Resolve(requestedVersion), LanguageVersionResolver.ExceedsMaximum(requestedVersion) ? value : null);
        state.LanguageVersionSpecified = true;

        return true;
    }

    #endregion // Methods
}