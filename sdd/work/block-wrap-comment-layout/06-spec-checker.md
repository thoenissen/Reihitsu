Role: spec checker
Commit: 637ff6e
Verdict: findings

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Findings
| # | Check | Location | What does not match | Owner |
|---|---|---|---|---|
| 1 | 4 | `sdd/specs/formatter/structural-transforms.md` and `switch-case-braces.md` (Code owners), `sdd/specs/README.md` | The diff adds `BlockWrapTriviaUtilities` (`src/Reihitsu.Formatter/Pipeline/Core/Utilities/BlockWrapTriviaUtilities.cs`), the one carrier that implements FMT-PIPE-013 for both wrap sites. No spec lists it as an owner, although comparable utilities are listed (`ExpressionBodiedTransformUtilities`, `DocumentationCommentUtilities`, `RegionDirectiveUtilities`). A spec header and its `sdd/specs/README.md` row must change together. SpecConsistencyTests does not catch the omission. | spec author |

## Checked
1. Change doc: ok. A clarification of FMT-SWITCH-007 with both maintainer answers recorded in `00-lead.md`, no behavior added, changed, or retired; the spec author's recommendation to stay without a change doc fits `sdd/README.md`.
2. Specs match the diff: ok. FMT-SWITCH-007 text and example match the implemented carry and the tests; format is kept (`Why:`, example, `FMT-SPACE-001` added to `Uses`).
3. IDs: ok. None added, `requirement-ids.txt` untouched.
4. Code owners: finding 1. `scripts/test.sh --project architecture`: Passed! Failed 0, Passed 92, Total 92.
5. User documentation: ok. No rule page affected.
6. Open questions: ok. Both output questions are answered under `Maintainer answers`; the spec author lists none open.
7. Coverage: ok. No new requirement; FMT-SWITCH-007 and FMT-PIPE-013 are covered by tagged tests in the diff.
8. Tags: ok. 24 + 3 test methods carry `[Requirement]` (FMT-PIPE-013, FMT-SWITCH-007, FMT-PIPE-004, FMT-SWITCH-010), matching the Tests table; the new FMT-SWITCH-007 example is covered by `SpecificationExampleKeepsEveryCommentLayoutOfTheBracedSections`.
9. Nothing left behind: ok. Only this run's `sdd/work/block-wrap-comment-layout/` is present (the lead deletes it before publishing).
