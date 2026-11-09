Role: spec checker
Commit: 4596aef
Verdict: consistent

Repair round 1. The prompt carried no intended fix or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| — | — | — | none | — |

## Checked
1. Change doc: ok. `sdd/changes/2026-11/conditional-group-fix-refusals.md` stays `Accepted`, has `Visible output changes`, a new `Rare-class withholdings` section outside it (Decision #14), and `Decisions`; no task list. The three visible changes keep the maintainer answers 1A, 2A, 3A; the round adds no new visible output change (Decision #12 keeps `main`'s output, #13 and #15 are demanded by SHARED-FIX-003).
2. Specs match the diff: ok. RH2001-010 extended (no-group-inside condition, group behind a property without an initializer), RH2001-004 and SHARED-FIX-009 clarified, SHARED-ORDER-008 reads a declaration that ends in `;`; the production diff (`OrderingDeclarationUtilities`, RH2001 code fix) matches these, with examples carrying markup and `Why:`.
3. IDs: ok. No ID added; `requirement-ids.txt` and `sdd/specs/README.md` unchanged and correct.
4. Code owners: ok. No owner changed; `OrderingDeclarationUtilities` is still listed.
5. User documentation: ok. No rule page touched or affected (withheld fixes are covered by `general-notes.md`).
6. Open questions: ok. This round has none; A6 remains under `Follow-up candidates` in `00-lead.md`.
7. Coverage: ok. `scripts/test.sh --project architecture`: Passed! - Failed: 0, Passed: 117, Skipped: 0, Total: 117. RH2001-010 and SHARED-ORDER-008 stay covered by tagged tests in the diff.
8. Tags: ok. All new tests carry `[Requirement]` for the requirements in the test implementer's table (RH2001-010, RH2001-004, SHARED-FIX-002, SHARED-ORDER-008, RH7109-006); the renamed test keeps `RH2001-010`; every requirement named under the spec author's `Examples for the tests` is tagged on a test.
9. Nothing left behind: ok. `sdd/work/` holds only this run's directory (the lead deletes it before publishing).
