namespace Reihitsu.Formatter.Pipeline.StructuralTransforms;

/// <summary>
/// Identifies the statement form an expression-bodied member's expression takes
/// once it is converted to a block body
/// </summary>
internal enum ExpressionBodyStatementForm
{
    /// <summary>
    /// Wrap the expression in a <see cref="Microsoft.CodeAnalysis.CSharp.Syntax.ReturnStatementSyntax"/>
    /// </summary>
    ReturnStatement,

    /// <summary>
    /// Wrap the expression in an <see cref="Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionStatementSyntax"/>
    /// </summary>
    ExpressionStatement
}