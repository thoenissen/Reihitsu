using System.Collections.Immutable;

using Microsoft.CodeAnalysis;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Test.Base;

/// <summary>
/// Probe analyzer exposing the protected multi-location diagnostic factory of <see cref="DiagnosticAnalyzerBase{TAnalyzer}"/>
/// </summary>
public sealed class DiagnosticAnalyzerBaseProbe : DiagnosticAnalyzerBase<DiagnosticAnalyzerBaseProbe>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public DiagnosticAnalyzerBaseProbe()
        : base("RH2001", DiagnosticCategory.Design, nameof(AnalyzerResources.RH2001Title), nameof(AnalyzerResources.RH2001MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Invokes the protected multi-location diagnostic factory
    /// </summary>
    /// <param name="locations">Locations</param>
    /// <returns>Created <see cref="Diagnostic"/> object</returns>
    public Diagnostic Probe(ImmutableArray<Location> locations)
    {
        return CreateDiagnostic(locations);
    }

    #endregion // Methods
}