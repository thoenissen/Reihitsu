using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Clarity;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Clarity;

/// <summary>
/// Code fix provider for <see cref="RH3107UnnecessaryAttributeConstructorParenthesesShouldBeRemovedAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH3107UnnecessaryAttributeConstructorParenthesesShouldBeRemovedCodeFixProvider))]
public class RH3107UnnecessaryAttributeConstructorParenthesesShouldBeRemovedCodeFixProvider : EmptyParenthesesCodeFixProviderBase<AttributeSyntax>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH3107UnnecessaryAttributeConstructorParenthesesShouldBeRemovedCodeFixProvider()
        : base(RH3107UnnecessaryAttributeConstructorParenthesesShouldBeRemovedAnalyzer.DiagnosticId, CodeFixResources.RH3107Title)
    {
    }

    #endregion // Constructor

    #region EmptyParenthesesCodeFixProviderBase

    /// <inheritdoc/>
    private protected override bool KeepsSpaceInFrontOfFollowingToken => false;

    /// <inheritdoc/>
    private protected override SyntaxNode GetList(AttributeSyntax node)
    {
        return node.ArgumentList;
    }

    /// <inheritdoc/>
    private protected override AttributeSyntax RemoveList(AttributeSyntax node, SyntaxTriviaList separatingTrivia)
    {
        return node.WithArgumentList(null)
                   .WithName(node.Name.WithTrailingTrivia(separatingTrivia));
    }

    #endregion // EmptyParenthesesCodeFixProviderBase
}