Role: spec checker
Commit: af54bb9
Verdict: consistent

Repair round 1, re-check, both issues (thoenissen/Reihitsu#849 and #850; one shared spec and one diff). My prompt carried no intended fix or hoped-for verdict. Diff checked: `git diff origin/main...HEAD` (base e3cf4cd) and the delta `79d4ebd..af54bb9` (spec author 19, merge of main). The merge delta touches only formatter specs, RH2001/RH7101 specs, one change doc of main, and the requirement ledger lines of main (FMT-BREAK-040, FMT-UNSUP-014); no get-only or RH3209/RH3210 file, no rule page.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|

None. Finding 1 of `18-spec-checker.md` is closed: `19-spec-author.md` tried adding `ExpressionBodyCodeFixUtilities` and `BlankLineCodeFixUtilities` to the Code owners header, found that `SpecStructureTests.EveryTypeIsCodeOwnerOfAtMostOneSpec` forbids a second owner spec (SHARED-BODY and SHARED-STMT own them), and instead named both helpers and the steps they perform in the introduction of `sdd/specs/shared/get-only-expression-bodies.md`. The sentence is true to the diff (`ReplaceBodyAsync` splice in both conversion directions, SHARED-GETONLY-005; `SharesLineWithSiblingMemberAcrossComment` and the shared-line separation, SHARED-GETONLY-008 / SHARED-FIX-007) and matches what `sdd/README.md` prescribes for a type that is owned elsewhere.

## Checked
1. ok: change doc stays `Accepted`, no task list, `Visible output changes` present, decisions recorded; no new output question since the last check.
2. ok: no requirement changed in the delta; SHARED-GETONLY-001 to -008 match the diff as checked in `18`.
3. ok: ledger sorted, SHARED-GETONLY-008 present, nothing reused, renumbered, or deleted (the two new ledger lines are main's FMT-BREAK-040 and FMT-UNSUP-014).
4. ok: header owners (three get-only types) exist; the `sdd/specs/README.md` row matches the header; helpers named in prose are existing owners of their specs. `SpecConsistencyTests` and `SpecStructureTests` pass.
5. ok: no rule page changed; `RH3209.md` and `RH3210.md` conform as before.
6. ok: no open output question; follow-up candidates are under `Follow-up candidates` in `00-lead.md`.
7. ok: `scripts/test.sh --project architecture`: "Passed! - Failed: 0, Passed: 198, Skipped: 0, Total: 198".
8. ok: no test added in the delta by this run; tags and tables as checked in `18`.
9. ok: `sdd/work/` holds only `get-only-expression-body` (the lead deletes it before publishing).
