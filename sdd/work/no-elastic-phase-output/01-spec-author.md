Role: spec author
Commit: 52b0517
Verdict: change doc written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. The issue's "Done when" (thoenissen/Reihitsu#1516, `agent-filed`) was treated as a proposal; the spec states "no phase adds elastic trivia" rather than "no phase output carries elastic trivia" (see *Internal choices*).

## Files

- `/home/user/Reihitsu/sdd/changes/2026-11/no-elastic-phase-output.md` — change doc, `Status: Draft`; `Visible output changes`: none.
- `/home/user/Reihitsu/sdd/specs/formatter/pipeline.md` — FMT-PIPE-034 added (inserted before FMT-PIPE-022, next to FMT-PIPE-021); one non-goal added; `Uses` gains SHARED-ATTR-018, FMT-BREAK-032, FMT-SPACE-008.
- `/home/user/Reihitsu/sdd/specs/formatter/structural-transforms.md` — FMT-STRUCT-026's last elastic sentence now points to FMT-PIPE-034; `Uses` gains FMT-PIPE-034.
- `/home/user/Reihitsu/sdd/specs/requirement-ids.txt` — FMT-PIPE-034 added.

No rule page changes. `sdd/specs/README.md` needs no change: no code owner was added or removed.

## Changed requirements

- FMT-PIPE-034 — No phase adds elastic trivia — added
- FMT-STRUCT-026 — The phase writes only the separators its text needs — changed (wording only: the sentence "This requirement does not decide whether the phases after this one write elastic trivia" is replaced by a pointer to FMT-PIPE-034; no behavior of the structural phase changes)

## Examples for the tests

- FMT-PIPE-034 — the accessor-list example (`int P { get; [param: A] [param: B] set; }` and the multi-line `Q`). The final output was produced with `scripts/format.sh` on today's code and matches the spec text: `int P { get; [param: A, B] set; }` and `get;` / `[param: A, B] set;`.
- FMT-PIPE-034 also states the document-entry-point consequence: no phase output of `FormatDocumentAsync` / `FormatSyntaxTree` carries elastic trivia on any token, including tokens inside directives and documentation comments (`descendIntoTrivia`). The issue proposes asserting it in the per-phase observer of `PhaseTreeRoundTrip` on every formatter fixture.

How I established that nothing visible changes today (facts, run in a throwaway worktree that is deleted again, nothing committed): I added a check to the per-phase observer of `PhaseTreeRoundTrip` that reports every trivia with `SyntaxAnnotation.ElasticAnnotation` on any token (`descendIntoTrivia: true`), and ran `scripts/test.sh --project formatter` at 52b0517. Result: 5392 passed, 5 failed, and all 5 failures were the probe's report. Only these tests were affected, all in `AttributeListStructuralFinalOutputTests`: `AccessorExampleOfTheRequirement`, `IndexerAccessorListsFollowTheAccessorListLayout`, `ParamListsOfASetterInAnAccessorListOverSeveralLinesAreMerged`, `ParamListsOfASetterInAnAccessorListOnTheDeclarationsLineAreMerged`, `ParamAndTypevarListsOfEveryAccessorInAnAccessorListOverSeveralLinesAreMerged`. In each, the elastic trivia sat on `CommaToken`s and was first reported in `LineBreakPhase`. It was then reported again in the output of `SwitchCaseBracePhase`, `BlankLinePhase`, `HorizontalSpacingPhase`, `IndentationPhase`, and `CleanupPhase`. No other phase and no other token kind was reported.

Boundary the tests must keep: the attribute-list merge code fixes (RH5502 … RH5531, via `TargetAttributeListShapeCodeFixProviderBase` → `AttributeTargetUtilities.MergeAttributeListGroup` with `AttributeListMergeSeparators.Spaced`) call the same shared merge without running a phase. They return commas written without a space, and their asserted result is `[A, B]` (SHARED-ATTR-018). Those results are visible output and do not change; FMT-PIPE-034 does not cover them.

`scripts/test.sh --project architecture`: 179 passed, 1 failed. The failure is `EveryRequirementOfAnEnforcedSpecIsCovered`, which reports that no test is tagged `[Requirement("FMT-PIPE-034")]`. That is expected until the test implementer adds the covering test.

## Output questions

_None._ The change doc lists no visible output change. Formatter output stays the same, and so do code-fix results, including the SHARED-ATTR-018 merge fixes. No diagnostic, rule page, or CLI behavior changes.

## Internal choices

- The requirement says "adds", not "carries": a phase's output carries elastic trivia only where it keeps elastic trivia from its input. The alternative was "no phase output carries any", which the issue proposes. For the node-level entry points, that would force the phases to strip elastic trivia from the nodes code fixes pass in, which changes code-fix spacing that no case asks about. For the document entry points the two readings are equivalent.
- Elastic trivia that a node-level input already carries is listed as a non-goal in `pipeline.md`. The alternative was to decide keep-or-drop now.
- The rule lives once in the pipeline spec, and FMT-STRUCT-026 points to it. The alternative was a sentence in each phase spec.
- FMT-BREAK-031's "recorded exception" (the space the shared merge writes) is not touched. That space is not elastic, so FMT-PIPE-034 leaves it alone. The change doc records that the phase's merge and the code fixes' merge may now differ in what they write in place of an elastic comma, provided SHARED-ATTR-018's output stays.

## Follow-up candidates

_None._
