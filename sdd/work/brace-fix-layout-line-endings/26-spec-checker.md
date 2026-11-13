Role: spec checker
Commit: 864b200
Verdict: findings

Run diff `166e3dd..HEAD`, with the focus on decision 23 (`e9ef643..HEAD`: SHARED-ATTR-022 restated as an open `{` on the list's line, tests 23, code 24). The prompt carried no intended fix, suspected cause, or hoped-for verdict. All four issues are `agent-filed`.

`scripts/test.sh --project architecture`: `Passed!  - Failed: 0, Passed: 117, Skipped: 0, Total: 117` (includes `EveryRequirementOfAnEnforcedSpecIsCovered` and `SpecConsistencyTests`).

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 2 | `sdd/specs/analyzer/RH5502.md` (-004), `RH5504.md` (-004), `RH5506.md` (-004), `RH5508.md` (-004), `RH5510.md` (-004), `RH5512.md` (-004), `RH5514.md` (-005), `RH5516.md` (-004), `RH5518.md` (-004), `RH5520.md` (-004), `RH5522.md` (-004), `RH5526.md` (-004), `RH5528.md` (-004), `RH5531.md` (-006): heading and `Why:` | Decision 23 replaced the condition "the `{` that opens the body holding the declaration" with "a `{` in front of the list on its line that is still open there" in SHARED-ATTR-022, SHARED-ATTR-017, and the change doc. The 14 per-rule requirements still carry the old condition in their heading ("on the line of the brace that opens the body holding the declaration") and in the `Why:` ("On the line of the `{` that opens the body holding the declaration, a further list would start at the indentation of the code that owns the braces..."). The normative sentence defers to SHARED-ATTR-022 and is right, but the heading and `Why:` now describe the narrower, withdrawn condition (a lambda in a statement or field initializer, a local function in a switch section, are not "the body holding the declaration"). IDs stay; only text changes. | spec author |

Not a finding: the XML summary of `SplitFixIsOfferedBehindTheBraceOfAnotherBodyOnTheLineOfTheList` (RH5514 tests, line 1178) still reads true ("a `{` ... that opens the body of another method does not withhold the fix").

## Checked, per issue

### #1416 (brace fixes keep the statement's layout)
Nothing changed for it in decision 23's round (`e9ef643..HEAD` touches no brace spec, code, or test). Earlier state unchanged from 20-spec-checker: change doc `Accepted`, SHARED-BRACE-002/003 consistent with the diff, tags present.

### #1388 (fixes keep the document's line ending)
No change in this round; RH5011-004, RH8304-004, RH7309-007, SHARED-FIX-008 as checked in 20-spec-checker.

### #1627 (trailing whitespace behind the body)
No change in this round; unchanged from 20-spec-checker.

### #1142 (attribute-list split fixes)
- SHARED-ATTR-022 restated (which `{`, in front of the list on its line, not closed in front of the list; examples on both sides, including `C3`, the lambda in a multi-line block, and the `"{"` literal); SHARED-ATTR-017 pointer updated; both match `StartsBehindAnOpenBraceOnItsLine` and its tests. The change doc's What bullet, Visible output section (before/after for `C2`, `O`, `P`, the offered lambda), Decisions entry (decision 23), and Alternatives considered match. `Accepted` kept; no rule page affected.
- IDs: none added, retired, or changed in this round; `requirement-ids.txt` unchanged.
- Tags: all 9 added test methods carry `[Requirement("SHARED-ATTR-022")]` (the four offered-side tests also SHARED-ATTR-017); 23's table names them. Every example under 22's `Examples for the tests` (`C2`, `C3`, `O`, `P`, multi-line lambda, literal) is tagged on an added test. The RH5528 sentence in the change doc is backed by the added RH5528 test.
- Finding 1 above.

## Whole run, by check
1. Change doc: ok (`Accepted`, `Visible output changes`, `Decisions` incl. 17 to 19 and 23, no task list).
2. Specs match the diff: finding 1 (stale heading and `Why:` of 14 per-rule requirements); otherwise ok.
3. IDs: ok.
4. Code owners: ok (`SpecConsistencyTests` pass; no owner added or removed).
5. User documentation: ok (no rule page in the diff; the change doc states they are unchanged).
6. Open questions routed: ok (22 lists none; follow-up candidates sit in `00-lead.md`).
7. Requirements covered: ok (architecture run above).
8. Tests carry their tag: ok.
9. Nothing left behind: ok for now; `sdd/work/` holds only this run's directory, which the lead deletes before publishing.
