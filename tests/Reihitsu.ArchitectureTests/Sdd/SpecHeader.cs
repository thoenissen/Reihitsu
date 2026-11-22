using System.Collections.Generic;

namespace Reihitsu.ArchitectureTests.Sdd;

/// <summary>
/// The header table of a spec
/// </summary>
/// <param name="SpecPath">Spec path relative to <c>sdd/specs</c></param>
/// <param name="Rows">Cell text by row label, such as <c>ID prefix</c> or <c>Uses</c></param>
internal sealed record SpecHeader(string SpecPath, IReadOnlyDictionary<string, string> Rows)
{
    #region Methods

    /// <summary>
    /// Gets the cell of a row
    /// </summary>
    /// <param name="label">Row label</param>
    /// <returns>The cell text, or <see langword="null"/> when the header has no such row</returns>
    public string GetRow(string label)
    {
        return Rows.TryGetValue(label, out var value) ? value : null;
    }

    #endregion // Methods
}