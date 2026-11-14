Role: spec checker
Commit: (this commit; see `git log -1 -- sdd/work/code-fix-scope-line-endings/06-spec-checker.md`)
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict. This handover states none.

`scripts/test.sh --project architecture`: `Passed!  - Failed: 0, Passed: 117, Skipped: 0, Total: 117`.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 4 | `sdd/specs/shared/expression-body-conversion.md` (header, `Code owners`) and its row in `sdd/specs/README.md` | The diff adds the internal type `Reihitsu.Analyzer.CodeFixes.Core.ExpressionBodyCodeFixUtilities`, which `04-implementer.md` names as the one owner of the SHARED-BODY-003 write-back (only the block body is written back). Neither the spec's owners nor the README row list it. `SpecConsistencyTests` passes because it checks only that listed owners exist. | spec author |

## thoenissen/Reihitsu#1283
No spec change was made, as `01-spec-author.md` states: SHARED-FIX-006, SHARED-FIX-008, and RH3006-003 already demand the output. No change doc is needed (bug fix). The diff touches only the RH3006 provider and tests. Each added RH3006 test carries a `[Requirement]` tag; the table in `03-test-implementer.md` names SHARED-FIX-006, SHARED-FIX-008, and RH3006-003, and the tags in the file match (RH3006-003 on 14 tests, SHARED-FIX-008 on 1). RH3006 documentation page is not affected. The RH3003 and RH3005 hazards are under `Follow-up candidates` in `00-lead.md`.

## thoenissen/Reihitsu#1284
Clarification: SHARED-BODY-003 (title and text) and the RH3202 spec intro, plus a new example in SHARED-BODY-003; no ID added, no change doc needed. The text matches the diff (only the body is replaced; the rest of the member stays). `Uses` gained SHARED-FIX-006 and SHARED-FIX-007. Finding 1 applies. All added RH3202, RH3203, RH3205 to RH3208 tests carry `[Requirement]` tags; SHARED-BODY-003 appears on tests of all six rules, and the spec's example (`FixKeepsTheSpacingOfTheSignature`) is tagged. RH3202 rule documentation is unchanged and still true (it describes the conversion, no layout claim).

## thoenissen/Reihitsu#1285
- Change doc `sdd/changes/2026-11/rh3001-fix-scope.md`: required (RH3001-004 changed, RH3001-008 added, visible output changes), status `Accepted`, has `Visible output changes` and no task list. Maintainer answers are recorded in `Decisions` (Q2 and Q3 from rounds 7a and 7b, the `orderby !a` answer from this run) and in `00-lead.md` `Maintainer answers`.
- RH3001-008 is the next free ID and sits at its sorted position in `requirement-ids.txt`; no ID reused or removed.
- RH3001-004 and RH3001-008 follow the requirement format (normative sentence, `Why:`, examples, `{|RH3001:…|}` markup).
- Coverage: RH3001-008 is covered by 10 tagged tests; every added RH3001 test is tagged. The architecture project passes, including `RequirementCoverageTests`.
- Rule page `documentation/rules/RH3001.md` is unchanged. Its Correction (`isValid == false`) still equals the fix output, and it makes no claim the new behavior contradicts. No finding.

## Checked
1. Change doc exactly when required: ok (only #1285; #1283 and #1284 are bug fix/clarification; Accepted; decisions recorded).
2. Specs match the diff: ok.
3. IDs: ok.
4. Code owners exist: finding 1 (new owner type not listed).
5. User documentation: ok (no rule page affected).
6. Open questions routed: ok (the only output question, `orderby !a`, answered in `00-lead.md`; RH3003 and RH3005 under `Follow-up candidates`).
7. Requirements covered, specs consistent: ok (117 passed, 0 failed).
8. Tests carry their requirement's tag: ok (all added test methods carry `[Requirement]`; the IDs in the table match).
9. Nothing left behind: ok for now; `sdd/work/` holds only this run's folder, which the lead deletes before publishing.
