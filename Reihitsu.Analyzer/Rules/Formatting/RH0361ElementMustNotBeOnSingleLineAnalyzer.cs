using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0361: Element must not be on a single line.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0361ElementMustNotBeOnSingleLineAnalyzer : DiagnosticAnalyzerBase<RH0361ElementMustNotBeOnSingleLineAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0361";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0361ElementMustNotBeOnSingleLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0361Title), nameof(AnalyzerResources.RH0361MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes the syntax tree.
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);

        foreach (var declaration in root.DescendantNodes().OfType<BaseTypeDeclarationSyntax>())
        {
            if (declaration is RecordDeclarationSyntax { ParameterList: not null } recordDeclaration
                && recordDeclaration.SemicolonToken.IsMissing == false)
            {
                continue;
            }

            if (declaration.OpenBraceToken.IsMissing
                || declaration.CloseBraceToken.IsMissing)
            {
                continue;
            }

            if (declaration.GetLocation().GetLineSpan().StartLinePosition.Line == declaration.GetLocation().GetLineSpan().EndLinePosition.Line)
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