using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5035: Comments must not be placed inside a parameter list
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5035CommentsMustNotBePlacedInsideParameterListAnalyzer : CommentRegionAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5035";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5035CommentsMustNotBePlacedInsideParameterListAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH5035Title), nameof(AnalyzerResources.RH5035MessageFormat))
    {
    }

    #endregion // Constructor

    #region CommentRegionAnalyzerBase

    /// <inheritdoc/>
    protected override bool TryGetRegion(SyntaxNode node, out SyntaxToken openToken, out SyntaxToken closeToken)
    {
        switch (node)
        {
            case ParameterListSyntax parameterList:
                {
                    openToken = parameterList.OpenParenToken;
                    closeToken = parameterList.CloseParenToken;

                    return true;
                }
            case BracketedParameterListSyntax bracketedParameterList:
                {
                    openToken = bracketedParameterList.OpenBracketToken;
                    closeToken = bracketedParameterList.CloseBracketToken;

                    return true;
                }
            case FunctionPointerParameterListSyntax functionPointerParameterList:
                {
                    openToken = functionPointerParameterList.LessThanToken;
                    closeToken = functionPointerParameterList.GreaterThanToken;

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

    /// <inheritdoc/>
    protected override bool IsMisplacedBeforeRegion(SyntaxToken previousToken, SyntaxToken nextToken)
    {
        var parameterList = nextToken.Parent switch
                            {
                                ParameterListSyntax list when list.OpenParenToken == nextToken => list,
                                BracketedParameterListSyntax list when list.OpenBracketToken == nextToken => list,
                                FunctionPointerParameterListSyntax list when list.LessThanToken == nextToken => list,
                                _ => (SyntaxNode)null
                            };

        return parameterList?.Parent != null
               && previousToken.SpanStart >= parameterList.Parent.SpanStart;
    }

    #endregion // CommentRegionAnalyzerBase
}