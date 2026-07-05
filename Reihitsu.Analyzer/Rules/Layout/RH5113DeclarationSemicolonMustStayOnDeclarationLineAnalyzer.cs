using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5113: Declaration semicolon must stay on the declaration line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5113DeclarationSemicolonMustStayOnDeclarationLineAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5113";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5113DeclarationSemicolonMustStayOnDeclarationLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5113Title), nameof(AnalyzerResources.RH5113MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Reports a diagnostic when the terminating semicolon is not on the same line as the end of the declaration
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="semicolonToken">Terminating semicolon token</param>
    private void CheckSemicolonLine(SyntaxNodeAnalysisContext context, SyntaxToken semicolonToken)
    {
        if (semicolonToken.IsMissing)
        {
            return;
        }

        var previousToken = semicolonToken.GetPreviousToken();

        if (previousToken.IsKind(SyntaxKind.None))
        {
            return;
        }

        var previousTokenEndLine = previousToken.GetLocation().GetLineSpan().EndLinePosition.Line;
        var semicolonLine = semicolonToken.GetLocation().GetLineSpan().StartLinePosition.Line;

        if (previousTokenEndLine != semicolonLine)
        {
            context.ReportDiagnostic(CreateDiagnostic(semicolonToken.GetLocation()));
        }
    }

    /// <summary>
    /// Analyzes field declarations
    /// </summary>
    /// <param name="context">Context</param>
    private void OnFieldDeclaration(SyntaxNodeAnalysisContext context)
    {
        var declaration = (FieldDeclarationSyntax)context.Node;

        CheckSemicolonLine(context, declaration.SemicolonToken);
    }

    /// <summary>
    /// Analyzes event field declarations
    /// </summary>
    /// <param name="context">Context</param>
    private void OnEventFieldDeclaration(SyntaxNodeAnalysisContext context)
    {
        var declaration = (EventFieldDeclarationSyntax)context.Node;

        CheckSemicolonLine(context, declaration.SemicolonToken);
    }

    /// <summary>
    /// Analyzes delegate declarations
    /// </summary>
    /// <param name="context">Context</param>
    private void OnDelegateDeclaration(SyntaxNodeAnalysisContext context)
    {
        var declaration = (DelegateDeclarationSyntax)context.Node;

        CheckSemicolonLine(context, declaration.SemicolonToken);
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnFieldDeclaration, SyntaxKind.FieldDeclaration);
        context.RegisterSyntaxNodeAction(OnEventFieldDeclaration, SyntaxKind.EventFieldDeclaration);
        context.RegisterSyntaxNodeAction(OnDelegateDeclaration, SyntaxKind.DelegateDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}