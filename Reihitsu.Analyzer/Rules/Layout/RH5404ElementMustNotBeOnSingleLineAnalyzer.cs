using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5404: Element must not be on a single line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5404ElementMustNotBeOnSingleLineAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5404";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5404ElementMustNotBeOnSingleLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5404Title), nameof(AnalyzerResources.RH5404MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes the syntax tree
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);

        foreach (var declaration in root.DescendantNodes().OfType<BaseTypeDeclarationSyntax>())
        {
            if (declaration is TypeDeclarationSyntax typeDeclaration
                && typeDeclaration.SemicolonToken.IsKind(SyntaxKind.SemicolonToken))
            {
                continue;
            }

            if (declaration.OpenBraceToken.IsMissing
                || declaration.CloseBraceToken.IsMissing)
            {
                continue;
            }

            var openBraceLine = declaration.OpenBraceToken.GetLocation().GetLineSpan().StartLinePosition.Line;
            var closeBraceLine = declaration.CloseBraceToken.GetLocation().GetLineSpan().EndLinePosition.Line;

            if (openBraceLine == closeBraceLine)
            {
                context.ReportDiagnostic(CreateDiagnostic(declaration.Identifier.GetLocation()));
            }
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxTreeAction(OnSyntaxTree);
    }

    #endregion // DiagnosticAnalyzer
}