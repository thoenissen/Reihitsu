namespace Reihitsu.ArchitectureTests.General;

/// <summary>
/// One string literal that quotes a resource text, found by a <see cref="ResourceTextLiteralTests"/> scan
/// </summary>
/// <param name="FilePath">Path of the file carrying the literal, relative to the scanned root</param>
/// <param name="Line">One-based line number of the quoted text</param>
/// <param name="ResourceText">The quoted resource text, or the fixed fragment of a value with placeholders</param>
internal readonly record struct ResourceTextLiteral(string FilePath, int Line, string ResourceText);