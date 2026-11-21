Role: spec checker
Commit: dd21564a
Verdict: consistent

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## Checked
1. Change doc: ok. `sdd/changes/2026-11/unsupported-link-dot-name-trivia.md` is required (new FMT-UNSUP-016, visible output change), Accepted, has no task list, has `Visible output changes` and `Decisions` holding the maintainer's answers (pre-run, batch 1 Q1 A, Q2 A).
2. Specs match the diff: ok. FMT-UNSUP-016 follows the format (normative sentence, `Why:`, examples both sides); FMT-UNSUP-015, FMT-BREAK-020, RH2001-007, RH7101-007 updated and consistent with `PrecheckPhase`/`ChainLineBreakRewriter` changes; the `Uses` rows (unsupported-syntax, line-breaks, RH2001, RH7101) include FMT-UNSUP-016; the AGENTS.md precheck row lists it as the tenth shape.
3. IDs: ok. FMT-UNSUP-016 is the next free number, in sorted position in `requirement-ids.txt`; nothing reused or removed.
4. Code owners: ok. No owner type renamed, added, or removed; `scripts/test.sh --project architecture` (includes SpecConsistencyTests).
5. User documentation: ok. No rule page is affected (no rule added or changed; fixes withheld per `general-notes.md`).
6. Open questions: ok. Both output questions answered in `00-lead.md`; the qualified-name candidate is under `Follow-up candidates`; none written into specs.
7. Coverage: ok. `scripts/test.sh --project architecture`: Passed! - Failed: 0, Passed: 198, Skipped: 0, Total: 198. FMT-UNSUP-016 is covered by many `[Requirement]` tests.
8. Test tags: ok. New and rewritten tests carry the requirements their handover tables (04, 07) list (FMT-UNSUP-016 plus -001/-003/-004/-005/-010/-015, SHARED-FIX-010, RH2001-007, RH7101-007, FMT-BREAK-020, FMT-INDENT-029); every requirement under the spec author's `Examples for the tests` is tagged on at least one new test.
9. Nothing left behind: ok. `sdd/work/` holds only `link-dot-name-trivia`.
