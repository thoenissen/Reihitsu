Role: spec checker
Commit: 231427b
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 4 | `sdd/specs/README.md:399` | The index row for `cli/reihitsu-format.md` lacks the new code owner `BufferedConsoleOutput`, which the spec header lists (after `FormatCommandHandler`). `scripts/test.sh --project architecture` fails `SpecStructureTests.SpecIndexListsEverySpecWithItsHeader`: "cli/reihitsu-format.md: the index row differs from the spec header's ID prefix or code owners". Result line: `Failed!  - Failed: 1, Passed: 152, Skipped: 0, Total: 153`. | spec author |

## Checked
1. Change doc: ok. Required (new option, new requirements CLI-022/CLI-023), `Accepted`, no task list, `Visible output changes` present, every decision (name, default, docs, Ctrl+C, aliases, accepted values) matches an answer in `00-lead.md`; the started-write repair is recorded as not asked, with its reason.
2. Specs match the diff: ok. CLI-001, 002, 006, 017, 022, 023 match the diff and the README; format (Why, examples, normative sentence first) holds.
3. IDs: ok. CLI-022 and CLI-023 are the next free numbers, inserted in sorted position; nothing reused or removed.
4. Code owners: finding 1 (the index row). The type check itself passes; `BufferedConsoleOutput` exists in `src/Reihitsu.Cli`.
5. User documentation: ok. No rule page affected; `src/Reihitsu.Cli/README.MD` row and example match CLI-023 and the change doc.
6. Open questions: ok. All six output questions answered under `Maintainer answers`; none open, none only in the PR; follow-ups are under `Follow-up candidates`.
7. Coverage: the architecture run fails only on finding 1; no coverage test (`EveryRequirementOfAnEnforcedSpecIsCovered`) failed, so CLI-022 and CLI-023 are covered.
8. Requirement tags: ok. Every added test carries `[Requirement]` for the IDs in the test implementer's table; CLI-006, 017, 022, 023, 001, 002 are each tagged on at least one added test.
9. Nothing left behind: ok. Only this run's `sdd/work/parallel-cli-formatting/` exists; the lead deletes it before publishing. No `TODO` in the diff.
