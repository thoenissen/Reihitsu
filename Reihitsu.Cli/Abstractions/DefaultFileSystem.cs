using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp;

namespace Reihitsu.Cli.Abstractions;

/// <summary>
/// Default file system implementation that delegates to <see cref="File"/>, <see cref="Directory"/>, and <see cref="Path"/>
/// </summary>
internal sealed class DefaultFileSystem : IFileSystem
{
    #region Constants

    /// <summary>
    /// Number of decoded characters read before checking whether the complete source header is available
    /// </summary>
    private const int SourceHeaderBufferSize = 4096;

    #endregion // Constants

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

    /// <summary>
    /// Determines whether decoded source contains a token after its leading trivia
    /// </summary>
    /// <param name="sourceText">The decoded source prefix</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests</param>
    /// <returns><see langword="true"/> when the complete source header is available; otherwise, <see langword="false"/></returns>
    private static bool HasCompleteSourceHeader(string sourceText, CancellationToken cancellationToken)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceText, cancellationToken: cancellationToken);
        var firstToken = syntaxTree.GetRoot(cancellationToken).GetFirstToken(includeZeroWidth: true);

        return firstToken.RawKind != (int)SyntaxKind.EndOfFileToken;
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
    public async Task<string> ReadSourceHeaderAsync(string path, CancellationToken cancellationToken)
    {
        using (var fileStream = new FileStream(path,
                                               FileMode.Open,
                                               FileAccess.Read,
                                               FileShare.ReadWrite | FileShare.Delete,
                                               SourceHeaderBufferSize,
                                               FileOptions.Asynchronous | FileOptions.SequentialScan))
            using (var reader = new StreamReader(fileStream,
                                                 new UTF8Encoding(encoderShouldEmitUTF8Identifier: false,
                                                                  throwOnInvalidBytes: true),
                                                 detectEncodingFromByteOrderMarks: true,
                                                 SourceHeaderBufferSize))
            {
                var sourceHeader = new StringBuilder();
                var buffer = new char[SourceHeaderBufferSize];
                var nextInspectionLength = SourceHeaderBufferSize;

                while (true)
                {
                    var charactersRead = await reader.ReadAsync(buffer.AsMemory(), cancellationToken).ConfigureAwait(false);

                    if (charactersRead == 0)
                    {
                        return sourceHeader.ToString();
                    }

                    sourceHeader.Append(buffer, 0, charactersRead);

                    if (sourceHeader.Length < nextInspectionLength)
                    {
                        continue;
                    }

                    var sourceHeaderText = sourceHeader.ToString();

                    if (HasCompleteSourceHeader(sourceHeaderText, cancellationToken))
                    {
                        return sourceHeaderText;
                    }

                    nextInspectionLength = nextInspectionLength <= int.MaxValue / 2
                                               ? nextInspectionLength * 2
                                               : int.MaxValue;
                }
            }
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