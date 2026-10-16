namespace Reihitsu.ArchitectureTests.Sdd;

/// <summary>
/// A <c>[Requirement]</c> attribute in a test source
/// </summary>
/// <param name="FilePath">Source path relative to the repository root, with forward slashes</param>
/// <param name="Line">One-based line of the attribute</param>
/// <param name="Id">Requirement ID the attribute names; <see langword="null"/> when its argument is not a single string literal</param>
/// <param name="IsOnTestMethod">Whether the attributed method is also marked <c>[TestMethod]</c></param>
internal sealed record RequirementReference(string FilePath, int Line, string Id, bool IsOnTestMethod);