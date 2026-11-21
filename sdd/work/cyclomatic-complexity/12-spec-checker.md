Role: spec checker
Commit: 9079180
Verdict: consistent

The prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| – | – | – | None | – |

## Checked
1. Change doc: ok. `sdd/changes/2026-11/method-complexity-rule.md` is Accepted, has `Visible output changes` and no task list; every maintainer decision (metric, nesting, lambdas, scoring points, answer 26, self-hosting) is under `Decisions`. A new rule and new settings need one.
2. Specs match the diff: ok. RH2009-001 to -006, SHARED-LIMIT-001/-002, and the changed RH0001-007 and SHARED-LINES-002 match the code, tests, rule page, and Package README row; format (Why, examples) followed. No reference to the SonarSource paper, "paper", or "departures" in the guide, rule page, specs, change doc, or source (answer 24).
3. IDs: ok. RH2009-001..006 follow RH2008-005, SHARED-LIMIT-001/002 sit between SHARED-JOIN and SHARED-LINES in `requirement-ids.txt`; nothing reused or removed.
4. Code owners: ok. `sdd/specs/README.md` lists RH2009 and SHARED-LIMIT rows and the RH2008 owner `MemberBodyUtilities`; RH0001 and line-count owners and Uses updated.
5. User documentation: ok. `documentation/rules/RH2009.md` has the RH2008 page structure, no formatter mention, no withheld-fix or corner-case notes, the footer, and no code fix (so no Correction to compare beyond the refactoring example); ID fits Design; the guide is linked from it. Release.ruleset carries `RH2009` with `Action="None"` (decision 4).
6. Open questions: ok. 08's open points (rule page example, recursion receiver, lambda parameter defaults) are answered by decision 5, answer 26, and decision 6; no handover holds an open output question; follow-ups are under `Follow-up candidates` in 00-lead.md.
7. Coverage: ok. `scripts/test.sh --project architecture`: Passed 198, Failed 0. Every new requirement has `[Requirement]` tests: RH2009-001..006 (12/14/40/21/15/15 tags), SHARED-LIMIT-001/002 (RH2008, RH2111, RH2009 tests), RH0001-007 (new tests).
8. Test tags: ok. All 93 RH2009 test methods carry a tag; the RH0001 additions carry RH0001-007 (the renamed existing tests keep theirs); 10's new tests carry RH2009-003 as listed in its table; the `Node.Depth` example of 09 is tagged on a new test.
9. Nothing left behind: ok. Only `sdd/work/cyclomatic-complexity/` exists under `sdd/work/`; the lead deletes it before publishing.
