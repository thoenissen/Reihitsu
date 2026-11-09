Role: spec checker
Commit: 42abff1
Verdict: consistent

The prompt carried no intended fix or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| — | — | — | none | — |

## Checked
1. Change doc: ok. `sdd/changes/2026-11/conditional-group-fix-refusals.md` exists for a new requirement (RH2001-010) and changed ones, has `Visible output changes`, status `Accepted`, no task list; the three visible changes have the maintainer's answers (1A, 2A, 3A) in `Maintainer answers` of `00-lead.md` and in its `Decisions` section.
2. Specs match the diff: ok. RH2001-010 added; RH2001-004/-009, SHARED-ORDER-007/-008, SHARED-FIX-009 changed; each has the normative sentence, `Why:`, and examples with markup; the A5 amendment and the A6 gap are written down as decisions.
3. IDs: ok. `RH2001-010` added once, at its sorted position; nothing reused, renumbered, or deleted.
4. Code owners: ok. No owner changed; `OrderingDeclarationUtilities` stays an owner of ordering-move-safety and is listed in `sdd/specs/README.md`. `RH2001-010` is added to `Uses` of `shared/code-fixes.md`.
5. User documentation: ok. No rule page affected (the behavior is a withheld fix, covered by `general-notes.md`); `documentation/rules/RH2001.md` exists and is untouched.
6. Open questions: ok. The three output questions are answered in `00-lead.md`; A5 is decided (Decisions #5, #9); A6 is under `Follow-up candidates` and recorded in the spec as a decided gap; nothing is only in the PR. (Lead Decisions #3, #5, #6, #8 show commit `pending`; cosmetic for the lead to fill.)
7. Coverage: ok. `scripts/test.sh --project architecture`: Passed! - Failed: 0, Passed: 117, Skipped: 0, Total: 117. RH2001-010 is covered by 15 tagged tests.
8. Tags: ok. All 51 test methods the diff adds appear in the test implementer's `Tests` table. The new tests carry `[Requirement]` for RH2001-010 (and -004), SHARED-ORDER-008 with RH7109-006/RH7103-007/RH7102-006/RH7309-006, and SHARED-FIX-002 for Fix All. Every requirement named under `Examples for the tests` (RH2001-010, SHARED-ORDER-008 examples incl. m06, RH7309 per consumer) is tagged on a test.
9. Leftovers: ok. `sdd/work/` holds only `conditional-group-fix-refusals`, which the lead deletes before publishing.
