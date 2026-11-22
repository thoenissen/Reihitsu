using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Core;
using Reihitsu.Formatter.Data.Enumerations;
using Reihitsu.Formatter.Utilities;

namespace Reihitsu.Formatter.Data;

/// <summary>
/// Carries state through the formatting pipeline.
/// Immutable — a single shared instance is passed to every phase
/// </summary>
internal record FormattingContext
{
    #region Constants

    /// <summary>
    /// Indentation unit: 4 spaces (non-configurable)
    /// </summary>
    public const int IndentSize = SyntaxIndentationUtilities.IndentSize;

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="endOfLine">End-of-line sequence</param>
    /// <param name="baseIndentLevel">The base indentation level for isolated node formatting</param>
    /// <param name="preserveRootDocumentationBoundary">Whether node-scoped formatting should preserve one line break before root documentation</param>
    /// <param name="disabledStructuralTransforms">The configurable structural transforms this run must skip</param>
    /// <param name="languageVersion">
    /// The C# language version the source targets. It is resolved through <see cref="LanguageVersionResolver.Resolve(Microsoft.CodeAnalysis.CSharp.LanguageVersion)"/>,
    /// so <see cref="LanguageVersion.Default"/> stands for <see cref="LanguageVersionResolver.MaxSupportedLanguageVersion"/>
    /// </param>
    /// <param name="rootFirstTokenColumn">
    /// The column of the formatting root's first token in its document, or <see langword="null"/> when the root is laid out at its depth alone
    /// </param>
    /// <param name="rootChainBoundary">
    /// The fact about the code above the formatting root that the member-access chain rule reads, or <see langword="default"/> when the caller supplies none
    /// </param>
    /// <param name="rootConstruct">
    /// The facts about the construct the formatting root stands in in its document, or <see cref="RootConstruct.None"/> when the caller supplies none
    /// </param>
    /// <param name="formatsRegionDirectives">
    /// Whether the region phase applies the region requirements; the node-level entry points pass <see langword="false"/> (FMT-PIPE-028)
    /// </param>
    /// <param name="targetAnnotation">
    /// The annotation that marks the first token of the target inside the formatting root, or <see langword="null"/> when the root has no such target
    /// </param>
    /// <param name="targetFirstTokenColumn">
    /// The column of the target's first token in its document, or <see langword="null"/> when the root has no such target
    /// </param>
    /// <param name="rootLineStartColumn">
    /// The column, in its document, of the first token or comment on the line on which the formatting root's first token
    /// stands, or <see langword="null"/> when the caller supplies none
    /// </param>
    public FormattingContext(string endOfLine,
                             int baseIndentLevel = 0,
                             bool preserveRootDocumentationBoundary = false,
                             ConfigurableStructuralTransforms disabledStructuralTransforms = ConfigurableStructuralTransforms.None,
                             LanguageVersion languageVersion = LanguageVersion.Default,
                             int? rootFirstTokenColumn = null,
                             FluentChainBoundary rootChainBoundary = default,
                             RootConstruct rootConstruct = RootConstruct.None,
                             bool formatsRegionDirectives = true,
                             SyntaxAnnotation targetAnnotation = null,
                             int? targetFirstTokenColumn = null,
                             int? rootLineStartColumn = null)
    {
        EndOfLine = endOfLine;
        BaseIndentLevel = baseIndentLevel;
        PreserveRootDocumentationBoundary = preserveRootDocumentationBoundary;
        DisabledStructuralTransforms = disabledStructuralTransforms;
        LanguageVersion = LanguageVersionResolver.Resolve(languageVersion);
        RootFirstTokenColumn = rootFirstTokenColumn;
        RootChainBoundary = rootChainBoundary;
        RootConstruct = rootConstruct;
        FormatsRegionDirectives = formatsRegionDirectives;
        TargetAnnotation = targetAnnotation;
        TargetFirstTokenColumn = targetFirstTokenColumn;
        RootLineStartColumn = rootLineStartColumn;
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// End-of-line sequence
    /// </summary>
    public string EndOfLine { get; }

    /// <summary>
    /// The depth of the formatting root, in indentation levels. A node-level entry point formats its root without a parent
    /// (FMT-PIPE-023), so this level stands for the scopes around the root that the root no longer reaches: the depth the
    /// indentation phase gives the root's first token at its position in its document (FMT-PIPE-012)
    /// </summary>
    public int BaseIndentLevel { get; }

    /// <summary>
    /// Whether node-scoped formatting should preserve one serialized line break before documentation at the formatting-root boundary
    /// </summary>
    public bool PreserveRootDocumentationBoundary { get; }

    /// <summary>
    /// The configurable structural transforms this run must skip. Every transform runs unless it is listed here
    /// </summary>
    public ConfigurableStructuralTransforms DisabledStructuralTransforms { get; }

    /// <summary>
    /// The concrete C# language version the source targets, never newer than <see cref="LanguageVersionResolver.MaxSupportedLanguageVersion"/>
    /// and never a symbolic value such as <see cref="LanguageVersion.Latest"/> or <see cref="LanguageVersion.Preview"/>.
    /// Version-dependent rules read it instead of a node's parse options, which an earlier rewrite can reset to the defaults
    /// </summary>
    public LanguageVersion LanguageVersion { get; }

    /// <summary>
    /// The column of the formatting root's first token in its document (FMT-PIPE-017). The indentation phase lays out the
    /// root with the columns it gives the root at its depth, shifted so that the first token keeps this column, and nothing
    /// moves a line after the last phase. Only <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/> and
    /// <see cref="ReihitsuFormatter.FormatNodeInDocumentWithContextAsync"/> supply it. Every other entry point leaves it at
    /// <see langword="null"/>, which lays out the root at its depth
    /// </summary>
    public int? RootFirstTokenColumn { get; }

    /// <summary>
    /// The fact about the code above the formatting root that the member-access chain rule reads (FMT-PIPE-024): whether
    /// the root's parent continues the root's chain. The rule
    /// (<see cref="FluentChain.IsOutermostChainNode(Microsoft.CodeAnalysis.SyntaxNode, FluentChainBoundary)"/>) reads it for
    /// the root itself only. Only <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/> supplies it. Every other entry
    /// point leaves it at <see langword="default"/>, which decides the root as a chain of its own
    /// </summary>
    public FluentChainBoundary RootChainBoundary { get; }

    /// <summary>
    /// The facts about the construct the formatting root stands in in its document (FMT-PIPE-024): one flag for each decision
    /// of the line-break phase that reads the root's ancestors, which a root without a parent does not reach. Only
    /// <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/> supplies them. Every other entry point leaves them at
    /// <see cref="RootConstruct.None"/>, which decides the root as a construct of its own
    /// </summary>
    public RootConstruct RootConstruct { get; }

    /// <summary>
    /// Whether the region phase applies the region requirements (FMT-REGION) to <c>#region</c> and <c>#endregion</c>
    /// directives. It defaults to <see langword="true"/>, so every context built without the argument keeps it, among them
    /// those of <see cref="ReihitsuFormatter.FormatDocumentAsync"/> and <see cref="ReihitsuFormatter.FormatSyntaxTree"/>.
    /// The three node-level entry points serve code fixes, which change only the code their diagnostic is about, so they
    /// set it to <see langword="false"/> and apply no region requirement (FMT-PIPE-028)
    /// </summary>
    public bool FormatsRegionDirectives { get; }

    /// <summary>
    /// The annotation that marks the first token of the target inside the formatting root, part of the target-position fact
    /// (FMT-PIPE-024). Only <see cref="ReihitsuFormatter.FormatNodeInDocumentWithContextAsync"/> supplies it, for the target
    /// it formats within its context; every other entry point leaves it at <see langword="null"/>
    /// </summary>
    public SyntaxAnnotation TargetAnnotation { get; }

    /// <summary>
    /// The column of the target's first token in its document (FMT-PIPE-017), part of the target-position fact
    /// (FMT-PIPE-024). The indentation phase lays out the target's other lines as if that token stood in this column,
    /// wherever the phases leave it in the root. Only <see cref="ReihitsuFormatter.FormatNodeInDocumentWithContextAsync"/>
    /// supplies it; every other entry point leaves it at <see langword="null"/>
    /// </summary>
    public int? TargetFirstTokenColumn { get; }

    /// <summary>
    /// The line-start column (FMT-PIPE-024): the column, in its document, of the first token or comment on the line on which
    /// the formatting root's first token stands. The indentation phase measures a continuation line whose construct starts on
    /// that line from it (FMT-INDENT-034). Only <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/>, for its target, and
    /// <see cref="ReihitsuFormatter.FormatNodeInDocumentWithContextAsync"/>, for its context, supply it; every other entry
    /// point leaves it at <see langword="null"/>, which measures that line from the root's first token
    /// </summary>
    public int? RootLineStartColumn { get; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Determines whether the given configurable structural transform runs in this formatting run
    /// </summary>
    /// <param name="transform">The transform to check</param>
    /// <returns><see langword="true"/> if the transform runs; otherwise, <see langword="false"/></returns>
    public bool IsStructuralTransformEnabled(ConfigurableStructuralTransforms transform)
    {
        return (DisabledStructuralTransforms & transform) == ConfigurableStructuralTransforms.None;
    }

    #endregion // Methods
}