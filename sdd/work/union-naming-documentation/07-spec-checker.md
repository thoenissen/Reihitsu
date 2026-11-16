Role: spec checker
Commit: c3121de
Verdict: consistent

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
_None._

## Checked
1. Change doc: ok. `sdd/changes/2026-11/union-naming-documentation.md` exists, no task list, has `Visible output changes`, status `Accepted`, and `Decisions` records the maintainer's answers 1A to 5A.
2. Specs match the diff: ok. RH4012, RH8035, RH8036 follow the format; RH8030, RH8108 to RH8111, RH8203, SHARED-DOC-001/002 and the non-goals of RH4003, RH8003, RH8004, RH8101 to RH8104 agree with `DocumentableDeclarationKinds` and `TypeParameterOwnerKinds` gaining `UnionDeclaration` and `ParameterOwnerKinds` staying unchanged.
3. IDs: ok. RH4012-001/002, RH8035-001/002, RH8036-001/002 are inserted at their sorted positions in `requirement-ids.txt`; nothing renumbered or removed.
4. Code owners: ok. `scripts/test.sh --project architecture`: `Passed! - Failed: 0, Passed: 118, Skipped: 0, Total: 118` (includes SpecConsistencyTests). `sdd/specs/README.md` lists the three new specs with matching owners.
5. User documentation: ok. RH4012, RH8035, RH8036 pages follow the format, name no formatter, and end with the footer; the RH4012 collision sentence is the same text RH4003/RH4010/RH4011 carry. RH8030 page names RH8035/RH8036. Release.ruleset, Reihitsu.sln, and the package README carry the new rules; ID ranges fit the categories.
6. Open questions: ok. The skeptic's handover has no open output question; questions 1 to 5 are answered under `Maintainer answers`; the one follow-up candidate (RH5404 comment, belongs to a layout issue) is under `Follow-up candidates`.
7. Coverage: ok. Every new active requirement (RH4012-001/002, RH8035-001/002, RH8036-001/002) and every extended one (RH8030-001/002, RH8108-001/002, RH8109-001/002, RH8110-002, RH8111-001/002, RH8203-001/002) has a tagged test in the diff.
8. Tags: ok. Each added test carries the `[Requirement]` attributes its row in `04-test-implementer.md` lists (checked per method); every requirement named under `Examples for the tests` is tagged on at least one added test. The untagged RH4105 and RH4122 characterization tests verify no requirement.
9. Leftovers: ok. `sdd/work/` holds only `union-naming-documentation`.
