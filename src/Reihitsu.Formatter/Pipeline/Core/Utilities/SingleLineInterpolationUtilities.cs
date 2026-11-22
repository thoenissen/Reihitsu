using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;

namespace Reihitsu.Formatter.Pipeline.Core.Utilities;

/// <summary>
/// Runs the rewriters of a phase that writes line breaks so that every interpolation which cannot hold a line break in the
/// target language version stays as written: an interpolation of a non-verbatim interpolated string before C# 11 (CS8967)
/// </summary>
internal static class SingleLineInterpolationUtilities
{
    #region Constants

    /// <summary>
    /// Annotation kind that marks an interpolation that stays as written
    /// </summary>
    private const string AnnotationKind = "Reihitsu.SingleLineInterpolation";

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Runs the rewriters in order. Before C# 11, every outermost interpolation of a non-verbatim interpolated string is
    /// put back as it stood in the input after each rewriter, so no rewriter writes a line break into it and no later
    /// rewriter sees one there
    /// </summary>
    /// <param name="root">The root node</param>
    /// <param name="languageVersion">The language version the source targets</param>
    /// <param name="rewriters">The rewriters of the phase, in order</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The rewritten root</returns>
    public static SyntaxNode RunKeepingSingleLineInterpolations(SyntaxNode root,
                                                                LanguageVersion languageVersion,
                                                                IEnumerable<CSharpSyntaxRewriter> rewriters,
                                                                CancellationToken cancellationToken)
    {
        var current = Annotate(root, languageVersion);
        var originals = current.GetAnnotatedNodes(AnnotationKind)
                               .ToDictionary(GetAnnotation);

        foreach (var rewriter in rewriters)
        {
            current = rewriter.Visit(current);

            if (originals.Count > 0)
            {
                current = current.ReplaceNodes(current.GetAnnotatedNodes(AnnotationKind), (rewritten, _) => originals[GetAnnotation(rewritten)]);
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        if (originals.Count > 0)
        {
            current = current.ReplaceNodes(current.GetAnnotatedNodes(AnnotationKind), (_, interpolation) => interpolation.WithoutAnnotations(AnnotationKind));
        }

        return current;
    }

    /// <summary>
    /// Annotates the outermost interpolations that cannot hold a line break in the target language version: each
    /// interpolation of a non-verbatim interpolated string that stands in no other such interpolation. Each one gets an
    /// annotation of its own
    /// </summary>
    /// <param name="root">The root node</param>
    /// <param name="languageVersion">The language version the source targets</param>
    /// <returns>The root with the interpolations annotated</returns>
    private static SyntaxNode Annotate(SyntaxNode root, LanguageVersion languageVersion)
    {
        var interpolations = root.DescendantNodes()
                                 .OfType<InterpolationSyntax>()
                                 .Where(interpolation => StringInterpolationUtilities.IsInsideSingleLineInterpolation(interpolation, languageVersion)
                                                         && StringInterpolationUtilities.IsInsideSingleLineInterpolation(interpolation.Parent, languageVersion) == false)
                                 .ToList();

        return interpolations.Count == 0
                   ? root
                   : root.ReplaceNodes(interpolations, (_, interpolation) => interpolation.WithAdditionalAnnotations(new SyntaxAnnotation(AnnotationKind)));
    }

    /// <summary>
    /// Gets the annotation that identifies an annotated interpolation
    /// </summary>
    /// <param name="interpolation">The annotated interpolation</param>
    /// <returns>The interpolation's annotation</returns>
    private static SyntaxAnnotation GetAnnotation(SyntaxNode interpolation)
    {
        return interpolation.GetAnnotations(AnnotationKind)
                            .First();
    }

    #endregion // Methods
}