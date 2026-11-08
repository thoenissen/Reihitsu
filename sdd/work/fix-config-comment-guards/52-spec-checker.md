Role: spec checker
Commit: 9d9c536a
Verdict: consistent

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Checked repair round 3, range `051f337f..HEAD`, against decisions 46-52 and handovers 47-51.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## thoenissen/Reihitsu#1595
- No spec or test change of its own; decisions 46/47 reach RH7102 through the shared `ChangesMemberOrder` path, stated by SHARED-ORDER-007/-008. RH7102 spec unchanged and consistent.

## thoenissen/Reihitsu#1593
- Change doc `member-move-conditional-groups.md` stays Accepted; the new visible changes (defining partial declaration, implementing `get => 0; set;`, RH7109 `[Serializable]` field move now `main`'s output) are covered by decisions 46, 47 and the standing answer / batch 7 (`00-lead.md`). No task list.
- Specs match the code: SHARED-ORDER-007 counts a backing field by accessor shape only, no `partial` exception (`HasBackingField` has no `PartialKeyword`); SHARED-ORDER-008 says a conditional header makes the type count as layout-sensitive, feeding only the layout flag (`(fieldOrderSetsLayout || DeclarationHoldsConditionalDirective) && ChangesOrder(IsLayoutField)`). Examples and the partial RH7309 counterparts are present.
- No new IDs; `requirement-ids.txt` unchanged. Owners unchanged; architecture `SpecConsistencyTests` pass.
- Tests: every new test carries its requirements (`RH7309-006`/`SHARED-ORDER-007`, `RH7103-007`/`SHARED-ORDER-008`, `SHARED-ORDER-008`/`RH7109-006`) and matches the table in `50-test-implementer.md`. Every example under `Examples for the tests` is tagged on a test the diff adds or keeps.

## thoenissen/Reihitsu#1616
- RH2001-009 retitled and rewritten to the refusal (decision 48): an `#if`/`#elif`/`#else`/`#endif` in the leading trivia of a modifier other than the first, or of the type behind a modifier. Matches `HasConditionalDirectiveBetweenModifiers`; no `HasConditionalGroupOfOtherModifiers`, keyword set, or excluded-text reading remains in `src` or `sdd/specs`. SHARED-FIX-009 amendment reverted and its RH2001 example reworded consistently with RH2001-009; RH2001's `Uses` still lists SHARED-FIX-009 (cited by the Why).
- Change doc `rh2001-modifier-reorder-trivia.md` Accepted, has `Visible output changes` with `main`'s output and a Decisions entry for decision 48; no task list.
- Tests: `RH2001-009` on the renamed type-arm test, the new modifier-arm test, and the extern/readonly/unsafe tests; the position counterpart carries `RH2001-004` and `RH2001-009`.
- Rule page `documentation/rules/RH2001.md` unchanged; a withheld corner case is not page content.

## Whole run
- `scripts/test.sh --project architecture`: Passed! - Failed: 0, Passed: 115, Skipped: 0, Total: 115.
- Output questions: none open; batch 7 answered A (decision 47 as written).
- Only this run's `sdd/work/` slug is on the branch; the lead deletes it before publishing.

## Checked
1 ok. 2 ok. 3 ok. 4 ok. 5 ok. 6 ok. 7 ok. 8 ok. 9 ok.
