using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0812: Final enum member must not have trailing comma
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0812FinalEnumMemberMustNotHaveTrailingCommaAnalyzer : DiagnosticAnalyzerBase<RH0812FinalEnumMemberMustNotHaveTrailingCommaAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0812";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0812FinalEnumMemberMustNotHaveTrailingCommaAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0812Title), nameof(AnalyzerResources.RH0812MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes enum declarations
    /// </summary>
    /// <param name="context">Context</param>
    private void OnEnumDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not EnumDeclarationSyntax enumDeclaration)
        {
            return;
        }

        var membersAndSeparators = enumDeclaration.Members.GetWithSeparators();

        if (membersAndSeparators.Count == 0)
        {
            return;
        }

        var lastItem = membersAndSeparators[membersAndSeparators.Count - 1];

        if (lastItem.IsToken && lastItem.AsToken().IsKind(SyntaxKind.CommaToken))
        {
            context.ReportDiagnostic(CreateDiagnostic(lastItem.AsToken().GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnEnumDeclaration, SyntaxKind.EnumDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}