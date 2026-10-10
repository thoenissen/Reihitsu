using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5047: Comments must not be placed inside an auto-property accessor list
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5047CommentsMustNotBePlacedInsideAutoPropertyAccessorListAnalyzer : CommentRegionAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5047";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5047CommentsMustNotBePlacedInsideAutoPropertyAccessorListAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH5047Title), nameof(AnalyzerResources.RH5047MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the accessor list belongs to a property or an indexer whose accessors have no body. The
    /// accessor test is the same one the formatter's <c>LineBreakDetection.IsAutoPropertyAccessorList</c> applies
    /// before it lays out an accessor list, and must stay in step with it
    /// </summary>
    /// <param name="accessorList">Accessor list</param>
    /// <returns><see langword="true"/> if the accessor list is an auto-property accessor list</returns>
    private static bool IsAutoPropertyAccessorList(AccessorListSyntax accessorList)
    {
        return accessorList.Parent is PropertyDeclarationSyntax or IndexerDeclarationSyntax
               && accessorList.Accessors.All(accessor => accessor.Body == null && accessor.ExpressionBody == null);
    }

    #endregion // Methods

    #region CommentRegionAnalyzerBase

    /// <inheritdoc/>
    protected override bool TryGetRegion(SyntaxNode node, out SyntaxToken openToken, out SyntaxToken closeToken)
    {
        if (node is AccessorListSyntax accessorList
            && IsAutoPropertyAccessorList(accessorList))
        {
            openToken = accessorList.OpenBraceToken;
            closeToken = accessorList.CloseBraceToken;

            return true;
        }

        openToken = default;
        closeToken = default;

        return false;
    }

    /// <inheritdoc/>
    protected override bool IsMisplacedBeforeRegion(SyntaxToken previousToken, SyntaxToken nextToken)
    {
        return nextToken.Parent is AccessorListSyntax accessorList
               && accessorList.OpenBraceToken == nextToken
               && IsAutoPropertyAccessorList(accessorList);
    }

    #endregion // CommentRegionAnalyzerBase
}