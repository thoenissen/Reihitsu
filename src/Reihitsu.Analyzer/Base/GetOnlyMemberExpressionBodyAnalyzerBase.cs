using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Base;

/// <summary>
/// Base class for analyzers that report a get-only property or indexer whose accessor list the formatter replaces with an
/// expression body (SHARED-GETONLY)
/// </summary>
public abstract class GetOnlyMemberExpressionBodyAnalyzerBase : DiagnosticAnalyzerBase
{
    #region Fields

    /// <summary>
    /// The declaration kind analyzed by this rule
    /// </summary>
    private readonly SyntaxKind _declarationKind;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="diagnosticId">Diagnostic ID</param>
    /// <param name="declarationKind">Declaration kind: a property or an indexer declaration</param>
    private protected GetOnlyMemberExpressionBodyAnalyzerBase(string diagnosticId, SyntaxKind declarationKind)
        : base(diagnosticId, DiagnosticCategory.Clarity)
    {
        _declarationKind = declarationKind;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Reports the accessor list of a member whose shape the formatter converts to an expression body. A comment, directive,
    /// or disabled text that keeps the formatter from converting it does not keep the member from being reported; its code
    /// fix is withheld instead (SHARED-GETONLY-003, SHARED-GETONLY-006, SHARED-GETONLY-007)
    /// </summary>
    /// <param name="context">Context</param>
    private void OnDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is BasePropertyDeclarationSyntax member
            && member.SyntaxTree.Options is CSharpParseOptions parseOptions
            && GetOnlyMemberExpressionBodyRewriteUtilities.HasConvertibleShape(member, parseOptions.LanguageVersion))
        {
            context.ReportDiagnostic(CreateDiagnostic(member.AccessorList.GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnDeclaration, _declarationKind);
    }

    #endregion // DiagnosticAnalyzer
}