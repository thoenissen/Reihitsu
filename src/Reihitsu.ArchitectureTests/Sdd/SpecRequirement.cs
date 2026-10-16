namespace Reihitsu.ArchitectureTests.Sdd;

/// <summary>
/// A requirement heading in a spec
/// </summary>
/// <param name="SpecPath">Spec path relative to <c>sdd/specs</c></param>
/// <param name="Line">One-based line of the heading</param>
/// <param name="Id">Requirement ID</param>
internal sealed record SpecRequirement(string SpecPath, int Line, string Id);