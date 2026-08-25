using System;
using System.Collections.Generic;

namespace Reihitsu.Analyzer.Test.SelfHosting;

/// <summary>
/// Rule documents whose <c>### Violation</c> / <c>### Correction</c> examples do not (yet) satisfy every
/// criterion asserted by <see cref="RuleDocumentationExampleTests"/>, together with the reason each is opted
/// out. This list must only shrink: <see cref="RuleDocumentationExampleTests.OptedOutDocumentsStillFailVerification"/>
/// fails when a listed document would actually pass, so a fixed document has to be removed here in the same
/// change that fixes it
/// </summary>
internal static class RuleDocumentationExampleOptOuts
{
    #region Fields

    /// <summary>
    /// Every opted-out rule document, keyed by diagnostic ID, with the reason it is opted out
    /// </summary>
    internal static readonly IReadOnlyDictionary<string, string> Reasons = new Dictionary<string, string>(StringComparer.Ordinal)
                                                                           {
                                                                               // Fragments: no enclosing type/member, does not parse as a compilation unit
                                                                               ["RH0001"] = FragmentReason,
                                                                               ["RH0002"] = FragmentReason,
                                                                               ["RH1001"] = FragmentReason,
                                                                               ["RH1002"] = FragmentReason,
                                                                               ["RH2002"] = FragmentReason,
                                                                               ["RH2003"] = FragmentReason,
                                                                               ["RH2109"] = FragmentReason,
                                                                               ["RH3002"] = FragmentReason,
                                                                               ["RH3102"] = FragmentReason,
                                                                               ["RH3105"] = FragmentReason,
                                                                               ["RH3201"] = FragmentReason,
                                                                               ["RH3202"] = FragmentReason,
                                                                               ["RH3203"] = FragmentReason,
                                                                               ["RH4103"] = FragmentReason,
                                                                               ["RH4104"] = FragmentReason,
                                                                               ["RH4105"] = FragmentReason,
                                                                               ["RH4116"] = FragmentReason,
                                                                               ["RH4118"] = FragmentReason,
                                                                               ["RH4122"] = FragmentReason,
                                                                               ["RH4123"] = FragmentReason,
                                                                               ["RH4127"] = FragmentReason,
                                                                               ["RH5001"] = FragmentReason,
                                                                               ["RH5002"] = FragmentReason,
                                                                               ["RH5003"] = FragmentReason,
                                                                               ["RH5004"] = FragmentReason,
                                                                               ["RH5005"] = FragmentReason,
                                                                               ["RH5006"] = FragmentReason,
                                                                               ["RH5007"] = FragmentReason,
                                                                               ["RH5008"] = FragmentReason,
                                                                               ["RH5009"] = FragmentReason,
                                                                               ["RH5010"] = FragmentReason,
                                                                               ["RH5012"] = FragmentReason,
                                                                               ["RH5013"] = FragmentReason,
                                                                               ["RH5014"] = FragmentReason,
                                                                               ["RH5015"] = FragmentReason,
                                                                               ["RH5016"] = FragmentReason,
                                                                               ["RH5017"] = FragmentReason,
                                                                               ["RH5018"] = FragmentReason,
                                                                               ["RH5019"] = FragmentReason,
                                                                               ["RH5021"] = FragmentReason,
                                                                               ["RH5029"] = FragmentReason,
                                                                               ["RH5030"] = FragmentReason,
                                                                               ["RH5602"] = FragmentReason,
                                                                               ["RH5603"] = FragmentReason,
                                                                               ["RH5604"] = FragmentReason,
                                                                               ["RH7301"] = FragmentReason,
                                                                               ["RH7302"] = FragmentReason,
                                                                               ["RH8101"] = FragmentReason,
                                                                               ["RH8102"] = FragmentReason,
                                                                               ["RH8103"] = FragmentReason,
                                                                               ["RH8104"] = FragmentReason,
                                                                               ["RH8105"] = FragmentReason,
                                                                               ["RH8106"] = FragmentReason,
                                                                               ["RH8107"] = FragmentReason,
                                                                               ["RH8204"] = FragmentReason,

                                                                               // Context-dependent: needs project configuration, a disabled-by-default rule, a file name, or an
                                                                               // undefined external symbol that a standalone fragment cannot supply
                                                                               ["RH1003"] = ContextDependentReason,
                                                                               ["RH3001"] = ContextDependentReason,
                                                                               ["RH3101"] = ContextDependentReason,
                                                                               ["RH3103"] = ContextDependentReason,
                                                                               ["RH3104"] = ContextDependentReason,
                                                                               ["RH4001"] = "Example's violation depends on the source file's name ('Foo.cs' in a comment), which the verifier's in-memory document does not carry.",
                                                                               ["RH4009"] = "Example needs a reihitsu.json configuration file listing the allowed namespace, which is outside a standalone compilation unit.",
                                                                               ["RH7305A"] = "The analyzer is disabled by default, so it reports nothing without explicit opt-in configuration.",
                                                                               ["RH7409"] = ContextDependentReason,
                                                                               ["RH7411"] = ContextDependentReason,
                                                                               ["RH8402"] = ContextDependentReason,

                                                                               // Fix declines its own example, throws, or does not converge within the iteration budget
                                                                               ["RH5205"] = FixDoesNotConvergeReason,
                                                                               ["RH5301"] = FixDoesNotConvergeReason,
                                                                               ["RH5302"] = FixDoesNotConvergeReason,
                                                                               ["RH5408"] = FixDoesNotConvergeReason,
                                                                               ["RH5417"] = "The code fix offers no action for the example printed on its own page, even though the metadata table advertises Code Fix ✓.",
                                                                               ["RH7004"] = "The code fix offers no action for the example printed on its own page, even though the metadata table advertises Code Fix ✓.",

                                                                               // Code-fix output differs from the documented correction: an alternative valid form, or a genuine
                                                                               // divergence tracked separately from this issue
                                                                               ["RH2005"] = CodeFixOutputDiffersReason,
                                                                               ["RH4106"] = CodeFixOutputDiffersReason,
                                                                               ["RH5102"] = CodeFixOutputDiffersReason,
                                                                               ["RH5107"] = CodeFixOutputDiffersReason,
                                                                               ["RH5110"] = CodeFixOutputDiffersReason,
                                                                               ["RH5111"] = CodeFixOutputDiffersReason,
                                                                               ["RH5304"] = CodeFixOutputDiffersReason,
                                                                               ["RH5501"] = CodeFixOutputDiffersReason,
                                                                               ["RH5503"] = CodeFixOutputDiffersReason,
                                                                               ["RH7103"] = CodeFixOutputDiffersReason,
                                                                               ["RH7104"] = CodeFixOutputDiffersReason,
                                                                               ["RH7107"] = CodeFixOutputDiffersReason,
                                                                               ["RH7108"] = CodeFixOutputDiffersReason,
                                                                               ["RH7201"] = CodeFixOutputDiffersReason,
                                                                               ["RH7202"] = CodeFixOutputDiffersReason,
                                                                               ["RH7203"] = CodeFixOutputDiffersReason,
                                                                               ["RH7204"] = CodeFixOutputDiffersReason,
                                                                               ["RH7205"] = CodeFixOutputDiffersReason,
                                                                               ["RH7206"] = CodeFixOutputDiffersReason,
                                                                               ["RH7207"] = CodeFixOutputDiffersReason
                                                                           };

    /// <summary>
    /// A rule document's example is a bare fragment (for example a single field or statement with no
    /// enclosing type) and does not parse as a standalone compilation unit. Wrapping fragments in an
    /// enclosing type or method to make them compile is an explicit non-goal of issue #671, since a plausible
    /// wrapping strategy is unproven and indentation-sensitive
    /// </summary>
    private const string FragmentReason = "Example is a fragment and does not parse as a standalone compilation unit; wrapping fragments is an explicit non-goal of issue #671.";

    /// <summary>
    /// A rule document's example does not report the document's own diagnostic ID when compiled and analyzed
    /// on its own, because the analyzer needs semantic context (a symbol, a type, or a project setting) that a
    /// standalone fragment cannot supply
    /// </summary>
    private const string ContextDependentReason = "Example needs semantic or project context beyond a standalone compilation unit for the analyzer to report; the reported diagnostic is real but not reproducible in isolation.";

    /// <summary>
    /// A rule document's example throws while applying the code fix, or the fix does not resolve the
    /// diagnostic within a bounded number of iterations
    /// </summary>
    private const string FixDoesNotConvergeReason = "The shipped code fix does not converge to a fixed point for this specific example (it throws or keeps re-reporting); this is a known code-fix gap tracked separately from issue #671.";

    /// <summary>
    /// A rule document's example produces a code-fix result that differs from the documented correction,
    /// either because the correction documents one of several valid alternative forms, or because the fix
    /// output has genuinely diverged from the page
    /// </summary>
    private const string CodeFixOutputDiffersReason = "The shipped code fix output does not match the documented correction character for character; this divergence is tracked separately from issue #671, which is scoped to landing the test green over already-passing documents.";

    #endregion // Fields
}