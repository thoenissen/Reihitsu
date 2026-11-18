Role: spec author
Commit: 0d6b588
Verdict: text written

## Changed requirements

None. The run changes no spec, change doc, or rule page. The only change is the two documentation comments the issue names, in `src/Reihitsu.Analyzer.CodeFixes/Core/FluentChainCodeFixHelper.cs`:

- `FormatChainAsync` summary: no longer says the line-starting statement or member is "formatted as positional context". It now states the column rule first (first token keeps its line and column, other lines laid out from it, FMT-PIPE-017; code around the chain stays, SHARED-FIX-006), then that the statement or member is handed to the formatter as the code around the chain, in which the formatter lays the chain out, and only the chain is written back.
- `GetContextNode` summary: states its two current roles — the code around the chain that `FormatChainAsync` hands to the formatter, and the code `IsFormattingRefused` checks for a shape the formatter refuses as unsupported.

Premise checked against the specs, not taken from the issue: FMT-PIPE-017 (fourth paragraph) and FMT-PIPE-018 state that the target of `FormatNodeInDocumentWithContextAsync` keeps its first token's document column whatever the context's layout does, and FMT-PIPE-024 gives that column as the target-position fact; so the context decides no column of the chain's first token. The context is still the root the phases format (FMT-PIPE-023) and supplies the depth fact (FMT-PIPE-024), which is why the new text says the formatter lays the chain out in it rather than claiming it does nothing for layout. `IsFormattingRefused` (line 45) calls `FindUnsupportedSyntax(GetContextNode(chainNode) ?? chainNode)`, confirming the refusal-scope role.

Proofs (worktree against base e91afdf):

- `scripts/verify-text-only.sh --head worktree`: `TEXT-ONLY PROOF: PASS — base=e91afdf head=worktree — 2 changed file(s) (1 non-compiled, 1 C# comment/layout-only); xml-doc impact: 1 file(s)`; the C# file: `261 tokens identical (0 literal), directives and disabled text identical, changed trivia: documentation`.
- `scripts/lane-check.sh docs --head worktree`: `LANE CHECK: confirmed — claimed docs, diff needs docs`.
- `scripts/format.sh` on the file: formatted 0 of 1 file(s).
- The xml-doc impact is the new `<see cref="IsFormattingRefused"/>`, a method of the same class; no build was run here.

## Examples for the tests

None (docs lane, no requirement changed).

## Output questions

_None._

## Internal choices

- Kept the word "context" in `<returns>` of `GetContextNode` and in the `IsFormattingRefused` summary: neither attributes a column to it, and the issue scopes only the two summaries. Alternative: reword those too.
- Did not touch `ReihitsuFormatter.FormatNodeInDocumentWithContextAsync`'s summary, which still says "using a containing node for positional context" (`src/Reihitsu.Formatter/ReihitsuFormatter.cs`, around line 257); its remarks already state the first-token column rule. Outside the issue's named file; listed below. Alternative: pull it in as an adjacent text fix (same lane, a one-line change).

## Follow-up candidates

- `ReihitsuFormatter.FormatNodeInDocumentWithContextAsync` summary: "Formats a target node using a containing node for positional context" carries the same outdated wording the issue removes from the helper; per FMT-PIPE-017/018 the context gives no column to the target's first token. Text only, docs lane. The lead may take it into this run as an adjacent fix instead.
- `sdd/changes/2026-11/context-target-first-line.md` line 24 describes a fall-back in the fluent-chain fixes ("fall back to formatting the context otherwise") that `FormatChainAsync` no longer has. It is a historical change doc; noted only, no action proposed.
