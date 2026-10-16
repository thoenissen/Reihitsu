namespace Reihitsu.ArchitectureTests.Sdd;

/// <summary>
/// A requirement heading in a spec
/// </summary>
/// <param name="SpecPath">Spec path relative to <c>sdd/specs</c></param>
/// <param name="Line">One-based line of the heading</param>
/// <param name="Id">Requirement ID</param>
/// <param name="IsRetired">Whether a <c>Status: Retired</c> line follows the heading within its requirement</param>
internal sealed record SpecRequirement(string SpecPath, int Line, string Id, bool IsRetired);