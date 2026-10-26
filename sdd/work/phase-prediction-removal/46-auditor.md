Role: auditor
Commit: 3be3e26
Verdict: pass, text cleanup

My prompt carried no intended fix, no suspected cause, and no verdict anyone hoped for. It asked for one narrow check, not a full audit.

## Scope

Narrow check of the text repair of #12 (`45-spec-author.md`, commit 3be3e26) on head 3be3e26 (checked out, clean):
- every column the new FMT-PIPE-017 "Recorded gap" example for the second case states;
- the truth of the repaired gap paragraph, the SHARED-FIX-007 bullet, and the changed change-doc sentences.

The round changes only `sdd/specs/formatter/pipeline.md`, `sdd/specs/shared/code-fixes.md`, and `sdd/changes/2026-10/phase-prediction-removal.md`. The production code is the code of 54d7004, which `44-auditor.md` compared with `main`.

**Harnesses** (in my scratchpad, both file-based apps against this head's formatter):
- the public `FormatNodeInDocumentWithContextAsync` (`aud/hc`);
- a reflection probe (`aud/hr2`) that runs `FormattingPipeline.Execute` on the context statement, configured as the context entry point configures it: first-token column from `ComputeDocumentColumn(…, false)`, base level from `ComputeBaseIndentLevel`, `preserveRootDocumentationBoundary: true`, `AccessorExpressionBody` disabled. It prints the phases' output of the context before anything is written back.

**Input**: the example's statement, `        var x = Call(1, Wrap("""` / `    a` / `    """));`, inside a method of a class, with the argument `Wrap(…)` as target and the statement as context. Columns are 0-based.

## The example's columns

| Statement in the example | Observed | Result |
|---|---|---|
| The phases put the argument on a line of its own under `1`, in column 21 | Phases' output: `        var x = Call(1,` with `1` at 21; next line `Wrap("""` with `Wrap(` at 21 | confirmed |
| The phases put the content line and the closing marker under the opening quotes, in column 26 | Phases' output: quotes at 26 on the `Wrap(` line; `a` at 26; `"""));` at 26 | confirmed |
| The line break is not written back, so `Wrap(` stays on the first line | Written back: `        var x = Call(1, Wrap("""`, with `Wrap(` on the first line at 24, its document column | confirmed |
| The opening quotes stand in column 29 | Written back: quotes at 29 | confirmed |
| The content line and the closing marker stand in column 26, three columns left of the quotes | Written back: `a` at 26; `"""));` at 26 | confirmed |

The same written-back output holds under CRLF.

## The repaired texts

| Text | Claim | Evidence | Result |
|---|---|---|---|
| FMT-PIPE-017, gap definition | The gap is a target whose first token the phases place on another line or in another column than it keeps in the document. Only the target is written back, with its leading trivia as in the document, and everything in front of it on its line stays | The code is unchanged since 54d7004 (the original trivia is written back in every case). The example above shows the first token kept at 24 while the phases put it on a new line at 21 | true |
| FMT-PIPE-017, first case | A later-line target whose line the phases give another column | `44-auditor.md`: the RH5111 declarator, `b` at 14 and `2` at 18 while `1` is at 24, identical to `main`; the code is unchanged | true |
| FMT-PIPE-017, second case | The phases break the line in front of a mid-line target; the break is not written back | the example above | true |
| FMT-PIPE-017, the other side | A target whose first token the phases leave on its line and in its column is outside the gap; examples: a target starting the context's first line, and a mid-line target with nothing changed in front of it on its line | RH5111 on a first declarator (`int b =` / `Other(1,` / `2);`) and a first parameter (`void M(int first =` / `1, int second = 2)`): the fix's code equals document-level formatting. The f1 file differs only in an expression-bodied method outside the fix. So do the second-declarator and second-parameter probes of `44-auditor.md`. The tests `TargetStartingTheFirstLineOfItsContext…` pin the first-line case | true for every member probed |
| SHARED-FIX-007, gap bullet | The gap also covers changed code that does not start its line when the formatter breaks the line in front of it. The first parameter behind `(` and the first declarator behind its type in the RH5111 fix are outside the gap | the same probes; no code fix reaches the second case (`44-auditor.md`) | true |
| Change doc, What bullet | A target keeps its first token's line and column in the document, also where the phases place it elsewhere: the later-line case and the line-break case | as above | true |
| Change doc, Decision paragraph | The two known cases; the example "(such as an argument holding a raw string literal, whose content lines then end up left of its opening quotes, with the same output as today)"; RH3208's label line is kept; RH5111 does not split in front of a first declarator or parameter; no code fix reaches the second case | Every clause holds, except "with the same output as today", which holds for the example as written but not for every such argument (#13). RH3208's label line: the label probes of `44-auditor.md` equal document-level formatting | true except #13 |

## Findings
| # | Severity | Location | Failure class | Repeats | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|---|
| 13 | low | `sdd/changes/2026-10/phase-prediction-removal.md`, Decision "The node-level entry points lay out their node at its own column", last paragraph: the parenthetical "with the same output as today", and the earlier sentence of the same paragraph "and the output stays as it is today" | — | – | same requirement (text written in this round and in round 38) | Both sentences hold for the example as written: a statement at its depth gives content at 26 under quotes at 29, identical to `main`. They do not hold for every member of the gap. With the same statement misindented by two columns, head writes the content at 28 under quotes at 31 and `main` at 26. A later-line target behind a comment tail in a misindented statement gives `3` at 37 on head and 39 on `main`. The source of these differences is the decided column step of the context path (`44-auditor.md`, the public-API rows), not the write-back. In each case both outputs are off the anchor. | Text only: limit the claim to the example ("with the same output as today for that example"), or say that the output differs from today only where the decided column step lays out a misindented context, and is off the anchor in both versions there. |

**Why `pass, text cleanup`:** #13 is confined to the change doc. It cannot reach compiled behavior, diagnostics, public API documentation, or rule metadata. Every column of the new example and every claim of the repaired spec texts is confirmed.

## Guard scope
_N/A — the round changes no code, predicate, or guard (`verify-text-only.sh --strict-docs` passed, per `45-spec-author.md`)._

## Policy ownership
_N/A — no policy changed; text only._

## Assertion adequacy
_N/A — no test added or changed; the example describes the recorded gap, which no requirement covers._

## Failure classes dismissed
FC-037: the repair's text-only status rests on `verify-text-only.sh`, not on a line-based check. I did not re-walk the other classes: the code is unchanged since the audit of 54d7004 (`44-auditor.md`).

## Architecture findings
_None._

## Hints
_None._
