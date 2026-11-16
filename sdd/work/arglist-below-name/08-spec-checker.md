Role: spec checker
Commit: (this commit)
Verdict: findings

Inputs: `00-lead.md`, `01`-`07` handovers, `git diff origin/main`. My prompt carried no intended fix or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 4 | `sdd/specs/formatter/line-breaks.md:7` (Code owners) and `sdd/specs/README.md:14` | The diff adds the production type `ArgumentListLineStartRewriter` (`src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/ArgumentListLineStartRewriter.cs`), the one owner of FMT-BREAK-039's gap, but neither the spec header's *Code owners* nor the matching `sdd/specs/README.md` row lists it (the other rewriters of the phase, e.g. `ChainLineBreakRewriter`, are listed). `SpecConsistencyTests` passes because it only checks that listed owners exist, not that added ones are listed. | spec author |

## Checked
1. Change doc: ok. Required (FMT-BREAK-039 added with new behavior); `Accepted`; has `Visible output changes`, no task list; every visible output change (1-5, and 6 as a consequence) is covered by the recorded maintainer answers 1A-4A in `Decisions`.
2. Specs match the diff: ok. FMT-BREAK-039 matches the implementation and tests (invocation, object creation, implicit creation, constructor initializer, primary-constructor base type, `with(...)`; comment/directive/disabled-text block; FMT-BREAK-022 constructs as written, lists and conditionals as joined); FMT-BREAK-020, -015, -031 now point to it without contradiction. Format ok: normative sentence first, `Why:`, examples, no markup issues seen.
3. IDs: ok. `FMT-BREAK-039` is the next free number, sorted position, nothing reused or removed.
4. Code owners: finding 1. `scripts/test.sh --project architecture`: `Passed!  - Failed: 0, Passed: 153, Skipped: 0, Total: 153`.
5. User documentation: ok. No rule page affected (no rule reports the shape; the change doc states it); `AGENTS.md` phase-table row edit is accurate and names `ArgumentListLineStartRewriter.IsArgumentListOpenParenthesis`, which exists.
6. Open questions: ok. All output questions answered in `00-lead.md` (1A 2A 3A 4A); open items outside the run are under *Follow-up candidates* (F1-F3, F5); none written into the spec.
7. Coverage: ok. FMT-BREAK-039 is covered (architecture suite green; 51 `[Requirement("FMT-BREAK-039")]` tags in the diff).
8. Tags: ok. The four spec examples are tagged on the tests named in `05-test-implementer.md`; FMT-BREAK-031 (1 test) and FMT-PIPE-017 (3 tests) tags match its tables.
9. Nothing left behind: ok. Only `sdd/work/arglist-below-name/` is present; the lead deletes it before publishing.
