using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.HorizontalSpacing.Rules;

/// <summary>
/// Puts one space between the <c>*</c> of a function-pointer type and the <c>managed</c> or <c>unmanaged</c> behind it
/// (FMT-SPACE-027)
/// </summary>
internal sealed class FunctionPointerCallingConventionSpacingRule : ISpacingRule
{
    #region ISpacingRule

    /// <inheritdoc/>
    public int? DesiredSpacesAfter(SyntaxToken left, SyntaxToken right)
    {
        if (left.IsKind(SyntaxKind.AsteriskToken)
            && left.Parent is FunctionPointerTypeSyntax { CallingConvention: { } callingConvention }
            && right == callingConvention.ManagedOrUnmanagedKeyword)
        {
            return 1;
        }

        return null;
    }

    #endregion // ISpacingRule
}