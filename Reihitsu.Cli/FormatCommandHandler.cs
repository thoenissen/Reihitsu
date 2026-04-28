using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Cli.Abstractions;

namespace Reihitsu.Cli;

/// <summary>
/// Handles the format command execution
/// </summary>
internal sealed class FormatCommandHandler
{
    #region Fields

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
    /// File-system abstraction used by the command
    /// </summary>
    private readonly IFileSystem _fileSystem;

    /// <summary>
    /// Console abstraction used for command output
    /// </summary>
    private readonly IConsoleOutput _console;

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
    /// <param name="dependencies">The command dependencies</param>
    public FormatCommandHandler(string[] paths, bool checkOnly, bool dryRun, bool verbose, FormatCommandDependencies dependencies)
    {
        ArgumentNullException.ThrowIfNull(paths);
        ArgumentNullException.ThrowIfNull(dependencies);
        ArgumentNullException.ThrowIfNull(dependencies.FileSystem);
        ArgumentNullException.ThrowIfNull(dependencies.Console);
        ArgumentNullException.ThrowIfNull(dependencies.Formatter);
        ArgumentNullException.ThrowIfNull(dependencies.DiffGenerator);

        _paths = paths;
        _checkOnly = checkOnly;
        _dryRun = dryRun;
        _verbose = verbose;
        _fileSystem = dependencies.FileSystem;
        _console = dependencies.Console;
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
        var files = CollectFiles();

        if (files.Count == 0)
        {
            _console.WriteLine("No .cs files found.");

            return ExitCodes.Error;
        }

        var totalFiles = 0;
        var changedFiles = 0;
        var skippedSyntaxErrors = 0;
        var skippedGenerated = 0;
        var errorFiles = 0;

        foreach (var filePath in files)
        {
            if (IsGeneratedFile(filePath))
            {
                skippedGenerated++;

                if (_verbose)
                {
                    _console.WriteLine($"Skipped (generated): {filePath}");
                }

                continue;
            }

            totalFiles++;

            var processResult = await ProcessFileAsync(filePath, cancellationToken).ConfigureAwait(false);

            changedFiles += processResult.ChangedFileCount;
            skippedSyntaxErrors += processResult.SkippedSyntaxErrorCount;
            errorFiles += processResult.ErrorFileCount;
        }

        PrintSummary(totalFiles, changedFiles, skippedSyntaxErrors, skippedGenerated, errorFiles);

        if (errorFiles > 0)
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
    /// Determines whether the specified file path is within a build output directory (bin/ or obj/)
    /// </summary>
    /// <param name="filePath">The file path to check</param>
    /// <returns><see langword="true"/> if the file is in a build output directory; otherwise, <see langword="false"/></returns>
    private static bool IsInBuildOutputDirectory(string filePath)
    {
        var normalizedPath = filePath.Replace('\\', '/');

        return normalizedPath.Contains("/bin/", StringComparison.OrdinalIgnoreCase)
               || normalizedPath.Contains("/obj/", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines whether the specified file is a generated file that should be skipped
    /// </summary>
    /// <param name="filePath">The file path to check</param>
    /// <returns><see langword="true"/> if the file is a generated file; otherwise, <see langword="false"/></returns>
    private static bool IsGeneratedFile(string filePath)
    {
        return filePath.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase)
               || filePath.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase)
               || filePath.EndsWith(".g.i.cs", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines whether the specified syntax tree contains syntax errors
    /// </summary>
    /// <param name="syntaxTree">The syntax tree to check</param>
    /// <returns><see langword="true"/> if the syntax tree has errors; otherwise, <see langword="false"/></returns>
    private static bool HasSyntaxErrors(SyntaxTree syntaxTree)
    {
        return syntaxTree.GetDiagnostics().Any(d => d.Severity == DiagnosticSeverity.Error);
    }

    /// <summary>
    /// Processes a single file and returns aggregate counters for summary reporting
    /// </summary>
    /// <param name="filePath">The file path to process</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests</param>
    /// <returns>The processing counters for the file</returns>
    private async Task<FileProcessResult> ProcessFileAsync(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            var originalContent = await _fileSystem.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
            var syntaxTree = CSharpSyntaxTree.ParseText(originalContent, path: filePath, cancellationToken: cancellationToken);

            if (HasSyntaxErrors(syntaxTree))
            {
                if (_verbose)
                {
                    _console.WriteLine($"Skipped (syntax errors): {filePath}");
                }

                return FileProcessResult.SkippedSyntaxError;
            }

            var formattedTree = _formatter.FormatSyntaxTree(syntaxTree, cancellationToken);
            var formattedContent = (await formattedTree.GetTextAsync(cancellationToken)).ToString();
            var hasChanges = string.Equals(originalContent, formattedContent, StringComparison.Ordinal) == false;

            if (hasChanges == false)
            {
                if (_verbose)
                {
                    _console.WriteLine($"Unchanged: {filePath}");
                }

                return FileProcessResult.NoChange;
            }

            await HandleChangedFileAsync(filePath, originalContent, formattedContent, cancellationToken).ConfigureAwait(false);

            return FileProcessResult.Changed;
        }
        catch (Exception ex)
        {
            await _console.WriteErrorLineAsync($"Error processing {filePath}: {ex.Message}");

            return FileProcessResult.Error;
        }
    }

    /// <summary>
    /// Handles output and optional writing for a changed file
    /// </summary>
    /// <param name="filePath">The file path</param>
    /// <param name="originalContent">The original file content</param>
    /// <param name="formattedContent">The formatted file content</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests</param>
    /// <returns>A task that completes when file handling is finished</returns>
    private async Task HandleChangedFileAsync(string filePath, string originalContent, string formattedContent, CancellationToken cancellationToken)
    {
        if (_checkOnly)
        {
            _console.WriteLine($"Not formatted: {filePath}");

            return;
        }

        if (_dryRun)
        {
            _console.WriteLine($"Would format: {filePath}");
            _console.WriteLine(_diffGenerator.Generate(filePath, originalContent, formattedContent));

            return;
        }

        var originalEncoding = await _fileSystem.DetectEncodingAsync(filePath, cancellationToken)
                                                .ConfigureAwait(false);

        await _fileSystem.WriteAllTextAsync(filePath, formattedContent, originalEncoding, cancellationToken)
                         .ConfigureAwait(false);
        _console.WriteLine($"Formatted: {filePath}");
    }

    /// <summary>
    /// Collects all C# source files from the configured paths
    /// </summary>
    /// <returns>A list of file paths to process</returns>
    private List<string> CollectFiles()
    {
        var files = new List<string>();

        foreach (var path in _paths)
        {
            if (_fileSystem.FileExists(path))
            {
                if (path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                {
                    files.Add(_fileSystem.GetFullPath(path));
                }
            }
            else if (_fileSystem.DirectoryExists(path))
            {
                var directoryFiles = _fileSystem.EnumerateFiles(path, "*.cs", SearchOption.AllDirectories)
                                                .Where(f => IsInBuildOutputDirectory(f) == false);

                files.AddRange(directoryFiles);
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
    /// <param name="skippedGenerated">The number of generated files that were skipped</param>
    /// <param name="errorFiles">The number of files that encountered errors during processing</param>
    private void PrintSummary(int totalFiles, int changedFiles, int skippedSyntaxErrors, int skippedGenerated, int errorFiles)
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

        if (errorFiles > 0)
        {
            _console.WriteLine($"{errorFiles} file(s) encountered errors.");
        }
    }

    #endregion // Methods
}