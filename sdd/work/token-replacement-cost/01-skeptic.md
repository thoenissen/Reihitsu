Role: skeptic
Commit: 47ef100
Verdict: proceed narrower

My prompt named the issue, the slug, the lane, and the handovers to read. It carried no intended fix, suspected cause, or hoped-for verdict. I worked from the issue, `00-lead.md`, the code, and my own measurements.

## Premise

**The premise is real, and a caller reaches it.** The caller is the CLI: `FormatCommandHandler` (`src/Reihitsu.Cli/FormatCommandHandler.cs:454`) calls `ReihitsuFormatter.FormatSyntaxTree`, which calls `FormattingPipeline.Execute` on the whole compilation unit. That pipeline runs `IndentationPhase` (→ `IndentationRewriter.Apply`, `src/Reihitsu.Formatter/Pipeline/Indentation/Utilities/IndentationRewriter.cs:39`) and `CleanupPhase.Execute` (`src/Reihitsu.Formatter/Pipeline/Cleanup/CleanupPhase.cs:33`). Both call `root.ReplaceTokens(root.DescendantTokens(), ...)` over every token of the file. Code fixes also reach both phases through the node-level entry points, but only for the node they format. No code fix in `src/Reihitsu.Analyzer.CodeFixes` formats a whole type declaration or a document (a `grep` for `TypeDeclarationSyntax` among the callers of `FormatNode*` found none), so the replacement set stays local there and the cost does not matter on that path.

The issue's own claims are derived partly from a profile, not only from reading code. I re-measured them on this commit and did not rely on the issue's numbers.

**1. CLI, per file.** Command: `scripts/format.sh <dir> --check --verbose --max-parallelism 1` (Debug CLI build), with a timestamp on every output line and the gap to the previous line taken as the file's time. Two runs.

Fixture: a synthetic class of N expression-bodied properties, each preceded by a three-line documentation comment. It was written into a temp directory outside the repository:

```csharp
namespace Bench;

/// <summary>
/// Bench
/// </summary>
public class Generated1600
{
    /// <summary>
    /// Gets value 1
    /// </summary>
    public int Value1 => 1;

    // ... repeated for 1..N, a blank line after each property
}
```

| N | Lines | Per-file time, run 1 / run 2 (s) |
|---|---|---|
| 200 | 1008 | 0.066 / 0.119 (and 0.082 / 0.102 for a copy) |
| 400 | 2008 | 0.184 / 0.242 |
| 800 | 4008 | 0.760 / 0.840 |
| 1600 | 8008 | 2.853–2.883 / 3.180–3.204 |

From 400 up, each doubling multiplies the time by about 3.5 to 4.

Real files, perturbed with every `private ` → `private  `, through the same command:

| File | Lines | Time per file (s) |
|---|---|---|
| `src/Reihitsu.Analyzer/AnalyzerResources.cs` (current) | 3503 | 2.49 / 2.64 |
| `src/Reihitsu.Analyzer.Test/Design/RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests.cs` | 6893 | 2.80 / 2.89 |

The `v1.2.0` tag is not in this shallow clone, so I did not re-measure the issue's release figure.

**2. In process, per phase.** Release build. I ran a file-based probe outside the repository: `dotnet run probe.cs -- src/Reihitsu.Cli/bin/Release/net11.0 <variant>`, whose source is quoted at the end. It calls the internal `FormattingPipeline.Execute(root, context, phaseObserver, ct)` through reflection and times the gaps between phase-observer calls. Values are the best of 3, in ms. The token count is `DescendantTokens()` without trivia tokens.

| N | Tokens | Identity `ReplaceTokens`, all tokens | Identity `CSharpSyntaxRewriter` | Indentation phase | `LayoutComputer.Compute` alone | Cleanup phase | Blank lines | Line breaks | Horizontal spacing | Total |
|---|---|---|---|---|---|---|---|---|---|---|
| 400 | 2409 | 47–50 | 0.8–1.4 | 74–88 | 24 | 52–56 | 19–33 | 13–19 | 6–10 | 180–222 |
| 800 | 4809 | 187–203 | 1.4–1.7 | 273–300 | 35–40 | 201–217 | 56–70 | 37–47 | 20–25 | 605–671 |
| 1600 | 9609 | 787–903 | 2.6–3.2 | 1156–1367 | 124–146 | 851–996 | 205–250 | 133–161 | 74–91 | 2684–2836 |

What the table shows:

- **Cleanup** is almost entirely the replacement call: 856 ms against 787 ms of identity replacement at N = 1600.
- **Indentation** is the replacement call plus `LayoutComputer.Compute` plus the token callback. The layout computation alone also grows about 3.1 to 3.7 times per doubling.
- **Identity replacement is quadratic in the size of its replacement set.** At N = 1600, a set of every 10th token costs 8.6 ms and the first tenth of the tokens costs 7.8 ms, about 100 times less than all tokens (846 ms). Reversing the set's order changes nothing (908 ms). An identity token rewriter stays near-linear.

So the issue's claim holds as measured: in the two phases, the whole-tree `ReplaceTokens` call is the dominant cost, and that cost is quadratic. Why Roslyn behaves this way inside I did not establish; the measurement is the evidence, not the issue's reading of `SyntaxReplacer`.

## Mechanism and candidates

**Mechanism.** A Roslyn replacement call (`ReplaceTokens`, `ReplaceTrivia`, `ReplaceSyntax`, `ReplaceNodes`) costs time that grows with the square of the size of its replacement set. So a call whose set grows with the input (the file, or the formatted node) costs time quadratic in that input. Membership is decidable for any call site: does the set's size grow with the input it runs on?

**Dispatch.** The candidates are every multi-item replacement call in `src/Reihitsu.Formatter`, found with `rg -n "\.ReplaceTokens\(|\.ReplaceTrivia\(|\.ReplaceSyntax\(|\.ReplaceNodes\(" src/Reihitsu.Formatter`. I classified each by what bounds its set.

| Site | Set | Grows with | Measured (N = 800 → 1600, ms) |
|---|---|---|---|
| `IndentationRewriter.cs:39` | every token | file (always) | phase 273–300 → 1156–1367 |
| `CleanupPhase.cs:33` | every token | file (always) | phase 201–217 → 851–996 |
| `TabReplacementPhase.cs:39` | tokens with a tab around them | file, when it is tab-indented | variant `tabs`: 7.9 → 28.2 (about 1 % of total) |
| `LineEndingNormalizationPhase.cs:31` (documentation newlines) and `:45` (end-of-line trivia) | line endings other than the majority one | file, only with mixed line endings; at most half the lines, since the majority decides (`LineEndingUtilities.DetectEndOfLine`) | variant `mixed`, 40 % foreign: 31.9 → 124.1 (about 4 % of total) |
| `DocumentationCommentFormattingPhase.cs:183`, `:553` | relocated documentation comments, normalized `///` trivia | number of documentation comments that need it | variant `docnospace` (`///<summary>`): 16.2 → 46.3. I did not separate how much of this the replacement causes. |
| `TabReplacementPhase.cs:137` | directives holding a tab | number of such directives | small; not measured |
| `TabReplacementPhase.cs:151` | one directive's own tokens | one directive | bounded |
| `RegionNamingRewriter.cs:81` | `#region` / `#endregion` trivia | number of regions | small; not measured |
| `RawStringLineShifts.cs:146` | lines of multi-line raw strings that move | raw-string content | rare; not measured |
| `LineBreakListRewriter.cs:526` | separators of one list without a line break | size of one list | see the `array` variant below |
| `LineBreakTriviaUtilities.cs:280`, `GenericConstraintLineBreakRewriter.cs:241`, `ReihitsuFormatter.cs:547` | gaps or tokens inside one node | one node or one code-fix target | local |
| the two- and three-token calls (`TokenGapNormalizer`, `BinaryOperator…`, `Ternary…`, `LineBreakAssignment…`, `LineBreakList…` :104/:184/:216, `SwitchCaseWhen…`) | constant | nothing | — |

**Variant `array`.** One field initialized with 2·N integers, 20 per line. The line-break phase gives each element its own line.

| N | Tokens | Indentation | Cleanup | Blank lines | Line breaks | Horizontal spacing | Total |
|---|---|---|---|---|---|---|---|
| 800 | 3221 | 356–364 | 208–210 | 117–120 | 60–62 | 37–38 | 799–816 |
| 1600 | 6421 | 1439–1441 | 819–843 | 472–477 | 233–240 | 147–149 | 3165–3187 |

Here the three other phases together are about 27 % of the total, and each grows about 4 times per doubling. I did not establish whether the replacement set at `LineBreakListRewriter.cs:526` or something else causes that growth (next paragraph).

**A second, separate mechanism (not this issue's).** Calling `GetPreviousToken()` once for every token also grows quadratically on the synthetic input: 4.1 → 14.4 → 55.0 ms for N = 400 → 800 → 1600. `GetNextToken()` grows less steeply: 2.1 → 3.8 → 11.6 ms. Several phases call these once per token or once per comment: `HorizontalSpacingRewriter.cs:68`/`:122`, `CommentIndentationContributor.cs:71`, `LayoutComputer.cs:504`, and `CleanupPhase.cs:100`/`:184`. The horizontal-spacing phase's growth (24 → 82 ms) tracks the `GetPreviousToken` figures closely. This is a candidate explanation for the "remaining phases grow faster than linearly" that the issue left uninvestigated, and for the layout computation's growth. Navigating in a node with many children is not a replacement set, so this lies outside the issue's mechanism. It goes to *Follow-up candidates*.

## Breaking inputs

The change must not change any output. So the "breaking inputs" are the inputs where a replacement with different internals could see a different set of tokens, in a different tree, or in a different order than `ReplaceTokens(root.DescendantTokens(), (original, rewritten) => …)` does today.

The failure classes that can reach this change:

- **FC-040, FC-019, FC-020 — tokens inside structured trivia.** `DescendantTokens()` without `descendIntoTrivia` does not yield the tokens inside documentation comments, directives, or skipped-token trivia. Today the callback never sees them. Inputs to cover: `///` and `/** */` documentation comments, `#if`/`#else`/`#endif` with disabled text, `#region`, `#pragma`, and a directive between tokens.
- **The original/rewritten contract.** Both callbacks read the original token's position against the original tree:
  - `IndentationRewriter`: `lineStarts.IsLineStart(originalTrivia[i].FullSpan.Start)` and `original.SpanStart`.
  - `CleanupPhase`: `original.FullSpan.Start == 0`, `original.GetNextToken(includeZeroWidth: true)` (`:100`), `originalToken.GetNextToken().LeadingTrivia` (`:184`), `original.IsKind(EndOfFileToken)`.

  Inputs to cover: the first token of the file (text starts at position 0); the `EndOfFileToken` with whitespace-only lines and trailing line breaks in front of it; a file whose last real token carries the trailing whitespace; and a token followed by a line break in the next token's leading trivia.
- **Missing and zero-width tokens.** `ApplyIndentation` returns missing tokens unchanged. Whole documents with syntax errors never reach the phases, but a node-level input from a code fix can carry a missing token. `GetNextToken(includeZeroWidth: true)` matters for the end of file.
- **FC-022 — raw strings.** `placedColumns` is filled during the token walk and read afterwards by `RawStringLineShifts.MoveLines`, including the continuation lines of a placed `///` comment. Inputs to cover: multi-line raw and interpolated raw strings, with and without holes, behind a `///` comment.
- **FC-028 — node-level entry points.** `IndentationPhase` re-roots an attached input into a tree of its own. Cleanup on a detached root removes only the last token's line breaks. Inputs to cover: `FormatNode` and `FormatNodeInDocumentAsync` / `…WithContextAsync` with an attached target and with a detached target.
- **FC-010 — CRLF.** Every case above under LF and under CRLF.
- **Cancellation.** Both callbacks check the cancellation token once per token. A replacement that keeps them must still observe cancellation within one file. Behavior only, not output.
- **FC-017, FC-033 — weak equivalence evidence.** `FormatterSelfHostingTests.FormatterProducesNoChangesOnSourceFile` formats the repository's sources, which are already formatted. The two phases then return every token unchanged, so that test cannot tell an equivalent rewrite from one that drops a cleanup or an indentation edit. Output equivalence has to be shown where the phases actually rewrite: wrong indentation, trailing whitespace, whitespace and line breaks at the end of the file, tabs, CRLF, and mixed line endings. The existing formatter unit, regression, and full-pipeline tests cover much of that.
- **FC-016 — sibling sites.** The sites in the table above that also grow with the file: `TabReplacementPhase.cs:39`, `LineEndingNormalizationPhase.cs:31`/`:45`, and `DocumentationCommentFormattingPhase.cs:183`/`:553`. Each is either made linear or carries its stated cost (see *Issue goals*).
- **FC-031.** Both `Apply`/`Execute` summaries, and the `original`/`rewritten` parameter docs of `ApplyIndentation` and `CleanToken`, describe the current call shape.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `IndentationRewriter.Apply` (indentation phase) | must change: the whole-tree replacement |
| `CleanupPhase.Execute` | must change: the whole-tree replacement |
| `TabReplacementPhase` :39, `LineEndingNormalizationPhase` :31/:45, `DocumentationCommentFormattingPhase` :183/:553 | lead's choice (*Internal choices* 2): make linear, or state the measured cost |
| Other formatter replacement sites (local or constant sets) | must stay |
| `LayoutComputer` and the other phases' navigation cost | must stay in this run; follow-up |
| Formatter output, every entry point | must stay byte-identical |
| Specs (`sdd/specs/formatter/*`) | not involved: no requirement states a cost bound, and nothing visible changes |
| Analyzers, code fixes, Fix All, `Reihitsu.Core`, rule docs, CLI | not involved |
| Tests | the reproduction and the equivalence evidence (*Internal choices* 1) |
| `scripts/benchmark.sh` | not involved. It times released NuGet versions only (`--commit` picks the corpus, not the tool), so it cannot measure this branch before a release |

## Refuse option

**Not making the change.** The CLI keeps paying about 2.5 to 2.9 s for each 3500- to 7000-line file on this machine (Debug CLI), and the time quadruples with each doubling of the file. Files of a few hundred lines pay only tens of milliseconds.

**What the change costs.** Two call sites in production code. It needs no new mechanism beyond a linear replacement, and nothing visible changes.

**Withholding or leaving as written.** Not applicable: no input class is mishandled. Refusing large files, for example by skipping them, would change visible output for code people commonly write, and it is not an option.

**Declaring a shape unsupported.** Not applicable: the cost comes from size, not from a syntactic shape.

**Simpler option.** The run could stop at the two whole-tree sites and only state the cost of the sibling sites, which measure at 1 to 4 % of the total. The issue's "Done when" already allows that.

## Output questions

_None._ The change keeps every output byte-identical. A version that does not would be a defect, not a question.

## Internal choices

1. **How to reproduce a performance bug in a test.**
   - A wall-clock test (a time budget, or the ratio between N and 2N) is flaky under CI contention, the hazard `AGENTS.md` describes for `Regex` timeouts.
   - A ratio test on a whole phase would also keep failing after a correct fix: the indentation phase includes `LayoutComputer.Compute`, which grows about 3.5 times per doubling on its own.
   - A deterministic proxy is possible but brittle. For example, an architecture test that forbids passing all of a root's descendant tokens or trivia to a `Replace*` call catches only that spelling.

   **Recommendation:** no timing assertion in CI. The guard is output equivalence, from existing tests plus new tests in the cells of *Breaking inputs* where the two phases rewrite. The scaling evidence is a before/after measurement the validator records in the PR: the probe below, or the CLI per-file timing above, at N = 400/800/1600. The lead decides whether a deterministic architecture test is worth adding. No visible output is affected either way.

2. **Sibling sites.**
   - Option (a): make `TabReplacementPhase.cs:39` and `LineEndingNormalizationPhase.cs:31`/`:45` linear in the same way as the hot spots.
   - Option (b): state their measured cost, which is quadratic but 1 % (tabs) and 4 % (40 % foreign line endings) of the total at N = 1600.

   **Recommendation:** (a) only if the replacement for the two hot sites is a shared helper that these sites can call with no extra machinery; otherwise (b). The same applies to `DocumentationCommentFormattingPhase.cs:183`/`:553`, whose set needs documentation comments that the phase actually relocates or normalizes. No visible output is affected.

## Issue goals

The issue is `agent-filed`.

- **"The cost of the indentation and cleanup phases grows linearly … (a doubling of N no longer multiplies it by about 4)."**
  - Cleanup: achievable as written, since the phase is about 92 % replacement call.
  - Indentation: **not achievable by this issue's mechanism.** `LayoutComputer.Compute` alone goes 35–40 → 124–146 ms (× 3.1 to 3.7) from N = 800 to 1600. Taking the replacement out leaves a phase that still grows faster than linearly.

  **Change the goal to:** the whole-tree replacement in both phases grows linearly with the token count, and both phases lose the replacement's share (about 787 to 903 ms of each at N = 1600). The indentation phase's remaining growth is a follow-up. This changes no visible output.
- **"Formatter output unchanged on the repository's own sources."** Keep, but on its own this is weak evidence: the self-hosting sources are already formatted (*Breaking inputs*, FC-017). Add: unchanged output on inputs where the two phases rewrite. No visible output involved.
- **"For each other site listed, either it is made linear or the remaining cost is stated with the reason it stays."** Keep, with the numbers above. Add `DocumentationCommentFormattingPhase.cs:183`/`:553`, which the issue counts among the "filtered or local sets" although its set grows with the number of documentation comments to normalize. Also note the claim "every other site replaces a filtered or local set of a few tokens" is too strong for `LineBreakListRewriter.cs:526` on a large initializer.
- **Severity claim** ("about a fifth of a whole run" over the v1.2.0 corpus). Not re-verified, since the tag is missing from the clone. The per-file cost for large files is confirmed. For typical files of a few hundred lines the cost is small, so the severity rests on large files, which are common enough: this repository has several over 3000 lines.

## Architecture findings

_None._ The cause is a property of a Roslyn API applied to a set that grows with the input. It is not two components owning one decision, a prediction, or a rule stated twice. No entry point predicts a phase's decision (question 10): no entry point changes.

## Follow-up candidates

1. **Quadratic navigation cost in the other phases.** On the synthetic input, `GetPreviousToken()` per token grows about 4 times per doubling (4.1 → 14.4 → 55.0 ms for N = 400/800/1600). The blank-line, line-break, and horizontal-spacing phases and `LayoutComputer.Compute` grow about 3.1 to 4 times per doubling. On the `array` variant at N = 1600, those three phases take 27 % of the run (about 860 of 3165 ms). Candidate per-token or per-comment call sites:
   - `HorizontalSpacingRewriter.cs:68`, `:122`
   - `CommentIndentationContributor.cs:71`
   - `LayoutComputer.cs:504`
   - `MethodChainAlignmentContributor.cs:69`
   - `CleanupPhase.cs:100`, `:184`

   The `LineBreakListRewriter.cs:526` set on a large initializer is a further candidate. The mechanism is not yet established: the follow-up must measure before it attributes. The output is unchanged; this is a maintenance issue.

## Probe source (outside the repository, for reproduction)

Run with `dotnet run probe.cs -- /home/user/Reihitsu/src/Reihitsu.Cli/bin/Release/net11.0 <plain|tabs|mixed|docnospace|array>` after `scripts/build.sh`.

```csharp
#:package Microsoft.CodeAnalysis.CSharp.Workspaces@5.12.0
#:property PublishAot=false
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

var bin = args[0];
var asm = Assembly.LoadFrom(Path.Combine(bin, "Reihitsu.Core.dll"));
asm = Assembly.LoadFrom(Path.Combine(bin, "Reihitsu.Formatter.dll"));
var ctxType = asm.GetType("Reihitsu.Formatter.Data.FormattingContext", true)!;
var ctor = ctxType.GetConstructors().First(c => c.GetParameters()[0].ParameterType == typeof(string));
var pipe = asm.GetType("Reihitsu.Formatter.Pipeline.FormattingPipeline", true)!;
var exec = pipe.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).First(m => m.Name == "Execute" && m.GetParameters().Length == 4);

string Gen(int n)
{
    var sb = new StringBuilder("namespace Bench;\n\n/// <summary>\n/// Bench\n/// </summary>\npublic class G\n{\n");
    for (var i = 1; i <= n; i++) sb.Append($"    /// <summary>\n    /// Gets value {i}\n    /// </summary>\n    public int Value{i} => {i};\n\n");
    sb.Append("}\n");
    return sb.ToString();
}

foreach (var n in new[] { 400, 800, 1600, 800, 1600 })
{
    var text = Gen(n);
    var variant = args.Length > 1 ? args[1] : "plain";
    if (variant == "tabs") text = string.Join("\n", text.Split('\n').Select(l => l.StartsWith("    ") ? "\t" + l.Substring(4) : l));
    if (variant == "mixed") { var ls = text.Split('\n'); text = string.Concat(ls.Select((l, i) => l + (i == ls.Length - 1 ? "" : (i % 5 < 2 ? "\r\n" : "\n")))); }
    if (variant == "docnospace") text = text.Replace("/// <", "///<");
    if (variant == "array") { var sb2 = new StringBuilder("namespace Bench;\n\npublic class G\n{\n    private static readonly int[] Table =\n    {\n"); for (var i = 0; i < n * 2; i++) sb2.Append(i % 20 == 0 ? "        " : " ").Append(i).Append(',').Append(i % 20 == 19 ? "\n" : ""); sb2.Append("\n    };\n}\n"); text = sb2.ToString(); }
    var root = CSharpSyntaxTree.ParseText(text).GetRoot();
    var tokens = root.DescendantTokens().Count();
    var best = new Dictionary<string, double>();
    void Rec(string k, double v) { best[k] = best.TryGetValue(k, out var o) ? Math.Min(o, v) : v; }
    for (var rep = 0; rep < 3; rep++)
    {
        var sw = Stopwatch.StartNew();
        root.ReplaceTokens(root.DescendantTokens(), (o, r) => r);
        Rec("idReplaceTokens", sw.Elapsed.TotalMilliseconds);
        sw.Restart();
        new IdRewriter().Visit(root);
        Rec("idRewriter", sw.Elapsed.TotalMilliseconds);
        var all = root.DescendantTokens().ToList();
        sw.Restart();
        foreach (var t in all) _ = t.GetPreviousToken();
        Rec("prevTokenAll", sw.Elapsed.TotalMilliseconds);
        sw.Restart();
        _ = root.ReplaceTokens(Enumerable.Reverse(all), (o, r) => r);
        Rec("idReplaceReversed", sw.Elapsed.TotalMilliseconds);
        sw.Restart();
        _ = root.ReplaceTokens(all.Where((t, i) => i % 10 == 0), (o, r) => r);
        Rec("idReplaceEvery10th", sw.Elapsed.TotalMilliseconds);
        sw.Restart();
        _ = root.ReplaceTokens(all.Take(all.Count / 10), (o, r) => r);
        Rec("idReplaceFirstTenth", sw.Elapsed.TotalMilliseconds);

        var args2 = ctor.GetParameters().Select((p, i) => i == 0 ? (object)"\n" : p.DefaultValue).ToArray();
        var ctx = ctor.Invoke(args2);
        var lc = asm.GetType("Reihitsu.Formatter.Pipeline.Indentation.Utilities.LayoutComputer", true)!.GetMethod("Compute")!;
        sw.Restart();
        lc.Invoke(null, new object?[] { root, ctx, CancellationToken.None });
        Rec("LayoutComputerOnly", sw.Elapsed.TotalMilliseconds);
        sw.Restart();
        foreach (var t in all) _ = t.GetNextToken();
        Rec("nextTokenAll", sw.Elapsed.TotalMilliseconds);
        var phase = Stopwatch.StartNew();
        var total = Stopwatch.StartNew();
        Action<string, SyntaxNode, SyntaxNode> obs = (name, i, o) => { Rec(name, phase.Elapsed.TotalMilliseconds); phase.Restart(); };
        exec.Invoke(null, new object?[] { root, ctx, obs, CancellationToken.None });
        Rec("TOTAL", total.Elapsed.TotalMilliseconds);
    }
    Console.WriteLine($"N={n} tokens={tokens} " + string.Join(" ", best.OrderByDescending(kv => kv.Value).Select(kv => $"{kv.Key}={kv.Value:F1}")));
}

class IdRewriter : CSharpSyntaxRewriter { public IdRewriter() : base(true) { } public override SyntaxToken VisitToken(SyntaxToken t) => base.VisitToken(t); }
```
