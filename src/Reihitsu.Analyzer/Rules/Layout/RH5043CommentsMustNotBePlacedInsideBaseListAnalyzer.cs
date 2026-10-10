using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5043: Comments must not be placed inside a base list
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5043CommentsMustNotBePlacedInsideBaseListAnalyzer : CommentRegionAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5043";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5043CommentsMustNotBePlacedInsideBaseListAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH5043Title), nameof(AnalyzerResources.RH5043MessageFormat))
    {
    }

    #endregion // Constructor

    #region CommentRegionAnalyzerBase

    /// <inheritdoc/>
    protected override bool TryGetRegion(SyntaxNode node, out SyntaxToken openToken, out SyntaxToken closeToken)
    {
        if (node is BaseListSyntax baseList)
        {
            openToken = baseList.ColonToken;
            closeToken = baseList.GetLastToken();

            return true;
        }

        openToken = default;
        closeToken = default;

        return false;
    }

    #endregion // CommentRegionAnalyzerBase
}