using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Cli.Abstractions;
using Reihitsu.Formatter;
using Reihitsu.Formatter.Utilities;

namespace Reihitsu.Cli;

/// <summary>
/// Handles the format command execution
/// </summary>
internal sealed class FormatCommandHandler
{
    #region Constants

    /// <summary>
    /// Maximum number of files a formatting run may write without confirmation (or --force)
    /// </summary>
    internal const int LargeRunConfirmationThreshold = 25;

    #endregion // Constants

    #region Fields

    /// <summary>
    /// UTF-8 encoding with a byte order mark used when encoding normalization is enabled
    /// </summary>
    private static readonly UTF8Encoding _utf8BomEncoding = new(encoderShouldEmitUTF8Identifier: true);

    /// <summary>
    /// Paths passed to the format command
    /// </summary>
    private readonly string[] _paths;

    /// <summary>
    /// Indicates whether the command should only check formatting
    /// </summary>
    private readonly bool _checkOnly;

    /// <summary>
    /// Indicates whether the command should report changes without writing them
    /// </summary>
    private readonly bool _dryRun;

    /// <summary>
    /// Indicates whether verbose output is enabled
    /// </summary>
    private readonly bool _verbose;

    /// <summary>
    /// Indicates whether the confirmation prompt for large formatting runs is skipped
    /// </summary>
    private readonly bool _force;

    /// <summary>
    /// Indicates whether processed files should be normalized to UTF-8 with a byte order mark
    /// </summary>
    private readonly bool _utf8Bom;

    /// <summary>
    /// Indicates whether a file the formatter refuses as unsupported is reported as a warning that does not change the exit
    /// code, instead of an error
    /// </summary>
    private readonly bool _suppressUnsupported;

    /// <summary>
    /// Maximum number of files processed at the same time
    /// </summary>
    private readonly int _maxParallelism;

    /// <summary>
    /// Parse options carrying the resolved C# language version, so parsing and formatting target the same version
    /// </summary>
    private readonly CSharpParseOptions _parseOptions;

    /// <summary>
    /// File-system abstraction used by the command
    /// </summary>
    private readonly IFileSystem _fileSystem;

    /// <summary>
    /// Console abstraction used for command output
    /// </summary>
    private readonly IConsoleOutput _console;

    /// <summary>
    /// Console abstraction used for reading confirmation input
    /// </summary>
    private readonly IConsoleInput _consoleInput;

    /// <summary>
    /// Source formatter used to process C# files
    /// </summary>
    private readonly ISourceFormatter _formatter;

    /// <summary>
    /// Diff generator used in dry-run mode
    /// </summary>
    private readonly IDiffGenerator _diffGenerator;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="FormatCommandHandler"/> class
    /// </summary>
    /// <param name="paths">One or more file or directory paths to process</param>
    /// <param name="checkOnly">If <see langword="true"/>, only check whether files are formatted without writing changes</param>
    /// <param name="dryRun">If <see langword="true"/>, show what would change without applying</param>
    /// <param name="verbose">If <see langword="true"/>, show detailed output for every file</param>
    /// <param name="force">If <see langword="true"/>, skip the confirmation prompt for large formatting runs</param>
    /// <param name="utf8Bom">If <see langword="true"/>, normalize processed files to UTF-8 with a byte order mark</param>
    /// <param name="languageVersion">The resolved C# language version every file is parsed and formatted for</param>
    /// <param name="dependencies">The command dependencies</param>
    /// <param name="suppressUnsupported">If <see langword="true"/>, report a file the formatter refuses as unsupported as a warning that does not change the exit code</param>
    /// <param name="maxParallelism">Maximum number of files processed at the same time; <c>1</c> processes one file after another</param>
    public FormatCommandHandler(string[] paths, bool checkOnly, bool dryRun, bool verbose, bool force, bool utf8Bom, LanguageVersion languageVersion, FormatCommandDependencies dependencies, bool suppressUnsupported, int maxParallelism)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxParallelism, 1);
        ArgumentNullException.ThrowIfNull(paths);
        ArgumentNullException.ThrowIfNull(dependencies);
        ArgumentNullException.ThrowIfNull(dependencies.FileSystem);
        ArgumentNullException.ThrowIfNull(dependencies.Console);
        ArgumentNullException.ThrowIfNull(dependencies.ConsoleInput);
        ArgumentNullException.ThrowIfNull(dependencies.Formatter);
        ArgumentNullException.ThrowIfNull(dependencies.DiffGenerator);

        _paths = paths;
        _checkOnly = checkOnly;
        _dryRun = dryRun;
        _verbose = verbose;
        _force = force;
        _utf8Bom = utf8Bom;
        _suppressUnsupported = suppressUnsupported;
        _maxParallelism = maxParallelism;
        _parseOptions = CSharpParseOptions.Default.WithLanguageVersion(languageVersion);
        _fileSystem = dependencies.FileSystem;
        _console = dependencies.Console;
        _consoleInput = dependencies.ConsoleInput;
        _formatter = dependencies.Formatter;
        _diffGenerator = dependencies.DiffGenerator;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Executes the format command
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests</param>
    /// <returns>Exit code: 0 = success, 1 = formatting needed (--check/--dry-run), 2 = error</returns>
    public async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        var missingPath = Array.Find(_paths, path => _fileSystem.FileExists(path) == false && _fileSystem.DirectoryExists(path) == false);

        if (missingPath != null)
        {
            _console.WriteErrorLine($"Path not found: {missingPath}");

            return ExitCodes.Error;
        }

        var files = CollectFiles();

        if (files.Count == 0)
        {
            _console.WriteLine("No .cs files found.");

            return ExitCodes.Success;
        }

        var confirmationExitCode = await ConfirmLargeRunIfRequiredAsync(files, cancellationToken).ConfigureAwait(false);

        if (confirmationExitCode != null)
        {
            return confirmationExitCode.Value;
        }

        var (totalFiles, changedFiles, skippedSyntaxErrors, skippedEncoding, skippedGenerated, errorFiles, skippedUnsupported, skippedHiddenCode) = await ProcessFilesAsync(files, cancellationToken).ConfigureAwait(false);

        PrintSummary(totalFiles, changedFiles, skippedSyntaxErrors, skippedEncoding, skippedGenerated, errorFiles, skippedUnsupported, skippedHiddenCode);

        // A file the formatter refuses is one the tool could not do its job on, unless the run reports it as a warning (CLI-017)
        if (errorFiles > 0
            || (skippedUnsupported > 0 && _suppressUnsupported == false))
        {
            return ExitCodes.Error;
        }

        if ((_checkOnly || _dryRun) && changedFiles > 0)
        {
            return ExitCodes.FormattingNeeded;
        }

        return ExitCodes.Success;
    }

    /// <summary>
    /// Determines whether a recursively discovered file is within a build output directory below the explicit
    /// directory target
    /// </summary>
    /// <param name="filePath">The file path to check</param>
    /// <param name="selectionRoot">The explicitly selected directory path</param>
    /// <param name="pathComparer">The platform-specific path comparer</param>
    /// <returns><see langword="true"/> if the file is in a build output directory; otherwise, <see langword="false"/></returns>
    private static bool IsInBuildOutputDirectory(string filePath, string selectionRoot, StringComparer pathComparer)
    {
        var relativePath = Path.GetRelativePath(selectionRoot, filePath);
        var segments = relativePath.Split(['\\', '/'], StringSplitOptions.RemoveEmptyEntries);

        // The final segment is the file name. Only recursively discovered directory segments are exclusions; the
        // explicitly selected root is absent from the relative path and therefore always remains eligible.
        for (var index = 0; index < segments.Length - 1; index++)
        {
            if (pathComparer.Equals(segments[index], "bin") || pathComparer.Equals(segments[index], "obj"))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether an encoding represents UTF-8 with a byte order mark
    /// </summary>
    /// <param name="encoding">The encoding to inspect</param>
    /// <returns><see langword="true"/> if the encoding is UTF-8 and emits the expected byte order mark; otherwise, <see langword="false"/></returns>
    private static bool IsUtf8WithBom(Encoding encoding)
    {
        return encoding.CodePage == _utf8BomEncoding.CodePage
               && encoding.GetPreamble().AsSpan().SequenceEqual(_utf8BomEncoding.GetPreamble());
    }

    /// <summary>
    /// Adds a file to the collection if it has not already been added
    /// </summary>
    /// <param name="fullPath">The full path of the file</param>
    /// <param name="files">The collection of files being built</param>
    /// <param name="seen">The set of full paths already added</param>
    private static void AddFile(string fullPath, List<string> files, HashSet<string> seen)
    {
        if (seen.Add(fullPath))
        {
            files.Add(fullPath);
        }
    }

    /// <summary>
    /// Processes the collected files, up to <see cref="_maxParallelism"/> of them at the same time, writes the lines of every
    /// file in selection order as soon as every file selected before it has finished, and aggregates the result counters
    /// (CLI-022)
    /// </summary>
    /// <param name="files">Files to process</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The aggregate file-processing counters</returns>
    /// <remarks>
    /// When the run is canceled, or a file's processing is canceled, no further file is started; the files in progress run
    /// to their end, the lines of every finished file are written in selection order, also behind an unfinished file, and the
    /// cancellation is rethrown without counters. In write mode, no file's read runs while a write is in progress, so a file
    /// reached through two selected paths is never read while the other path's write has truncated it (CLI-022); formatting
    /// stays parallel
    /// </remarks>
    private async Task<(int TotalFiles, int ChangedFiles, int SkippedSyntaxErrors, int SkippedEncoding, int SkippedGenerated, int ErrorFiles, int SkippedUnsupported, int SkippedHiddenCode)> ProcessFilesAsync(IReadOnlyList<string> files,
                                                                                                                                                                                                                CancellationToken cancellationToken)
    {
        var outputs = new BufferedConsoleOutput[files.Count];
        var results = new FileProcessResult?[files.Count];
        var outputGate = new object();
        var nextToWrite = 0;
        var tasks = new List<Task>(files.Count);
        ExceptionDispatchInfo cancellation = null;

        using (SemaphoreSlim slots = new(_maxParallelism, _maxParallelism), fileAccess = _checkOnly || _dryRun ? null : new(1, 1))
        {
            for (var index = 0; index < files.Count; index++)
            {
                try
                {
                    await slots.WaitAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                // A file whose processing was canceled stops the run like the run's own cancellation: no further file starts
                if (Volatile.Read(ref cancellation) != null)
                {
                    slots.Release();

                    break;
                }

                var fileIndex = index;

                outputs[fileIndex] = new BufferedConsoleOutput();

                tasks.Add(Task.Run(async () =>
                                   {
                                       try
                                       {
                                           var result = await ProcessSelectedFileAsync(files[fileIndex], outputs[fileIndex], fileAccess, cancellationToken).ConfigureAwait(false);

                                           lock (outputGate)
                                           {
                                               results[fileIndex] = result;

                                               while (nextToWrite < files.Count
                                                      && results[nextToWrite] != null)
                                               {
                                                   outputs[nextToWrite].FlushTo(_console);
                                                   nextToWrite++;
                                               }
                                           }
                                       }
                                       catch (OperationCanceledException ex)
                                       {
                                           Interlocked.CompareExchange(ref cancellation, ExceptionDispatchInfo.Capture(ex), null);
                                       }
                                       finally
                                       {
                                           slots.Release();
                                       }
                                   },
                                   CancellationToken.None));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        if (cancellation != null
            || cancellationToken.IsCancellationRequested)
        {
            // Every finished file is named, also when a file selected before it is unfinished (CLI-022)
            for (var index = nextToWrite; index < files.Count; index++)
            {
                if (results[index] != null)
                {
                    outputs[index].FlushTo(_console);
                }
            }

            cancellation?.Throw();
            cancellationToken.ThrowIfCancellationRequested();
        }

        var totalFiles = 0;
        var changedFiles = 0;
        var skippedSyntaxErrors = 0;
        var skippedEncoding = 0;
        var skippedGenerated = 0;
        var errorFiles = 0;
        var skippedUnsupported = 0;
        var skippedHiddenCode = 0;

        foreach (var result in results)
        {
            var value = result.GetValueOrDefault();

            totalFiles += value.SkippedGeneratedCount == 0 ? 1 : 0;
            changedFiles += value.ChangedFileCount;
            skippedSyntaxErrors += value.SkippedSyntaxErrorCount;
            skippedEncoding += value.SkippedEncodingCount;
            skippedGenerated += value.SkippedGeneratedCount;
            errorFiles += value.ErrorFileCount;
            skippedUnsupported += value.SkippedUnsupportedCount;
            skippedHiddenCode += value.SkippedHiddenCodeCount;
        }

        return (totalFiles, changedFiles, skippedSyntaxErrors, skippedEncoding, skippedGenerated, errorFiles, skippedUnsupported, skippedHiddenCode);
    }

    /// <summary>
    /// Processes one selected file: a file generated by its name is skipped without being read; every other file is processed
    /// by <see cref="ProcessFileAsync"/>
    /// </summary>
    /// <param name="filePath">The file path to process</param>
    /// <param name="console">The output that collects the file's lines</param>
    /// <param name="fileAccess">The gate every read and write of the run takes in write mode; <see langword="null"/> when nothing is written</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests</param>
    /// <returns>The processing counters for the file</returns>
    private async Task<FileProcessResult> ProcessSelectedFileAsync(string filePath, IConsoleOutput console, SemaphoreSlim fileAccess, CancellationToken cancellationToken)
    {
        if (GeneratedFileUtilities.IsGeneratedFile(filePath))
        {
            if (_verbose)
            {
                console.WriteLine($"Skipped (generated): {filePath}");
            }

            return FileProcessResult.SkippedGenerated;
        }

        return await ProcessFileAsync(filePath, console, fileAccess, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Processes a single file and returns aggregate counters for summary reporting
    /// </summary>
    /// <param name="filePath">The file path to process</param>
    /// <param name="console">The output that collects the file's lines</param>
    /// <param name="fileAccess">The gate every read and write of the run takes in write mode; <see langword="null"/> when nothing is written</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests</param>
    /// <returns>The processing counters for the file</returns>
    private async Task<FileProcessResult> ProcessFileAsync(string filePath, IConsoleOutput console, SemaphoreSlim fileAccess, CancellationToken cancellationToken)
    {
        try
        {
            var fileRead = await ReadFileAsync(filePath, fileAccess, cancellationToken).ConfigureAwait(false);
            var originalContent = fileRead.Content;
            var syntaxTree = CSharpSyntaxTree.ParseText(originalContent, _parseOptions, filePath, cancellationToken: cancellationToken);

            if (ReihitsuFormatterHelpers.HasSyntaxErrors(syntaxTree))
            {
                if (_verbose)
                {
                    console.WriteLine($"Skipped (syntax errors): {filePath}");
                }

                return FileProcessResult.SkippedSyntaxError;
            }

            if (_formatter.IsAutoGeneratedSource(syntaxTree, cancellationToken))
            {
                if (_verbose)
                {
                    console.WriteLine($"Skipped (generated): {filePath}");
                }

                return FileProcessResult.SkippedGenerated;
            }

            // The formatter leaves such a file as written; it is named as skipped, by the formatter's own definition, so that
            // the user learns why a file that may need formatting was left alone (CLI-021)
            if (_formatter.HoldsHidingCarrier(syntaxTree, cancellationToken))
            {
                if (_verbose)
                {
                    console.WriteLine($"Skipped (code hidden in another build configuration): {filePath}");
                }

                return FileProcessResult.SkippedHiddenCode;
            }

            var formattingResult = _formatter.FormatSyntaxTree(syntaxTree, cancellationToken);

            if (formattingResult.Unsupported != null)
            {
                ReportUnsupported(filePath, formattingResult.Unsupported, console);

                return FileProcessResult.SkippedUnsupported;
            }

            var formattedContent = (await formattingResult.Output.GetTextAsync(cancellationToken).ConfigureAwait(false)).ToString();
            var hasContentChanges = string.Equals(originalContent, formattedContent, StringComparison.Ordinal) == false;
            var hasEncodingChanges = _utf8Bom && IsUtf8WithBom(fileRead.Encoding) == false;

            if (hasContentChanges == false && hasEncodingChanges == false)
            {
                if (_verbose)
                {
                    console.WriteLine($"Unchanged: {filePath}");
                }

                return FileProcessResult.NoChange;
            }

            await HandleChangedFileAsync(filePath, originalContent, formattedContent, fileRead.Encoding, console, fileAccess, cancellationToken).ConfigureAwait(false);

            return FileProcessResult.Changed;
        }
        catch (OperationCanceledException)
        {
            // Cancellation is not a per-file error: let it propagate so the run starts no further file.
            throw;
        }
        catch (DecoderFallbackException)
        {
            console.WriteErrorLine($"Skipped (could not decode): {filePath}");

            return FileProcessResult.SkippedEncoding;
        }
        catch (Exception ex)
        {
            console.WriteErrorLine($"Error processing {filePath}: {ex.Message}");

            return FileProcessResult.Error;
        }
    }

    /// <summary>
    /// Reads a file; in write mode it takes the run's file-access gate, so the read never sees a file another selected path
    /// is writing
    /// </summary>
    /// <param name="filePath">The file path to read</param>
    /// <param name="fileAccess">The gate every read and write of the run takes in write mode; <see langword="null"/> when nothing is written</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests</param>
    /// <returns>The file's content and detected encoding</returns>
    private async Task<FileReadResult> ReadFileAsync(string filePath, SemaphoreSlim fileAccess, CancellationToken cancellationToken)
    {
        if (fileAccess == null)
        {
            return await _fileSystem.ReadFileAsync(filePath, cancellationToken).ConfigureAwait(false);
        }

        await fileAccess.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            return await _fileSystem.ReadFileAsync(filePath, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            fileAccess.Release();
        }
    }

    /// <summary>
    /// Reports a file the formatter refuses as unsupported on standard error, as an error, or as a warning with
    /// <c>--suppress-unsupported</c>, with the one-based line and column of the shape's first character (CLI-019, CLI-020)
    /// </summary>
    /// <param name="filePath">The file path</param>
    /// <param name="unsupported">The shape the formatter refuses the file for</param>
    /// <param name="console">The output that collects the file's lines</param>
    private void ReportUnsupported(string filePath, UnsupportedSyntax unsupported, IConsoleOutput console)
    {
        var severity = _suppressUnsupported ? "warning" : "error";
        var start = unsupported.Location.GetLineSpan().StartLinePosition;

        console.WriteErrorLine($"{severity}: {filePath}({start.Line + 1},{start.Character + 1}): unsupported syntax: {unsupported.Reason}. The file was left unchanged.");
    }

    /// <summary>
    /// Handles output and optional writing for a changed file; a write is not started once the run is canceled, and a write
    /// that has started is not interrupted, so a canceled run never leaves a truncated file (CLI-022). A write holds the run's
    /// file-access gate, so no read of the run sees the file while it is truncated
    /// </summary>
    /// <param name="filePath">The file path</param>
    /// <param name="originalContent">The original file content</param>
    /// <param name="formattedContent">The formatted file content</param>
    /// <param name="originalEncoding">The encoding detected while reading the original file</param>
    /// <param name="console">The output that collects the file's lines</param>
    /// <param name="fileAccess">The gate every read and write of the run takes in write mode; <see langword="null"/> when nothing is written</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests before the write starts</param>
    /// <returns>A task that completes when file handling is finished</returns>
    private async Task HandleChangedFileAsync(string filePath, string originalContent, string formattedContent, Encoding originalEncoding, IConsoleOutput console, SemaphoreSlim fileAccess, CancellationToken cancellationToken)
    {
        if (_checkOnly)
        {
            console.WriteLine($"Not formatted: {filePath}");

            return;
        }

        if (_dryRun)
        {
            console.WriteLine($"Would format: {filePath}");

            if (string.Equals(originalContent, formattedContent, StringComparison.Ordinal) == false)
            {
                console.WriteLine(_diffGenerator.Generate(filePath, originalContent, formattedContent));
            }

            return;
        }

        var outputEncoding = _utf8Bom ? _utf8BomEncoding : originalEncoding;

        await fileAccess.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _fileSystem.WriteAllTextAsync(filePath, formattedContent, outputEncoding)
                             .ConfigureAwait(false);
        }
        finally
        {
            fileAccess.Release();
        }

        console.WriteLine($"Formatted: {filePath}");
    }

    /// <summary>
    /// Determines whether a file is marked as auto-generated for large-run planning
    /// </summary>
    /// <param name="filePath">The file path to inspect</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests</param>
    /// <returns>A task containing <see langword="true"/> when the file is marked as auto-generated; otherwise, <see langword="false"/></returns>
    private async Task<bool> IsAutoGeneratedFileAsync(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            var sourceHeader = await _fileSystem.ReadSourceHeaderAsync(filePath, cancellationToken).ConfigureAwait(false);
            var syntaxTree = CSharpSyntaxTree.ParseText(sourceHeader, path: filePath, cancellationToken: cancellationToken);

            return _formatter.IsAutoGeneratedSource(syntaxTree, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            // Planning must not replace the normal per-file error and encoding handling. A file that cannot be
            // inspected remains a confirmation candidate and is processed through the usual path after confirmation.
            return false;
        }
    }

    /// <summary>
    /// Asks the user to confirm the run when it would write more than <see cref="LargeRunConfirmationThreshold"/> files
    /// </summary>
    /// <param name="files">The collected file paths</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests</param>
    /// <returns>A task containing the exit code to return when the run must not continue; otherwise, <see langword="null"/></returns>
    private async Task<int?> ConfirmLargeRunIfRequiredAsync(List<string> files, CancellationToken cancellationToken)
    {
        // Only the real formatting path writes files; --check and --dry-run never need confirmation.
        if (_checkOnly || _dryRun || _force)
        {
            return null;
        }

        var candidateFiles = files.Where(filePath => GeneratedFileUtilities.IsGeneratedFile(filePath) == false).ToArray();

        if (candidateFiles.Length <= LargeRunConfirmationThreshold)
        {
            return null;
        }

        var fileCount = 0;

        foreach (var filePath in candidateFiles)
        {
            if (await IsAutoGeneratedFileAsync(filePath, cancellationToken).ConfigureAwait(false) == false)
            {
                fileCount++;
            }
        }

        if (fileCount <= LargeRunConfirmationThreshold)
        {
            return null;
        }

        if (_consoleInput.IsInteractive == false)
        {
            _console.WriteErrorLine($"This run would format {fileCount} file(s), but standard input is not interactive, so the confirmation prompt cannot be answered. Use --force to format without confirmation.");

            return ExitCodes.Error;
        }

        _console.WriteLine($"This run will format {fileCount} file(s). Continue? [y/N]");

        var answer = _consoleInput.ReadLine()?.Trim();

        if (string.Equals(answer, "y", StringComparison.OrdinalIgnoreCase)
            || string.Equals(answer, "yes", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        _console.WriteLine("Formatting aborted. No files were changed.");

        return ExitCodes.Success;
    }

    /// <summary>
    /// Collects all C# source files from the configured paths
    /// </summary>
    /// <returns>A list of file paths to process</returns>
    private List<string> CollectFiles()
    {
        var files = new List<string>();

        // Normalize to full paths and track what has already been added so a file passed both directly and via a
        // parent directory (overlapping inputs) is formatted and counted only once. Path case sensitivity is
        // platform-dependent, so dedupe case-insensitively on Windows and case-sensitively elsewhere.
        var comparer = OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
        var seen = new HashSet<string>(comparer);

        foreach (var path in _paths)
        {
            if (_fileSystem.FileExists(path))
            {
                if (path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                {
                    AddFile(_fileSystem.GetFullPath(path), files, seen);
                }
            }
            else if (_fileSystem.DirectoryExists(path))
            {
                var selectionRoot = _fileSystem.GetFullPath(path);
                var directoryFiles = _fileSystem.EnumerateFiles(path, "*.cs", SearchOption.AllDirectories);

                foreach (var directoryFile in directoryFiles)
                {
                    var fullPath = _fileSystem.GetFullPath(directoryFile);

                    if (IsInBuildOutputDirectory(fullPath, selectionRoot, comparer) == false)
                    {
                        AddFile(fullPath, files, seen);
                    }
                }
            }
        }

        return files;
    }

    /// <summary>
    /// Prints a summary of the formatting operation to the console
    /// </summary>
    /// <param name="totalFiles">The total number of files processed</param>
    /// <param name="changedFiles">The number of files that were changed or need changes</param>
    /// <param name="skippedSyntaxErrors">The number of files skipped due to syntax errors</param>
    /// <param name="skippedEncoding">The number of files skipped because they could not be decoded using their detected encoding</param>
    /// <param name="skippedGenerated">The number of generated files that were skipped</param>
    /// <param name="errorFiles">The number of files that encountered errors during processing</param>
    /// <param name="skippedUnsupported">The number of files skipped because the formatter refuses them as unsupported</param>
    /// <param name="skippedHiddenCode">The number of files skipped because they hold code that another build configuration hides</param>
    private void PrintSummary(int totalFiles, int changedFiles, int skippedSyntaxErrors, int skippedEncoding, int skippedGenerated, int errorFiles, int skippedUnsupported, int skippedHiddenCode)
    {
        _console.WriteLine(string.Empty);

        if (_checkOnly)
        {
            _console.WriteLine($"{changedFiles} of {totalFiles} file(s) need formatting.");
        }
        else if (_dryRun)
        {
            _console.WriteLine($"{changedFiles} of {totalFiles} file(s) would be formatted.");
        }
        else
        {
            _console.WriteLine($"Formatted {changedFiles} of {totalFiles} file(s).");
        }

        if (skippedGenerated > 0)
        {
            _console.WriteLine($"Skipped {skippedGenerated} generated file(s).");
        }

        if (skippedSyntaxErrors > 0)
        {
            _console.WriteLine($"Skipped {skippedSyntaxErrors} file(s) with syntax errors.");
        }

        if (skippedHiddenCode > 0)
        {
            _console.WriteLine($"Skipped {skippedHiddenCode} file(s) with code hidden in another build configuration.");
        }

        if (skippedUnsupported > 0)
        {
            _console.WriteLine($"Skipped {skippedUnsupported} file(s) with unsupported syntax.");
        }

        if (skippedEncoding > 0)
        {
            _console.WriteLine($"Skipped {skippedEncoding} file(s) that could not be decoded.");
        }

        if (errorFiles > 0)
        {
            _console.WriteLine($"{errorFiles} file(s) encountered errors.");
        }
    }

    #endregion // Methods
}