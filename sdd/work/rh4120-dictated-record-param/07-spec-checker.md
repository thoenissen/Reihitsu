Role: spec checker
Commit: 42165dd
Verdict: consistent

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## Checked
1. Change doc: ok. Required (SHARED-CASING-014 and RH4120-001 change behavior); `sdd/changes/2026-11/record-parameter-dictated-name.md` is Accepted, has no task list, has `Visible output changes`; answers A, 1A, 2B are in its `Decisions` and in `00-lead.md`.
2. Specs match the diff: ok. The record-parameter bullet, the boundary sentence, and the example in SHARED-CASING-014 match the `IsDictatedName` override and `GetMember`; RH4120-001 widening describes existing behavior. Format ok (Why present, markup `{|RH####:...|}`).
3. IDs: ok. No new, renamed, or retired requirement; `requirement-ids.txt` untouched.
4. Code owners: ok. No type added or renamed (`GetMember` is a method on an existing class); RH4120 `Uses` gains SHARED-CASING-014. `scripts/test.sh --project architecture`: Passed: 198, Failed: 0.
5. User documentation: ok. No rule page change, as the change doc states (exception, not the normal case); no resource text.
6. Open questions: ok. Spec author and skeptic output questions are answered under `Maintainer answers`; follow-up candidates are none.
7. Coverage: ok. SHARED-CASING-014's new bullet is covered by 14 new tests tagged `[Requirement("SHARED-CASING-014")]`, and the RH4120-001 `B`/`D`/`P` example by the two existing tests newly tagged; architecture result quoted above.
8. Tags: ok. All 16 tests in the test implementer's table carry the tags the table names (including `RH4120-003` on the fix test); every requirement named under `Examples for the tests` is tagged on at least one test.
9. Leftovers: ok. `sdd/work/` holds only `rh4120-dictated-record-param`, which the lead deletes before publishing.
