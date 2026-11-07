Role: spec checker
Commit: 692a6f5
Verdict: consistent

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| none | | | | |

## Checked
1. ok. The change doc `sdd/changes/2026-11/constraint-clause-wrap.md` is Accepted, has no task list, has `Visible output changes` (entries 1-15), and records Q1-Q11 under `Decisions`. They match the answers in `00-lead.md`.
2. ok. FMT-BREAK-037, FMT-INDENT-032, FMT-SPACE-009 (changed, with its example extended), FMT-SPACE-019 (reference), RH5110-003 (changed), and the RH6021 non-goal agree with the diff and with the change doc. Each new requirement has a normative sentence, `Why:`, and examples.
3. ok. FMT-BREAK-037 and FMT-INDENT-032 are inserted at their sorted positions in `requirement-ids.txt`. No ID was reused or deleted. The dropped RH5110-004 was never issued on main.
4. ok. `GenericConstraintLineBreakRewriter` and `GenericConstraintContributor` are already owners in `line-breaks.md`, `indentation.md`, and `specs/README.md`. No owner was added, renamed, or removed. SpecConsistencyTests is part of the architecture run below.
5. ok. No rule page changed, and none is affected (change doc, "No analyzer change"). RH5110's ID range and `helpLinkUri` page are unchanged.
6. ok. Every output question from the spec author and the skeptic (Q1-Q11, S1, S2) has an answer under `Maintainer answers`. Follow-ups 1-8 are under `Follow-up candidates`. None is written into a spec.
7. ok. `scripts/test.sh --project architecture`: Passed! - Failed: 0, Passed: 115, Skipped: 0, Total: 115. Active requirements with a covering `[Requirement]` test in the diff: FMT-BREAK-037 (46), FMT-INDENT-032 (20), FMT-SPACE-009 (10), RH5110-003 (6).
8. ok. The tags added in the diff are FMT-BREAK-037, FMT-INDENT-032, FMT-SPACE-009, FMT-SPACE-013 (2), FMT-SPACE-019 (1), and RH5110-003. The test implementers' tables name each of them. Every requirement under the spec author's examples is tagged on at least one new test.
9. ok. `sdd/work/` holds only `constraint-clause-wrap`, this run's directory. The lead deletes it before publishing.
