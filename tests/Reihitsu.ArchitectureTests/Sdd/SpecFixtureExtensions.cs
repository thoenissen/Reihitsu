using System.Text;

using Reihitsu.ArchitectureTests.Helpers;

namespace Reihitsu.ArchitectureTests.Sdd;

/// <summary>
/// Writes the fixture specs of the spec-catalog self-tests in <see cref="SpecConsistencyTests"/>, <see cref="SpecStructureTests"/>,
/// <see cref="RequirementCoverageTests"/>, and <see cref="Formatter.PhaseOwnershipDocumentationTests"/>
/// </summary>
internal static class SpecFixtureExtensions
{
    #region Methods

    /// <summary>
    /// Writes a spec with the given code-owners cell and requirement IDs
    /// </summary>
    /// <param name="fixture">Fixture directory that stands in for the repository root</param>
    /// <param name="relativePath">Spec path relative to <c>sdd/specs</c></param>
    /// <param name="codeOwners">Content of the <c>Code owners</c> cell</param>
    /// <param name="requirementIds">Requirement IDs, one heading each</param>
    public static void WriteSpec(this FixtureDirectory fixture, string relativePath, string codeOwners, params string[] requirementIds)
    {
        var builder = new StringBuilder();

        builder.Append("# Spec\n\n| | |\n|---|---|\n| ID prefix | X |\n| Status | Active |\n")
               .Append($"| Code owners | {codeOwners} |\n")
               .Append("| Uses | — |\n\n## Requirements\n");

        foreach (var requirementId in requirementIds)
        {
            builder.Append($"\n### {requirementId} — Title\n\nThe formatter MUST do it.\n");
        }

        fixture.WriteFile($"sdd/specs/{relativePath}", builder.ToString());
    }

    #endregion // Methods
}