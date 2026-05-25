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
/// RH0231: Primary constructor parameter names on records should be in PascalCase
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0231RecordPrimaryConstructorParameterCasingAnalyzer : CasingAnalyzerBase<RH0231RecordPrimaryConstructorParameterCasingAnalyzer>
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0231";

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0231RecordPrimaryConstructorParameterCasingAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Naming, nameof(AnalyzerResources.RH0231Title), nameof(AnalyzerResources.RH0231MessageFormat), SyntaxKind.Parameter, CasingUtilities.IsPascalCase)
    {
    }

    #endregion // Constructor

    #region CasingAnalyzerBase

    /// <inheritdoc/>
    protected override IEnumerable<(string Name, Location Location)> GetLocations(SyntaxNode node)
    {
        if (node is ParameterSyntax parameter
            && parameter.Identifier.ValueText != "_"
            && parameter.Parent is ParameterListSyntax { Parent: RecordDeclarationSyntax })
        {
            yield return (parameter.Identifier.ValueText, parameter.Identifier.GetLocation());
        }
    }

    #endregion // CasingAnalyzerBase
}