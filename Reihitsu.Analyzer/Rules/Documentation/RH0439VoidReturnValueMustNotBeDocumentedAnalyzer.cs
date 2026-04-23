using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0439: Void return value must not be documented.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0439VoidReturnValueMustNotBeDocumentedAnalyzer : DiagnosticAnalyzerBase<RH0439VoidReturnValueMustNotBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0439";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0439VoidReturnValueMustNotBeDocumentedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Documentation, nameof(AnalyzerResources.RH0439Title), nameof(AnalyzerResources.RH0439MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyze a declaration with a void return type.
    /// </summary>
    /// <param name="context">Context</param>
    private void OnDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MemberDeclarationSyntax declaration
            || DocumentationAnalysisUtilities.IsVoidReturnType(declaration) == false)
        {
            return;
        }

        var documentationComment = DocumentationAnalysisUtilities.GetDocumentationComment(declaration);

        if (documentationComment == null)
        {
            return;
        }

        var returnsNode = DocumentationAnalysisUtilities.GetFirstDirectTag(documentationComment, "returns");

        if (returnsNode == null)
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(returnsNode.GetLocation()));
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnDeclaration, DocumentationAnalysisUtilities.ReturnValueOwnerKinds);
    }

    #endregion // DiagnosticAnalyzer
}