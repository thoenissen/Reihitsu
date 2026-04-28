using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Reihitsu.Cli.Abstractions;

/// <summary>
/// Default file system implementation that delegates to <see cref="File"/>, <see cref="Directory"/>, and <see cref="Path"/>
/// </summary>
internal sealed class DefaultFileSystem : IFileSystem
{
    #region IFileSystem

    /// <inheritdoc/>
    public bool FileExists(string path)
    {
        return File.Exists(path);
    }

    /// <inheritdoc/>
    public bool DirectoryExists(string path)
    {
        return Directory.Exists(path);
    }

    /// <inheritdoc/>
    public Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken)
    {
        return File.ReadAllTextAsync(path, cancellationToken);
    }

    /// <inheritdoc/>
    public Task WriteAllTextAsync(string path, string content, Encoding encoding, CancellationToken cancellationToken)
    {
        return File.WriteAllTextAsync(path, content, encoding, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Encoding> DetectEncodingAsync(string path, CancellationToken cancellationToken)
    {
        var fileBytes = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);

        return DetectEncoding(fileBytes);
    }

    /// <inheritdoc/>
    public IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
    {
        return Directory.EnumerateFiles(path, searchPattern, searchOption);
    }

    /// <inheritdoc/>
    public string GetFullPath(string path)
    {
        return Path.GetFullPath(path);
    }

    #endregion // IFileSystem

    #region Methods

    /// <summary>
    /// Detects the text encoding from the file BOM and falls back to UTF-8 without BOM
    /// </summary>
    /// <param name="fileBytes">The raw file bytes</param>
    /// <returns>The detected encoding</returns>
    private static Encoding DetectEncoding(byte[] fileBytes)
    {
        if (fileBytes.AsSpan().StartsWith(new byte[] { 0xFF, 0xFE, 0x00, 0x00 }))
        {
            return new UTF32Encoding(bigEndian: false, byteOrderMark: true);
        }

        if (fileBytes.AsSpan().StartsWith(new byte[] { 0x00, 0x00, 0xFE, 0xFF }))
        {
            return new UTF32Encoding(bigEndian: true, byteOrderMark: true);
        }

        if (fileBytes.AsSpan().StartsWith(new byte[] { 0xEF, 0xBB, 0xBF }))
        {
            return new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
        }

        if (fileBytes.AsSpan().StartsWith(new byte[] { 0xFF, 0xFE }))
        {
            return Encoding.Unicode;
        }

        if (fileBytes.AsSpan().StartsWith(new byte[] { 0xFE, 0xFF }))
        {
            return Encoding.BigEndianUnicode;
        }

        return new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
    }

    #endregion // Methods
}