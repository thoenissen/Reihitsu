using System.Text;

namespace Reihitsu.Cli.Abstractions;

/// <summary>
/// Represents the content and detected encoding of a file read in a single pass
/// </summary>
/// <param name="Content">The decoded file content</param>
/// <param name="Encoding">The detected file encoding, used when writing the file back</param>
internal readonly record struct FileReadResult(string Content, Encoding Encoding);