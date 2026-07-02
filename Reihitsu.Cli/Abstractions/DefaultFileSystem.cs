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
    public async Task<FileReadResult> ReadFileAsync(string path, CancellationToken cancellationToken)
    {
        // Read the raw bytes a single time, then both detect the encoding and decode the content from the same
        // buffer. This avoids reading every changed file twice (once for content and once for encoding detection).
        var fileBytes = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        var encoding = DetectEncoding(fileBytes);
        var content = Decode(fileBytes, encoding);

        return new FileReadResult(content, encoding);
    }

    /// <inheritdoc/>
    public Task WriteAllTextAsync(string path, string content, Encoding encoding, CancellationToken cancellationToken)
    {
        return File.WriteAllTextAsync(path, content, encoding, cancellationToken);
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
    /// Decodes the raw file bytes using the detected encoding, stripping any byte order mark
    /// </summary>
    /// <param name="fileBytes">The raw file bytes</param>
    /// <param name="encoding">The detected encoding</param>
    /// <returns>The decoded file content</returns>
    private static string Decode(byte[] fileBytes, Encoding encoding)
    {
        var preamble = encoding.GetPreamble();
        var offset = fileBytes.AsSpan().StartsWith(preamble) ? preamble.Length : 0;

        if (encoding is UTF8Encoding)
        {
            // Decode UTF-8 with a throwing decoder so legacy non-UTF-8 files (for example Windows-1252) are not
            // silently corrupted by replacement characters. Invalid UTF-8 raises a DecoderFallbackException.
            var strictUtf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

            return strictUtf8.GetString(fileBytes, offset, fileBytes.Length - offset);
        }

        return encoding.GetString(fileBytes, offset, fileBytes.Length - offset);
    }

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