namespace Reihitsu.ArchitectureTests.General;

/// <summary>
/// One self-referential tracker reference found by a <see cref="SelfReferentialTrackerReferenceTests"/> scan
/// </summary>
/// <param name="FilePath">Path of the file carrying the reference, relative to the scanned root</param>
/// <param name="Line">One-based line number of the reference</param>
/// <param name="MatchedText">The matched tracker reference text</param>
internal readonly record struct TrackerReference(string FilePath, int Line, string MatchedText);