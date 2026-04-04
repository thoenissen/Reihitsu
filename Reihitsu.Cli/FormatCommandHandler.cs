using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Cli.Abstractions;

namespace Reihitsu.Cli;

/// <summary>
/// Handles the format command execution.
/// </summary>
internal sealed class FormatCommandHandler
{
    #region Fields

    private readonly string[] _paths;
    private readonly bool _checkOnly;
    private readonly bool _dryRun;
    private readonly bool _verbose;
    private readonly IFileSystem _fileSystem;
    private readonly IConsoleOutput _console;
    private readonly ISourceFormatter _formatter;
    private readonly IDiffGenerator _diffGenerator;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="FormatCommandHandler"/> class.
    /// </summary>
    /// <param name="paths">One or more file or directory paths to process.</param>
    /// <param name="checkOnly">If <see langword="true"/>, only check whether files are formatted without writing changes.</param>
    /// <param name="dryRun">If <see langword="true"/>, show what would change without applying.</param>
    /// <param name="verbose">If <see langword="true"/>, show detailed output for every file.</param>
    /// <param name="fileSystem">The file system abstraction.</param>
    /// <param name="console">The console output abstraction.</param>
    /// <param name="formatter">The source formatter abstraction.</param>
    /// <param name="diffGenerator">The diff generator abstraction.</param>
    public FormatCommandHandler(string[] paths, bool checkOnly, bool dryRun, bool verbose, IFileSystem fileSystem, IConsoleOutput console, ISourceFormatter formatter, IDiffGenerator diffGenerator)
    {
        _paths = paths;
        _checkOnly = checkOnly;
        _dryRun = dryRun;
        _verbose = verbose;
        _fileSystem = fileSystem;
        _console = console;
        _formatter = formatter;
        _diffGenerator = diffGenerator;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Executes the format command.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>Exit code: 0 = success, 1 = formatting needed (--check/--dry-run), 2 = error.</returns>
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

            try
            {
                var originalContent = await _fileSystem.ReadAllTextAsync(filePath, cancellationToken).ConfigureAwait(false);
                var syntaxTree = CSharpSyntaxTree.ParseText(originalContent, path: filePath, cancellationToken: cancellationToken);

                if (HasSyntaxErrors(syntaxTree))
                {
                    skippedSyntaxErrors++;

                    if (_verbose)
                    {
                        _console.WriteLine($"Skipped (syntax errors): {filePath}");
                    }

                    continue;
                }

                var formattedTree = _formatter.FormatSyntaxTree(syntaxTree, cancellationToken);
                var formattedContent = (await formattedTree.GetTextAsync(cancellationToken)).ToString();
                var hasChanges = string.Equals(originalContent, formattedContent, StringComparison.Ordinal) == false;

                if (hasChanges)
                {
                    changedFiles++;

                    if (_checkOnly)
                    {
                        _console.WriteLine($"Not formatted: {filePath}");
                    }
                    else if (_dryRun)
                    {
                        _console.WriteLine($"Would format: {filePath}");
                        _console.WriteLine(_diffGenerator.Generate(filePath, originalContent, formattedContent));
                    }
                    else
                    {
                        await _fileSystem.WriteAllTextAsync(filePath, formattedContent, cancellationToken).ConfigureAwait(false);

                        _console.WriteLine($"Formatted: {filePath}");
                    }
                }
                else
                {
                    if (_verbose)
                    {
                        _console.WriteLine($"Unchanged: {filePath}");
                    }
                }
            }
            catch (Exception ex)
            {
                errorFiles++;

                await _console.WriteErrorLineAsync($"Error processing {filePath}: {ex.Message}");
            }
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
    /// Determines whether the specified file path is within a build output directory (bin/ or obj/).
    /// </summary>
    /// <param name="filePath">The file path to check.</param>
    /// <returns><see langword="true"/> if the file is in a build output directory; otherwise, <see langword="false"/>.</returns>
    private static bool IsInBuildOutputDirectory(string filePath)
    {
        var normalizedPath = filePath.Replace('\\', '/');

        return normalizedPath.Contains("/bin/", StringComparison.OrdinalIgnoreCase)
               || normalizedPath.Contains("/obj/", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines whether the specified file is a generated file that should be skipped.
    /// </summary>
    /// <param name="filePath">The file path to check.</param>
    /// <returns><see langword="true"/> if the file is a generated file; otherwise, <see langword="false"/>.</returns>
    private static bool IsGeneratedFile(string filePath)
    {
        return filePath.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase)
               || filePath.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase)
               || filePath.EndsWith(".g.i.cs", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines whether the specified syntax tree contains syntax errors.
    /// </summary>
    /// <param name="syntaxTree">The syntax tree to check.</param>
    /// <returns><see langword="true"/> if the syntax tree has errors; otherwise, <see langword="false"/>.</returns>
    private static bool HasSyntaxErrors(SyntaxTree syntaxTree)
    {
        return syntaxTree.GetDiagnostics().Any(d => d.Severity == DiagnosticSeverity.Error);
    }

    /// <summary>
    /// Collects all C# source files from the configured paths.
    /// </summary>
    /// <returns>A list of file paths to process.</returns>
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
    /// Prints a summary of the formatting operation to the console.
    /// </summary>
    /// <param name="totalFiles">The total number of files processed.</param>
    /// <param name="changedFiles">The number of files that were changed or need changes.</param>
    /// <param name="skippedSyntaxErrors">The number of files skipped due to syntax errors.</param>
    /// <param name="skippedGenerated">The number of generated files that were skipped.</param>
    /// <param name="errorFiles">The number of files that encountered errors during processing.</param>
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