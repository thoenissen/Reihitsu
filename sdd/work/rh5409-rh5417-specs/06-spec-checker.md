Role: spec checker
Commit: 846ac74
Verdict: consistent

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Lane: tests (specs and tests for existing behavior). The diff against `origin/main` (78af33e) is 9 analyzer specs, 2 shared specs, the index and ledger rows, 9 test files under `src/Reihitsu.Analyzer.Test/Formatting/`, and this run's `sdd/work/` files. No production code, no rule page, and no change doc is touched.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| _None._ | | | | |

## Checked
1. Change doc: ok. None is required. The diff adds specs and tests for existing behavior, changes no intended behavior, and adds no rule, phase, or surface. The skeptic's repair round (`03-spec-author.md`) changed only wording of requirements this PR introduces.
2. Specs match the diff: ok. The diff changes no production behavior, so the specs are clarifications. Every requirement has one heading, the normative sentence first, `Why:`, and an example with `{|RH####:…|}` markup where behavior is visible. The test implementer reports that every spec example passed unchanged. Observation, not a finding: RH5415-001's "Below C# 9 it MUST NOT report a record" holds because Roslyn does not parse `record A { }` as a record declaration at C# 8 (`04-test-implementer.md`, Findings); the spec's example claim is true.
3. IDs: ok. The ledger gains 21 lines (RH5409-001, RH5410-001/002, RH5411-001/002, RH5412-001 to RH5416-001, RH5417-001 to 004, SHARED-EMPTY-001 to 004, SHARED-TRAIL-001 to 003) at sorted positions, with no line removed, moved, or reused. `SpecConsistencyTests` and the ledger-sorted check in `SpecStructureTests` pass.
4. Code owners: ok. `SpecConsistencyTests` passes. `sdd/specs/README.md` gains 11 rows (2 shared, 9 analyzer) whose owners match the headers. No `FMT-STRUCT` type is claimed as an owner, as the lead required.
5. User documentation: ok. `documentation/rules/RH5409.md` to `RH5417.md` exist, sit in the Layout category (RH5xxx), and carry the footer. None mentions the formatter. Each `### Correction` equals what the fix produces (comma removed with the comment kept; `;` declaration; Allman layout). The "Exceptions" paragraph of RH5409–RH5411 names only `#if` blocks, a simplification of SHARED-TRAIL-002 that does not contradict it. No page was changed, and none needed to be.
6. Open questions: ok. All seven questions of `03-spec-author.md` are filed, each in the issue for its spec. Q1 in thoenissen/Reihitsu#1168; Q2, Q3, Q4, Q7 in #1169 (its sections 1, 2, 3, 4); Q5, Q6 in #1170 (its sections 1, 2). Titles are `SDD open questions: <spec path>`, and each is a sub-issue of #912. The no-op-fix instance is a comment on #1150. No question is recorded only in a spec or in the PR.
7. Coverage and consistency: ok. `scripts/test.sh --project architecture --no-install`: `Passed!  - Failed:     0, Passed:    72, Skipped:     0, Total:    72`. I grepped each of the 21 new IDs: every one has at least one `[Requirement]` test (counts range from 2 for RH5417-004 to 19 for SHARED-TRAIL-003), all in `Reihitsu.Analyzer.Test`. Cleaner's analyzer run on 81b99f7: 267 passed, 0 failed.
8. Nothing left behind: ok for now. `git ls-files sdd/work` lists only `sdd/work/rh5409-rh5417-specs/`. The lead deletes it before the PR is marked ready.
