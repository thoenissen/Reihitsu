using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.Indentation.Contributors;

/// <summary>
/// Aligns object initializer braces to the <c>new</c> keyword column and members at +1 level
/// </summary>
internal sealed class ObjectInitializerContributor : ILayoutContributor
{
    #region Constants

    /// <summary>
    /// The source identifier for layout entries contributed by this class
    /// </summary>
    private const string ObjectInitializerSource = "ObjectInitializer";

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Aligns the initializer braces and members relative to the <c>new</c> keyword
    /// </summary>
    /// <param name="newKeyword">The <c>new</c> keyword token</param>
    /// <param name="initializer">The initializer expression</param>
    /// <param name="model">The layout model</param>
    private static void AlignInitializer(SyntaxToken newKeyword, InitializerExpressionSyntax initializer, LayoutModel model)
    {
        var newColumn = LayoutComputer.GetAdjustedColumn(newKeyword, model);

        LayoutComputer.SetIfFirstOnLine(initializer.OpenBraceToken, newColumn, ObjectInitializerSource, model);
        LayoutComputer.SetIfFirstOnLine(initializer.CloseBraceToken, newColumn, ObjectInitializerSource, model);

        var memberColumn = newColumn + FormattingContext.IndentSize;

        foreach (var expression in initializer.Expressions)
        {
            var firstToken = expression.GetFirstToken();

            LayoutComputer.SetIfFirstOnLine(firstToken, memberColumn, ObjectInitializerSource, model);
        }
    }

    #endregion // Methods

    #region ILayoutContributor

    /// <inheritdoc/>
    public void Contribute(SyntaxNode node, FormattingScope scope, LayoutModel model, FormattingContext context)
    {
        switch (node)
        {
            case ObjectCreationExpressionSyntax { Initializer: not null } creation:
                {
                    AlignInitializer(creation.NewKeyword, creation.Initializer, model);
                }
                break;

            case ArrayCreationExpressionSyntax { Initializer: not null } array:
                {
                    AlignInitializer(array.NewKeyword, array.Initializer, model);
                }
                break;

            case ImplicitArrayCreationExpressionSyntax implicitArray:
                {
                    AlignInitializer(implicitArray.NewKeyword, implicitArray.Initializer, model);
                }
                break;

            case ImplicitObjectCreationExpressionSyntax { Initializer: not null } implicitObj:
                {
                    AlignInitializer(implicitObj.NewKeyword, implicitObj.Initializer, model);
                }
                break;

            case InitializerExpressionSyntax { Parent: AssignmentExpressionSyntax assignment } initializer:
                {
                    int anchorColumn;

                    if (LayoutComputer.IsFirstOnLine(initializer.OpenBraceToken))
                    {
                        anchorColumn = LayoutComputer.GetAdjustedColumn(assignment.Left.GetFirstToken(), model);

                        LayoutComputer.SetIfFirstOnLine(initializer.OpenBraceToken, anchorColumn, ObjectInitializerSource, model);
                    }
                    else
                    {
                        anchorColumn = LayoutComputer.GetAdjustedColumn(initializer.OpenBraceToken, model);
                    }

                    LayoutComputer.SetIfFirstOnLine(initializer.CloseBraceToken, anchorColumn, ObjectInitializerSource, model);

                    var memberColumn = anchorColumn + FormattingContext.IndentSize;

                    foreach (var expression in initializer.Expressions)
                    {
                        var firstToken = expression.GetFirstToken();

                        LayoutComputer.SetIfFirstOnLine(firstToken, memberColumn, ObjectInitializerSource, model);
                    }
                }
                break;
        }
    }

    #endregion // ILayoutContributor
}