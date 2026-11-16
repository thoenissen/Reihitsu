using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Reihitsu.Cli.Abstractions;

namespace Reihitsu.Cli.Test.Helpers;

/// <summary>
/// A file system with one physical file reached through two paths, as through a symbolic link. A write is not atomic: while it
/// is in progress, the file holds a given content without a byte order mark, as a truncated file whose write has not finished
/// does. The fake brings a read through the second path into an overlap with a write if the caller lets it: that read waits
/// until a write has started, and a write waits until that read has taken its snapshot. Both waits end after a hold timeout,
/// so a caller that never lets a read overlap a write still finishes
/// </summary>
internal sealed class SharedFileFileSystem : IFileSystem
{
    #region Fields

    /// <summary>
    /// UTF-8 encoding without a byte order mark
    /// </summary>
    private static readonly Encoding _utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    /// <summary>
    /// Guards the file's state
    /// </summary>
    private readonly Lock _gate = new();

    /// <summary>
    /// The path the file is reached through first
    /// </summary>
    private readonly string _firstPath;

    /// <summary>
    /// The second path that names the same file; its read is held until a write has started
    /// </summary>
    private readonly string _secondPath;

    /// <summary>
    /// What the file holds while a write is in progress
    /// </summary>
    private readonly string _contentDuringWrite;

    /// <summary>
    /// How long a held read or write waits for its event before it goes on
    /// </summary>
    private readonly TimeSpan _holdTimeout;

    /// <summary>
    /// Completed when the first write has started
    /// </summary>
    private readonly TaskCompletionSource _writeStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>
    /// Completed when the first write has completed
    /// </summary>
    private readonly TaskCompletionSource _writeCompleted = new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>
    /// Completed when the read through the second path has taken its snapshot
    /// </summary>
    private readonly TaskCompletionSource _secondReadTaken = new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>
    /// The number of writes in progress
    /// </summary>
    private int _writesInProgress;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="SharedFileFileSystem"/> class
    /// </summary>
    /// <param name="paths">The two paths that name the file: the first one, and the second one whose read is held</param>
    /// <param name="content">The file's content, without a byte order mark</param>
    /// <param name="contentDuringWrite">What the file holds while a write is in progress</param>
    /// <param name="holdTimeout">How long a held read or write waits for its event before it goes on</param>
    public SharedFileFileSystem(string[] paths, string content, string contentDuringWrite, TimeSpan holdTimeout)
    {
        _firstPath = paths[0];
        _secondPath = paths[1];
        _contentDuringWrite = contentDuringWrite;
        _holdTimeout = holdTimeout;

        Content = content;
        Encoding = _utf8NoBom;
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// The file's content
    /// </summary>
    public string Content { get; private set; }

    /// <summary>
    /// The file's encoding
    /// </summary>
    public Encoding Encoding { get; private set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Waits for a task or for the hold timeout, whichever comes first
    /// </summary>
    /// <param name="task">The task</param>
    /// <returns>A task that completes when the wait ends</returns>
    private Task WaitForAsync(Task task)
    {
        return Task.WhenAny(task, Task.Delay(_holdTimeout, CancellationToken.None));
    }

    #endregion // Methods

    #region IFileSystem

    /// <inheritdoc/>
    public bool FileExists(string path)
    {
        return path == _firstPath || path == _secondPath;
    }

    /// <inheritdoc/>
    public bool DirectoryExists(string path)
    {
        return false;
    }

    /// <inheritdoc/>
    public async Task<FileReadResult> ReadFileAsync(string path, CancellationToken cancellationToken)
    {
        if (path == _secondPath)
        {
            await WaitForAsync(_writeStarted.Task);
        }

        FileReadResult snapshot;
        bool duringWrite;

        lock (_gate)
        {
            snapshot = new FileReadResult(Content, Encoding);
            duringWrite = _writesInProgress > 0;
        }

        if (path == _secondPath)
        {
            _secondReadTaken.TrySetResult();
        }

        // A read that saw a write in progress returns after that write, so the caller's own write comes after it
        if (duringWrite)
        {
            await WaitForAsync(_writeCompleted.Task);
        }

        return snapshot;
    }

    /// <inheritdoc/>
    public Task<string> ReadSourceHeaderAsync(string path, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            return Task.FromResult(Content);
        }
    }

    /// <inheritdoc/>
    public async Task WriteAllTextAsync(string path, string content, Encoding encoding)
    {
        lock (_gate)
        {
            _writesInProgress++;

            Content = _contentDuringWrite;
            Encoding = _utf8NoBom;
        }

        _writeStarted.TrySetResult();

        await WaitForAsync(_secondReadTaken.Task);

        lock (_gate)
        {
            Content = content;
            Encoding = encoding;

            _writesInProgress--;
        }

        _writeCompleted.TrySetResult();
    }

    /// <inheritdoc/>
    public IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
    {
        return [];
    }

    /// <inheritdoc/>
    public string GetFullPath(string path)
    {
        return path;
    }

    #endregion // IFileSystem
}