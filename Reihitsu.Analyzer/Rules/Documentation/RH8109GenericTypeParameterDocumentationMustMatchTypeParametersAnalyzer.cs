using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH8109: Generic type parameter documentation must match type parameters
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8109GenericTypeParameterDocumentationMustMatchTypeParametersAnalyzer : DiagnosticAnalyzerBase<RH8109GenericTypeParameterDocumentationMustMatchTypeParametersAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH8109";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH8109GenericTypeParameterDocumentationMustMatchTypeParametersAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Documentation, nameof(AnalyzerResources.RH8109Title), nameof(AnalyzerResources.RH8109MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyze a declaration with generic type parameters
    /// </summary>
    /// <param name="context">Context</param>
    private void OnDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MemberDeclarationSyntax declaration)
        {
            return;
        }

        var typeParameters = DocumentationAnalysisUtilities.GetTypeParameters(declaration);
        var documentationComment = DirectDocumentationSyntaxChecker.GetDocumentationComment(declaration);

        if (typeParameters.IsDefaultOrEmpty
            || documentationComment == null)
        {
            return;
        }

        var typeParameterNodes = DirectDocumentationSyntaxChecker.GetDirectTags(documentationComment, "typeparam");

        if (typeParameterNodes.IsDefaultOrEmpty)
        {
            return;
        }

        var declaredTypeParameterNames = typeParameters.Select(obj => obj.Identifier.ValueText).ToImmutableArray();

        for (var typeParameterIndex = 0; typeParameterIndex < typeParameterNodes.Length; typeParameterIndex++)
        {
            var documentedTypeParameterName = DocumentationAnalysisUtilities.GetNameAttributeValue(typeParameterNodes[typeParameterIndex]);

            if (string.IsNullOrWhiteSpace(documentedTypeParameterName))
            {
                continue;
            }

            if ((typeParameterIndex >= declaredTypeParameterNames.Length
                 || string.Equals(declaredTypeParameterNames[typeParameterIndex], documentedTypeParameterName, StringComparison.Ordinal) == false)
                && (declaredTypeParameterNames.Contains(documentedTypeParameterName, StringComparer.Ordinal) == false
                    || typeParameterIndex >= declaredTypeParameterNames.Length
                    || string.Equals(declaredTypeParameterNames[typeParameterIndex], documentedTypeParameterName, StringComparison.Ordinal) == false))
            {
                context.ReportDiagnostic(CreateDiagnostic(typeParameterNodes[typeParameterIndex].GetLocation()));
            }
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeActionWithDocumentationModeCheck(OnDeclaration, DocumentationAnalysisUtilities.TypeParameterOwnerKinds);
    }

    #endregion // DiagnosticAnalyzer
}