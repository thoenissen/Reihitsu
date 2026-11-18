Role: spec checker
Commit: 41a695e
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict. The diff checked is `origin/main...HEAD`.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 2 | `sdd/changes/2026-11/attached-part-below-element.md` lines 304, 344, 362, 408, 426, 450, 478 | The CLI lines in the examples start with `Error:`. The CLI prints `error:` in lower case (`FormatCommandHandler.ReportUnsupported`, `severity = "error"`). Lead decision 6 asks for the correction. | spec author |
| 2 | 4 | `sdd/specs/formatter/line-breaks.md` and `sdd/specs/formatter/unsupported-syntax.md` headers, `sdd/specs/README.md` rows | The diff adds `AttachedPartGapUtilities` (`src/Reihitsu.Formatter/Pipeline/Core/Utilities/AttachedPartGapUtilities.cs`), the one definition of the attached-part gap that FMT-BREAK-039/040 (join) and FMT-UNSUP-014 (precheck) share. It is named in no spec header and no README row. `AttachedPartLineStartRewriter` is correctly in the line-breaks owners and README. | spec author |
| 3 | 2 | `sdd/changes/2026-11/attached-part-below-element.md` line 627 (*Special cases / Deleted*) | It names `ArgumentListLineStartRewriter.CanJoin`. The diff renames that type to `AttachedPartLineStartRewriter`, and the old name no longer exists in the tree. Name the deletion by the current owner, or say "formerly". | spec author |

## Checked
1. Change doc: ok. `attached-part-below-element.md` exists, Status Accepted, has `Visible output changes` and `Decisions`. The Decisions section records the maintainer's batch 1 answers (1A to 5A) and the batch 2 answers (6.1A to 6.3A, 1A, 2A). The behavior change (FMT-BREAK-040 new, FMT-UNSUP-014 new, FMT-BREAK-020/-015/-039 and FMT-SPACE-028 changed) justifies it, and there is no task list.
2. Specs match the diff: findings 1 and 3. Otherwise ok. FMT-BREAK-040 and FMT-UNSUP-014 follow the format (normative sentence, `Why:`, examples). FMT-BREAK-015/-019-adjacent/-020/-039, FMT-SPACE-028, RH2001-007 and RH7101-007 are consistent with the new behavior. The AGENTS.md `PrecheckPhase` and `LineBreakPhase` rows are updated (`AttachedPartGapUtilities.IsAttachedPartGap`, FMT-UNSUP-014).
3. IDs: ok. FMT-BREAK-040 and FMT-UNSUP-014 are the next free numbers and sit in sorted position in `requirement-ids.txt`. Nothing is reused, renumbered, or removed.
4. Code owners: finding 2. `scripts/test.sh --project architecture` is green, so every named owner exists. `ArgumentListLineStartRewriter` is renamed in the line-breaks header and README row and no stale mention remains in specs. `AttachedPartGapUtilities` is unlisted (finding 2).
5. User documentation: ok. No rule page changes. RH2001, RH7101, RH3202 and RH5112 pages carry no list of withheld fixes, and `general-notes.md` already covers comments blocking a fix. The diagnostic ID ranges are unchanged.
6. Open questions: ok. 05-skeptic's and 06-spec-author's questions are answered under `Maintainer answers` in `00-lead.md`. 06 states none open. Follow-up candidates are listed in `00-lead.md`.
7. Coverage: ok. `scripts/test.sh --project architecture` gave `Passed! - Failed: 0, Passed: 198, Skipped: 0, Total: 198`. Every new active requirement has covering `[Requirement]` tests: FMT-BREAK-040 (`AttachedPartLineStartTests`, `ChainLinkLayoutTests`, `ChainGapJoinTests`, RH5302 tests), FMT-UNSUP-014 (`UnsupportedAttachedPartCommentTests`, the CLI end-to-end test, and the formatter and analyzer tests).
8. Tags: ok. The test implementer's table (07) matches the tags the diff adds. Every requirement named under `Examples for the tests` in 04 and 06 (FMT-UNSUP-014, FMT-BREAK-039, FMT-BREAK-040, FMT-SPACE-028, RH2001-007, RH7101-007, SHARED-FIX-010, the RH5112 fix) is tagged on at least one test the diff adds or changes.
9. Nothing left behind: ok for now. `sdd/work/` holds only `join-attached-parts`, which the lead deletes before publishing.
