Role: spec checker
Commit: f5599cd
Verdict: consistent

My prompt carried no intended fix, suspected cause, or hoped-for verdict.

Scope: `git diff 894256a HEAD -- . ':(exclude)sdd/work'`: `sdd/specs/shared/code-fixes.md`, `squad/decisions/2026-10-19-audit-repair-doc-layout-followups.md`, `RH4106PrivateFieldCasingAnalyzerTests.cs`, `RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzerTests.cs`.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## Checked
1. Change doc: ok. The repair changes one example of SHARED-FIX-003 (an input and a result block). The normative text and the bounds are unchanged, so it is a clarification and needs no change doc. The diff adds none.
2. Specs match the diff: ok.
   - The RH7103 example's input and result block equal the test constants `CrossesOnlyAnInstanceFieldTestCode` and `CrossesOnlyAnInstanceFieldFixedCode`.
   - The `_b` no-action test uses the example's input, with `_a` and `_b` marked.
   - The result block carries `{|RH7103:_b|}` and the prose names it as the result of the fix for `_a`, as the format requires for a block that mixes diagnostics with and without a fix.
   - The RH4106 compile-error example (class `C`, `Value` and `_value`) equals the input of the tagged `NoFixIsOfferedWhenTheNewNameIsTaken`.
3. IDs: ok. `git diff 894256a HEAD --stat -- sdd/specs` shows only `code-fixes.md`. The ledger is untouched and no requirement was added or retired.
4. Code owners: ok. The spec header is unchanged and `SpecConsistencyTests` pass.
5. User documentation: ok. No rule page is affected by an example-only change. The RH7103 and RH2005 non-goal pages are follow-up thoenissen/Reihitsu#1137, decided in the 2026-10-19 decision (answer 3).
6. Open questions: ok. The spec author's handover (`15`) lists none. The decision's follow-ups are issues thoenissen/Reihitsu#1134 to #1137. I read #1136 and its text matches decision answer 3, including disabled text.
7. Coverage and consistency: ok.
   - `scripts/test.sh --project architecture`: "Passed! - Failed: 0, Passed: 72, Skipped: 0, Total: 72".
   - No new requirement was added. The SHARED-FIX-003 tests keep their `[Requirement]` tags. The removed RH4106 duplicate's tag moved to `NoFixIsOfferedWhenTheNewNameIsTaken`, which now carries SHARED-CASING-009, RH4106-006 and SHARED-FIX-003.
   - `scripts/test.sh --project analyzer --filter "...RH7103...|...RH4106..."`: "Passed! - Failed: 0, Passed: 59, Skipped: 0, Total: 59".
8. Nothing left behind: ok. `sdd/work/` holds only `shared-fix-safety/`, which the lead deletes before publishing.

## Notes (not findings)
- The new decision file records the maintainer's answers: one more repair, doc-comment layout confirmed, and four follow-ups created (thoenissen/Reihitsu#1134, #1135, #1136, #1137).
- The repaired RH7103 test asserts exact text (`HasCount(1)` plus `AreEqual`). Its LF and CRLF variants share one helper.
