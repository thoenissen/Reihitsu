Role: spec checker
Commit: 3c7b8c7
Verdict: consistent

My prompt carried no intended fix or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| - | - | - | none | - |

## Checked
1. Change doc: ok. `sdd/changes/2026-11/rh4001-rh4009-names.md` is Accepted and has `Visible output changes`. It has no task list. Changes 1 and 2 have the maintainer's answers in `Decisions`, matching `00-lead.md` answers 1-2.
2. Specs match the diff: ok. RH4001-008 and RH4009-003 are added in the format (normative sentence, `Why:`, examples). The RH4001-006 example follows the prefixed new name, which is what `GetRenamedFileName` now produces.
3. IDs: ok. RH4001-008 and RH4009-003 are the next free numbers, inserted in sorted position. Nothing is reused or removed.
4. Code owners: ok. The owner of RH4009 is unchanged and exists. Architecture run: `Passed! - Failed: 0, Passed: 198, Skipped: 0, Total: 198`.
5. User docs: ok. Both rule pages gain one behavior sentence each, with no formatter mention and no withheld-fix note. The RH4001 page has no `### Correction` section for the prefix example, so none can differ from the fix output. The IDs fit the Naming range.
6. Open questions: ok. The skeptic and spec author handovers have no output questions, and there are no follow-up candidates.
7. Coverage: ok. The architecture suite, including `EveryRequirementOfAnEnforcedSpecIsCovered`, passes. RH4001-008 and RH4009-003 each have tagged tests.
8. Tags: ok. Every added test carries `[Requirement]` for the requirements its table row lists. All examples named by the spec author are tagged (RH4001-006, RH4001-008, RH4009-003).
9. Nothing left behind: ok for now. `sdd/work/` holds only this run's directory, which the lead deletes before publishing.
