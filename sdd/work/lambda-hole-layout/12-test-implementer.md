Role: test implementer
Commit: <see git log: "Test implementer 12: pin modifier join of a parenthesized lambda">
Verdict: tests written

Repair round 1, auditor finding 1 (`11-auditor.md`, thoenissen/Reihitsu#1567). The auditor's suggested change was treated as a suggestion and re-derived: the issue's own inputs (`static⏎(int x) => x` as a declaration's value, `async`, and a conditional branch `: static⏎(int x) => x`) and the issue's "Before" shape, where the `(` stands in the statement's column, are joined by FMT-BREAK-041 (gap behind a modifier, named in FMT-BREAK-011 for a lambda's `(`). `rg` found them pinned only inside an argument list (FMT-BREAK-015), as a simple lambda (`static⏎y => y`), and together with a return type (FMT-BREAK-045). My prompt carried no intended fix or hoped-for verdict beyond the finding itself.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `ContinuationJoinTests.EachGapIsJoined` row "Modifier of a parenthesized lambda" (`static⏎            (int x) => x` as a declaration value) | FMT-BREAK-041 (method tag) | `AssertRuleResult` (LF and CRLF, second pass) | pass (characterization) |
| row "Modifier of a parenthesized lambda whose parenthesis starts in the statement's column" (the issue's "Before" layout) | FMT-BREAK-041 | `AssertRuleResult` | pass |
| row "Async modifier of a parenthesized lambda" | FMT-BREAK-041 | `AssertRuleResult` | pass |
| row "Modifier of a parenthesized lambda in a conditional branch" (`o is null ? null : static⏎(int x) => x`) | FMT-BREAK-041 | `AssertRuleResult` | pass |

File: `tests/Reihitsu.Formatter.Test/Regression/LineBreaks/ContinuationJoinTests.cs`. All four expected outputs differ from their inputs, so a lost join fails them. They pass on the current tree, as a characterization of existing behavior must.

## Scenarios (bug fix)
_Not a bug fix; characterization rows only._

## Not covered, and why
- Comment, directive, or disabled text in the modifier → `(` gap: the gap behind a modifier is an FMT-UNSUP-017 gap that the precheck refuses, and that refusal is already pinned by existing tests. This repair adds nothing new there.
- The code-fix path: FMT-BREAK-041 is unchanged by this run, and the auditor's finding is about the formatter rows only.
- Other failure classes: no production code was touched, and these rows only pin the existing join.

## Commands
scripts/test.sh --project formatter --filter "FullyQualifiedName~Reihitsu.Formatter.Test.Regression.LineBreaks.ContinuationJoinTests.EachGapIsJoined"  (24 passed)

## Follow-up candidates
_None._
