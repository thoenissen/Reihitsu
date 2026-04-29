using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0434: Element parameter documentation must match element parameters
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0434ElementParameterDocumentationMustMatchElementParametersAnalyzer : DiagnosticAnalyzerBase<RH0434ElementParameterDocumentationMustMatchElementParametersAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0434";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0434ElementParameterDocumentationMustMatchElementParametersAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Documentation, nameof(AnalyzerResources.RH0434Title), nameof(AnalyzerResources.RH0434MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyze a parameter-bearing declaration
    /// </summary>
    /// <param name="context">Context</param>
    private void OnDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MemberDeclarationSyntax declaration)
        {
            return;
        }

        var parameters = DocumentationAnalysisUtilities.GetParameters(declaration);
        var documentationComment = DocumentationAnalysisUtilities.GetDocumentationComment(declaration);

        if (parameters.IsDefaultOrEmpty
            || documentationComment == null)
        {
            return;
        }

        var paramNodes = DocumentationAnalysisUtilities.GetDirectTags(documentationComment, "param");

        if (paramNodes.IsDefaultOrEmpty)
        {
            return;
        }

        var declaredParameterNames = parameters.Select(obj => obj.Identifier.ValueText).ToImmutableArray();

        for (var parameterIndex = 0; parameterIndex < paramNodes.Length; parameterIndex++)
        {
            var documentedParameterName = DocumentationAnalysisUtilities.GetNameAttributeValue(paramNodes[parameterIndex]);

            if (string.IsNullOrWhiteSpace(documentedParameterName))
            {
                continue;
            }

            if ((parameterIndex >= declaredParameterNames.Length
                 || string.Equals(declaredParameterNames[parameterIndex], documentedParameterName, StringComparison.Ordinal) == false)
                && (declaredParameterNames.Contains(documentedParameterName, StringComparer.Ordinal) == false
                    || parameterIndex >= declaredParameterNames.Length
                    || string.Equals(declaredParameterNames[parameterIndex], documentedParameterName, StringComparison.Ordinal) == false))
            {
                context.ReportDiagnostic(CreateDiagnostic(paramNodes[parameterIndex].GetLocation()));
            }
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnDeclaration, DocumentationAnalysisUtilities.ParameterOwnerKinds);
    }

    #endregion // DiagnosticAnalyzer
}