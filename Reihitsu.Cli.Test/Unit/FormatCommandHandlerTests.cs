using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Reihitsu.Cli;
using Reihitsu.Cli.Abstractions;
using Reihitsu.Cli.Test.Helpers;

namespace Reihitsu.Cli.Test.Unit;

/// <summary>
/// Tests for the <see cref="FormatCommandHandler"/> class.
/// </summary>
[TestClass]
public sealed class FormatCommandHandlerTests
{
    #region Fields

    private const string ValidCsContent = "namespace Test;\n\ninternal class Foo\n{\n}";
    private const string FormattedCsContent = "namespace Test;\n\ninternal class Foo\n{\n}";
    private const string UnformattedCsContent = "namespace  Test;\n\ninternal  class  Foo\n{\n}";
    private const string SyntaxErrorContent = "namespace Test { class { }";

    #endregion // Fields

    #region Helper Methods

    /// <summary>
    /// Creates a <see cref="FormatCommandHandler"/> with the specified configuration and pre-configured mocks.
    /// </summary>
    /// <param name="paths">The paths to process.</param>
    /// <param name="checkOnly">Whether to run in check-only mode.</param>
    /// <param name="dryRun">Whether to run in dry-run mode.</param>
    /// <param name="verbose">Whether to enable verbose output.</param>
    /// <param name="fileSystem">The file system mock.</param>
    /// <param name="console">The captured console output.</param>
    /// <param name="formatter">The source formatter mock.</param>
    /// <param name="diffGenerator">The diff generator mock.</param>
    /// <returns>A configured <see cref="FormatCommandHandler"/> instance.</returns>
    private static FormatCommandHandler CreateHandler(string[] paths, bool checkOnly, bool dryRun, bool verbose, IFileSystem fileSystem, CapturedConsoleOutput console, ISourceFormatter formatter, IDiffGenerator diffGenerator)
    {
        var dependencies = new FormatCommandDependencies(fileSystem, console, formatter, diffGenerator);

        return new FormatCommandHandler(paths, checkOnly, dryRun, verbose, dependencies);
    }

    /// <summary>
    /// Sets up the file system mock to return a single file that exists.
    /// </summary>
    /// <param name="fileSystem">The file system mock.</param>
    /// <param name="filePath">The file path.</param>
    /// <param name="content">The file content.</param>
    private static void SetupSingleFile(IFileSystem fileSystem, string filePath, string content)
    {
        fileSystem.FileExists(filePath).Returns(true);
        fileSystem.DirectoryExists(filePath).Returns(false);
        fileSystem.GetFullPath(filePath).Returns(filePath);
        fileSystem.ReadAllTextAsync(filePath, Arg.Any<CancellationToken>()).Returns(content);
    }

    /// <summary>
    /// Sets up the formatter mock to return a syntax tree with the specified content.
    /// </summary>
    /// <param name="formatter">The source formatter mock.</param>
    /// <param name="formattedContent">The formatted content to return.</param>
    private static void SetupFormatter(ISourceFormatter formatter, string formattedContent)
    {
        var formattedTree = CSharpSyntaxTree.ParseText(formattedContent);

        formatter.FormatSyntaxTree(Arg.Any<SyntaxTree>(), Arg.Any<CancellationToken>()).Returns(formattedTree);
    }

    /// <summary>
    /// Sets up the file system mock to return a directory with the specified files.
    /// </summary>
    /// <param name="fileSystem">The file system mock.</param>
    /// <param name="directoryPath">The directory path.</param>
    /// <param name="files">The files in the directory.</param>
    private static void SetupDirectory(IFileSystem fileSystem, string directoryPath, string[] files)
    {
        fileSystem.FileExists(directoryPath).Returns(false);
        fileSystem.DirectoryExists(directoryPath).Returns(true);
        fileSystem.EnumerateFiles(directoryPath, "*.cs", SearchOption.AllDirectories).Returns(files);
    }

    #endregion // Helper Methods

    #region File Collection

    /// <summary>
    /// Verifies that <see cref="FormatCommandHandler.ExecuteAsync"/> returns an error exit code when no .cs files are found.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [TestMethod]
    public async Task ExecuteAsyncNoFilesFoundReturnsError()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var formatter = Substitute.For<ISourceFormatter>();
        var diffGenerator = Substitute.For<IDiffGenerator>();
        var directoryPath = "/test/empty";

        SetupDirectory(fileSystem, directoryPath, []);

        var handler = CreateHandler([directoryPath], checkOnly: false, dryRun: false, verbose: false, fileSystem, console, formatter, diffGenerator);

        var exitCode = await handler.ExecuteAsync(CancellationToken.None);

        Assert.AreEqual(ExitCodes.Error, exitCode);
        Assert.IsTrue(console.StandardOutput.Any(line => line.Contains("No .cs files found.")));
    }

    /// <summary>
    /// Verifies that <see cref="FormatCommandHandler.ExecuteAsync"/> collects a single .cs file path.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [TestMethod]
    public async Task ExecuteAsyncSingleFilePathCollectsFile()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var formatter = Substitute.For<ISourceFormatter>();
        var diffGenerator = Substitute.For<IDiffGenerator>();
        var filePath = "/test/file.cs";

        SetupSingleFile(fileSystem, filePath, ValidCsContent);
        SetupFormatter(formatter, FormattedCsContent);

        var handler = CreateHandler([filePath], checkOnly: false, dryRun: false, verbose: false, fileSystem, console, formatter, diffGenerator);

        await handler.ExecuteAsync(CancellationToken.None);

        await fileSystem.Received(1).ReadAllTextAsync(filePath, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies that <see cref="FormatCommandHandler.ExecuteAsync"/> collects files recursively from a directory.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [TestMethod]
    public async Task ExecuteAsyncDirectoryCollectsRecursively()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var formatter = Substitute.For<ISourceFormatter>();
        var diffGenerator = Substitute.For<IDiffGenerator>();
        var directoryPath = "/test/dir";
        var files = new[] { "/test/dir/file1.cs", "/test/dir/sub/file2.cs" };

        SetupDirectory(fileSystem, directoryPath, files);

        foreach (var file in files)
        {
            fileSystem.ReadAllTextAsync(file, Arg.Any<CancellationToken>()).Returns(ValidCsContent);
        }

        SetupFormatter(formatter, FormattedCsContent);

        var handler = CreateHandler([directoryPath], checkOnly: false, dryRun: false, verbose: false, fileSystem, console, formatter, diffGenerator);

        await handler.ExecuteAsync(CancellationToken.None);

        foreach (var file in files)
        {
            await fileSystem.Received(1).ReadAllTextAsync(file, Arg.Any<CancellationToken>());
        }
    }

    /// <summary>
    /// Verifies that <see cref="FormatCommandHandler.ExecuteAsync"/> skips files in bin/ directories.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [TestMethod]
    public async Task ExecuteAsyncSkipsBinDirectory()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var formatter = Substitute.For<ISourceFormatter>();
        var diffGenerator = Substitute.For<IDiffGenerator>();
        var directoryPath = "/test/dir";
        var binFile = "/test/dir/bin/Debug/file.cs";
        var normalFile = "/test/dir/src/file.cs";

        SetupDirectory(fileSystem, directoryPath, [binFile, normalFile]);

        fileSystem.ReadAllTextAsync(normalFile, Arg.Any<CancellationToken>()).Returns(ValidCsContent);

        SetupFormatter(formatter, FormattedCsContent);

        var handler = CreateHandler([directoryPath], checkOnly: false, dryRun: false, verbose: false, fileSystem, console, formatter, diffGenerator);

        await handler.ExecuteAsync(CancellationToken.None);

        await fileSystem.DidNotReceive().ReadAllTextAsync(binFile, Arg.Any<CancellationToken>());
        await fileSystem.Received(1).ReadAllTextAsync(normalFile, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies that <see cref="FormatCommandHandler.ExecuteAsync"/> skips files in obj/ directories.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [TestMethod]
    public async Task ExecuteAsyncSkipsObjDirectory()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var formatter = Substitute.For<ISourceFormatter>();
        var diffGenerator = Substitute.For<IDiffGenerator>();
        var directoryPath = "/test/dir";
        var objFile = "/test/dir/obj/Debug/file.cs";
        var normalFile = "/test/dir/src/file.cs";

        SetupDirectory(fileSystem, directoryPath, [objFile, normalFile]);

        fileSystem.ReadAllTextAsync(normalFile, Arg.Any<CancellationToken>()).Returns(ValidCsContent);

        SetupFormatter(formatter, FormattedCsContent);

        var handler = CreateHandler([directoryPath], checkOnly: false, dryRun: false, verbose: false, fileSystem, console, formatter, diffGenerator);

        await handler.ExecuteAsync(CancellationToken.None);

        await fileSystem.DidNotReceive().ReadAllTextAsync(objFile, Arg.Any<CancellationToken>());
        await fileSystem.Received(1).ReadAllTextAsync(normalFile, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies that <see cref="FormatCommandHandler.ExecuteAsync"/> ignores non-.cs files.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [TestMethod]
    public async Task ExecuteAsyncNonCsFileIgnored()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var formatter = Substitute.For<ISourceFormatter>();
        var diffGenerator = Substitute.For<IDiffGenerator>();
        var txtFile = "/test/file.txt";

        fileSystem.FileExists(txtFile).Returns(true);
        fileSystem.DirectoryExists(txtFile).Returns(false);

        var handler = CreateHandler([txtFile], checkOnly: false, dryRun: false, verbose: false, fileSystem, console, formatter, diffGenerator);

        var exitCode = await handler.ExecuteAsync(CancellationToken.None);

        Assert.AreEqual(ExitCodes.Error, exitCode);
        fileSystem.DidNotReceive().GetFullPath(Arg.Any<string>());
    }

    #endregion // File Collection

    #region Generated File Detection

    /// <summary>
    /// Verifies that <see cref="FormatCommandHandler.ExecuteAsync"/> skips .Designer.cs files.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [TestMethod]
    public async Task ExecuteAsyncSkipsDesignerCsFiles()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var formatter = Substitute.For<ISourceFormatter>();
        var diffGenerator = Substitute.For<IDiffGenerator>();
        var directoryPath = "/test/dir";
        var designerFile = "/test/dir/Resources.Designer.cs";
        var normalFile = "/test/dir/file.cs";

        SetupDirectory(fileSystem, directoryPath, [designerFile, normalFile]);

        fileSystem.ReadAllTextAsync(normalFile, Arg.Any<CancellationToken>()).Returns(ValidCsContent);

        SetupFormatter(formatter, FormattedCsContent);

        var handler = CreateHandler([directoryPath], checkOnly: false, dryRun: false, verbose: false, fileSystem, console, formatter, diffGenerator);

        await handler.ExecuteAsync(CancellationToken.None);

        await fileSystem.DidNotReceive().ReadAllTextAsync(designerFile, Arg.Any<CancellationToken>());
        Assert.IsTrue(console.StandardOutput.Any(line => line.Contains("Skipped 1 generated file(s).")));
    }

    /// <summary>
    /// Verifies that <see cref="FormatCommandHandler.ExecuteAsync"/> skips .g.cs files.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [TestMethod]
    public async Task ExecuteAsyncSkipsGCsFiles()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var formatter = Substitute.For<ISourceFormatter>();
        var diffGenerator = Substitute.For<IDiffGenerator>();
        var directoryPath = "/test/dir";
        var generatedFile = "/test/dir/file.g.cs";
        var normalFile = "/test/dir/file.cs";

        SetupDirectory(fileSystem, directoryPath, [generatedFile, normalFile]);

        fileSystem.ReadAllTextAsync(normalFile, Arg.Any<CancellationToken>()).Returns(ValidCsContent);

        SetupFormatter(formatter, FormattedCsContent);

        var handler = CreateHandler([directoryPath], checkOnly: false, dryRun: false, verbose: false, fileSystem, console, formatter, diffGenerator);

        await handler.ExecuteAsync(CancellationToken.None);

        await fileSystem.DidNotReceive().ReadAllTextAsync(generatedFile, Arg.Any<CancellationToken>());
        Assert.IsTrue(console.StandardOutput.Any(line => line.Contains("Skipped 1 generated file(s).")));
    }

    /// <summary>
    /// Verifies that <see cref="FormatCommandHandler.ExecuteAsync"/> skips .g.i.cs files.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [TestMethod]
    public async Task ExecuteAsyncSkipsGICsFiles()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var formatter = Substitute.For<ISourceFormatter>();
        var diffGenerator = Substitute.For<IDiffGenerator>();

        var directoryPath = "/test/dir";
        var generatedFile = "/test/dir/file.g.i.cs";
        var normalFile = "/test/dir/file.cs";

        SetupDirectory(fileSystem, directoryPath, [generatedFile, normalFile]);

        fileSystem.ReadAllTextAsync(normalFile, Arg.Any<CancellationToken>()).Returns(ValidCsContent);

        SetupFormatter(formatter, FormattedCsContent);

        var handler = CreateHandler([directoryPath], checkOnly: false, dryRun: false, verbose: false, fileSystem, console, formatter, diffGenerator);

        await handler.ExecuteAsync(CancellationToken.None);

        await fileSystem.DidNotReceive().ReadAllTextAsync(generatedFile, Arg.Any<CancellationToken>());
        Assert.IsTrue(console.StandardOutput.Any(line => line.Contains("Skipped 1 generated file(s).")));
    }

    /// <summary>
    /// Verifies that <see cref="FormatCommandHandler.ExecuteAsync"/> logs skipped generated files in verbose mode.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [TestMethod]
    public async Task ExecuteAsyncVerboseLogsSkippedGeneratedFiles()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var formatter = Substitute.For<ISourceFormatter>();
        var diffGenerator = Substitute.For<IDiffGenerator>();

        var directoryPath = "/test/dir";
        var designerFile = "/test/dir/Resources.Designer.cs";
        var normalFile = "/test/dir/file.cs";

        SetupDirectory(fileSystem, directoryPath, [designerFile, normalFile]);

        fileSystem.ReadAllTextAsync(normalFile, Arg.Any<CancellationToken>()).Returns(ValidCsContent);

        SetupFormatter(formatter, FormattedCsContent);

        var handler = CreateHandler([directoryPath], checkOnly: false, dryRun: false, verbose: true, fileSystem, console, formatter, diffGenerator);

        await handler.ExecuteAsync(CancellationToken.None);

        Assert.IsTrue(console.StandardOutput.Any(line => line.Contains("Skipped (generated)")));
    }

    #endregion // Generated File Detection

    #region Syntax Error Handling

    /// <summary>
    /// Verifies that <see cref="FormatCommandHandler.ExecuteAsync"/> skips files with syntax errors.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [TestMethod]
    public async Task ExecuteAsyncSkipsFilesWithSyntaxErrors()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var formatter = Substitute.For<ISourceFormatter>();
        var diffGenerator = Substitute.For<IDiffGenerator>();

        var filePath = "/test/broken.cs";

        SetupSingleFile(fileSystem, filePath, SyntaxErrorContent);

        var handler = CreateHandler([filePath], checkOnly: false, dryRun: false, verbose: false, fileSystem, console, formatter, diffGenerator);

        await handler.ExecuteAsync(CancellationToken.None);

        formatter.DidNotReceive().FormatSyntaxTree(Arg.Any<SyntaxTree>(), Arg.Any<CancellationToken>());
        Assert.IsTrue(console.StandardOutput.Any(line => line.Contains("Skipped 1 file(s) with syntax errors.")));
    }

    /// <summary>
    /// Verifies that <see cref="FormatCommandHandler.ExecuteAsync"/> logs skipped syntax error files in verbose mode.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [TestMethod]
    public async Task ExecuteAsyncVerboseLogsSkippedSyntaxErrorFiles()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var formatter = Substitute.For<ISourceFormatter>();
        var diffGenerator = Substitute.For<IDiffGenerator>();

        var filePath = "/test/broken.cs";

        SetupSingleFile(fileSystem, filePath, SyntaxErrorContent);

        var handler = CreateHandler([filePath], checkOnly: false, dryRun: false, verbose: true, fileSystem, console, formatter, diffGenerator);

        await handler.ExecuteAsync(CancellationToken.None);

        Assert.IsTrue(console.StandardOutput.Any(line => line.Contains("Skipped (syntax errors)")));
    }

    #endregion // Syntax Error Handling

    #region Check Mode

    /// <summary>
    /// Verifies that <see cref="FormatCommandHandler.ExecuteAsync"/> returns success when file is already formatted in check mode.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [TestMethod]
    public async Task ExecuteAsyncCheckModeFormattedFileReturnsSuccess()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var formatter = Substitute.For<ISourceFormatter>();
        var diffGenerator = Substitute.For<IDiffGenerator>();

        var filePath = "/test/file.cs";

        SetupSingleFile(fileSystem, filePath, ValidCsContent);
        SetupFormatter(formatter, ValidCsContent);

        var handler = CreateHandler([filePath], checkOnly: true, dryRun: false, verbose: false, fileSystem, console, formatter, diffGenerator);

        var exitCode = await handler.ExecuteAsync(CancellationToken.None);

        Assert.AreEqual(ExitCodes.Success, exitCode);
    }

    /// <summary>
    /// Verifies that <see cref="FormatCommandHandler.ExecuteAsync"/> returns formatting needed when file needs formatting in check mode.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [TestMethod]
    public async Task ExecuteAsyncCheckModeUnformattedFileReturnsFormattingNeeded()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var formatter = Substitute.For<ISourceFormatter>();
        var diffGenerator = Substitute.For<IDiffGenerator>();

        var filePath = "/test/file.cs";

        SetupSingleFile(fileSystem, filePath, UnformattedCsContent);
        SetupFormatter(formatter, FormattedCsContent);

        var handler = CreateHandler([filePath], checkOnly: true, dryRun: false, verbose: false, fileSystem, console, formatter, diffGenerator);

        var exitCode = await handler.ExecuteAsync(CancellationToken.None);

        Assert.AreEqual(ExitCodes.FormattingNeeded, exitCode);
    }

    /// <summary>
    /// Verifies that <see cref="FormatCommandHandler.ExecuteAsync"/> does not write files in check mode.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [TestMethod]
    public async Task ExecuteAsyncCheckModeDoesNotWriteFiles()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var formatter = Substitute.For<ISourceFormatter>();
        var diffGenerator = Substitute.For<IDiffGenerator>();

        var filePath = "/test/file.cs";

        SetupSingleFile(fileSystem, filePath, UnformattedCsContent);
        SetupFormatter(formatter, FormattedCsContent);

        var handler = CreateHandler([filePath], checkOnly: true, dryRun: false, verbose: false, fileSystem, console, formatter, diffGenerator);

        await handler.ExecuteAsync(CancellationToken.None);

        await fileSystem.DidNotReceive().WriteAllTextAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies that <see cref="FormatCommandHandler.ExecuteAsync"/> outputs "Not formatted" message in check mode.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [TestMethod]
    public async Task ExecuteAsyncCheckModeOutputsNotFormatted()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var formatter = Substitute.For<ISourceFormatter>();
        var diffGenerator = Substitute.For<IDiffGenerator>();

        var filePath = "/test/file.cs";

        SetupSingleFile(fileSystem, filePath, UnformattedCsContent);
        SetupFormatter(formatter, FormattedCsContent);

        var handler = CreateHandler([filePath], checkOnly: true, dryRun: false, verbose: false, fileSystem, console, formatter, diffGenerator);

        await handler.ExecuteAsync(CancellationToken.None);

        Assert.IsTrue(console.StandardOutput.Any(line => line == $"Not formatted: {filePath}"));
    }

    #endregion // Check Mode

    #region Dry-Run Mode

    /// <summary>
    /// Verifies that <see cref="FormatCommandHandler.ExecuteAsync"/> shows diff output in dry-run mode.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [TestMethod]
    public async Task ExecuteAsyncDryRunShowsDiff()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var formatter = Substitute.For<ISourceFormatter>();
        var diffGenerator = Substitute.For<IDiffGenerator>();

        var filePath = "/test/file.cs";
        var diffOutput = "--- a/file.cs\n+++ b/file.cs\n@@ -1 +1 @@\n-original\n+formatted";

        SetupSingleFile(fileSystem, filePath, UnformattedCsContent);
        SetupFormatter(formatter, FormattedCsContent);

        diffGenerator.Generate(filePath, Arg.Any<string>(), Arg.Any<string>()).Returns(diffOutput);

        var handler = CreateHandler([filePath], checkOnly: false, dryRun: true, verbose: false, fileSystem, console, formatter, diffGenerator);

        await handler.ExecuteAsync(CancellationToken.None);

        Assert.IsTrue(console.StandardOutput.Any(line => line == diffOutput));
    }

    /// <summary>
    /// Verifies that <see cref="FormatCommandHandler.ExecuteAsync"/> does not write files in dry-run mode.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [TestMethod]
    public async Task ExecuteAsyncDryRunDoesNotWriteFiles()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var formatter = Substitute.For<ISourceFormatter>();
        var diffGenerator = Substitute.For<IDiffGenerator>();

        var filePath = "/test/file.cs";

        SetupSingleFile(fileSystem, filePath, UnformattedCsContent);
        SetupFormatter(formatter, FormattedCsContent);

        diffGenerator.Generate(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns("diff");

        var handler = CreateHandler([filePath], checkOnly: false, dryRun: true, verbose: false, fileSystem, console, formatter, diffGenerator);

        await handler.ExecuteAsync(CancellationToken.None);

        await fileSystem.DidNotReceive().WriteAllTextAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies that <see cref="FormatCommandHandler.ExecuteAsync"/> returns formatting needed when changes exist in dry-run mode.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [TestMethod]
    public async Task ExecuteAsyncDryRunReturnsFormattingNeeded()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var formatter = Substitute.For<ISourceFormatter>();
        var diffGenerator = Substitute.For<IDiffGenerator>();

        var filePath = "/test/file.cs";

        SetupSingleFile(fileSystem, filePath, UnformattedCsContent);
        SetupFormatter(formatter, FormattedCsContent);

        diffGenerator.Generate(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns("diff");

        var handler = CreateHandler([filePath], checkOnly: false, dryRun: true, verbose: false, fileSystem, console, formatter, diffGenerator);

        var exitCode = await handler.ExecuteAsync(CancellationToken.None);

        Assert.AreEqual(ExitCodes.FormattingNeeded, exitCode);
    }

    /// <summary>
    /// Verifies that <see cref="FormatCommandHandler.ExecuteAsync"/> calls the diff generator with correct arguments in dry-run mode.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [TestMethod]
    public async Task ExecuteAsyncDryRunCallsDiffGenerator()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var formatter = Substitute.For<ISourceFormatter>();
        var diffGenerator = Substitute.For<IDiffGenerator>();

        var filePath = "/test/file.cs";

        SetupSingleFile(fileSystem, filePath, UnformattedCsContent);
        SetupFormatter(formatter, FormattedCsContent);

        diffGenerator.Generate(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns("diff");

        var handler = CreateHandler([filePath], checkOnly: false, dryRun: true, verbose: false, fileSystem, console, formatter, diffGenerator);

        await handler.ExecuteAsync(CancellationToken.None);

        diffGenerator.Received(1).Generate(filePath, UnformattedCsContent, Arg.Any<string>());
    }

    #endregion // Dry-Run Mode

    #region Normal Format Mode

    /// <summary>
    /// Verifies that <see cref="FormatCommandHandler.ExecuteAsync"/> formats and writes changed files.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [TestMethod]
    public async Task ExecuteAsyncFormatsAndWritesChangedFile()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var formatter = Substitute.For<ISourceFormatter>();
        var diffGenerator = Substitute.For<IDiffGenerator>();

        var filePath = "/test/file.cs";

        SetupSingleFile(fileSystem, filePath, UnformattedCsContent);
        SetupFormatter(formatter, FormattedCsContent);

        var handler = CreateHandler([filePath], checkOnly: false, dryRun: false, verbose: false, fileSystem, console, formatter, diffGenerator);

        await handler.ExecuteAsync(CancellationToken.None);

        await fileSystem.Received(1).WriteAllTextAsync(filePath, Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies that <see cref="FormatCommandHandler.ExecuteAsync"/> does not write unchanged files.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [TestMethod]
    public async Task ExecuteAsyncUnchangedFileNotWritten()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var formatter = Substitute.For<ISourceFormatter>();
        var diffGenerator = Substitute.For<IDiffGenerator>();

        var filePath = "/test/file.cs";

        SetupSingleFile(fileSystem, filePath, ValidCsContent);
        SetupFormatter(formatter, ValidCsContent);

        var handler = CreateHandler([filePath], checkOnly: false, dryRun: false, verbose: false, fileSystem, console, formatter, diffGenerator);

        await handler.ExecuteAsync(CancellationToken.None);

        await fileSystem.DidNotReceive().WriteAllTextAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies that <see cref="FormatCommandHandler.ExecuteAsync"/> outputs "Formatted" message for changed files.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [TestMethod]
    public async Task ExecuteAsyncFormatsOutputsFormattedMessage()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var formatter = Substitute.For<ISourceFormatter>();
        var diffGenerator = Substitute.For<IDiffGenerator>();

        var filePath = "/test/file.cs";

        SetupSingleFile(fileSystem, filePath, UnformattedCsContent);
        SetupFormatter(formatter, FormattedCsContent);

        var handler = CreateHandler([filePath], checkOnly: false, dryRun: false, verbose: false, fileSystem, console, formatter, diffGenerator);

        await handler.ExecuteAsync(CancellationToken.None);

        Assert.IsTrue(console.StandardOutput.Any(line => line == $"Formatted: {filePath}"));
    }

    /// <summary>
    /// Verifies that <see cref="FormatCommandHandler.ExecuteAsync"/> outputs "Unchanged" message in verbose mode for unchanged files.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [TestMethod]
    public async Task ExecuteAsyncVerboseOutputsUnchangedMessage()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var formatter = Substitute.For<ISourceFormatter>();
        var diffGenerator = Substitute.For<IDiffGenerator>();

        var filePath = "/test/file.cs";

        SetupSingleFile(fileSystem, filePath, ValidCsContent);
        SetupFormatter(formatter, ValidCsContent);

        var handler = CreateHandler([filePath], checkOnly: false, dryRun: false, verbose: true, fileSystem, console, formatter, diffGenerator);

        await handler.ExecuteAsync(CancellationToken.None);

        Assert.IsTrue(console.StandardOutput.Any(line => line == $"Unchanged: {filePath}"));
    }

    /// <summary>
    /// Verifies that <see cref="FormatCommandHandler.ExecuteAsync"/> returns success when all files are already formatted.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [TestMethod]
    public async Task ExecuteAsyncReturnsSuccessWhenAllFilesFormatted()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var formatter = Substitute.For<ISourceFormatter>();
        var diffGenerator = Substitute.For<IDiffGenerator>();

        var filePath = "/test/file.cs";

        SetupSingleFile(fileSystem, filePath, ValidCsContent);
        SetupFormatter(formatter, ValidCsContent);

        var handler = CreateHandler([filePath], checkOnly: false, dryRun: false, verbose: false, fileSystem, console, formatter, diffGenerator);

        var exitCode = await handler.ExecuteAsync(CancellationToken.None);

        Assert.AreEqual(ExitCodes.Success, exitCode);
    }

    #endregion // Normal Format Mode

    #region Error Handling

    /// <summary>
    /// Verifies that <see cref="FormatCommandHandler.ExecuteAsync"/> continues processing when a file read error occurs.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [TestMethod]
    public async Task ExecuteAsyncFileReadErrorContinuesProcessing()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var formatter = Substitute.For<ISourceFormatter>();
        var diffGenerator = Substitute.For<IDiffGenerator>();

        var directoryPath = "/test/dir";
        var errorFile = "/test/dir/error.cs";
        var goodFile = "/test/dir/good.cs";

        SetupDirectory(fileSystem, directoryPath, [errorFile, goodFile]);

        fileSystem.ReadAllTextAsync(errorFile, Arg.Any<CancellationToken>()).Throws(new IOException("Access denied"));
        fileSystem.ReadAllTextAsync(goodFile, Arg.Any<CancellationToken>()).Returns(ValidCsContent);

        SetupFormatter(formatter, FormattedCsContent);

        var handler = CreateHandler([directoryPath], checkOnly: false, dryRun: false, verbose: false, fileSystem, console, formatter, diffGenerator);

        await handler.ExecuteAsync(CancellationToken.None);

        await fileSystem.Received(1).ReadAllTextAsync(goodFile, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Verifies that <see cref="FormatCommandHandler.ExecuteAsync"/> increments the error count when a file read error occurs.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [TestMethod]
    public async Task ExecuteAsyncFileReadErrorIncrementsErrorCount()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var formatter = Substitute.For<ISourceFormatter>();
        var diffGenerator = Substitute.For<IDiffGenerator>();

        var filePath = "/test/error.cs";

        SetupSingleFile(fileSystem, filePath, ValidCsContent);

        fileSystem.ReadAllTextAsync(filePath, Arg.Any<CancellationToken>()).Throws(new IOException("Access denied"));

        var handler = CreateHandler([filePath], checkOnly: false, dryRun: false, verbose: false, fileSystem, console, formatter, diffGenerator);

        await handler.ExecuteAsync(CancellationToken.None);

        Assert.IsTrue(console.StandardOutput.Any(line => line.Contains("1 file(s) encountered errors.")));
    }

    /// <summary>
    /// Verifies that <see cref="FormatCommandHandler.ExecuteAsync"/> returns an error exit code when files have errors.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [TestMethod]
    public async Task ExecuteAsyncErrorFilesReturnsErrorExitCode()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var formatter = Substitute.For<ISourceFormatter>();
        var diffGenerator = Substitute.For<IDiffGenerator>();

        var filePath = "/test/error.cs";

        SetupSingleFile(fileSystem, filePath, ValidCsContent);

        fileSystem.ReadAllTextAsync(filePath, Arg.Any<CancellationToken>()).Throws(new IOException("Access denied"));

        var handler = CreateHandler([filePath], checkOnly: false, dryRun: false, verbose: false, fileSystem, console, formatter, diffGenerator);

        var exitCode = await handler.ExecuteAsync(CancellationToken.None);

        Assert.AreEqual(ExitCodes.Error, exitCode);
    }

    /// <summary>
    /// Verifies that <see cref="FormatCommandHandler.ExecuteAsync"/> outputs error messages to stderr.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [TestMethod]
    public async Task ExecuteAsyncErrorOutputsErrorMessage()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var formatter = Substitute.For<ISourceFormatter>();
        var diffGenerator = Substitute.For<IDiffGenerator>();

        var filePath = "/test/error.cs";

        SetupSingleFile(fileSystem, filePath, ValidCsContent);

        fileSystem.ReadAllTextAsync(filePath, Arg.Any<CancellationToken>()).Throws(new IOException("Access denied"));

        var handler = CreateHandler([filePath], checkOnly: false, dryRun: false, verbose: false, fileSystem, console, formatter, diffGenerator);

        await handler.ExecuteAsync(CancellationToken.None);

        Assert.IsTrue(console.ErrorOutput.Any(line => line.Contains($"Error processing {filePath}") && line.Contains("Access denied")));
    }

    #endregion // Error Handling

    #region Summary Output

    /// <summary>
    /// Verifies that <see cref="FormatCommandHandler.ExecuteAsync"/> prints summary with files needing formatting in check mode.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [TestMethod]
    public async Task ExecuteAsyncCheckModePrintsSummaryWithNeedFormatting()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var formatter = Substitute.For<ISourceFormatter>();
        var diffGenerator = Substitute.For<IDiffGenerator>();

        var filePath = "/test/file.cs";

        SetupSingleFile(fileSystem, filePath, UnformattedCsContent);
        SetupFormatter(formatter, FormattedCsContent);

        var handler = CreateHandler([filePath], checkOnly: true, dryRun: false, verbose: false, fileSystem, console, formatter, diffGenerator);

        await handler.ExecuteAsync(CancellationToken.None);

        Assert.IsTrue(console.StandardOutput.Any(line => line == "1 of 1 file(s) need formatting."));
    }

    /// <summary>
    /// Verifies that <see cref="FormatCommandHandler.ExecuteAsync"/> prints summary with "would be formatted" in dry-run mode.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [TestMethod]
    public async Task ExecuteAsyncDryRunPrintsSummaryWithWouldFormat()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var formatter = Substitute.For<ISourceFormatter>();
        var diffGenerator = Substitute.For<IDiffGenerator>();

        var filePath = "/test/file.cs";

        SetupSingleFile(fileSystem, filePath, UnformattedCsContent);
        SetupFormatter(formatter, FormattedCsContent);

        diffGenerator.Generate(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns("diff");

        var handler = CreateHandler([filePath], checkOnly: false, dryRun: true, verbose: false, fileSystem, console, formatter, diffGenerator);

        await handler.ExecuteAsync(CancellationToken.None);

        Assert.IsTrue(console.StandardOutput.Any(line => line == "1 of 1 file(s) would be formatted."));
    }

    /// <summary>
    /// Verifies that <see cref="FormatCommandHandler.ExecuteAsync"/> prints summary with "Formatted" in normal mode.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [TestMethod]
    public async Task ExecuteAsyncFormatModePrintsSummaryWithFormatted()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var formatter = Substitute.For<ISourceFormatter>();
        var diffGenerator = Substitute.For<IDiffGenerator>();

        var filePath = "/test/file.cs";

        SetupSingleFile(fileSystem, filePath, UnformattedCsContent);
        SetupFormatter(formatter, FormattedCsContent);

        var handler = CreateHandler([filePath], checkOnly: false, dryRun: false, verbose: false, fileSystem, console, formatter, diffGenerator);

        await handler.ExecuteAsync(CancellationToken.None);

        Assert.IsTrue(console.StandardOutput.Any(line => line == "Formatted 1 of 1 file(s)."));
    }

    /// <summary>
    /// Verifies that <see cref="FormatCommandHandler.ExecuteAsync"/> includes skipped generated files in the summary.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [TestMethod]
    public async Task ExecuteAsyncSkippedGeneratedFilesInSummary()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var formatter = Substitute.For<ISourceFormatter>();
        var diffGenerator = Substitute.For<IDiffGenerator>();

        var directoryPath = "/test/dir";
        var designerFile = "/test/dir/Resources.Designer.cs";
        var normalFile = "/test/dir/file.cs";

        SetupDirectory(fileSystem, directoryPath, [designerFile, normalFile]);

        fileSystem.ReadAllTextAsync(normalFile, Arg.Any<CancellationToken>()).Returns(ValidCsContent);

        SetupFormatter(formatter, FormattedCsContent);

        var handler = CreateHandler([directoryPath], checkOnly: false, dryRun: false, verbose: false, fileSystem, console, formatter, diffGenerator);

        await handler.ExecuteAsync(CancellationToken.None);

        Assert.IsTrue(console.StandardOutput.Any(line => line == "Skipped 1 generated file(s)."));
    }

    /// <summary>
    /// Verifies that <see cref="FormatCommandHandler.ExecuteAsync"/> includes skipped syntax error files in the summary.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [TestMethod]
    public async Task ExecuteAsyncSkippedSyntaxErrorsInSummary()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var formatter = Substitute.For<ISourceFormatter>();
        var diffGenerator = Substitute.For<IDiffGenerator>();

        var filePath = "/test/broken.cs";

        SetupSingleFile(fileSystem, filePath, SyntaxErrorContent);

        var handler = CreateHandler([filePath], checkOnly: false, dryRun: false, verbose: false, fileSystem, console, formatter, diffGenerator);

        await handler.ExecuteAsync(CancellationToken.None);

        Assert.IsTrue(console.StandardOutput.Any(line => line == "Skipped 1 file(s) with syntax errors."));
    }

    /// <summary>
    /// Verifies that <see cref="FormatCommandHandler.ExecuteAsync"/> includes error files in the summary.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [TestMethod]
    public async Task ExecuteAsyncErrorFilesInSummary()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var formatter = Substitute.For<ISourceFormatter>();
        var diffGenerator = Substitute.For<IDiffGenerator>();

        var filePath = "/test/error.cs";

        SetupSingleFile(fileSystem, filePath, ValidCsContent);

        fileSystem.ReadAllTextAsync(filePath, Arg.Any<CancellationToken>()).Throws(new IOException("Access denied"));

        var handler = CreateHandler([filePath], checkOnly: false, dryRun: false, verbose: false, fileSystem, console, formatter, diffGenerator);

        await handler.ExecuteAsync(CancellationToken.None);

        Assert.IsTrue(console.StandardOutput.Any(line => line == "1 file(s) encountered errors."));
    }

    #endregion // Summary Output

    #region CancellationToken Propagation

    /// <summary>
    /// Verifies that <see cref="FormatCommandHandler.ExecuteAsync"/> passes the cancellation token to file read operations.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [TestMethod]
    public async Task ExecuteAsyncPassesCancellationTokenToFileRead()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var formatter = Substitute.For<ISourceFormatter>();
        var diffGenerator = Substitute.For<IDiffGenerator>();

        using var cts = new CancellationTokenSource();

        var filePath = "/test/file.cs";

        SetupSingleFile(fileSystem, filePath, ValidCsContent);
        SetupFormatter(formatter, FormattedCsContent);

        var handler = CreateHandler([filePath], checkOnly: false, dryRun: false, verbose: false, fileSystem, console, formatter, diffGenerator);

        await handler.ExecuteAsync(cts.Token);

        await fileSystem.Received(1).ReadAllTextAsync(filePath, cts.Token);
    }

    /// <summary>
    /// Verifies that <see cref="FormatCommandHandler.ExecuteAsync"/> passes the cancellation token to the formatter.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [TestMethod]
    public async Task ExecuteAsyncPassesCancellationTokenToFormatter()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var formatter = Substitute.For<ISourceFormatter>();
        var diffGenerator = Substitute.For<IDiffGenerator>();

        using var cts = new CancellationTokenSource();

        var filePath = "/test/file.cs";

        SetupSingleFile(fileSystem, filePath, ValidCsContent);
        SetupFormatter(formatter, FormattedCsContent);

        var handler = CreateHandler([filePath], checkOnly: false, dryRun: false, verbose: false, fileSystem, console, formatter, diffGenerator);

        await handler.ExecuteAsync(cts.Token);

        formatter.Received(1).FormatSyntaxTree(Arg.Any<SyntaxTree>(), cts.Token);
    }

    /// <summary>
    /// Verifies that <see cref="FormatCommandHandler.ExecuteAsync"/> passes the cancellation token to file write operations.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [TestMethod]
    public async Task ExecuteAsyncPassesCancellationTokenToFileWrite()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var formatter = Substitute.For<ISourceFormatter>();
        var diffGenerator = Substitute.For<IDiffGenerator>();

        using var cts = new CancellationTokenSource();

        var filePath = "/test/file.cs";

        SetupSingleFile(fileSystem, filePath, UnformattedCsContent);
        SetupFormatter(formatter, FormattedCsContent);

        var handler = CreateHandler([filePath], checkOnly: false, dryRun: false, verbose: false, fileSystem, console, formatter, diffGenerator);

        await handler.ExecuteAsync(cts.Token);

        await fileSystem.Received(1).WriteAllTextAsync(filePath, Arg.Any<string>(), cts.Token);
    }

    #endregion // CancellationToken Propagation
}