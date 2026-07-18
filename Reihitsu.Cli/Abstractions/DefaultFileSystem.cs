using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Reihitsu.Cli.Abstractions;

/// <summary>
/// Default file system implementation that delegates to <see cref="File"/>, <see cref="Directory"/>, and <see cref="Path"/>
/// </summary>
internal sealed class DefaultFileSystem : IFileSystem
{
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
        var strictEncoding = CreateStrictDecodingEncoding(encoding);

        return strictEncoding.GetString(fileBytes, offset, fileBytes.Length - offset);
    }

    /// <summary>
    /// Builds a throwing-fallback variant of the detected encoding used only to validate the decoded bytes
    /// </summary>
    /// <param name="encoding">The detected encoding</param>
    /// <returns>An encoding whose decoder raises <see cref="DecoderFallbackException"/> on invalid byte sequences instead of substituting U+FFFD</returns>
    private static Encoding CreateStrictDecodingEncoding(Encoding encoding)
    {
        // Invalid byte sequences (for example an unpaired UTF-16 surrogate) must fail loudly so the caller can skip
        // the file instead of silently persisting replacement characters on the next formatting write. The
        // replacement-fallback encoding instance detected by DetectEncoding is still what gets reused for writing.
        return encoding.CodePage switch
               {
                   1200 => new UnicodeEncoding(bigEndian: false, byteOrderMark: false, throwOnInvalidBytes: true),
                   1201 => new UnicodeEncoding(bigEndian: true, byteOrderMark: false, throwOnInvalidBytes: true),
                   12000 => new UTF32Encoding(bigEndian: false, byteOrderMark: false, throwOnInvalidCharacters: true),
                   12001 => new UTF32Encoding(bigEndian: true, byteOrderMark: false, throwOnInvalidCharacters: true),
                   _ => new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true),
               };
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
}