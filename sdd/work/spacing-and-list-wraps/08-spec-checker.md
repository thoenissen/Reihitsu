Role: spec checker
Commit: e955c4f
Verdict: findings

My prompt carried no intended fix, suspected cause, or hoped-for verdict. Checked against `git diff 8a26419...HEAD` (merge base with origin/main).

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 1 | `sdd/changes/2026-11/list-wrap-by-elements.md` line 5 | `Status` is `Draft`. The spec author's handover and the lead's worklist say the change doc is accepted, and every entry has an answer or a recorded decision (entries 1–5 maintainer answers, 6–11 decisions 8, 10, 13), so it is `Accepted` in substance. The lead sets `Accepted` before the PR is marked ready (`sdd/README.md`, change docs) | lead |
| 2 | 1, 2 | `sdd/changes/2026-11/list-wrap-by-elements.md` line 29 (*Visible output changes* intro) | States "the code to produce them is not written yet". The implementation is in (c119a8a) and the docs's "after" outputs are now what the code produces; the sentence is stale | spec author |

## Checked
1. Change doc exactly when required: finding 1 (Status). A change doc exists for #1635 (requirement changed: FMT-BREAK-012, FMT-BREAK-011, one non-goal); none for #1642, correct (no spec change, characterization tests only). Has `Visible output changes` and no task list; every entry has the maintainer's answer or a lead decision under `Decisions`.
2. Specs match the diff: finding 2 only. FMT-BREAK-012/011 text and examples match `LineBreakListRewriter` (shared `CollapseSeparatorsToPreviousElementLine` for argument, attribute argument, parameter, and bracketed parameter lists; indexer-only opener signal deleted; leading-comma separator counts as a line break; held gap left as written). Format ok (normative sentence, `Why:`, examples). No contradicting requirement found; the `Uses` header already holds SHARED-BLANK-008 and FMT-PIPE-014.
3. IDs: ok. No new ID; `requirement-ids.txt` and `sdd/specs/README.md` are untouched and need no change.
4. Code owners: ok. Header unchanged; `LineBreakListRewriter` is already an owner and no type was added, renamed, or removed. Covered by SpecConsistencyTests inside the architecture run.
5. User documentation: ok. No rule page changed; RH5101, RH5102, RH5107, RH5109 report the same inputs as before, and their fix results follow the formatter (RH5101-003, RH5102-003). No `documentation/` file in the diff. (RH5109-003 vs FMT-BREAK-012 is pre-existing and already under Follow-up candidates.)
6. Open questions routed: ok. Output questions of 01 and 03 are answered under `Maintainer answers` or decided in `00-lead.md` (decisions 8, 10–13); the four outside-the-issue items are under `Follow-up candidates`; the spec has no open question and the PR is not the only record.
7. Requirements covered, specs consistent: `scripts/test.sh --project architecture` result line: `Passed!  - Failed: 0, Passed: 118, Skipped: 0, Total: 118`. FMT-BREAK-012 (changed, no new requirement) is covered by 31 `[Requirement("FMT-BREAK-012")]` tests in `ListWrapByElementsTests` plus the updated indexer, parameter, analyzer tests. ok.
8. Tags: ok. `ListWrapByElementsTests`: 31 test methods, 31 FMT-BREAK-012 tags and 4 FMT-BREAK-011 tags, matching the table of 04 (N, O, indexer, lambda); the added `MultiLineCommentGapSpacingTests` carry FMT-SPACE-001 and FMT-SPACE-013; the new RH5101/RH5102 fix tests carry RH5101-003/RH5102-003 plus FMT-BREAK-012, and the not-reported test RH5102-001; the retagged tests of 06 (indexer, `CommaAloneOnItsLineStaysAligned`, `CommaAfterDirectiveHeldFirstParameterStaysAligned`) carry FMT-BREAK-012. Every example of the amended FMT-BREAK-012 (all five, `P`, the `#if X` list with and without `X`) is tagged on a test.
9. Nothing left behind: ok. `sdd/work/` holds only `spacing-and-list-wraps`; the lead deletes it before publishing.
