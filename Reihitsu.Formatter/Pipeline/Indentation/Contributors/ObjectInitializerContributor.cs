using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.Indentation.Contributors;

/// <summary>
/// Aligns object, array, <c>with</c>-expression, and <c>stackalloc</c> initializer braces to their
/// anchor keyword (or, when there is none, to the already-computed open brace column) and members
/// at +1 level
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
    /// Aligns the initializer braces and members relative to an anchor keyword such as <c>new</c>,
    /// <c>with</c>, or <c>stackalloc</c>
    /// </summary>
    /// <param name="anchorKeyword">The keyword token the initializer is anchored to</param>
    /// <param name="initializer">The initializer expression</param>
    /// <param name="model">The layout model</param>
    private static void AlignInitializer(SyntaxToken anchorKeyword, InitializerExpressionSyntax initializer, LayoutModel model)
    {
        var anchorColumn = LayoutComputer.GetAdjustedColumn(anchorKeyword, model);

        AlignInitializerToColumn(anchorColumn, initializer, model, alignOpenBrace: true);
    }

    /// <summary>
    /// Aligns the initializer's close brace and members to an already-computed anchor column,
    /// optionally aligning the open brace to the same column
    /// </summary>
    /// <param name="anchorColumn">The column to align the close brace and, optionally, the open brace to</param>
    /// <param name="initializer">The initializer expression</param>
    /// <param name="model">The layout model</param>
    /// <param name="alignOpenBrace"><see langword="true"/> to also align the open brace to <paramref name="anchorColumn"/>; otherwise, <see langword="false"/></param>
    private static void AlignInitializerToColumn(int anchorColumn, InitializerExpressionSyntax initializer, LayoutModel model, bool alignOpenBrace)
    {
        if (alignOpenBrace)
        {
            LayoutComputer.SetIfFirstOnLine(initializer.OpenBraceToken, anchorColumn, ObjectInitializerSource, model);
        }

        LayoutComputer.SetIfFirstOnLine(initializer.CloseBraceToken, anchorColumn, ObjectInitializerSource, model);

        var memberColumn = anchorColumn + FormattingContext.IndentSize;

        foreach (var expression in initializer.Expressions)
        {
            var firstToken = expression.GetFirstToken();

            LayoutComputer.SetIfFirstOnLine(firstToken, memberColumn, ObjectInitializerSource, model);
        }
    }

    #endregion // Methods

    #region ILayoutContributor

    /// <inheritdoc/>
    public void Contribute(SyntaxNode node, LayoutModel model, FormattingContext context)
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

            case WithExpressionSyntax withExpression:
                {
                    AlignInitializer(withExpression.WithKeyword, withExpression.Initializer, model);
                }
                break;

            case StackAllocArrayCreationExpressionSyntax { Initializer: not null } stackAlloc:
                {
                    AlignInitializer(stackAlloc.StackAllocKeyword, stackAlloc.Initializer, model);
                }
                break;

            case ImplicitStackAllocArrayCreationExpressionSyntax implicitStackAlloc:
                {
                    AlignInitializer(implicitStackAlloc.StackAllocKeyword, implicitStackAlloc.Initializer, model);
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

                    AlignInitializerToColumn(anchorColumn, initializer, model, alignOpenBrace: false);
                }
                break;

            case InitializerExpressionSyntax { Parent: EqualsValueClauseSyntax } initializer:
                {
                    var anchorColumn = LayoutComputer.GetAdjustedColumn(initializer.OpenBraceToken, model);

                    AlignInitializerToColumn(anchorColumn, initializer, model, alignOpenBrace: false);
                }
                break;
        }
    }

    #endregion // ILayoutContributor
}