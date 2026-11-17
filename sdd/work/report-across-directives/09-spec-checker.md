Role: spec checker
Commit: 41460ba
Verdict: consistent

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## Checked
1. ok. The change doc `sdd/changes/2026-11/report-across-conditional-directives.md` is required (requirements changed with new reported behavior), is `Accepted`, has `Visible output changes`, and has no task list. Its `Decisions` section records maintainer answers 1A to 6A of batch 1, which `00-lead.md` also holds.
2. ok. The changed requirements (SHARED-DIAG-008, -010, SHARED-TRAIL-002, SHARED-ATTR-019/020, SHARED-EMPTY-001, SHARED-MODORDER-003, RH5102-004, RH5109-004, RH5111-005, RH7101-001, RH7102-001/006, RH7103-001, RH7105-001, RH7106-001, RH7107-001/005, RH7108-001/005, RH7311-001, RH8201-003, and the clarifications RH2001/2005/3002/7003) match the analyzer and code fix diff. A grep over `sdd/specs` for the old silence ("not reported" or "keeps ... from being reported" with a conditional directive) finds no contradicting requirement. RH5408-003 (any directive blocks the join) and SHARED-USING-009 are outside the issue's list and are already follow-up candidates or unchanged.
3. ok. No ID added, retired, or renumbered; `requirement-ids.txt` is untouched.
4. ok. No spec header owner changed. `TrailingCommaAnalyzerBase<TNode>`, `TrailingCommaRemovalCodeFixProviderBase`, `TrailingCommaCodeFixHelper`, and the RH8201 owners exist, and `sdd/specs/README.md` lists them. `scripts/test.sh --project architecture`: `Passed! - Failed: 0, Passed: 185, Skipped: 0, Total: 185`.
5. ok. The "Exceptions" sections that stated the old silence are removed from RH5409, RH5410, RH5411, and RH8201. No other rule page of the affected rules mentions the silence (grep of RH5102, 5109, 5111, 5412, 7101 to 7108, 7311). The new paragraph in `general-notes.md` is shared behavior and names no formatter. Each Correction is unchanged, because no fix output changed.
6. ok. All six output questions have answers under `Maintainer answers`. Follow-up candidates sit under `Follow-up candidates` in `00-lead.md`. No question is written into a spec.
7. ok. No new requirement, so no new coverage is needed. Every changed requirement has a covering `[Requirement]` test in the diff. Every requirement the spec author named under `Examples for the tests` (01, 03, 06) is tagged on at least one added or newly tagged test, including SHARED-EMPTY-001 (RH5412 test), RH2005-001, RH7003-001, and SHARED-ATTR-019/020.
8. ok. A script over `HEAD` against `origin/main` listed 114 test methods added or retagged. Every one carries at least one `[Requirement]`, and the tags equal the Tests table in `04-test-implementer.md`. The test implementer's repair round (07) changed only markup handling and the RH7102 example, and no tag changed.
9. ok. The only `sdd/work/` directory on the branch is this run's (`report-across-directives`). The lead deletes it before publishing.
