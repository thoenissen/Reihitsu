Role: spec checker
Commit: (this commit; see `git log -1 -- sdd/work/code-fix-scope-line-endings/08-spec-checker.md`)
Verdict: consistent

The prompt carried no intended fix, suspected cause, or hoped-for verdict. This handover states none.

Repair of finding 1 in `06-spec-checker.md`: verified. `ExpressionBodyCodeFixUtilities` is listed as a code owner in `sdd/specs/shared/expression-body-conversion.md` and in its row of `sdd/specs/README.md`. The type exists in `src/Reihitsu.Analyzer.CodeFixes/Core/`. The repair commits (3dab93e) touch only these two spec files; no code or test changed since 06, so the earlier checks still hold.

`scripts/test.sh --project architecture`: `Passed!  - Failed: 0, Passed: 117, Skipped: 0, Total: 117`.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| - | - | - | None. | - |

## thoenissen/Reihitsu#1283
No spec change, none needed: SHARED-FIX-006, SHARED-FIX-008, and RH3006-003 already demand the output. No change doc (bug fix). Added RH3006 tests carry `[Requirement]` tags matching the test implementer's table. The rule page is unaffected. The RH3003 and RH3005 hazards are under `Follow-up candidates` in `00-lead.md`.

## thoenissen/Reihitsu#1284
Clarification of SHARED-BODY-003 and the RH3202 intro; no ID added, no change doc needed. Owners now complete. Added tests for RH3202, RH3203, RH3205 to RH3208 carry tags, SHARED-BODY-003 among them. The rule page is unchanged and still true.

## thoenissen/Reihitsu#1285
Change doc `sdd/changes/2026-11/rh3001-fix-scope.md` is required, `Accepted`, has `Visible output changes`, and records the maintainer answers in `Decisions`. RH3001-008 is the next free ID at its sorted position in `requirement-ids.txt`; no ID reused or removed. RH3001-004 and -008 follow the format. RH3001-008 is covered by tagged tests. The rule page `RH3001.md` is unchanged and still true.

## Checked
1. Change doc exactly when required: ok.
2. Specs match the diff: ok.
3. IDs: ok.
4. Code owners exist: ok (finding 1 of 06 repaired).
5. User documentation: ok (no rule page affected).
6. Open questions routed: ok.
7. Requirements covered, specs consistent: ok (117 passed, 0 failed).
8. Tests carry their requirement's tag: ok.
9. Nothing left behind: ok for now; `sdd/work/` holds only this run's folder, which the lead deletes before publishing.
