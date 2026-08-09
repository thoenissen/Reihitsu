using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CodeFixes;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Compatibility name for <see cref="RH5020CommentsShouldBePrecededByABlankLineCodeFixProvider"/>
/// </summary>
[Obsolete("Use RH5020CommentsShouldBePrecededByABlankLineCodeFixProvider instead.")]
public class RH5020SingleLineCommentsShouldBePrecededByABlankLineCodeFixProvider : CodeFixProvider
{
    #region Fields

    /// <summary>
    /// Code-fix implementation
    /// </summary>
    private readonly RH5020CommentsShouldBePrecededByABlankLineCodeFixProvider _implementation = new();

    #endregion // Fields

    #region CodeFixProvider

    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => _implementation.FixableDiagnosticIds;

    /// <inheritdoc/>
    public override FixAllProvider GetFixAllProvider()
    {
        return _implementation.GetFixAllProvider();
    }

    /// <inheritdoc/>
    public override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        return _implementation.RegisterCodeFixesAsync(context);
    }

    #endregion // CodeFixProvider
}