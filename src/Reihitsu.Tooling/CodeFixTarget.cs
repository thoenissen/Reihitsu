using System.Collections.Immutable;

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Reihitsu.Tooling;

/// <summary>
/// The analyzer and code fix provider a diagnostic ID resolves to
/// </summary>
public sealed class CodeFixTarget
{
    #region Properties

    /// <summary>
    /// Diagnostic ID the target was resolved from
    /// </summary>
    public string DiagnosticId { get; }

    /// <summary>
    /// Analyzers that report the diagnostic
    /// </summary>
    public ImmutableArray<DiagnosticAnalyzer> Analyzers { get; }

    /// <summary>
    /// The single code fix provider that declares the diagnostic as fixable
    /// </summary>
    public CodeFixProvider CodeFixProvider { get; }

    #endregion // Properties

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="CodeFixTarget"/> class
    /// </summary>
    /// <param name="diagnosticId">Diagnostic ID the target was resolved from</param>
    /// <param name="analyzers">Analyzers that report the diagnostic</param>
    /// <param name="codeFixProvider">The code fix provider that declares the diagnostic as fixable</param>
    public CodeFixTarget(string diagnosticId, ImmutableArray<DiagnosticAnalyzer> analyzers, CodeFixProvider codeFixProvider)
    {
        DiagnosticId = diagnosticId;
        Analyzers = analyzers;
        CodeFixProvider = codeFixProvider;
    }

    #endregion // Constructor
}