using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Utilities;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms.Rewriter;

/// <summary>
/// Splits field declarations that declare multiple variables
/// </summary>
internal sealed class FieldDeclarationSplitTransform : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    /// Formatting context
    /// </summary>
    private readonly FormattingContext _context;

    /// <summary>
    /// Cancellation token
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">Formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public FieldDeclarationSplitTransform(FormattingContext context, CancellationToken cancellationToken)
    {
        _context = context;
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether a field declaration carries a preprocessor directive or disabled text
    /// anywhere in its trivia, which the split would otherwise drop
    /// </summary>
    /// <param name="fieldDeclaration">The field declaration to inspect</param>
    /// <returns><see langword="true"/> if the declaration carries a directive or disabled text; otherwise, <see langword="false"/></returns>
    internal static bool CarriesDirective(FieldDeclarationSyntax fieldDeclaration)
    {
        return fieldDeclaration.DescendantTrivia(descendIntoTrivia: true)
                               .Any(ReihitsuFormatterHelpers.IsDirectiveOrDisabledTextTrivia);
    }

    /// <summary>
    /// Determines whether the member list contains a field declaration that has to be split
    /// </summary>
    /// <param name="members">The member list</param>
    /// <returns><c>true</c> when at least one field declares multiple variables; otherwise <c>false</c></returns>
    private static bool HasFieldToSplit(SyntaxList<MemberDeclarationSyntax> members)
    {
        foreach (var member in members)
        {
            if (member is FieldDeclarationSyntax fieldDeclaration
                && fieldDeclaration.Declaration.Variables.Count > 1)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets the comment trivia contained in the provided trivia list, including documentation comments
    /// </summary>
    /// <param name="trivia">The trivia list</param>
    /// <returns>The comment trivia</returns>
    private static IEnumerable<SyntaxTrivia> GetComments(SyntaxTriviaList trivia)
    {
        return trivia.Where(ReihitsuFormatterHelpers.IsCommentTrivia);
    }

    /// <summary>
    /// Gets the contiguous whitespace trivia run that starts at the provided index
    /// </summary>
    /// <param name="trivia">The trivia list</param>
    /// <param name="startIndex">The index the run starts at</param>
    /// <returns>The whitespace trivia run, which is empty when no whitespace starts at the index</returns>
    private static SyntaxTriviaList GetWhitespaceRun(SyntaxTriviaList trivia, int startIndex)
    {
        var run = new List<SyntaxTrivia>();
        var triviaIndex = startIndex;

        while (triviaIndex < trivia.Count
               && trivia[triviaIndex].IsKind(SyntaxKind.WhitespaceTrivia))
        {
            run.Add(trivia[triviaIndex]);

            triviaIndex++;
        }

        return SyntaxFactory.TriviaList(run);
    }

    /// <summary>
    /// Determines whether the provided trivia ends the line it sits on
    /// </summary>
    /// <param name="trivia">The trivia to inspect</param>
    /// <returns><see langword="true"/> if the trivia ends its line; otherwise, <see langword="false"/></returns>
    private static bool EndsLine(SyntaxTrivia trivia)
    {
        // Deciding this on the trivia text rather than on its kind is what lets the line break a single-line
        // documentation comment carries inside its own structure count just like a plain end-of-line trivia.
        // BlankLineTriviaUtilities.EndsWithLineBreak answers a deliberately narrower question - it is scoped to
        // directive, disabled-text and documentation trivia and reports false for a plain end-of-line - so it cannot
        // serve the callers here, which have to treat both as ending the line.
        return trivia.ToFullString().EndsWith("\n", StringComparison.Ordinal);
    }

    /// <summary>
    /// Gets the indentation trivia for additional generated fields, which is the whitespace that begins the
    /// physical line the field declaration starts on
    /// </summary>
    /// <param name="leadingTrivia">The original leading trivia</param>
    /// <returns>The indentation trivia</returns>
    private static SyntaxTriviaList GetMemberIndentationTrivia(SyntaxTriviaList leadingTrivia)
    {
        // The whitespace that opens the field's line is the trailing whitespace run of the leading trivia, but only
        // when the trivia in front of that run ends the previous line. Locating it by scanning for the last
        // top-level end-of-line trivia is wrong twice over: a single-line documentation comment terminates its line
        // inside its own structure, so no top-level end-of-line follows it and the scan restarts at the beginning,
        // summing the whitespace of every preceding line; and an inline comment leaves a second whitespace run on
        // the same line, which the scan appends to the first (issue #592).
        var runStart = leadingTrivia.Count;

        while (runStart > 0
               && leadingTrivia[runStart - 1].IsKind(SyntaxKind.WhitespaceTrivia))
        {
            runStart--;
        }

        if (runStart > 0
            && EndsLine(leadingTrivia[runStart - 1]) == false)
        {
            // The declaration shares its line with preceding trivia, so the run found above is an inner gap rather
            // than the line's indentation. The whitespace that opens the trivia list opens the line instead.
            return GetWhitespaceRun(leadingTrivia, 0);
        }

        // When the field is the first member of its type its leading trivia carries no line break at all (the
        // newline sits on the opening brace), which leaves the run starting at index 0 — still the right answer.
        return GetWhitespaceRun(leadingTrivia, runStart);
    }

    /// <summary>
    /// Builds the trailing trivia for a split field, re-attaching comments that followed the declarator's separator
    /// </summary>
    /// <param name="comments">The comments to re-attach</param>
    /// <param name="suffixTrivia">The trivia appended after the comments</param>
    /// <returns>The trailing trivia</returns>
    private static SyntaxTriviaList BuildTrailingTrivia(List<SyntaxTrivia> comments, SyntaxTriviaList suffixTrivia)
    {
        if (comments.Count == 0)
        {
            return suffixTrivia;
        }

        var trivia = new List<SyntaxTrivia>((comments.Count * 2) + suffixTrivia.Count);

        foreach (var comment in comments)
        {
            trivia.Add(SyntaxFactory.Space);
            trivia.Add(comment);
        }

        trivia.AddRange(suffixTrivia);

        return SyntaxFactory.TriviaList(trivia);
    }

    /// <summary>
    /// Builds the leading trivia for a split field, re-attaching standalone comments that preceded the declarator
    /// </summary>
    /// <param name="indentationTrivia">The indentation trivia for the generated field</param>
    /// <param name="declaratorLeadingTrivia">The leading trivia of the declarator</param>
    /// <returns>The leading trivia</returns>
    private SyntaxTriviaList BuildLeadingTrivia(SyntaxTriviaList indentationTrivia, SyntaxTriviaList declaratorLeadingTrivia)
    {
        var trivia = new List<SyntaxTrivia>();

        foreach (var comment in GetComments(declaratorLeadingTrivia))
        {
            trivia.AddRange(indentationTrivia);
            trivia.Add(comment);

            // A single-line documentation comment already terminates its line, so appending another end-of-line would
            // put a blank line between the comment and the field it documents. The pipeline's blank-line phase
            // absorbs that break, but the RH7101 code fix runs this transform on its own and would emit the detached
            // comment verbatim. A delimited documentation comment (/** … */) carries no break and still needs one,
            // which is why the question is asked of the trivia text rather than of its kind (issue #592).
            if (EndsLine(comment) == false)
            {
                trivia.Add(SyntaxFactory.EndOfLine(_context.EndOfLine));
            }
        }

        trivia.AddRange(indentationTrivia);

        return SyntaxFactory.TriviaList(trivia);
    }

    /// <summary>
    /// Splits field declarations in the provided member list
    /// </summary>
    /// <param name="members">The member list</param>
    /// <returns>The updated members</returns>
    private SyntaxList<MemberDeclarationSyntax> SplitFields(SyntaxList<MemberDeclarationSyntax> members)
    {
        if (HasFieldToSplit(members) == false)
        {
            return members;
        }

        var updatedMembers = new List<MemberDeclarationSyntax>(members.Count);
        var lineBreakTrivia = SyntaxFactory.TriviaList(SyntaxFactory.EndOfLine(_context.EndOfLine));

        foreach (var member in members)
        {
            _cancellationToken.ThrowIfCancellationRequested();

            if (member is not FieldDeclarationSyntax fieldDeclaration
                || fieldDeclaration.Declaration.Variables.Count <= 1)
            {
                updatedMembers.Add(member);

                continue;
            }

            // Splitting rebuilds each generated field's trivia from comments only. A preprocessor
            // directive or disabled text entangled with the declarators or separators would be dropped,
            // so leave a directive-bearing field declaration intact rather than losing the directive.
            if (CarriesDirective(fieldDeclaration))
            {
                updatedMembers.Add(member);

                continue;
            }

            var indentationTrivia = GetMemberIndentationTrivia(fieldDeclaration.GetLeadingTrivia());
            var variables = fieldDeclaration.Declaration.Variables;

            for (var variableIndex = 0; variableIndex < variables.Count; variableIndex++)
            {
                var variable = variables[variableIndex];

                // Only the declarator's leading trivia is dropped, because the generated field rebuilds it below.
                // Its trailing trivia is what the author wrote between the declarator and its terminator, so it
                // stays on the declarator - carrying it over to the semicolon instead would move an ordinary
                // comment into a leading position the blank-line phase separates onto its own line (issue #625).
                var updatedField = fieldDeclaration.WithDeclaration(fieldDeclaration.Declaration.WithVariables(SyntaxFactory.SingletonSeparatedList(variable.WithoutTrivia()
                                                                                                                                                            .WithTrailingTrivia(variable.GetTrailingTrivia()))));

                if (variableIndex == 0)
                {
                    updatedField = updatedField.WithLeadingTrivia(fieldDeclaration.GetLeadingTrivia());
                }
                else
                {
                    updatedField = updatedField.WithLeadingTrivia(BuildLeadingTrivia(indentationTrivia, variable.GetLeadingTrivia()));
                }

                // The terminator the author wrote for this declarator becomes the generated field's semicolon: the
                // separator for every declarator but the last, and the declaration's own semicolon for the last.
                // Everything written between the declarator and that terminator therefore stays in front of the
                // generated semicolon, and everything written after it stays behind it. Reusing the declaration's
                // semicolon token through WithDeclaration above would otherwise copy its leading trivia onto every
                // generated field, and the separator's leading trivia would be discarded with the separator
                // (issues #624, #625).
                var terminator = variableIndex == variables.Count - 1
                                     ? fieldDeclaration.SemicolonToken
                                     : variables.GetSeparator(variableIndex);

                if (variableIndex == variables.Count - 1)
                {
                    updatedField = updatedField.WithTrailingTrivia(fieldDeclaration.GetTrailingTrivia());
                }
                else
                {
                    var trailingComments = GetComments(terminator.TrailingTrivia).ToList();

                    updatedField = updatedField.WithTrailingTrivia(BuildTrailingTrivia(trailingComments, lineBreakTrivia));
                }

                updatedField = updatedField.WithSemicolonToken(updatedField.SemicolonToken.WithLeadingTrivia(terminator.LeadingTrivia));

                updatedMembers.Add(updatedField);
            }
        }

        return SyntaxFactory.List(updatedMembers);
    }

    #endregion // Methods

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (ClassDeclarationSyntax)base.VisitClassDeclaration(node);

        if (node == null)
        {
            return null;
        }

        return node.WithMembers(SplitFields(node.Members));
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitStructDeclaration(StructDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (StructDeclarationSyntax)base.VisitStructDeclaration(node);

        if (node == null)
        {
            return null;
        }

        return node.WithMembers(SplitFields(node.Members));
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitRecordDeclaration(RecordDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (RecordDeclarationSyntax)base.VisitRecordDeclaration(node);

        if (node == null)
        {
            return null;
        }

        return node.WithMembers(SplitFields(node.Members));
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (InterfaceDeclarationSyntax)base.VisitInterfaceDeclaration(node);

        if (node == null)
        {
            return null;
        }

        return node.WithMembers(SplitFields(node.Members));
    }

    #endregion // CSharpSyntaxVisitor
}