using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0397: Region descriptions should not be Member or Members
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0397RegionDescriptionsShouldNotBeMemberOrMembersAnalyzer : DiagnosticAnalyzerBase<RH0397RegionDescriptionsShouldNotBeMemberOrMembersAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0397";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0397RegionDescriptionsShouldNotBeMemberOrMembersAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0397Title), nameof(AnalyzerResources.RH0397MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Gets the description of an endregion directive
    /// </summary>
    /// <param name="directive">Directive</param>
    /// <returns>Description text</returns>
    private static string GetEndRegionDescription(EndRegionDirectiveTriviaSyntax directive)
    {
        var description = (directive.EndRegionKeyword.TrailingTrivia.ToFullString()
                           + directive.EndOfDirectiveToken.LeadingTrivia.ToFullString()).Trim();

        if (description.StartsWith("//", StringComparison.Ordinal))
        {
            description = description.Substring(2).TrimStart();
        }

        return description;
    }

    /// <summary>
    /// Gets the description of a region directive
    /// </summary>
    /// <param name="directive">Directive</param>
    /// <returns>Description text</returns>
    private static string GetRegionDescription(RegionDirectiveTriviaSyntax directive)
    {
        var messageTrivia = directive.EndOfDirectiveToken.LeadingTrivia.FirstOrDefault(static trivia => trivia.IsKind(SyntaxKind.PreprocessingMessageTrivia));

        return messageTrivia == default
                   ? string.Empty
                   : messageTrivia.ToString().Trim();
    }

    /// <summary>
    /// Determines whether the provided description is forbidden
    /// </summary>
    /// <param name="description">Description</param>
    /// <returns><see langword="true"/> if the description is forbidden</returns>
    private static bool IsForbiddenDescription(string description)
    {
        return description.Equals("Member", StringComparison.OrdinalIgnoreCase)
               || description.Equals("Members", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Analyzes endregion directives
    /// </summary>
    /// <param name="context">Context</param>
    private void OnEndRegion(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is EndRegionDirectiveTriviaSyntax node
            && IsForbiddenDescription(GetEndRegionDescription(node)))
        {
            context.ReportDiagnostic(CreateDiagnostic(node.GetLocation()));
        }
    }

    /// <summary>
    /// Analyzes region directives
    /// </summary>
    /// <param name="context">Context</param>
    private void OnRegion(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is RegionDirectiveTriviaSyntax node
            && IsForbiddenDescription(GetRegionDescription(node)))
        {
            context.ReportDiagnostic(CreateDiagnostic(node.GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnRegion, SyntaxKind.RegionDirectiveTrivia);
        context.RegisterSyntaxNodeAction(OnEndRegion, SyntaxKind.EndRegionDirectiveTrivia);
    }

    #endregion // DiagnosticAnalyzer
}