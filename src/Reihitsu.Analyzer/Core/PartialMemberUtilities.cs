using System.IO;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Analyzer.Core;

/// <summary>
/// The one owner of where the single diagnostic of a partial member stands (SHARED-DIAG-012): of a method, property,
/// indexer, event, or constructor declared in a defining part and an implementing part, the part that carries it
/// </summary>
internal static class PartialMemberUtilities
{
    #region Methods

    /// <summary>
    /// Tries to get the two parts of a partial member from the symbol of either part
    /// </summary>
    /// <param name="symbol">Symbol of the defining or the implementing part</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="definingPart">The part without an implementation</param>
    /// <param name="implementingPart">The part with the implementation</param>
    /// <returns><see langword="true"/> when the symbol is a member declared in both parts</returns>
    internal static bool TryGetParts(ISymbol symbol, CancellationToken cancellationToken, out MemberDeclarationSyntax definingPart, out MemberDeclarationSyntax implementingPart)
    {
        var (definition, implementation) = symbol switch
                                           {
                                               IMethodSymbol method => GetPartSymbols(method, method.PartialDefinitionPart, method.PartialImplementationPart),
                                               IPropertySymbol property => GetPartSymbols(property, property.PartialDefinitionPart, property.PartialImplementationPart),
                                               IEventSymbol eventSymbol => GetPartSymbols(eventSymbol, eventSymbol.PartialDefinitionPart, eventSymbol.PartialImplementationPart),
                                               _ => (null, null)
                                           };

        definingPart = GetDeclaration(definition, cancellationToken);
        implementingPart = GetDeclaration(implementation, cancellationToken);

        return definingPart != null
               && implementingPart != null;
    }

    /// <summary>
    /// Gets the symbol a member declaration declares: for an event field, the event its first variable declares
    /// </summary>
    /// <param name="memberDeclaration">Member declaration</param>
    /// <param name="semanticModel">Semantic model of the declaration's tree</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The symbol, or <see langword="null"/></returns>
    internal static ISymbol GetDeclaredSymbol(MemberDeclarationSyntax memberDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        return memberDeclaration is EventFieldDeclarationSyntax { Declaration.Variables.Count: > 0 } eventFieldDeclaration
                   ? semanticModel.GetDeclaredSymbol(eventFieldDeclaration.Declaration.Variables[0], cancellationToken)
                   : semanticModel.GetDeclaredSymbol(memberDeclaration, cancellationToken);
    }

    /// <summary>
    /// Gets the part on which the analysis of a declaration reports the diagnostic of the member it declares: the
    /// declaration itself when it is not a part of a partial member, and for a part of a partial member the part
    /// <see cref="SelectReportedPart"/> chooses, from the analysis of exactly one of the parts
    /// </summary>
    /// <param name="declaration">Declaration the rule analyzes and would report</param>
    /// <param name="symbol">Symbol the declaration declares</param>
    /// <param name="qualifies">Whether the rule would report a part if it judged that part alone, by what that part writes</param>
    /// <param name="compilation">Compilation containing the declaration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// The part that carries the diagnostic, <paramref name="declaration"/> itself when that is the part, or
    /// <see langword="null"/> when the analysis of this declaration reports nothing
    /// </returns>
    /// <remarks>
    /// Each part is analyzed on its own, so the analysis of a part reports only when that part is the chosen one. The
    /// analyzer driver does not analyze the defining part of a partial constructor, so for a constructor the analysis of
    /// the implementing part reports on the chosen part, whichever it is
    /// </remarks>
    internal static MemberDeclarationSyntax GetReportedPart(MemberDeclarationSyntax declaration,
                                                            ISymbol symbol,
                                                            Func<MemberDeclarationSyntax, bool> qualifies,
                                                            Compilation compilation,
                                                            CancellationToken cancellationToken)
    {
        if (symbol == null
            || TryGetParts(symbol, cancellationToken, out var definingPart, out var implementingPart) == false)
        {
            return declaration;
        }

        var reportedPart = SelectReportedPart(definingPart, implementingPart, qualifies, compilation, cancellationToken);
        var analyzingPart = declaration is ConstructorDeclarationSyntax
                                ? implementingPart
                                : reportedPart;

        if (IsSameDeclaration(analyzingPart, declaration) == false)
        {
            return null;
        }

        return IsSameDeclaration(reportedPart, declaration)
                   ? declaration
                   : reportedPart;
    }

    /// <summary>
    /// Selects the part of a partial member that carries the member's one diagnostic. A part in generated code is never
    /// chosen; of the other parts, the first that qualifies, the defining part before the implementing part; when none
    /// qualifies, the defining part, or the implementing part when the defining part is in generated code. The order
    /// of the parts in a file and the files that hold them do not change the choice
    /// </summary>
    /// <param name="definingPart">The part without an implementation</param>
    /// <param name="implementingPart">The part with the implementation</param>
    /// <param name="qualifies">Whether the rule would report a part if it judged that part alone, by what that part writes</param>
    /// <param name="compilation">Compilation containing the parts</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The part, or <see langword="null"/> when every part is in generated code</returns>
    internal static MemberDeclarationSyntax SelectReportedPart(MemberDeclarationSyntax definingPart,
                                                               MemberDeclarationSyntax implementingPart,
                                                               Func<MemberDeclarationSyntax, bool> qualifies,
                                                               Compilation compilation,
                                                               CancellationToken cancellationToken)
    {
        var candidates = new[] { definingPart, implementingPart }.Where(part => IsGeneratedCode(part.SyntaxTree, compilation, cancellationToken) == false)
                                                                 .ToList();

        return candidates.FirstOrDefault(qualifies) ?? candidates.FirstOrDefault();
    }

    /// <summary>
    /// Gets the symbols of the defining and the implementing part of a member
    /// </summary>
    /// <param name="symbol">Symbol of either part</param>
    /// <param name="definitionPart">The symbol's defining part, when the symbol is the implementing part</param>
    /// <param name="implementationPart">The symbol's implementing part, when the symbol is the defining part</param>
    /// <returns>The two symbols, or <see langword="null"/> for a part the member does not have</returns>
    private static (ISymbol Definition, ISymbol Implementation) GetPartSymbols(ISymbol symbol, ISymbol definitionPart, ISymbol implementationPart)
    {
        if (implementationPart != null)
        {
            return (symbol, implementationPart);
        }

        return definitionPart != null
                   ? (definitionPart, symbol)
                   : (null, null);
    }

    /// <summary>
    /// Gets the member declaration of a part's symbol
    /// </summary>
    /// <param name="symbol">Symbol of the part</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The declaration, or <see langword="null"/> when the symbol has none in source</returns>
    private static MemberDeclarationSyntax GetDeclaration(ISymbol symbol, CancellationToken cancellationToken)
    {
        return symbol?.DeclaringSyntaxReferences.Length == 1
                   ? symbol.DeclaringSyntaxReferences[0].GetSyntax(cancellationToken).FirstAncestorOrSelf<MemberDeclarationSyntax>()
                   : null;
    }

    /// <summary>
    /// Determines whether two nodes are the same declaration
    /// </summary>
    /// <param name="left">First declaration</param>
    /// <param name="right">Second declaration</param>
    /// <returns><see langword="true"/> when both stand at the same place of the same tree</returns>
    private static bool IsSameDeclaration(SyntaxNode left, SyntaxNode right)
    {
        return left != null
               && right != null
               && left.SyntaxTree == right.SyntaxTree
               && left.Span == right.Span;
    }

    /// <summary>
    /// Determines whether a syntax tree is generated code in the sense the compiler gives it (SHARED-DIAG-001): marked
    /// so by the compilation's options, such as <c>generated_code</c> in an analyzer configuration file, or, when the
    /// options do not decide, named as a generated file or starting with an <c>&lt;auto-generated&gt;</c> comment
    /// </summary>
    /// <param name="syntaxTree">Syntax tree</param>
    /// <param name="compilation">Compilation containing the tree</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> when the tree is generated code</returns>
    /// <remarks>
    /// The analyzer driver skips generated code and suppresses every diagnostic in it, but it tells an action only
    /// whether the tree the action analyzes is generated. Choosing the part of a partial member needs the answer for the
    /// other part's tree too, so the compiler's rule is read here: the options first, then the file name and the header
    /// </remarks>
    private static bool IsGeneratedCode(SyntaxTree syntaxTree, Compilation compilation, CancellationToken cancellationToken)
    {
        switch (compilation?.Options.SyntaxTreeOptionsProvider?.IsGenerated(syntaxTree, cancellationToken) ?? GeneratedKind.Unknown)
        {
            case GeneratedKind.NotGenerated:
                {
                    return false;
                }

            case GeneratedKind.MarkedGenerated:
                {
                    return true;
                }

            default:
                {
                    return IsGeneratedFileName(syntaxTree.FilePath)
                           || BeginsWithAutoGeneratedComment(syntaxTree, cancellationToken);
                }
        }
    }

    /// <summary>
    /// Determines whether a file name marks the file as generated, as the compiler reads it: a name that starts with
    /// <c>TemporaryGeneratedFile_</c>, or whose name without its extension ends with <c>.designer</c>,
    /// <c>.generated</c>, <c>.g</c>, or <c>.g.i</c>
    /// </summary>
    /// <param name="filePath">File path</param>
    /// <returns><see langword="true"/> when the name marks the file as generated</returns>
    private static bool IsGeneratedFileName(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return false;
        }

        var fileName = Path.GetFileName(filePath);

        if (fileName.StartsWith("TemporaryGeneratedFile_", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.IsNullOrEmpty(Path.GetExtension(fileName)))
        {
            return false;
        }

        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

        return fileNameWithoutExtension.EndsWith(".designer", StringComparison.OrdinalIgnoreCase)
               || fileNameWithoutExtension.EndsWith(".generated", StringComparison.OrdinalIgnoreCase)
               || fileNameWithoutExtension.EndsWith(".g", StringComparison.OrdinalIgnoreCase)
               || fileNameWithoutExtension.EndsWith(".g.i", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines whether a syntax tree starts with a comment that marks it as generated
    /// </summary>
    /// <param name="syntaxTree">Syntax tree</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> when a comment in front of the first token contains <c>&lt;autogenerated</c> or <c>&lt;auto-generated</c></returns>
    private static bool BeginsWithAutoGeneratedComment(SyntaxTree syntaxTree, CancellationToken cancellationToken)
    {
        var root = syntaxTree.GetRoot(cancellationToken);

        return root.HasLeadingTrivia
               && root.GetLeadingTrivia()
                      .Where(static trivia => trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)
                                              || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
                      .Select(static trivia => trivia.ToString())
                      .Any(static text => text.IndexOf("<autogenerated", StringComparison.Ordinal) >= 0
                                          || text.IndexOf("<auto-generated", StringComparison.Ordinal) >= 0);
    }

    #endregion // Methods
}