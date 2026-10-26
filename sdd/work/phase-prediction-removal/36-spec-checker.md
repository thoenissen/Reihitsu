Role: spec checker
Commit: e108e6e
Verdict: consistent

The prompt carried no intended fix, no suspected cause, and no verdict anyone hoped for. Repair diff checked: `git diff 504310f` on e108e6e, with handovers 28 to 35, the last two sections of `00-lead.md`, and `squad/decisions/2026-10-26-phase-prediction-removal-*.md`. The verdicts of `14-`, `16-`, and `27-spec-checker.md` still cover everything this repair did not touch.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|

None. Two observations, not findings, are listed at the end.

## Checked
1. Change doc exactly when required: ok. `sdd/changes/2026-10/phase-prediction-removal.md` stays `Accepted`, has no task list, and now lists FMT-PIPE-018 in Affected requirements. The new sentence and the "change no line's column" wording are approved by the decisions `audit-2` (#8 option A) and `context-blank-lines` (1A, 2A), taken under the maintainer's standing instruction.
2. Specs match the diff: ok. FMT-PIPE-018 and the reworded FMT-PIPE-017 match `ReihitsuFormatter.FormatNodeInDocumentWithContextAsync` and `ReihitsuFormatterHelpers.RestoreBlankLines`: the target's original leading trivia is written back only where FMT-PIPE-010 keeps it; for a target that starts its line the document's blank lines stay, none is added or removed, and comments, directives, and the first token take the phase's columns. The `AGENTS.md` sentence ("changes a line's column") matches FMT-PIPE-017 and the code, which still adds or removes whole blank lines after the last phase. The removal of the line-offset mapping in `IndentationRewriter` and the comment edit in `IndentationPhase` are internal and covered by FMT-PIPE-017 and FMT-RAW-001. The `RestoreBlankLines` fallback for a changed trivia item set is unreachable according to `31-implementer.md` and `32-test-implementer.md`, so the spec needs no sentence for it. The question of `FormatNodeInDocumentAsync` blank lines is left undecided in the spec, as the decision records.
3. IDs: ok. FMT-PIPE-018 is the next free number and sits at its sorted ledger position; no ID was reused, renumbered, or deleted.
4. Code owners exist: ok. `SpecConsistencyTests` passes inside the architecture run. The diff adds no production type and the `Code owners` row of `pipeline.md` is unchanged and right. `Uses` of `pipeline.md` lists FMT-BLANK-011, FMT-BLANK-013, and FMT-BLANK-014, which FMT-PIPE-018 cites.
5. User documentation: ok. The diff touches no rule page and no rule's behavior or fix availability.
6. Open questions: ok. Q1 of `33-spec-author.md` (blank lines in front of a `FormatNodeInDocumentAsync` target) is not blocking, is decided as a follow-up candidate in `context-blank-lines`, and is written into no spec. It is for the lead's closing message together with the earlier follow-ups (public `DocumentationCommentUtilities.ShiftContinuationExteriorMarkers` without a production caller, per `audit-2` #10).
7. Requirements covered, specs consistent: ok. `scripts/test.sh --project architecture`: `Passed!  - Failed:     0, Passed:    89, Skipped:     0, Total:    89`. FMT-PIPE-018 has covering tests.
8. New tests carry their requirement's tag: ok. By script, all 25 test methods the diff adds appear in handovers 29, 32, or 34, and every row's listed requirement tags are on the method (no missing tag). The tests that carry FMT-PIPE-018 are the eight `ReihitsuFormatterNodeColumnTests` and ten RH5111 tests named in `34-test-implementer.md`, plus its two new spec-example tests and the qualifier test; the two RH5111 tests `VerifyCommentInFrontOf…DoesNotWithholdTheFix` are newly tagged FMT-PIPE-018 and appear in the `34-` table. Every requirement under the spec author's `Examples for the tests` (FMT-PIPE-018, with FMT-PIPE-017) is tagged on a test the diff adds or newly tags.
9. Nothing left behind: ok. `sdd/work/` holds only `phase-prediction-removal`, which the lead deletes before publishing.

## Observations, not findings
- The change doc says "Three new pipeline requirements state what this guarantees" (FMT-PIPE-015 to FMT-PIPE-017); with FMT-PIPE-018 the run adds four. FMT-PIPE-018 records kept behavior and is named in the node-level bullet and in Affected requirements, so the count is the only loose end.
- `squad/failure-classes.md` still names `RawStringAlignmentPhase` and FMT-CLEAN-002 as history (carried over from `14-spec-checker.md`); the diff does not touch it.
