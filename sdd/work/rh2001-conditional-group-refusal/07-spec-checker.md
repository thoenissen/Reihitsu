Role: spec checker
Commit: c6ff405 (audited head; this handover adds no code)
Verdict: consistent

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## Checked
1. ok. RH2001-010 changes behavior, so a change doc is required; `sdd/changes/2026-11/rh2001-conditional-group-refusal.md` is `Accepted`, has no task list, has `Visible output changes` (sections 1-3, the third added after the skeptic), and `Decisions` records maintainer answer A (issue comment 2026-11-22).
2. ok. RH2001-010 (changed), RH2001-004, RH2001-009, SHARED-FIX-009 and SHARED-FIX-011-area text (clarified) match the diff: the fix reads the leading trivia of the first modifier and of the token behind a property without an initializer for `#if`/`#elif`/`#else`/`#endif`, and no excluded branch is parsed. Format kept (normative sentence first, `Why:`, markup examples). No stale references to the deleted helpers remain in specs or docs.
3. ok. No new ID; `RH2001-010` stays in `sdd/specs/requirement-ids.txt`, nothing renumbered or deleted.
4. ok. Code owners of RH2001 unchanged and existing; `sdd/specs/README.md` row matches. `scripts/test.sh --project architecture`: Passed! Failed 0, Passed 198, Skipped 0, Total 198 (includes SpecConsistencyTests).
5. ok. `documentation/rules/RH2001.md` untouched; a withheld fix is covered by `general-notes.md`.
6. ok. No open output questions in the spec author's or skeptic's handovers; the only one is answered under `Maintainer answers`. No follow-up candidates outside the run.
7. ok. Every changed or new RH2001-010 example has a `[Requirement("RH2001-010")]` test in the diff; architecture tests pass.
8. ok. All 16 test methods the diff adds or flips carry `RH2001-010` (the two Fix All tests also `SHARED-FIX-002`), matching the test implementer's table. Every example under `Examples for the tests` (six flipped tests, `#endif`-alone, earlier member, whole-type wrap, plus the wraps-the-property pair) is tagged on a test in the diff.
9. ok for now. `sdd/work/` holds only this run's folder `rh2001-conditional-group-refusal`; the lead deletes it before publishing.
