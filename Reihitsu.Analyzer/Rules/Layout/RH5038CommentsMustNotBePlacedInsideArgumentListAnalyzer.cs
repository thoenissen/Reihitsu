using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5038: Comments must not be placed inside an argument list
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5038CommentsMustNotBePlacedInsideArgumentListAnalyzer : CommentRegionAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5038";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5038CommentsMustNotBePlacedInsideArgumentListAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH5038Title), nameof(AnalyzerResources.RH5038MessageFormat))
    {
    }

    #endregion // Constructor

    #region CommentRegionAnalyzerBase

    /// <inheritdoc/>
    protected override bool TryGetRegion(SyntaxNode node, out SyntaxToken openToken, out SyntaxToken closeToken)
    {
        switch (node)
        {
            case ArgumentListSyntax argumentList:
                {
                    openToken = argumentList.OpenParenToken;
                    closeToken = argumentList.CloseParenToken;

                    return true;
                }
            case BracketedArgumentListSyntax bracketedArgumentList:
                {
                    openToken = bracketedArgumentList.OpenBracketToken;
                    closeToken = bracketedArgumentList.CloseBracketToken;

                    return true;
                }
            case AttributeArgumentListSyntax attributeArgumentList:
                {
                    openToken = attributeArgumentList.OpenParenToken;
                    closeToken = attributeArgumentList.CloseParenToken;

                    return true;
                }
            default:
                {
                    openToken = default;
                    closeToken = default;

                    return false;
                }
        }
    }

    #endregion // CommentRegionAnalyzerBase
}