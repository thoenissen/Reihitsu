using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5046: Comments must not be placed inside a type parameter, type argument or tuple list
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5046CommentsMustNotBePlacedInsideTypeParameterTypeArgumentOrTupleListAnalyzer : CommentRegionAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5046";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5046CommentsMustNotBePlacedInsideTypeParameterTypeArgumentOrTupleListAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH5046Title), nameof(AnalyzerResources.RH5046MessageFormat))
    {
    }

    #endregion // Constructor

    #region CommentRegionAnalyzerBase

    /// <inheritdoc/>
    protected override bool TryGetRegion(SyntaxNode node, out SyntaxToken openToken, out SyntaxToken closeToken)
    {
        switch (node)
        {
            case TypeParameterListSyntax typeParameterList:
                {
                    openToken = typeParameterList.LessThanToken;
                    closeToken = typeParameterList.GreaterThanToken;

                    return true;
                }
            case TypeArgumentListSyntax typeArgumentList:
                {
                    openToken = typeArgumentList.LessThanToken;
                    closeToken = typeArgumentList.GreaterThanToken;

                    return true;
                }
            case TupleTypeSyntax tupleType:
                {
                    openToken = tupleType.OpenParenToken;
                    closeToken = tupleType.CloseParenToken;

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