using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Naming;

/// <summary>
/// RH4116: Named tuple elements should be in PascalCase
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH4116TupleElementCasingAnalyzer : CasingAnalyzerBase
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH4116";

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH4116TupleElementCasingAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Naming, nameof(AnalyzerResources.RH4116Title), nameof(AnalyzerResources.RH4116MessageFormat), SyntaxKind.TupleType, CasingUtilities.IsPascalCase)
    {
    }

    #endregion // Constructor

    #region CasingAnalyzerBase

    /// <inheritdoc/>
    protected override IEnumerable<(string Name, Location Location)> GetLocations(SyntaxNode node)
    {
        if (node is TupleTypeSyntax tupleType)
        {
            foreach (var identifier in tupleType.Elements
                                                .Where(element => element.Identifier != default)
                                                .Select(element => element.Identifier))
            {
                yield return (identifier.ValueText, identifier.GetLocation());
            }
        }
    }

    #endregion // CasingAnalyzerBase
}