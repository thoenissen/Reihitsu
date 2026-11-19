Role: spec checker
Commit: 468e2c31
Verdict: consistent

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Inputs: `00-lead.md`, handovers 01-11, `git diff f597ab53...HEAD`.

## thoenissen/Reihitsu#1865

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|

None.

### Checked
1. Change doc: ok. `sdd/changes/2026-11/enable-line-count-rules.md` is required (RH2008-001 and RH2111-001 change defaults) and is `Accepted`. It has a `Visible output changes` section and no task list. Each of its three visible changes (RH2008 test 120, RH2111 production 1500, RH2111 test 3000) has its answer (1A, 2A, 3C) in `00-lead.md` under `Maintainer answers`, and in the `Decisions` section.
2. Specs match the diff: ok. RH2008-001, RH2111-001, and the SHARED-LINES-002 example state 60/120 and 1500/3000. The analyzers carry `DefaultTestLimit = 120`, `DefaultProductionLimit = 1500`, `DefaultTestLimit = 3000`, and the production method default stays 60. A grep of `sdd/specs` and `documentation` found no stale 100/500/1000 default. Format kept: `Why:` and examples are present. The resource naming (3C) is deliberately no requirement: no user-visible text changes.
3. IDs: ok. No requirement was added or retired, and `requirement-ids.txt` is untouched.
4. Code owners: ok. `scripts/test.sh --project architecture`: "Passed! - Failed: 0, Passed: 198, Skipped: 0, Total: 198". Spec owners are unchanged, and `sdd/specs/README.md` still lists RH2008 and RH2111 with matching owners.
5. User documentation: ok. `RH2008.md` and `RH2111.md` carry the new defaults in text and in the configuration example, as the spec states. The edits are numbers only, and no formatter mention, withheld-fix note, or footer change was introduced. Neither rule has a code fix (no `### Correction`). The IDs sit in the RH2### Design range, and the `helpLinkUri` pages exist.
6. Open questions: ok. All three output questions have answers in `00-lead.md`. The handovers 01-10 list no open ones. The one follow-up candidate (pass the root `reihitsu.json` to the build once `binaries/` is rebuilt) is under `Follow-up candidates` and named in the change doc.
7. Requirement coverage: ok. No new active requirement. The changed ones (RH2008-001, RH2111-001) keep their `[Requirement]` tests, which assert the new limits (`DefaultTestLimitIsOneHundredTwenty`, `DefaultProductionLimitIsFifteenHundred`, `DefaultTestLimitIsThreeThousand` and the configured/invalid-setting tests), and each carries RH20xx-001 and SHARED-LINES-002.
8. Test tags: ok. The tests named in `04-test-implementer.md` that touch a requirement carry the tags of the table. The self-hosting tests are deliberately untagged (no specified surface). The 1586 tests moved in 09 keep their attributes unchanged (proven by member-text hashes in 09). The spec author's `Examples for the tests` (120/121, 1500/1501, 3000/3001, the configured-limit cases) are tagged on the changed tests.
9. Nothing left behind: ok. `sdd/work/` holds only this run's directory (`enable-line-count-rules`), which the lead deletes before publishing.

Notes, not findings: the change doc's statement that the build does not pass the root `reihitsu.json` to its analyzers matches decision 13. `AGENTS.md` now names `DiagnosticAnalyzerBase` without a type argument, as the code does, and `.claude/commands/add-resource-texts.md` matches the 3C design.

## thoenissen/Reihitsu#1866

### Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|

None.

### Checked
1. Change doc: ok. Not required, since the issue is a tooling correction of a ruleset entry with no intended-behavior change. The run's change doc covers #1865 only.
2. Specs match the diff: ok. No spec covers the ruleset. RH8402 behavior is unchanged: it still reports nothing without a `copyright` config, and the new root `reihitsu.json` has none.
3. IDs: ok. `Debug.ruleset` now reads `RH8402` (was `RH08402`), and `Release.ruleset` already read `RH8402`. No ID file change.
4. Code owners: ok. No spec header is touched.
5. User documentation: ok. No rule page is affected.
6. Open questions: ok. None.
7. Requirement coverage: ok. No spec change. The architecture run is quoted under #1865.
8. Test tags: ok. The ruleset-ID test of decision 6 verifies no requirement, so it is untagged.
9. Nothing left behind: ok.
