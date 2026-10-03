using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5039: Comments must not be placed inside a statement header
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5039CommentsMustNotBePlacedInsideStatementHeaderAnalyzer : CommentRegionAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5039";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5039CommentsMustNotBePlacedInsideStatementHeaderAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH5039Title), nameof(AnalyzerResources.RH5039MessageFormat))
    {
    }

    #endregion // Constructor

    #region CommentRegionAnalyzerBase

    /// <inheritdoc/>
    protected override bool TryGetRegion(SyntaxNode node, out SyntaxToken openToken, out SyntaxToken closeToken)
    {
        return CommentPositionUtilities.TryGetStatementHeader(node, out openToken, out closeToken, out _);
    }

    #endregion // CommentRegionAnalyzerBase
}