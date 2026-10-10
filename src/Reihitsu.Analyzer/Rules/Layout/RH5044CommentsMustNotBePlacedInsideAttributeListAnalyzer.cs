using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5044: Comments must not be placed inside an attribute list
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5044CommentsMustNotBePlacedInsideAttributeListAnalyzer : CommentRegionAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5044";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5044CommentsMustNotBePlacedInsideAttributeListAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH5044Title), nameof(AnalyzerResources.RH5044MessageFormat))
    {
    }

    #endregion // Constructor

    #region CommentRegionAnalyzerBase

    /// <inheritdoc/>
    protected override bool TryGetRegion(SyntaxNode node, out SyntaxToken openToken, out SyntaxToken closeToken)
    {
        if (node is AttributeListSyntax attributeList)
        {
            openToken = attributeList.OpenBracketToken;
            closeToken = attributeList.CloseBracketToken;

            return true;
        }

        openToken = default;
        closeToken = default;

        return false;
    }

    #endregion // CommentRegionAnalyzerBase
}