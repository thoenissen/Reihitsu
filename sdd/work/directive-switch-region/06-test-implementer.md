Role: test implementer
Commit: 8a76f40 (production head; this commit changes one test and adds this handover)
Verdict: tests written

Repair round for Decision 6 in `00-lead.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's recorded decision that the test is wrong against FMT-BLANK-016; the expected placement below was derived from the spec text, not from the implementer's handover or the formatter's output.

## Derivation

`PhaseLineEndingOwnershipTests.LineBreaksAroundDirectivesAndCommentsAreTheDetectedLineEndingInEveryPhase` formats, inside `M`, the statement `Enabled();` (the active `#else` branch) followed by `if (a)`. The gap in front of `if` holds `#endif`, `#pragma warning disable CS0168`, and `/* block */`.

- FMT-BLANK-001 asks for a blank line in front of the compound statement `if`.
- FMT-BLANK-006: the last trivia in front of `if`, apart from whitespace and line breaks, is the comment `/* block */`, so `if` is not directly preceded by a directive and the exemption does not apply; the blank line is inserted "after the last directive (FMT-BLANK-016)".
- FMT-BLANK-016: the blank line goes "after the last directive of the gap, in front of the comments that stand between that directive and the code behind it", and "MUST NOT insert a blank line directly in front of a directive".

So the blank line moves from in front of `#endif` to between `#pragma warning disable CS0168` and `/* block */`. No other line of `expected` is affected: the region, field, documentation, brace-wrap, and `#pragma warning restore` lines are decided by requirements this run does not touch, and the `#pragma warning restore` in front of `}` is not a gap in front of a statement or section.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `PhaseLineEndingOwnershipTests.LineBreaksAroundDirectivesAndCommentsAreTheDetectedLineEndingInEveryPhase` (`expected` only: blank line moved from above `#endif` to behind `#pragma warning disable CS0168`; tag unchanged) | FMT-PIPE-019 | `AssertEveryPhaseUsesTheDetectedLineEnding` | pass |

The test keeps its single `[Requirement("FMT-PIPE-019")]` tag: its subject is line endings; the placement it now asserts is incidental and is covered by the FMT-BLANK-016-tagged tests of `04-test-implementer.md`.

## Other tests pinning the old placement

Whole formatter project at 8a76f40 with this change: `Passed! - Failed: 0, Passed: 4733`. No other formatter test pins a blank line in front of a directive. The implementer reported the analyzer project green at aaa7241 (7352 passed), so no analyzer formatter-parity test pins it either.

## Not covered, and why
- No new test: the placement is already specified and tested by `RequiredBlankLineBehindDirectiveTests.BlankLineBeforeReturnGoesBehindPragmaInFrontOfComment` and `BlankLineBeforeIfGoesBehindLastOfTwoDirectivesWithComments` (04).
- Not re-run against the pre-fix production code: the implementer's handover shows the formatter at aaa7241's parent wrote the old placement this test pinned, so the changed `expected` differs from that output exactly by the moved blank line.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~PhaseLineEndingOwnershipTests"
scripts/test.sh --project formatter
```
