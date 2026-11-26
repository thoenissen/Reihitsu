Role: test implementer
Commit: 8f6368d
Verdict: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Worked from `08-spec-author.md` and the FMT-BREAK-030 example in `sdd/specs/formatter/line-breaks.md`.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `LineBreakPhaseBlankLineTests.KeepsBlankLinesAboveBlockStatementAndGluedComment` (changed) | FMT-BREAK-030 | `AssertPhaseKeepsInput` | Passes: the `Child =⏎⏎/* c */ {` block is replaced by the spec's `var p = new P⏎⏎/* c */ {⏎X = 2⏎};` with the spec's columns; summary now says "glued to the brace of the creation's initializer" |

## Scenarios (bug fix)
_Not a bug fix._

## Not covered, and why
- FMT-INDENT-030: the spec author reports no test pins the dropped wording; nothing to change.
- The test passed before and after the change (the phase alone never refused the old carrier); this is a spec-alignment edit, so no red run is expected.

## Commands
```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Unit.LineBreaks.LineBreakPhaseBlankLineTests"
```
Result: 10 passed, 0 failed.

## Follow-up candidates
_None._
