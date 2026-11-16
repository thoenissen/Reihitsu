using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using NSubstitute;

using Reihitsu.Cli.Abstractions;
using Reihitsu.Cli.Test.Helpers;
using Reihitsu.Formatter;
using Reihitsu.Formatter.Utilities;

namespace Reihitsu.Cli.Test.Unit;

/// <summary>
/// Tests for the parallel processing of <see cref="FormatCommandHandler"/>: the output is the output of one-after-another
/// processing in selection order whichever file finishes first (CLI-022), the degree of parallelism bounds how many files are
/// processed at the same time (CLI-023), and a canceled run completes every started write and names every finished file
/// </summary>
[TestClass]
public sealed class FormatCommandHandlerParallelTests
{
    #region Constants

    /// <summary>
    /// Formatted C# source; the formatter fake returns it for every file
    /// </summary>
    private const string FormattedCsContent = "namespace Test;\n\ninternal class Foo\n{\n}";

    /// <summary>
    /// Source the formatter fake changes
    /// </summary>
    private const string UnformattedCsContent = "namespace  Test;\n\ninternal  class  Foo\n{\n}";

    /// <summary>
    /// Invalid C# source, skipped with syntax errors
    /// </summary>
    private const string SyntaxErrorContent = "namespace Test { class { }";

    /// <summary>
    /// The reason the formatter fake gives for a file it refuses as unsupported
    /// </summary>
    private const string UnsupportedReason = "a shape the formatter refuses";

    /// <summary>
    /// The prefix of a standard output line in <see cref="CapturedConsoleOutput.InterleavedOutput"/>
    /// </summary>
    private const string Out = CapturedConsoleOutput.StandardOutputPrefix;

    /// <summary>
    /// The prefix of a standard error line in <see cref="CapturedConsoleOutput.InterleavedOutput"/>
    /// </summary>
    private const string Err = CapturedConsoleOutput.ErrorOutputPrefix;

    #endregion // Constants

    #region Fields

    /// <summary>
    /// How long a held fake waits for the event it is held for before it gives up, so a test of an implementation that never
    /// produces the event fails instead of hanging
    /// </summary>
    private static readonly TimeSpan _holdTimeout = TimeSpan.FromSeconds(10);

    /// <summary>
    /// UTF-8 encoding without a byte order mark, the encoding the file system fake reports for every file
    /// </summary>
    private static readonly Encoding _utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    #endregion // Fields

    #region Helper Methods

    /// <summary>
    /// Creates a <see cref="FormatCommandHandler"/> over the given fakes
    /// </summary>
    /// <param name="paths">The paths to process</param>
    /// <param name="mode">The mode: <c>check</c>, <c>dry-run</c>, or <c>write</c></param>
    /// <param name="verbose">Whether to enable verbose output</param>
    /// <param name="maxParallelism">The maximum number of files processed at the same time</param>
    /// <param name="fileSystem">The file system fake</param>
    /// <param name="console">The captured console output</param>
    /// <param name="formatter">The source formatter fake</param>
    /// <param name="diffGenerator">The diff generator fake, or <see langword="null"/> for a default one</param>
    /// <param name="consoleInput">The console input fake, or <see langword="null"/> for a default one</param>
    /// <param name="force">Whether to skip the confirmation prompt of a large write run</param>
    /// <returns>The handler</returns>
    private static FormatCommandHandler CreateHandler(string[] paths, string mode, bool verbose, int maxParallelism, IFileSystem fileSystem, CapturedConsoleOutput console, ISourceFormatter formatter, IDiffGenerator diffGenerator = null, IConsoleInput consoleInput = null, bool force = true)
    {
        diffGenerator ??= Substitute.For<IDiffGenerator>();
        consoleInput ??= Substitute.For<IConsoleInput>();

        var dependencies = new FormatCommandDependencies(fileSystem, console, consoleInput, formatter, diffGenerator);

        return new FormatCommandHandler(paths,
                                        checkOnly: mode == "check",
                                        dryRun: mode == "dry-run",
                                        verbose,
                                        force,
                                        utf8Bom: false,
                                        LanguageVersionResolver.MaxSupportedLanguageVersion,
                                        dependencies,
                                        suppressUnsupported: false,
                                        maxParallelism: maxParallelism);
    }

    /// <summary>
    /// Sets up the file system fake so that every given path is an existing file with the given content, whose full path is the
    /// path itself
    /// </summary>
    /// <param name="fileSystem">The file system fake</param>
    /// <param name="files">The paths and contents</param>
    private static void SetupFiles(IFileSystem fileSystem, params (string Path, string Content)[] files)
    {
        foreach (var (path, content) in files)
        {
            fileSystem.FileExists(path).Returns(true);
            fileSystem.DirectoryExists(path).Returns(false);
            fileSystem.GetFullPath(path).Returns(path);
            fileSystem.ReadFileAsync(path, Arg.Any<CancellationToken>()).Returns(new FileReadResult(content, _utf8NoBom));
            fileSystem.ReadSourceHeaderAsync(path, Arg.Any<CancellationToken>()).Returns(content);
        }
    }

    /// <summary>
    /// Sets up the file system fake so that reading the given file waits for the given event before it returns the content
    /// </summary>
    /// <param name="fileSystem">The file system fake</param>
    /// <param name="path">The path</param>
    /// <param name="content">The content</param>
    /// <param name="heldFor">The event the read waits for</param>
    /// <param name="releasedByEvent">Set when the read was released by the event rather than by the timeout</param>
    private static void HoldRead(IFileSystem fileSystem, string path, string content, Task heldFor, StrongBox<bool> releasedByEvent)
    {
        fileSystem.ReadFileAsync(path, Arg.Any<CancellationToken>())
                  .Returns(async _ =>
                           {
                               releasedByEvent.Value = await WaitForAsync(heldFor);

                               // Give the file that finished first the time to write its lines, if it writes them on finishing
                               await Task.Delay(100);

                               return new FileReadResult(content, _utf8NoBom);
                           });
    }

    /// <summary>
    /// Waits for a task or for <see cref="_holdTimeout"/>, whichever comes first
    /// </summary>
    /// <param name="task">The task</param>
    /// <returns><see langword="true"/> when the task completed first; otherwise, <see langword="false"/></returns>
    private static async Task<bool> WaitForAsync(Task task)
    {
        var completed = await Task.WhenAny(task, Task.Delay(_holdTimeout));

        return completed == task;
    }

    /// <summary>
    /// Creates a formatter fake that returns <see cref="FormattedCsContent"/> for every file and signals when it has formatted
    /// the given file
    /// </summary>
    /// <param name="signalPath">The file whose formatting is signaled</param>
    /// <param name="formatted">Completed when the formatter has formatted <paramref name="signalPath"/></param>
    /// <param name="refusedPath">A file the formatter refuses as unsupported at its first character, or <see langword="null"/></param>
    /// <param name="failingPath">A file the formatter throws for, or <see langword="null"/></param>
    /// <returns>The formatter fake</returns>
    private static ISourceFormatter CreateFormatter(string signalPath, TaskCompletionSource formatted, string refusedPath = null, string failingPath = null)
    {
        var formatter = Substitute.For<ISourceFormatter>();

        formatter.FormatSyntaxTree(Arg.Any<SyntaxTree>(), Arg.Any<CancellationToken>())
                 .Returns(callInfo =>
                          {
                              var syntaxTree = callInfo.Arg<SyntaxTree>();

                              if (syntaxTree.FilePath == failingPath)
                              {
                                  throw new InvalidOperationException("boom");
                              }

                              FormattingResult<SyntaxTree> result;

                              if (syntaxTree.FilePath == refusedPath)
                              {
                                  result = new FormattingResult<SyntaxTree>(syntaxTree, new UnsupportedSyntax(UnsupportedReason, Location.Create(syntaxTree, new TextSpan(0, 1))));
                              }
                              else
                              {
                                  result = new FormattingResult<SyntaxTree>(CSharpSyntaxTree.ParseText(FormattedCsContent), null);
                              }

                              if (syntaxTree.FilePath == signalPath)
                              {
                                  formatted.TrySetResult();
                              }

                              return result;
                          });

        return formatter;
    }

    /// <summary>
    /// Prefixes standard output lines for a comparison with <see cref="CapturedConsoleOutput.InterleavedOutput"/>
    /// </summary>
    /// <param name="lines">The lines</param>
    /// <returns>The prefixed lines</returns>
    private static string[] StandardOutputLines(params string[] lines)
    {
        return lines.Select(line => Out + line).ToArray();
    }

    #endregion // Helper Methods

    #region Output Order

    /// <summary>
    /// Verifies the specification example: with <c>--check --verbose</c> and a degree of four, <c>A.cs</c> and <c>C.cs</c>
    /// unformatted and <c>B.cs</c> with a syntax error, the output is the one-after-another output in selection order, also when
    /// <c>C.cs</c> finishes before <c>A.cs</c>
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    [Requirement("CLI-022")]
    public async Task CheckSpecExampleKeepsSelectionOrderWhenLastFileFinishesFirst()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var cFormatted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var formatter = CreateFormatter("/work/C.cs", cFormatted);
        var releasedByEvent = new StrongBox<bool>();

        SetupFiles(fileSystem, ("/work/A.cs", UnformattedCsContent), ("/work/B.cs", SyntaxErrorContent), ("/work/C.cs", UnformattedCsContent));
        HoldRead(fileSystem, "/work/A.cs", UnformattedCsContent, cFormatted.Task, releasedByEvent);

        var handler = CreateHandler(["/work/A.cs", "/work/B.cs", "/work/C.cs"], "check", verbose: true, maxParallelism: 4, fileSystem, console, formatter);

        var exitCode = await handler.ExecuteAsync(CancellationToken.None);

        Assert.IsTrue(releasedByEvent.Value, "Precondition: C.cs must have been formatted while A.cs was still being read.");
        Assert.AreEqual(ExitCodes.FormattingNeeded, exitCode);
        Assert.AreSequenceEqual(StandardOutputLines("Not formatted: /work/A.cs",
                                                    "Skipped (syntax errors): /work/B.cs",
                                                    "Not formatted: /work/C.cs",
                                                    string.Empty,
                                                    "2 of 3 file(s) need formatting.",
                                                    "Skipped 1 file(s) with syntax errors."),
                                console.InterleavedOutput.ToArray());
    }

    /// <summary>
    /// Verifies the specification example with a degree of one: the output is the same as with a degree of four
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    [Requirement("CLI-022")]
    [Requirement("CLI-023")]
    public async Task CheckSpecExampleWithDegreeOneGivesTheSameOutput()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var formatter = CreateFormatter(null, new TaskCompletionSource());

        SetupFiles(fileSystem, ("/work/A.cs", UnformattedCsContent), ("/work/B.cs", SyntaxErrorContent), ("/work/C.cs", UnformattedCsContent));

        var handler = CreateHandler(["/work/A.cs", "/work/B.cs", "/work/C.cs"], "check", verbose: true, maxParallelism: 1, fileSystem, console, formatter);

        var exitCode = await handler.ExecuteAsync(CancellationToken.None);

        Assert.AreEqual(ExitCodes.FormattingNeeded, exitCode);
        Assert.AreSequenceEqual(StandardOutputLines("Not formatted: /work/A.cs",
                                                    "Skipped (syntax errors): /work/B.cs",
                                                    "Not formatted: /work/C.cs",
                                                    string.Empty,
                                                    "2 of 3 file(s) need formatting.",
                                                    "Skipped 1 file(s) with syntax errors."),
                                console.InterleavedOutput.ToArray());
    }

    /// <summary>
    /// Verifies that a file skipped as generated by its name is named in selection order between the files around it, also when
    /// the file after it finishes before the file in front of it
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    [Requirement("CLI-022")]
    public async Task GeneratedFileSkippedByNameKeepsSelectionOrder()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var cFormatted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var formatter = CreateFormatter("/work/C.cs", cFormatted);
        var releasedByEvent = new StrongBox<bool>();

        SetupFiles(fileSystem, ("/work/A.cs", UnformattedCsContent), ("/work/X.g.cs", UnformattedCsContent), ("/work/C.cs", UnformattedCsContent));
        HoldRead(fileSystem, "/work/A.cs", UnformattedCsContent, cFormatted.Task, releasedByEvent);

        var handler = CreateHandler(["/work/A.cs", "/work/X.g.cs", "/work/C.cs"], "check", verbose: true, maxParallelism: 4, fileSystem, console, formatter);

        var exitCode = await handler.ExecuteAsync(CancellationToken.None);

        Assert.IsTrue(releasedByEvent.Value, "Precondition: C.cs must have been formatted while A.cs was still being read.");
        Assert.AreEqual(ExitCodes.FormattingNeeded, exitCode);
        Assert.AreSequenceEqual(StandardOutputLines("Not formatted: /work/A.cs",
                                                    "Skipped (generated): /work/X.g.cs",
                                                    "Not formatted: /work/C.cs",
                                                    string.Empty,
                                                    "2 of 2 file(s) need formatting.",
                                                    "Skipped 1 generated file(s)."),
                                console.InterleavedOutput.ToArray());
    }

    /// <summary>
    /// Verifies the specification example with an unsupported file between two unformatted files: the <c>error:</c> line on
    /// standard error stands between the two <c>Not formatted:</c> lines on standard output, and the run exits with code 2
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    [Requirement("CLI-022")]
    public async Task UnsupportedFileErrorLineStaysBetweenItsNeighborsAcrossStreams()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var cFormatted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var formatter = CreateFormatter("/work/C.cs", cFormatted, refusedPath: "/work/u.cs");
        var releasedByEvent = new StrongBox<bool>();

        SetupFiles(fileSystem, ("/work/A.cs", UnformattedCsContent), ("/work/u.cs", UnformattedCsContent), ("/work/C.cs", UnformattedCsContent));
        HoldRead(fileSystem, "/work/A.cs", UnformattedCsContent, cFormatted.Task, releasedByEvent);

        var handler = CreateHandler(["/work/A.cs", "/work/u.cs", "/work/C.cs"], "check", verbose: false, maxParallelism: 4, fileSystem, console, formatter);

        var exitCode = await handler.ExecuteAsync(CancellationToken.None);

        Assert.IsTrue(releasedByEvent.Value, "Precondition: C.cs must have been formatted while A.cs was still being read.");
        Assert.AreEqual(ExitCodes.Error, exitCode);
        Assert.AreSequenceEqual(new[]
                                {
                                    Out + "Not formatted: /work/A.cs",
                                    Err + $"error: /work/u.cs(1,1): unsupported syntax: {UnsupportedReason}. The file was left unchanged.",
                                    Out + "Not formatted: /work/C.cs",
                                    Out,
                                    Out + "2 of 3 file(s) need formatting.",
                                    Out + "Skipped 1 file(s) with unsupported syntax."
                                },
                                console.InterleavedOutput.ToArray());
    }

    /// <summary>
    /// Verifies that a file the formatter fails on does not stop or change the processing of the files around it, and that its
    /// error line on standard error stands between their lines
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    [Requirement("CLI-022")]
    public async Task FailingFileDoesNotAffectOtherFilesAndKeepsSelectionOrder()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var cFormatted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var formatter = CreateFormatter("/work/C.cs", cFormatted, failingPath: "/work/B.cs");
        var releasedByEvent = new StrongBox<bool>();

        SetupFiles(fileSystem, ("/work/A.cs", UnformattedCsContent), ("/work/B.cs", UnformattedCsContent), ("/work/C.cs", UnformattedCsContent));
        HoldRead(fileSystem, "/work/A.cs", UnformattedCsContent, cFormatted.Task, releasedByEvent);

        var handler = CreateHandler(["/work/A.cs", "/work/B.cs", "/work/C.cs"], "check", verbose: false, maxParallelism: 4, fileSystem, console, formatter);

        var exitCode = await handler.ExecuteAsync(CancellationToken.None);

        Assert.IsTrue(releasedByEvent.Value, "Precondition: C.cs must have been formatted while A.cs was still being read.");
        Assert.AreEqual(ExitCodes.Error, exitCode);
        Assert.AreSequenceEqual(new[]
                                {
                                    Out + "Not formatted: /work/A.cs",
                                    Err + "Error processing /work/B.cs: boom",
                                    Out + "Not formatted: /work/C.cs",
                                    Out,
                                    Out + "2 of 3 file(s) need formatting.",
                                    Out + "1 file(s) encountered errors."
                                },
                                console.InterleavedOutput.ToArray());
    }

    /// <summary>
    /// Verifies that a file that cannot be decoded does not stop or change the processing of the files around it, and that its
    /// line on standard error stands between their lines
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    [Requirement("CLI-022")]
    public async Task UndecodableFileDoesNotAffectOtherFilesAndKeepsSelectionOrder()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var cFormatted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var formatter = CreateFormatter("/work/C.cs", cFormatted);
        var releasedByEvent = new StrongBox<bool>();

        SetupFiles(fileSystem, ("/work/A.cs", UnformattedCsContent), ("/work/B.cs", UnformattedCsContent), ("/work/C.cs", UnformattedCsContent));
        HoldRead(fileSystem, "/work/A.cs", UnformattedCsContent, cFormatted.Task, releasedByEvent);
        fileSystem.ReadFileAsync("/work/B.cs", Arg.Any<CancellationToken>())
                  .Returns<Task<FileReadResult>>(async _ =>
                                                 {
                                                     await Task.Yield();

                                                     throw new DecoderFallbackException();
                                                 });

        var handler = CreateHandler(["/work/A.cs", "/work/B.cs", "/work/C.cs"], "check", verbose: false, maxParallelism: 4, fileSystem, console, formatter);

        var exitCode = await handler.ExecuteAsync(CancellationToken.None);

        Assert.IsTrue(releasedByEvent.Value, "Precondition: C.cs must have been formatted while A.cs was still being read.");
        Assert.AreEqual(ExitCodes.FormattingNeeded, exitCode);
        Assert.AreSequenceEqual(new[]
                                {
                                    Out + "Not formatted: /work/A.cs",
                                    Err + "Skipped (could not decode): /work/B.cs",
                                    Out + "Not formatted: /work/C.cs",
                                    Out,
                                    Out + "2 of 3 file(s) need formatting.",
                                    Out + "Skipped 1 file(s) that could not be decoded."
                                },
                                console.InterleavedOutput.ToArray());
    }

    /// <summary>
    /// Verifies that <c>--dry-run</c> writes each file's diff directly behind its <c>Would format:</c> line, in selection order,
    /// also when the later file finishes first
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    [Requirement("CLI-022")]
    public async Task DryRunKeepsEveryDiffWithItsFileInSelectionOrder()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var cFormatted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var formatter = CreateFormatter("/work/C.cs", cFormatted);
        var diffGenerator = Substitute.For<IDiffGenerator>();
        var releasedByEvent = new StrongBox<bool>();

        SetupFiles(fileSystem, ("/work/A.cs", UnformattedCsContent), ("/work/C.cs", UnformattedCsContent));
        HoldRead(fileSystem, "/work/A.cs", UnformattedCsContent, cFormatted.Task, releasedByEvent);
        diffGenerator.Generate(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                     .Returns(callInfo => $"diff of {callInfo.ArgAt<string>(0)}");

        var handler = CreateHandler(["/work/A.cs", "/work/C.cs"], "dry-run", verbose: false, maxParallelism: 4, fileSystem, console, formatter, diffGenerator);

        var exitCode = await handler.ExecuteAsync(CancellationToken.None);

        Assert.IsTrue(releasedByEvent.Value, "Precondition: C.cs must have been formatted while A.cs was still being read.");
        Assert.AreEqual(ExitCodes.FormattingNeeded, exitCode);
        Assert.AreSequenceEqual(StandardOutputLines("Would format: /work/A.cs",
                                                    "diff of /work/A.cs",
                                                    "Would format: /work/C.cs",
                                                    "diff of /work/C.cs",
                                                    string.Empty,
                                                    "2 of 2 file(s) would be formatted."),
                                console.InterleavedOutput.ToArray());
    }

    /// <summary>
    /// Verifies that write mode writes every changed file with its formatted content, leaves the unchanged file alone, and names
    /// the files in selection order, also when the last file is written before the first one
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    [Requirement("CLI-022")]
    public async Task WriteModeWritesEveryChangedFileAndKeepsSelectionOrder()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var cWritten = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var formatter = CreateFormatter(null, new TaskCompletionSource());
        var releasedByEvent = new StrongBox<bool>();
        var writes = new ConcurrentDictionary<string, string>();

        SetupFiles(fileSystem, ("/work/A.cs", UnformattedCsContent), ("/work/B.cs", FormattedCsContent), ("/work/C.cs", UnformattedCsContent));
        HoldRead(fileSystem, "/work/A.cs", UnformattedCsContent, cWritten.Task, releasedByEvent);
        fileSystem.WriteAllTextAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Encoding>())
                  .Returns(callInfo =>
                           {
                               var path = callInfo.ArgAt<string>(0);

                               writes[path] = callInfo.ArgAt<string>(1);

                               if (path == "/work/C.cs")
                               {
                                   cWritten.TrySetResult();
                               }

                               return Task.CompletedTask;
                           });

        var handler = CreateHandler(["/work/A.cs", "/work/B.cs", "/work/C.cs"], "write", verbose: true, maxParallelism: 4, fileSystem, console, formatter);

        var exitCode = await handler.ExecuteAsync(CancellationToken.None);

        Assert.IsTrue(releasedByEvent.Value, "Precondition: C.cs must have been written while A.cs was still being read.");
        Assert.AreEqual(ExitCodes.Success, exitCode);
        Assert.AreSequenceEqual(StandardOutputLines("Formatted: /work/A.cs",
                                                    "Unchanged: /work/B.cs",
                                                    "Formatted: /work/C.cs",
                                                    string.Empty,
                                                    "Formatted 2 of 3 file(s)."),
                                console.InterleavedOutput.ToArray());
        Assert.HasCount(2, writes);
        Assert.AreEqual(FormattedCsContent, writes["/work/A.cs"]);
        Assert.AreEqual(FormattedCsContent, writes["/work/C.cs"]);
    }

    #endregion // Output Order

    #region Degree of Parallelism

    /// <summary>
    /// Verifies that a degree of one processes one file after another: no two files are read at the same time
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    [Requirement("CLI-023")]
    public async Task DegreeOneNeverProcessesTwoFilesAtTheSameTime()
    {
        var maximumInFlight = await RunWithInFlightCounter(fileCount: 4, expectedDegree: 1, ForDegree(1));

        Assert.AreEqual(1, maximumInFlight);
    }

    /// <summary>
    /// Verifies that a degree of two processes two files at the same time and never three
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    [Requirement("CLI-023")]
    public async Task DegreeTwoProcessesTwoFilesAtTheSameTimeButNeverThree()
    {
        var maximumInFlight = await RunWithInFlightCounter(fileCount: 6, expectedDegree: 2, ForDegree(2));

        Assert.AreEqual(2, maximumInFlight);
    }

    /// <summary>
    /// Verifies that a degree larger than the number of files is accepted and every file is processed, with the output of a
    /// degree of one
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    [Requirement("CLI-023")]
    public async Task DegreeAboveFileCountProcessesEveryFile()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var formatter = CreateFormatter(null, new TaskCompletionSource());

        SetupFiles(fileSystem, ("/work/A.cs", UnformattedCsContent), ("/work/B.cs", FormattedCsContent), ("/work/C.cs", UnformattedCsContent));

        var handler = CreateHandler(["/work/A.cs", "/work/B.cs", "/work/C.cs"], "check", verbose: true, maxParallelism: 1000, fileSystem, console, formatter);

        var exitCode = await handler.ExecuteAsync(CancellationToken.None);

        Assert.AreEqual(ExitCodes.FormattingNeeded, exitCode);
        Assert.AreSequenceEqual(StandardOutputLines("Not formatted: /work/A.cs",
                                                    "Unchanged: /work/B.cs",
                                                    "Not formatted: /work/C.cs",
                                                    string.Empty,
                                                    "2 of 3 file(s) need formatting."),
                                console.InterleavedOutput.ToArray());
    }

    /// <summary>
    /// Verifies that the count given with <c>--max-parallelism</c> on the command line is the degree the handler the CLI
    /// creates processes files with
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    [Requirement("CLI-023")]
    public async Task CommandLineCountIsTheDegreeOfTheHandler()
    {
        var parseResult = Program.ParseArguments(["--check", "--max-parallelism", "2"]);

        var maximumInFlight = await RunWithInFlightCounter(fileCount: 6, expectedDegree: 2, (paths, dependencies) => Program.CreateHandler(parseResult, paths, dependencies));

        Assert.AreEqual(2, maximumInFlight);
    }

    /// <summary>
    /// Verifies that without <c>--max-parallelism</c> the handler the CLI creates processes as many files at the same time as
    /// the machine has logical processors
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    [Requirement("CLI-023")]
    public async Task WithoutCountTheHandlerProcessesAsManyFilesAsLogicalProcessors()
    {
        var parseResult = Program.ParseArguments(["--check"]);
        var processorCount = Environment.ProcessorCount;

        var maximumInFlight = await RunWithInFlightCounter(fileCount: processorCount + 2, expectedDegree: processorCount, (paths, dependencies) => Program.CreateHandler(parseResult, paths, dependencies));

        Assert.AreEqual(processorCount, maximumInFlight);
    }

    /// <summary>
    /// Creates a factory for a check handler with the given degree of parallelism
    /// </summary>
    /// <param name="maxParallelism">The degree of parallelism</param>
    /// <returns>The factory</returns>
    private static Func<string[], FormatCommandDependencies, FormatCommandHandler> ForDegree(int maxParallelism)
    {
        return (paths, dependencies) => new FormatCommandHandler(paths,
                                                                 checkOnly: true,
                                                                 dryRun: false,
                                                                 verbose: false,
                                                                 force: true,
                                                                 utf8Bom: false,
                                                                 LanguageVersionResolver.MaxSupportedLanguageVersion,
                                                                 dependencies,
                                                                 suppressUnsupported: false,
                                                                 maxParallelism: maxParallelism);
    }

    /// <summary>
    /// Runs a check over the given number of files with a read fake that counts the reads in progress; every read waits until
    /// as many reads as the expected degree are in progress, until every file has started, or until one shared deadline, and
    /// then holds briefly, so a run that processes more files at the same time than the expected degree shows it in the count,
    /// and a run that processes fewer ends after the deadline with a lower count
    /// </summary>
    /// <param name="fileCount">The number of files</param>
    /// <param name="expectedDegree">The degree of parallelism the run is expected to reach</param>
    /// <param name="createHandler">Creates a check handler over the given paths and dependencies</param>
    /// <returns>The largest number of reads in progress at the same time</returns>
    private static async Task<int> RunWithInFlightCounter(int fileCount, int expectedDegree, Func<string[], FormatCommandDependencies, FormatCommandHandler> createHandler)
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var formatter = CreateFormatter(null, new TaskCompletionSource());
        var paths = Enumerable.Range(0, fileCount).Select(index => $"/work/File{index}.cs").ToArray();
        var gate = new object();
        var inFlight = 0;
        var started = 0;
        var maximumInFlight = 0;
        var deadline = DateTime.UtcNow + _holdTimeout;

        SetupFiles(fileSystem, paths.Select(path => (path, UnformattedCsContent)).ToArray());

        foreach (var path in paths)
        {
            fileSystem.ReadFileAsync(path, Arg.Any<CancellationToken>())
                      .Returns(async _ =>
                               {
                                   lock (gate)
                                   {
                                       inFlight++;
                                       started++;
                                       maximumInFlight = Math.Max(maximumInFlight, inFlight);
                                   }

                                   while (DateTime.UtcNow < deadline)
                                   {
                                       lock (gate)
                                       {
                                           if (inFlight >= expectedDegree || started == fileCount)
                                           {
                                               break;
                                           }
                                       }

                                       await Task.Delay(5);
                                   }

                                   await Task.Delay(50);

                                   lock (gate)
                                   {
                                       inFlight--;
                                   }

                                   return new FileReadResult(UnformattedCsContent, _utf8NoBom);
                               });
        }

        var dependencies = new FormatCommandDependencies(fileSystem, console, Substitute.For<IConsoleInput>(), formatter, Substitute.For<IDiffGenerator>());
        var handler = createHandler(paths, dependencies);

        var exitCode = await handler.ExecuteAsync(CancellationToken.None);

        Assert.AreEqual(ExitCodes.FormattingNeeded, exitCode);
        Assert.AreEqual($"{fileCount} of {fileCount} file(s) need formatting.", console.StandardOutput[^1]);

        return maximumInFlight;
    }

    #endregion // Degree of Parallelism

    #region Confirmation

    /// <summary>
    /// Verifies that the confirmation of a large write run is answered before any file is read for formatting, also with a
    /// degree above one
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    [Requirement("CLI-022")]
    public async Task LargeRunConfirmationIsAnsweredBeforeAnyFileIsReadForFormatting()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var formatter = CreateFormatter(null, new TaskCompletionSource());
        var consoleInput = Substitute.For<IConsoleInput>();
        var events = new ConcurrentQueue<string>();
        var paths = Enumerable.Range(0, FormatCommandHandler.LargeRunConfirmationThreshold + 1).Select(index => $"/work/File{index}.cs").ToArray();

        SetupFiles(fileSystem, paths.Select(path => (path, UnformattedCsContent)).ToArray());

        foreach (var path in paths)
        {
            fileSystem.ReadFileAsync(path, Arg.Any<CancellationToken>())
                      .Returns(_ =>
                               {
                                   events.Enqueue("read");

                                   return new FileReadResult(UnformattedCsContent, _utf8NoBom);
                               });
        }

        consoleInput.IsInteractive.Returns(true);
        consoleInput.ReadLine()
                    .Returns(_ =>
                             {
                                 events.Enqueue("confirm");

                                 return "y";
                             });

        var handler = CreateHandler(paths, "write", verbose: false, maxParallelism: 4, fileSystem, console, formatter, consoleInput: consoleInput, force: false);

        var exitCode = await handler.ExecuteAsync(CancellationToken.None);

        var recorded = events.ToArray();

        Assert.AreEqual(ExitCodes.Success, exitCode);
        Assert.AreEqual("confirm", recorded[0]);
        Assert.HasCount(paths.Length + 1, recorded);
    }

    #endregion // Confirmation

    #region Cancellation

    /// <summary>
    /// Verifies the specification example: Ctrl+C while <c>A.cs</c> is still being read, <c>B.cs</c> has been written, and the
    /// write of <c>C.cs</c> has started; the write of <c>C.cs</c> completes, <c>B.cs</c> and <c>C.cs</c> are named in selection
    /// order, <c>A.cs</c> is left as it was and not named, no summary is written, and the run ends canceled
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    [Requirement("CLI-017")]
    [Requirement("CLI-022")]
    public async Task CancellationSpecExampleNamesEveryFinishedFileInSelectionOrder()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var formatter = CreateFormatter(null, new TaskCompletionSource());
        var bWritten = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var writes = new ConcurrentDictionary<string, string>();
        var cWriteObservedB = false;

        using (var cts = new CancellationTokenSource())
        {
            SetupFiles(fileSystem, ("/work/A.cs", UnformattedCsContent), ("/work/B.cs", UnformattedCsContent), ("/work/C.cs", UnformattedCsContent));

            // A.cs is still being read when the run is canceled; its read observes its token
            fileSystem.ReadFileAsync("/work/A.cs", Arg.Any<CancellationToken>())
                      .Returns<Task<FileReadResult>>(async callInfo =>
                                                     {
                                                         await Task.Delay(_holdTimeout, callInfo.ArgAt<CancellationToken>(1));

                                                         throw new TimeoutException("A.cs was never canceled.");
                                                     });
            fileSystem.WriteAllTextAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Encoding>())
                      .Returns(async callInfo =>
                               {
                                   var path = callInfo.ArgAt<string>(0);

                                   if (path == "/work/C.cs")
                                   {
                                       // Ctrl+C arrives after B.cs is written and while the write of C.cs is in progress
                                       cWriteObservedB = await WaitForAsync(bWritten.Task);

                                       await cts.CancelAsync();
                                       await Task.Yield();
                                   }

                                   writes[path] = callInfo.ArgAt<string>(1);

                                   if (path == "/work/B.cs")
                                   {
                                       bWritten.TrySetResult();
                                   }
                               });

            var handler = CreateHandler(["/work/A.cs", "/work/B.cs", "/work/C.cs"], "write", verbose: true, maxParallelism: 4, fileSystem, console, formatter);

            await Assert.ThrowsAsync<OperationCanceledException>(() => handler.ExecuteAsync(cts.Token));

            Assert.IsTrue(cWriteObservedB, "Precondition: B.cs must have been written before the write of C.cs was interrupted.");
            Assert.AreSequenceEqual(StandardOutputLines("Formatted: /work/B.cs",
                                                        "Formatted: /work/C.cs"),
                                    console.InterleavedOutput.ToArray());
            Assert.HasCount(2, writes);
            Assert.AreEqual(FormattedCsContent, writes["/work/B.cs"]);
            Assert.AreEqual(FormattedCsContent, writes["/work/C.cs"]);
        }
    }

    /// <summary>
    /// Verifies that Ctrl+C while <c>A.cs</c> is still being read, <c>C.cs</c> has been written, and the write of <c>B.cs</c>
    /// has started names <c>B.cs</c> before <c>C.cs</c>, in selection order, although <c>C.cs</c> finished first
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    [Requirement("CLI-017")]
    [Requirement("CLI-022")]
    public async Task CancellationNamesFinishedFilesInSelectionOrderWhenLaterFileFinishedFirst()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var formatter = CreateFormatter(null, new TaskCompletionSource());
        var cWritten = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var writes = new ConcurrentDictionary<string, string>();
        var bWriteObservedC = false;

        using (var cts = new CancellationTokenSource())
        {
            SetupFiles(fileSystem, ("/work/A.cs", UnformattedCsContent), ("/work/B.cs", UnformattedCsContent), ("/work/C.cs", UnformattedCsContent));

            // A.cs is still being read when the run is canceled; its read observes its token
            fileSystem.ReadFileAsync("/work/A.cs", Arg.Any<CancellationToken>())
                      .Returns<Task<FileReadResult>>(async callInfo =>
                                                     {
                                                         await Task.Delay(_holdTimeout, callInfo.ArgAt<CancellationToken>(1));

                                                         throw new TimeoutException("A.cs was never canceled.");
                                                     });
            fileSystem.WriteAllTextAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Encoding>())
                      .Returns(async callInfo =>
                               {
                                   var path = callInfo.ArgAt<string>(0);

                                   if (path == "/work/B.cs")
                                   {
                                       // Ctrl+C arrives after C.cs is written and has finished, while the write of B.cs is in progress
                                       bWriteObservedC = await WaitForAsync(cWritten.Task);

                                       await Task.Delay(100, CancellationToken.None);
                                       await cts.CancelAsync();
                                       await Task.Yield();
                                   }

                                   writes[path] = callInfo.ArgAt<string>(1);

                                   if (path == "/work/C.cs")
                                   {
                                       cWritten.TrySetResult();
                                   }
                               });

            var handler = CreateHandler(["/work/A.cs", "/work/B.cs", "/work/C.cs"], "write", verbose: true, maxParallelism: 4, fileSystem, console, formatter);

            await Assert.ThrowsAsync<OperationCanceledException>(() => handler.ExecuteAsync(cts.Token));

            Assert.IsTrue(bWriteObservedC, "Precondition: C.cs must have been written before the write of B.cs was interrupted.");
            Assert.AreSequenceEqual(StandardOutputLines("Formatted: /work/B.cs",
                                                        "Formatted: /work/C.cs"),
                                    console.InterleavedOutput.ToArray());
            Assert.HasCount(2, writes);
            Assert.AreEqual(FormattedCsContent, writes["/work/B.cs"]);
            Assert.AreEqual(FormattedCsContent, writes["/work/C.cs"]);
        }
    }

    /// <summary>
    /// Verifies that Ctrl+C during the write of the file a degree of one processes first completes that write, names that file,
    /// and starts no further file; which of the three files comes first is not asserted
    /// </summary>
    /// <returns>A task representing the asynchronous test operation</returns>
    [TestMethod]
    [Requirement("CLI-017")]
    [Requirement("CLI-022")]
    public async Task CancellationDuringWriteCompletesItAndStartsNoFurtherFile()
    {
        var fileSystem = Substitute.For<IFileSystem>();
        var console = new CapturedConsoleOutput();
        var formatter = CreateFormatter(null, new TaskCompletionSource());
        var writes = new ConcurrentDictionary<string, string>();

        using (var cts = new CancellationTokenSource())
        {
            SetupFiles(fileSystem, ("/work/A.cs", UnformattedCsContent), ("/work/B.cs", UnformattedCsContent), ("/work/C.cs", UnformattedCsContent));
            fileSystem.WriteAllTextAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Encoding>())
                      .Returns(async callInfo =>
                               {
                                   await cts.CancelAsync();
                                   await Task.Yield();

                                   writes[callInfo.ArgAt<string>(0)] = callInfo.ArgAt<string>(1);
                               });

            var handler = CreateHandler(["/work/A.cs", "/work/B.cs", "/work/C.cs"], "write", verbose: false, maxParallelism: 1, fileSystem, console, formatter);

            await Assert.ThrowsAsync<OperationCanceledException>(() => handler.ExecuteAsync(cts.Token));

            Assert.HasCount(1, writes);

            var writtenPath = writes.Keys.Single();
            var readPaths = fileSystem.ReceivedCalls()
                                      .Where(call => call.GetMethodInfo().Name == nameof(IFileSystem.ReadFileAsync))
                                      .Select(call => (string)call.GetArguments()[0])
                                      .ToArray();

            Assert.AreEqual(FormattedCsContent, writes[writtenPath]);
            Assert.AreSequenceEqual(StandardOutputLines($"Formatted: {writtenPath}"), console.InterleavedOutput.ToArray());
            Assert.AreSequenceEqual(new[] { writtenPath }, readPaths);
        }
    }

    #endregion // Cancellation
}