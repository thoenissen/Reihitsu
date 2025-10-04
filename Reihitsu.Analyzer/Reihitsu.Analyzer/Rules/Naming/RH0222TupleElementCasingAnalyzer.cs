using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Naming;

/// <summary>
/// RH0222: Named tuple elements should be in PascalCase
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0222TupleElementCasingAnalyzer : CasingAnalyzerBase<RH0222TupleElementCasingAnalyzer>
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0222";

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0222TupleElementCasingAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Naming, nameof(AnalyzerResources.RH0222Title), nameof(AnalyzerResources.RH0222MessageFormat), SyntaxKind.TupleType, CasingUtilities.IsPascalCase)
    {
    }

    #endregion // Constructor

    #region CasingAnalyzerBase

    /// <inheritdoc/>
    protected override IEnumerable<(string Name, Location Location)> GetLocations(SyntaxNode node)
    {
        if (node is TupleTypeSyntax tupleType)
        {
            foreach (var element in tupleType.Elements)
            {
                if (element.Identifier != default)
                {
                    yield return (element.Identifier.ValueText, element.Identifier.GetLocation());
                }
            }
        }
    }

    #endregion // CasingAnalyzerBase
}