namespace Reihitsu.ArchitectureTests.Sdd;

/// <summary>
/// A type named in the <c>Code owners</c> header of a spec
/// </summary>
/// <param name="SpecPath">Spec path relative to <c>sdd/specs</c></param>
/// <param name="Text">The entry as written in the spec</param>
/// <param name="Name">Simple type name</param>
/// <param name="Arity">Number of type parameters</param>
internal sealed record SpecCodeOwner(string SpecPath, string Text, string Name, int Arity);