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
    /// Normalization of property and indexer body forms: conversion of single-statement accessor blocks to
    /// expression-bodied accessors, and of get-only accessor lists to expression-bodied members
    /// </summary>
    AccessorExpressionBody = 1
}