using System;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms.Enumerations;

/// <summary>
/// Identifies the structural transforms that a formatting run can switch off individually. A transform
/// that is not listed here always runs
/// </summary>
[Flags]
internal enum ConfigurableStructuralTransforms
{
    /// <summary>
    /// No structural transform
    /// </summary>
    None = 0,

    /// <summary>
    /// Conversion of single-statement property and indexer accessor blocks to expression bodies
    /// </summary>
    AccessorExpressionBody = 1
}